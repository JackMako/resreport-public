using ClosedXML.Excel;
using ResReportBackEnd.Models.Report;

namespace ResReportBackEnd.services.builders.csv;

public static class YoYComparisonSheetBuilder
{
    public static void Build(
        XLWorkbook workbook,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        int yearA,
        int yearB)
    {
        var worksheet = workbook.AddWorksheet("Client Report Pack YOY");

        AddHeaders(worksheet);
        AddResidences(worksheet, yearARows, yearBRows, yearA, yearB);
        ApplyStyles(worksheet);
    }

    private static void AddHeaders(IXLWorksheet worksheet)
    {
        worksheet.Cell("A1").Value = "Residence";
        worksheet.Cell("B1").Value = "Metric";
        worksheet.Cell("C1").Value = "Year";

        var months = new[]
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        for (var i = 0; i < months.Length; i++) worksheet.Cell(1, 4 + i).Value = months[i];

        worksheet.Cell(1, 16).Value = "Yearly Summary";
    }

    private static void AddResidences(
        IXLWorksheet sheet,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        int yearA,
        int yearB)
    {
        var currentRow = 2;

        var residences = yearARows
            .Concat(yearBRows)
            .Select(x => x.Residence)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        foreach (var residence in residences)
        {
            var startRow = currentRow;
            var endRow = currentRow + 7;

            sheet.Range($"A{startRow}:A{endRow}").Merge();
            sheet.Cell(startRow, 1).Value = residence;
            var residenceCell = sheet.Cell(startRow, 1);
            residenceCell.Style.Font.Bold = true;
            residenceCell.Style.Font.FontSize = 12;
            residenceCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAD3");
            residenceCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            residenceCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            sheet.Cell(startRow, 1).Style.Font.Bold = true;
            sheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            sheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            WriteMetricPair(sheet, currentRow, residence, "Total Room Nights",
                yearARows, yearBRows, yearA, yearB, MetricType.TotalRoomNights, 0);
            currentRow += 2;

            WriteMetricPair(sheet, currentRow, residence, "Revenue Spend",
                yearARows, yearBRows, yearA, yearB, MetricType.RevenueSpend, 1);
            currentRow += 2;

            WriteMetricPair(sheet, currentRow, residence, "ARR",
                yearARows, yearBRows, yearA, yearB, MetricType.Arr, 2);
            currentRow += 2;

            WriteMetricPair(sheet, currentRow, residence, "ALOS",
                yearARows, yearBRows, yearA, yearB, MetricType.Alos, 3);
            currentRow += 2;

            currentRow += 1;
        }
    }

    private static void WriteMetricPair(
        IXLWorksheet sheet,
        int row,
        string residence,
        string metric,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        int yearA,
        int yearB,
        MetricType metricType, int metricIndex)
    {
        sheet.Range(row, 2, row + 1, 2).Merge();
        sheet.Cell(row, 2).Value = metric;
        sheet.Cell(row, 2).Style.Font.Bold = true;
        sheet.Cell(row, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        sheet.Cell(row, 3).Value = yearA;
        sheet.Cell(row + 1, 3).Value = yearB;

        StyleMetricPair(sheet, row, metricIndex);

        for (var month = 1; month <= 12; month++)
        {
            var yearAMonthRows = yearARows
                .Where(x =>
                    x.Residence == residence &&
                    x.Arrive.Month == month)
                .ToList();

            var yearBMonthRows = yearBRows
                .Where(x =>
                    x.Residence == residence &&
                    x.Arrive.Month == month)
                .ToList();

            var yearACell = sheet.Cell(row, 3 + month);
            yearACell.Value = CalculateMetric(yearAMonthRows, metricType);

            var yearBCell = sheet.Cell(row + 1, 3 + month);
            yearBCell.Value = CalculateMetric(yearBMonthRows, metricType);

            ApplyMetricFormat(yearACell, metricType);
            ApplyMetricFormat(yearBCell, metricType);
        }

        WriteMetricTotals(sheet, residence, row, yearARows, yearBRows, metricType);
    }

    private static void StyleMetricPair(IXLWorksheet sheet, int row, int metricIndex)
    {
        var pairRange = sheet.Range(row, 2, row + 1, 16);

        pairRange.Style.Fill.BackgroundColor =
            metricIndex % 2 == 0
                ? XLColor.White
                : XLColor.FromHtml("#F5F7FA");

        pairRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        pairRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        var yearARow = sheet.Range(row, 3, row, 16);
        var yearBRow = sheet.Range(row + 1, 3, row + 1, 16);

        yearARow.Style.Font.FontColor = XLColor.FromHtml("#1F4E79");
        yearBRow.Style.Font.FontColor = XLColor.FromHtml("#375623");
    }

    private static void WriteMetricTotals(
        IXLWorksheet sheet,
        string residence,
        int row,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        MetricType metricType)
    {
        var yearATotalCell = sheet.Cell(row, 16);
        var yearBTotalCell = sheet.Cell(row + 1, 16);

        yearATotalCell.Value = CalculateYearlyTotalMetric(yearARows, residence, metricType);
        yearBTotalCell.Value = CalculateYearlyTotalMetric(yearBRows, residence, metricType);

        ApplyMetricFormat(yearATotalCell, metricType);
        ApplyMetricFormat(yearBTotalCell, metricType);
    }

    private static decimal CalculateMetric(
        List<CompanyActivityRow> rows,
        MetricType metricType)
    {
        var totalNights = rows.Sum(x => x.Nights);
        var totalRevenue = rows.Sum(x => x.RevenueSpend);
        var bookingCount = rows.Count;

        return metricType switch
        {
            MetricType.TotalRoomNights => totalNights,
            MetricType.RevenueSpend => totalRevenue,
            MetricType.Arr => totalNights == 0
                ? 0
                : Math.Round(totalRevenue / totalNights, 2),
            MetricType.Alos => bookingCount == 0
                ? 0
                : Math.Round((decimal)totalNights / bookingCount, 2),
            _ => 0
        };
    }

    private static decimal CalculateYearlyTotalMetric(
        List<CompanyActivityRow> rows,
        string residence,
        MetricType metricType)
    {
        var yearlyRows = rows
            .Where(x => x.Residence == residence)
            .ToList();

        return CalculateMetric(yearlyRows, metricType);
    }

    private static void StyleYearRows(IXLWorksheet sheet, int row)
    {
        var yearARange = sheet.Range(row, 2, row, 16);
        var yearBRange = sheet.Range(row + 1, 2, row + 1, 16);

        yearARange.Style.Fill.BackgroundColor = XLColor.White;
        yearBRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F6FA");

        yearARange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        yearBRange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
    }

    private static void ApplyMetricFormat(IXLCell cell, MetricType metricType)
    {
        if (metricType == MetricType.Arr ||
            metricType == MetricType.RevenueSpend)
        {
            cell.Style.NumberFormat.Format = "$#,##0.00";
            return;
        }

        if (metricType == MetricType.Alos)
        {
            cell.Style.NumberFormat.Format = "0.00";
            return;
        }

        cell.Style.NumberFormat.Format = "#,##0";
    }

    private static void ApplyStyles(IXLWorksheet worksheet)
    {
        worksheet.SheetView.FreezeRows(1);

        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            return;

        var header = worksheet.Range("A1:P1");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        worksheet.Columns().AdjustToContents();
        worksheet.Column("B").Width = 16;
    }

    private enum MetricType
    {
        TotalRoomNights,
        RevenueSpend,
        Arr,
        Alos
    }
}
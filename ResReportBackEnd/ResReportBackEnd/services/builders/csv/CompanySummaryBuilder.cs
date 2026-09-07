using ClosedXML.Excel;
using ResReportBackEnd.Models.Report;

namespace ResReportBackEnd.services.builders.csv;

public class CompanySummaryBuilder
{
    public static void Build(
        XLWorkbook workbook,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        int yearA,
        int yearB)
    {
        var worksheet = workbook.AddWorksheet("Company Summary");

        AddHeaders(worksheet, yearA.ToString(), yearB.ToString());
        AddPropertySummaries(worksheet, yearARows, yearBRows);
        ApplyStyles(worksheet);
    }

    private static void AddHeaders(IXLWorksheet worksheet, string year1, string year2)
    {
        worksheet.Cell("A1").Value = "Residence";
        worksheet.Cell("B1").Value = "Metric";
        worksheet.Cell("C1").Value = year1;
        worksheet.Cell("D1").Value = year2;
        worksheet.Cell("E1").Value = "Variance";
    }

    private static void AddPropertySummaries(
        IXLWorksheet worksheet,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows)
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
            var propertyYearA = yearARows
                .Where(x => x.Residence == residence)
                .ToList();

            var propertyYearB = yearBRows
                .Where(x => x.Residence == residence)
                .ToList();

            WriteSummaryBlock(
                worksheet,
                currentRow,
                residence,
                propertyYearA,
                propertyYearB);

            // Five metric rows plus one blank row.
            currentRow += 6;
        }

        WriteSummaryBlock(worksheet, currentRow, "TOTAL", yearARows, yearBRows);

        var totalRange = worksheet.Range(currentRow, 1, currentRow + 3, 5);
        totalRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2F0D9");
        totalRange.Style.Font.Bold = true;
    }

    private static void WriteSummaryBlock(
        IXLWorksheet worksheet,
        int startRow,
        string residence,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows)
    {
        var yearANights = yearARows.Sum(x => x.Nights);
        var yearBNights = yearBRows.Sum(x => x.Nights);

        var yearARevenue = yearARows.Sum(x => x.RevenueSpend);
        var yearBRevenue = yearBRows.Sum(x => x.RevenueSpend);

        var yearABookingCount = yearARows.Count;
        var yearBBookingCount = yearBRows.Count;

        var yearAArr = yearANights == 0
            ? 0
            : yearARevenue / yearANights;

        var yearBArr = yearBNights == 0
            ? 0
            : yearBRevenue / yearBNights;

        var yearAAlos = yearABookingCount == 0
            ? 0
            : (decimal)yearANights / yearABookingCount;

        var yearBAlos = yearBBookingCount == 0
            ? 0
            : (decimal)yearBNights / yearBBookingCount;

        // Merge the residence name across all five metric rows.
        worksheet.Range(
            startRow,
            1,
            startRow + 4,
            1).Merge();

        var residenceCell = worksheet.Cell(startRow, 1);
        residenceCell.Value = residence;
        residenceCell.Style.Font.Bold = true;
        residenceCell.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;
        residenceCell.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        if (residence != "TOTAL")
            residenceCell.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#EAF4EA");

        WriteMetricRow(
            worksheet,
            startRow,
            "Total Room Nights",
            yearANights,
            yearBNights,
            PercentageDiff(yearANights, yearBNights),
            "#,##0",
            "0.0%");

        WriteMetricRow(
            worksheet,
            startRow + 1,
            "Number of Bookings",
            yearABookingCount,
            yearBBookingCount,
            PercentageDiff(yearABookingCount, yearBBookingCount),
            "#,##0",
            "0.0%");

        WriteMetricRow(
            worksheet,
            startRow + 2,
            "Total Revenue Spend",
            yearARevenue,
            yearBRevenue,
            PercentageDiff(yearARevenue, yearBRevenue),
            "$#,##0.00",
            "0.0%");

        WriteMetricRow(
            worksheet,
            startRow + 3,
            "Average Room Rate",
            yearAArr,
            yearBArr,
            yearBArr - yearAArr,
            "$#,##0.00",
            "$#,##0.00;-$#,##0.00");

        WriteMetricRow(
            worksheet,
            startRow + 4,
            "Average Length of Stay",
            yearAAlos,
            yearBAlos,
            yearBAlos - yearAAlos,
            "0.00",
            "0.00");
    }

    private static void WriteMetricRow(
        IXLWorksheet worksheet,
        int row,
        string metric,
        decimal yearAValue,
        decimal yearBValue,
        decimal variance,
        string valueFormat,
        string varianceFormat)
    {
        worksheet.Cell(row, 2).Value = metric;

        worksheet.Cell(row, 3).Value = Math.Round(yearAValue, 2);
        worksheet.Cell(row, 3).Style.NumberFormat.Format = valueFormat;

        worksheet.Cell(row, 4).Value = Math.Round(yearBValue, 2);
        worksheet.Cell(row, 4).Style.NumberFormat.Format = valueFormat;

        worksheet.Cell(row, 5).Value = Math.Round(variance, 4);
        worksheet.Cell(row, 5).Style.NumberFormat.Format = varianceFormat;
    }

    private static decimal PercentageDiff(decimal oldValue, decimal newValue)
    {
        return oldValue == 0 ? 0 : (newValue - oldValue) / oldValue;
    }

    private static void ApplyStyles(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            return;

        var lastRow = usedRange.LastRow().RowNumber();

        worksheet.SheetView.FreezeRows(1);

        var header = worksheet.Range("A1:E1");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.Range(1, 1, lastRow, 5).Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;

        worksheet.Range(2, 1, lastRow, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Range(2, 2, lastRow, 2).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Left;

        worksheet.Range(2, 3, lastRow, 5).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Right;

        worksheet.Column("A").Width = 12;
        worksheet.Column("B").Width = 24;
        worksheet.Column("C").Width = 16;
        worksheet.Column("D").Width = 16;
        worksheet.Column("E").Width = 16;

        worksheet.Rows(1, lastRow).Height = 22;
    }
}
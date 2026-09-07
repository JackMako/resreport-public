using ClosedXML.Excel;
using ResReportBackEnd.Models.Report;

namespace ResReportBackEnd.services.builders.csv;

public class CompanyActivityBuilder
{
    public static void Build(
        XLWorkbook workbook,
        List<CompanyActivityRow> yearARows,
        List<CompanyActivityRow> yearBRows,
        int yearA,
        int yearB)
    {
        var worksheet = workbook.AddWorksheet("Company Activity");
        var sortedRows = yearARows
            .Concat(yearBRows)
            .OrderBy(x => x.Residence)
            .ThenBy(x => x.Arrive)
            .ToList();
        AddHeaders(worksheet);
        AddReservations(worksheet, sortedRows);
        ApplyStyles(worksheet);
    }

    private static void AddHeaders(IXLWorksheet worksheet)
    {
        worksheet.Cell("A1").Value = "Residence";
        worksheet.Cell("B1").Value = "Resident";
        worksheet.Cell("C1").Value = "Res";
        worksheet.Cell("D1").Value = "Arrive";
        worksheet.Cell("E1").Value = "Depart";
        worksheet.Cell("F1").Value = "Nights";
        worksheet.Cell("G1").Value = "Nightly Rate";
        worksheet.Cell("H1").Value = "Total Spend";
        worksheet.Cell("I1").Value = "Rate Type";
        worksheet.Cell("J1").Value = "Booking Source";
    }

    public static void AddReservations(IXLWorksheet worksheet, List<CompanyActivityRow> yearARows)
    {
        var cellPos = 2;

        foreach (var row in yearARows)
        {
            worksheet.Cell($"A{cellPos}").Value = row.Residence;
            worksheet.Cell($"B{cellPos}").Value = row.ResidentName;
            worksheet.Cell($"C{cellPos}").Value = row.ResNo;
            worksheet.Cell($"D{cellPos}").Value = row.Arrive;
            worksheet.Cell($"E{cellPos}").Value = row.Depart;
            worksheet.Cell($"F{cellPos}").Value = row.Nights;
            worksheet.Cell($"F{cellPos}").Value = row.BaseRateNightly;
            worksheet.Cell($"F{cellPos}").Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell($"H{cellPos}").Value = row.RevenueSpend;
            worksheet.Cell($"H{cellPos}").Style.NumberFormat.Format = "$#,##0.00";
            worksheet.Cell($"I{cellPos}").Value = row.RateType;
            worksheet.Cell($"J{cellPos}").Value = row.BookingSource;
            cellPos++;
        }
    }

    private static void ApplyStyles(IXLWorksheet worksheet)
    {
        worksheet.SheetView.FreezeRows(1);

        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
            return;

        var lastRow = usedRange.LastRow().RowNumber();

        var header = worksheet.Range("A1:J1");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        // IMPORTANT: only style the actual used rows
        worksheet.Range(2, 1, lastRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Range(2, 2, lastRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Range(2, 3, lastRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        worksheet.Range(1, 1, lastRow, 8).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        worksheet.Column("A").Width = 12;
        worksheet.Column("B").Width = 24;
        worksheet.Column("C").Width = 14;
        worksheet.Column("D").Width = 14;
        worksheet.Column("E").Width = 14;
        worksheet.Column("F").Width = 14;
        worksheet.Column("G").Width = 14;
        worksheet.Column("H").Width = 14;
        worksheet.Column("I").Width = 18;
        worksheet.Column("J").Width = 25;
    }
}
using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using ResReportBackEnd.Models.Report;
using ResReportBackEnd.services.builders.csv;

namespace ResReportBackEnd.services.reports.ClientReportPack;

public class ClientReportPack : IReportService
{
    public string ReportName => "ClientReportPack";

    public async Task<ReportResult> GetReport(Dictionary<string, object> parameters)
    {
        var yearAFile = parameters["YearAFile"] as IFormFile;
        var yearBFile = parameters["YearBFile"] as IFormFile;

        if (yearAFile == null || yearBFile == null)
            throw new ArgumentException("Both Year A and Year B CSV files are required.");

        var yearARows = await ParseCsv(yearAFile);
        var yearBRows = await ParseCsv(yearBFile);

        var yearA = yearARows.FirstOrDefault()?.Arrive.Year ?? DateTime.Now.Year;
        var yearB = yearBRows.FirstOrDefault()?.Arrive.Year ?? DateTime.Now.Year;

        using var workbook = new XLWorkbook();

        CompanySummaryBuilder.Build(workbook,
            yearARows,
            yearBRows,
            yearA,
            yearB);

        YoYComparisonSheetBuilder.Build(
            workbook,
            yearARows,
            yearBRows,
            yearA,
            yearB
        );

        CompanyActivityBuilder.Build(
            workbook,
            yearARows,
            yearBRows,
            yearA,
            yearB);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        var companyName = yearARows
                              .Select(x => x.Company)
                              .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                          ?? "Client";

        return new ReportResult
        {
            FileName = $"{SanitizeFileName(companyName)} YOY Comparison.xlsx",
            FileBytes = ms.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }

    private string SanitizeFileName(string fileName)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');

        return fileName;
    }

    private async Task<List<CompanyActivityRow>> ParseCsv(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        };

        using var csv = new CsvReader(reader, config);

        var records = new List<CompanyActivityRow>();

        await csv.ReadAsync();
        csv.ReadHeader();

        while (await csv.ReadAsync())
        {
            var given = csv.GetField("Given") ?? "";
            var surname = csv.GetField("Surname") ?? "";

            records.Add(new CompanyActivityRow
            {
                Company = csv.GetField("CompanyName") ?? "",
                ResidentName = $"{given} {surname}",
                ResNo = csv.GetField("Res_No") ?? "",
                Arrive = DateTime.ParseExact(
                    csv.GetField("Arrive_Date_Short") ?? "",
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                ),
                Depart = DateTime.ParseExact(
                    csv.GetField("Depart_Date_Short") ?? "",
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                ),
                RoomType = csv.GetField("Room Type") ?? "",
                Nights = csv.GetField<int>("Nights"),
                BaseRateNightly = csv.GetField<decimal>("BaseRateNightly"),
                RateType = csv.GetField("RateType") ?? "",
                BookingSource = csv.GetField("Travel AgentName") ?? ""
            });
        }

        return records;
    }
}
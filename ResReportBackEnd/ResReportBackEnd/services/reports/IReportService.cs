namespace ResReportBackEnd.services.reports;

public interface IReportService
{
    string ReportName { get; }

    Task<ReportResult> GetReport(Dictionary<string, object> parameters);
}
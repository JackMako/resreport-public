namespace ResReportBackEnd.services.reports;

public class ReportResult
{
    public string ContentType { get; set; } = "";
    public string FileName { get; set; } = "";
    public byte[]? FileBytes { get; set; }
    public object? Data { get; set; }
}
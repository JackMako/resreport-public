namespace ResReportBackEnd.Models.Report;

public class Report
{
    public List<ResidenceReport> Residences { get; set; } = [];
}

public class ResidenceReport
{
    public string ResidenceName { get; set; } = "";

    public YearlyMetrics YearA { get; set; } = new();
    public YearlyMetrics YearB { get; set; } = new();
}

public class YearlyMetrics
{
    public Dictionary<int, MonthlyMetrics> Months { get; set; } = new();
}

public class MonthlyMetrics
{
    public int TotalRoomNights { get; set; }
    public decimal RevenueSpend { get; set; }

    public decimal Arr =>
        TotalRoomNights == 0 ? 0 : RevenueSpend / TotalRoomNights;

    public decimal Alos { get; set; }
}
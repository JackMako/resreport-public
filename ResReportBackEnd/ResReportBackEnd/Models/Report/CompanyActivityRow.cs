namespace ResReportBackEnd.Models.Report;

public class CompanyActivityRow
{
    public string Company { get; set; } = "";

    public string ResidentName { get; set; } = "";

    public string ResNo { get; set; } = "";
    public DateTime Arrive { get; set; }

    public DateTime Depart { get; set; }
    public string RoomType { get; set; } = "";
    public int Nights { get; set; }
    public decimal BaseRateNightly { get; set; }

    public decimal RevenueSpend => Nights * BaseRateNightly;

    public string RateType { get; set; } = "";

    public string BookingSource { get; set; } = "";


    public string Residence =>
        string.IsNullOrWhiteSpace(RoomType)
            ? "Unknown"
            : RoomType.Trim()[..Math.Min(3, RoomType.Trim().Length)];
}
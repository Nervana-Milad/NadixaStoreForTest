namespace Nadixa.Core.Models.Dashboard;

public class TopProductModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
}
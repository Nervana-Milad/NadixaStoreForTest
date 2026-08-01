namespace Nadixa.Core.Models.Dashboard;

public class RecentOrderModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
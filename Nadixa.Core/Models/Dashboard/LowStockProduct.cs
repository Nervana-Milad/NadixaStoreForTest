namespace Nadixa.Core.Models.Dashboard;

public class LowStockProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
}
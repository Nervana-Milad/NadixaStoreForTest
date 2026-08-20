namespace Nadixa.Web.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Action { get; set; } = "Index";
        public string? Controller { get; set; } // null = نفس الكنترولر الحالي
        public Dictionary<string, string?> RouteValues { get; set; } = new();
    }
}

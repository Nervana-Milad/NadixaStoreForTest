namespace Nadixa.Web.Models.ViewModels
{
    public class BlogListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; } // ✅ للعرض
        public string Author { get; set; }
        public string Category { get; set; } // ✅ للعرض
        public DateTime Date { get; set; }
        public int CommentsCount { get; set; }
    }
}

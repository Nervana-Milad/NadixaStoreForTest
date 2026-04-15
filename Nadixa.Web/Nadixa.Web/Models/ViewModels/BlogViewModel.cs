using System.Globalization;

namespace Nadixa.Web.Models.ViewModels
{
    public class BlogViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string ImageUrl { get; set; }
        public String Author { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
    }
}

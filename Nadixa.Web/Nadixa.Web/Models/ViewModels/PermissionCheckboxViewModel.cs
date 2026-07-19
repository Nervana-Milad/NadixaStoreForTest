namespace Nadixa.Web.Models.ViewModels
{
    public class PermissionCheckboxViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsChecked { get; set; }
    }
}

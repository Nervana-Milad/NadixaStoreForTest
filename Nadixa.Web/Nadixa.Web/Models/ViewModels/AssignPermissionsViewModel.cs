namespace Nadixa.Web.Models.ViewModels
{
    public class AssignPermissionsViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<PermissionCheckboxViewModel> Permissions { get; set; } = new();
    }
}

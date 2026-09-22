namespace Neurix.Models
{
    public class CrmEmptyStateViewModel
    {
        public string Icon { get; set; } = "inbox";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
    }
}

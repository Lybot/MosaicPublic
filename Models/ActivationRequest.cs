namespace MozaikaApp.Models
{
    public class ActivationRequest
    {
        public string HashData { get; set; }
        public string Key { get; set; }
        public string Email { get; set; }
        public bool Activate { get; set; }
        public string AppName { get; set; } = "Mozaika";
    }
}

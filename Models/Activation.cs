namespace MozaikaApp.Models
{
    public static  class Activation
    {
        public static bool IsTrial { get; set; } = true;
        public static bool WasStart { get; set; } = false;
        public static bool IsEditable { get; set; } = true;
    }
}

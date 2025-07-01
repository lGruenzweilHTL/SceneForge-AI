using UnityEditor.PackageManager;

public class GroqErrorResponse
{
    public Error error { get; set; }

    public class Error
    {
        public string message { get; set; }
        public string type { get; set; }
    }
}
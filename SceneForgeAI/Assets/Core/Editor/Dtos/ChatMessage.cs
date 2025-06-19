using JetBrains.Annotations;

public class ChatMessage
{
    public string role { get; set; }
    public string content { get; set; }
    [CanBeNull] public string json { get; set; }
}
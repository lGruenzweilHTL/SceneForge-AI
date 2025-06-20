using System;
using JetBrains.Annotations;

public class ChatMessage
{
    public string Role;
    public string Content;
    [CanBeNull] public string Json = null;
    public SceneDiff[] Diffs = Array.Empty<SceneDiff>();
    public bool Display = true;
}
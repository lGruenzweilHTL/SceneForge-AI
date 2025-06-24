using System;
using JetBrains.Annotations;

public class ChatMessage
{
    public string Role;
    public string Content;
    [CanBeNull] public string Json = null;
    public SceneDiff[] Diffs = Array.Empty<SceneDiff>();
    public bool Display = true;
    public string ToolCallId = null; // Used to track tool calls
    public string Name = null; // Used for tool calls to identify the tool
}
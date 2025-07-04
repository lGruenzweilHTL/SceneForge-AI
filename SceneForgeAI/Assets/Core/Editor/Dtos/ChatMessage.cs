using System;
using JetBrains.Annotations;
using UnityEngine;

public class ChatMessage
{
    public string Role;
    public string Content;
    public string Reasoning;
    public SceneDiff[] Diffs = Array.Empty<SceneDiff>();
    public bool Display = true;
    public string ToolCallId = null; // Used to track tool calls
    public string Name = null; // Used for tool calls to identify the tool
    public string[] Images = null; // Used for image messages
    public Texture[] CachedTextures = null; // Cached textures for images, used to avoid re-decoding
}
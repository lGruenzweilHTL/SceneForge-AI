using UnityEditor;
using UnityEngine;

public static class HeaderStyles
{
    public static readonly GUIStyle HeaderStyle = new(EditorStyles.label)
    {
        fontSize = 20,
        alignment = TextAnchor.MiddleCenter,
        fontStyle = FontStyle.Bold
    };


    public static readonly GUIStyle SubheaderStyle = new(EditorStyles.label)
    {
        fontSize = 16,
        alignment = TextAnchor.MiddleLeft,
        fontStyle = FontStyle.Bold,
    };
}
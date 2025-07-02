using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ImageUtility
{
    public static string SelectAndEncodeImage()
    {
        string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (string.IsNullOrEmpty(path))
            return null;
        
        return GetDataHeader(path) + EncodeImageToBase64(path);
    }

    public static string GetDataHeader(string imagePath)
    {
        // GetExtension returns the file extension with a leading dot, so we skip the first character.
        return $"data:image/{Path.GetExtension(imagePath)[1..]};base64,";
    }
    
    public static string EncodeImageToBase64(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return null;

        byte[] imageBytes = File.ReadAllBytes(imagePath);
        return Convert.ToBase64String(imageBytes);
    }
    
    public static Texture DecodeBase64Image(string base64String, bool resizeToPreview)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        var headerEndIndex = base64String.IndexOf("base64,", StringComparison.Ordinal) + "base64,".Length;
        byte[] imageBytes = Convert.FromBase64String(base64String[headerEndIndex..]);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // Resize to a preview size (max 100x100)
        const int maxSize = 100;
        if ((texture.width > maxSize || texture.height > maxSize) && resizeToPreview)
        {
            var scale = Mathf.Min(maxSize / (float)texture.width, maxSize / (float)texture.height);
            var newWidth = Mathf.RoundToInt(texture.width * scale);
            var newHeight = Mathf.RoundToInt(texture.height * scale);

            var rt = RenderTexture.GetTemporary(newWidth, newHeight);
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);

            Texture2D resizedTexture = new Texture2D(newWidth, newHeight, texture.format, false);
            resizedTexture.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resizedTexture.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            texture = resizedTexture;
        }

        texture.Apply();
        return texture;
    }
}
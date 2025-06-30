using System.IO;
using UnityEditor;

public static class ImageUtility
{
    public static string SelectAndEncodeImage()
    {
        string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (path == null || path.Length == 0)
            return null;
        
        var dataHeader = $"data:image/{Path.GetExtension(path)[1..]};base64,"; // GetExtension returns the file extension with a leading dot, so we skip the first character.
        return dataHeader + EncodeImageToBase64(path);
    }
    
    public static string EncodeImageToBase64(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return null;

        byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
        return System.Convert.ToBase64String(imageBytes);
    }
}
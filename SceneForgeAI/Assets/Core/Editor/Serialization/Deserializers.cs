using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Plastic.Newtonsoft.Json;

public static class Deserializers
{
    public static object Property(Type type, object value)
    {
        if (value == null) return null;
        if (type.IsPrimitive || type == typeof(string)) return Convert.ChangeType(value, type);
        if (type.IsEnum) return Enum.Parse(type, value.ToString());
        if (type == typeof(Vector2)) return DeserializeVector2(value);
        if (type == typeof(Vector3)) return DeserializeVector3(value);
        if (type == typeof(Vector4)) return DeserializeVector4(value);
        if (type == typeof(Vector2Int)) return DeserializeVector2Int(value);
        if (type == typeof(Vector3Int)) return DeserializeVector3Int(value);
        if (type == typeof(Bounds)) return DeserializeBounds(value);
        if (type == typeof(BoundsInt)) return DeserializeBoundsInt(value);
        if (type == typeof(Transform)) return null;
        if (type == typeof(Quaternion)) return DeserializeQuaternion(value);
        if (type == typeof(Color)) return DeserializeColor(value);
        if (type == typeof(Color32)) return DeserializeColor32(value);
        if (type == typeof(Rect)) return DeserializeRect(value);
        if (type == typeof(RectInt)) return DeserializeRectInt(value);
        if (type == typeof(Matrix4x4)) return DeserializeMatrix4x4(value);
        if (type == typeof(LayerMask)) return new LayerMask { value = Convert.ToInt32(value) };
        if (type == typeof(AnimationCurve)) return DeserializeAnimationCurve(value);
        if (type == typeof(Gradient)) return DeserializeGradient(value);
        if (type == typeof(Sprite)) return null;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Property(type.GetGenericArguments()[0], value);
        }
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var arr = value as IEnumerable ?? JsonConvert.DeserializeObject(value.ToString(), typeof(object[])) as IEnumerable;
            if (arr == null) return null;
            var list = new List<object>();
            foreach (var item in arr)
                list.Add(Property(elementType, item));
            var result = Array.CreateInstance(elementType, list.Count);
            for (int i = 0; i < list.Count; i++)
                result.SetValue(list[i], i);
            return result;
        }
        return null;
    }

    public static Vector2 DeserializeVector2(object value)
    {
        var arr = ToFloatArray(value, 2);
        return new Vector2(arr[0], arr[1]);
    }
    public static Vector3 DeserializeVector3(object value)
    {
        var arr = ToFloatArray(value, 3);
        return new Vector3(arr[0], arr[1], arr[2]);
    }
    public static Vector4 DeserializeVector4(object value)
    {
        var arr = ToFloatArray(value, 4);
        return new Vector4(arr[0], arr[1], arr[2], arr[3]);
    }
    public static Vector2Int DeserializeVector2Int(object value)
    {
        var arr = ToIntArray(value, 2);
        return new Vector2Int(arr[0], arr[1]);
    }
    public static Vector3Int DeserializeVector3Int(object value)
    {
        var arr = ToIntArray(value, 3);
        return new Vector3Int(arr[0], arr[1], arr[2]);
    }
    public static Bounds DeserializeBounds(object value)
    {
        var dict = value as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
        var center = DeserializeVector3(dict["center"]);
        var size = DeserializeVector3(dict["size"]);
        return new Bounds(center, size);
    }
    public static BoundsInt DeserializeBoundsInt(object value)
    {
        var dict = value as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
        var pos = DeserializeVector3Int(dict["position"]);
        var size = DeserializeVector3Int(dict["size"]);
        return new BoundsInt(pos, size);
    }
    public static Quaternion DeserializeQuaternion(object value)
    {
        var euler = DeserializeVector3(value);
        return Quaternion.Euler(euler);
    }
    public static Color DeserializeColor(object value)
    {
        var arr = ToFloatArray(value, 3, 4);
        if (arr.Length == 4)
            return new Color(arr[0], arr[1], arr[2], arr[3]);
        return new Color(arr[0], arr[1], arr[2]);
    }
    public static Color32 DeserializeColor32(object value)
    {
        var arr = ToIntArray(value, 4);
        return new Color32((byte)arr[0], (byte)arr[1], (byte)arr[2], (byte)arr[3]);
    }
    public static Rect DeserializeRect(object value)
    {
        var dict = value as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
        return new Rect(
            Convert.ToSingle(dict["x"]),
            Convert.ToSingle(dict["y"]),
            Convert.ToSingle(dict["width"]),
            Convert.ToSingle(dict["height"])
        );
    }
    public static RectInt DeserializeRectInt(object value)
    {
        var dict = value as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
        return new RectInt(
            Convert.ToInt32(dict["x"]),
            Convert.ToInt32(dict["y"]),
            Convert.ToInt32(dict["width"]),
            Convert.ToInt32(dict["height"])
        );
    }
    public static Matrix4x4 DeserializeMatrix4x4(object value)
    {
        var arr = value as IEnumerable ?? JsonConvert.DeserializeObject(value.ToString(), typeof(object[])) as IEnumerable;
        var rows = arr.Cast<object>().Select(DeserializeVector4).ToArray();
        var m = new Matrix4x4();
        for (int i = 0; i < 4; i++)
            m.SetRow(i, rows[i]);
        return m;
    }
    public static AnimationCurve DeserializeAnimationCurve(object value)
    {
        var keys = value as IEnumerable ?? JsonConvert.DeserializeObject(value.ToString(), typeof(object[])) as IEnumerable;
        var keyframes = new List<Keyframe>();
        foreach (var k in keys)
        {
            var dict = k as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(k.ToString());
            var key = new Keyframe(
                Convert.ToSingle(dict["time"]),
                Convert.ToSingle(dict["value"]),
                Convert.ToSingle(dict["inTangent"]),
                Convert.ToSingle(dict["outTangent"])
            );
            if (dict.Contains("weightedMode"))
                key.weightedMode = (WeightedMode)Enum.Parse(typeof(WeightedMode), dict["weightedMode"].ToString());
            keyframes.Add(key);
        }
        return new AnimationCurve(keyframes.ToArray());
    }
    public static Gradient DeserializeGradient(object value)
    {
        var dict = value as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(value.ToString());
        var colorKeys = ((IEnumerable)dict["colorKeys"]).Cast<object>().Select(k =>
        {
            var kd = k as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(k.ToString());
            return new GradientColorKey(
                DeserializeColor(kd["color"]),
                Convert.ToSingle(kd["time"])
            );
        }).ToArray();
        var alphaKeys = ((IEnumerable)dict["alphaKeys"]).Cast<object>().Select(k =>
        {
            var kd = k as IDictionary ?? JsonConvert.DeserializeObject<Dictionary<string, object>>(k.ToString());
            return new GradientAlphaKey(
                Convert.ToSingle(kd["alpha"]),
                Convert.ToSingle(kd["time"])
            );
        }).ToArray();
        var grad = new Gradient();
        grad.SetKeys(colorKeys, alphaKeys);
        return grad;
    }
    
    private static float[] ToFloatArray(object value, int minLen, int maxLen = -1)
    {
        var arr = value as IEnumerable ?? JsonConvert.DeserializeObject(value.ToString(), typeof(object[])) as IEnumerable;
        var floats = arr.Cast<object>().Select(Convert.ToSingle).ToArray();
        if (floats.Length < minLen || (maxLen > 0 && floats.Length > maxLen))
            throw new ArgumentException("Array has invalid length", nameof(value));
        return floats;
    }
    private static int[] ToIntArray(object value, int minLen, int maxLen = -1)
    {
        var arr = value as IEnumerable ?? JsonConvert.DeserializeObject(value.ToString(), typeof(object[])) as IEnumerable;
        var ints = arr.Cast<object>().Select(Convert.ToInt32).ToArray();
        if (ints.Length < minLen || (maxLen > 0 && ints.Length > maxLen))
            throw new ArgumentException("Array has invalid length", nameof(value));
        return ints;
    }
}
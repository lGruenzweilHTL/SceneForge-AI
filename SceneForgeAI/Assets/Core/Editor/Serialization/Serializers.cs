using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Serializers
{
    // TODO: materials, meshes, textures, audio clips, etc.
    public static object Property(Type type, object value)
    {
        if (value == null) return null;
        
        if (type.IsPrimitive || type == typeof(string)) return value;
        if (type.IsEnum) return value.ToString();
        if (type == typeof(Vector2)) return Vector2((Vector2)value);
        if (type == typeof(Vector3)) return Vector3((Vector3)value);
        if (type == typeof(Vector4)) return Vector4((Vector4)value);
        if (type == typeof(Vector2Int)) return Vector2Int((Vector2Int)value);
        if (type == typeof(Vector3Int)) return Vector3Int((Vector3Int)value);
        if (type == typeof(Bounds)) return Bounds((Bounds)value);
        if (type == typeof(BoundsInt)) return BoundsInt((BoundsInt)value);
        if (type == typeof(Quaternion)) return Quaternion((Quaternion)value);
        if (type == typeof(Color)) return Color((Color)value);
        if (type == typeof(Color32)) return Color32((Color32)value);
        if (type == typeof(Rect)) return Rect((Rect)value);
        if (type == typeof(RectInt)) return RectInt((RectInt)value);
        if (type == typeof(Matrix4x4)) return Matrix4X4((Matrix4x4)value);
        if (type == typeof(LayerMask)) return LayerMask((LayerMask)value);
        if (type == typeof(AnimationCurve)) return AnimationCurve((AnimationCurve)value);
        if (type == typeof(Gradient)) return Gradient((Gradient)value);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Property(type.GetGenericArguments()[0], value);
        }
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType == null) return null;
            var array = (Array)value;
            var result = new object[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = Property(elementType, array.GetValue(i));
            }
            return result;
        }
        
        // The unity objects that need special treatment
        if (!IsUnityObjectAlive(value)) return null; // manually perform object lifetime checks
        if (type == typeof(Sprite)) return Sprite((Sprite)value);
        if (type == typeof(Transform)) return Transform((Transform)value);
        
        return null;
    }

    private static bool IsUnityObjectAlive(object o)
    {
        if (o is Object unityObject) return (bool)unityObject;
        return false;
    }
    
    public static object Vector2(Vector2 vec) => new[]
    {
        vec.x, vec.y
    };
    public static object Vector3(Vector3 vec) => new[]
    {
        vec.x, vec.y, vec.z
    };
    public static object Vector4(Vector4 vec) => new[]
    {
        vec.x, vec.y, vec.z, vec.w
    };
    public static object Vector2Int(Vector2Int vec) => new[]
    {
        vec.x, vec.y
    };
    public static object Vector3Int(Vector3Int vec) => new[]
    {
        vec.x, vec.y, vec.z
    };
    public static object Bounds(Bounds bounds) => new
    {
        center = Vector3(bounds.center),
        size = Vector3(bounds.size)
    };
    public static object BoundsInt(BoundsInt bounds) => new
    {
        position = Vector3Int(bounds.position),
        size = Vector3Int(bounds.size)
    };
    public static object Transform(Transform transform) => new
    {
        position = Vector3(transform.position),
        rotation = Vector3(transform.rotation.eulerAngles),
        localScale = Vector3(transform.localScale)
    };

    public static object Quaternion(Quaternion quaternion) => Vector3(quaternion.eulerAngles);
    
    public static object Color(Color color) => new[]
    {
        color.r, color.g, color.b, color.a
    };
    public static object Color32(Color32 color) => new[]
    {
        color.r, color.g, color.b, color.a
    };
    public static object Rect(Rect rect) => new
    {
        rect.x,
        rect.y,
        rect.width,
        rect.height
    };
    public static object RectInt(RectInt rect) => new
    {
        rect.x,
        rect.y,
        rect.width,
        rect.height
    };
    public static object Matrix4X4(Matrix4x4 matrix) => new[]
    {
        Vector4(matrix.GetRow(0)),
        Vector4(matrix.GetRow(1)),
        Vector4(matrix.GetRow(2)),
        Vector4(matrix.GetRow(3))
    };
    public static object LayerMask(LayerMask layerMask) => layerMask.value;

    public static object AnimationCurve(AnimationCurve curve) => curve.keys
        .Select(k => new
        {
            k.time,
            k.value,
            k.inTangent,
            k.outTangent,
            weightedMode = k.weightedMode.ToString()
        });
    public static object Gradient(Gradient gradient) => new
    {
        colorKeys = gradient.colorKeys.Select(k => new
        {
            k.time,
            color = Color(k.color)
        }).ToArray(),
        alphaKeys = gradient.alphaKeys.Select(k => new
        {
            k.time,
            k.alpha
        }).ToArray()
    };
    public static object Sprite(Sprite sprite) => new
    {
        name = sprite.name,
        texture = sprite.texture.name,
        rect = Rect(sprite.rect)
    };
}
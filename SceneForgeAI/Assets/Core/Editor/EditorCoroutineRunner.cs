using System.Collections;
using UnityEditor;

public static class EditorCoroutineRunner
{
    public static void StartCoroutine(IEnumerator routine)
    {
        void Update()
        {
            if (!routine.MoveNext())
                EditorApplication.update -= Update;
        }
        EditorApplication.update += Update;
    }
}
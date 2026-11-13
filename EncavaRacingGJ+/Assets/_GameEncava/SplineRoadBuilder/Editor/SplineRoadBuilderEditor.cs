using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector personalizado para <see cref="SplineRoadBuilder"/>.
/// Añade botones grandes para hornear y limpiar la malla.
/// </summary>
[CustomEditor(typeof(SplineRoadBuilder))]
public class SplineRoadBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var builder = (SplineRoadBuilder)target;

        EditorGUILayout.Space(8f);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Bake Road", GUILayout.Height(32f)))
            {
                Undo.RegisterCompleteObjectUndo(builder.gameObject, "Bake Road");
                builder.Bake();
                EditorUtility.SetDirty(builder);
            }

            if (GUILayout.Button("Clear", GUILayout.Height(32f), GUILayout.Width(80f)))
            {
                Undo.RegisterCompleteObjectUndo(builder.gameObject, "Clear Road");
                builder.Clear();
                EditorUtility.SetDirty(builder);
            }
        }

        EditorGUILayout.HelpBox(
            "Bake Road genera asfalto, bordillos, puente y barandillas desde el Spline.\n" +
            "Si editas el Spline, vuelve a hornear.\n\n" +
            "Materiales: 0 = asfalto, 1 = estructura/concreto, 2 = metal (barandillas).",
            MessageType.Info);
    }
}

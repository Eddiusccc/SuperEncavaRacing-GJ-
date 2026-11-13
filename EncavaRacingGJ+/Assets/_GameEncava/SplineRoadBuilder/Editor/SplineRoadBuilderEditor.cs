using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector personalizado para <see cref="SplineRoadBuilder"/>.
/// Añade un botón grande "Bake Road" para que no haya que buscar el ContextMenu.
/// </summary>
[CustomEditor(typeof(SplineRoadBuilder))]
public class SplineRoadBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var builder = (SplineRoadBuilder)target;

        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Bake Road", GUILayout.Height(32f)))
        {
            Undo.RegisterCompleteObjectUndo(builder.gameObject, "Bake Road");
            builder.Bake();
            EditorUtility.SetDirty(builder);
        }

        EditorGUILayout.HelpBox(
            "Genera la malla de asfalto y bordillos a partir del Spline asignado.\n" +
            "Si editas el Spline, vuelve a pulsar Bake Road para regenerarla.",
            MessageType.Info);
    }
}

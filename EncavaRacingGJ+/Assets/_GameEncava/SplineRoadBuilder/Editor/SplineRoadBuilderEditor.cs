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
            "Materiales: 0 = asfalto, 1 = estructura/concreto, 2 = metal (barandillas), 3 = pilares.\n" +
            "Barandillas: Railing At Curb Edge / Railing Distance From Asphalt controlan la separación lateral.",
            MessageType.Info);

        DrawRailingRange(builder);
    }

    /// <summary>
    /// Muestra un slider 0-1 para limitar la barandilla a un tramo del spline.
    /// Es más cómodo que escribir distancias en metros a mano.
    /// </summary>
    void DrawRailingRange(SplineRoadBuilder builder)
    {
        float length = builder.GetLength();
        if (length <= 0f) return;

        serializedObject.Update();

        var railingsProp = serializedObject.FindProperty("generateRailings");
        if (railingsProp == null || !railingsProp.boolValue) return;

        var startProp = serializedObject.FindProperty("railingStartDistance");
        var endProp = serializedObject.FindProperty("railingEndDistance");
        if (startProp == null || endProp == null) return;

        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Rango de barandilla", EditorStyles.boldLabel);

        float start01 = Mathf.Clamp01(startProp.floatValue / length);
        float end01 = endProp.floatValue > startProp.floatValue
            ? Mathf.Clamp01(endProp.floatValue / length)
            : 1f;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.MinMaxSlider("Inicio / Fin", ref start01, ref end01, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(builder, "Railing Range");
            startProp.floatValue = start01 * length;
            endProp.floatValue = end01 * length;
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.LabelField(
            $"Desde {start01 * length:F1} m hasta {end01 * length:F1} m  (longitud total {length:F1} m)",
            EditorStyles.miniLabel);

        EditorGUILayout.HelpBox(
            "Mueve los dos tiradores para que la barandilla cubra solo el tramo del puente.\n" +
            "0 = inicio del spline, 1 = final. Si quieres la barandilla en todo el spline, deja el rango completo.",
            MessageType.None);
    }
}

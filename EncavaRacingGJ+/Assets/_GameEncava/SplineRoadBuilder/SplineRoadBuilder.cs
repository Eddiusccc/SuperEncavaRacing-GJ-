using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Genera una malla de carretera (asfalto + bordillos/acera) siguiendo un Spline de Unity.
/// </summary>
/// <remarks>
/// Úsalo para trazar la red vial de Valencia, Carabobo (Av. Bolívar, vías secundarias,
/// glorietas, accesos al cerro, etc.) y tener una carretera sobre la que colocar paradas
/// de bus, semáforos y tráfico.
///
/// Requisitos: paquete com.unity.splines 2.9.x (probado con la API de 2.9.2 en Unity 6.3).
/// La malla se genera en espacio de mundo, así que el GameObject puede estar en cualquier
/// posición; el resultado coincide con el SplineContainer.
/// </remarks>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRoadBuilder : MonoBehaviour
{
    [Header("Referencias Spline")]
    [Tooltip("SplineContainer que contiene la ruta usada para generar la carretera.")]
    [SerializeField] SplineContainer splineContainer;
    [Tooltip("Índice del spline dentro del SplineContainer que se utilizará.")]
    [SerializeField] int splineIndex = 0;

    [Header("Carretera")]
    [Tooltip("Ancho total del asfalto en metros.")]
    [SerializeField] float roadWidth = 7f;           // Ancho total del asfalto (m)
    [Tooltip("Ancho del bordillo o acera en cada lado, en metros.")]
    [SerializeField] float curbWidth = 1f;           // Ancho del borde/acera cada lado (m)
    [Tooltip("Altura del bordillo respecto al asfalto, en metros.")]
    [SerializeField] float curbHeight = 0.15f;       // Alto del bordillo (m)
    [Tooltip("Distancia entre muestras del spline en metros. Valores menores suavizan la carretera, pero generan más polígonos.")]
    [SerializeField] float sampleStep = 2f;          // Distancia entre muestras (m). Menos = más suave, más polígonos.
    [Tooltip("Intensidad de la inclinación en curvas. 0 deja la carretera plana.")]
    [SerializeField] float bankFactor = 0.25f;       // Inclinación (bank) en curvas. 0 = plano.
    [Tooltip("Material que se asignará al MeshRenderer de la carretera.")]
    [SerializeField] Material roadMaterial;

    [Header("Colisiones")]
    [Tooltip("Genera o actualiza un MeshCollider usando la malla de carretera horneada.")]
    [SerializeField] bool generateCollider = true;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh roadMesh;

    void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && roadMaterial != null)
            meshRenderer.sharedMaterial = roadMaterial;
    }

    void OnValidate()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && roadMaterial != null)
            meshRenderer.sharedMaterial = roadMaterial;
    }

    /// <summary>
    /// Limpia y genera la malla de carretera desde el spline asignado.
    /// Llamar desde el Inspector (ContextMenu > Bake Road) o por código.
    /// </summary>
    [ContextMenu("Bake Road")]
    public void Bake()
    {
        if (splineContainer == null)
        {
            Debug.LogWarning("SplineRoadBuilder: Asigna un SplineContainer.", this);
            return;
        }

        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count)
        {
            Debug.LogWarning("SplineRoadBuilder: splineIndex fuera de rango.", this);
            return;
        }

        // La malla se genera en espacio de mundo, así que el objeto va en identidad.
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;

        // Protecciones para valores del Inspector.
        sampleStep = Mathf.Max(0.25f, sampleStep);
        roadWidth = Mathf.Max(0.5f, roadWidth);
        curbWidth = Mathf.Max(0f, curbWidth);
        curbHeight = Mathf.Max(0f, curbHeight);

        // Longitud en espacio de mundo (respeta la escala/rotación del contenedor).
        float length = splineContainer.CalculateLength(splineIndex);
        if (length <= 0f) return;

        int sampleCount = Mathf.CeilToInt(length / sampleStep) + 1;
        sampleCount = Mathf.Clamp(sampleCount, 2, 4096);

        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        float halfWidth = roadWidth * 0.5f;
        float curbOuter = halfWidth + curbWidth;

        Vector3 prevTangent = Vector3.forward;

        for (int i = 0; i < sampleCount; i++)
        {
            float distance = Mathf.Min(i * sampleStep, length);
            float stepDist = (i == 0) ? sampleStep : distance - Mathf.Min((i - 1) * sampleStep, length);
            if (stepDist <= 0f) stepDist = sampleStep;

            // t normalizado (0..1). La API de Splines 2.9.x evalúa por ratio, no por distancia.
            float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;

            // El SplineContainer devuelve posición, tangente y up ya en espacio de mundo.
            Vector3 pos = splineContainer.EvaluatePosition(splineIndex, t);
            Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(splineIndex, t)).normalized;
            Vector3 up = ((Vector3)splineContainer.EvaluateUpVector(splineIndex, t)).normalized;

            if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;
            if (up.sqrMagnitude < 0.0001f) up = Vector3.up;

            // Banking: inclina el vector arriba según la curvatura aproximada.
            float curvature = (i == 0) ? 0f : Vector3.Angle(prevTangent, tangent) / stepDist;
            float bank = curvature * bankFactor;
            Quaternion roll = Quaternion.AngleAxis(bank, tangent);
            Vector3 rolledUp = (roll * up).normalized;

            Vector3 right = Vector3.Cross(tangent, rolledUp).normalized;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

            // Bordes de la calzada (nivel del asfalto).
            Vector3 left = pos - right * halfWidth;
            Vector3 rightEdge = pos + right * halfWidth;

            // Bordillos exteriores (más abajo).
            Vector3 leftCurb = pos - right * curbOuter - rolledUp * curbHeight;
            Vector3 rightCurb = pos + right * curbOuter - rolledUp * curbHeight;

            // Bordillos inferiores (cara lateral).
            Vector3 leftCurbBottom = leftCurb - rolledUp * curbHeight;
            Vector3 rightCurbBottom = rightCurb - rolledUp * curbHeight;

            int baseIndex = vertices.Count;
            vertices.Add(left);
            vertices.Add(rightEdge);
            vertices.Add(leftCurb);
            vertices.Add(rightCurb);
            vertices.Add(leftCurbBottom);
            vertices.Add(rightCurbBottom);

            // Normales y tangentes.
            normals.Add(rolledUp);
            normals.Add(rolledUp);
            normals.Add(rolledUp);
            normals.Add(rolledUp);
            normals.Add(-rolledUp);
            normals.Add(-rolledUp);

            Vector4 tangent4 = new Vector4(tangent.x, tangent.y, tangent.z, 1f);
            tangents.Add(tangent4);
            tangents.Add(tangent4);
            tangents.Add(tangent4);
            tangents.Add(tangent4);
            tangents.Add(tangent4);
            tangents.Add(tangent4);

            // UV: x = progreso a lo largo de la vía, y = lado (0 interior / 1 exterior).
            float u = t;
            uvs.Add(new Vector2(u, 0f));
            uvs.Add(new Vector2(u, 1f));
            uvs.Add(new Vector2(u, 0f));
            uvs.Add(new Vector2(u, 1f));
            uvs.Add(new Vector2(u, 0f));
            uvs.Add(new Vector2(u, 1f));

            // Triángulos que conectan la muestra i con la i+1.
            if (i < sampleCount - 1)
            {
                int next = baseIndex + 6;

                // Calzada.
                indices.Add(baseIndex + 0); indices.Add(baseIndex + 1); indices.Add(next + 1);
                indices.Add(baseIndex + 0); indices.Add(next + 1); indices.Add(next + 0);

                // Bordillo izquierdo (superficie inclinada hacia fuera).
                indices.Add(baseIndex + 0); indices.Add(baseIndex + 2); indices.Add(next + 2);
                indices.Add(baseIndex + 0); indices.Add(next + 2); indices.Add(next + 0);

                // Cara lateral bordillo izquierdo.
                indices.Add(baseIndex + 2); indices.Add(baseIndex + 4); indices.Add(next + 4);
                indices.Add(baseIndex + 2); indices.Add(next + 4); indices.Add(next + 2);

                // Bordillo derecho (superficie).
                indices.Add(baseIndex + 1); indices.Add(baseIndex + 3); indices.Add(next + 3);
                indices.Add(baseIndex + 1); indices.Add(next + 3); indices.Add(next + 1);

                // Cara lateral bordillo derecho.
                indices.Add(baseIndex + 3); indices.Add(baseIndex + 5); indices.Add(next + 5);
                indices.Add(baseIndex + 3); indices.Add(next + 5); indices.Add(next + 3);
            }

            prevTangent = tangent;
        }

        if (roadMesh == null) roadMesh = new Mesh();
        roadMesh.Clear();
        roadMesh.SetVertices(vertices);
        roadMesh.SetNormals(normals);
        roadMesh.SetTangents(tangents);
        roadMesh.SetUVs(0, uvs);
        roadMesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        roadMesh.RecalculateBounds();

        if (meshFilter != null) meshFilter.sharedMesh = roadMesh;

        if (generateCollider)
        {
            var col = GetComponent<MeshCollider>();
            if (col == null) col = gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = roadMesh;
        }

        Debug.Log($"SplineRoadBuilder: Carretera generada ({sampleCount} muestras, {length:F1} m de largo).", this);
    }

    /// <summary>
    /// Longitud total del spline en metros (espacio de mundo).
    /// </summary>
    public float GetLength()
    {
        if (splineContainer == null) return 0f;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return 0f;
        return splineContainer.CalculateLength(splineIndex);
    }

    /// <summary>
    /// Devuelve un punto del mundo a una distancia dada a lo largo del spline.
    /// Ideal para colocar paradas de bus, semáforos o puntos de tráfico sobre la vía.
    /// </summary>
    public Vector3 GetPointAtDistance(float distance)
    {
        if (splineContainer == null) return transform.position;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return transform.position;

        float length = GetLength();
        float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;
        return splineContainer.EvaluatePosition(splineIndex, t);
    }

    /// <summary>
    /// Devuelve la tangente (dirección de la vía) en el mundo a una distancia dada.
    /// </summary>
    public Vector3 GetTangentAtDistance(float distance)
    {
        if (splineContainer == null) return transform.forward;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return transform.forward;

        float length = GetLength();
        float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;
        return ((Vector3)splineContainer.EvaluateTangent(splineIndex, t)).normalized;
    }
}

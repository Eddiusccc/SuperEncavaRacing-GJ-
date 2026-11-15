using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Genera una carretera, puente o viaducto a partir de un Spline de Unity.
/// </summary>
/// <remarks>
/// Características:
/// - Asfalto con UVs en metros (la textura no se estira a lo largo de toda la vía).
/// - Bordillos a ambos lados con winding correcto (se ven las dos caras).
/// - Modo puente: losa con laterales y fondo de profundidad configurable.
/// - Barandillas opcionales con postes y barras en ambos lados, limitables a un tramo (por ejemplo, solo el puente) y con distancia lateral configurable respecto al asfalto.
/// - Pilares opcionales que bajan hasta el terreno (raycast), con material propio.
/// - Cuatro submallas con materiales independientes: asfalto, estructura, metal y pilares.
///
/// Requisitos: com.unity.splines 2.9.x (API probada con 2.9.2 en Unity 6.3).
/// La malla se genera en espacio de mundo: el GameObject queda en identidad.
/// No lo cuelgues de un padre con escala distinta de 1.
/// </remarks>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SplineRoadBuilder : MonoBehaviour
{
    public enum PierPlacement
    {
        Center,
        BothSides,
        LeftSide,
        RightSide
    }

    [Header("Referencias Spline")]
    [Tooltip("SplineContainer que contiene la ruta usada para generar la carretera.")]
    [SerializeField] SplineContainer splineContainer;
    [Tooltip("Índice del spline dentro del SplineContainer que se utilizará.")]
    [SerializeField] int splineIndex = 0;

    [Header("Carretera")]
    [Tooltip("Ancho total del asfalto en metros.")]
    [SerializeField] float roadWidth = 7f;
    [Tooltip("Ancho del bordillo o acera en cada lado, en metros.")]
    [SerializeField] float curbWidth = 1f;
    [Tooltip("Altura del bordillo respecto al asfalto, en metros.")]
    [SerializeField] float curbHeight = 0.15f;
    [Tooltip("Distancia entre muestras del spline en metros. Menos = más suave, más polígonos.")]
    [SerializeField] float sampleStep = 2f;
    [Tooltip("Intensidad de la inclinación en curvas. 0 deja la carretera plana.")]
    [SerializeField] float bankFactor = 0.25f;
    [Tooltip("Metros que cubre una repetición de la textura de asfalto. Si usas una textura con líneas, ponlo igual al ancho de la vía.")]
    [SerializeField] float roadUvTileSize = 8f;
    [Tooltip("Material del asfalto (URP/Lit).")]
    [SerializeField] Material roadMaterial;

    [Header("Puente / Estructura")]
    [Tooltip("Activa la losa de puente: laterales y fondo hacia abajo.")]
    [SerializeField] bool generateBridge = false;
    [Tooltip("Profundidad de la estructura del puente bajo el asfalto, en metros.")]
    [SerializeField] float bridgeDepth = 2.5f;
    [Tooltip("Metros que cubre una repetición de la textura de concreto/estructura.")]
    [SerializeField] float structureUvTileSize = 4f;
    [Tooltip("Material de concreto/piedra para bordillos y laterales del puente.")]
    [SerializeField] Material structureMaterial;

    [Header("Barandillas")]
    [Tooltip("Genera barandillas metálicas a lo largo de la vía.")]
    [SerializeField] bool generateRailings = false;
    [SerializeField] bool railingLeft = true;
    [SerializeField] bool railingRight = true;
    [Tooltip("Altura de la barandilla sobre el bordillo, en metros.")]
    [SerializeField] float railingHeight = 1.1f;
    [Tooltip("Separación entre postes de la barandilla, en metros.")]
    [SerializeField] float railingPostSpacing = 3f;
    [SerializeField] float railingPostThickness = 0.08f;
    [SerializeField] float railingBarThickness = 0.06f;
    [Tooltip("Coloca la barandilla en el borde exterior del bordillo (comportamiento clásico).")]
    [SerializeField] bool railingAtCurbEdge = true;
    [Tooltip("Si 'Railing At Curb Edge' está desactivado: distancia desde el borde del asfalto hasta la barandilla, en metros. Menor que Curb Width = sobre el bordillo; mayor = por fuera (puede quedar sin apoyo).")]
    [SerializeField] float railingDistanceFromAsphalt = 1f;
    [Tooltip("Distancia (m) desde el inicio del spline donde empieza la barandilla. 0 = desde el inicio.")]
    [SerializeField] float railingStartDistance = 0f;
    [Tooltip("Distancia (m) desde el inicio del spline donde termina la barandilla. 0 o menor que el inicio = hasta el final del spline.")]
    [SerializeField] float railingEndDistance = 0f;
    [Tooltip("Material metálico para las barandillas.")]
    [SerializeField] Material railingMaterial;

    [Header("Pilares (opcional)")]
    [Tooltip("Genera pilares verticales bajo el puente hasta el terreno.")]
    [SerializeField] bool generatePiers = false;
    [Tooltip("Dónde se colocan los pilares: centro, ambos lados, solo izquierda o solo derecha.")]
    [SerializeField] PierPlacement pierPlacement = PierPlacement.BothSides;
    [Tooltip("Separación entre pilares a lo largo de la vía, en metros.")]
    [SerializeField] float pierSpacing = 20f;
    [Tooltip("Distancia desde el inicio del spline hasta el primer pilar, en metros.")]
    [SerializeField] float pierStartOffset = 2f;
    [Tooltip("Ancho de cada pilar, en metros.")]
    [SerializeField] float pierWidth = 1.2f;
    [Tooltip("Margen desde el borde de la losa hacia adentro. Menor = pilares más al borde y más espacio libre para la vía de abajo.")]
    [SerializeField] float pierInset = 0.3f;
    [Tooltip("Cuánto se entierra el pilar dentro de la losa del puente, en metros (evita huecos).")]
    [SerializeField] float pierTopEmbed = 0.3f;
    [Tooltip("Cuánto se entierra el pilar bajo el terreno, en metros (evita que flote).")]
    [SerializeField] float pierBottomEmbed = 0.3f;
    [Tooltip("Altura mínima del pilar. Si el terreno está muy cerca, no se genera.")]
    [SerializeField] float pierMinHeight = 1.5f;
    [Tooltip("Capas que cuentan como terreno para el raycast. Excluye la capa de las carreteras para no chocar con el asfalto de abajo.")]
    [SerializeField] LayerMask pierGroundMask = ~0;
    [Tooltip("Distancia máxima del raycast para encontrar el terreno.")]
    [SerializeField] float pierMaxDepth = 80f;
    [Tooltip("Metros que cubre una repetición de la textura de los pilares.")]
    [SerializeField] float pierUvTileSize = 2f;
    [Tooltip("Material específico de los pilares (piedra, concreto, ladrillo...). Si se deja vacío, usa el material de estructura.")]
    [SerializeField] Material pierMaterial;

    [Header("Colisiones")]
    [Tooltip("Genera o actualiza un MeshCollider usando la malla horneada.")]
    [SerializeField] bool generateCollider = true;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh roadMesh;

    // Datos por muestra del spline.
    struct Sample
    {
        public Vector3 center;
        public Vector3 tangent;
        public Vector3 up;      // Arriba inclinado por banking.
        public Vector3 right;
        public Vector3 leftEdge;    // Borde izquierdo del asfalto.
        public Vector3 rightEdge;   // Borde derecho del asfalto.
        public Vector3 leftCurb;    // Borde exterior del bordillo izquierdo.
        public Vector3 rightCurb;   // Borde exterior del bordillo derecho.
        public Vector3 leftBottom;  // Fondo de la estructura izquierda.
        public Vector3 rightBottom; // Fondo de la estructura derecha.
        public float distance;
        public float t;
    }

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
    }

    /// <summary>
    /// Dibuja en la escena el tramo exacto que cubrirá la barandilla.
    /// Sirve para ajustar Railing Start/End Distance sin adivinar.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!generateRailings || splineContainer == null) return;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return;

        float length = GetLength();
        if (length <= 0f) return;

        float start = Mathf.Clamp(railingStartDistance, 0f, length);
        float end = railingEndDistance > start
            ? Mathf.Clamp(railingEndDistance, 0f, length)
            : length;

        if (end <= start) return;

        Gizmos.color = new Color(1f, 0.55f, 0f, 1f);

        const int steps = 64;

        // Dos líneas, una por cada barandilla, a la distancia lateral configurada.
        for (int side = 0; side < 2; side++)
        {
            bool leftSide = side == 0;
            Vector3 prev = GetRailingGizmoPoint(start, length, leftSide);

            for (int i = 1; i <= steps; i++)
            {
                float d = Mathf.Lerp(start, end, i / (float)steps);
                Vector3 p = GetRailingGizmoPoint(d, length, leftSide);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetRailingGizmoPoint(start, length, true), 0.3f);
        Gizmos.DrawSphere(GetRailingGizmoPoint(end, length, true), 0.3f);
        Gizmos.DrawSphere(GetRailingGizmoPoint(start, length, false), 0.3f);
        Gizmos.DrawSphere(GetRailingGizmoPoint(end, length, false), 0.3f);
    }

    /// <summary>
    /// Genera la malla completa desde el spline asignado.
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

        // La malla se genera en espacio de mundo: el objeto va en identidad.
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;

        // Protecciones del Inspector.
        sampleStep = Mathf.Max(0.25f, sampleStep);
        roadWidth = Mathf.Max(0.5f, roadWidth);
        curbWidth = Mathf.Max(0f, curbWidth);
        curbHeight = Mathf.Max(0f, curbHeight);
        bridgeDepth = Mathf.Max(curbHeight, bridgeDepth);
        roadUvTileSize = Mathf.Max(0.25f, roadUvTileSize);
        structureUvTileSize = Mathf.Max(0.25f, structureUvTileSize);
        railingHeight = Mathf.Max(0.1f, railingHeight);
        railingPostSpacing = Mathf.Max(0.5f, railingPostSpacing);
        railingPostThickness = Mathf.Max(0.01f, railingPostThickness);
        railingBarThickness = Mathf.Max(0.01f, railingBarThickness);
        railingDistanceFromAsphalt = Mathf.Max(0f, railingDistanceFromAsphalt);
        railingStartDistance = Mathf.Max(0f, railingStartDistance);
        railingEndDistance = Mathf.Max(0f, railingEndDistance);
        pierSpacing = Mathf.Max(1f, pierSpacing);
        pierStartOffset = Mathf.Max(0f, pierStartOffset);
        pierWidth = Mathf.Max(0.1f, pierWidth);
        pierInset = Mathf.Max(0f, pierInset);
        pierTopEmbed = Mathf.Max(0f, pierTopEmbed);
        pierBottomEmbed = Mathf.Max(0f, pierBottomEmbed);
        pierMinHeight = Mathf.Max(0f, pierMinHeight);
        pierMaxDepth = Mathf.Max(1f, pierMaxDepth);
        pierUvTileSize = Mathf.Max(0.25f, pierUvTileSize);

        float length = splineContainer.CalculateLength(splineIndex);
        if (length <= 0f) return;

        int sampleCount = Mathf.Clamp(Mathf.CeilToInt(length / sampleStep) + 1, 2, 8192);

        // 1) Muestrear el spline.
        var samples = new List<Sample>(sampleCount);
        Vector3 prevTangent = Vector3.forward;

        for (int i = 0; i < sampleCount; i++)
        {
            float distance = Mathf.Min(i * sampleStep, length);
            float stepDist = (i == 0) ? sampleStep : distance - Mathf.Min((i - 1) * sampleStep, length);
            if (stepDist <= 0f) stepDist = sampleStep;

            float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;

            Vector3 pos = splineContainer.EvaluatePosition(splineIndex, t);
            Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(splineIndex, t)).normalized;
            Vector3 up = ((Vector3)splineContainer.EvaluateUpVector(splineIndex, t)).normalized;

            if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;
            if (up.sqrMagnitude < 0.0001f) up = Vector3.up;

            // Banking.
            float curvature = (i == 0) ? 0f : Vector3.Angle(prevTangent, tangent) / stepDist;
            float bank = curvature * bankFactor;
            Vector3 rolledUp = (Quaternion.AngleAxis(bank, tangent) * up).normalized;
            Vector3 right = Vector3.Cross(tangent, rolledUp).normalized;
            if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

            float halfWidth = roadWidth * 0.5f;
            float curbOuter = halfWidth + curbWidth;
            float structureDepth = generateBridge ? bridgeDepth : curbHeight;

            samples.Add(new Sample
            {
                center = pos,
                tangent = tangent,
                up = rolledUp,
                right = right,
                leftEdge = pos - right * halfWidth,
                rightEdge = pos + right * halfWidth,
                leftCurb = pos - right * curbOuter - rolledUp * curbHeight,
                rightCurb = pos + right * curbOuter - rolledUp * curbHeight,
                leftBottom = pos - right * curbOuter - rolledUp * structureDepth,
                rightBottom = pos + right * curbOuter - rolledUp * structureDepth,
                distance = distance,
                t = t
            });

            prevTangent = tangent;
        }

        // 2) Listas de malla. Cuatro submallas: 0 = asfalto, 1 = estructura, 2 = metal, 3 = pilares.
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var roadIndices = new List<int>();
        var structureIndices = new List<int>();
        var railingIndices = new List<int>();
        var pierIndices = new List<int>();

        // 3) Carretera, bordillos, laterales y fondo.
        for (int i = 0; i < samples.Count - 1; i++)
        {
            Sample a = samples[i];
            Sample b = samples[i + 1];

            float roadUa = a.distance / roadUvTileSize;
            float roadUb = b.distance / roadUvTileSize;
            float roadV = roadWidth / roadUvTileSize;

            // Asfalto.
            AddQuad(vertices, normals, uvs, roadIndices,
                a.leftEdge, a.rightEdge, b.rightEdge, b.leftEdge, a.up,
                new Vector2(roadUa, 0f), new Vector2(roadUa, roadV),
                new Vector2(roadUb, roadV), new Vector2(roadUb, 0f));

            float structUa = a.distance / structureUvTileSize;
            float structUb = b.distance / structureUvTileSize;
            float curbV = curbWidth / structureUvTileSize;
            float sideV = (generateBridge ? bridgeDepth : curbHeight) / structureUvTileSize;
            float bottomV = (roadWidth + curbWidth * 2f) / structureUvTileSize;

            // Bordillo izquierdo (superficie).
            AddQuad(vertices, normals, uvs, structureIndices,
                a.leftEdge, a.leftCurb, b.leftCurb, b.leftEdge, a.up,
                new Vector2(structUa, 0f), new Vector2(structUa, curbV),
                new Vector2(structUb, curbV), new Vector2(structUb, 0f));

            // Bordillo derecho (superficie).
            AddQuad(vertices, normals, uvs, structureIndices,
                a.rightEdge, a.rightCurb, b.rightCurb, b.rightEdge, a.up,
                new Vector2(structUa, 0f), new Vector2(structUa, curbV),
                new Vector2(structUb, curbV), new Vector2(structUb, 0f));

            // Lateral izquierdo (mira hacia -right).
            AddQuad(vertices, normals, uvs, structureIndices,
                a.leftCurb, a.leftBottom, b.leftBottom, b.leftCurb, -a.right,
                new Vector2(structUa, 0f), new Vector2(structUa, sideV),
                new Vector2(structUb, sideV), new Vector2(structUb, 0f));

            // Lateral derecho (mira hacia +right).
            AddQuad(vertices, normals, uvs, structureIndices,
                a.rightCurb, a.rightBottom, b.rightBottom, b.rightCurb, a.right,
                new Vector2(structUa, 0f), new Vector2(structUa, sideV),
                new Vector2(structUb, sideV), new Vector2(structUb, 0f));

            // Fondo de la losa (solo en modo puente).
            if (generateBridge)
            {
                AddQuad(vertices, normals, uvs, structureIndices,
                    a.leftBottom, a.rightBottom, b.rightBottom, b.leftBottom, -a.up,
                    new Vector2(structUa, 0f), new Vector2(structUa, bottomV),
                    new Vector2(structUb, bottomV), new Vector2(structUb, 0f));
            }
        }

        // 4) Tapas en los extremos para cerrar la estructura.
        AddDeckCap(vertices, normals, uvs, structureIndices, samples[0], -samples[0].tangent);
        AddDeckCap(vertices, normals, uvs, structureIndices, samples[samples.Count - 1], samples[samples.Count - 1].tangent);

        // 5) Barandillas.
        if (generateRailings)
        {
            float railStart = Mathf.Clamp(railingStartDistance, 0f, length);
            float railEnd = railingEndDistance > railStart
                ? Mathf.Clamp(railingEndDistance, 0f, length)
                : length;

            if (railEnd > railStart + 0.01f)
            {
                if (railingLeft) AddRailing(vertices, normals, uvs, railingIndices, samples, true, railStart, railEnd);
                if (railingRight) AddRailing(vertices, normals, uvs, railingIndices, samples, false, railStart, railEnd);
            }
        }

        // 6) Pilares opcionales.
        if (generateBridge && generatePiers)
        {
            AddPiers(vertices, normals, uvs, pierIndices, samples);
        }

        // 7) Construir el Mesh.
        if (roadMesh == null) roadMesh = new Mesh();
        roadMesh.Clear();
        roadMesh.subMeshCount = 4;
        roadMesh.SetVertices(vertices);
        roadMesh.SetNormals(normals);
        roadMesh.SetUVs(0, uvs);
        roadMesh.SetIndices(roadIndices.ToArray(), MeshTopology.Triangles, 0);
        roadMesh.SetIndices(structureIndices.ToArray(), MeshTopology.Triangles, 1);
        if (railingIndices.Count > 0)
            roadMesh.SetIndices(railingIndices.ToArray(), MeshTopology.Triangles, 2);
        if (pierIndices.Count > 0)
            roadMesh.SetIndices(pierIndices.ToArray(), MeshTopology.Triangles, 3);
        roadMesh.RecalculateTangents();
        roadMesh.RecalculateBounds();

        if (meshFilter != null) meshFilter.sharedMesh = roadMesh;

        // 8) Materiales por submalla.
        if (meshRenderer != null)
        {
            if (roadMaterial == null)
                Debug.LogWarning("SplineRoadBuilder: no hay material de asfalto asignado. La vía se verá con el material por defecto.", this);

            Material asphalt = roadMaterial;
            Material structure = structureMaterial != null ? structureMaterial : asphalt;
            Material metal = railingMaterial != null ? railingMaterial : structure;
            Material piers = pierMaterial != null ? pierMaterial : structure;
            meshRenderer.sharedMaterials = new[] { asphalt, structure, metal, piers };
        }

        // 9) Colisión.
        if (generateCollider)
        {
            var col = GetComponent<MeshCollider>();
            if (col == null) col = gameObject.AddComponent<MeshCollider>();
            col.sharedMesh = roadMesh;
        }

        Debug.Log($"SplineRoadBuilder: generado ({sampleCount} muestras, {length:F1} m, puente={generateBridge}, barandillas={generateRailings}).", this);
    }

    /// <summary>
    /// Borra la malla generada.
    /// </summary>
    [ContextMenu("Clear Road")]
    public void Clear()
    {
        if (roadMesh != null) roadMesh.Clear();
        if (meshFilter != null) meshFilter.sharedMesh = null;

        var col = GetComponent<MeshCollider>();
        if (col != null) col.sharedMesh = null;
    }

    // ---------------------------------------------------------------------
    // Generación de geometría
    // ---------------------------------------------------------------------

    void AddDeckCap(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                    List<int> indices, Sample s, Vector3 normal)
    {
        float uMax = (roadWidth * 0.5f + curbWidth) / structureUvTileSize;
        float vMax = (generateBridge ? bridgeDepth : curbHeight) / structureUvTileSize;

        // Parte inferior (laterales + fondo).
        AddQuad(vertices, normals, uvs, indices,
            s.leftCurb, s.rightCurb, s.rightBottom, s.leftBottom, normal,
            new Vector2(0f, 0f), new Vector2(uMax, 0f),
            new Vector2(uMax, vMax), new Vector2(0f, vMax));

        // Parte superior (entre asfalto y bordillos).
        AddQuad(vertices, normals, uvs, indices,
            s.leftEdge, s.rightEdge, s.rightCurb, s.leftCurb, normal,
            new Vector2(0f, 0f), new Vector2(uMax, 0f),
            new Vector2(uMax, vMax), new Vector2(0f, vMax));
    }

    void AddRailing(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                    List<int> indices, List<Sample> samples, bool leftSide,
                    float startDistance, float endDistance)
    {
        float halfBar = railingBarThickness * 0.5f;

        // Dos barras horizontales: superior y media.
        float[] heights = { railingHeight, railingHeight * 0.55f };

        foreach (float height in heights)
        {
            for (int i = 0; i < samples.Count - 1; i++)
            {
                Sample a = samples[i];
                Sample b = samples[i + 1];

                // Solo los tramos que caen dentro del rango de la barandilla.
                if (b.distance <= startDistance || a.distance >= endDistance) continue;

                // Recorta el tramo en los extremos exactos del rango.
                float t0 = 0f;
                float t1 = 1f;
                if (a.distance < startDistance)
                    t0 = Mathf.InverseLerp(a.distance, b.distance, startDistance);
                if (b.distance > endDistance)
                    t1 = Mathf.InverseLerp(a.distance, b.distance, endDistance);

                Sample sa = LerpSample(a, b, t0);
                Sample sb = LerpSample(a, b, t1);

                Vector3 baseA = GetRailingBasePoint(sa, leftSide);
                Vector3 baseB = GetRailingBasePoint(sb, leftSide);

                Vector3 ca = baseA + sa.up * height;
                Vector3 cb = baseB + sb.up * height;

                // Esquinas de la sección cuadrada de la barra.
                Vector3 aTL = ca - sa.right * halfBar + sa.up * halfBar;
                Vector3 aTR = ca + sa.right * halfBar + sa.up * halfBar;
                Vector3 aBL = ca - sa.right * halfBar - sa.up * halfBar;
                Vector3 aBR = ca + sa.right * halfBar - sa.up * halfBar;

                Vector3 bTL = cb - sb.right * halfBar + sb.up * halfBar;
                Vector3 bTR = cb + sb.right * halfBar + sb.up * halfBar;
                Vector3 bBL = cb - sb.right * halfBar - sb.up * halfBar;
                Vector3 bBR = cb + sb.right * halfBar - sb.up * halfBar;

                float ua = sa.distance / structureUvTileSize;
                float ub = sb.distance / structureUvTileSize;

                // Cara superior.
                AddQuad(vertices, normals, uvs, indices,
                    aTL, aTR, bTR, bTL, sa.up,
                    new Vector2(ua, 0f), new Vector2(ua, 1f), new Vector2(ub, 1f), new Vector2(ub, 0f));

                // Cara inferior.
                AddQuad(vertices, normals, uvs, indices,
                    aBL, aBR, bBR, bBL, -sa.up,
                    new Vector2(ua, 0f), new Vector2(ua, 1f), new Vector2(ub, 1f), new Vector2(ub, 0f));

                // Cara interior.
                AddQuad(vertices, normals, uvs, indices,
                    aTL, aBL, bBL, bTL, -sa.right,
                    new Vector2(ua, 0f), new Vector2(ua, 1f), new Vector2(ub, 1f), new Vector2(ub, 0f));

                // Cara exterior.
                AddQuad(vertices, normals, uvs, indices,
                    aTR, aBR, bBR, bTR, sa.right,
                    new Vector2(ua, 0f), new Vector2(ua, 1f), new Vector2(ub, 1f), new Vector2(ub, 0f));
            }

            // Tapas en los extremos del rango para cerrar el tubo de la barra.
            Sample startSample = SampleAtDistance(samples, startDistance);
            Sample endSample = SampleAtDistance(samples, endDistance);
            AddRailingBarCap(vertices, normals, uvs, indices, startSample, height, leftSide, -startSample.tangent);
            AddRailingBarCap(vertices, normals, uvs, indices, endSample, height, leftSide, endSample.tangent);
        }

        // Postes: empiezan en el inicio del rango y se repiten hasta el final.
        float postDistance = startDistance;
        float lastPostDistance = float.NegativeInfinity;

        while (postDistance <= endDistance + 0.001f)
        {
            Sample s = SampleAtDistance(samples, postDistance);
            AddRailingPost(vertices, normals, uvs, indices, s, leftSide);
            lastPostDistance = postDistance;
            postDistance += railingPostSpacing;
        }

        // Asegura un poste en el extremo final si el último quedó muy lejos.
        if (endDistance - lastPostDistance > railingPostSpacing * 0.5f)
        {
            Sample s = SampleAtDistance(samples, endDistance);
            AddRailingPost(vertices, normals, uvs, indices, s, leftSide);
        }
    }

    void AddRailingPost(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                        List<int> indices, Sample s, bool leftSide)
    {
        Vector3 basePoint = GetRailingBasePoint(s, leftSide);
        Vector3 center = basePoint + s.up * (railingHeight * 0.5f);
        Vector3 size = new Vector3(railingPostThickness, railingHeight, railingPostThickness);

        AddBox(vertices, normals, uvs, indices,
            center, size, s.right, s.up, s.tangent, 1f);
    }

    /// <summary>
    /// Punto base de la barandilla (donde nace el poste) para un lado de la vía.
    /// Si <see cref="railingAtCurbEdge"/> está activo usa el borde exterior del bordillo;
    /// si no, usa <see cref="railingDistanceFromAsphalt"/> medido desde el borde del asfalto.
    /// </summary>
    Vector3 GetRailingBasePoint(Sample s, bool leftSide)
    {
        return GetRailingBasePoint(s.center, s.right, s.up, leftSide);
    }

    /// <summary>
    /// Punto base de la barandilla a partir del centro, el vector derecha y el arriba.
    /// </summary>
    Vector3 GetRailingBasePoint(Vector3 center, Vector3 right, Vector3 up, bool leftSide)
    {
        float halfRoad = roadWidth * 0.5f;
        float lateralDistance = halfRoad +
            (railingAtCurbEdge ? curbWidth : Mathf.Max(0f, railingDistanceFromAsphalt));

        // Altura de la superficie del bordillo en esa distancia lateral.
        // En el borde del asfalto la altura es 0; en el borde exterior baja curbHeight.
        float curbOffset = lateralDistance - halfRoad;
        float surfaceDrop = curbWidth > 0.001f
            ? curbHeight * Mathf.Clamp01(curbOffset / curbWidth)
            : curbHeight;

        Vector3 lateral = leftSide ? -right : right;
        return center + lateral * lateralDistance - up * surfaceDrop;
    }

    /// <summary>
    /// Punto de la barandilla a una distancia del spline, para dibujar el gizmo.
    /// </summary>
    Vector3 GetRailingGizmoPoint(float distance, float length, bool leftSide)
    {
        float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;

        Vector3 pos = splineContainer.EvaluatePosition(splineIndex, t);
        Vector3 tangent = ((Vector3)splineContainer.EvaluateTangent(splineIndex, t)).normalized;
        Vector3 up = ((Vector3)splineContainer.EvaluateUpVector(splineIndex, t)).normalized;
        Vector3 right = Vector3.Cross(tangent, up).normalized;
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;

        Vector3 basePoint = GetRailingBasePoint(pos, right, up, leftSide);
        return basePoint + up * (railingHeight + 0.3f);
    }

    /// <summary>
    /// Tapa el extremo de una barra de la barandilla (el tubo cuadrado).
    /// </summary>
    void AddRailingBarCap(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                          List<int> indices, Sample s, float height, bool leftSide, Vector3 normal)
    {
        float halfBar = railingBarThickness * 0.5f;

        Vector3 basePoint = GetRailingBasePoint(s, leftSide);
        Vector3 c = basePoint + s.up * height;

        Vector3 tl = c - s.right * halfBar + s.up * halfBar;
        Vector3 tr = c + s.right * halfBar + s.up * halfBar;
        Vector3 bl = c - s.right * halfBar - s.up * halfBar;
        Vector3 br = c + s.right * halfBar - s.up * halfBar;

        AddQuad(vertices, normals, uvs, indices,
            tl, tr, br, bl, normal,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f));
    }

    /// <summary>
    /// Interpola dos muestras del spline (posición, orientación y distancia).
    /// Se usa para recortar las barandillas exactamente en los extremos del rango.
    /// </summary>
    Sample LerpSample(Sample a, Sample b, float t)
    {
        t = Mathf.Clamp01(t);

        return new Sample
        {
            center = Vector3.Lerp(a.center, b.center, t),
            tangent = Vector3.Slerp(a.tangent, b.tangent, t).normalized,
            up = Vector3.Slerp(a.up, b.up, t).normalized,
            right = Vector3.Slerp(a.right, b.right, t).normalized,
            leftEdge = Vector3.Lerp(a.leftEdge, b.leftEdge, t),
            rightEdge = Vector3.Lerp(a.rightEdge, b.rightEdge, t),
            leftCurb = Vector3.Lerp(a.leftCurb, b.leftCurb, t),
            rightCurb = Vector3.Lerp(a.rightCurb, b.rightCurb, t),
            leftBottom = Vector3.Lerp(a.leftBottom, b.leftBottom, t),
            rightBottom = Vector3.Lerp(a.rightBottom, b.rightBottom, t),
            distance = Mathf.Lerp(a.distance, b.distance, t),
            t = Mathf.Lerp(a.t, b.t, t)
        };
    }

    /// <summary>
    /// Devuelve una muestra interpolada a una distancia concreta del spline.
    /// </summary>
    Sample SampleAtDistance(List<Sample> samples, float distance)
    {
        if (samples == null || samples.Count == 0) return default;

        if (distance <= samples[0].distance) return samples[0];

        for (int i = 0; i < samples.Count - 1; i++)
        {
            if (distance <= samples[i + 1].distance)
            {
                float t = Mathf.InverseLerp(samples[i].distance, samples[i + 1].distance, distance);
                return LerpSample(samples[i], samples[i + 1], t);
            }
        }

        return samples[samples.Count - 1];
    }

    void AddPiers(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                  List<int> indices, List<Sample> samples)
    {
        float halfDeck = roadWidth * 0.5f + curbWidth;
        // Centro del pilar medido desde el eje de la vía. Se mantiene dentro de la losa.
        float sideOffset = Mathf.Max(0f, halfDeck - pierInset - pierWidth * 0.5f);

        float nextPierDistance = pierStartOffset;

        foreach (Sample s in samples)
        {
            if (s.distance < nextPierDistance) continue;
            nextPierDistance += pierSpacing;

            switch (pierPlacement)
            {
                case PierPlacement.Center:
                    AddPierAt(vertices, normals, uvs, indices, s, 0f);
                    break;

                case PierPlacement.BothSides:
                    if (sideOffset > 0.001f)
                    {
                        AddPierAt(vertices, normals, uvs, indices, s, -sideOffset);
                        AddPierAt(vertices, normals, uvs, indices, s, sideOffset);
                    }
                    else
                    {
                        // Si la losa es muy angosta, cae al centro para no salirse del puente.
                        AddPierAt(vertices, normals, uvs, indices, s, 0f);
                    }
                    break;

                case PierPlacement.LeftSide:
                    AddPierAt(vertices, normals, uvs, indices, s, -sideOffset);
                    break;

                case PierPlacement.RightSide:
                    AddPierAt(vertices, normals, uvs, indices, s, sideOffset);
                    break;
            }
        }
    }

    /// <summary>
    /// Añade un pilar vertical en una muestra del spline, desplazado lateralmente
    /// respecto al eje de la vía (<paramref name="lateralOffset"/> en metros).
    /// </summary>
    void AddPierAt(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                   List<int> indices, Sample s, float lateralOffset)
    {
        Vector3 deckBottom = (s.leftBottom + s.rightBottom) * 0.5f;
        Vector3 top = deckBottom + s.right * lateralOffset;

        // Origen un poco por debajo del fondo de la losa para no golpear la propia losa.
        Vector3 rayOrigin = top + Vector3.down * 0.1f;

        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, pierMaxDepth, pierGroundMask, QueryTriggerInteraction.Ignore))
            return;

        // El pilar se entierra en la losa (arriba) y en el terreno (abajo).
        float topY = top.y + pierTopEmbed;
        float bottomY = hit.point.y - pierBottomEmbed;
        float height = topY - bottomY;

        if (height < pierMinHeight) return;

        Vector3 center = new Vector3(top.x, (topY + bottomY) * 0.5f, top.z);
        Vector3 size = new Vector3(pierWidth, height, pierWidth);

        // Pilares verticales, alineados con el eje de la vía para que la sección
        // quede paralela a la losa. Se usa una base horizontal para que el pilar
        // no quede torcido en rampas.
        Vector3 rightH = Vector3.ProjectOnPlane(s.right, Vector3.up);
        if (rightH.sqrMagnitude < 0.0001f) rightH = Vector3.right;
        rightH.Normalize();

        Vector3 forward = Vector3.Cross(rightH, Vector3.up).normalized;

        AddBox(vertices, normals, uvs, indices,
            center, size, rightH, Vector3.up, forward, pierUvTileSize);
    }

    /// <summary>
    /// Añade una caja orientada. Se usa para postes y pilares.
    /// </summary>
    void AddBox(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                List<int> indices, Vector3 center, Vector3 size,
                Vector3 right, Vector3 up, Vector3 forward, float uvScale)
    {
        Vector3 h = size * 0.5f;
        uvScale = Mathf.Max(0.01f, uvScale);

        Vector3 c000 = center - right * h.x - up * h.y - forward * h.z;
        Vector3 c100 = center + right * h.x - up * h.y - forward * h.z;
        Vector3 c110 = center + right * h.x + up * h.y - forward * h.z;
        Vector3 c010 = center - right * h.x + up * h.y - forward * h.z;
        Vector3 c001 = center - right * h.x - up * h.y + forward * h.z;
        Vector3 c101 = center + right * h.x - up * h.y + forward * h.z;
        Vector3 c111 = center + right * h.x + up * h.y + forward * h.z;
        Vector3 c011 = center - right * h.x + up * h.y + forward * h.z;

        float sx = size.x / uvScale;
        float sy = size.y / uvScale;
        float sz = size.z / uvScale;

        // Atrás (-forward).
        AddQuad(vertices, normals, uvs, indices, c000, c100, c110, c010, -forward,
            new Vector2(0f, 0f), new Vector2(sx, 0f), new Vector2(sx, sy), new Vector2(0f, sy));

        // Frente (+forward).
        AddQuad(vertices, normals, uvs, indices, c001, c101, c111, c011, forward,
            new Vector2(0f, 0f), new Vector2(sx, 0f), new Vector2(sx, sy), new Vector2(0f, sy));

        // Abajo (-up).
        AddQuad(vertices, normals, uvs, indices, c000, c100, c101, c001, -up,
            new Vector2(0f, 0f), new Vector2(sx, 0f), new Vector2(sx, sz), new Vector2(0f, sz));

        // Arriba (+up).
        AddQuad(vertices, normals, uvs, indices, c010, c110, c111, c011, up,
            new Vector2(0f, 0f), new Vector2(sx, 0f), new Vector2(sx, sz), new Vector2(0f, sz));

        // Izquierda (-right).
        AddQuad(vertices, normals, uvs, indices, c000, c010, c011, c001, -right,
            new Vector2(0f, 0f), new Vector2(sz, 0f), new Vector2(sz, sy), new Vector2(0f, sy));

        // Derecha (+right).
        AddQuad(vertices, normals, uvs, indices, c100, c110, c111, c101, right,
            new Vector2(0f, 0f), new Vector2(sz, 0f), new Vector2(sz, sy), new Vector2(0f, sy));
    }

    /// <summary>
    /// Añade un quad corrigiendo automáticamente el orden de los vértices para que
    /// la cara mire hacia <paramref name="normal"/>. Esto evita el problema de que
    /// un lado se vea y el otro no (backface culling).
    /// </summary>
    void AddQuad(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
                 List<int> indices,
                 Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal,
                 Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
    {
        Vector3 faceNormal = Vector3.Cross(b - a, c - a);
        bool flip = Vector3.Dot(faceNormal, normal) < 0f;

        int i0 = vertices.Count;

        if (!flip)
        {
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            uvs.Add(uvA); uvs.Add(uvB); uvs.Add(uvC); uvs.Add(uvD);
        }
        else
        {
            vertices.Add(a); vertices.Add(d); vertices.Add(c); vertices.Add(b);
            uvs.Add(uvA); uvs.Add(uvD); uvs.Add(uvC); uvs.Add(uvB);
        }

        normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);

        indices.Add(i0); indices.Add(i0 + 1); indices.Add(i0 + 2);
        indices.Add(i0); indices.Add(i0 + 2); indices.Add(i0 + 3);
    }

    // ---------------------------------------------------------------------
    // Helpers públicos
    // ---------------------------------------------------------------------

    /// <summary>Longitud total del spline en metros (espacio de mundo).</summary>
    public float GetLength()
    {
        if (splineContainer == null) return 0f;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return 0f;
        return splineContainer.CalculateLength(splineIndex);
    }

    /// <summary>Punto del mundo a una distancia dada a lo largo del spline.</summary>
    public Vector3 GetPointAtDistance(float distance)
    {
        if (splineContainer == null) return transform.position;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return transform.position;

        float length = GetLength();
        float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;
        return splineContainer.EvaluatePosition(splineIndex, t);
    }

    /// <summary>Tangente (dirección de la vía) en el mundo a una distancia dada.</summary>
    public Vector3 GetTangentAtDistance(float distance)
    {
        if (splineContainer == null) return transform.forward;
        if (splineIndex < 0 || splineIndex >= splineContainer.Splines.Count) return transform.forward;

        float length = GetLength();
        float t = length > 0f ? Mathf.Clamp01(distance / length) : 0f;
        return ((Vector3)splineContainer.EvaluateTangent(splineIndex, t)).normalized;
    }
}

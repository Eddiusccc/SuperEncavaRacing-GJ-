using Framework.Core;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Ciclo de día y noche de la run. Avanza el sol desde el amanecer hasta la noche
/// a medida que transcurre el tiempo de la partida (<see cref="RunTimer"/>).
/// Los colores, curvas e intensidades se configuran en un <see cref="DayNightProfile"/>.
/// </summary>
/// <remarks>
/// Se reinicia al empezar una run nueva y NO se reinicia al reanudar desde pausa
/// o desde la selección de perks, igual que <see cref="RunResetter"/>.
/// </remarks>
[DisallowMultipleComponent]
public class DayNightCycle : MonoBehaviour
{
    /// <summary>De dónde sale el tiempo que controla el ciclo.</summary>
    public enum TimeSource
    {
        RunTimer,
        Independent
    }

    [Header("Referencias")]
    [Tooltip("Luz direccional que hace de sol. Si se deja vacía se busca RenderSettings.sun o la primera luz direccional.")]
    [SerializeField] Light sun;

    [Tooltip("Asset con los colores y curvas del ciclo. Si se deja vacío se usan los valores por defecto en runtime.")]
    [SerializeField] DayNightProfile profile;

    [Header("Duración del día")]
    [Tooltip("RunTimer: el día termina cuando termina la run. Independent: usa los segundos indicados abajo.")]
    [SerializeField] TimeSource timeSource = TimeSource.RunTimer;

    [Tooltip("Duración del día en segundos. Solo se usa con TimeSource = Independent.")]
    [SerializeField, Min(1f)] float dayLengthSeconds = 90f;

    [Header("Reloj del día")]
    [Tooltip("Hora a la que empieza el día (0-24). El progreso 0 corresponde a esta hora.")]
    [SerializeField, Range(0f, 24f)] float startHour = 5f;

    [Tooltip("Hora a la que termina el día (0-24). El progreso 1 corresponde a esta hora. Por defecto 19.5 = 7:30 PM. Si es igual a Start Hour, el día dura 24 h.")]
    [SerializeField, Range(0f, 24f)] float endHour = 19.5f;

    [Tooltip("Publica DayTimeChanged cuando cambia el minuto, para que un widget de HUD pueda mostrar la hora.")]
    [SerializeField] bool publishTimeEvents = true;

    [Header("Control")]
    [Tooltip("Si está activo, el script pone la fuente de luz ambiental en Color (Flat) y anima el color.")]
    [SerializeField] bool controlAmbient = true;

    [Tooltip("Si está activo, tiñe el skybox para que el cielo también se oscurezca. Requiere un skybox con _SkyTint o _Tint (y opcionalmente _Exposure).")]
    [SerializeField] bool controlSkybox = true;

    float progress;
    float runTotal = -1f;
    bool running;
    int lastPublishedHour = int.MinValue;
    int lastPublishedMinute = int.MinValue;

    // Perfil de respaldo cuando no hay uno asignado en el Inspector.
    DayNightProfile runtimeProfile;

    Material skyboxInstance;
    Material originalSkybox;
    int skyTintID = -1;
    bool skyboxHasExposure;
    bool skyboxIsBlend;

    AmbientMode originalAmbientMode;
    Color originalAmbientLight;
    float originalAmbientIntensity;

    // Skybox/Procedural (el de por defecto) usa _SkyTint; los skybox de 6 caras usan _Tint.
    static readonly int ProceduralTintID = Shader.PropertyToID("_SkyTint");
    static readonly int SixSidedTintID = Shader.PropertyToID("_Tint");
    static readonly int ExposureID = Shader.PropertyToID("_Exposure");
    static readonly int DayTexID = Shader.PropertyToID("_DayTex");
    static readonly int NightTexID = Shader.PropertyToID("_NightTex");
    static readonly int BlendID = Shader.PropertyToID("_Blend");

    const string SkyboxBlendShaderName = "Skybox/BlendTwoCubemaps";
    const string SkyboxBlendResourcePath = "SkyboxBlend";

    DayNightProfile ActiveProfile => profile != null ? profile : runtimeProfile;

    /// <summary>Progreso del ciclo: 0 = inicio del día, 1 = fin (noche).</summary>
    public float Progress => progress;

    /// <summary>
    /// Hora actual del día en formato 24 h con decimales.
    /// Con el valor por defecto: 5 = 5:00 AM, 12.5 = 12:30 PM, 19 = 7:00 PM.
    /// </summary>
    public float CurrentHour => HourAt(progress);

    /// <summary>Hora actual en formato 24 h, de 0 a 23.</summary>
    public int CurrentHour24
    {
        get
        {
            SplitHour(CurrentHour, out int hour24, out _);
            return hour24;
        }
    }

    /// <summary>Minuto actual, de 0 a 59.</summary>
    public int CurrentMinute
    {
        get
        {
            SplitHour(CurrentHour, out _, out int minute);
            return minute;
        }
    }

    /// <summary>Hora actual lista para un HUD, con formato de 12 o 24 horas.</summary>
    public DayTimeChanged CurrentTime
    {
        get
        {
            float hours = SplitHour(CurrentHour, out int hour24, out int minute);
            return new DayTimeChanged(hours, hour24, minute);
        }
    }

    /// <summary>Duración del día en horas de juego, respetando el cruce de medianoche.</summary>
    float DayHours
    {
        get
        {
            float hours = Mathf.Repeat(endHour - startHour, 24f);
            return hours <= 0.001f ? 24f : hours;
        }
    }

    void Awake()
    {
        if (sun == null) sun = FindSun();
        if (sun != null) RenderSettings.sun = sun;

        if (profile == null)
        {
            runtimeProfile = ScriptableObject.CreateInstance<DayNightProfile>();
            runtimeProfile.name = "DayNightProfile (Runtime)";
        }

        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientLight = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;

        if (controlAmbient) RenderSettings.ambientMode = AmbientMode.Flat;

        if (controlSkybox)
        {
            if (!TryCreateBlendedSkybox() && RenderSettings.skybox != null)
            {
                originalSkybox = RenderSettings.skybox;
                skyboxInstance = new Material(originalSkybox);
                RenderSettings.skybox = skyboxInstance;

                if (skyboxInstance.HasProperty(ProceduralTintID)) skyTintID = ProceduralTintID;
                else if (skyboxInstance.HasProperty(SixSidedTintID)) skyTintID = SixSidedTintID;

                skyboxHasExposure = skyboxInstance.HasProperty(ExposureID);

                if (skyTintID < 0)
                    Debug.LogWarning("[DayNightCycle] El skybox no tiene _SkyTint ni _Tint. Desactiva Control Skybox o usa el Skybox por defecto.", this);
            }
        }

        Apply(progress);
    }

    /// <summary>
    /// Si el perfil tiene un cubemap de día y otro de noche, crea un material con
    /// el shader de fundido y lo pone como skybox de la escena.
    /// </summary>
    bool TryCreateBlendedSkybox()
    {
        var p = ActiveProfile;
        if (p == null) return false;

        Cubemap dayCube = ResolveCubemap(p.daySkybox, p.daySkyboxMaterial);
        Cubemap nightCube = ResolveCubemap(p.nightSkybox, p.nightSkyboxMaterial);

        if (dayCube == null || nightCube == null)
        {
            if (HasSkyboxAssignment(p))
                Debug.LogWarning("[DayNightCycle] Para el fundido de cielo necesitas un cubemap de día y otro de noche. Se usará el skybox normal.", this);

            return false;
        }

        Shader blendShader = Resources.Load<Shader>(SkyboxBlendResourcePath);
        if (blendShader == null) blendShader = Shader.Find(SkyboxBlendShaderName);

        if (blendShader == null)
        {
            Debug.LogWarning("[DayNightCycle] No se encontró el shader " + SkyboxBlendShaderName + ". Se usará el skybox normal.", this);
            return false;
        }

        originalSkybox = RenderSettings.skybox;
        skyboxInstance = new Material(blendShader);
        skyboxInstance.name = "Skybox (Day/Night Blend)";
        skyboxInstance.SetTexture(DayTexID, dayCube);
        skyboxInstance.SetTexture(NightTexID, nightCube);
        skyboxInstance.SetColor(SixSidedTintID, Color.white);
        skyboxInstance.SetFloat(ExposureID, 1f);
        skyboxInstance.SetFloat(BlendID, p.skyboxBlend != null ? p.skyboxBlend.Evaluate(progress) : 0f);
        RenderSettings.skybox = skyboxInstance;

        skyboxIsBlend = true;
        return true;
    }

    static Cubemap ResolveCubemap(Cubemap direct, Material material)
    {
        if (direct != null) return direct;
        if (material == null) return null;

        if (material.HasProperty("_Tex")) return material.GetTexture("_Tex") as Cubemap;
        if (material.HasProperty("_MainTex")) return material.GetTexture("_MainTex") as Cubemap;
        if (material.HasProperty("_Cube")) return material.GetTexture("_Cube") as Cubemap;

        return null;
    }

    static bool HasSkyboxAssignment(DayNightProfile p)
    {
        return p.daySkybox != null || p.nightSkybox != null ||
               p.daySkyboxMaterial != null || p.nightSkyboxMaterial != null;
    }

    void Start()
    {
        // Si la escena arranca directamente en Playing, el GameStateChanged inicial
        // puede ocurrir antes de que este componente se suscriba.
        var flow = FindFirstObjectByType<GameFlow>();
        running = flow != null && flow.CurrentState == GameState.Playing;
        Apply(progress);

        // Asegura que un widget suscrito en OnEnable reciba el valor inicial.
        PublishTime(force: true);
    }

    void OnEnable()
    {
        EventBus.Subscribe<GameStateChanged>(OnStateChanged);
        EventBus.Subscribe<TimeChanged>(OnTimeChanged);
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChanged>(OnStateChanged);
        EventBus.Unsubscribe<TimeChanged>(OnTimeChanged);
    }

    void OnDestroy()
    {
        if (skyboxInstance != null)
        {
            if (RenderSettings.skybox == skyboxInstance)
                RenderSettings.skybox = originalSkybox;

            Destroy(skyboxInstance);
        }

        if (runtimeProfile != null)
            Destroy(runtimeProfile);

        if (controlAmbient)
        {
            RenderSettings.ambientMode = originalAmbientMode;
            RenderSettings.ambientLight = originalAmbientLight;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
        }
    }

    void Update()
    {
        if (timeSource != TimeSource.Independent || !running) return;
        if (dayLengthSeconds <= 0.01f) return;

        progress = Mathf.Clamp01(progress + Time.deltaTime / dayLengthSeconds);
        Apply(progress);
    }

    void OnStateChanged(GameStateChanged e)
    {
        if (e.Current == GameState.Playing)
        {
            bool newRun = e.Previous != GameState.Paused && e.Previous != GameState.PerkSelection;

            if (newRun)
            {
                progress = 0f;
                runTotal = -1f;
                Apply(progress);
            }

            running = true;
        }
        else
        {
            running = false;
        }
    }

    void OnTimeChanged(TimeChanged e)
    {
        if (timeSource != TimeSource.RunTimer) return;

        if (runTotal <= 0f)
            runTotal = Mathf.Max(e.Remaining, 0.01f);

        float target = Mathf.Clamp01(1f - e.Remaining / runTotal);

        // El sol nunca retrocede si un pasajero añade tiempo: el día se alarga
        // y el sol se queda en la posición en la que estaba.
        progress = Mathf.Max(progress, target);
        Apply(progress);
    }

    /// <summary>Fija el ciclo en un punto concreto (0 = inicio del día, 1 = noche).</summary>
    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);
        Apply(progress);
    }

    /// <summary>
    /// Fija el reloj en una hora del día (por ejemplo, 12.5 = 12:30 PM).
    /// Si la hora cae fuera del rango del día, se ajusta al extremo más cercano.
    /// </summary>
    public void SetHour(float hour)
    {
        float delta = Mathf.Repeat(hour - startHour, 24f);
        SetProgress(delta / DayHours);
    }

    float HourAt(float t)
    {
        return Mathf.Repeat(startHour + Mathf.Clamp01(t) * DayHours, 24f);
    }

    /// <summary>
    /// Convierte una hora decimal en hora y minuto. Redondea al minuto más cercano
    /// para que el final del día no se muestre como 19:29 en lugar de 19:30.
    /// </summary>
    static float SplitHour(float hours, out int hour24, out int minute)
    {
        float normalized = Mathf.Repeat(hours, 24f);
        int totalMinutes = Mathf.RoundToInt(normalized * 60f) % (24 * 60);
        if (totalMinutes < 0) totalMinutes += 24 * 60;

        hour24 = totalMinutes / 60;
        minute = totalMinutes % 60;
        return normalized;
    }

    void PublishTime(bool force = false)
    {
        if (!publishTimeEvents) return;

        float hours = SplitHour(HourAt(progress), out int hour24, out int minute);

        if (!force && hour24 == lastPublishedHour && minute == lastPublishedMinute)
            return;

        lastPublishedHour = hour24;
        lastPublishedMinute = minute;
        EventBus.Publish(new DayTimeChanged(hours, hour24, minute));
    }

    void Apply(float t)
    {
        t = Mathf.Clamp01(t);

        var p = ActiveProfile;
        if (p == null) return;

        if (sun != null)
        {
            sun.transform.rotation = Quaternion.Euler(p.sunPitch.Evaluate(t), p.sunYaw, 0f);
            sun.color = p.sunColor.Evaluate(t);
            sun.intensity = p.sunIntensity.Evaluate(t);
        }

        if (controlAmbient)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = p.ambientColor.Evaluate(t);
            RenderSettings.ambientIntensity = p.ambientIntensity.Evaluate(t);
        }

        if (skyboxInstance != null)
        {
            if (skyTintID >= 0) skyboxInstance.SetColor(skyTintID, p.skyTint.Evaluate(t));
            if (skyboxHasExposure) skyboxInstance.SetFloat(ExposureID, p.skyExposure.Evaluate(t));
            if (skyboxIsBlend && p.skyboxBlend != null)
                skyboxInstance.SetFloat(BlendID, p.skyboxBlend.Evaluate(t));
        }

        PublishTime();
    }

    Light FindSun()
    {
        if (RenderSettings.sun != null && RenderSettings.sun.type == LightType.Directional)
            return RenderSettings.sun;

        var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
                return light;
        }

        return null;
    }

    void Reset()
    {
        sun = GetComponent<Light>();
    }
}

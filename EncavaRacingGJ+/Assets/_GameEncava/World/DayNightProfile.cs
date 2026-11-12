using UnityEngine;

/// <summary>
/// Configuración visual del ciclo de día y noche: trayectoria, colores, intensidades,
/// luz ambiental y skybox. Es un asset reutilizable que se asigna a <see cref="DayNightCycle"/>.
/// </summary>
/// <remarks>
/// Crea uno con: Create → Super Bus Racing → Day Night Profile.
/// Duplica el asset si quieres variantes (por ejemplo, día normal y día lluvioso).
/// </remarks>
[CreateAssetMenu(menuName = "Super Bus Racing/Day Night Profile", fileName = "DayNightProfile")]
public class DayNightProfile : ScriptableObject
{
    [Header("Sol: trayectoria")]
    [Tooltip("Ángulo X del sol (grados) a lo largo del día. 0 = horizonte al amanecer; 90 = sol encima; 180 = horizonte al atardecer; menos de 0 o más de 180 = bajo el horizonte.")]
    public AnimationCurve sunPitch = DefaultSunPitch();

    [Tooltip("Giro horizontal del sol (grados). Con 0 el sol sale por -Z y se pone por +Z; con 90 cruza de -X a +X.")]
    public float sunYaw = 0f;

    [Header("Sol: color e intensidad")]
    [Tooltip("Color del sol a lo largo del día (0 = inicio, 1 = fin de la run).")]
    public Gradient sunColor = DefaultSunColor();

    [Tooltip("Intensidad del sol a lo largo del día. En la noche conviene dejarla casi en 0.")]
    public AnimationCurve sunIntensity = DefaultSunIntensity();

    [Header("Luz ambiental")]
    [Tooltip("Color de la luz ambiental. Es lo que mantiene visible la escena cuando el sol ya se ocultó.")]
    public Gradient ambientColor = DefaultAmbientColor();

    [Tooltip("Multiplicador de la luz ambiental a lo largo del día.")]
    public AnimationCurve ambientIntensity = DefaultAmbientIntensity();

    [Header("Cielo")]
    [Tooltip("Color del tinte del skybox a lo largo del día.")]
    public Gradient skyTint = DefaultSkyTint();

    [Tooltip("Exposición del skybox a lo largo del día. Menos exposición = cielo más oscuro.")]
    public AnimationCurve skyExposure = DefaultSkyExposure();

    [Header("Cielo: día y noche")]
    [Tooltip("Cubemap del cielo de día. Si está vacío, se usa Day Skybox Material.")]
    public Cubemap daySkybox;

    [Tooltip("Cubemap del cielo de noche. Si está vacío, se usa Night Skybox Material.")]
    public Cubemap nightSkybox;

    [Tooltip("Material opcional con shader Skybox/Cubemap para el día. Se usa si Day Skybox está vacío.")]
    public Material daySkyboxMaterial;

    [Tooltip("Material opcional con shader Skybox/Cubemap para la noche. Se usa si Night Skybox está vacío.")]
    public Material nightSkyboxMaterial;

    [Tooltip("0 = cielo de día, 1 = cielo de noche. Solo se usa cuando hay un cubemap de día y otro de noche.")]
    public AnimationCurve skyboxBlend = DefaultSkyboxBlend();

    /// <summary>Restablece todos los valores a la configuración recomendada.</summary>
    [ContextMenu("Restablecer valores por defecto")]
    public void ApplyDefaults()
    {
        sunPitch = DefaultSunPitch();
        sunYaw = 0f;
        sunColor = DefaultSunColor();
        sunIntensity = DefaultSunIntensity();
        ambientColor = DefaultAmbientColor();
        ambientIntensity = DefaultAmbientIntensity();
        skyTint = DefaultSkyTint();
        skyExposure = DefaultSkyExposure();
        skyboxBlend = DefaultSkyboxBlend();
    }

    void OnEnable() => EnsureDefaults();

    void OnValidate() => EnsureDefaults();

    void EnsureDefaults()
    {
        if (sunPitch == null || sunPitch.length == 0) sunPitch = DefaultSunPitch();
        if (sunColor == null || sunColor.colorKeys.Length == 0) sunColor = DefaultSunColor();
        if (sunIntensity == null || sunIntensity.length == 0) sunIntensity = DefaultSunIntensity();
        if (ambientColor == null || ambientColor.colorKeys.Length == 0) ambientColor = DefaultAmbientColor();
        if (ambientIntensity == null || ambientIntensity.length == 0) ambientIntensity = DefaultAmbientIntensity();
        if (skyTint == null || skyTint.colorKeys.Length == 0) skyTint = DefaultSkyTint();
        if (skyExposure == null || skyExposure.length == 0) skyExposure = DefaultSkyExposure();
        if (skyboxBlend == null || skyboxBlend.length == 0) skyboxBlend = DefaultSkyboxBlend();
    }

    // ---------------------------------------------------------------------
    // Valores por defecto
    // ---------------------------------------------------------------------
    //
    // El eje X de las curvas es el progreso de la run (0 = inicio, 1 = fin).
    // Para un día venezolano de 5:00 AM a 7:30 PM (14.5 h):
    //
    //   5:00 AM = 0.00000   6:00 AM = 0.06897   7:00 AM = 0.13793
    //  12:15 PM = 0.50000   4:00 PM = 0.75862   5:30 PM = 0.86207
    //   6:30 PM = 0.93103   7:00 PM = 0.96552   7:30 PM = 1.00000
    //
    // El sol está sobre el horizonte entre 0° y 180°; por debajo de 0°
    // o por encima de 180° está bajo el horizonte.

    static AnimationCurve DefaultSunPitch() => new(
        new Keyframe(0.00000f, -12f), // 5:00 AM: sol bajo el horizonte
        new Keyframe(0.06897f, 0f),   // 6:00 AM: amanecer
        new Keyframe(0.13793f, 22f),  // 7:00 AM: sol amarillo bajo
        new Keyframe(0.50000f, 90f),  // 12:15 PM: sol encima
        new Keyframe(0.75862f, 158f), // 4:00 PM: sol bajando
        new Keyframe(0.86207f, 180f), // 5:30 PM: puesta de sol
        new Keyframe(0.93103f, 196f), // 6:30 PM: casi sin luz
        new Keyframe(1.00000f, 212f)); // 7:30 PM: noche

    static AnimationCurve DefaultSunIntensity() => new(
        new Keyframe(0.00000f, 0.00f), // 5:00 AM
        new Keyframe(0.06897f, 0.35f), // 6:00 AM: amanecer
        new Keyframe(0.13793f, 1.00f), // 7:00 AM
        new Keyframe(0.50000f, 2.00f), // 12:15 PM
        new Keyframe(0.75862f, 1.60f), // 4:00 PM
        new Keyframe(0.86207f, 0.55f), // 5:30 PM: crepúsculo
        new Keyframe(0.93103f, 0.06f), // 6:30 PM
        new Keyframe(1.00000f, 0.00f)); // 7:30 PM

    static AnimationCurve DefaultAmbientIntensity() => new(
        new Keyframe(0.00000f, 0.70f), // 5:00 AM: luz de antes del amanecer
        new Keyframe(0.06897f, 0.85f), // 6:00 AM
        new Keyframe(0.13793f, 1.00f), // 7:00 AM
        new Keyframe(0.50000f, 1.00f), // 12:15 PM
        new Keyframe(0.86207f, 0.90f), // 5:30 PM
        new Keyframe(0.93103f, 0.75f), // 6:30 PM
        new Keyframe(1.00000f, 0.65f)); // 7:30 PM

    static AnimationCurve DefaultSkyExposure() => new(
        new Keyframe(0.00000f, 0.55f), // 5:00 AM
        new Keyframe(0.06897f, 0.85f), // 6:00 AM
        new Keyframe(0.13793f, 1.25f), // 7:00 AM
        new Keyframe(0.50000f, 1.30f), // 12:15 PM
        new Keyframe(0.75862f, 1.20f), // 4:00 PM
        new Keyframe(0.86207f, 1.00f), // 5:30 PM
        new Keyframe(0.93103f, 0.65f), // 6:30 PM
        new Keyframe(1.00000f, 0.30f)); // 7:30 PM

    // 0 = cubemap de día, 1 = cubemap de noche.
    static AnimationCurve DefaultSkyboxBlend() => new(
        new Keyframe(0.00000f, 0.00f), // 5:00 AM: cielo de día
        new Keyframe(0.82759f, 0.00f), // 5:00 PM: todavía de día
        new Keyframe(0.86207f, 0.15f), // 5:30 PM: empieza el crepúsculo
        new Keyframe(0.93103f, 0.80f), // 6:30 PM: casi de noche
        new Keyframe(0.96552f, 0.96f), // 7:00 PM: oscurece
        new Keyframe(1.00000f, 1.00f)); // 7:30 PM: cielo de noche

    static Gradient DefaultSunColor() => MakeGradient(
        (0.00000f, new Color(0.25f, 0.32f, 0.55f)), // 5:00 AM: sin sol
        (0.06897f, new Color(1.00f, 0.55f, 0.22f)), // 6:00 AM: naranja del amanecer
        (0.13793f, new Color(1.00f, 0.86f, 0.48f)), // 7:00 AM: amarillo
        (0.50000f, new Color(1.00f, 0.97f, 0.92f)), // 12:15 PM: blanco
        (0.75862f, new Color(1.00f, 0.82f, 0.60f)), // 4:00 PM
        (0.86207f, new Color(1.00f, 0.42f, 0.18f)), // 5:30 PM: atardecer
        (0.93103f, new Color(0.50f, 0.20f, 0.18f)), // 6:30 PM: casi sin luz
        (1.00000f, new Color(0.22f, 0.28f, 0.50f))); // 7:30 PM: noche

    static Gradient DefaultAmbientColor() => MakeGradient(
        (0.00000f, new Color(0.16f, 0.19f, 0.30f)), // 5:00 AM
        (0.06897f, new Color(0.34f, 0.38f, 0.52f)), // 6:00 AM
        (0.13793f, new Color(0.52f, 0.56f, 0.66f)), // 7:00 AM
        (0.50000f, new Color(0.55f, 0.60f, 0.70f)), // 12:15 PM
        (0.75862f, new Color(0.52f, 0.48f, 0.50f)), // 4:00 PM
        (0.86207f, new Color(0.40f, 0.30f, 0.30f)), // 5:30 PM
        (0.93103f, new Color(0.16f, 0.14f, 0.22f)), // 6:30 PM
        (1.00000f, new Color(0.06f, 0.08f, 0.16f))); // 7:30 PM

    static Gradient DefaultSkyTint() => MakeGradient(
        (0.00000f, new Color(0.08f, 0.11f, 0.22f)), // 5:00 AM
        (0.06897f, new Color(0.80f, 0.55f, 0.45f)), // 6:00 AM
        (0.13793f, new Color(0.60f, 0.68f, 0.85f)), // 7:00 AM
        (0.50000f, new Color(0.52f, 0.58f, 0.72f)), // 12:15 PM
        (0.75862f, new Color(0.70f, 0.62f, 0.55f)), // 4:00 PM
        (0.86207f, new Color(0.95f, 0.50f, 0.30f)), // 5:30 PM
        (0.93103f, new Color(0.35f, 0.25f, 0.35f)), // 6:30 PM
        (1.00000f, new Color(0.04f, 0.06f, 0.13f))); // 7:30 PM

    static Gradient MakeGradient(params (float time, Color color)[] keys)
    {
        var colorKeys = new GradientColorKey[keys.Length];
        for (int i = 0; i < keys.Length; i++)
            colorKeys[i] = new GradientColorKey(keys[i].color, keys[i].time);

        var gradient = new Gradient();
        gradient.SetKeys(colorKeys, new[]
        {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
        });
        return gradient;
    }
}

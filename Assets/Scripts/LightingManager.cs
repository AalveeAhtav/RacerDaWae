using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// LightingManager.cs - Juliette Kolto
/// 
/// Controls all scene lighting for the racetrack environment:
///   - Main directional light (sun) with configurable color and intensity
///   - Ambient light (sky, equator, ground) for environment fill
///   - Shadow quality and distance settings
///   - Optional time-of-day angle control for the sun
/// 
/// HOW TO USE:
///   1. Create an empty GameObject in your scene, name it "LightingManager"
///   2. Attach this script to it
///   3. Drag your scene's Directional Light into the "Sun Light" field in the Inspector
///   4. Tweak the exposed settings in the Inspector to match your desired look
/// </summary>
public class LightingManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector-exposed fields
    // -------------------------------------------------------------------------

    [Header("Sun (Directional Light)")]
    [Tooltip("Drag your scene's Directional Light here.")]
    public Light sunLight;

    [Tooltip("Color of the main sunlight. Warm yellow-white works well for daytime.")]
    public Color sunColor = new Color(1f, 0.95f, 0.8f);

    [Tooltip("Brightness of the sun. 1.0 is Unity default.")]
    [Range(0f, 5f)]
    public float sunIntensity = 1.2f;

    [Tooltip("Horizontal rotation of the sun (0-360). Controls where shadows point.")]
    [Range(0f, 360f)]
    public float sunAzimuth = 45f;

    [Tooltip("Vertical angle of the sun. 90 = directly overhead, lower = more dramatic.")]
    [Range(5f, 90f)]
    public float sunElevation = 50f;

    // -------------------------------------------------------------------------

    [Header("Ambient Lighting")]
    [Tooltip("How the scene is lit in shadowed areas. Tricolor gives natural depth.")]
    public AmbientMode ambientMode = AmbientMode.Trilight;

    [Tooltip("Color of the sky (top). Slightly blue-tinted for realism.")]
    public Color ambientSkyColor = new Color(0.5f, 0.65f, 0.85f);

    [Tooltip("Color of the equator (middle). Neutral grey blend.")]
    public Color ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);

    [Tooltip("Color of the ground (bottom). Slightly warm to fake ground bounce.")]
    public Color ambientGroundColor = new Color(0.25f, 0.2f, 0.15f);

    [Tooltip("Overall strength of ambient lighting.")]
    [Range(0f, 2f)]
    public float ambientIntensity = 1.0f;

    // -------------------------------------------------------------------------

    [Header("Shadows")]
    [Tooltip("How far from the camera shadows are rendered. Lower = better performance.")]
    [Range(10f, 500f)]
    public float shadowDistance = 150f;

    [Tooltip("Overall shadow quality setting for the scene.")]
    public ShadowQuality shadowQuality = ShadowQuality.All;

    [Tooltip("Bias to reduce shadow acne (self-shadowing artifacts on surfaces).")]
    [Range(0f, 2f)]
    public float shadowBias = 0.05f;

    [Tooltip("Normal bias to reduce peter-panning (shadows detaching from objects).")]
    [Range(0f, 3f)]
    public float shadowNormalBias = 0.4f;

    // -------------------------------------------------------------------------

    [Header("Fog (Optional Atmosphere)")]
    [Tooltip("Enable scene fog for depth/atmosphere on the racetrack.")]
    public bool enableFog = true;

    [Tooltip("Color of the fog. Should roughly match your horizon/skybox color.")]
    public Color fogColor = new Color(0.7f, 0.75f, 0.8f);

    [Tooltip("Distance at which fog starts appearing.")]
    [Range(0f, 200f)]
    public float fogStartDistance = 80f;

    [Tooltip("Distance at which fog is fully opaque.")]
    [Range(50f, 1000f)]
    public float fogEndDistance = 400f;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        ApplyAllSettings();
    }

    // Lets you tweak values live in the Inspector during Play Mode
    void OnValidate()
    {
        // OnValidate runs in editor too, so guard against missing references
        if (Application.isPlaying)
        {
            ApplyAllSettings();
        }
        else
        {
            // Still apply sun rotation in editor so you can preview the look
            ApplySunRotation();
        }
    }

    // -------------------------------------------------------------------------
    // Core apply methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies all lighting settings at once. Call this after changing any value at runtime.
    /// </summary>
    public void ApplyAllSettings()
    {
        ApplySunSettings();
        ApplyAmbientSettings();
        ApplyShadowSettings();
        ApplyFogSettings();
    }

    void ApplySunSettings()
    {
        if (sunLight == null)
        {
            Debug.LogWarning("[LightingManager] Sun Light is not assigned! Drag your Directional Light into the Inspector.");
            return;
        }

        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;

        ApplySunRotation();
    }

    void ApplySunRotation()
    {
        if (sunLight == null) return;

        // Azimuth = horizontal spin around Y axis
        // Elevation = tilt up from horizon
        // Unity directional lights point along -Z by default, so we rotate accordingly
        sunLight.transform.rotation = Quaternion.Euler(sunElevation, sunAzimuth, 0f);
    }

    void ApplyAmbientSettings()
    {
        RenderSettings.ambientMode = ambientMode;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientIntensity = ambientIntensity;
    }

    void ApplyShadowSettings()
    {
        QualitySettings.shadows = shadowQuality;
        QualitySettings.shadowDistance = shadowDistance;

        if (sunLight != null)
        {
            sunLight.shadowBias = shadowBias;
            sunLight.shadowNormalBias = shadowNormalBias;
        }
    }

    void ApplyFogSettings()
    {
        RenderSettings.fog = enableFog;

        if (enableFog)
        {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
    }

    // -------------------------------------------------------------------------
    // Optional public API for future use (e.g. SkyboxController calling these)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Change the sun's elevation angle at runtime (useful for a day/night cycle).
    /// </summary>
    public void SetSunElevation(float elevation)
    {
        sunElevation = Mathf.Clamp(elevation, 5f, 90f);
        ApplySunRotation();
    }

    /// <summary>
    /// Change sun color and intensity together (useful when SkyboxController switches time of day).
    /// </summary>
    public void SetSunAppearance(Color color, float intensity)
    {
        sunColor = color;
        sunIntensity = intensity;
        ApplySunSettings();
    }
}
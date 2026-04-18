using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// SkyboxController.cs - Juliette Kolto
///
/// Controls skybox and lighting for Day, Sunset, and Night.
/// Auto-animates by cycling through each preset on a timer.
/// </summary>
public class SkyboxController : MonoBehaviour
{
    public enum TimeOfDay { Day, Sunset, Night }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("References")]
    public LightingManager lightingManager;

    [Header("Skybox Materials")]
    public Material daySkyboxMat;
    public Material sunsetSkyboxMat;
    public Material nightSkyboxMat;

    [Header("Time of Day")]
    public TimeOfDay startingTimeOfDay = TimeOfDay.Day;

    [Tooltip("Auto cycle through Day -> Sunset -> Night -> Day on a timer.")]
    public bool autoAnimate = false;

    [Tooltip("How many seconds each time of day lasts before switching to the next.")]
    [Range(3f, 300f)]
    public float secondsPerPhase = 10f;

    [Header("Day Preset")]
    public Color daySunColor = new Color(1f, 0.95f, 0.8f);
    [Range(0f, 5f)] public float daySunIntensity = 1.2f;
    [Range(5f, 90f)] public float daySunElevation = 50f;
    public Color dayFogColor = new Color(0.7f, 0.75f, 0.8f);

    [Header("Sunset Preset")]
    public Color sunsetSunColor = new Color(1f, 0.5f, 0.15f);
    [Range(0f, 5f)] public float sunsetSunIntensity = 0.8f;
    [Range(5f, 90f)] public float sunsetSunElevation = 12f;
    public Color sunsetFogColor = new Color(0.85f, 0.45f, 0.2f);

    [Header("Night Preset")]
    public Color nightSunColor = new Color(0.1f, 0.15f, 0.3f);
    [Range(0f, 5f)] public float nightSunIntensity = 0.1f;
    [Range(5f, 90f)] public float nightSunElevation = 8f;
    public Color nightFogColor = new Color(0.05f, 0.05f, 0.1f);

    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------

    private TimeOfDay currentTimeOfDay;
    private float phaseTimer = 0f;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        currentTimeOfDay = startingTimeOfDay;
        ApplyPreset(currentTimeOfDay);
        phaseTimer = 0f;
    }

    void Update()
    {
        if (!autoAnimate) return;

        phaseTimer += Time.deltaTime;

        if (phaseTimer >= secondsPerPhase)
        {
            phaseTimer = 0f;
            AdvanceToNextPhase();
        }
    }

    // -------------------------------------------------------------------------
    // Phase logic
    // -------------------------------------------------------------------------

    void AdvanceToNextPhase()
    {
        // Day -> Sunset -> Night -> Day -> ...
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Day:    ApplyPreset(TimeOfDay.Sunset); break;
            case TimeOfDay.Sunset: ApplyPreset(TimeOfDay.Night);  break;
            case TimeOfDay.Night:  ApplyPreset(TimeOfDay.Day);    break;
        }
    }

    // -------------------------------------------------------------------------
    // Core apply
    // -------------------------------------------------------------------------

    public void ApplyPreset(TimeOfDay tod)
    {
        currentTimeOfDay = tod;

        // 1. Swap skybox material
        SwapSkybox(tod);

        // 2. Update lighting
        if (lightingManager == null)
        {
            Debug.LogWarning("[SkyboxController] LightingManager not assigned!");
            return;
        }

        switch (tod)
        {
            case TimeOfDay.Day:
                lightingManager.SetSunAppearance(daySunColor, daySunIntensity);
                lightingManager.SetSunElevation(daySunElevation);
                lightingManager.fogColor = dayFogColor;
                break;
            case TimeOfDay.Sunset:
                lightingManager.SetSunAppearance(sunsetSunColor, sunsetSunIntensity);
                lightingManager.SetSunElevation(sunsetSunElevation);
                lightingManager.fogColor = sunsetFogColor;
                break;
            case TimeOfDay.Night:
                lightingManager.SetSunAppearance(nightSunColor, nightSunIntensity);
                lightingManager.SetSunElevation(nightSunElevation);
                lightingManager.fogColor = nightFogColor;
                break;
        }

        lightingManager.ApplyAllSettings();
        DynamicGI.UpdateEnvironment();

        Debug.Log($"[SkyboxController] Switched to: {tod}"); // you'll see this in Console when it switches
    }

    void SwapSkybox(TimeOfDay tod)
    {
        Material mat = null;

        switch (tod)
        {
            case TimeOfDay.Day:    mat = daySkyboxMat;    break;
            case TimeOfDay.Sunset: mat = sunsetSkyboxMat != null ? sunsetSkyboxMat : daySkyboxMat; break;
            case TimeOfDay.Night:  mat = nightSkyboxMat;  break;
        }

        if (mat == null)
        {
            Debug.LogWarning($"[SkyboxController] No material for {tod}!");
            return;
        }

        RenderSettings.skybox = mat;
        DynamicGI.UpdateEnvironment();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void SetDay()    => ApplyPreset(TimeOfDay.Day);
    public void SetSunset() => ApplyPreset(TimeOfDay.Sunset);
    public void SetNight()  => ApplyPreset(TimeOfDay.Night);
}
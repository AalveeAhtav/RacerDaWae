using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// PostProcessingSetup.cs - Juliette Kolto
///
/// Configures post-processing effects for the racetrack scene using Unity URP Volumes.
/// Effects included:
///   - Bloom:          Makes bright areas glow (headlights, sun glare, etc.)
///   - Depth of Field: Blurs objects outside the focus range (cinematic look)
///   - Anti-Aliasing:  Smooths jagged edges on geometry
///   - Color Grading:  Adjusts overall color tone, contrast, and saturation
///   - Vignette:       Darkens screen edges for a cinematic framed look
///
/// HOW TO USE:
///   1. In the Hierarchy, find the existing "Global Volume" object
///      (your scene already has one - check the Hierarchy panel)
///   2. Click it and in the Inspector click "Add Component"
///   3. Search for "PostProcessingSetup" and add it
///   4. Tweak settings in the Inspector to your liking
///   5. Hit Play to see the effects
///
/// NOTE: Your scene already has a Global Volume. Use that instead of making a new one!
/// </summary>
[RequireComponent(typeof(Volume))]
public class PostProcessingSetup : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Bloom")]
    [Tooltip("Makes bright parts of the scene glow. Great for headlights and sun.")]
    public bool enableBloom = true;

    [Tooltip("How bright a pixel needs to be before it starts glowing.")]
    [Range(0f, 2f)]
    public float bloomThreshold = 0.9f;

    [Tooltip("How intense the bloom glow is overall.")]
    [Range(0f, 1f)]
    public float bloomIntensity = 0.3f;

    [Tooltip("How far the bloom spreads out from bright areas.")]
    [Range(0f, 1f)]
    public float bloomScatter = 0.7f;

    // -------------------------------------------------------------------------

    [Header("Depth of Field")]
    [Tooltip("Blurs objects that are too close or too far from the focus point.")]
    public bool enableDepthOfField = true;

    [Tooltip("Distance from the camera where everything is perfectly sharp.")]
    [Range(1f, 50f)]
    public float focusDistance = 10f;

    [Tooltip("How wide the in-focus zone is. Higher = more things in focus.")]
    [Range(1f, 20f)]
    public float focalLength = 7f;

    [Tooltip("Controls the shape of the out-of-focus blur (bokeh). Higher = more blur.")]
    [Range(1f, 32f)]
    public float aperture = 5.6f;

    // -------------------------------------------------------------------------

    [Header("Color Grading")]
    [Tooltip("Overall brightness adjustment.")]
    [Range(-1f, 1f)]
    public float colorGradingExposure = 0.1f;

    [Tooltip("Difference between dark and light areas.")]
    [Range(-1f, 1f)]
    public float contrast = 0.1f;

    [Tooltip("Color vibrancy. Higher = more vivid colors.")]
    [Range(-1f, 1f)]
    public float saturation = 0.1f;

    // -------------------------------------------------------------------------

    [Header("Vignette")]
    [Tooltip("Darkens the edges of the screen for a cinematic look.")]
    public bool enableVignette = true;

    [Tooltip("Color of the vignette. Usually black.")]
    public Color vignetteColor = Color.black;

    [Tooltip("How far the vignette reaches toward the center.")]
    [Range(0f, 1f)]
    public float vignetteIntensity = 0.25f;

    [Tooltip("How sharp the vignette edge is.")]
    [Range(0f, 1f)]
    public float vignetteSmoothness = 0.4f;

    // -------------------------------------------------------------------------
    // Internal references
    // -------------------------------------------------------------------------

    private Volume volume;
    private Bloom bloom;
    private DepthOfField dof;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        volume = GetComponent<Volume>();

        // Make sure the volume profile exists
        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        SetupAllEffects();
    }

    // Live tweaking in Inspector during Play Mode
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        if (volume == null) return;
        ApplyAllSettings();
    }

    // -------------------------------------------------------------------------
    // Setup - adds each effect to the volume profile if not already there
    // -------------------------------------------------------------------------

    void SetupAllEffects()
    {
        SetupBloom();
        SetupDepthOfField();
        SetupColorGrading();
        SetupVignette();
        SetupAntiAliasing();
    }

    void SetupBloom()
    {
        if (!volume.profile.TryGet(out bloom))
            bloom = volume.profile.Add<Bloom>(false);

        bloom.active = enableBloom;
        bloom.threshold.Override(bloomThreshold);
        bloom.intensity.Override(bloomIntensity);
        bloom.scatter.Override(bloomScatter);
    }

    void SetupDepthOfField()
    {
        if (!volume.profile.TryGet(out dof))
            dof = volume.profile.Add<DepthOfField>(false);

        dof.active = enableDepthOfField;
        dof.mode.Override(DepthOfFieldMode.Bokeh);
        dof.focusDistance.Override(focusDistance);
        dof.focalLength.Override(focalLength);
        dof.aperture.Override(aperture);
    }

    void SetupColorGrading()
    {
        if (!volume.profile.TryGet(out colorAdjustments))
            colorAdjustments = volume.profile.Add<ColorAdjustments>(false);

        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(colorGradingExposure);
        colorAdjustments.contrast.Override(contrast * 100f);     // Unity uses -100 to 100
        colorAdjustments.saturation.Override(saturation * 100f); // Unity uses -100 to 100
    }

    void SetupVignette()
    {
        if (!volume.profile.TryGet(out vignette))
            vignette = volume.profile.Add<Vignette>(false);

        vignette.active = enableVignette;
        vignette.color.Override(vignetteColor);
        vignette.intensity.Override(vignetteIntensity);
        vignette.smoothness.Override(vignetteSmoothness);
    }

    void SetupAntiAliasing()
    {
        // Anti-aliasing in URP is set on the Camera, not the Volume
        // Find the main camera and set SMAA (best quality AA for URP)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[PostProcessingSetup] No Main Camera found for anti-aliasing setup.");
            return;
        }

        UniversalAdditionalCameraData cameraData = mainCam.GetUniversalAdditionalCameraData();
        if (cameraData != null)
        {
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
        }
    }

    // -------------------------------------------------------------------------
    // Apply settings at runtime (called by OnValidate during play)
    // -------------------------------------------------------------------------

    void ApplyAllSettings()
    {
        if (bloom != null)
        {
            bloom.active = enableBloom;
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(bloomScatter);
        }

        if (dof != null)
        {
            dof.active = enableDepthOfField;
            dof.focusDistance.Override(focusDistance);
            dof.focalLength.Override(focalLength);
            dof.aperture.Override(aperture);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(colorGradingExposure);
            colorAdjustments.contrast.Override(contrast * 100f);
            colorAdjustments.saturation.Override(saturation * 100f);
        }

        if (vignette != null)
        {
            vignette.active = enableVignette;
            vignette.color.Override(vignetteColor);
            vignette.intensity.Override(vignetteIntensity);
            vignette.smoothness.Override(vignetteSmoothness);
        }
    }

    // -------------------------------------------------------------------------
    // Public API - can be called by SkyboxController to shift post processing
    // per time of day (e.g. more bloom at sunset)
    // -------------------------------------------------------------------------

    public void SetBloomIntensity(float intensity)
    {
        bloomIntensity = intensity;
        if (bloom != null) bloom.intensity.Override(bloomIntensity);
    }

    public void SetFocusDistance(float distance)
    {
        focusDistance = distance;
        if (dof != null) dof.focusDistance.Override(focusDistance);
    }
}
using UnityEngine;

/// <summary>
/// CarColorApplier.cs - Juliette Kolto
///
/// Reads the color the player picked on the selection screen
/// and applies it to the car when the race scene loads.
///
/// HOW TO USE:
///   1. Drop this script into Assets/Scripts
///   2. Create an empty GameObject in MainScene, name it "CarColorApplier"
///   3. Attach this script to it
///   4. Drag the car's Body mesh renderer into the "Car Body Renderer" field
///   5. Drag all color variation materials into the "Color Materials" array
///      (same order as in CarColorSelector!)
/// </summary>
public class CarColorApplier : MonoBehaviour
{
    [Header("Car References")]
    [Tooltip("Drag the Body object (child of Free Racing Car Red Variant) here")]
    public Renderer carBodyRenderer;

    [Tooltip("Same color materials array as in CarColorSelector, same order")]
    public Material[] colorMaterials;

    void Start()
    {
        ApplySelectedColor();
    }

    void ApplySelectedColor()
    {
        if (carBodyRenderer == null)
        {
            Debug.LogWarning("[CarColorApplier] Car Body Renderer not assigned!");
            return;
        }

        if (colorMaterials == null || colorMaterials.Length == 0)
        {
            Debug.LogWarning("[CarColorApplier] No color materials assigned!");
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt(CarColorSelector.COLOR_PREF_KEY, 0);
        selectedIndex = Mathf.Clamp(selectedIndex, 0, colorMaterials.Length - 1);

        // Swap Element 0 (the body material) with the selected color
        Material[] mats = carBodyRenderer.materials;
        mats[0] = colorMaterials[selectedIndex];
        carBodyRenderer.materials = mats;

        Debug.Log($"[CarColorApplier] Applied color index {selectedIndex}");
    }
}
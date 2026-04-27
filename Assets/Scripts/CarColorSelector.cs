using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// CarColorSelector.cs - Juliette Kolto
///
/// Shows a pre-race screen where the player picks a car color,
/// then hits Race! to start. Uses the existing color variation
/// materials already in the ARCADE asset pack.
///
/// HOW TO USE:
///   1. Create a new Scene called "ColorSelect" in Assets/Scenes
///   2. Create a Canvas in that scene (GameObject -> UI -> Canvas)
///   3. Create an empty GameObject, name it "CarColorSelector", attach this script
///   4. Set up the Inspector fields (see below)
///   5. In Build Settings (File -> Build Settings) add both scenes:
///      - ColorSelect scene index 0
///      - MainScene index 1
///
/// CANVAS SETUP:
///   - Add a Panel for the background
///   - Add a Title Text that says "Choose Your Car"
///   - Add color buttons (one per color variant)
///   - Add a "Race!" button
///   - Add a preview Image to show the selected color
/// </summary>
public class CarColorSelector : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Car Color Materials")]
    [Tooltip("Drag the color variation materials here from Assets > ARCADE - FREE Racing Car > Materials > Color Variations")]
    public Material[] colorMaterials;

    [Tooltip("Names to show on the buttons for each color (must match colorMaterials order)")]
    public string[] colorNames;

    [Header("UI References")]
    [Tooltip("The buttons the player clicks to pick a color (one per color)")]
    public Button[] colorButtons;

    [Tooltip("The Race! button to start the game")]
    public Button raceButton;

    [Tooltip("Image that shows a preview swatch of the selected color")]
    public Image colorPreviewImage;

    [Tooltip("Text showing which color is currently selected")]
    public Text selectedColorText;

    [Header("Scene")]
    [Tooltip("Name of your main race scene - must match exactly")]
    public string raceSceneName = "MainScene";

    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------

    private int selectedColorIndex = 0;

    // This is how we pass the selected material to the race scene
    // We store the index in PlayerPrefs so it survives scene loading
    public static readonly string COLOR_PREF_KEY = "SelectedCarColorIndex";

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        // Default to first color
        selectedColorIndex = PlayerPrefs.GetInt(COLOR_PREF_KEY, 0);

        SetupColorButtons();
        SetupRaceButton();
        UpdatePreview();
    }

    // -------------------------------------------------------------------------
    // Setup
    // -------------------------------------------------------------------------

    void SetupColorButtons()
    {
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i] == null) continue;

            int index = i; // Capture for lambda
            colorButtons[i].onClick.AddListener(() => SelectColor(index));

            // Tint the button itself to match the color
            if (i < colorMaterials.Length && colorMaterials[i] != null)
            {
                ColorBlock cb = colorButtons[i].colors;
                cb.normalColor = colorMaterials[i].color;
                cb.selectedColor = colorMaterials[i].color;
                colorButtons[i].colors = cb;
            }
        }
    }

    void SetupRaceButton()
    {
        if (raceButton != null)
            raceButton.onClick.AddListener(StartRace);
    }

    // -------------------------------------------------------------------------
    // Color selection
    // -------------------------------------------------------------------------

    public void SelectColor(int index)
    {
        if (index < 0 || index >= colorMaterials.Length) return;

        selectedColorIndex = index;
        PlayerPrefs.SetInt(COLOR_PREF_KEY, selectedColorIndex);
        PlayerPrefs.Save();

        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (colorMaterials == null || selectedColorIndex >= colorMaterials.Length) return;

        Material selected = colorMaterials[selectedColorIndex];

        // Update preview swatch color
        if (colorPreviewImage != null && selected != null)
            colorPreviewImage.color = selected.color;

        // Update selected color label
        if (selectedColorText != null)
        {
            string name = (colorNames != null && selectedColorIndex < colorNames.Length)
                ? colorNames[selectedColorIndex]
                : $"Color {selectedColorIndex + 1}";
            selectedColorText.text = $"Selected: {name}";
        }
    }

    // -------------------------------------------------------------------------
    // Start race
    // -------------------------------------------------------------------------

    void StartRace()
    {
        SceneManager.LoadScene(raceSceneName);
    }
}
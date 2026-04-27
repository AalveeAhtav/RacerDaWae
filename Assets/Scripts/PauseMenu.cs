using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// PauseMenu.cs - Juliette Kolto
///
/// Handles pausing the game with ESC and showing a pause menu
/// with Resume and Restart buttons.
///
/// HOW TO USE:
///   1. Drop this script into Assets/Scripts
///   2. In MainScene, create a UI Canvas (GameObject -> UI -> Canvas)
///      Name it "PauseCanvas"
///   3. Inside the Canvas add a Panel (this is the pause menu background)
///   4. Inside the Panel add:
///      - A Text that says "PAUSED"
///      - A Button named "ResumeButton" with text "Resume"
///      - A Button named "RestartButton" with text "Restart"
///   5. Create an empty GameObject, name it "PauseMenu", attach this script
///   6. Drag the references into the Inspector fields below
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("UI References")]
    [Tooltip("The whole pause menu panel — we show/hide this")]
    public GameObject pauseMenuPanel;

    [Tooltip("Resume button")]
    public Button resumeButton;

    [Tooltip("Restart button")]
    public Button restartButton;

    [Header("Settings")]
    [Tooltip("Name of the color select scene to go back to on restart")]
    public string colorSelectSceneName = "ColorSelect";

    [Tooltip("Name of the main race scene to reload on restart")]
    public string raceSceneName = "MainScene";

    [Tooltip("If true, restart goes back to color select screen. If false, just reloads the race.")]
    public bool restartGoesToColorSelect = true;

    // -------------------------------------------------------------------------
    // Internal state
    // -------------------------------------------------------------------------

    private bool isPaused = false;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        // Make sure the pause menu starts hidden
        SetPauseMenuVisible(false);

        // Hook up buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
    }

    void Update()
    {
        // Toggle pause on ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // -------------------------------------------------------------------------
    // Pause / Resume
    // -------------------------------------------------------------------------

    void Pause()
    {
        isPaused = true;
        SetPauseMenuVisible(true);

        // Freeze the game
        Time.timeScale = 0f;

        // Unlock the cursor so the player can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        SetPauseMenuVisible(false);

        // Unfreeze the game
        Time.timeScale = 1f;

        // Re-lock cursor for driving
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Restart()
    {
        // Always unfreeze time before loading a new scene!
        Time.timeScale = 1f;

        if (restartGoesToColorSelect)
            SceneManager.LoadScene(colorSelectSceneName);
        else
            SceneManager.LoadScene(raceSceneName);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    void SetPauseMenuVisible(bool visible)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(visible);
    }

    public bool IsPaused => isPaused;
}
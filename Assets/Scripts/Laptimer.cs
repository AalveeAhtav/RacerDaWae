using UnityEngine;
using TMPro;

/// <summary>
/// LapTimer.cs - Juliette Kolto
///
/// Tracks lap times using distance detection instead of triggers.
/// Attach this to any GameObject in the scene (e.g. a LapTimer empty object).
/// Drag the car and the Arch into the Inspector fields.
/// </summary>
public class LapTimer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the car GameObject (Free Racing Car Red Variant) here")]
    public Transform car;

    [Tooltip("Drag the Arch GameObject here")]
    public Transform finishLine;

    [Header("Detection")]
    [Tooltip("How close the car needs to be to the finish line to trigger a lap (in Unity units)")]
    [Range(1f, 20f)]
    public float detectionRadius = 8f;

    [Header("UI References")]
    public TMP_Text currentLapText;
    public TMP_Text bestLapText;
    public TMP_Text lapCompleteText;

    [Header("Settings")]
    [Range(1f, 5f)]
    public float lapCompleteDisplayTime = 2f;

    // -------------------------------------------------------------------------
    private float currentLapTime = 0f;
    private float bestLapTime = float.MaxValue;
    private bool timerRunning = false;
    private bool carNearFinish = false; // true when car is in the zone
    private bool firstCrossing = true;
    private float lapCompleteTimer = 0f;
    private bool showingLapComplete = false;

    // -------------------------------------------------------------------------

    void Start()
    {
        UpdateUI();
        if (lapCompleteText != null)
            lapCompleteText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (car == null || finishLine == null) return;

        // Check distance between car and finish line
        float distance = Vector3.Distance(car.position, finishLine.position);
        Debug.Log($"[LapTimer] Distance to finish: {distance:F1}");

        bool isNear = distance < detectionRadius;

        // Car just entered the finish zone
        if (isNear && !carNearFinish)
        {
            carNearFinish = true;
            OnFinishLineCrossed();
        }

        // Car left the finish zone
        if (!isNear && carNearFinish)
        {
            carNearFinish = false;
        }

        // Update timer
        if (timerRunning)
        {
            currentLapTime += Time.deltaTime;
            UpdateCurrentLapUI();
        }

        // Hide lap complete message after a few seconds
        if (showingLapComplete)
        {
            lapCompleteTimer += Time.deltaTime;
            if (lapCompleteTimer >= lapCompleteDisplayTime)
            {
                showingLapComplete = false;
                if (lapCompleteText != null)
                    lapCompleteText.gameObject.SetActive(false);
            }
        }
    }

    void OnFinishLineCrossed()
    {
        if (firstCrossing)
        {
            firstCrossing = false;
            timerRunning = true;
            currentLapTime = 0f;
            Debug.Log("[LapTimer] Timer started!");
            return;
        }

        CompleteLap();
    }

    void CompleteLap()
    {
        timerRunning = false;

        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            UpdateBestLapUI();
        }

        Debug.Log($"[LapTimer] Lap complete! Time: {FormatTime(currentLapTime)}");
        ShowLapComplete();

        currentLapTime = 0f;
        timerRunning = true;
    }

    void ShowLapComplete()
    {
        if (lapCompleteText == null) return;
        lapCompleteText.gameObject.SetActive(true);
        lapCompleteText.text = $"LAP COMPLETE!\n{FormatTime(currentLapTime)}";
        showingLapComplete = true;
        lapCompleteTimer = 0f;
    }

    void UpdateUI()
    {
        UpdateCurrentLapUI();
        UpdateBestLapUI();
    }

    void UpdateCurrentLapUI()
    {
        if (currentLapText != null)
            currentLapText.text = $"Lap: {FormatTime(currentLapTime)}";
    }

    void UpdateBestLapUI()
    {
        if (bestLapText != null)
        {
            string best = bestLapTime == float.MaxValue ? "--:--.--" : FormatTime(bestLapTime);
            bestLapText.text = $"Best: {best}";
        }
    }

    string FormatTime(float time)
    {
        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        int milliseconds = (int)((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}
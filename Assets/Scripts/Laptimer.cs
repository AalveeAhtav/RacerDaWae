using UnityEngine;
using TMPro;

/// <summary>
/// LapTimer.cs - Juliette Kolto
/// Tracks lap times using distance detection from car to finish line.
/// </summary>
public class LapTimer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag Free Racing Car Red Variant here")]
    public Transform car;

    [Tooltip("Drag the Arch GameObject here")]
    public Transform finishLine;

    [Header("Detection")]
    [Tooltip("How close the car needs to be to trigger a lap crossing")]
    [Range(1f, 30f)]
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
    private bool carInZone = false;
    private bool firstCrossing = true;
    private float lapCompleteTimer = 0f;
    private bool showingLapComplete = false;

    void Start()
    {
        UpdateUI();
        if (lapCompleteText != null)
            lapCompleteText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (car == null || finishLine == null) return;

        float distance = Vector3.Distance(car.position, finishLine.position);

        bool isNear = distance < detectionRadius;

        if (isNear && !carInZone)
        {
            carInZone = true;
            OnFinishLineCrossed();
        }

        if (!isNear && carInZone)
        {
            carInZone = false;
        }

        if (timerRunning)
        {
            currentLapTime += Time.deltaTime;
            UpdateCurrentLapUI();
        }

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
    void UpdateUI()
{
    UpdateCurrentLapUI();
    UpdateBestLapUI();
}
}
using System;
using UnityEngine;

public class GlobalBombTimer : MonoBehaviour
{
    private float startingTimeSeconds;
    private float remainingTimeSeconds;
    private bool isRunning;
    private bool isPaused;
    private bool hasExpired;
    private bool expirationInvoked;
    private bool isPermanentlyStopped;
    private bool isInitialized;

    public float StartingTimeSeconds => startingTimeSeconds;
    public float RemainingTimeSeconds => remainingTimeSeconds;
    public float NormalizedRemaining =>
        startingTimeSeconds > 0f
            ? Mathf.Clamp01(remainingTimeSeconds / startingTimeSeconds)
            : 0f;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public bool HasExpired => hasExpired;

    public event Action<float> RemainingTimeChanged;
    public event Action TimerExpired;

    private void Update()
    {
        if (!isRunning || isPaused || hasExpired)
        {
            return;
        }

        float previousTime = remainingTimeSeconds;
        remainingTimeSeconds = Mathf.Max(
            0f,
            remainingTimeSeconds - Time.unscaledDeltaTime);

        if (remainingTimeSeconds <= 0f)
        {
            CompleteExpiration();
            return;
        }

        if (!Mathf.Approximately(previousTime, remainingTimeSeconds))
        {
            RemainingTimeChanged?.Invoke(remainingTimeSeconds);
        }
    }

    public void Initialize(float startingDurationSeconds)
    {
        if (!IsFinite(startingDurationSeconds) ||
            startingDurationSeconds < 0f)
        {
            isInitialized = false;
            isRunning = false;
            isPaused = false;
            Debug.LogError(
                "GlobalBombTimer requires a non-negative, finite " +
                "starting duration.",
                this);
            return;
        }

        startingTimeSeconds = startingDurationSeconds;
        remainingTimeSeconds = startingDurationSeconds;
        isRunning = false;
        isPaused = false;
        hasExpired = false;
        expirationInvoked = false;
        isPermanentlyStopped = false;
        isInitialized = true;
        RemainingTimeChanged?.Invoke(remainingTimeSeconds);
    }

    public void StartTimer()
    {
        if (!isInitialized)
        {
            Debug.LogError(
                "GlobalBombTimer cannot start before valid initialization.",
                this);
            return;
        }

        if (isPermanentlyStopped || hasExpired)
        {
            return;
        }

        isPaused = false;

        if (remainingTimeSeconds <= 0f)
        {
            CompleteExpiration();
            return;
        }

        isRunning = true;
    }

    public void PauseTimer()
    {
        if (!isRunning || hasExpired || isPermanentlyStopped)
        {
            return;
        }

        isPaused = true;
        isRunning = false;
    }

    public void ResumeTimer()
    {
        if (!isInitialized ||
            !isPaused ||
            hasExpired ||
            isPermanentlyStopped ||
            remainingTimeSeconds <= 0f)
        {
            return;
        }

        isPaused = false;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        isPermanentlyStopped = true;
    }

    public void ResetTimer()
    {
        if (!isInitialized)
        {
            Debug.LogError(
                "GlobalBombTimer cannot reset before valid initialization.",
                this);
            return;
        }

        remainingTimeSeconds = startingTimeSeconds;
        isRunning = false;
        isPaused = false;
        hasExpired = false;
        expirationInvoked = false;
        isPermanentlyStopped = false;
        RemainingTimeChanged?.Invoke(remainingTimeSeconds);
    }

    private void CompleteExpiration()
    {
        remainingTimeSeconds = 0f;
        isRunning = false;
        isPaused = false;
        hasExpired = true;
        isPermanentlyStopped = true;
        RemainingTimeChanged?.Invoke(remainingTimeSeconds);

        if (expirationInvoked)
        {
            return;
        }

        expirationInvoked = true;
        TimerExpired?.Invoke();
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

using UnityEngine;

/// <summary>
/// Pauses the game (physics + audio) when the browser tab/window loses focus or is
/// backgrounded, so balls don't keep falling and timers don't keep ticking while
/// nobody is looking - important for a physics-driven game running in a WebGL tab
/// that can sit in the background for an arbitrary amount of time.
///
/// Add one instance of this to each gameplay scene (or make it a persistent
/// DontDestroyOnLoad object alongside YandexGamesBridge in the menu scene).
/// </summary>
public class ApplicationPauseHandler : MonoBehaviour
{
    private float _previousTimeScale = 1f;
    private bool _isPaused;

    private void OnApplicationPause(bool pauseStatus)
    {
        SetPaused(pauseStatus);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetPaused(!hasFocus);
    }

    private void SetPaused(bool pause)
    {
        if (pause == _isPaused)
        {
            return;
        }

        _isPaused = pause;

        if (pause)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = _previousTimeScale;
        }

        AudioListener.pause = pause;
    }
}

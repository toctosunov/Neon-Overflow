using UnityEngine;

/// <summary>
/// In-game pause menu: a Pause button opens a panel (freezing gameplay via
/// Time.timeScale = 0 and muting audio) with three options - Resume, Restart,
/// and Main Menu.
///
/// Time.timeScale is global and survives a scene load, so RestartFromPause()
/// and GoMenuFromPause() both reset it to 1 before handing off to
/// UiController.Restart()/GoMenu() - otherwise the next scene would load
/// already frozen.
///
/// This only touches its own _pausePanel and Time.timeScale/AudioListener.pause;
/// it doesn't know about ApplicationPauseHandler (which does the same two
/// things on tab focus loss) and doesn't need to - whichever of the two paused
/// last "wins" until something unpauses, and both restore the same way, so they
/// never fight each other.
///
/// Wire a Pause button's OnClick to TogglePause(), and the pause panel's three
/// buttons to Resume(), RestartFromPause(), and GoMenuFromPause().
/// </summary>
public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;

    private bool _isPaused;

    private void Start()
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    public void TogglePause()
    {
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        // Don't let the pause button pop its panel on top of Game Over or the
        // revive offer - both already stop the player from doing anything else.
        if (IsOtherPanelShowing())
        {
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }
    }

    public void Resume()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    public void RestartFromPause()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UiController.Instance.Restart();
    }

    public void GoMenuFromPause()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UiController.Instance.GoMenu();
    }

    private static bool IsOtherPanelShowing()
    {
        UiController ui = UiController.Instance;
        if (ui == null)
        {
            return false;
        }

        bool loseShowing = ui.losePanel != null && ui.losePanel.activeSelf;
        bool reviveShowing = ui.revivePanel != null && ui.revivePanel.activeSelf;
        return loseShowing || reviveShowing;
    }
}

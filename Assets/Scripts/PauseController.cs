using UnityEngine;

/// <summary>
/// In-game pause menu: a Pause button opens a panel with three options -
/// Resume, Restart, and Main Menu.
///
/// Deliberately does NOT use Time.timeScale (that would also freeze every
/// animation, VFX and DOTween tween in the scene, which looked like
/// everything "turning off" while paused). Instead it freezes only actual
/// gameplay:
/// - BallSpawn is disabled, so no new ball can be dropped.
/// - BlitzTimer (if this scene has one) is disabled, so the countdown
///   doesn't keep burning away while paused.
/// - Every ball's Rigidbody2D has `simulated` set to false, which excludes it
///   from physics entirely - no gravity, no collisions, it just hangs in the
///   air exactly where it was - while leaving everything else (its sprite,
///   any running animation/VFX, audio) untouched. Setting `simulated` back to
///   true on resume continues the simulation from the ball's last velocity,
///   so it falls again exactly as if nothing happened.
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
        SetGameplayFrozen(true);

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
        }
    }

    public void Resume()
    {
        _isPaused = false;
        SetGameplayFrozen(false);

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(false);
        }
    }

    public void RestartFromPause()
    {
        // Restart/GoMenu load a different scene, so there's nothing left here
        // to un-freeze - just hand off.
        UiController.Instance.Restart();
    }

    public void GoMenuFromPause()
    {
        UiController.Instance.GoMenu();
    }

    private static void SetGameplayFrozen(bool frozen)
    {
        BallSpawn spawner = FindObjectOfType<BallSpawn>();
        if (spawner != null)
        {
            spawner.enabled = !frozen;
        }

        // Only present in Blitz-style scenes; harmless no-op everywhere else.
        BlitzTimer timer = FindObjectOfType<BlitzTimer>();
        if (timer != null)
        {
            timer.enabled = !frozen;
        }

        Rigidbody2D[] bodies = FindObjectsOfType<Rigidbody2D>();
        foreach (Rigidbody2D body in bodies)
        {
            body.simulated = !frozen;
        }
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

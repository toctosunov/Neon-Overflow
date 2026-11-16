using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the "Gravity" game mode: periodically rotates the global 2D gravity
/// vector between down/up/left/right, so every ball on the field - whether it
/// was spawned before or after the flip - reacts to the new direction with no
/// per-ball bookkeeping needed.
///
/// A ball the player is actively aiming is unaffected by a flip: BallSpawn.Hold()
/// drives it with Rigidbody2D.MovePosition() every FixedUpdate, which overrides
/// physics entirely while held, so gravity direction doesn't matter until the
/// player actually releases it.
///
/// Add this component to one GameObject in the Gravity-mode scene only (it's
/// not referenced by any other script, so it's opt-in per scene). It resets
/// Physics2D.gravity back to its original value in OnDisable, so leaving the
/// scene never leaves the next scene's gravity flipped.
/// </summary>
public class GravityFlipController : MonoBehaviour
{
    [Tooltip("Seconds between each gravity flip.")]
    [SerializeField] private float _flipInterval = 6f;

    [Tooltip("Magnitude of gravity after a flip (sign/direction comes from the chosen axis).")]
    [SerializeField] private float _gravityMagnitude = 9.81f;

    [Tooltip("Optional. Spawned at the center of the field right as a flip happens, as a heads-up cue.")]
    [SerializeField] private GameObject _flipWarningVfx;

    private static readonly Vector2[] Directions =
    {
        Vector2.down, Vector2.up, Vector2.left, Vector2.right,
    };

    private Vector2 _originalGravity;
    private int _lastDirectionIndex = -1;
    private Coroutine _flipLoop;

    private void OnEnable()
    {
        _originalGravity = Physics2D.gravity;
        _flipLoop = StartCoroutine(FlipLoop());
    }

    private void OnDisable()
    {
        if (_flipLoop != null)
        {
            StopCoroutine(_flipLoop);
        }
        Physics2D.gravity = _originalGravity;
    }

    private IEnumerator FlipLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_flipInterval);
            ApplyNextDirection();
        }
    }

    private void ApplyNextDirection()
    {
        int nextIndex;
        do
        {
            nextIndex = Random.Range(0, Directions.Length);
        }
        while (nextIndex == _lastDirectionIndex && Directions.Length > 1);

        _lastDirectionIndex = nextIndex;
        Physics2D.gravity = Directions[nextIndex] * _gravityMagnitude;

        if (_flipWarningVfx != null)
        {
            Instantiate(_flipWarningVfx, Vector3.zero, Quaternion.identity);
        }
    }
}

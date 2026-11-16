using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the "Gravity" game mode. Originally this hard-flipped Physics2D.gravity
/// between down/up/left/right at full strength - that turned out to feel awful in
/// practice: an instant full-strength reversal slams straight into whatever
/// velocity a ball already had, so balls rocketed off the field within about a
/// second of a flip. Instead this now does a gentle "weightless pulse": gravity
/// eases from normal down to a soft upward pull (balls drift up for a bit), then
/// eases back to normal (balls fall back down on their own, no second impulse
/// needed) - matching the "they float up a little and fall back down" feel this
/// mode is meant to have.
///
/// A ball the player is actively aiming is unaffected: BallSpawn.Hold() drives it
/// with Rigidbody2D.MovePosition() every FixedUpdate, which overrides physics
/// entirely while held, so a pulse never yanks the ball currently being aimed.
///
/// Add this component to one GameObject in the Gravity-mode scene only (it's not
/// referenced by any other script, so it's opt-in per scene). It resets
/// Physics2D.gravity back to its original value in OnDisable, so leaving the
/// scene never leaves the next scene's gravity altered.
/// </summary>
public class GravityFlipController : MonoBehaviour
{
    [Tooltip("Seconds of normal gravity between the start of one pulse and the next.")]
    [SerializeField] private float _intervalBetweenPulses = 6f;

    [Tooltip("How long balls float upward before gravity starts easing back to normal.")]
    [SerializeField] private float _pulseHoldDuration = 1f;

    [Tooltip("How long the ease from normal gravity into the upward pull takes, and the same for easing back out. Keeping this non-instant is what avoids the old 'balls rocket away' problem.")]
    [SerializeField] private float _easeDuration = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Strength of the upward pull during a pulse, as a fraction of normal gravity. 0.35 is a gentle float; 1.0 would fully cancel and reverse normal gravity.")]
    [SerializeField] private float _pulseStrength = 0.35f;

    [Tooltip("Optional. Spawned at the center of the field right as a pulse begins, as a heads-up cue.")]
    [SerializeField] private GameObject _flipWarningVfx;

    private Vector2 _normalGravity;
    private Coroutine _loop;

    private void OnEnable()
    {
        _normalGravity = Physics2D.gravity;
        _loop = StartCoroutine(PulseLoop());
    }

    private void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
        }
        Physics2D.gravity = _normalGravity;
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_intervalBetweenPulses);

            if (_flipWarningVfx != null)
            {
                Instantiate(_flipWarningVfx, Vector3.zero, Quaternion.identity);
            }

            // Flips only the vertical component, so balls drift gently upward
            // instead of being launched sideways into the field's walls.
            Vector2 pulseGravity = new Vector2(_normalGravity.x, -_normalGravity.y) * _pulseStrength;

            yield return EaseGravity(_normalGravity, pulseGravity, _easeDuration);
            yield return new WaitForSeconds(_pulseHoldDuration);
            yield return EaseGravity(pulseGravity, _normalGravity, _easeDuration);
        }
    }

    private IEnumerator EaseGravity(Vector2 from, Vector2 to, float duration)
    {
        if (duration <= 0f)
        {
            Physics2D.gravity = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Physics2D.gravity = Vector2.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        Physics2D.gravity = to;
    }
}

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    public static event Action OnDestroyed;

    public int ballId;
    public bool _isMerged;

    [SerializeField] private Vector3 scaleInterval;

    [SerializeField] private GameObject _vfx;
    [SerializeField] private Collider2D _collider;
    private AudioSource _audioSource;

    private Rigidbody2D _rigidbody;

    // Pool of pre-made, real PhysicsMaterial2D ASSETS (not created at runtime) used
    // only by Chaos mode, loaded once and shared across the whole session.
    //
    // History: Chaos mode used to mutate the one shared material asset that every
    // ball prefab points to, which meant every ball on screen kept jumping to
    // whichever value the LATEST spawned ball rolled (a real leak). The fix for
    // that was to give each ball its own *runtime-created* PhysicsMaterial2D
    // instance (via `new` and later `Instantiate`) - but that broke restitution
    // between two balls entirely (confirmed by testing: ball-vs-floor bounced
    // fine, ball-vs-ball didn't bounce at all, even though Inspector values looked
    // correct). Floor and other static geometry have no material assigned at all,
    // so that path never actually exercises Unity's dual-material combine logic;
    // ball-vs-ball does, and that combine apparently doesn't work right for
    // non-asset, runtime-only PhysicsMaterial2D objects on this Unity version.
    //
    // Using a handful of real, asset-file-backed materials (Assets/Resources/
    // ChaosBallMaterials/*.physicsMaterial2D) sidesteps that entirely: every ball
    // in Chaos still gets one of several different bounciness values (so the
    // original leak is fixed - a ball's value no longer overwrites every other
    // ball's), but each value lives on a real, persistently-loaded asset exactly
    // like the game's original single material did, which is confirmed to combine
    // and bounce correctly between two balls.
    private static PhysicsMaterial2D[] _chaosMaterialPool;

    private BallSpawn.gameMode _gameMode;
    private Coroutine _gravityEaseIn;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        if (_isMerged)
        {
            StartCoroutine(WaitForDestroy());
        }
    }

    public void OnGravityChanged(float gravity)
    {
        // Gravity mode periodically flips Physics2D.gravity to pull everything
        // upward for a moment (GravityFlipController). A ball released right as
        // that pulse is active used to snap straight to gravityScale 1 and get
        // the FULL pulse instantly - and since a just-dropped ball starts right
        // next to the ceiling, "full upward force" meant it reached the ceiling
        // almost the instant it was released. Easing gravityScale in over a
        // fraction of a second instead gives the ball (and the pulse, which eases
        // itself in/out over a similar timescale) a moment to settle rather than
        // slamming into the ceiling the instant it's let go. Every other mode is
        // unaffected - this only changes behavior for Gravity.
        if (_gravityEaseIn != null)
        {
            StopCoroutine(_gravityEaseIn);
            _gravityEaseIn = null;
        }

        if (gravity > 0f && _gameMode == BallSpawn.gameMode.Gravity)
        {
            _gravityEaseIn = StartCoroutine(EaseInGravityScale(gravity));
        }
        else
        {
            _rigidbody.gravityScale = gravity;
        }
    }

    private IEnumerator EaseInGravityScale(float targetScale)
    {
        const float duration = 0.4f;
        float elapsed = 0f;
        _rigidbody.gravityScale = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _rigidbody.gravityScale = Mathf.Lerp(0f, targetScale, elapsed / duration);
            yield return null;
        }

        _rigidbody.gravityScale = targetScale;
        _gravityEaseIn = null;
    }

    public void OnGameModeChanged(BallSpawn.gameMode gameMode)
    {
        _gameMode = gameMode;

        if (gameMode == BallSpawn.gameMode.Chaos)
        {
            PhysicsMaterial2D[] pool = GetChaosMaterialPool();
            if (pool.Length > 0)
            {
                _collider.sharedMaterial = pool[UnityEngine.Random.Range(0, pool.Length)];
            }
            return;
        }

        // Every other mode (Base, Snowball, Blitz, Gravity, Infinite, and any
        // future addition) always uses the exact same fixed friction/bounciness for
        // every ball, so there's nothing to leak between balls here - mutating the
        // one shared asset directly (same as the game's original behavior) is fine
        // and is confirmed to bounce correctly both against the floor and against
        // other balls. Only Snowball layers an extra per-ball behavior (growing
        // over time) on top of this.
        PhysicsMaterial2D material = _collider.sharedMaterial;
        if (material == null)
        {
            return;
        }

        material.friction = 0.3f;
        material.bounciness = 0.4f;

        if (gameMode == BallSpawn.gameMode.Snawball)
        {
            StartCoroutine(OnScaleChanged());
        }
    }

    private static PhysicsMaterial2D[] GetChaosMaterialPool()
    {
        if (_chaosMaterialPool == null || _chaosMaterialPool.Length == 0)
        {
            _chaosMaterialPool = Resources.LoadAll<PhysicsMaterial2D>("ChaosBallMaterials");
        }
        return _chaosMaterialPool;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Ball ball))
        {
            if (ball.ballId == ballId && !_isMerged && !ball._isMerged)
            {
                if (this.GetInstanceID() > ball.GetInstanceID())
                {
                    _isMerged = true;
                    ball._isMerged = true;
                    OnDestroyed?.Invoke();
                }
                StartCoroutine(WaitForDestroy());
            }
        }
        if (collision.relativeVelocity.magnitude > 0.3f)
        {
            _audioSource.PlayOneShot(_audioSource.clip, collision.relativeVelocity.magnitude / 7);
        }
    }

    private IEnumerator OnScaleChanged()
    {
        while (transform.localScale != transform.localScale * 5)
        {
            yield return new WaitForSeconds(1f);
            Vector3 scaleInterval = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localScale = transform.localScale + scaleInterval;
        }
    }

    private IEnumerator WaitForDestroy()
    {
        yield return new WaitForEndOfFrame();
        Instantiate(_vfx, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

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

    // Collider2D has no auto-instancing `.material` shorthand the way 3D Collider
    // does - only `sharedMaterial`, which really is shared across every ball using
    // the same PhysicsMaterial2D asset. This is a lazily-created per-ball copy so
    // tweaking bounciness/friction here never leaks into other balls.
    private PhysicsMaterial2D _instanceMaterial;

    private Rigidbody2D _rigidbody;

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
        _rigidbody.gravityScale = gravity;
    }

    /// <summary>
    /// Returns this ball's own instance of its physics material, creating it (as a
    /// copy of whatever sharedMaterial was assigned in the Inspector) the first
    /// time it's needed, and assigning it back to the collider so this ball no
    /// longer shares the original asset with every other ball.
    /// </summary>
    private PhysicsMaterial2D GetInstanceMaterial()
    {
        if (_instanceMaterial == null)
        {
            PhysicsMaterial2D source = _collider.sharedMaterial;
            if (source != null)
            {
                // Object.Instantiate() makes a full, properly engine-backed clone
                // (every serialized field, including frictionCombine/bounceCombine)
                // the same way Unity creates per-instance copies for Renderer.material
                // or 3D Collider.material. A plain `new PhysicsMaterial2D(...)` with
                // manually-copied fields looked identical in the Inspector but didn't
                // actually affect physics at runtime - balls stopped bouncing entirely.
                _instanceMaterial = Instantiate(source);
                _instanceMaterial.name = source.name + " (Instance)";
            }
            else
            {
                _instanceMaterial = new PhysicsMaterial2D("Ball Material (Instance)");
            }
            _collider.sharedMaterial = _instanceMaterial;
        }
        return _instanceMaterial;
    }

    public void OnGameModeChanged(BallSpawn.gameMode gameMode)
    {
        PhysicsMaterial2D material = GetInstanceMaterial();
        material.friction = 0.3f;
        if (gameMode == BallSpawn.gameMode.Base)
        {
            material.bounciness = 0.4f;
        }
        if (gameMode == BallSpawn.gameMode.Chaos)
        {
            float index = UnityEngine.Random.Range(0, 0.8f);
            material.bounciness = index;
        }
        if (gameMode == BallSpawn.gameMode.Snawball)
        {
            material.bounciness = 0.4f;
            StartCoroutine(OnScaleChanged());
        }
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
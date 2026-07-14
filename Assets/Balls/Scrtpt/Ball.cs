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

    public void OnGameModeChanged(BallSpawn.gameMode gameMode)
    {
        _collider.sharedMaterial.friction = 0.3f;
        if (gameMode == BallSpawn.gameMode.Base)
        {
            _collider.sharedMaterial.bounciness = 0.4f;
        }
        if (gameMode == BallSpawn.gameMode.Chaos)
        {
            float index = UnityEngine.Random.Range(0, 0.8f);
            _collider.sharedMaterial.bounciness = index;
        }
        if (gameMode == BallSpawn.gameMode.Snawball)
        {
            _collider.sharedMaterial.bounciness = 0.4f;
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
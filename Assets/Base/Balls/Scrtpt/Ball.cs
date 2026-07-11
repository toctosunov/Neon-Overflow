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

    [SerializeField] private GameObject _vfx;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
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
    }

    private IEnumerator WaitForDestroy()
    {
        yield return new WaitForEndOfFrame();
        Instantiate(_vfx, gameObject.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
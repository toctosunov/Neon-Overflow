using System.Collections;
using UnityEngine;

public class BallSpawn : MonoBehaviour
{
    public enum gameMode { Chaos, Snawball, Base, Blitz }
    public gameMode _gameMode;

    [SerializeField] GameObject[] balls;
    [SerializeField] Camera _camera;
    private GameObject _lastball;
    [SerializeField] private float _speed;

    [Header("Difficulty curve")]
    [Tooltip("Spawn cooldown (seconds) at score 0.")]
    [SerializeField] private float _baseSpawnCooldown = 1f;
    [Tooltip("Spawn cooldown (seconds) once the difficulty ramp is fully maxed out.")]
    [SerializeField] private float _minSpawnCooldown = 0.5f;
    [Tooltip("Score at which the cooldown reaches its minimum value.")]
    [SerializeField] private int _cooldownRampScore = 40;
    [Tooltip("How many ball colors are available right at the start of a run.")]
    [SerializeField] private int _startingColorCount = 4;
    [Tooltip("Score points needed to unlock each additional color beyond the starting set.")]
    [SerializeField] private int _scorePerColorUnlock = 8;

    [SerializeField] private bool _isSpawning = false;
    [SerializeField] private bool _hold = false;
    [SerializeField] private bool _canSpawn = true;

    Vector3 _mousePosition;

    private void Update()
    {
        if (IsPointerDown())
        {
            Spawn();
        }
        if (IsPointerUp())
        {
            // _lastball can be null if the player releases the pointer
            // without ever having held a ball (e.g. pointer-up outside the game
            // view, or a stray event before the first spawn).
            if (_lastball != null)
            {
                Ball ball = _lastball.GetComponent<Ball>();
                if (ball != null)
                {
                    ball.OnGravityChanged(1);
                }
            }
            _hold = false;
            _isSpawning = false;
        }
    }

    /// <summary>
    /// Mouse works fine in the Editor and on desktop browsers, but WebGL on mobile
    /// doesn't reliably synthesize mouse events from touches, so touch is handled
    /// explicitly here rather than relying on that emulation. When a touch is
    /// active it takes priority over mouse for that frame to avoid double input
    /// from browsers that *do* also emit a synthetic mouse event alongside touch.
    /// </summary>
    private bool IsPointerDown()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).phase == TouchPhase.Began;
        }
        return Input.GetMouseButtonDown(0);
    }

    private bool IsPointerUp()
    {
        if (Input.touchCount > 0)
        {
            TouchPhase phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Ended || phase == TouchPhase.Canceled;
        }
        return Input.GetMouseButtonUp(0);
    }

    private Vector3 GetPointerScreenPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Input.mousePosition;
    }

    private void FixedUpdate()
    {
        if (_hold)
        {
            Hold();
        }
    }

    private void Hold()
    {
        if (_lastball == null)
        {
            _hold = false;
            return;
        }

        Vector3 pointerWorldPos = _camera.ScreenToWorldPoint(GetPointerScreenPosition());
        float targetX = pointerWorldPos.x;

        targetX = Mathf.Clamp(targetX, -1.40f, 1.40f);

        Vector2 nextPosition = new Vector2(targetX, 2.8f);
        Vector2 smoothedPosition = Vector2.Lerp(_lastball.transform.position, nextPosition, _speed * Time.fixedDeltaTime);

        _lastball.GetComponent<Rigidbody2D>().MovePosition(smoothedPosition);
    }

    private void Spawn()
    {
        if (!_isSpawning && !_hold && _canSpawn)
        {
            _mousePosition = _camera.ScreenToWorldPoint(GetPointerScreenPosition());
            _mousePosition.z = 0;
            _mousePosition.y = 2.8f;

            int randomIndex = GetWeightedBallIndex();
            _lastball = Instantiate(balls[randomIndex], _mousePosition, Quaternion.identity);
            _lastball.GetComponent<Ball>().OnGravityChanged(0);
            _lastball.GetComponent<Ball>().OnGameModeChanged(_gameMode);
            _isSpawning = true;
            _hold = true;
            StartCoroutine(CanSpawn());
        }
    }

    /// <summary>
    /// Picks a ball color index, but only among the colors unlocked for the
    /// current score. Early game = fewer colors = easier matches; the full
    /// palette opens up gradually as the player scores more.
    /// </summary>
    private int GetWeightedBallIndex()
    {
        int activeColorCount = GetActiveColorCount();
        return Random.Range(0, activeColorCount);
    }

    private int GetActiveColorCount()
    {
        int score = ScoreController.Instance != null ? ScoreController.Instance.score : 0;

        int unlockedByScore = score / Mathf.Max(1, _scorePerColorUnlock);
        int activeCount = Mathf.Clamp(_startingColorCount + unlockedByScore, 1, balls.Length);
        return activeCount;
    }

    /// <summary>
    /// Spawn cooldown shrinks from _baseSpawnCooldown down to _minSpawnCooldown
    /// as the score climbs toward _cooldownRampScore, so later play is
    /// noticeably faster-paced than the opening seconds of a run.
    /// </summary>
    private float GetCurrentSpawnCooldown()
    {
        int score = ScoreController.Instance != null ? ScoreController.Instance.score : 0;
        float t = Mathf.Clamp01((float)score / Mathf.Max(1, _cooldownRampScore));
        return Mathf.Lerp(_baseSpawnCooldown, _minSpawnCooldown, t);
    }

    private IEnumerator CanSpawn()
    {
        _canSpawn = false;
        yield return new WaitForSeconds(GetCurrentSpawnCooldown());
        _canSpawn = true;
    }
}

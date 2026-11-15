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
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Spawn();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            // _lastball can be null if the player releases the mouse button
            // without ever having held a ball (e.g. mouse-up outside the game
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

        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        float targetX = mouseWorldPos.x;

        targetX = Mathf.Clamp(targetX, -1.40f, 1.40f);

        Vector2 nextPosition = new Vector2(targetX, 2.8f);
        Vector2 smoothedPosition = Vector2.Lerp(_lastball.transform.position, nextPosition, _speed * Time.fixedDeltaTime);

        _lastball.GetComponent<Rigidbody2D>().MovePosition(smoothedPosition);
    }

    private void Spawn()
    {
        if (!_isSpawning && !_hold && _canSpawn)
        {
            _mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
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

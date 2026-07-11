using System.Collections;
using UnityEngine;

public class BallSpawn : MonoBehaviour
{
    [SerializeField] GameObject[] balls;
    [SerializeField] Camera _camera;
    private GameObject _lastball;
    [SerializeField] private float _speed;

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
            _lastball.GetComponent<Ball>().OnGravityChanged(1);
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

            int randomIndex = Random.Range(0, balls.Length);
            _lastball = Instantiate(balls[randomIndex], _mousePosition, Quaternion.identity);
            _lastball.GetComponent<Ball>().OnGravityChanged(0);
            _isSpawning = true;
            _hold = true;
            _canSpawn = false;
            StartCoroutine(CanSpawn());
        }
    }

    private IEnumerator CanSpawn()
    {
        yield return new WaitForSeconds(1f);
        _canSpawn = true;
    }
}
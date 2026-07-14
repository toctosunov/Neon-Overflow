using UnityEngine;

public class BallVfx : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.Play();
        Destroy(gameObject, _lifeTime);
    }
}
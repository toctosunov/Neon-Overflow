    using UnityEngine;

    public class PlaySound : MonoBehaviour
    {
        [SerializeField] private Sounds _sound;
        [SerializeField] private AudioSource _audioSource;

        private void OnEnable()
        {
            Ball.OnDestroyed += SoundPlayOut;
        }

        private void OnDisable()
        { 
            Ball.OnDestroyed -= SoundPlayOut;
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void SoundPlayOut()
        {
            _audioSource.PlayOneShot(_sound.source[1].clip, 0.5f);
            }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.relativeVelocity.magnitude > 0.3f)
            {
                _audioSource.PlayOneShot(_sound.source[0].clip, collision.relativeVelocity.magnitude / 10);
            }
        }
    }
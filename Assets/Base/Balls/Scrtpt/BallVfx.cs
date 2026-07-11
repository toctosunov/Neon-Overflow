using UnityEngine;

public class BallVfx : MonoBehaviour
{
    [SerializeField] private float _lifeTime;

    void Start()
    {
        Destroy(gameObject, _lifeTime);    
    }
}
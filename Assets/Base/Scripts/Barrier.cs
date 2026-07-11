using System;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    public static event Action OnBarrierChanged;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
        OnBarrierChanged?.Invoke();

    }
}
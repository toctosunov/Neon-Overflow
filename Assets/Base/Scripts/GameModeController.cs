using Unity.VisualScripting;
using UnityEngine;

public class GameModeController : MonoBehaviour
{
    private enum gameMode {Chaos, Order, Base}

    [SerializeField]private gameMode _gameMode;

    private void Start()
    {
        if(_gameMode == gameMode.Chaos)
        {

        }
        
    }
}

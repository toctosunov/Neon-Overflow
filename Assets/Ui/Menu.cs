using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallAnimation : MonoBehaviour
{
    [SerializeField] private int _levelToLoad;
    [SerializeField] private GameObject _title;

    [SerializeField] private Animator _animator;
    private int _rotateAnimation = Animator.StringToHash("Rotation");

    public void FastRotation()
    {
        _animator.SetTrigger(_rotateAnimation);
        _title.SetActive(false);
        StartCoroutine(LoadScene());
    }

    public IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(_levelToLoad);
    }
}
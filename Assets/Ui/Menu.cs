using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [SerializeField] private int _levelToLoad;
    [SerializeField] private GameObject _title;

    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float duration = 0.5f;

    private void Start()
    {
        fadePanel.alpha = 1;
        fadePanel.blocksRaycasts = true;

        fadePanel.DOFade(0, duration)
            .SetEase(Ease.InOutQuad) 
            .OnComplete(() => fadePanel.blocksRaycasts = false);
    }

    public void LoadNewScene(string sceneName)
    {

        _title.SetActive(false);
        fadePanel.blocksRaycasts = true;

        fadePanel.DOFade(1, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => SceneManager.LoadScene(_levelToLoad));
    }
}
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
        if (fadePanel == null)
        {
            return;
        }

        // Defensive: this is the only thing standing between the player and a
        // half-broken intro (no fade, timer starting too early) if the Panel
        // GameObject itself ever got left disabled in the Editor - switching it
        // on from code here means that can't happen regardless of its state
        // when the scene loads.
        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1;
        fadePanel.blocksRaycasts = true;

        fadePanel.DOFade(0, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                fadePanel.blocksRaycasts = false;
                fadePanel.gameObject.SetActive(false);

                // Same place that controls the black panel's visibility is what
                // starts the Blitz countdown - that way there's no second timer
                // needed just to wait for this fade to finish; the panel
                // finishing its fade *is* "the scene is actually playable now".
                BlitzTimer timer = FindObjectOfType<BlitzTimer>();
                if (timer != null)
                {
                    timer.BeginCountdown();
                }
            });
    }

    public void LoadNewScene(string sceneName)
    {

        _title.SetActive(false);

        if (fadePanel == null)
        {
            SceneManager.LoadScene(_levelToLoad);
            return;
        }

        fadePanel.gameObject.SetActive(true);
        fadePanel.blocksRaycasts = true;

        fadePanel.DOFade(1, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => SceneManager.LoadScene(_levelToLoad));
    }
}
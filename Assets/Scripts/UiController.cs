using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiController : MonoBehaviour
{
    public static UiController Instance;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI losePanelHighScoreText;

    public GameObject losePanel;

    [Tooltip("Optional. \"Continue for a rewarded ad?\" panel, shown by " +
        "ScoreController instead of an immediate Game Over when a revive is " +
        "available. Leave empty to keep today's behavior (no revive offer).")]
    public GameObject revivePanel;

    [SerializeField] private int _menuScene;

    private void Start()
    {
        Instance = this;
        losePanel.SetActive(false);
        if (revivePanel != null)
        {
            revivePanel.SetActive(false);
        }
        if (countText != null)
        {
            countText.SetText(Localization.Get(Localization.Score) + ScoreController.Instance.score.ToString());
        }

        if (highScoreText != null)
        {
            ScoreController.Instance.OnScoreChanged();
            highScoreText.SetText(Localization.Get(Localization.Record) + ScoreController.Instance.recordScore.ToString());
        }
    }

    public void GoMenu()
    {
        Yandex.YandexInterstitialAdTrigger.TryShowThenRun(() => SceneManager.LoadScene(_menuScene));
    }

    public void Restart()
    {
        int activeScene = SceneManager.GetActiveScene().buildIndex;
        Yandex.YandexInterstitialAdTrigger.TryShowThenRun(() => SceneManager.LoadScene(activeScene));
    }
}

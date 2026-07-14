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
    [SerializeField] private int _menuScene;

    private void Start()
    {
        Instance = this;
        losePanel.SetActive(false);
        if (countText != null)
        {
            countText.SetText("Очки: " + ScoreController.Instance.score.ToString());
        }

        if (highScoreText != null)
        {
            ScoreController.Instance.OnScoreChanged();
            highScoreText.SetText("Рекорд: " + ScoreController.Instance.recordScore.ToString());
        }
    }

    public void GoMenu()
    {
        SceneManager.LoadScene(_menuScene);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

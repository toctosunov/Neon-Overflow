using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController Instance;
    public int score;
    public int recordScore;
    public bool isRecorted = false;
    public int life = 1;

    [SerializeField] private GameObject _spawner;

    private void OnEnable()
    {
        Barrier.OnBarrierChanged += MinusLife;
        BlitzTimer.OnTimeEnd += GameOver;
        Ball.OnDestroyed += CountChanged;
    }

    private void OnDisable()
    {
        Barrier.OnBarrierChanged -= MinusLife;
        BlitzTimer.OnTimeEnd -= GameOver;
        Ball.OnDestroyed -= CountChanged;
    }

    private void Awake()
    {
        Instance = this;
        score = 0;
        isRecorted = false;
    }


    private void CountChanged()
    {
        score += 1;
        UiController.Instance.countText.SetText("Очки: " + score.ToString());

        if (score >= recordScore && !isRecorted)
        {
            UiController.Instance.highScoreText.enabled = false;
            isRecorted = true;
        }
    }

    private void MinusLife()
    {
        if (life > 0)
        {
            life -= 1;
            if (life <= 0)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        if (isRecorted)
        {
            BallSpawn.gameMode mode = _spawner.GetComponent<BallSpawn>()._gameMode;
            string key = "";

            switch (mode)
            {
                case BallSpawn.gameMode.Base: key = "BaseScore"; break;
                case BallSpawn.gameMode.Snawball: key = "SnawballScore"; break;
                case BallSpawn.gameMode.Chaos: key = "ChaosScore"; break;
                case BallSpawn.gameMode.Blitz: key = "BlitzScore"; break;
            }
            PlayerPrefs.SetInt(key, score);
            recordScore = PlayerPrefs.GetInt(key, 0);
            UiController.Instance.losePanelHighScoreText.SetText("Новый рекорд: " + recordScore.ToString());
        }
        else
        {
            OnScoreChanged();

            UiController.Instance.losePanelHighScoreText.SetText("Рекорд: " + recordScore.ToString());
        }
        UiController.Instance.losePanel.SetActive(true);
        _spawner.SetActive(false);
    }

    public int OnScoreChanged()
    {
        BallSpawn.gameMode mode = _spawner.GetComponent<BallSpawn>()._gameMode;

        string key = "";
        switch (mode)
        {
            case BallSpawn.gameMode.Base: key = "BaseScore"; break;
            case BallSpawn.gameMode.Snawball: key = "SnawballScore"; break; 
            case BallSpawn.gameMode.Chaos: key = "ChaosScore"; break;
            case BallSpawn.gameMode.Blitz: key = "BlitzScore"; break;
        }

        recordScore = PlayerPrefs.GetInt(key, 0); 
        return recordScore;
    }
}
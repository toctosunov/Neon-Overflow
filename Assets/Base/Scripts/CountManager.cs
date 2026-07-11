namespace BaseMode
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class CountManager : MonoBehaviour
    {
        [SerializeField] private GameObject _spawner;

        [SerializeField] private int _count;
        [SerializeField] private int _highScore;
        [SerializeField] private bool _isRecorted = false;
        [SerializeField] private int _life = 1;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _losePanelHighScoreText;

        [SerializeField] private GameObject _losePanel;

        [SerializeField] private int _menuScene;

        private void OnEnable()
        {
            Barrier.OnBarrierChanged += MinusLife;
            Ball.OnDestroyed += CountChanged;
        }

        private void OnDisable()
        {
            Barrier.OnBarrierChanged -= MinusLife;
            Ball.OnDestroyed -= CountChanged;
        }

        private void Awake()
        {
            _losePanel.SetActive(false);
            if (_countText != null) 
            {
                _countText.SetText("Очки: " + _count.ToString());
            }

            if (_highScoreText != null)
            {
                _highScore = PlayerPrefs.GetInt("BaseScore");
                _highScoreText.SetText("Рекорд: " + _highScore.ToString());
            }
        }

        private void CountChanged()
        {
            _count += 1;
            _countText.SetText("Очки: " + _count.ToString());

            if (_count >= _highScore && !_isRecorted)
            {
                _highScoreText.enabled = false;
                Debug.Log("Новый рекорд");
                _isRecorted = true;
            }
        }

        private void MinusLife()
        {
            if (_life > 0)
            {
                _life -= 1;
                if (_life <= 0)
                {
                    GameOver();
                }
            }
        }

        public void OnRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnMenu()
        {
            SceneManager.LoadScene(_menuScene);
        }

        private void GameOver()
        {
            if (_isRecorted)
            {
                PlayerPrefs.SetInt("BaseScore", _count);
                _highScore = PlayerPrefs.GetInt("BaseScore");
                _losePanelHighScoreText.SetText("Новый рекорд: " + _highScore.ToString());
            }
            else
            {
                _losePanelHighScoreText.SetText("Рекорд: " + _highScore.ToString());
            }
            _losePanel.SetActive(true);
            _spawner.SetActive(false);
        }
    }
}
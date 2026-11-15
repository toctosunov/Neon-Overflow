using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public static ScoreController Instance;
    public int score;
    public int recordScore;
    public bool isRecorted = false;
    public int life = 1;

    [SerializeField] private GameObject _spawner;

    [Header("Revive (rewarded ad)")]
    [Tooltip("Optional. A MonoBehaviour implementing IRewardedAdProvider (e.g. " +
        "StubRewardedAdProvider, or a real Yandex SDK wrapper once it's wired in). " +
        "If left empty, or if it reports no ad available, revive is skipped and the " +
        "game ends immediately like it always did.")]
    [SerializeField] private MonoBehaviour _rewardedAdProviderBehaviour;
    private IRewardedAdProvider _rewardedAdProvider;

    // Only one revive allowed per run, so this can't be farmed indefinitely.
    private bool _reviveUsed;
    private bool _pendingGameOver;

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
        _reviveUsed = false;
        _pendingGameOver = false;

        _rewardedAdProvider = _rewardedAdProviderBehaviour as IRewardedAdProvider;
        if (_rewardedAdProvider == null)
        {
            // Nothing wired up in the Inspector - fall back to whatever
            // IRewardedAdProvider GameBootstrap set up automatically (normally a
            // Yandex.YandexRewardedAdProvider on the persistent bridge object).
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is IRewardedAdProvider provider)
                {
                    _rewardedAdProvider = provider;
                    break;
                }
            }
        }
    }


    private void CountChanged()
    {
        score += 1;
        UiController.Instance.countText.SetText(Localization.Get(Localization.Score) + score.ToString());

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
                TryOfferReviveOrGameOver();
            }
        }
    }

    /// <summary>
    /// Called when the player would otherwise lose right now. If a revive hasn't
    /// been used yet this run and a rewarded ad is available, pauses the spawner and
    /// shows the revive prompt instead of ending the run immediately. Falls back to
    /// the normal GameOver() flow if revive isn't available/used, or if no UI hook
    /// is wired up to ask the player.
    /// </summary>
    private void TryOfferReviveOrGameOver()
    {
        bool canOfferRevive = !_reviveUsed
            && _rewardedAdProvider != null
            && _rewardedAdProvider.IsAvailable()
            && UiController.Instance != null
            && UiController.Instance.revivePanel != null;

        if (!canOfferRevive)
        {
            GameOver();
            return;
        }

        _pendingGameOver = true;
        _spawner.SetActive(false);
        UiController.Instance.revivePanel.SetActive(true);
    }

    /// <summary>Hook this up to the revive panel's "Watch ad" button.</summary>
    public void OnReviveAccepted()
    {
        if (!_pendingGameOver)
        {
            return;
        }

        _rewardedAdProvider.Show(
            onRewarded: () =>
            {
                _reviveUsed = true;
                _pendingGameOver = false;
                life = 1;
                if (UiController.Instance != null && UiController.Instance.revivePanel != null)
                {
                    UiController.Instance.revivePanel.SetActive(false);
                }
                _spawner.SetActive(true);
            },
            onFailedOrSkipped: OnReviveDeclined);
    }

    /// <summary>Hook this up to the revive panel's "No thanks" button, and call it directly
    /// if the ad fails to load/play.</summary>
    public void OnReviveDeclined()
    {
        if (!_pendingGameOver)
        {
            return;
        }

        _pendingGameOver = false;
        if (UiController.Instance != null && UiController.Instance.revivePanel != null)
        {
            UiController.Instance.revivePanel.SetActive(false);
        }
        GameOver();
    }

    private void GameOver()
    {
        BallSpawn.gameMode mode = _spawner.GetComponent<BallSpawn>()._gameMode;
        string key = GetScoreKey(mode);

        if (isRecorted)
        {
            PlayerPrefs.SetInt(key, score);
            recordScore = PlayerPrefs.GetInt(key, 0);
            UiController.Instance.losePanelHighScoreText.SetText(Localization.Get(Localization.NewRecord) + recordScore.ToString());
        }
        else
        {
            OnScoreChanged();

            UiController.Instance.losePanelHighScoreText.SetText(Localization.Get(Localization.Record) + recordScore.ToString());
        }

        // Fire-and-forget: no-ops outside a Yandex Games WebGL build (see
        // YandexGamesBridge). Submits every run's score, not just new records -
        // the Yandex leaderboard itself only displays each player's best.
        if (Yandex.YandexGamesBridge.Instance != null && !string.IsNullOrEmpty(key))
        {
            Yandex.YandexGamesBridge.Instance.SubmitLeaderboardScore(key, score);
        }

        UiController.Instance.losePanel.SetActive(true);
        _spawner.SetActive(false);
    }

    private static string GetScoreKey(BallSpawn.gameMode mode)
    {
        switch (mode)
        {
            case BallSpawn.gameMode.Base: return "BaseScore";
            case BallSpawn.gameMode.Snawball: return "SnawballScore";
            case BallSpawn.gameMode.Chaos: return "ChaosScore";
            case BallSpawn.gameMode.Blitz: return "BlitzScore";
            default: return "";
        }
    }

    public int OnScoreChanged()
    {
        BallSpawn.gameMode mode = _spawner.GetComponent<BallSpawn>()._gameMode;
        string key = GetScoreKey(mode);

        recordScore = PlayerPrefs.GetInt(key, 0); 
        return recordScore;
    }
}
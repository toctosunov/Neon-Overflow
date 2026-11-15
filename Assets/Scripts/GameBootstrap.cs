using UnityEngine;
using Yandex;

/// <summary>
/// Creates the handful of persistent, cross-scene singletons the game needs
/// (Yandex SDK bridge, tab-focus pause handling) automatically on startup, so
/// nobody has to remember to drop a GameObject into every scene in the Unity
/// Editor. Runs once, before the first scene's own Start() methods.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (YandexGamesBridge.Instance == null)
        {
            var bridgeObject = new GameObject("YandexGamesBridge");
            bridgeObject.AddComponent<YandexGamesBridge>();
            // YandexGamesBridge.Awake() marks itself DontDestroyOnLoad and calls
            // Ready() on the SDK.

            // Also wires up the real rewarded-ad provider automatically, so nobody
            // has to manually assign ScoreController's _rewardedAdProviderBehaviour
            // field in every scene - it auto-resolves to this if left empty (see
            // ScoreController.Awake()). The only remaining manual step is building
            // a revivePanel UI and assigning it to UiController.revivePanel.
            bridgeObject.AddComponent<YandexRewardedAdProvider>();
        }

        var pauseHandlerObject = new GameObject("ApplicationPauseHandler");
        pauseHandlerObject.AddComponent<ApplicationPauseHandler>();
        Object.DontDestroyOnLoad(pauseHandlerObject);
    }
}

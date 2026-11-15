using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Yandex
{
    /// <summary>
    /// Single entry point to the Yandex Games SDK (see
    /// Assets/Plugins/WebGL/YandexGamesBridge.jslib and
    /// Assets/WebGLTemplates/YandexGames/index.html for the JS side).
    ///
    /// Outside of an actual WebGL build (Editor play mode, other platforms) every
    /// call here safely no-ops or reports failure instead of throwing, so the rest
    /// of the game can call this unconditionally without #if guards everywhere.
    ///
    /// Must live on a persistent GameObject present in every scene that needs it
    /// (simplest: add it to the first scene that loads - MenuScene - and mark it
    /// DontDestroyOnLoad, which this component does itself).
    /// </summary>
    public class YandexGamesBridge : MonoBehaviour
    {
        public static YandexGamesBridge Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void YandexSDKReady();
        [DllImport("__Internal")] private static extern IntPtr YandexSDKGetLanguage();
        [DllImport("__Internal")] private static extern void YandexSDKShowRewardedAd(string gameObjectName, string onRewardedMethod, string onClosedMethod);
        [DllImport("__Internal")] private static extern void YandexSDKShowInterstitialAd(string gameObjectName, string onClosedMethod);
        [DllImport("__Internal")] private static extern void YandexSDKSubmitLeaderboardScore(string leaderboardName, int score);
#endif

        private Action _pendingRewardedOnRewarded;
        private Action<string> _pendingRewardedOnClosed;
        private Action<string> _pendingInterstitialOnClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tell the SDK the game has finished loading and the first frame is
            // interactive. Required for the game to pass Yandex Games moderation.
            Ready();
        }

        public void Ready()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            YandexSDKReady();
#else
            Debug.Log("[YandexGamesBridge] Ready() - no-op outside a WebGL build.");
#endif
        }

        /// <summary>Two-letter language code from the Yandex Games environment (e.g. "ru", "en"),
        /// or "en" as a safe default outside WebGL / before the SDK responds.</summary>
        public string GetLanguage()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            IntPtr ptr = YandexSDKGetLanguage();
            if (ptr == IntPtr.Zero)
            {
                return "en";
            }
            string lang = Marshal.PtrToStringUTF8(ptr);
            Marshal.FreeHGlobal(ptr);
            return string.IsNullOrEmpty(lang) ? "en" : lang;
#else
            return "en";
#endif
        }

        /// <summary>
        /// Shows a rewarded video. onRewarded fires only if the viewer watched it to
        /// completion; onFailedOrSkipped fires for every other outcome (no SDK, no ad
        /// loaded, closed early, error). Only one rewarded-ad request is tracked at a
        /// time - don't call this again before the previous one's callback fired.
        /// </summary>
        public void ShowRewardedAd(Action onRewarded, Action onFailedOrSkipped)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingRewardedOnRewarded = onRewarded;
            _pendingRewardedOnClosed = _ => onFailedOrSkipped?.Invoke();
            YandexSDKShowRewardedAd(gameObject.name, nameof(HandleRewardedAdRewarded), nameof(HandleRewardedAdClosed));
#else
            Debug.Log("[YandexGamesBridge] ShowRewardedAd() - no SDK outside a WebGL build; reporting failed/skipped.");
            onFailedOrSkipped?.Invoke();
#endif
        }

        /// <summary>
        /// Shows a fullscreen interstitial. onClosed receives "shown"/"not_shown"/
        /// "no_sdk"/"error" - most callers only care that it fired to resume gameplay.
        /// </summary>
        public void ShowInterstitialAd(Action<string> onClosed)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingInterstitialOnClosed = onClosed;
            YandexSDKShowInterstitialAd(gameObject.name, nameof(HandleInterstitialAdClosed));
#else
            Debug.Log("[YandexGamesBridge] ShowInterstitialAd() - no-op outside a WebGL build.");
            onClosed?.Invoke("no_sdk");
#endif
        }

        /// <summary>Fire-and-forget leaderboard submit; silently does nothing if unavailable.</summary>
        public void SubmitLeaderboardScore(string leaderboardName, int score)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            YandexSDKSubmitLeaderboardScore(leaderboardName, score);
#else
            Debug.Log($"[YandexGamesBridge] SubmitLeaderboardScore({leaderboardName}, {score}) - no-op outside a WebGL build.");
#endif
        }

        // --- Callback targets invoked by the .jslib plugin via SendMessage. Must be
        // public instance methods taking zero or one string argument. ---

        public void HandleRewardedAdRewarded(string _)
        {
            Action callback = _pendingRewardedOnRewarded;
            _pendingRewardedOnRewarded = null;
            _pendingRewardedOnClosed = null;
            callback?.Invoke();
        }

        public void HandleRewardedAdClosed(string reason)
        {
            Action<string> callback = _pendingRewardedOnClosed;
            _pendingRewardedOnRewarded = null;
            _pendingRewardedOnClosed = null;
            callback?.Invoke(reason);
        }

        public void HandleInterstitialAdClosed(string reason)
        {
            Action<string> callback = _pendingInterstitialOnClosed;
            _pendingInterstitialOnClosed = null;
            callback?.Invoke(reason);
        }
    }
}

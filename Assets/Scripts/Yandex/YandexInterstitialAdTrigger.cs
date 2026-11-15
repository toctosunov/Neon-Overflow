using System;
using UnityEngine;

namespace Yandex
{
    /// <summary>
    /// Convenience wrapper for showing interstitials at natural break points
    /// (returning to the menu, restarting a run). The Yandex Games SDK already
    /// enforces its own minimum interval between fullscreen ads server-side, but
    /// this adds a small client-side cooldown too, purely to avoid spamming
    /// Show() calls (and the resulting SendMessage callbacks) back to back if this
    /// is wired to more than one UI button in a short span.
    /// </summary>
    public static class YandexInterstitialAdTrigger
    {
        private const float MinSecondsBetweenAttempts = 30f;
        private static float _lastAttemptRealtime = -Mathf.Infinity;

        /// <summary>
        /// Attempts to show an interstitial and only runs `onDone` once the ad has
        /// closed (or immediately, if no ad was shown/available) - so a scene
        /// transition queued in `onDone` doesn't race the ad request.
        /// </summary>
        public static void TryShowThenRun(Action onDone)
        {
            bool withinCooldown = Time.realtimeSinceStartup - _lastAttemptRealtime < MinSecondsBetweenAttempts;

            if (YandexGamesBridge.Instance == null || withinCooldown)
            {
                onDone?.Invoke();
                return;
            }

            _lastAttemptRealtime = Time.realtimeSinceStartup;
            YandexGamesBridge.Instance.ShowInterstitialAd(reason =>
            {
                Debug.Log($"[YandexInterstitialAdTrigger] Interstitial closed: {reason}");
                onDone?.Invoke();
            });
        }
    }
}

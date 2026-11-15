using System;
using UnityEngine;

namespace Yandex
{
    /// <summary>
    /// Real IRewardedAdProvider backed by YandexGamesBridge. Drop this component
    /// (instead of StubRewardedAdProvider) on ScoreController's
    /// _rewardedAdProviderBehaviour field once YandexGamesBridge is present in the
    /// scene (see that class's doc comment).
    /// </summary>
    public class YandexRewardedAdProvider : MonoBehaviour, IRewardedAdProvider
    {
        public bool IsAvailable()
        {
            // The Yandex Games SDK doesn't expose a separate "is a rewarded ad
            // preloaded" check in the basic adv API - availability is really "is the
            // bridge present at all". A real "no ad right now" case surfaces through
            // Show()'s onFailedOrSkipped callback instead.
            return YandexGamesBridge.Instance != null;
        }

        public void Show(Action onRewarded, Action onFailedOrSkipped)
        {
            if (YandexGamesBridge.Instance == null)
            {
                onFailedOrSkipped?.Invoke();
                return;
            }

            YandexGamesBridge.Instance.ShowRewardedAd(onRewarded, onFailedOrSkipped);
        }
    }
}

using System;
using UnityEngine;

/// <summary>
/// Placeholder IRewardedAdProvider used until the real Yandex Games SDK is wired in
/// (see IRewardedAdProvider.cs for the integration steps). Always reports itself as
/// unavailable, so any revive-for-ad UI built against IRewardedAdProvider simply stays
/// hidden and the game behaves exactly as it does today (no ads, immediate Game Over)
/// rather than silently faking a successful ad watch.
/// </summary>
public class StubRewardedAdProvider : MonoBehaviour, IRewardedAdProvider
{
    public bool IsAvailable() => false;

    public void Show(Action onRewarded, Action onFailedOrSkipped)
    {
        Debug.LogWarning("StubRewardedAdProvider.Show() called, but no real ad SDK is " +
            "wired in yet - see IRewardedAdProvider.cs. Treating this as a failed/skipped ad.");
        onFailedOrSkipped?.Invoke();
    }
}

using System;

/// <summary>
/// Abstraction over "show a rewarded video ad" so gameplay code (ScoreController,
/// BlitzTimer, etc.) never talks to a specific ad SDK directly.
///
/// To wire this up to the real Yandex Games SDK:
///   1. Import the Yandex Games SDK for Unity (or your WebGL JS bridge of choice).
///   2. Implement this interface in a new MonoBehaviour, e.g. YandexRewardedAdProvider,
///      calling the SDK's RewardedAdAPI.Show()-equivalent inside Show(), and invoking
///      onRewarded only when the SDK reports the video was watched to completion.
///   3. Replace the StubRewardedAdProvider component on the persistent "Ads" object
///      (see StubRewardedAdProvider.cs) with your new implementation in each scene,
///      or swap it at runtime based on platform.
///
/// Until that's done, StubRewardedAdProvider is wired in and always reports "not
/// available", so the revive flow degrades gracefully to today's behavior
/// (immediate Game Over) instead of pretending ads work when they don't.
/// </summary>
public interface IRewardedAdProvider
{
    /// <summary>Whether a rewarded ad can currently be shown (SDK ready, ad loaded, etc.).</summary>
    bool IsAvailable();

    /// <summary>
    /// Attempts to show a rewarded ad. Calls exactly one of the two callbacks:
    /// onRewarded when the viewer watched it through to completion, onFailedOrSkipped
    /// otherwise (no ad loaded, network error, the viewer closed it early, etc.).
    /// </summary>
    void Show(Action onRewarded, Action onFailedOrSkipped);
}

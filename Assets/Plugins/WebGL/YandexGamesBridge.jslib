// WebGL plugin bridge to the Yandex Games SDK (window.ysdk).
//
// Assumes `window.ysdk` was already created and assigned by the WebGL template's
// index.html (see Assets/WebGLTemplates/YandexGames/index.html) BEFORE the Unity
// instance is created - i.e. the page awaits YaGames.init() first. If ysdk isn't
// present (local testing outside Yandex Games, or the SDK failed to load), every
// function here degrades gracefully instead of throwing.
mergeInto(LibraryManager.library, {

  YandexSDKReady: function () {
    try {
      if (window.ysdk && window.ysdk.features && window.ysdk.features.LoadingAPI) {
        window.ysdk.features.LoadingAPI.ready();
      }
    } catch (e) {
      console.warn('YandexSDKReady failed:', e);
    }
  },

  // Returns a malloc'd UTF8 string the C# side must free via Marshal.FreeHGlobal.
  YandexSDKGetLanguage: function () {
    var lang = 'en';
    try {
      if (window.ysdk && window.ysdk.environment && window.ysdk.environment.i18n) {
        lang = window.ysdk.environment.i18n.lang || 'en';
      }
    } catch (e) {
      console.warn('YandexSDKGetLanguage failed:', e);
    }
    var bufferSize = lengthBytesUTF8(lang) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(lang, buffer, bufferSize);
    return buffer;
  },

  // Shows a rewarded video. Calls SendMessage(gameObjectName, onRewardedMethod, "")
  // only if the viewer watched it through to completion; otherwise calls
  // SendMessage(gameObjectName, onClosedMethod, <reason string>).
  YandexSDKShowRewardedAd: function (gameObjectNamePtr, onRewardedMethodPtr, onClosedMethodPtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    var onRewardedMethod = UTF8ToString(onRewardedMethodPtr);
    var onClosedMethod = UTF8ToString(onClosedMethodPtr);

    if (!window.ysdk || !window.ysdk.adv || !window.ysdk.adv.showRewardedVideo) {
      SendMessage(gameObjectName, onClosedMethod, 'no_sdk');
      return;
    }

    var wasRewarded = false;
    try {
      window.ysdk.adv.showRewardedVideo({
        callbacks: {
          onRewarded: function () {
            wasRewarded = true;
          },
          onClose: function () {
            if (wasRewarded) {
              SendMessage(gameObjectName, onRewardedMethod, '');
            } else {
              SendMessage(gameObjectName, onClosedMethod, 'closed_without_reward');
            }
          },
          onError: function (error) {
            console.warn('showRewardedVideo error:', error);
            SendMessage(gameObjectName, onClosedMethod, 'error');
          }
        }
      });
    } catch (e) {
      console.warn('YandexSDKShowRewardedAd failed:', e);
      SendMessage(gameObjectName, onClosedMethod, 'error');
    }
  },

  // Shows a fullscreen interstitial. Calls SendMessage(gameObjectName, onClosedMethod,
  // "shown" | "not_shown" | "no_sdk" | "error").
  YandexSDKShowInterstitialAd: function (gameObjectNamePtr, onClosedMethodPtr) {
    var gameObjectName = UTF8ToString(gameObjectNamePtr);
    var onClosedMethod = UTF8ToString(onClosedMethodPtr);

    if (!window.ysdk || !window.ysdk.adv || !window.ysdk.adv.showFullscreenAdv) {
      SendMessage(gameObjectName, onClosedMethod, 'no_sdk');
      return;
    }

    try {
      window.ysdk.adv.showFullscreenAdv({
        callbacks: {
          onClose: function (wasShown) {
            SendMessage(gameObjectName, onClosedMethod, wasShown ? 'shown' : 'not_shown');
          },
          onError: function (error) {
            console.warn('showFullscreenAdv error:', error);
            SendMessage(gameObjectName, onClosedMethod, 'error');
          }
        }
      });
    } catch (e) {
      console.warn('YandexSDKShowInterstitialAd failed:', e);
      SendMessage(gameObjectName, onClosedMethod, 'error');
    }
  },

  // Fire-and-forget: submits a score to a named leaderboard. Silently no-ops if the
  // SDK/leaderboard isn't available (e.g. the player isn't authorized).
  YandexSDKSubmitLeaderboardScore: function (leaderboardNamePtr, score) {
    try {
      var leaderboardName = UTF8ToString(leaderboardNamePtr);
      if (window.ysdk && window.ysdk.getLeaderboards) {
        window.ysdk.getLeaderboards().then(function (lb) {
          lb.setLeaderboardScore(leaderboardName, score).catch(function (e) {
            console.warn('setLeaderboardScore failed:', e);
          });
        }).catch(function (e) {
          console.warn('getLeaderboards failed:', e);
        });
      }
    } catch (e) {
      console.warn('YandexSDKSubmitLeaderboardScore failed:', e);
    }
  }

});

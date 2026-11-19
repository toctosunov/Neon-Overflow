using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimal string localization. Language is picked once per session: from the
/// Yandex Games SDK's reported language when actually running on Yandex Games,
/// English everywhere else (Editor, CrazyGames, any other platform).
///
/// Earlier this fell back to Application.systemLanguage outside Yandex, which
/// meant a WebGL build hosted anywhere else (CrazyGames included) still showed
/// Russian for any player whose OS/browser language was Russian - not just for
/// players actually on Yandex. Since other platforms require English and have
/// no SDK of their own to report a language through, "not Yandex" now just
/// means English, full stop.
///
/// Add more languages by adding another inner dictionary to _strings and
/// widening ResolveLanguageCode(); every call site only ever asks for a key via
/// Localization.Get(...), so adding languages never touches gameplay code.
/// </summary>
public static class Localization
{
    public const string Score = "score_label";
    public const string Record = "record_label";
    public const string NewRecord = "new_record_label";

    private static readonly Dictionary<string, Dictionary<string, string>> _strings =
        new Dictionary<string, Dictionary<string, string>>
        {
            ["ru"] = new Dictionary<string, string>
            {
                [Score] = "Очки: ",
                [Record] = "Рекорд: ",
                [NewRecord] = "Новый рекорд: ",
            },
            ["en"] = new Dictionary<string, string>
            {
                [Score] = "Score: ",
                [Record] = "Best: ",
                [NewRecord] = "New best: ",
            },
        };

    private static string _languageCode;

    private static string LanguageCode
    {
        get
        {
            if (_languageCode == null)
            {
                _languageCode = ResolveLanguageCode();
            }
            return _languageCode;
        }
    }

    public static string Get(string key)
    {
        if (_strings.TryGetValue(LanguageCode, out Dictionary<string, string> table) &&
            table.TryGetValue(key, out string value))
        {
            return value;
        }

        // Fall back to Russian (the game's original/default language) if the active
        // language is missing this key, then finally the key itself so a typo is at
        // least visible instead of silently blank.
        if (_strings.TryGetValue("ru", out Dictionary<string, string> ru) &&
            ru.TryGetValue(key, out string ruValue))
        {
            return ruValue;
        }

        return key;
    }

    private static string ResolveLanguageCode()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Yandex.YandexGamesBridge.Instance != null)
        {
            string sdkLang = Yandex.YandexGamesBridge.Instance.GetLanguage();
            if (_strings.ContainsKey(sdkLang))
            {
                return sdkLang;
            }
        }
#endif

        // No Yandex SDK reporting a language (Editor, CrazyGames, any other host):
        // always English. This intentionally does NOT consult
        // Application.systemLanguage anymore - it used to, which meant a build
        // hosted anywhere other than Yandex still showed Russian for any player
        // whose OS/browser happened to be set to Russian, defeating the point of
        // shipping an English build for platforms that require one.
        return "en";
    }
}

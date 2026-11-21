using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimal string localization so the UI isn't hardcoded to Russian. Language is
/// picked once per session: from the Yandex Games SDK's reported language when
/// running on Yandex Games, falling back to Unity's Application.systemLanguage
/// elsewhere (Editor, other platforms), and finally to Russian if neither maps to
/// a known language.
///
/// Add more languages by adding another inner dictionary to _strings and another
/// case to ResolveLanguageCode(); every call site only ever asks for a key via
/// Localization.Get(...), so adding languages never touches gameplay code.
/// </summary>
public static class Localization
{
    public const string Score = "score_label";
    public const string Record = "record_label";
    public const string NewRecord = "new_record_label";

    public static string Get(string key)
    {
        if (key == Score)
            return "Score: ";

        else if (key == Record)
            return "Best: ";

        else if (key == NewRecord)
            return "New best: ";

        return key;
    }

    //    private static readonly Dictionary<string, Dictionary<string, string>> _strings =
    //        new Dictionary<string, Dictionary<string, string>>
    //        {
    //            ["ru"] = new Dictionary<string, string>
    //            {
    //                [Score] = "Очки: ",
    //                [Record] = "Рекорд: ",
    //                [NewRecord] = "Новый рекорд: ",
    //            },
    //            ["en"] = new Dictionary<string, string>
    //            {
    //                [Score] = "Score: ",
    //                [Record] = "Best: ",
    //                [NewRecord] = "New best: ",
    //            },
    //        };

    //    private static string _languageCode;

    //    private static string LanguageCode
    //    {
    //        get
    //        {
    //            if (_languageCode == null)
    //            {
    //                _languageCode = ResolveLanguageCode();
    //            }
    //            return _languageCode;
    //        }
    //    }

    //    public static string Get(string key)
    //    {
    //        if (_strings.TryGetValue(LanguageCode, out Dictionary<string, string> table) &&
    //            table.TryGetValue(key, out string value))
    //        {
    //            return value;
    //        }

    //        // Fall back to Russian (the game's original/default language) if the active
    //        // language is missing this key, then finally the key itself so a typo is at
    //        // least visible instead of silently blank.
    //        if (_strings.TryGetValue("ru", out Dictionary<string, string> ru) &&
    //            ru.TryGetValue(key, out string ruValue))
    //        {
    //            return ruValue;
    //        }

    //        return key;
    //    }

    //    private static string ResolveLanguageCode()
    //    {
    //#if UNITY_WEBGL && !UNITY_EDITOR
    //        if (Yandex.YandexGamesBridge.Instance != null)
    //        {
    //            string sdkLang = Yandex.YandexGamesBridge.Instance.GetLanguage();
    //            if (_strings.ContainsKey(sdkLang))
    //            {
    //                return sdkLang;
    //            }
    //        }
    //#endif

    //        switch (Application.systemLanguage)
    //        {
    //            case SystemLanguage.Russian:
    //                return "ru";
    //            default:
    //                return _strings.ContainsKey("en") ? "en" : "ru";
    //        }
    //    }
}

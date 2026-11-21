using UnityEngine;

/// <summary>
/// Minimal string localization. For now this always returns English - CrazyGames
/// (and anywhere else outside Yandex) requires English, and Yandex Games isn't
/// currently accepting new partner submissions, so there's no live benefit to
/// keeping a Russian table around right now.
///
/// If/when this project goes back to Yandex and Russian matters again, bring
/// back a per-language dictionary here (keyed by Score/Record/NewRecord) and
/// resolve it from Yandex.YandexGamesBridge.Instance.GetLanguage() when running
/// on WebGL outside the Editor, English otherwise. Every call site only ever
/// asks for a key via Localization.Get(...), so that change never touches
/// gameplay code.
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

        if (key == Record)
            return "Best: ";

        if (key == NewRecord)
            return "New best: ";

        return key;
    }
}
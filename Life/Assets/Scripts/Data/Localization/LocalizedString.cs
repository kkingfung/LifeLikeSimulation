#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// 対応言語
    /// </summary>
    public enum Language
    {
        Japanese,
        English,
        ChineseSimplified,
        ChineseTraditional,
        Korean,
        Spanish,
        French,
        German,
        Portuguese,
        Russian,
        Italian,
        Thai,
        Vietnamese,
        Indonesian
    }

    /// <summary>
    /// 言語ごとのテキストエントリ
    /// </summary>
    [Serializable]
    public class LocalizedEntry
    {
        [Tooltip("言語")]
        public Language language = Language.Japanese;

        [Tooltip("テキスト")]
        [TextArea(2, 10)]
        public string text = string.Empty;
    }

    /// <summary>
    /// ローカライズされた文字列
    /// 複数言語のテキストを保持
    /// </summary>
    [Serializable]
    public class LocalizedString
    {
        [Tooltip("ローカライズキー（オプション、外部ファイル参照用）")]
        public string key = string.Empty;

        [Tooltip("デフォルト言語")]
        public Language defaultLanguage = Language.Japanese;

        [Tooltip("各言語のテキスト")]
        public List<LocalizedEntry> entries = new();

        /// <summary>
        /// 指定した言語のテキストを取得
        /// </summary>
        public string GetText(Language language)
        {
            // 指定言語を検索
            var entry = entries.Find(e => e.language == language);
            if (entry != null && !string.IsNullOrEmpty(entry.text))
            {
                return entry.text;
            }

            // デフォルト言語にフォールバック
            var defaultEntry = entries.Find(e => e.language == defaultLanguage);
            if (defaultEntry != null && !string.IsNullOrEmpty(defaultEntry.text))
            {
                return defaultEntry.text;
            }

            // 最初のエントリにフォールバック
            if (entries.Count > 0 && !string.IsNullOrEmpty(entries[0].text))
            {
                return entries[0].text;
            }

            return string.Empty;
        }

        /// <summary>
        /// テキストを設定
        /// </summary>
        public void SetText(Language language, string text)
        {
            var entry = entries.Find(e => e.language == language);
            if (entry != null)
            {
                entry.text = text;
            }
            else
            {
                entries.Add(new LocalizedEntry
                {
                    language = language,
                    text = text
                });
            }
        }

        /// <summary>
        /// 指定言語のテキストが存在するか
        /// </summary>
        public bool HasLanguage(Language language)
        {
            var entry = entries.Find(e => e.language == language);
            return entry != null && !string.IsNullOrEmpty(entry.text);
        }

        /// <summary>
        /// デフォルト言語のテキストを取得（簡易アクセス）
        /// </summary>
        public override string ToString()
        {
            return GetText(defaultLanguage);
        }

        /// <summary>
        /// 日本語テキストを設定するコンストラクタ
        /// </summary>
        public static LocalizedString Create(string japaneseText, string? englishText = null)
        {
            var localized = new LocalizedString
            {
                defaultLanguage = Language.Japanese
            };
            localized.SetText(Language.Japanese, japaneseText);

            if (!string.IsNullOrEmpty(englishText))
            {
                localized.SetText(Language.English, englishText);
            }

            return localized;
        }
    }

    /// <summary>
    /// 字幕データ（タイムスタンプ付き）
    /// </summary>
    [Serializable]
    public class SubtitleEntry
    {
        [Tooltip("開始時間（秒）")]
        public float startTime;

        [Tooltip("終了時間（秒）")]
        public float endTime;

        [Tooltip("話者ID（発信者ID）")]
        public string speakerId = string.Empty;

        [Tooltip("字幕テキスト（ローカライズ対応）")]
        public LocalizedString text = new();

        [Tooltip("感情タグ（表示スタイルに影響）")]
        public EmotionalState emotion = EmotionalState.Neutral;
    }

    /// <summary>
    /// 字幕トラック（1つの通話/動画の字幕全体）
    /// </summary>
    [Serializable]
    public class SubtitleTrack
    {
        [Tooltip("字幕トラックID")]
        public string trackId = string.Empty;

        [Tooltip("字幕エントリのリスト")]
        public List<SubtitleEntry> entries = new();

        /// <summary>
        /// 指定時間の字幕を取得
        /// </summary>
        public SubtitleEntry? GetSubtitleAt(float time)
        {
            foreach (var entry in entries)
            {
                if (time >= entry.startTime && time <= entry.endTime)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// 指定時間範囲の字幕を取得
        /// </summary>
        public List<SubtitleEntry> GetSubtitlesInRange(float startTime, float endTime)
        {
            var result = new List<SubtitleEntry>();
            foreach (var entry in entries)
            {
                if (entry.startTime <= endTime && entry.endTime >= startTime)
                {
                    result.Add(entry);
                }
            }
            return result;
        }
    }
}

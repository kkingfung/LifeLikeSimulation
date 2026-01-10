#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// シナリオ固有のローカライズデータ
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenarioLocalization", menuName = "LifeLike/Localization/Scenario Localization")]
    public class ScenarioLocalizationData : ScriptableObject
    {
        [Header("シナリオ情報")]
        [Tooltip("対象シナリオID")]
        public string scenarioId = string.Empty;

        [Header("発信者名")]
        [Tooltip("発信者の表示名のローカライズ")]
        public List<LocalizationTableEntry> callerNames = new();

        [Header("通話テキスト")]
        [Tooltip("通話セグメントのダイアログテキスト")]
        public List<LocalizationTableEntry> dialogueTexts = new();

        [Header("応答テキスト")]
        [Tooltip("プレイヤー応答の表示テキスト")]
        public List<LocalizationTableEntry> responseTexts = new();

        [Header("証拠テキスト")]
        [Tooltip("証拠の内容テキスト")]
        public List<LocalizationTableEntry> evidenceTexts = new();

        [Header("エンディングテキスト")]
        [Tooltip("エンディングのタイトルと説明")]
        public List<LocalizationTableEntry> endingTexts = new();

        [Header("字幕トラック")]
        [Tooltip("通話の字幕データ")]
        public List<SubtitleTrackEntry> subtitleTracks = new();

        // キャッシュ
        private Dictionary<string, LocalizedString>? _allTextCache;
        private Dictionary<string, SubtitleTrack>? _subtitleCache;

        /// <summary>
        /// キーからテキストを取得
        /// </summary>
        public string GetText(string key, Language language)
        {
            BuildCacheIfNeeded();
            if (_allTextCache!.TryGetValue(key, out var localized))
            {
                return localized.GetText(language);
            }
            return key;
        }

        /// <summary>
        /// 字幕トラックを取得
        /// </summary>
        public SubtitleTrack? GetSubtitleTrack(string trackId)
        {
            BuildSubtitleCacheIfNeeded();
            return _subtitleCache!.TryGetValue(trackId, out var track) ? track : null;
        }

        private void BuildCacheIfNeeded()
        {
            if (_allTextCache != null) return;

            _allTextCache = new Dictionary<string, LocalizedString>();

            void AddEntries(List<LocalizationTableEntry> list)
            {
                foreach (var entry in list)
                {
                    if (!string.IsNullOrEmpty(entry.key))
                    {
                        _allTextCache[entry.key] = entry.value;
                    }
                }
            }

            AddEntries(callerNames);
            AddEntries(dialogueTexts);
            AddEntries(responseTexts);
            AddEntries(evidenceTexts);
            AddEntries(endingTexts);
        }

        private void BuildSubtitleCacheIfNeeded()
        {
            if (_subtitleCache != null) return;

            _subtitleCache = new Dictionary<string, SubtitleTrack>();
            foreach (var entry in subtitleTracks)
            {
                if (!string.IsNullOrEmpty(entry.trackId))
                {
                    _subtitleCache[entry.trackId] = entry.track;
                }
            }
        }

        public void ClearCache()
        {
            _allTextCache = null;
            _subtitleCache = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }

    /// <summary>
    /// 字幕トラックのエントリ
    /// </summary>
    [Serializable]
    public class SubtitleTrackEntry
    {
        [Tooltip("トラックID（通常はCallIDと同じ）")]
        public string trackId = string.Empty;

        [Tooltip("字幕トラック")]
        public SubtitleTrack track = new();
    }
}

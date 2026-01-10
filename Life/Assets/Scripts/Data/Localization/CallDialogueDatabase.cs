#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// 通話のダイアログデータベース（ScriptableObject）
    /// 1つの通話の全セグメント・全言語のテキストを管理
    /// </summary>
    [CreateAssetMenu(fileName = "NewCallDialogue", menuName = "LifeLike/Localization/Call Dialogue Database")]
    public class CallDialogueDatabase : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("対象の通話ID")]
        public string callId = string.Empty;

        [Tooltip("説明")]
        [TextArea(1, 3)]
        public string description = string.Empty;

        [Header("セグメントダイアログ")]
        [Tooltip("各セグメントのダイアログデータ")]
        public List<SegmentDialogue> segments = new();

        // キャッシュ
        private Dictionary<string, SegmentDialogue>? _segmentCache;

        /// <summary>
        /// 指定セグメントのダイアログを取得
        /// </summary>
        public SegmentDialogue? GetSegmentDialogue(string segmentId)
        {
            BuildCacheIfNeeded();
            return _segmentCache!.TryGetValue(segmentId, out var segment) ? segment : null;
        }

        /// <summary>
        /// 指定セグメントの字幕トラックを取得
        /// </summary>
        public SubtitleTrack? GetSubtitleTrack(string segmentId)
        {
            var segment = GetSegmentDialogue(segmentId);
            return segment?.ToSubtitleTrack();
        }

        /// <summary>
        /// 指定セグメント・指定行のテキストを取得
        /// </summary>
        public string GetLineText(string segmentId, int lineIndex, Language language)
        {
            var segment = GetSegmentDialogue(segmentId);
            if (segment == null || lineIndex < 0 || lineIndex >= segment.lines.Count)
            {
                return string.Empty;
            }
            return segment.lines[lineIndex].GetText(language);
        }

        /// <summary>
        /// 指定セグメントの全テキストを結合して取得（テキストモード用）
        /// </summary>
        public string GetFullDialogueText(string segmentId, Language language)
        {
            var segment = GetSegmentDialogue(segmentId);
            if (segment == null)
            {
                return string.Empty;
            }

            var texts = new List<string>();
            foreach (var line in segment.lines)
            {
                var text = line.GetText(language);
                if (!string.IsNullOrEmpty(text))
                {
                    texts.Add(text);
                }
            }

            return string.Join("\n", texts);
        }

        /// <summary>
        /// 応答のローカライズテキストを取得
        /// </summary>
        public string GetResponseDisplayText(string segmentId, string responseId, Language language)
        {
            var segment = GetSegmentDialogue(segmentId);
            var response = segment?.GetResponseLocalization(responseId);
            return response?.GetDisplayText(language) ?? string.Empty;
        }

        private void BuildCacheIfNeeded()
        {
            if (_segmentCache != null) return;

            _segmentCache = new Dictionary<string, SegmentDialogue>();
            foreach (var segment in segments)
            {
                if (!string.IsNullOrEmpty(segment.segmentId))
                {
                    _segmentCache[segment.segmentId] = segment;
                }
            }
        }

        public void ClearCache()
        {
            _segmentCache = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }
}

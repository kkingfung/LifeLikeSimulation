#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// ダイアログ行（1つのセリフ）
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("行ID（セグメントID_連番など）")]
        public string lineId = string.Empty;

        [Tooltip("話者ID（発信者ID）")]
        public string speakerId = string.Empty;

        [Tooltip("感情状態")]
        public EmotionalState emotion = EmotionalState.Neutral;

        [Tooltip("開始時間（秒）- 音声/動画用")]
        public float startTime = 0f;

        [Tooltip("終了時間（秒）- 音声/動画用")]
        public float endTime = 0f;

        [Header("ローカライズテキスト")]
        [TextArea(2, 5)]
        public string japanese = string.Empty;

        [TextArea(2, 5)]
        public string english = string.Empty;

        [TextArea(2, 5)]
        public string chineseSimplified = string.Empty;

        [TextArea(2, 5)]
        public string chineseTraditional = string.Empty;

        [TextArea(2, 5)]
        public string korean = string.Empty;

        /// <summary>
        /// 指定言語のテキストを取得
        /// </summary>
        public string GetText(Language language)
        {
            var text = language switch
            {
                Language.Japanese => japanese,
                Language.English => english,
                Language.ChineseSimplified => chineseSimplified,
                Language.ChineseTraditional => chineseTraditional,
                Language.Korean => korean,
                _ => english
            };

            // 空の場合は日本語にフォールバック
            if (string.IsNullOrEmpty(text))
            {
                text = japanese;
            }

            // それでも空なら英語
            if (string.IsNullOrEmpty(text))
            {
                text = english;
            }

            return text;
        }

        /// <summary>
        /// LocalizedStringに変換
        /// </summary>
        public LocalizedString ToLocalizedString()
        {
            var localized = new LocalizedString { defaultLanguage = Language.Japanese };

            if (!string.IsNullOrEmpty(japanese))
                localized.SetText(Language.Japanese, japanese);
            if (!string.IsNullOrEmpty(english))
                localized.SetText(Language.English, english);
            if (!string.IsNullOrEmpty(chineseSimplified))
                localized.SetText(Language.ChineseSimplified, chineseSimplified);
            if (!string.IsNullOrEmpty(chineseTraditional))
                localized.SetText(Language.ChineseTraditional, chineseTraditional);
            if (!string.IsNullOrEmpty(korean))
                localized.SetText(Language.Korean, korean);

            return localized;
        }

        /// <summary>
        /// SubtitleEntryに変換
        /// </summary>
        public SubtitleEntry ToSubtitleEntry()
        {
            return new SubtitleEntry
            {
                startTime = startTime,
                endTime = endTime,
                speakerId = speakerId,
                text = ToLocalizedString(),
                emotion = emotion
            };
        }
    }

    /// <summary>
    /// 応答選択肢のローカライズデータ
    /// </summary>
    [Serializable]
    public class ResponseLocalization
    {
        [Tooltip("応答ID（ResponseDataのresponseIdと一致させる）")]
        public string responseId = string.Empty;

        [Header("表示テキスト")]
        [TextArea(1, 3)]
        public string displayJapanese = string.Empty;

        [TextArea(1, 3)]
        public string displayEnglish = string.Empty;

        [TextArea(1, 3)]
        public string displayChineseSimplified = string.Empty;

        [TextArea(1, 3)]
        public string displayChineseTraditional = string.Empty;

        [TextArea(1, 3)]
        public string displayKorean = string.Empty;

        [Header("実際の発言（異なる場合のみ）")]
        [TextArea(1, 3)]
        public string actualJapanese = string.Empty;

        [TextArea(1, 3)]
        public string actualEnglish = string.Empty;

        /// <summary>
        /// 表示テキストを取得
        /// </summary>
        public string GetDisplayText(Language language)
        {
            var text = language switch
            {
                Language.Japanese => displayJapanese,
                Language.English => displayEnglish,
                Language.ChineseSimplified => displayChineseSimplified,
                Language.ChineseTraditional => displayChineseTraditional,
                Language.Korean => displayKorean,
                _ => displayEnglish
            };

            if (string.IsNullOrEmpty(text)) text = displayJapanese;
            if (string.IsNullOrEmpty(text)) text = displayEnglish;

            return text;
        }

        /// <summary>
        /// 表示テキストをLocalizedStringに変換
        /// </summary>
        public LocalizedString ToDisplayLocalizedString()
        {
            var localized = new LocalizedString { defaultLanguage = Language.Japanese };

            if (!string.IsNullOrEmpty(displayJapanese))
                localized.SetText(Language.Japanese, displayJapanese);
            if (!string.IsNullOrEmpty(displayEnglish))
                localized.SetText(Language.English, displayEnglish);
            if (!string.IsNullOrEmpty(displayChineseSimplified))
                localized.SetText(Language.ChineseSimplified, displayChineseSimplified);
            if (!string.IsNullOrEmpty(displayChineseTraditional))
                localized.SetText(Language.ChineseTraditional, displayChineseTraditional);
            if (!string.IsNullOrEmpty(displayKorean))
                localized.SetText(Language.Korean, displayKorean);

            return localized;
        }

        /// <summary>
        /// 実際の発言をLocalizedStringに変換
        /// </summary>
        public LocalizedString ToActualLocalizedString()
        {
            var localized = new LocalizedString { defaultLanguage = Language.Japanese };

            if (!string.IsNullOrEmpty(actualJapanese))
                localized.SetText(Language.Japanese, actualJapanese);
            if (!string.IsNullOrEmpty(actualEnglish))
                localized.SetText(Language.English, actualEnglish);

            return localized;
        }
    }

    /// <summary>
    /// 通話セグメントのダイアログデータ
    /// </summary>
    [Serializable]
    public class SegmentDialogue
    {
        [Tooltip("セグメントID（CallSegmentのsegmentIdと一致させる）")]
        public string segmentId = string.Empty;

        [Tooltip("このセグメントのダイアログ行")]
        public List<DialogueLine> lines = new();

        [Tooltip("このセグメントの応答選択肢のローカライズ")]
        public List<ResponseLocalization> responses = new();

        /// <summary>
        /// 字幕トラックに変換
        /// </summary>
        public SubtitleTrack ToSubtitleTrack()
        {
            var track = new SubtitleTrack { trackId = segmentId };
            foreach (var line in lines)
            {
                track.entries.Add(line.ToSubtitleEntry());
            }
            return track;
        }

        /// <summary>
        /// 指定IDの応答ローカライズを取得
        /// </summary>
        public ResponseLocalization? GetResponseLocalization(string responseId)
        {
            return responses.Find(r => r.responseId == responseId);
        }
    }

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

    /// <summary>
    /// シナリオ全体のダイアログデータベース（ScriptableObject）
    /// 複数の通話のダイアログをまとめて管理
    /// </summary>
    [CreateAssetMenu(fileName = "NewScenarioDialogue", menuName = "LifeLike/Localization/Scenario Dialogue Database")]
    public class ScenarioDialogueDatabase : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("対象のシナリオID")]
        public string scenarioId = string.Empty;

        [Tooltip("シナリオタイトル")]
        public LocalizedString title = new();

        [Tooltip("シナリオ説明")]
        public LocalizedString description = new();

        [Header("発信者名")]
        [Tooltip("発信者の表示名")]
        public List<CallerNameLocalization> callerNames = new();

        [Header("通話ダイアログ")]
        [Tooltip("各通話のダイアログデータベース")]
        public List<CallDialogueDatabase> callDialogues = new();

        [Header("エンディング")]
        [Tooltip("エンディングのローカライズ")]
        public List<EndingLocalization> endings = new();

        [Header("証拠")]
        [Tooltip("証拠のローカライズ")]
        public List<EvidenceLocalization> evidences = new();

        // キャッシュ
        private Dictionary<string, CallDialogueDatabase>? _callCache;
        private Dictionary<string, CallerNameLocalization>? _callerCache;
        private Dictionary<string, EndingLocalization>? _endingCache;
        private Dictionary<string, EvidenceLocalization>? _evidenceCache;

        /// <summary>
        /// 指定通話のダイアログデータベースを取得
        /// </summary>
        public CallDialogueDatabase? GetCallDialogue(string callId)
        {
            BuildCacheIfNeeded();
            return _callCache!.TryGetValue(callId, out var db) ? db : null;
        }

        /// <summary>
        /// 発信者の表示名を取得
        /// </summary>
        public string GetCallerDisplayName(string callerId, Language language)
        {
            BuildCacheIfNeeded();
            if (_callerCache!.TryGetValue(callerId, out var localization))
            {
                return localization.GetDisplayName(language);
            }
            return callerId;
        }

        /// <summary>
        /// エンディングタイトルを取得
        /// </summary>
        public string GetEndingTitle(string endingId, Language language)
        {
            BuildCacheIfNeeded();
            if (_endingCache!.TryGetValue(endingId, out var localization))
            {
                return localization.GetTitle(language);
            }
            return endingId;
        }

        /// <summary>
        /// 証拠の内容を取得
        /// </summary>
        public string GetEvidenceContent(string evidenceId, Language language)
        {
            BuildCacheIfNeeded();
            if (_evidenceCache!.TryGetValue(evidenceId, out var localization))
            {
                return localization.GetContent(language);
            }
            return evidenceId;
        }

        private void BuildCacheIfNeeded()
        {
            if (_callCache == null)
            {
                _callCache = new Dictionary<string, CallDialogueDatabase>();
                foreach (var db in callDialogues)
                {
                    if (db != null && !string.IsNullOrEmpty(db.callId))
                    {
                        _callCache[db.callId] = db;
                    }
                }
            }

            if (_callerCache == null)
            {
                _callerCache = new Dictionary<string, CallerNameLocalization>();
                foreach (var name in callerNames)
                {
                    if (!string.IsNullOrEmpty(name.callerId))
                    {
                        _callerCache[name.callerId] = name;
                    }
                }
            }

            if (_endingCache == null)
            {
                _endingCache = new Dictionary<string, EndingLocalization>();
                foreach (var ending in endings)
                {
                    if (!string.IsNullOrEmpty(ending.endingId))
                    {
                        _endingCache[ending.endingId] = ending;
                    }
                }
            }

            if (_evidenceCache == null)
            {
                _evidenceCache = new Dictionary<string, EvidenceLocalization>();
                foreach (var evidence in evidences)
                {
                    if (!string.IsNullOrEmpty(evidence.evidenceId))
                    {
                        _evidenceCache[evidence.evidenceId] = evidence;
                    }
                }
            }
        }

        public void ClearCache()
        {
            _callCache = null;
            _callerCache = null;
            _endingCache = null;
            _evidenceCache = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }

    /// <summary>
    /// 発信者名のローカライズ
    /// </summary>
    [Serializable]
    public class CallerNameLocalization
    {
        public string callerId = string.Empty;

        [Header("表示名")]
        public string displayNameJapanese = string.Empty;
        public string displayNameEnglish = string.Empty;
        public string displayNameChineseSimplified = string.Empty;
        public string displayNameChineseTraditional = string.Empty;
        public string displayNameKorean = string.Empty;

        [Header("本名（判明後）")]
        public string realNameJapanese = string.Empty;
        public string realNameEnglish = string.Empty;

        public string GetDisplayName(Language language)
        {
            var text = language switch
            {
                Language.Japanese => displayNameJapanese,
                Language.English => displayNameEnglish,
                Language.ChineseSimplified => displayNameChineseSimplified,
                Language.ChineseTraditional => displayNameChineseTraditional,
                Language.Korean => displayNameKorean,
                _ => displayNameEnglish
            };

            if (string.IsNullOrEmpty(text)) text = displayNameJapanese;
            if (string.IsNullOrEmpty(text)) text = displayNameEnglish;

            return text;
        }
    }

    /// <summary>
    /// エンディングのローカライズ
    /// </summary>
    [Serializable]
    public class EndingLocalization
    {
        public string endingId = string.Empty;

        [Header("タイトル")]
        public string titleJapanese = string.Empty;
        public string titleEnglish = string.Empty;
        public string titleChineseSimplified = string.Empty;
        public string titleChineseTraditional = string.Empty;
        public string titleKorean = string.Empty;

        [Header("説明")]
        [TextArea(2, 5)]
        public string descriptionJapanese = string.Empty;
        [TextArea(2, 5)]
        public string descriptionEnglish = string.Empty;

        public string GetTitle(Language language)
        {
            var text = language switch
            {
                Language.Japanese => titleJapanese,
                Language.English => titleEnglish,
                Language.ChineseSimplified => titleChineseSimplified,
                Language.ChineseTraditional => titleChineseTraditional,
                Language.Korean => titleKorean,
                _ => titleEnglish
            };

            if (string.IsNullOrEmpty(text)) text = titleJapanese;
            if (string.IsNullOrEmpty(text)) text = titleEnglish;

            return text;
        }
    }

    /// <summary>
    /// 証拠のローカライズ
    /// </summary>
    [Serializable]
    public class EvidenceLocalization
    {
        public string evidenceId = string.Empty;

        [Header("内容")]
        [TextArea(1, 3)]
        public string contentJapanese = string.Empty;
        [TextArea(1, 3)]
        public string contentEnglish = string.Empty;
        [TextArea(1, 3)]
        public string contentChineseSimplified = string.Empty;
        [TextArea(1, 3)]
        public string contentChineseTraditional = string.Empty;
        [TextArea(1, 3)]
        public string contentKorean = string.Empty;

        [Header("説明")]
        [TextArea(2, 4)]
        public string descriptionJapanese = string.Empty;
        [TextArea(2, 4)]
        public string descriptionEnglish = string.Empty;

        public string GetContent(Language language)
        {
            var text = language switch
            {
                Language.Japanese => contentJapanese,
                Language.English => contentEnglish,
                Language.ChineseSimplified => contentChineseSimplified,
                Language.ChineseTraditional => contentChineseTraditional,
                Language.Korean => contentKorean,
                _ => contentEnglish
            };

            if (string.IsNullOrEmpty(text)) text = contentJapanese;
            if (string.IsNullOrEmpty(text)) text = contentEnglish;

            return text;
        }
    }
}

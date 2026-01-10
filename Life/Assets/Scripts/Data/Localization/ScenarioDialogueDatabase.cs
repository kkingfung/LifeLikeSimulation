#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
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
}

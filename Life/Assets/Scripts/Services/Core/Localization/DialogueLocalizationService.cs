#nullable enable
using System.Collections.Generic;
using System.IO;
using LifeLike.Data.Localization;
using UnityEngine;

namespace LifeLike.Services.Core.Localization
{
    /// <summary>
    /// ダイアログ（ストーリーコンテンツ）のローカライズを管理するサービス
    /// </summary>
    public class DialogueLocalizationService : IDialogueLocalizationService
    {
        private readonly ILocalizationService _localizationService;
        private NightTranslationData? _currentNightData;
        private readonly Dictionary<string, NightTranslationData> _cachedNights = new();

        public Language CurrentLanguage => _localizationService.CurrentLanguage;
        public bool IsLoaded => _currentNightData != null;
        public string? CurrentScenarioId => _currentNightData?.scenarioId;

        public DialogueLocalizationService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// 夜の翻訳データを読み込む
        /// </summary>
        public bool LoadNightTranslation(string scenarioId)
        {
            // キャッシュをチェック
            if (_cachedNights.TryGetValue(scenarioId, out var cached))
            {
                _currentNightData = cached;
                Debug.Log($"[DialogueLocalizationService] キャッシュから翻訳データを読み込みました: {scenarioId}");
                return true;
            }

            // JSONファイルを読み込む
            string nightNumber = scenarioId.Replace("night", "").PadLeft(2, '0');
            string fileName = $"Night{nightNumber}_Translations";
            string resourcePath = $"Translations/{fileName}";

            // Resourcesから読み込みを試行
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                // Data/Translationsから直接読み込みを試行
                string dataPath = Path.Combine(Application.dataPath, "Data", "Translations", $"{fileName}.json");
                if (File.Exists(dataPath))
                {
                    try
                    {
                        string json = File.ReadAllText(dataPath);
                        _currentNightData = JsonUtility.FromJson<NightTranslationData>(json);
                        if (_currentNightData != null)
                        {
                            _cachedNights[scenarioId] = _currentNightData;
                            Debug.Log($"[DialogueLocalizationService] ファイルから翻訳データを読み込みました: {dataPath}");
                            return true;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[DialogueLocalizationService] 翻訳ファイルの読み込みに失敗: {e.Message}");
                    }
                }

                Debug.LogWarning($"[DialogueLocalizationService] 翻訳データが見つかりません: {scenarioId}");
                _currentNightData = null;
                return false;
            }

            try
            {
                _currentNightData = JsonUtility.FromJson<NightTranslationData>(textAsset.text);
                if (_currentNightData != null)
                {
                    _cachedNights[scenarioId] = _currentNightData;
                    Debug.Log($"[DialogueLocalizationService] Resourcesから翻訳データを読み込みました: {resourcePath}");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueLocalizationService] 翻訳データのパースに失敗: {e.Message}");
            }

            return false;
        }

        public CallTranslation? GetCallTranslation(string callId)
        {
            return _currentNightData?.GetCallTranslation(callId);
        }

        public CallerTranslation? GetCallerTranslation(string callerId)
        {
            return _currentNightData?.GetCallerTranslation(callerId);
        }

        public EvidenceTranslation? GetEvidenceTranslation(string evidenceId)
        {
            return _currentNightData?.GetEvidenceTranslation(evidenceId);
        }

        /// <summary>
        /// セグメントの発信者セリフを現在の言語で取得
        /// </summary>
        public List<string> GetCallerLines(string callId, string segmentId)
        {
            var result = new List<string>();

            var callTranslation = GetCallTranslation(callId);
            if (callTranslation == null) return result;

            var segmentTranslation = callTranslation.GetSegmentTranslation(segmentId);
            if (segmentTranslation == null) return result;

            foreach (var line in segmentTranslation.callerLines)
            {
                result.Add(line.GetText(CurrentLanguage));
            }

            return result;
        }

        /// <summary>
        /// 応答テキストを現在の言語で取得
        /// </summary>
        public string GetResponseText(string callId, string segmentId, string responseId)
        {
            var callTranslation = GetCallTranslation(callId);
            if (callTranslation == null) return string.Empty;

            var segmentTranslation = callTranslation.GetSegmentTranslation(segmentId);
            if (segmentTranslation == null) return string.Empty;

            var responseTranslation = segmentTranslation.responses.Find(r => r.responseId == responseId);
            if (responseTranslation == null) return string.Empty;

            return responseTranslation.displayText.GetText(CurrentLanguage);
        }

        /// <summary>
        /// 発信者名を現在の言語で取得
        /// </summary>
        public string GetCallerDisplayName(string callerId)
        {
            var callerTranslation = GetCallerTranslation(callerId);
            if (callerTranslation == null) return string.Empty;

            return callerTranslation.displayName.GetText(CurrentLanguage);
        }

        /// <summary>
        /// 証拠の内容を現在の言語で取得
        /// </summary>
        public string GetEvidenceContent(string evidenceId)
        {
            var evidenceTranslation = GetEvidenceTranslation(evidenceId);
            if (evidenceTranslation == null) return string.Empty;

            return evidenceTranslation.content.GetText(CurrentLanguage);
        }

        /// <summary>
        /// シナリオタイトルを現在の言語で取得
        /// </summary>
        public string GetScenarioTitle()
        {
            return _currentNightData?.title.GetText(CurrentLanguage) ?? string.Empty;
        }

        /// <summary>
        /// シナリオ説明を現在の言語で取得
        /// </summary>
        public string GetScenarioDescription()
        {
            return _currentNightData?.description.GetText(CurrentLanguage) ?? string.Empty;
        }
    }
}

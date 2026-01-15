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

            // 夜番号を抽出（"0", "1", "night01", "night_01" など様々な形式に対応）
            string nightNumber = ExtractNightNumber(scenarioId);
            string fileName = $"Night{nightNumber}_Translations";
            string resourcePath = $"Translations/{fileName}";

            Debug.Log($"[DialogueLocalizationService] 翻訳読み込み開始: scenarioId={scenarioId}, nightNumber={nightNumber}, resourcePath={resourcePath}");

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
                    Debug.Log($"[DialogueLocalizationService] 読み込みデータ - scenarioId: {_currentNightData.scenarioId}, calls: {_currentNightData.calls.Count}, callers: {_currentNightData.callers.Count}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[DialogueLocalizationService] JSONパース結果がnull: {resourcePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DialogueLocalizationService] 翻訳データのパースに失敗: {e.Message}\n{e.StackTrace}");
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
            if (callTranslation == null)
            {
                Debug.LogWarning($"[DialogueLocalizationService] GetCallerLines - callId '{callId}' が見つかりません (IsLoaded: {IsLoaded}, calls count: {_currentNightData?.calls.Count ?? 0})");
                return result;
            }

            var segmentTranslation = callTranslation.GetSegmentTranslation(segmentId);
            if (segmentTranslation == null)
            {
                Debug.LogWarning($"[DialogueLocalizationService] GetCallerLines - segmentId '{segmentId}' が見つかりません");
                return result;
            }

            foreach (var line in segmentTranslation.callerLines)
            {
                result.Add(line.GetText(CurrentLanguage));
            }

            Debug.Log($"[DialogueLocalizationService] GetCallerLines - 言語: {CurrentLanguage}, 行数: {result.Count}");
            return result;
        }

        /// <summary>
        /// 応答テキストを現在の言語で取得
        /// </summary>
        public string GetResponseText(string callId, string segmentId, string responseId)
        {
            var callTranslation = GetCallTranslation(callId);
            if (callTranslation == null)
            {
                Debug.LogWarning($"[DialogueLocalizationService] GetResponseText - callId '{callId}' not found");
                return string.Empty;
            }

            var segmentTranslation = callTranslation.GetSegmentTranslation(segmentId);
            if (segmentTranslation == null)
            {
                Debug.LogWarning($"[DialogueLocalizationService] GetResponseText - segmentId '{segmentId}' not found in call '{callId}'");
                return string.Empty;
            }

            var responseTranslation = segmentTranslation.responses.Find(r => r.responseId == responseId);
            if (responseTranslation == null)
            {
                Debug.LogWarning($"[DialogueLocalizationService] GetResponseText - responseId '{responseId}' not found in segment '{segmentId}' (available: {string.Join(", ", segmentTranslation.responses.ConvertAll(r => r.responseId))})");
                return string.Empty;
            }

            var result = responseTranslation.displayText.GetText(CurrentLanguage);
            Debug.Log($"[DialogueLocalizationService] GetResponseText - Language: {CurrentLanguage}, Result: '{result}'");
            return result;
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

        /// <summary>
        /// シナリオIDから夜番号を抽出（2桁ゼロパディング）
        /// 対応形式: "0", "1", "night01", "night_01", "night1"
        /// </summary>
        private string ExtractNightNumber(string scenarioId)
        {
            // 純粋な数値の場合（"0", "1", "2"...）
            if (int.TryParse(scenarioId, out int numericId))
            {
                // 0-based indexを1-basedに変換
                return (numericId + 1).ToString("D2");
            }

            // "night" プレフィックスを除去して数値を抽出
            string cleaned = scenarioId
                .Replace("night_", "")
                .Replace("night", "")
                .Trim();

            if (int.TryParse(cleaned, out int nightNum))
            {
                return nightNum.ToString("D2");
            }

            // フォールバック: そのまま2桁パディング
            return cleaned.PadLeft(2, '0');
        }
    }
}

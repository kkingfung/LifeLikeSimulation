#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LifeLike.Editor
{
    /// <summary>
    /// 翻訳データとシナリオデータの整合性を検証するエディターツール
    /// 各Nightの通話データにあるresponseIdが翻訳ファイルに存在するかチェックする
    /// </summary>
    public class TranslationValidator : EditorWindow
    {
        private readonly string[] _nightOptions = { "Night01", "Night02", "Night03", "Night04", "Night05", "Night06", "Night07", "Night08", "Night09", "Night10" };
        private bool[] _selectedNights = new bool[10];
        private Vector2 _scrollPosition;
        private string _validationReport = "";
        private bool _showOnlyErrors = true;
        private int _totalErrors = 0;
        private int _totalWarnings = 0;

        [MenuItem("LifeLike/Translation Validator")]
        public static void ShowWindow()
        {
            GetWindow<TranslationValidator>("Translation Validator");
        }

        private void OnEnable()
        {
            // デフォルトで全て選択
            for (int i = 0; i < _selectedNights.Length; i++)
            {
                _selectedNights[i] = true;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Translation Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Night選択
            GUILayout.Label("Select Nights to Validate:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                for (int i = 0; i < _selectedNights.Length; i++)
                    _selectedNights[i] = true;
            }
            if (GUILayout.Button("Deselect All"))
            {
                for (int i = 0; i < _selectedNights.Length; i++)
                    _selectedNights[i] = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 0; i < _nightOptions.Length; i++)
            {
                if (i == 5) // 5個ごとに改行
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                _selectedNights[i] = GUILayout.Toggle(_selectedNights[i], _nightOptions[i], GUILayout.Width(70));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            _showOnlyErrors = GUILayout.Toggle(_showOnlyErrors, "Show Only Errors/Warnings");
            GUILayout.Space(10);

            if (GUILayout.Button("Validate Translations", GUILayout.Height(40)))
            {
                ValidateAllSelectedNights();
            }

            GUILayout.Space(10);

            // サマリー表示
            if (!string.IsNullOrEmpty(_validationReport))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Errors: {_totalErrors}", _totalErrors > 0 ? EditorStyles.boldLabel : EditorStyles.label);
                GUILayout.Label($"Warnings: {_totalWarnings}", _totalWarnings > 0 ? EditorStyles.boldLabel : EditorStyles.label);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy to Clipboard"))
                {
                    GUIUtility.systemCopyBuffer = _validationReport;
                    Debug.Log("Validation report copied to clipboard.");
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);

            // 結果表示
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            GUILayout.TextArea(_validationReport, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
        }

        private void ValidateAllSelectedNights()
        {
            var report = new StringBuilder();
            _totalErrors = 0;
            _totalWarnings = 0;

            report.AppendLine("=== Translation Validation Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            for (int i = 0; i < _nightOptions.Length; i++)
            {
                if (_selectedNights[i])
                {
                    ValidateNight(_nightOptions[i], report);
                }
            }

            report.AppendLine();
            report.AppendLine("=== Summary ===");
            report.AppendLine($"Total Errors: {_totalErrors}");
            report.AppendLine($"Total Warnings: {_totalWarnings}");

            _validationReport = report.ToString();
        }

        private void ValidateNight(string nightId, StringBuilder report)
        {
            report.AppendLine($"--- {nightId} ---");

            string callsPath = $"Assets/Data/{nightId}/Calls";
            string translationPath = $"Assets/Resources/Translations/{nightId}_Translations.json";

            // 翻訳ファイルを読み込む
            if (!File.Exists(translationPath))
            {
                report.AppendLine($"  [ERROR] Translation file not found: {translationPath}");
                _totalErrors++;
                return;
            }

            string translationJson = File.ReadAllText(translationPath);
            var translationData = ParseTranslationFile(translationJson);

            if (translationData == null)
            {
                report.AppendLine($"  [ERROR] Failed to parse translation file: {translationPath}");
                _totalErrors++;
                return;
            }

            // 通話データフォルダを確認
            if (!Directory.Exists(callsPath))
            {
                report.AppendLine($"  [WARNING] Calls folder not found: {callsPath}");
                _totalWarnings++;
                return;
            }

            // 各通話JSONファイルを検証
            string[] callFiles = Directory.GetFiles(callsPath, "*.json");
            foreach (string callFile in callFiles)
            {
                ValidateCallFile(callFile, translationData, report);
            }

            report.AppendLine();
        }

        private void ValidateCallFile(string callFilePath, TranslationData translationData, StringBuilder report)
        {
            string fileName = Path.GetFileName(callFilePath);
            string callJson = File.ReadAllText(callFilePath);

            try
            {
                var callData = JsonUtility.FromJson<CallJsonData>(callJson);
                if (callData == null || callData.segments == null)
                {
                    report.AppendLine($"  [WARNING] Could not parse call file: {fileName}");
                    _totalWarnings++;
                    return;
                }

                // 翻訳データからこの通話のデータを取得
                var callTranslation = translationData.calls?.FirstOrDefault(c => c.callId == callData.callId);
                if (callTranslation == null)
                {
                    report.AppendLine($"  [ERROR] Call '{callData.callId}' not found in translation file");
                    _totalErrors++;
                    return;
                }

                // 各セグメントを検証
                foreach (var segment in callData.segments)
                {
                    ValidateSegment(callData.callId, segment, callTranslation, report);
                }
            }
            catch (Exception e)
            {
                report.AppendLine($"  [ERROR] Error parsing {fileName}: {e.Message}");
                _totalErrors++;
            }
        }

        private void ValidateSegment(string callId, SegmentJsonData segment, CallTranslationData callTranslation, StringBuilder report)
        {
            var segmentTranslation = callTranslation.segments?.FirstOrDefault(s => s.segmentId == segment.segmentId);
            if (segmentTranslation == null)
            {
                report.AppendLine($"  [ERROR] Segment '{segment.segmentId}' in call '{callId}' not found in translation");
                _totalErrors++;
                return;
            }

            // レスポンスを検証
            if (segment.responses != null)
            {
                foreach (var response in segment.responses)
                {
                    var responseTranslation = segmentTranslation.responses?.FirstOrDefault(r => r.responseId == response.responseId);
                    if (responseTranslation == null)
                    {
                        // 翻訳にあるresponseIdをリスト化
                        var availableIds = segmentTranslation.responses?.Select(r => r.responseId).ToList() ?? new List<string>();
                        string available = availableIds.Count > 0 ? string.Join(", ", availableIds) : "(none)";

                        report.AppendLine($"  [ERROR] Response '{response.responseId}' in segment '{segment.segmentId}' not found");
                        report.AppendLine($"         Available: {available}");
                        _totalErrors++;
                    }
                    else
                    {
                        // 各言語の翻訳が存在するか確認
                        ValidateLanguages(responseTranslation.displayText, response.responseId, segment.segmentId, report);
                    }
                }
            }

            // CallerLinesを検証（数が一致するか）
            if (segment.callerLines != null && segmentTranslation.callerLines != null)
            {
                if (segment.callerLines.Length != segmentTranslation.callerLines.Count)
                {
                    report.AppendLine($"  [WARNING] Segment '{segment.segmentId}': callerLines count mismatch (scenario: {segment.callerLines.Length}, translation: {segmentTranslation.callerLines.Count})");
                    _totalWarnings++;
                }
            }
        }

        private void ValidateLanguages(LocalizedText? text, string responseId, string segmentId, StringBuilder report)
        {
            if (text == null)
            {
                report.AppendLine($"  [ERROR] Response '{responseId}' in segment '{segmentId}' has no displayText");
                _totalErrors++;
                return;
            }

            var missingLanguages = new List<string>();
            if (string.IsNullOrEmpty(text.ja)) missingLanguages.Add("ja");
            if (string.IsNullOrEmpty(text.en)) missingLanguages.Add("en");
            if (string.IsNullOrEmpty(text.zh_CN)) missingLanguages.Add("zh_CN");
            if (string.IsNullOrEmpty(text.zh_TW)) missingLanguages.Add("zh_TW");
            if (string.IsNullOrEmpty(text.ko)) missingLanguages.Add("ko");

            if (missingLanguages.Count > 0 && !_showOnlyErrors)
            {
                report.AppendLine($"  [WARNING] Response '{responseId}' missing languages: {string.Join(", ", missingLanguages)}");
                _totalWarnings++;
            }
        }

        private TranslationData? ParseTranslationFile(string json)
        {
            try
            {
                return JsonUtility.FromJson<TranslationData>(json);
            }
            catch
            {
                return null;
            }
        }

        #region JSON Data Classes

        [Serializable]
        private class CallJsonData
        {
            public string callId = "";
            public string callerId = "";
            public SegmentJsonData[]? segments;
        }

        [Serializable]
        private class SegmentJsonData
        {
            public string segmentId = "";
            public string[]? callerLines;
            public ResponseJsonData[]? responses;
        }

        [Serializable]
        private class ResponseJsonData
        {
            public string responseId = "";
            public string text = "";
        }

        [Serializable]
        private class TranslationData
        {
            public string scenarioId = "";
            public List<CallTranslationData>? calls;
        }

        [Serializable]
        private class CallTranslationData
        {
            public string callId = "";
            public List<SegmentTranslationData>? segments;
        }

        [Serializable]
        private class SegmentTranslationData
        {
            public string segmentId = "";
            public List<LocalizedText>? callerLines;
            public List<ResponseTranslationData>? responses;
        }

        [Serializable]
        private class ResponseTranslationData
        {
            public string responseId = "";
            public LocalizedText? displayText;
        }

        [Serializable]
        private class LocalizedText
        {
            public string ja = "";
            public string en = "";
            public string zh_CN = "";
            public string zh_TW = "";
            public string ko = "";
        }

        #endregion
    }
}

#nullable enable
using System.Collections.Generic;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Services.EndState;
using LifeLike.Services.Flag;
using UnityEditor;
using UnityEngine;

namespace LifeLike.Editor
{
    /// <summary>
    /// Night01のエンドステート計算を統合テストするエディタツール
    /// 各エンディングへの到達パターンをシミュレートして検証
    /// </summary>
    public class EndStateIntegrationTest : EditorWindow
    {
        private NightFlagsDefinition? _flagDefinition;
        private EndStateDefinition? _endStateDefinition;
        private Vector2 _scrollPosition;
        private List<TestResult> _testResults = new();

        [MenuItem("LifeLike/EndState Integration Test")]
        public static void ShowWindow()
        {
            GetWindow<EndStateIntegrationTest>("EndState Integration Test");
        }

        private void OnGUI()
        {
            GUILayout.Label("EndState Integration Test", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // データアセットの選択
            _flagDefinition = (NightFlagsDefinition?)EditorGUILayout.ObjectField(
                "Flag Definition",
                _flagDefinition,
                typeof(NightFlagsDefinition),
                false);

            _endStateDefinition = (EndStateDefinition?)EditorGUILayout.ObjectField(
                "EndState Definition",
                _endStateDefinition,
                typeof(EndStateDefinition),
                false);

            GUILayout.Space(10);

            if (GUILayout.Button("Run All Tests"))
            {
                RunAllTests();
            }

            GUILayout.Space(10);

            // 結果表示
            if (_testResults.Count > 0)
            {
                GUILayout.Label("Test Results:", EditorStyles.boldLabel);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                foreach (var result in _testResults)
                {
                    var style = result.passed ? EditorStyles.label : EditorStyles.boldLabel;
                    var color = GUI.color;
                    GUI.color = result.passed ? Color.green : Color.red;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(result.passed ? "✓" : "✗", GUILayout.Width(20));
                    GUILayout.Label(result.testName, style);
                    GUILayout.EndHorizontal();

                    if (!result.passed)
                    {
                        GUI.color = Color.yellow;
                        GUILayout.Label($"  Expected: {result.expected}");
                        GUILayout.Label($"  Actual: {result.actual}");
                    }

                    GUI.color = color;
                }

                GUILayout.EndScrollView();

                // サマリー
                int passCount = _testResults.FindAll(r => r.passed).Count;
                GUILayout.Space(10);
                GUILayout.Label($"Passed: {passCount}/{_testResults.Count}");
            }
        }

        private void RunAllTests()
        {
            _testResults.Clear();

            if (_flagDefinition == null || _endStateDefinition == null)
            {
                Debug.LogError("[EndStateIntegrationTest] FlagDefinitionとEndStateDefinitionを設定してください。");
                return;
            }

            // 各エンディングパターンをテスト
            TestExposedSurvived();
            TestExposedDied();
            TestContainedSurvived();
            TestContainedDied();
            TestComplicitSurvived();
            TestComplicitDied();
            TestFlagged();
            TestAbsorbed();

            Debug.Log($"[EndStateIntegrationTest] テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
        }

        /// <summary>
        /// テスト: EXPOSED + Victim Survived → ending_truth_save
        /// パターン: 矛盾指摘 + 真実追求 + 派遣実行（早期）
        /// </summary>
        private void TestExposedSurvived()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Exposedに必要なフラグを設定
            // Disclosure >= 3
            flagService.SetFlag("shared_medical_history", 60); // weight: 1
            flagService.SetFlag("revealed_affair", 70);        // weight: 1
            flagService.SetFlag("karen_confession", 120);      // weight: 2

            // Evidence >= 2
            flagService.SetFlag("found_contradiction", 90);    // weight: 2

            // emergency_dispatched = true（早期）
            flagService.SetFlag("emergency_dispatched", 150);  // 02:30 - 生存可能時間内

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 150; // 02:30
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "EXPOSED + Victim Survived → ending_truth_save",
                expected = "ending_truth_save",
                actual = endingId,
                passed = endingId == "ending_truth_save"
            });
        }

        /// <summary>
        /// テスト: EXPOSED + Victim Died → ending_truth_late
        /// パターン: 矛盾指摘 + 真実追求 + 派遣実行（遅延）
        /// </summary>
        private void TestExposedDied()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Exposedに必要なフラグを設定
            flagService.SetFlag("shared_medical_history", 60);
            flagService.SetFlag("revealed_affair", 70);
            flagService.SetFlag("karen_confession", 120);
            flagService.SetFlag("found_contradiction", 90);

            // 派遣遅延（02:50以降）
            flagService.SetFlag("emergency_dispatched", 175);  // 02:55 - 生存不可能

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 175; // 02:55
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "EXPOSED + Victim Died → ending_truth_late",
                expected = "ending_truth_late",
                actual = endingId,
                passed = endingId == "ending_truth_late"
            });
        }

        /// <summary>
        /// テスト: CONTAINED + Victim Survived → ending_protocol_save
        /// パターン: 標準対応 + 早期派遣
        /// </summary>
        private void TestContainedSurvived()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Containedはデフォルト（他の条件を満たさない）
            // Escalation < 3, 派遣あり
            flagService.SetFlag("caller_reassured", 60);  // Reassurance
            flagService.SetFlag("emergency_dispatched", 140);  // 02:20 - 早期

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 140;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "CONTAINED + Victim Survived → ending_protocol_save",
                expected = "ending_protocol_save",
                actual = endingId,
                passed = endingId == "ending_protocol_save"
            });
        }

        /// <summary>
        /// テスト: CONTAINED + Victim Died → ending_protocol_fail
        /// パターン: 標準対応 + 派遣なし/遅延
        /// </summary>
        private void TestContainedDied()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 派遣なし
            flagService.SetFlag("caller_reassured", 60);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null; // 派遣なし
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "CONTAINED + Victim Died (no dispatch) → ending_protocol_fail",
                expected = "ending_protocol_fail",
                actual = endingId,
                passed = endingId == "ending_protocol_fail"
            });
        }

        /// <summary>
        /// テスト: COMPLICIT + Victim Survived → ending_complicit_save
        /// パターン: 隠蔽協力 + 派遣実行
        /// </summary>
        private void TestComplicitSurvived()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Complicitに必要なフラグを設定
            // Alignment >= 3
            flagService.SetFlag("agreed_to_cover", 100);       // weight: 2
            flagService.SetFlag("deleted_evidence", 110);      // weight: 2
            // Escalation < 2
            flagService.SetFlag("emergency_dispatched", 150);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 150;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "COMPLICIT + Victim Survived → ending_complicit_save",
                expected = "ending_complicit_save",
                actual = endingId,
                passed = endingId == "ending_complicit_save"
            });
        }

        /// <summary>
        /// テスト: COMPLICIT + Victim Died → ending_complicit_death
        /// パターン: 隠蔽協力 + 派遣なし
        /// </summary>
        private void TestComplicitDied()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Complicitに必要なフラグを設定
            flagService.SetFlag("agreed_to_cover", 100);
            flagService.SetFlag("deleted_evidence", 110);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "COMPLICIT + Victim Died → ending_complicit_death",
                expected = "ending_complicit_death",
                actual = endingId,
                passed = endingId == "ending_complicit_death"
            });
        }

        /// <summary>
        /// テスト: FLAGGED → ending_flagged
        /// パターン: 高エスカレーション
        /// </summary>
        private void TestFlagged()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Escalation >= 4
            flagService.SetFlag("threatened_caller", 60);      // weight: 2
            flagService.SetFlag("violated_protocol", 80);      // weight: 2
            flagService.SetFlag("emergency_dispatched", 90);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 90;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "FLAGGED (high escalation) → ending_flagged",
                expected = "ending_flagged",
                actual = endingId,
                passed = endingId == "ending_flagged"
            });
        }

        /// <summary>
        /// テスト: ABSORBED → ending_absorbed
        /// パターン: 最小介入 + 派遣なし
        /// </summary>
        private void TestAbsorbed()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 何も設定しない（最小介入）
            // 派遣なし

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "ABSORBED (minimal intervention) → ending_absorbed",
                expected = "ending_absorbed",
                actual = endingId,
                passed = endingId == "ending_absorbed"
            });
        }

        private class TestResult
        {
            public string testName = string.Empty;
            public string expected = string.Empty;
            public string actual = string.Empty;
            public bool passed;
        }
    }
}

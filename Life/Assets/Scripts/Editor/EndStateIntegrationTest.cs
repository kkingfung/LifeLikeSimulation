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

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Night01 Tests"))
            {
                RunAllTests();
            }
            if (GUILayout.Button("Run Night02 Tests"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight02Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night02テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Run Night03 Tests"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight03Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night03テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            GUILayout.EndHorizontal();

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
            TestComplicit();
            TestFlagged();
            TestAbsorbed();

            Debug.Log($"[EndStateIntegrationTest] テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
        }

        /// <summary>
        /// テスト: EXPOSED + Victim Survived → ending_dawn_light
        /// パターン: 情報開示 + エスカレーション + 派遣実行（早期）
        /// 条件: Disclosure >= 4, Escalation >= 3
        /// </summary>
        private void TestExposedSurvived()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Disclosure >= 4
            flagService.SetFlag("disclosed_to_driver", 60);         // weight: 3
            flagService.SetFlag("confirmed_location_to_driver", 70); // weight: 2

            // Escalation >= 3
            flagService.SetFlag("emergency_dispatched", 150);       // weight: 3, 02:30 - 生存可能時間内

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 150; // 02:30
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "EXPOSED + Victim Survived → ending_dawn_light",
                expected = "ending_dawn_light",
                actual = endingId,
                passed = endingId == "ending_dawn_light"
            });
        }

        /// <summary>
        /// テスト: EXPOSED + Victim Died → ending_weight_of_knowing
        /// パターン: 情報開示 + エスカレーション + 派遣実行（遅延）
        /// 条件: Disclosure >= 4, Escalation >= 3, dispatch > 169分
        /// </summary>
        private void TestExposedDied()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Disclosure >= 4
            flagService.SetFlag("disclosed_to_driver", 60);         // weight: 3
            flagService.SetFlag("confirmed_location_to_driver", 70); // weight: 2

            // Escalation >= 3 (派遣遅延)
            flagService.SetFlag("emergency_dispatched", 175);       // weight: 3, 02:55 - 生存不可能

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 175; // 02:55
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "EXPOSED + Victim Died → ending_weight_of_knowing",
                expected = "ending_weight_of_knowing",
                actual = endingId,
                passed = endingId == "ending_weight_of_knowing"
            });
        }

        /// <summary>
        /// テスト: CONTAINED + Victim Survived → ending_long_road
        /// パターン: 標準対応 + 早期派遣（他の条件を満たさない）
        /// </summary>
        private void TestContainedSurvived()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Containedはデフォルト（他の条件を満たさない）
            // 安心を与える（少量）
            flagService.SetFlag("reassurance_given", 60);           // weight: 1

            // 派遣あり（早期）
            flagService.SetFlag("emergency_dispatched", 140);       // weight: 3, 02:20 - 早期

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 140;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "CONTAINED + Victim Survived → ending_long_road",
                expected = "ending_long_road",
                actual = endingId,
                passed = endingId == "ending_long_road"
            });
        }

        /// <summary>
        /// テスト: CONTAINED + Victim Died → ending_just_another_night
        /// パターン: 標準対応 + 派遣なし/遅延
        /// CONTAINEDはデフォルト — 他の条件（ABSORBED含む）を満たさない場合
        /// ABSORBEDを避けるため: Reassurance > 2 または Disclosure > 1
        /// </summary>
        private void TestContainedDied()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // ABSORBEDを避けるためReassurance > 2 にする
            flagService.SetFlag("reassurance_given", 60);           // weight: 1
            flagService.SetFlag("early_reassurance", 70);           // weight: 1
            flagService.SetFlag("suggested_wait_family", 80);       // weight: 1
            // Reassurance = 3 (ABSORBEDの条件 <= 2 を満たさない)

            // Disclosure は低いまま
            flagService.SetFlag("footbridge_connection", 90);       // weight: 1
            // Disclosure = 1

            // 派遣なし → Escalation = 0

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null; // 派遣なし
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "CONTAINED + Victim Died (no dispatch) → ending_just_another_night",
                expected = "ending_just_another_night",
                actual = endingId,
                passed = endingId == "ending_just_another_night"
            });
        }

        /// <summary>
        /// テスト: COMPLICIT → ending_silent_accomplice
        /// パターン: 山田を保護 + 高いReassurance + 低いEscalation
        /// 条件: Reassurance >= 5, Escalation <= 0, yamada_protected = true
        /// </summary>
        private void TestComplicit()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Reassurance >= 5
            flagService.SetFlag("yamada_protected", 60);            // weight: 2
            flagService.SetFlag("offered_excuse", 70);              // weight: 2
            flagService.SetFlag("early_reassurance", 80);           // weight: 1

            // Escalation <= 0 (派遣なし、待機を提案)
            flagService.SetFlag("suggested_wait", 90);              // weight: -1

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "COMPLICIT → ending_silent_accomplice",
                expected = "ending_silent_accomplice",
                actual = endingId,
                passed = endingId == "ending_silent_accomplice"
            });
        }

        /// <summary>
        /// テスト: FLAGGED → ending_just_another_night_flagged
        /// パターン: 主任に対して防御的/否定的な対応
        /// 条件: Alignment <= -3 または supervisor_flagged = true
        /// </summary>
        private void TestFlagged()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // supervisor_flagged = true
            flagService.SetFlag("supervisor_flagged", 100);         // weight: -3

            // 派遣あり
            flagService.SetFlag("emergency_dispatched", 150);       // weight: 3

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = 150;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "FLAGGED (supervisor_flagged) → ending_just_another_night_flagged",
                expected = "ending_just_another_night_flagged",
                actual = endingId,
                passed = endingId == "ending_just_another_night_flagged"
            });
        }

        /// <summary>
        /// テスト: ABSORBED → ending_silence_weighs
        /// パターン: 最小介入 + 派遣なし
        /// 条件: Escalation = 0, Disclosure <= 1, Reassurance <= 2
        /// </summary>
        private void TestAbsorbed()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 何も設定しない（最小介入）
            // Escalation = 0, Disclosure <= 1, Reassurance <= 2

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            int? dispatchTime = null;
            string endingId = endStateService.DetermineEnding(dispatchTime);

            _testResults.Add(new TestResult
            {
                testName = "ABSORBED (minimal intervention) → ending_silence_weighs",
                expected = "ending_silence_weighs",
                actual = endingId,
                passed = endingId == "ending_silence_weighs"
            });
        }

        private class TestResult
        {
            public string testName = string.Empty;
            public string expected = string.Empty;
            public string actual = string.Empty;
            public bool passed;
        }

        #region Night02 Tests

        /// <summary>
        /// Night02テストを実行
        /// </summary>
        public void RunNight02Tests()
        {
            TestVigilant();
            TestCompliant();
            TestConnected();
            TestIsolated();
            TestRoutine();
        }

        /// <summary>
        /// テスト: VIGILANT → ending_watching_shadows
        /// 条件: Evidence >= 5, security_company_noted = true, explosion_noted = true
        /// </summary>
        private void TestVigilant()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Evidence >= 5
            flagService.SetFlag("security_company_noted", 60);      // weight: 3
            flagService.SetFlag("explosion_noted", 70);             // weight: 2
            // Evidence = 5

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "VIGILANT → ending_watching_shadows",
                expected = "ending_watching_shadows",
                actual = endingId,
                passed = endingId == "ending_watching_shadows"
            });
        }

        /// <summary>
        /// テスト: COMPLIANT → ending_good_employee
        /// 条件: Alignment >= 2, accepted_rewrite = true
        /// </summary>
        private void TestCompliant()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Alignment >= 2
            flagService.SetFlag("accepted_rewrite", 60);            // weight: 2

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "COMPLIANT → ending_good_employee",
                expected = "ending_good_employee",
                actual = endingId,
                passed = endingId == "ending_good_employee"
            });
        }

        /// <summary>
        /// テスト: CONNECTED → ending_threads_weaving
        /// 条件: Evidence >= 4, vehicles_match = true
        /// </summary>
        private void TestConnected()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Evidence >= 4
            flagService.SetFlag("clerk_vehicle_noted", 60);         // weight: 2
            flagService.SetFlag("explosion_noted", 70);             // weight: 2
            // Evidence = 4

            // vehicles_match = true
            flagService.SetFlag("vehicles_match", 80);              // Contradiction weight: 2

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "CONNECTED → ending_threads_weaving",
                expected = "ending_threads_weaving",
                actual = endingId,
                passed = endingId == "ending_threads_weaving"
            });
        }

        /// <summary>
        /// テスト: ISOLATED → ending_alone_in_dark
        /// 条件: Alignment <= -2, lied_to_supervisor = true
        /// </summary>
        private void TestIsolated()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Alignment <= -2
            flagService.SetFlag("lied_to_supervisor", 60);          // weight: -3

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "ISOLATED → ending_alone_in_dark",
                expected = "ending_alone_in_dark",
                actual = endingId,
                passed = endingId == "ending_alone_in_dark"
            });
        }

        /// <summary>
        /// テスト: ROUTINE → ending_another_night
        /// 条件: fire_reported = true (他の条件を満たさない)
        /// </summary>
        private void TestRoutine()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // fire_reported = true
            flagService.SetFlag("fire_reported", 60);               // weight: 0

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "ROUTINE → ending_another_night",
                expected = "ending_another_night",
                actual = endingId,
                passed = endingId == "ending_another_night"
            });
        }

        #endregion

        #region Night03 Tests

        /// <summary>
        /// Night03テストを実行
        /// </summary>
        public void RunNight03Tests()
        {
            TestCrossroads();
            TestIntervention();
            TestDisclosure();
            TestSilence();
        }

        /// <summary>
        /// テスト: CROSSROADS → ending_crossroads
        /// 条件: mari_saved = true, told_sato_about_haruka = true
        /// </summary>
        private void TestCrossroads()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            flagService.SetFlag("mari_saved", 30);
            flagService.SetFlag("told_sato_about_haruka", 60);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(30); // 早期派遣

            _testResults.Add(new TestResult
            {
                testName = "CROSSROADS → ending_crossroads",
                expected = "ending_crossroads",
                actual = endingId,
                passed = endingId == "ending_crossroads"
            });
        }

        /// <summary>
        /// テスト: INTERVENTION → ending_intervention
        /// 条件: mari_saved = true, refused_sato = true
        /// </summary>
        private void TestIntervention()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            flagService.SetFlag("mari_saved", 30);
            flagService.SetFlag("refused_sato", 60);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(30); // 早期派遣

            _testResults.Add(new TestResult
            {
                testName = "INTERVENTION → ending_intervention",
                expected = "ending_intervention",
                actual = endingId,
                passed = endingId == "ending_intervention"
            });
        }

        /// <summary>
        /// テスト: DISCLOSURE → ending_disclosure
        /// 条件: mari_not_saved = true, told_sato_about_haruka = true
        /// </summary>
        private void TestDisclosure()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            flagService.SetFlag("mari_not_saved", 60);
            flagService.SetFlag("told_sato_about_haruka", 70);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null); // 派遣なし

            _testResults.Add(new TestResult
            {
                testName = "DISCLOSURE → ending_disclosure",
                expected = "ending_disclosure",
                actual = endingId,
                passed = endingId == "ending_disclosure"
            });
        }

        /// <summary>
        /// テスト: SILENCE → ending_silence
        /// 条件: mari_not_saved = true, refused_sato = true
        /// </summary>
        private void TestSilence()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            flagService.SetFlag("mari_not_saved", 60);
            flagService.SetFlag("refused_sato", 70);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null); // 派遣なし

            _testResults.Add(new TestResult
            {
                testName = "SILENCE → ending_silence",
                expected = "ending_silence",
                actual = endingId,
                passed = endingId == "ending_silence"
            });
        }

        #endregion
    }
}

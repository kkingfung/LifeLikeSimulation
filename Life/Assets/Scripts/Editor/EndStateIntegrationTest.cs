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

            // Row 1: Night01-05
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Night01"))
            {
                RunAllTests();
            }
            if (GUILayout.Button("Night02"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight02Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night02テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night03"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight03Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night03テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night04"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight04Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night04テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night05"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight05Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night05テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            GUILayout.EndHorizontal();

            // Row 2: Night06-10
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Night06"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight06Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night06テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night07"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight07Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night07テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night08"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight08Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night08テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night09"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight09Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night09テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
                }
            }
            if (GUILayout.Button("Night10"))
            {
                _testResults.Clear();
                if (_flagDefinition != null && _endStateDefinition != null)
                {
                    RunNight10Tests();
                    Debug.Log($"[EndStateIntegrationTest] Night10テスト完了: {_testResults.FindAll(r => r.passed).Count}/{_testResults.Count} passed");
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

        #region Night04 Tests

        /// <summary>
        /// Night04テストを実行
        /// Night04は独立した2つのフラグ（Witness/Connected）で判定
        /// 4パターン: 両方あり、Witnessのみ、Connectedのみ、どちらもなし
        /// </summary>
        public void RunNight04Tests()
        {
            TestWitnessAndConnected();
            TestWitnessOnly();
            TestConnectedOnly();
            TestNeither();
        }

        /// <summary>
        /// テスト: Witness + Connected → ending_witness_connected
        /// 条件: witness_detailed_info = true, connected_nights = true
        /// パターン: 良いアプローチで詳細情報を得て、Night02との繋がりを警察に伝えた
        /// </summary>
        private void TestWitnessAndConnected()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 良いアプローチで詳細情報を得た
            flagService.SetFlag("good_approach", 60);
            flagService.SetFlag("caller_calmed", 65);
            flagService.SetFlag("evidence_body_details", 70);
            flagService.SetFlag("evidence_drag_marks", 75);
            flagService.SetFlag("evidence_car_details", 80);
            flagService.SetFlag("witness_detailed_info", 85);          // Witnessフラグ

            // 警察に正直に答え、Night02との繋がりを伝えた
            flagService.SetFlag("honest_with_police", 90);
            flagService.SetFlag("connected_nights", 95);               // Connectedフラグ
            flagService.SetFlag("told_police_connection", 100);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "WITNESS + CONNECTED → ending_witness_connected",
                expected = "ending_witness_connected",
                actual = endingId,
                passed = endingId == "ending_witness_connected"
            });
        }

        /// <summary>
        /// テスト: Witness のみ → ending_witness_only
        /// 条件: witness_detailed_info = true, connected_nights = false
        /// パターン: 詳細情報は得たが、繋がりは伝えなかった
        /// </summary>
        private void TestWitnessOnly()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 良いアプローチで詳細情報を得た
            flagService.SetFlag("good_approach", 60);
            flagService.SetFlag("caller_calmed", 65);
            flagService.SetFlag("evidence_body_details", 70);
            flagService.SetFlag("evidence_drag_marks", 75);
            flagService.SetFlag("evidence_car_details", 80);
            flagService.SetFlag("witness_detailed_info", 85);          // Witnessフラグ

            // 警察に正直に答えたが、繋がりは伝えなかった
            flagService.SetFlag("honest_with_police", 90);
            // connected_nights は設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "WITNESS only → ending_witness_only",
                expected = "ending_witness_only",
                actual = endingId,
                passed = endingId == "ending_witness_only"
            });
        }

        /// <summary>
        /// テスト: Connected のみ → ending_connected_only
        /// 条件: witness_detailed_info = false, connected_nights = true
        /// パターン: 詳細情報は少ないが、過去の知識からNight02との繋がりを伝えた
        /// </summary>
        private void TestConnectedOnly()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 悪いアプローチで詳細情報を得られなかった
            flagService.SetFlag("bad_approach", 60);
            flagService.SetFlag("caller_partial_calm", 65);
            flagService.SetFlag("location_confirmed", 70);
            // witness_detailed_info は設定しない

            // 警察に正直に答え、Night02との繋がりを伝えた（過去の記憶から）
            flagService.SetFlag("honest_with_police", 90);
            flagService.SetFlag("connected_nights", 95);               // Connectedフラグ

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "CONNECTED only → ending_connected_only",
                expected = "ending_connected_only",
                actual = endingId,
                passed = endingId == "ending_connected_only"
            });
        }

        /// <summary>
        /// テスト: どちらもなし → ending_neither
        /// 条件: witness_detailed_info = false, connected_nights = false
        /// パターン: 詳細情報を得られず、繋がりも伝えなかった
        /// </summary>
        private void TestNeither()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 悪いアプローチで詳細情報を得られなかった
            flagService.SetFlag("bad_approach", 60);
            flagService.SetFlag("caller_partial_calm", 65);
            flagService.SetFlag("location_confirmed", 70);
            // witness_detailed_info は設定しない

            // 警察に曖昧に答えた
            flagService.SetFlag("evasive_with_police", 90);
            // connected_nights は設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "NEITHER → ending_neither",
                expected = "ending_neither",
                actual = endingId,
                passed = endingId == "ending_neither"
            });
        }

        #endregion

        #region Night05 Tests

        /// <summary>
        /// Night05テストを実行
        /// Night05は情報収集量でエンドステートが決まる
        /// 3パターン: VoiceReached（多い）、VoiceDistant（中程度）、VoiceLost（少ない）
        /// </summary>
        public void RunNight05Tests()
        {
            TestVoiceReached();
            TestVoiceDistant();
            TestVoiceLost();
        }

        /// <summary>
        /// テスト: VoiceReached → ending_voice_reached
        /// 条件: 美咲から多くの情報を得て、調査も行い、デモの話も聞いた
        /// パターン: 情報収集を徹底した
        /// 必須: misaki_info_gathered = true, スコア >= 6
        /// スコア計算: learned_mari_name(2) + learned_yoshida_existence(1) + heard_mari_voice(2) +
        ///            heard_yoshida_name(1) + misaki_comforted(1) + noticed_no_info(1) +
        ///            listened_demo_story(2) + drug_connection(1) = 11
        /// </summary>
        private void TestVoiceReached()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 美咲からの情報収集（スコア計算対象）
            flagService.SetFlag("misaki_called", 30);
            flagService.SetFlag("learned_mari_name", 35);              // weight: 2
            flagService.SetFlag("learned_yoshida_existence", 40);     // weight: 1
            flagService.SetFlag("heard_mari_voice", 45);              // weight: 2
            flagService.SetFlag("heard_yoshida_name", 50);            // weight: 1
            flagService.SetFlag("misaki_comforted", 55);              // weight: 1
            // 小計: 7

            // 必須フラグ：美咲から十分な情報を集めた
            flagService.SetFlag("misaki_info_gathered", 56);          // 必須条件

            // 調査関連（スコア計算対象）
            flagService.SetFlag("researched_company", 60);
            flagService.SetFlag("noticed_no_info", 65);               // weight: 1
            // 小計: 8

            // デモ関連（スコア計算対象）
            flagService.SetFlag("demo_reported", 70);
            flagService.SetFlag("listened_demo_story", 75);           // weight: 2
            flagService.SetFlag("drug_connection", 80);               // weight: 1
            // 合計: 11

            // 組織トップの名前（スコア計算対象外）
            flagService.SetFlag("heard_boss_name", 85);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "VOICE_REACHED → ending_voice_reached",
                expected = "ending_voice_reached",
                actual = endingId,
                passed = endingId == "ending_voice_reached"
            });
        }

        /// <summary>
        /// テスト: VoiceDistant → ending_voice_distant
        /// 条件: 部分的に情報を得た
        /// パターン: 一部の情報のみ収集
        /// 必須: スコア 3-5（misaki_info_gathered なしでも可）
        /// スコア計算: learned_mari_name(2) + misaki_comforted(1) + learned_yoshida_existence(1) = 4
        /// </summary>
        private void TestVoiceDistant()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 美咲からの部分的な情報収集（スコア計算対象）
            flagService.SetFlag("misaki_called", 30);
            flagService.SetFlag("learned_mari_name", 35);              // weight: 2
            flagService.SetFlag("learned_yoshida_existence", 38);     // weight: 1
            flagService.SetFlag("misaki_comforted", 40);              // weight: 1
            // 合計: 4 (スコア範囲 3-5 に収まる)

            // heard_mari_voice は設定しない（真理の声を聞いていない）
            // misaki_info_gathered も設定しない

            // 調査は行わなかった
            // researched_company は設定しない

            // デモの通報だけ受けた（スコア計算対象外）
            flagService.SetFlag("demo_reported", 50);
            // listened_demo_story は設定しない（話を聞いていない）

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "VOICE_DISTANT → ending_voice_distant",
                expected = "ending_voice_distant",
                actual = endingId,
                passed = endingId == "ending_voice_distant"
            });
        }

        /// <summary>
        /// テスト: VoiceLost → ending_voice_lost
        /// 条件: ほとんど情報を得られなかった
        /// パターン: 美咲との会話を早く切り上げた
        /// 必須: スコア < 3 または他の条件を満たさない（デフォルト）
        /// スコア計算: misaki_called(0) + misaki_call_ended_early(-1) = -1
        /// </summary>
        private void TestVoiceLost()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 美咲からの電話を受けたが、すぐに切った
            flagService.SetFlag("misaki_called", 30);                  // weight: 0
            flagService.SetFlag("misaki_call_ended_early", 35);       // weight: -1
            // 合計: -1 (スコア < 3)

            // 他の情報は何も得ていない
            // デモも無視した（スコア計算対象外）
            flagService.SetFlag("demo_ignored", 50);                   // weight: 0

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "VOICE_LOST → ending_voice_lost",
                expected = "ending_voice_lost",
                actual = endingId,
                passed = endingId == "ending_voice_lost"
            });
        }

        #endregion

        #region Night06 Tests

        /// <summary>
        /// Night06テストを実行
        /// Night06は情報収集量と対策の有無でエンドステートが決まる
        /// 4パターン: StormPrepared（多い+対策）、StormAware（多い）、StormDistant（中程度）、StormUnaware（少ない）
        /// </summary>
        public void RunNight06Tests()
        {
            TestStormPrepared();
            TestStormAware();
            TestStormDistant();
            TestStormUnaware();
        }

        /// <summary>
        /// テスト: StormPrepared → ending_storm_prepared
        /// 条件: Evidenceスコア >= 6, patrol_dispatched_yoshida = true
        /// パターン: 多くの繋がりを見つけ、パトロールを派遣した
        /// スコア計算: heard_nakamura_claim(2) + nakamura_name_known(1) + heard_haruka_missing(2) +
        ///            yoshida_mentioned_misaki(2) + shinagawa_32_confirmed(2) = 9
        /// </summary>
        private void TestStormPrepared()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 中村の情報（Evidenceスコア計算対象）
            flagService.SetFlag("demo_argument_reported", 30);
            flagService.SetFlag("nakamura_calm", 35);
            flagService.SetFlag("heard_nakamura_claim", 40);              // weight: 2
            flagService.SetFlag("nakamura_name_known", 45);               // weight: 1
            // 小計: 3

            // 保育園からの情報（Evidenceスコア計算対象）
            flagService.SetFlag("kindergarten_contacted", 50);
            flagService.SetFlag("heard_haruka_missing", 55);              // weight: 2
            // 小計: 5

            // 吉田さんからの情報（Evidenceスコア計算対象）
            flagService.SetFlag("yoshida_reported_again", 60);
            flagService.SetFlag("yoshida_mentioned_misaki", 65);          // weight: 2
            flagService.SetFlag("shinagawa_32_confirmed", 70);            // weight: 2
            // 合計: 9

            // パトロール派遣（必須条件）
            flagService.SetFlag("patrol_dispatched_yoshida", 75);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "STORM_PREPARED → ending_storm_prepared",
                expected = "ending_storm_prepared",
                actual = endingId,
                passed = endingId == "ending_storm_prepared"
            });
        }

        /// <summary>
        /// テスト: StormAware → ending_storm_aware
        /// 条件: Evidenceスコア >= 4, パトロール派遣なし
        /// パターン: 繋がりに気づいたが、完全な対策は取れなかった
        /// スコア計算: heard_haruka_missing(2) + noticed_hayashi_name(2) + yoshida_knows_misaki(1) = 5
        /// </summary>
        private void TestStormAware()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 保育園からの情報（Evidenceスコア計算対象）
            flagService.SetFlag("kindergarten_contacted", 30);
            flagService.SetFlag("heard_haruka_missing", 35);              // weight: 2
            // 小計: 2

            // チャットイベントで気づいた（Evidenceスコア計算対象）
            flagService.SetFlag("noticed_hayashi_name", 40);              // weight: 2
            // 小計: 4

            // 吉田さんからの情報（Evidenceスコア計算対象）
            flagService.SetFlag("yoshida_reported_again", 50);
            flagService.SetFlag("yoshida_knows_misaki", 55);              // weight: 1
            // 合計: 5

            // パトロールは派遣していない
            // 吉田さんには真理に知らせるよう伝えた
            flagService.SetFlag("yoshida_will_warn_mari", 60);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "STORM_AWARE → ending_storm_aware",
                expected = "ending_storm_aware",
                actual = endingId,
                passed = endingId == "ending_storm_aware"
            });
        }

        /// <summary>
        /// テスト: StormDistant → ending_storm_distant
        /// 条件: Evidenceスコア 2-3
        /// パターン: 部分的な情報しか得られなかった
        /// スコア計算: heard_haruka_missing(2) = 2
        /// </summary>
        private void TestStormDistant()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 保育園からの情報のみ（Evidenceスコア計算対象）
            flagService.SetFlag("kindergarten_contacted", 30);
            flagService.SetFlag("heard_haruka_missing", 35);              // weight: 2
            // 合計: 2

            // 吉田さんからの通報は受けたが、詳細は聞かなかった
            flagService.SetFlag("yoshida_reported_again", 50);
            // yoshida_knows_misaki、shinagawa_32_confirmed は設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "STORM_DISTANT → ending_storm_distant",
                expected = "ending_storm_distant",
                actual = endingId,
                passed = endingId == "ending_storm_distant"
            });
        }

        /// <summary>
        /// テスト: StormUnaware → ending_storm_unaware
        /// 条件: Evidenceスコア < 2（デフォルト）
        /// パターン: ほとんど情報を得られなかった
        /// スコア計算: なし（全ての通話を最低限で処理）
        /// </summary>
        private void TestStormUnaware()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 口論の通報は受けたが、詳細は聞かなかった
            flagService.SetFlag("demo_argument_reported", 30);
            flagService.SetFlag("nakamura_left_alone", 35);               // weight: -1
            // Evidenceスコア対象外

            // 保育園からの通報は受けたが、もう少し連絡を試みるよう伝えた
            flagService.SetFlag("kindergarten_contacted", 40);
            flagService.SetFlag("kindergarten_will_try_again", 45);       // weight: -1
            // Evidenceスコア対象外

            // 吉田さんからの通報は様子見を指示
            flagService.SetFlag("yoshida_reported_again", 50);
            flagService.SetFlag("night06_ominous_end", 55);               // weight: -2
            // Evidenceスコア対象外

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "STORM_UNAWARE → ending_storm_unaware",
                expected = "ending_storm_unaware",
                actual = endingId,
                passed = endingId == "ending_storm_unaware"
            });
        }

        #endregion

        #region Night07 Tests

        /// <summary>
        /// Night07テストを実行
        /// Night07は美咲の運命と情報収集量でエンドステートが決まる
        /// 4パターン: MisakiProtected（安全+情報多い）、MisakiSafeUnaware（安全+情報少ない）、
        ///           MisakiTaken（連れ去られた）、CollapseWitnessed（運命不明）
        /// </summary>
        public void RunNight07Tests()
        {
            TestMisakiProtected();
            TestMisakiSafeUnaware();
            TestMisakiTaken();
            TestCollapseWitnessed();
        }

        /// <summary>
        /// テスト: MisakiProtected → ending_misaki_protected
        /// 条件: misaki_safe_at_yoshida = true, Evidenceスコア >= 6
        /// パターン: Night06で吉田が真理に警告、多くの証拠を集めた
        /// スコア計算: nakamura_hanging_confirmed(2) + room_ransacked_noted(2) + no_note_suspicious(2) +
        ///            mari_taken_witnessed(2) + shinagawa_32_abduction(3) = 11
        /// </summary>
        private void TestMisakiProtected()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night06からの継承フラグ
            flagService.SetFlag("yoshida_will_warn_mari", 0);
            flagService.SetFlag("misaki_safe_at_yoshida", 5);             // 必須条件

            // 大家からの通報（Evidenceスコア計算対象）
            flagService.SetFlag("landlord_reported", 30);
            flagService.SetFlag("nakamura_hanging_confirmed", 35);         // weight: 2
            flagService.SetFlag("room_ransacked_noted", 40);               // weight: 2
            flagService.SetFlag("no_note_suspicious", 45);                 // weight: 2
            // 小計: 6

            // 真理からの緊急電話
            flagService.SetFlag("mari_emergency_call", 50);
            flagService.SetFlag("mari_abducted", 55);

            // 吉田からの目撃報告（Evidenceスコア計算対象）
            flagService.SetFlag("yoshida_witness_call", 60);
            flagService.SetFlag("mari_taken_witnessed", 65);               // weight: 2
            flagService.SetFlag("shinagawa_32_abduction", 70);             // weight: 3
            // 合計: 11

            // 警察に通報
            flagService.SetFlag("police_notified_abduction", 75);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "MISAKI_PROTECTED → ending_misaki_protected",
                expected = "ending_misaki_protected",
                actual = endingId,
                passed = endingId == "ending_misaki_protected"
            });
        }

        /// <summary>
        /// テスト: MisakiSafeUnaware → ending_misaki_safe_unaware
        /// 条件: misaki_safe_at_yoshida = true, Evidenceスコア < 6
        /// パターン: Night06で吉田が真理に警告したが、情報収集が不十分
        /// スコア計算: nakamura_hanging_confirmed(2) + mari_taken_witnessed(2) = 4
        /// </summary>
        private void TestMisakiSafeUnaware()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night06からの継承フラグ
            flagService.SetFlag("yoshida_will_warn_mari", 0);
            flagService.SetFlag("misaki_safe_at_yoshida", 5);             // 必須条件

            // 大家からの通報（最低限の情報）
            flagService.SetFlag("landlord_reported", 30);
            flagService.SetFlag("nakamura_hanging_confirmed", 35);         // weight: 2
            flagService.SetFlag("police_dispatched_nakamura", 40);
            // 小計: 2

            // 真理からの緊急電話
            flagService.SetFlag("mari_emergency_call", 50);
            flagService.SetFlag("mari_abducted", 55);

            // 吉田からの目撃報告（部分的な情報）
            flagService.SetFlag("yoshida_witness_call", 60);
            flagService.SetFlag("mari_taken_witnessed", 65);               // weight: 2
            // 合計: 4

            // 警察への通報なし

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "MISAKI_SAFE_UNAWARE → ending_misaki_safe_unaware",
                expected = "ending_misaki_safe_unaware",
                actual = endingId,
                passed = endingId == "ending_misaki_safe_unaware"
            });
        }

        /// <summary>
        /// テスト: MisakiTaken → ending_misaki_taken
        /// 条件: misaki_taken = true
        /// パターン: Night06で吉田が真理に警告しなかった、美咲も連れ去られた
        /// </summary>
        private void TestMisakiTaken()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night06からの継承フラグ（警告なし）
            flagService.SetFlag("mari_unwarned", 0);

            // 大家からの通報
            flagService.SetFlag("landlord_reported", 30);
            flagService.SetFlag("nakamura_hanging_confirmed", 35);
            flagService.SetFlag("police_dispatched_nakamura", 40);

            // 真理からの途切れる電話
            flagService.SetFlag("mari_cutoff_call", 50);
            flagService.SetFlag("mari_mentioned_misaki", 55);
            flagService.SetFlag("misaki_with_mari_confirmed", 60);
            flagService.SetFlag("mari_taken", 65);
            flagService.SetFlag("misaki_taken", 70);                       // 必須条件（最悪の結果）

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "MISAKI_TAKEN → ending_misaki_taken",
                expected = "ending_misaki_taken",
                actual = endingId,
                passed = endingId == "ending_misaki_taken"
            });
        }

        /// <summary>
        /// テスト: CollapseWitnessed → ending_collapse_witnessed
        /// 条件: デフォルト（yoshida_will_warn_mari なし、misaki_taken なし）
        /// パターン: 真理は連れ去られたが、美咲が一緒だったかは不明
        /// </summary>
        private void TestCollapseWitnessed()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night06からの継承フラグ（警告なし）
            flagService.SetFlag("mari_unwarned", 0);

            // 大家からの通報
            flagService.SetFlag("landlord_reported", 30);
            flagService.SetFlag("nakamura_hanging_confirmed", 35);
            flagService.SetFlag("police_dispatched_nakamura", 40);

            // 真理からの途切れる電話（美咲については言及なし）
            flagService.SetFlag("mari_cutoff_call", 50);
            flagService.SetFlag("mari_taken", 55);
            // misaki_mentioned_misaki、misaki_with_mari_confirmed、misaki_taken は設定しない
            // 美咲がどこにいるかは不明

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "COLLAPSE_WITNESSED → ending_collapse_witnessed",
                expected = "ending_collapse_witnessed",
                actual = endingId,
                passed = endingId == "ending_collapse_witnessed"
            });
        }

        #endregion

        #region Night08 Tests

        /// <summary>
        /// Night08テストを実行
        /// Night08は警察への協力度、組織への対応、情報収集量で分岐
        /// 4パターン: TruthSeeker（逆らった+情報多い）、InformedCaution（従った+情報多い）、
        ///           SilentWitness（中立+情報中程度）、UnawareSurvivor（情報少ない）
        /// </summary>
        public void RunNight08Tests()
        {
            TestTruthSeeker();
            TestInformedCaution();
            TestSilentWitness();
            TestUnawareSurvivor();
        }

        /// <summary>
        /// テスト: TruthSeeker → ending_truth_seeker
        /// 条件: defied_organization = true, Evidenceスコア >= 8
        /// パターン: 警察に全面協力し、組織に逆らった
        /// スコア計算: told_police_mari_call(2) + told_police_voices(2) + told_police_plate(3) +
        ///            haruka_mari_sisters_confirmed(2) + kenji_knew_something(2) +
        ///            organization_mentioned_names(3) = 14
        /// </summary>
        private void TestTruthSeeker()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night07からの継承フラグ
            flagService.SetFlag("dispatched_to_mari_house", 0);
            flagService.SetFlag("operator_on_radar", 5);

            // 警察への情報提供（Evidenceスコア計算対象）
            flagService.SetFlag("confirmed_mari_call", 30);                // weight: 1
            flagService.SetFlag("told_police_mari_call", 35);              // weight: 2
            flagService.SetFlag("told_police_voices", 40);                 // weight: 2
            flagService.SetFlag("told_police_plate", 45);                  // weight: 3
            flagService.SetFlag("police_knows_connection", 50);            // weight: 3
            // 小計: 11

            // 吉田からの情報（Evidenceスコア計算対象）
            flagService.SetFlag("yoshida_info_call", 55);
            flagService.SetFlag("haruka_mari_sisters_confirmed", 60);      // weight: 2
            flagService.SetFlag("kenji_knew_something", 65);               // weight: 2
            // 小計: 15

            // 組織からの警告に逆らった
            flagService.SetFlag("received_warning_call", 70);
            flagService.SetFlag("felt_threatened", 75);
            flagService.SetFlag("organization_mentioned_names", 80);       // weight: 3
            flagService.SetFlag("defied_organization", 85);                // 必須条件
            // 合計: 18

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "TRUTH_SEEKER → ending_truth_seeker",
                expected = "ending_truth_seeker",
                actual = endingId,
                passed = endingId == "ending_truth_seeker"
            });
        }

        /// <summary>
        /// テスト: InformedCaution → ending_informed_caution
        /// 条件: submitted_to_warning = true, Evidenceスコア >= 6
        /// パターン: 多くの情報を得たが、組織には従った
        /// スコア計算: told_police_mari_call(2) + haruka_mari_sisters_confirmed(2) +
        ///            kenji_knew_something(2) + seiwaq_mentioned_by_mari(2) = 8
        /// </summary>
        private void TestInformedCaution()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night07からの継承フラグ
            flagService.SetFlag("dispatched_to_mari_house", 0);
            flagService.SetFlag("operator_on_radar", 5);

            // 警察への情報提供（部分的）
            flagService.SetFlag("confirmed_mari_call", 30);                // weight: 1
            flagService.SetFlag("told_police_mari_call", 35);              // weight: 2
            // 小計: 3

            // 吉田からの情報（Evidenceスコア計算対象）
            flagService.SetFlag("yoshida_info_call", 40);
            flagService.SetFlag("haruka_mari_sisters_confirmed", 45);      // weight: 2
            flagService.SetFlag("kenji_knew_something", 50);               // weight: 2
            flagService.SetFlag("seiwaq_mentioned_by_mari", 55);           // weight: 2
            // 小計: 9

            // 組織からの警告に従った
            flagService.SetFlag("received_warning_call", 60);
            flagService.SetFlag("accepted_warning", 65);
            flagService.SetFlag("submitted_to_warning", 70);               // 必須条件

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "INFORMED_CAUTION → ending_informed_caution",
                expected = "ending_informed_caution",
                actual = endingId,
                passed = endingId == "ending_informed_caution"
            });
        }

        /// <summary>
        /// テスト: SilentWitness → ending_silent_witness
        /// 条件: Evidenceスコア 4-7（中程度）
        /// パターン: 情報を得たが、警察にも組織にも協力的ではなかった
        /// スコア計算: confirmed_mari_call(1) + heard_about_sister(1) +
        ///            haruka_mari_sisters_confirmed(2) = 4
        /// </summary>
        private void TestSilentWitness()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 警察への情報提供（最低限）
            flagService.SetFlag("confirmed_mari_call", 30);                // weight: 1
            // 小計: 1

            // 吉田からの情報（部分的）
            flagService.SetFlag("yoshida_info_call", 40);
            flagService.SetFlag("heard_about_sister", 45);                 // weight: 1
            flagService.SetFlag("haruka_mari_sisters_confirmed", 50);      // weight: 2
            // 合計: 4

            // 組織からの警告はなし（dispatched_to_mari_houseなし）

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "SILENT_WITNESS → ending_silent_witness",
                expected = "ending_silent_witness",
                actual = endingId,
                passed = endingId == "ending_silent_witness"
            });
        }

        /// <summary>
        /// テスト: UnawareSurvivor → ending_unaware_survivor
        /// 条件: デフォルト（Evidenceスコア < 4）
        /// パターン: 情報をほとんど得ずに夜を終えた
        /// スコア計算: confirmed_mari_call(1) = 1
        /// </summary>
        private void TestUnawareSurvivor()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 警察への情報提供（最低限）
            flagService.SetFlag("confirmed_mari_call", 30);                // weight: 1
            // 合計: 1

            // 吉田からの電話は取らなかった
            // yoshida_info_call は設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "UNAWARE_SURVIVOR → ending_unaware_survivor",
                expected = "ending_unaware_survivor",
                actual = endingId,
                passed = endingId == "ending_unaware_survivor"
            });
        }

        #endregion

        #region Night09 Tests

        /// <summary>
        /// Night09テストを実行
        /// Night09はオペレーターの決断、告発者の運命、警察との同盟で分岐
        /// 5パターン: FullAlliance（完全な同盟）、ActiveAlliance（積極的な協力）、
        ///           WhistleblowerSaved（告発者を救った）、PassiveTruth（真実を知った沈黙）、
        ///           UncertainFuture（不確かな未来）
        /// </summary>
        public void RunNight09Tests()
        {
            TestFullAlliance();
            TestActiveAlliance();
            TestWhistleblowerSaved();
            TestPassiveTruth();
            TestUncertainFuture();
        }

        /// <summary>
        /// テスト: FullAlliance → ending_full_alliance
        /// 条件: police_alliance_formed = true, whistleblower_likely_safe = true, operator_will_tell_police = true
        /// パターン: 警察と同盟を結び、告発者を保護し、全てを話すことを決意
        /// </summary>
        private void TestFullAlliance()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night08からの継承フラグ
            flagService.SetFlag("police_knows_connection", 0);               // 警察が繋がりに気づいている

            // 警察との同盟
            flagService.SetFlag("police_update_call", 30);
            flagService.SetFlag("tracking_car", 35);
            flagService.SetFlag("encouraged_police", 40);
            flagService.SetFlag("police_ally", 45);
            flagService.SetFlag("police_alliance_formed", 50);               // 必須条件1

            // 吉田からの報告（美咲発見）
            flagService.SetFlag("yoshida_status_call", 55);
            flagService.SetFlag("operator_knows_misaki_with_yoshida", 60);
            flagService.SetFlag("yoshida_will_protect_confirmed", 65);

            // 告発者との通話
            flagService.SetFlag("whistleblower_call", 70);
            flagService.SetFlag("heard_whistleblower_identity", 75);
            flagService.SetFlag("kenji_murder_confirmed", 80);
            flagService.SetFlag("heard_coverup", 85);
            flagService.SetFlag("heard_subsidiary_truth", 90);
            flagService.SetFlag("told_whistleblower_police", 95);
            flagService.SetFlag("whistleblower_going_to_police", 100);
            flagService.SetFlag("whistleblower_likely_safe", 105);           // 必須条件2

            // オペレーターの決断
            flagService.SetFlag("operator_will_tell_police", 110);           // 必須条件3

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "FULL_ALLIANCE → ending_full_alliance",
                expected = "ending_full_alliance",
                actual = endingId,
                passed = endingId == "ending_full_alliance"
            });
        }

        /// <summary>
        /// テスト: ActiveAlliance → ending_active_alliance
        /// 条件: operator_will_tell_police = true（他の条件は部分的）
        /// パターン: 警察と協力し、全てを話すことを決意
        /// </summary>
        private void TestActiveAlliance()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 警察との通話（同盟なし）
            flagService.SetFlag("police_update_call", 30);
            flagService.SetFlag("tracking_car", 35);
            // police_alliance_formed は設定しない

            // 告発者との通話
            flagService.SetFlag("whistleblower_call", 50);
            flagService.SetFlag("heard_whistleblower_identity", 55);
            flagService.SetFlag("heard_coverup", 60);
            flagService.SetFlag("whistleblower_going_to_police_uncertain", 65);
            // whistleblower_likely_safe は設定しない（不確定）

            // オペレーターの決断
            flagService.SetFlag("operator_will_tell_police", 70);            // 必須条件

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "ACTIVE_ALLIANCE → ending_active_alliance",
                expected = "ending_active_alliance",
                actual = endingId,
                passed = endingId == "ending_active_alliance"
            });
        }

        /// <summary>
        /// テスト: WhistleblowerSaved → ending_whistleblower_saved
        /// 条件: whistleblower_likely_safe = true, told_whistleblower_police = true
        /// パターン: 告発者を警察に保護させることに成功
        /// </summary>
        private void TestWhistleblowerSaved()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night08からの継承フラグ
            flagService.SetFlag("police_knows_connection", 0);

            // 告発者との通話
            flagService.SetFlag("whistleblower_call", 50);
            flagService.SetFlag("heard_whistleblower_identity", 55);
            flagService.SetFlag("heard_coverup", 60);
            flagService.SetFlag("told_whistleblower_police", 65);            // 必須条件1
            flagService.SetFlag("whistleblower_going_to_police", 70);
            flagService.SetFlag("whistleblower_likely_safe", 75);            // 必須条件2

            // オペレーターの決断（沈黙を選ぶ）
            flagService.SetFlag("operator_will_stay_silent", 80);

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "WHISTLEBLOWER_SAVED → ending_whistleblower_saved",
                expected = "ending_whistleblower_saved",
                actual = endingId,
                passed = endingId == "ending_whistleblower_saved"
            });
        }

        /// <summary>
        /// テスト: PassiveTruth → ending_passive_truth
        /// 条件: operator_will_stay_silent = true, heard_coverup = true
        /// パターン: 多くの真実を知ったが、沈黙を選んだ
        /// </summary>
        private void TestPassiveTruth()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 告発者との通話（一部情報のみ）
            flagService.SetFlag("whistleblower_call", 50);
            flagService.SetFlag("heard_whistleblower_identity", 55);
            flagService.SetFlag("heard_coverup", 70);                        // 必須条件1
            // heard_subsidiary_truth は設定しない（TruthRevealedを避ける）

            // オペレーターの決断
            flagService.SetFlag("operator_will_stay_silent", 90);            // 必須条件2

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "PASSIVE_TRUTH → ending_passive_truth",
                expected = "ending_passive_truth",
                actual = endingId,
                passed = endingId == "ending_passive_truth"
            });
        }

        /// <summary>
        /// テスト: UncertainFuture → ending_uncertain_future
        /// 条件: デフォルト（告発者の運命不明、決定的な行動なし）
        /// パターン: 多くの情報を得たが、決定的な行動は取れなかった
        /// </summary>
        private void TestUncertainFuture()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // 警察との通話（最低限）
            flagService.SetFlag("police_update_call", 30);
            flagService.SetFlag("tracking_car", 35);

            // 告発者との通話
            flagService.SetFlag("whistleblower_call", 50);
            flagService.SetFlag("heard_whistleblower_identity", 55);
            flagService.SetFlag("whistleblower_fate_unknown", 60);           // 運命不明

            // オペレーターの決断なし（どちらも選ばない）
            // operator_will_tell_police も operator_will_stay_silent も設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "UNCERTAIN_FUTURE → ending_uncertain_future",
                expected = "ending_uncertain_future",
                actual = endingId,
                passed = endingId == "ending_uncertain_future"
            });
        }

        #endregion

        #region Night10 Tests

        /// <summary>
        /// Night10テストを実行
        /// Night10は美咲の運命とUSB発見で最終エンディングが決まる
        /// 4パターン: TruthDawn（ベスト）、InvestigationContinues（グッド）、
        ///           IntoDarkness（バッド）、UncertainDawn（ニュートラル）
        /// </summary>
        public void RunNight10Tests()
        {
            TestTruthDawn();
            TestInvestigationContinues();
            TestIntoDarkness();
            TestUncertainDawn();
        }

        /// <summary>
        /// テスト: TruthDawn → ending_truth_dawn
        /// 条件: misaki_saved = true, usb_can_be_found = true, whistleblower_contact_completed = true
        /// パターン: 美咲救出、USB発見、誠和製薬壊滅（ベストエンディング）
        /// </summary>
        private void TestTruthDawn()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night09からの継承フラグ
            flagService.SetFlag("police_alliance_formed", 0);
            flagService.SetFlag("operator_will_tell_police", 5);
            flagService.SetFlag("whistleblower_likely_safe", 10);
            flagService.SetFlag("operator_knows_misaki_with_yoshida", 15);

            // 真理の遺体発見
            flagService.SetFlag("mari_body_found", 30);
            flagService.SetFlag("mari_found_near_river", 35);
            flagService.SetFlag("mari_identity_confirmed", 40);
            flagService.SetFlag("mari_wounds_reported", 45);

            // 警察を吉田の家に派遣
            flagService.SetFlag("operator_choice_event", 50);
            flagService.SetFlag("sent_police_to_yoshida", 55);
            flagService.SetFlag("police_arrived_in_time", 60);
            flagService.SetFlag("yoshida_saved", 65);
            flagService.SetFlag("misaki_saved", 70);                            // 必須条件1

            // 内部告発者との通話でUSB情報を得た
            flagService.SetFlag("told_police_about_whistleblower", 75);
            flagService.SetFlag("contacted_whistleblower", 80);
            flagService.SetFlag("knows_usb_exists", 85);
            flagService.SetFlag("knows_usb_method", 90);
            flagService.SetFlag("knows_usb_content", 95);
            flagService.SetFlag("whistleblower_contact_completed", 100);        // 必須条件2
            flagService.SetFlag("usb_can_be_found", 105);                       // 必須条件3

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "TRUTH_DAWN → ending_truth_dawn",
                expected = "ending_truth_dawn",
                actual = endingId,
                passed = endingId == "ending_truth_dawn"
            });
        }

        /// <summary>
        /// テスト: InvestigationContinues → ending_investigation_continues
        /// 条件: misaki_saved = true, usb_can_be_found = false
        /// パターン: 美咲救出、USB未発見、捜査継続（グッドエンディング）
        /// </summary>
        private void TestInvestigationContinues()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night09からの継承フラグ
            flagService.SetFlag("police_alliance_formed", 0);
            flagService.SetFlag("operator_will_tell_police", 5);
            flagService.SetFlag("operator_knows_misaki_with_yoshida", 10);
            // whistleblower_likely_safe は設定しない（告発者が安全ではない）

            // 真理の遺体発見
            flagService.SetFlag("mari_body_found", 30);
            flagService.SetFlag("mari_identity_confirmed", 35);

            // 警察を吉田の家に派遣
            flagService.SetFlag("sent_police_to_yoshida", 40);
            flagService.SetFlag("police_arrived_in_time", 45);
            flagService.SetFlag("yoshida_saved", 50);
            flagService.SetFlag("misaki_saved", 55);                            // 必須条件

            // 内部告発者との通話なし（usb_can_be_found を得られない）
            // whistleblower_contact_completed は設定しない

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "INVESTIGATION_CONTINUES → ending_investigation_continues",
                expected = "ending_investigation_continues",
                actual = endingId,
                passed = endingId == "ending_investigation_continues"
            });
        }

        /// <summary>
        /// テスト: IntoDarkness → ending_into_darkness
        /// 条件: misaki_dead = true
        /// パターン: 美咲死亡、隠蔽成功（バッドエンディング）
        /// </summary>
        private void TestIntoDarkness()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night09からの継承フラグ（警察との協力なし）
            flagService.SetFlag("operator_will_stay_silent", 5);

            // 真理の遺体発見
            flagService.SetFlag("mari_body_found", 30);
            flagService.SetFlag("mari_identity_confirmed", 35);

            // 警察を派遣しなかった
            flagService.SetFlag("did_not_send_police", 40);
            flagService.SetFlag("yoshida_dead", 45);
            flagService.SetFlag("misaki_dead", 50);                             // 必須条件（最悪の結果）

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "INTO_DARKNESS → ending_into_darkness",
                expected = "ending_into_darkness",
                actual = endingId,
                passed = endingId == "ending_into_darkness"
            });
        }

        /// <summary>
        /// テスト: UncertainDawn → ending_uncertain_dawn
        /// 条件: misaki_fate_unknown = true
        /// パターン: 美咲の運命不明（ニュートラルエンディング）
        /// </summary>
        private void TestUncertainDawn()
        {
            var flagService = new FlagService();
            flagService.Initialize(_flagDefinition!.nightId, _flagDefinition!);

            // Night09からの継承フラグ（部分的な情報）
            flagService.SetFlag("operator_knows_misaki_with_yoshida", 5);

            // 真理の遺体発見
            flagService.SetFlag("mari_body_found", 30);
            flagService.SetFlag("mari_identity_confirmed", 35);

            // 警察を派遣したが間に合わなかった
            flagService.SetFlag("sent_police_to_yoshida", 40);
            flagService.SetFlag("police_arrived_too_late", 45);
            flagService.SetFlag("yoshida_dead", 50);
            flagService.SetFlag("misaki_fate_unknown", 55);                     // 必須条件（美咲の運命不明）

            var endStateService = new EndStateService(flagService);
            endStateService.Initialize(_endStateDefinition!);

            string endingId = endStateService.DetermineEnding(null);

            _testResults.Add(new TestResult
            {
                testName = "UNCERTAIN_DAWN → ending_uncertain_dawn",
                expected = "ending_uncertain_dawn",
                actual = endingId,
                passed = endingId == "ending_uncertain_dawn"
            });
        }

        #endregion
    }
}

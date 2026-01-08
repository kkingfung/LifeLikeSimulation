#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using LifeLike.Data;
using LifeLike.Data.Conditions;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Data.Localization;
using UnityEditor;
using UnityEngine;

namespace LifeLike.Editor
{
    /// <summary>
    /// 汎用シナリオデータインポーター（Night01/Night02共通）
    /// </summary>
    public class NightDataImporter : EditorWindow
    {
        private string _selectedNight = "Night01";
        private readonly string[] _nightOptions = { "Night01", "Night02" };
        private int _selectedNightIndex = 0;

        private string JsonDataPath => $"Assets/Data/{_selectedNight}";
        private string OutputPath => $"Assets/ScriptableObjects/{_selectedNight}";

        [MenuItem("LifeLike/Night Data Importer")]
        public static void ShowWindow()
        {
            GetWindow<NightDataImporter>("Night Data Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Night Data Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Night選択
            GUILayout.Label("Select Night:", EditorStyles.boldLabel);
            _selectedNightIndex = GUILayout.SelectionGrid(_selectedNightIndex, _nightOptions, 2);
            _selectedNight = _nightOptions[_selectedNightIndex];

            GUILayout.Space(10);
            GUILayout.Label($"JSON Data Path: {JsonDataPath}");
            GUILayout.Label($"Output Path: {OutputPath}");
            GUILayout.Space(10);

            if (GUILayout.Button("Import All Data", GUILayout.Height(40)))
            {
                ImportAllData();
            }

            GUILayout.Space(10);

            GUILayout.Label("Individual Import:", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Callers"))
            {
                ImportCallers();
            }
            if (GUILayout.Button("Flags"))
            {
                ImportFlagDefinitions();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("EndState"))
            {
                ImportEndStateDefinition();
            }
            if (GUILayout.Button("Scenario"))
            {
                ImportScenario();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Import Calls"))
            {
                ImportCalls();
            }
        }

        private void ImportAllData()
        {
            EnsureOutputDirectories();

            ImportCallers();
            ImportFlagDefinitions();
            ImportEndStateDefinition();
            ImportScenario();
            ImportCalls();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NightDataImporter] {_selectedNight}のすべてのデータをインポートしました。");
        }

        private void EnsureOutputDirectories()
        {
            CreateDirectoryIfNotExists("Assets/ScriptableObjects");
            CreateDirectoryIfNotExists(OutputPath);
            CreateDirectoryIfNotExists($"{OutputPath}/Callers");
            CreateDirectoryIfNotExists($"{OutputPath}/Calls");
        }

        private void CreateDirectoryIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "";
                string folderName = Path.GetFileName(path);

                if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
                {
                    CreateDirectoryIfNotExists(parentPath);
                }

                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        #region Callers Import

        private void ImportCallers()
        {
            string jsonPath = $"{JsonDataPath}/{_selectedNight}_Callers.json";
            string jsonContent = ReadJsonFile(jsonPath);

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError($"[NightDataImporter] ファイルが見つかりません: {jsonPath}");
                return;
            }

            var callersData = JsonUtility.FromJson<CallersJsonWrapper>(jsonContent);
            if (callersData?.callers == null)
            {
                Debug.LogError("[NightDataImporter] Callersデータのパースに失敗しました。");
                return;
            }

            EnsureOutputDirectories();

            foreach (var callerJson in callersData.callers)
            {
                var callerAsset = CreateOrLoadAsset<CallerData>($"{OutputPath}/Callers/{callerJson.callerId}.asset");

                callerAsset.callerId = callerJson.callerId;
                callerAsset.displayName = callerJson.displayName;
                callerAsset.realName = callerJson.realName;
                callerAsset.description = callerJson.description;
                callerAsset.voiceDescription = callerJson.voiceDescription;
                callerAsset.voicePitch = callerJson.voicePitch;

                if (callerJson.personality != null)
                {
                    callerAsset.personality = new CallerPersonality
                    {
                        honesty = callerJson.personality.honesty,
                        stability = callerJson.personality.stability,
                        cooperation = callerJson.personality.cooperation,
                        aggression = callerJson.personality.aggression
                    };
                }

                callerAsset.hiddenInfo = callerJson.hiddenInfo;
                callerAsset.trueMotivation = callerJson.trueMotivation;

                // Relationsをパース
                callerAsset.relations = new List<CallerRelation>();
                if (callerJson.relations != null)
                {
                    foreach (var relationJson in callerJson.relations)
                    {
                        callerAsset.relations.Add(new CallerRelation
                        {
                            targetCallerId = relationJson.targetCallerId,
                            relationType = ParseRelationType(relationJson.relationType),
                            description = relationJson.description,
                            isKnown = relationJson.isKnown
                        });
                    }
                }

                EditorUtility.SetDirty(callerAsset);
                Debug.Log($"[NightDataImporter] Caller作成: {callerJson.callerId}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[NightDataImporter] {callersData.callers.Count}人のCallerをインポートしました。");
        }

        #endregion

        #region Flag Definitions Import

        private void ImportFlagDefinitions()
        {
            string jsonPath = $"{JsonDataPath}/{_selectedNight}_FlagDefinitions.json";
            string jsonContent = ReadJsonFile(jsonPath);

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError($"[NightDataImporter] ファイルが見つかりません: {jsonPath}");
                return;
            }

            var flagsData = JsonUtility.FromJson<FlagDefinitionsJsonWrapper>(jsonContent);
            if (flagsData?.flagDefinitions == null)
            {
                Debug.LogError("[NightDataImporter] FlagDefinitionsデータのパースに失敗しました。");
                return;
            }

            EnsureOutputDirectories();

            var flagsAsset = CreateOrLoadAsset<NightFlagsDefinition>($"{OutputPath}/{_selectedNight}_Flags.asset");
            flagsAsset.nightId = flagsData.nightId;
            flagsAsset.flagDefinitions = new List<FlagDefinition>();
            flagsAsset.mutualExclusionRules = new List<MutualExclusionRule>();

            foreach (var flagJson in flagsData.flagDefinitions)
            {
                var flagDef = new FlagDefinition
                {
                    flagId = flagJson.flagId,
                    category = ParseFlagCategory(flagJson.category),
                    description = flagJson.description,
                    weight = flagJson.weight,
                    persistsAcrossNights = flagJson.persistsAcrossNights
                };

                flagsAsset.flagDefinitions.Add(flagDef);
            }

            if (flagsData.mutualExclusionRules != null)
            {
                foreach (var ruleJson in flagsData.mutualExclusionRules)
                {
                    var rule = new MutualExclusionRule
                    {
                        whenFlagSet = ruleJson.whenFlagSet,
                        cancelFlags = ruleJson.cancelFlags ?? new List<string>()
                    };

                    flagsAsset.mutualExclusionRules.Add(rule);
                }
            }

            EditorUtility.SetDirty(flagsAsset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[NightDataImporter] {flagsAsset.flagDefinitions.Count}個のフラグ定義をインポートしました。");
        }

        private FlagCategory ParseFlagCategory(string category)
        {
            return category switch
            {
                "Reassurance" => FlagCategory.Reassurance,
                "Disclosure" => FlagCategory.Disclosure,
                "Escalation" => FlagCategory.Escalation,
                "Alignment" => FlagCategory.Alignment,
                "Evidence" => FlagCategory.Evidence,
                "Contradiction" => FlagCategory.Contradiction,
                "Foreshadowing" => FlagCategory.Foreshadowing,
                "Event" => FlagCategory.Event,
                "Dispatch" => FlagCategory.Dispatch,
                _ => FlagCategory.Event
            };
        }

        private RelationType ParseRelationType(string type)
        {
            return type switch
            {
                "Stranger" => RelationType.Stranger,
                "Acquaintance" => RelationType.Acquaintance,
                "Friend" => RelationType.Friend,
                "Family" => RelationType.Family,
                "Colleague" => RelationType.Colleague,
                "Lover" => RelationType.Lover,
                "ExLover" => RelationType.ExLover,
                "Rival" => RelationType.Rival,
                "Enemy" => RelationType.Enemy,
                "Accomplice" => RelationType.Accomplice,
                "Victim" => RelationType.Victim,
                "Suspect" => RelationType.Suspect,
                "Neighbor" => RelationType.Neighbor,
                _ => RelationType.Stranger
            };
        }

        #endregion

        #region EndState Definition Import

        private void ImportEndStateDefinition()
        {
            string jsonPath = $"{JsonDataPath}/{_selectedNight}_EndStateDefinition.json";
            string jsonContent = ReadJsonFile(jsonPath);

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError($"[NightDataImporter] ファイルが見つかりません: {jsonPath}");
                return;
            }

            var endStateData = JsonUtility.FromJson<EndStateDefinitionJsonWrapper>(jsonContent);
            if (endStateData == null)
            {
                Debug.LogError("[NightDataImporter] EndStateDefinitionデータのパースに失敗しました。");
                return;
            }

            EnsureOutputDirectories();

            var endStateAsset = CreateOrLoadAsset<EndStateDefinition>($"{OutputPath}/{_selectedNight}_EndState.asset");
            endStateAsset.nightId = endStateData.nightId;
            endStateAsset.defaultEndState = ParseEndStateType(endStateData.defaultEndState);
            endStateAsset.defaultEndingId = endStateData.defaultEndingId;

            // EndState Conditions
            endStateAsset.conditions = new List<EndStateCondition>();
            if (endStateData.endStateConditions != null)
            {
                foreach (var condJson in endStateData.endStateConditions)
                {
                    var condition = new EndStateCondition
                    {
                        endStateType = ParseEndStateType(condJson.endStateType),
                        priority = condJson.priority,
                        description = condJson.description,
                        scoreConditions = new List<ScoreCondition>(),
                        flagConditions = new List<FlagCondition>()
                    };

                    if (condJson.scoreConditions != null)
                    {
                        foreach (var scoreJson in condJson.scoreConditions)
                        {
                            condition.scoreConditions.Add(new ScoreCondition
                            {
                                category = ParseFlagCategory(scoreJson.category),
                                comparison = ParseComparisonOperator(scoreJson.comparison),
                                value = scoreJson.value
                            });
                        }
                    }

                    if (condJson.flagConditions != null)
                    {
                        foreach (var flagJson in condJson.flagConditions)
                        {
                            condition.flagConditions.Add(new FlagCondition
                            {
                                flagId = flagJson.variableName,
                                requiredValue = flagJson.boolValue
                            });
                        }
                    }

                    endStateAsset.conditions.Add(condition);
                }
            }

            // Ending Mappings
            endStateAsset.endingMappings = new List<EndingMapping>();
            if (endStateData.endingMappings != null)
            {
                foreach (var mappingJson in endStateData.endingMappings)
                {
                    endStateAsset.endingMappings.Add(new EndingMapping
                    {
                        endState = ParseEndStateType(mappingJson.endState),
                        endingIdIfSurvived = mappingJson.endingIdIfSurvived,
                        endingIdIfDied = mappingJson.endingIdIfDied,
                        endingIdRegardless = mappingJson.endingIdRegardless
                    });
                }
            }

            // Victim Survival
            if (endStateData.victimSurvival != null)
            {
                endStateAsset.victimSurvival = new VictimSurvivalCondition
                {
                    requiresDispatch = endStateData.victimSurvival.requiresDispatch,
                    maxDispatchTimeMinutes = endStateData.victimSurvival.maxDispatchTimeMinutes,
                    dispatchFlagId = endStateData.victimSurvival.dispatchFlagId
                };
            }

            // Endings
            endStateAsset.endings = new List<ScenarioEnding>();
            if (endStateData.endings != null)
            {
                foreach (var endingJson in endStateData.endings)
                {
                    endStateAsset.endings.Add(new ScenarioEnding
                    {
                        endingId = endingJson.endingId,
                        title = endingJson.title,
                        description = endingJson.description,
                        endingType = ParseEndingType(endingJson.endingType)
                    });
                }
            }

            EditorUtility.SetDirty(endStateAsset);
            AssetDatabase.SaveAssets();
            Debug.Log($"[NightDataImporter] EndStateDefinitionをインポートしました。");
        }

        private EndStateType ParseEndStateType(string type)
        {
            return type switch
            {
                "Contained" => EndStateType.Contained,
                "Exposed" => EndStateType.Exposed,
                "Complicit" => EndStateType.Complicit,
                "Flagged" => EndStateType.Flagged,
                "Absorbed" => EndStateType.Absorbed,
                // Night02追加
                "Vigilant" => EndStateType.Vigilant,
                "Compliant" => EndStateType.Compliant,
                "Connected" => EndStateType.Connected,
                "Isolated" => EndStateType.Isolated,
                "Routine" => EndStateType.Routine,
                _ => EndStateType.Contained
            };
        }

        private ComparisonOperator ParseComparisonOperator(string op)
        {
            return op switch
            {
                "Equal" => ComparisonOperator.Equal,
                "NotEqual" => ComparisonOperator.NotEqual,
                "GreaterThan" => ComparisonOperator.GreaterThan,
                "GreaterThanOrEqual" => ComparisonOperator.GreaterThanOrEqual,
                "LessThan" => ComparisonOperator.LessThan,
                "LessThanOrEqual" => ComparisonOperator.LessThanOrEqual,
                _ => ComparisonOperator.Equal
            };
        }

        private EndingType ParseEndingType(string type)
        {
            return type switch
            {
                "EveryoneSaved" => EndingType.EveryoneSaved,
                "SomeoneSaved" => EndingType.SomeoneSaved,
                "NooneSaved" => EndingType.NooneSaved,
                "BecameAccomplice" => EndingType.BecameAccomplice,
                "TruthRevealed" => EndingType.TruthRevealed,
                "Neutral" => EndingType.Neutral,
                "DamageMinimized" => EndingType.DamageMinimized,
                "SomeoneAbandoned" => EndingType.SomeoneAbandoned,
                "CoverUpSucceeded" => EndingType.CoverUpSucceeded,
                "JusticeServed" => EndingType.JusticeServed,
                // Night02追加
                "Vigilant" => EndingType.Vigilant,
                "Compliant" => EndingType.Compliant,
                "Connected" => EndingType.Connected,
                "Isolated" => EndingType.Isolated,
                "Routine" => EndingType.Routine,
                _ => EndingType.Neutral
            };
        }

        #endregion

        #region Scenario Import

        private void ImportScenario()
        {
            string jsonPath = $"{JsonDataPath}/{_selectedNight}_Scenario.json";
            string jsonContent = ReadJsonFile(jsonPath);

            if (string.IsNullOrEmpty(jsonContent))
            {
                Debug.LogError($"[NightDataImporter] ファイルが見つかりません: {jsonPath}");
                return;
            }

            var scenarioData = JsonUtility.FromJson<ScenarioJsonWrapper>(jsonContent);
            if (scenarioData == null)
            {
                Debug.LogError("[NightDataImporter] Scenarioデータのパースに失敗しました。");
                return;
            }

            EnsureOutputDirectories();

            var scenarioAsset = CreateOrLoadAsset<NightScenarioData>($"{OutputPath}/{_selectedNight}_Scenario.asset");
            scenarioAsset.scenarioId = scenarioData.scenarioId;
            scenarioAsset.title = scenarioData.title;
            scenarioAsset.description = scenarioData.description;
            scenarioAsset.startTimeMinutes = scenarioData.startTimeMinutes;
            scenarioAsset.endTimeMinutes = scenarioData.endTimeMinutes;
            scenarioAsset.realSecondsPerGameMinute = scenarioData.realSecondsPerGameMinute;
            scenarioAsset.difficulty = scenarioData.difficulty;
            scenarioAsset.estimatedPlayTimeMinutes = scenarioData.estimatedPlayTimeMinutes;
            scenarioAsset.theTruth = scenarioData.theTruth;

            // Callerアセットをロード
            var callerAssets = new Dictionary<string, CallerData>();
            var callerGuids = AssetDatabase.FindAssets("t:CallerData", new[] { $"{OutputPath}/Callers" });
            foreach (var guid in callerGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var caller = AssetDatabase.LoadAssetAtPath<CallerData>(path);
                if (caller != null)
                {
                    callerAssets[caller.callerId] = caller;
                }
            }

            // Callアセットをロード
            var callAssets = new Dictionary<string, CallData>();
            var callGuids = AssetDatabase.FindAssets("t:CallData", new[] { $"{OutputPath}/Calls" });
            foreach (var guid in callGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var call = AssetDatabase.LoadAssetAtPath<CallData>(path);
                if (call != null)
                {
                    callAssets[call.callId] = call;
                }
            }

            // callScheduleからCallDataの時刻情報を更新
            scenarioAsset.calls = new List<CallData>();
            scenarioAsset.callers = new List<CallerData>();

            if (scenarioData.callSchedule != null)
            {
                foreach (var scheduleJson in scenarioData.callSchedule)
                {
                    if (callAssets.TryGetValue(scheduleJson.callId, out var callAsset))
                    {
                        callAsset.incomingTimeMinutes = scheduleJson.incomingTimeMinutes;
                        callAsset.ringDuration = scheduleJson.ringDuration;
                        callAsset.priority = scheduleJson.priority;
                        callAsset.isCritical = scheduleJson.isCritical;
                        EditorUtility.SetDirty(callAsset);

                        scenarioAsset.calls.Add(callAsset);

                        // Callerも追加
                        if (callerAssets.TryGetValue(scheduleJson.callerId, out var callerAsset))
                        {
                            if (!scenarioAsset.callers.Contains(callerAsset))
                            {
                                scenarioAsset.callers.Add(callerAsset);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[NightDataImporter] CallData '{scheduleJson.callId}' が見つかりません。先にCallsをインポートしてください。");
                    }
                }
            }

            // WorldStatesをパース
            scenarioAsset.initialWorldStates = new List<WorldStateSnapshot>();
            if (scenarioData.initialWorldStates != null)
            {
                foreach (var worldStateJson in scenarioData.initialWorldStates)
                {
                    scenarioAsset.initialWorldStates.Add(new WorldStateSnapshot
                    {
                        stateId = worldStateJson.stateId,
                        description = worldStateJson.description,
                        timeMinutes = worldStateJson.timeMinutes,
                        involvedCallerIds = worldStateJson.involvedCallerIds ?? new List<string>(),
                        isKnownToPlayer = worldStateJson.isKnownToPlayer
                    });
                }
            }

            // EvidenceTemplatesをパース
            // Note: EvidenceTemplateはScriptableObjectなので、新規インスタンスはCreateInstanceで作成
            scenarioAsset.evidenceTemplates = new List<EvidenceTemplate>();
            if (scenarioData.evidenceTemplates != null)
            {
                CreateDirectoryIfNotExists($"{OutputPath}/Evidence");

                foreach (var evidenceJson in scenarioData.evidenceTemplates)
                {
                    var evidenceAsset = CreateOrLoadAsset<EvidenceTemplate>($"{OutputPath}/Evidence/{evidenceJson.evidenceId}.asset");
                    evidenceAsset.data = new EvidenceData
                    {
                        evidenceId = evidenceJson.evidenceId,
                        title = evidenceJson.title,
                        content = evidenceJson.content,
                        evidenceType = ParseEvidenceType(evidenceJson.category)
                    };
                    EditorUtility.SetDirty(evidenceAsset);
                    scenarioAsset.evidenceTemplates.Add(evidenceAsset);
                }
            }

            EditorUtility.SetDirty(scenarioAsset);
            AssetDatabase.SaveAssets();
            Debug.Log("[NightDataImporter] Scenarioをインポートしました。");
        }

        private EvidenceType ParseEvidenceType(string category)
        {
            return category switch
            {
                "Time" => EvidenceType.Timestamp,
                "Location" => EvidenceType.Location,
                "Description" => EvidenceType.Statement,
                "Incident" => EvidenceType.Physical,
                "Testimony" => EvidenceType.Statement,
                "Document" => EvidenceType.Statement,
                "Physical" => EvidenceType.Physical,
                "Timestamp" => EvidenceType.Timestamp,
                "Statement" => EvidenceType.Statement,
                "Contradiction" => EvidenceType.Contradiction,
                "Silence" => EvidenceType.Silence,
                "Emotion" => EvidenceType.Emotion,
                "Relationship" => EvidenceType.Relationship,
                _ => EvidenceType.Statement
            };
        }

        #endregion

        #region Calls Import

        private void ImportCalls()
        {
            string callsPath = $"{JsonDataPath}/Calls";

            if (!Directory.Exists(callsPath))
            {
                Debug.LogError($"[NightDataImporter] Callsディレクトリが見つかりません: {callsPath}");
                return;
            }

            EnsureOutputDirectories();

            var callFiles = Directory.GetFiles(callsPath, "*.json");
            int importedCount = 0;

            // 先にCallerアセットを読み込む
            var callerAssets = new Dictionary<string, CallerData>();
            var callerGuids = AssetDatabase.FindAssets("t:CallerData", new[] { $"{OutputPath}/Callers" });
            foreach (var guid in callerGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var caller = AssetDatabase.LoadAssetAtPath<CallerData>(path);
                if (caller != null)
                {
                    callerAssets[caller.callerId] = caller;
                }
            }

            foreach (var filePath in callFiles)
            {
                string jsonContent = File.ReadAllText(filePath);
                var callData = JsonUtility.FromJson<CallJsonWrapper>(jsonContent);

                if (callData == null)
                {
                    Debug.LogWarning($"[NightDataImporter] パースに失敗: {filePath}");
                    continue;
                }

                var callAsset = CreateOrLoadAsset<CallData>($"{OutputPath}/Calls/{callData.callId}.asset");

                callAsset.callId = callData.callId;
                callAsset.description = callData.description;

                // Callerを参照
                if (callerAssets.TryGetValue(callData.callerId, out var caller))
                {
                    callAsset.caller = caller;
                }

                // Segmentsをパース
                callAsset.segments = new List<CallSegment>();

                // startSegmentIdがなければ最初のセグメントIDを使用
                if (!string.IsNullOrEmpty(callData.startSegmentId))
                {
                    callAsset.startSegmentId = callData.startSegmentId;
                }
                else if (callData.segments != null && callData.segments.Count > 0)
                {
                    callAsset.startSegmentId = callData.segments[0].segmentId;
                }

                if (callData.segments != null)
                {
                    foreach (var segJson in callData.segments)
                    {
                        var segment = new CallSegment
                        {
                            segmentId = segJson.segmentId,
                            responses = new List<ResponseData>()
                        };

                        // Caller linesをmediaに変換
                        if (segJson.callerLines != null && segJson.callerLines.Count > 0)
                        {
                            string combinedDialogue = string.Join("\n", segJson.callerLines);
                            segment.media = new CallMediaReference
                            {
                                mediaType = CallMediaType.SilhouetteText,
                                dialogueText = LocalizedString.Create(combinedDialogue)
                            };
                        }

                        // Responses
                        if (segJson.responses != null)
                        {
                            foreach (var respJson in segJson.responses)
                            {
                                var response = new ResponseData
                                {
                                    responseId = respJson.responseId,
                                    displayText = LocalizedString.Create(respJson.text),
                                    nextSegmentId = respJson.nextSegmentId ?? string.Empty,
                                    endsCall = respJson.nextSegmentId == null || segJson.isEnding,
                                    setFlags = respJson.setFlags ?? new List<string>(),
                                    clearFlags = respJson.clearFlags ?? new List<string>(),
                                    isDispatchAction = respJson.isDispatchAction
                                };

                                segment.responses.Add(response);
                            }
                        }

                        callAsset.segments.Add(segment);
                    }
                }

                EditorUtility.SetDirty(callAsset);
                importedCount++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[NightDataImporter] {importedCount}個のCallをインポートしました。");
        }

        #endregion

        #region Utility Methods

        private string ReadJsonFile(string path)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", path).Replace("\\", "/");

            if (!File.Exists(fullPath))
            {
                return string.Empty;
            }

            return File.ReadAllText(fullPath);
        }

        private T CreateOrLoadAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        #endregion

        #region JSON Wrapper Classes

        [Serializable]
        private class CallersJsonWrapper
        {
            public string nightId = string.Empty;
            public List<CallerJson> callers = new();
        }

        [Serializable]
        private class CallerJson
        {
            public string callerId = string.Empty;
            public string displayName = string.Empty;
            public string realName = string.Empty;
            public string description = string.Empty;
            public string voiceDescription = string.Empty;
            public float voicePitch = 1.0f;
            public PersonalityJson? personality;
            public string hiddenInfo = string.Empty;
            public string trueMotivation = string.Empty;
            public List<CallerRelationJson>? relations;
        }

        [Serializable]
        private class CallerRelationJson
        {
            public string targetCallerId = string.Empty;
            public string relationType = string.Empty;
            public string description = string.Empty;
            public bool isKnown = false;
        }

        [Serializable]
        private class PersonalityJson
        {
            public int honesty;
            public int stability;
            public int cooperation;
            public int aggression;
        }

        [Serializable]
        private class FlagDefinitionsJsonWrapper
        {
            public string nightId = string.Empty;
            public List<FlagDefinitionJson> flagDefinitions = new();
            public List<MutualExclusionRuleJson>? mutualExclusionRules;
        }

        [Serializable]
        private class FlagDefinitionJson
        {
            public string flagId = string.Empty;
            public string category = string.Empty;
            public string description = string.Empty;
            public int weight;
            public bool persistsAcrossNights;
        }

        [Serializable]
        private class MutualExclusionRuleJson
        {
            public string whenFlagSet = string.Empty;
            public List<string>? cancelFlags;
        }

        [Serializable]
        private class EndStateDefinitionJsonWrapper
        {
            public string nightId = string.Empty;
            public string defaultEndState = string.Empty;
            public string defaultEndingId = string.Empty;
            public List<EndStateConditionJson>? endStateConditions;
            public List<EndingMappingJson>? endingMappings;
            public VictimSurvivalJson? victimSurvival;
            public List<EndingJson>? endings;
        }

        [Serializable]
        private class EndStateConditionJson
        {
            public string endStateType = string.Empty;
            public int priority;
            public string description = string.Empty;
            public List<ScoreConditionJson>? scoreConditions;
            public List<FlagConditionJson>? flagConditions;
        }

        [Serializable]
        private class ScoreConditionJson
        {
            public string category = string.Empty;
            public string comparison = string.Empty;
            public int value;
        }

        [Serializable]
        private class FlagConditionJson
        {
            public string variableName = string.Empty;
            public string variableType = string.Empty;
            public string comparisonOperator = string.Empty;
            public bool boolValue;
        }

        [Serializable]
        private class EndingMappingJson
        {
            public string endState = string.Empty;
            public string endingIdIfSurvived = string.Empty;
            public string endingIdIfDied = string.Empty;
            public string endingIdRegardless = string.Empty;
        }

        [Serializable]
        private class VictimSurvivalJson
        {
            public bool requiresDispatch;
            public int maxDispatchTimeMinutes;
            public string dispatchFlagId = string.Empty;
        }

        [Serializable]
        private class EndingJson
        {
            public string endingId = string.Empty;
            public string title = string.Empty;
            public string description = string.Empty;
            public string endingType = string.Empty;
        }

        [Serializable]
        private class ScenarioJsonWrapper
        {
            public string scenarioId = string.Empty;
            public string title = string.Empty;
            public string description = string.Empty;
            public int startTimeMinutes;
            public int endTimeMinutes;
            public float realSecondsPerGameMinute;
            public int difficulty;
            public int estimatedPlayTimeMinutes;
            public string theTruth = string.Empty;
            public List<CallScheduleJson>? callSchedule;
            public List<WorldStateJson>? initialWorldStates;
            public List<EvidenceTemplateJson>? evidenceTemplates;
        }

        [Serializable]
        private class CallScheduleJson
        {
            public string callId = string.Empty;
            public string callerId = string.Empty;
            public string title = string.Empty;
            public int incomingTimeMinutes;
            public float ringDuration = 30f;
            public int priority = 5;
            public bool isCritical = false;
            public string description = string.Empty;
        }

        [Serializable]
        private class WorldStateJson
        {
            public string stateId = string.Empty;
            public string description = string.Empty;
            public int timeMinutes;
            public List<string>? involvedCallerIds;
            public bool isKnownToPlayer;
        }

        [Serializable]
        private class EvidenceTemplateJson
        {
            public string evidenceId = string.Empty;
            public string title = string.Empty;
            public string content = string.Empty;
            public string category = string.Empty;
        }

        [Serializable]
        private class CallJsonWrapper
        {
            public string callId = string.Empty;
            public string callerId = string.Empty;
            public string title = string.Empty;
            public string description = string.Empty;
            public string startSegmentId = string.Empty;
            public List<SegmentJson>? segments;
        }

        [Serializable]
        private class SegmentJson
        {
            public string segmentId = string.Empty;
            public List<string>? callerLines;
            public List<ResponseJson>? responses;
            public bool isEnding;
        }

        [Serializable]
        private class ResponseJson
        {
            public string responseId = string.Empty;
            public string text = string.Empty;
            public string? nextSegmentId;
            public List<string>? setFlags;
            public List<string>? clearFlags;
            public bool isDispatchAction;
        }

        #endregion
    }
}

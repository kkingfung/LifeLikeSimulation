#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Localization;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 世界状態のスナップショット
    /// 「実際に何が起きているか」を追跡
    /// </summary>
    [Serializable]
    public class WorldStateSnapshot
    {
        [Tooltip("状態のID")]
        public string stateId = string.Empty;

        [Tooltip("状態の説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Tooltip("この状態が発生したゲーム内時刻")]
        public int timeMinutes = 0;

        [Tooltip("関連する発信者ID")]
        public List<string> involvedCallerIds = new();

        [Tooltip("プレイヤーがこれを知っているか")]
        public bool isKnownToPlayer = false;
    }

    /// <summary>
    /// シナリオのエンディング
    /// </summary>
    [Serializable]
    public class ScenarioEnding
    {
        [Tooltip("エンディングのID")]
        public string endingId = string.Empty;

        [Tooltip("エンディングのタイトル")]
        public string title = string.Empty;

        [Tooltip("エンディングの説明")]
        [TextArea(3, 6)]
        public string description = string.Empty;

        [Tooltip("このエンディングの条件")]
        public List<Conditions.StoryCondition> conditions = new();

        [Tooltip("エンディングのタイプ")]
        public EndingType endingType = EndingType.Neutral;

        [Tooltip("エンディング動画/メディア")]
        public CallMediaReference? endingMedia;
    }

    /// <summary>
    /// エンディングのタイプ
    /// </summary>
    public enum EndingType
    {
        /// <summary>真相解明</summary>
        TruthRevealed,

        /// <summary>被害最小化</summary>
        DamageMinimized,

        /// <summary>誰かを救った</summary>
        SomeoneSaved,

        /// <summary>誰かを見捨てた</summary>
        SomeoneAbandoned,

        /// <summary>共犯者になった</summary>
        BecameAccomplice,

        /// <summary>全員を救った</summary>
        EveryoneSaved,

        /// <summary>誰も救えなかった</summary>
        NooneSaved,

        /// <summary>中立/曖昧</summary>
        Neutral,

        /// <summary>隠蔽成功</summary>
        CoverUpSucceeded,

        /// <summary>正義執行</summary>
        JusticeServed
    }

    /// <summary>
    /// 一夜のシナリオデータ
    /// </summary>
    [CreateAssetMenu(fileName = "NewNightScenario", menuName = "LifeLike/Operator/Night Scenario")]
    public class NightScenarioData : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("シナリオの一意なID")]
        public string scenarioId = string.Empty;

        [Tooltip("シナリオのタイトル")]
        public string title = string.Empty;

        [Tooltip("シナリオの説明")]
        [TextArea(3, 6)]
        public string description = string.Empty;

        [Header("時間設定")]
        [Tooltip("シナリオ開始時刻（分単位、例：22:00 = 1320）")]
        public int startTimeMinutes = 1320; // 22:00

        [Tooltip("シナリオ終了時刻（分単位、例：06:00 = 360 ※翌日）")]
        public int endTimeMinutes = 360; // 06:00

        [Tooltip("1ゲーム内分あたりの実時間（秒）")]
        public float realSecondsPerGameMinute = 2f;

        [Header("登場人物")]
        [Tooltip("このシナリオに登場する発信者")]
        public List<CallerData> callers = new();

        [Header("通話")]
        [Tooltip("このシナリオの通話リスト")]
        public List<CallData> calls = new();

        [Header("証拠")]
        [Tooltip("このシナリオで発見可能な証拠テンプレート")]
        public List<EvidenceTemplate> evidenceTemplates = new();

        [Header("世界状態")]
        [Tooltip("初期の世界状態")]
        public List<WorldStateSnapshot> initialWorldStates = new();

        [Tooltip("真実（シナリオの本当の出来事）")]
        [TextArea(5, 10)]
        public string theTruth = string.Empty;

        [Header("エンディング")]
        [Tooltip("可能なエンディング")]
        public List<ScenarioEnding> endings = new();

        [Header("ローカライズ")]
        [Tooltip("シナリオ全体のダイアログデータベース")]
        public ScenarioDialogueDatabase? dialogueDatabase;

        [Header("難易度")]
        [Tooltip("シナリオの難易度（1-5）")]
        [Range(1, 5)]
        public int difficulty = 3;

        [Tooltip("推定プレイ時間（分）")]
        public int estimatedPlayTimeMinutes = 60;

        /// <summary>
        /// ゲーム内時刻を文字列に変換
        /// </summary>
        public string FormatGameTime(int timeMinutes)
        {
            // 24時間を超える場合（翌日）の処理
            int adjustedMinutes = timeMinutes % 1440;
            int hours = adjustedMinutes / 60;
            int minutes = adjustedMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// 指定時刻にトリガーされる通話を取得
        /// </summary>
        public List<CallData> GetCallsAtTime(int currentTimeMinutes)
        {
            var result = new List<CallData>();
            foreach (var call in calls)
            {
                if (call.incomingTimeMinutes == currentTimeMinutes)
                {
                    result.Add(call);
                }
            }
            return result;
        }

        /// <summary>
        /// 指定IDの発信者を取得
        /// </summary>
        public CallerData? GetCaller(string callerId)
        {
            return callers.Find(c => c.callerId == callerId);
        }

        /// <summary>
        /// 指定通話のダイアログデータベースを取得
        /// </summary>
        public CallDialogueDatabase? GetCallDialogue(string callId)
        {
            return dialogueDatabase?.GetCallDialogue(callId);
        }

        /// <summary>
        /// 発信者の表示名を取得（ローカライズ対応）
        /// </summary>
        public string GetCallerDisplayName(string callerId, Language language)
        {
            if (dialogueDatabase != null)
            {
                return dialogueDatabase.GetCallerDisplayName(callerId, language);
            }

            // フォールバック：CallerDataから取得
            var caller = GetCaller(callerId);
            return caller?.displayName ?? callerId;
        }

        /// <summary>
        /// エンディングタイトルを取得（ローカライズ対応）
        /// </summary>
        public string GetEndingTitle(string endingId, Language language)
        {
            if (dialogueDatabase != null)
            {
                return dialogueDatabase.GetEndingTitle(endingId, language);
            }

            // フォールバック：ScenarioEndingから取得
            var ending = endings.Find(e => e.endingId == endingId);
            return ending?.title ?? endingId;
        }

        /// <summary>
        /// 証拠の内容を取得（ローカライズ対応）
        /// </summary>
        public string GetEvidenceContent(string evidenceId, Language language)
        {
            if (dialogueDatabase != null)
            {
                return dialogueDatabase.GetEvidenceContent(evidenceId, language);
            }

            // フォールバック：EvidenceTemplateから取得
            var template = evidenceTemplates.Find(e => e.data.evidenceId == evidenceId);
            return template?.data.content ?? evidenceId;
        }
    }
}

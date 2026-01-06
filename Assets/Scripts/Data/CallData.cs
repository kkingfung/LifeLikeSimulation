#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using LifeLike.Data.Localization;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 通話の状態
    /// </summary>
    public enum CallState
    {
        /// <summary>着信中（未応答）</summary>
        Incoming,

        /// <summary>通話中</summary>
        Active,

        /// <summary>保留中</summary>
        OnHold,

        /// <summary>終了（正常）</summary>
        Ended,

        /// <summary>不在着信</summary>
        Missed,

        /// <summary>切断（相手が切った）</summary>
        Disconnected
    }

    /// <summary>
    /// 応答データ（プレイヤーの選択肢）
    /// </summary>
    [Serializable]
    public class ResponseData
    {
        [Header("基本情報")]
        [Tooltip("応答の一意なID")]
        public string responseId = string.Empty;

        [Tooltip("表示テキスト（ローカライズ対応）")]
        public LocalizedString displayText = new();

        [Tooltip("実際に言う内容（表示テキストと異なる場合、ローカライズ対応）")]
        public LocalizedString actualText = new();

        /// <summary>
        /// 指定言語の表示テキストを取得
        /// </summary>
        public string GetDisplayText(Language language) => displayText.GetText(language);

        /// <summary>
        /// 指定言語の実際の発言テキストを取得
        /// </summary>
        public string GetActualText(Language language)
        {
            var text = actualText.GetText(language);
            return string.IsNullOrEmpty(text) ? displayText.GetText(language) : text;
        }

        [Header("特殊タイプ")]
        [Tooltip("沈黙（何も言わない）かどうか")]
        public bool isSilence = false;

        [Tooltip("嘘かどうか（内部フラグ）")]
        public bool isLie = false;

        [Tooltip("証拠を提示するかどうか")]
        public bool presentsEvidence = false;

        [Tooltip("提示する証拠ID")]
        public string evidenceIdToPresent = string.Empty;

        [Header("条件")]
        [Tooltip("この応答を選択可能にする条件")]
        public List<StoryCondition> conditions = new();

        [Tooltip("必要な証拠ID（持っていないと選択不可）")]
        public List<string> requiredEvidenceIds = new();

        [Header("効果")]
        [Tooltip("選択時の効果")]
        public List<StoryEffect> effects = new();

        [Tooltip("信頼度への影響")]
        public int trustImpact = 0;

        [Header("遷移")]
        [Tooltip("次のセグメントID（同じ通話内）")]
        public string nextSegmentId = string.Empty;

        [Tooltip("通話を終了するかどうか")]
        public bool endsCall = false;

        [Tooltip("新しい証拠を発見するかどうか")]
        public bool discoversEvidence = false;

        [Tooltip("発見する証拠ID")]
        public string discoveredEvidenceId = string.Empty;
    }

    /// <summary>
    /// 通話セグメント（通話内の一区切り）
    /// </summary>
    [Serializable]
    public class CallSegment
    {
        [Header("基本情報")]
        [Tooltip("セグメントの一意なID")]
        public string segmentId = string.Empty;

        [Header("メディア")]
        [Tooltip("このセグメントのメディア（シルエット/音声/動画）")]
        public CallMediaReference media = new();

        [Header("応答")]
        [Tooltip("プレイヤーの応答選択肢")]
        public List<ResponseData> responses = new();

        [Tooltip("応答の制限時間（0で無制限）")]
        public float responseTimeLimit = 0f;

        [Tooltip("タイムアウト時のデフォルト応答ID")]
        public string timeoutResponseId = string.Empty;

        [Header("証拠")]
        [Tooltip("このセグメントで自動的に得られる証拠ID")]
        public List<string> autoDiscoveredEvidenceIds = new();

        [Header("条件")]
        [Tooltip("このセグメントを表示する条件")]
        public List<StoryCondition> conditions = new();
    }

    /// <summary>
    /// 通話データ（1回の電話通話全体）
    /// </summary>
    [CreateAssetMenu(fileName = "NewCall", menuName = "LifeLike/Operator/Call Data")]
    public class CallData : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("通話の一意なID")]
        public string callId = string.Empty;

        [Tooltip("発信者")]
        public CallerData? caller;

        [Tooltip("通話の内部説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("タイミング")]
        [Tooltip("ゲーム内での着信時刻（分単位、0:00からの経過分）")]
        public int incomingTimeMinutes = 0;

        [Tooltip("着信してから消えるまでの時間（秒）")]
        public float ringDuration = 30f;

        [Tooltip("優先度（高いほど早く処理すべき）")]
        [Range(1, 10)]
        public int priority = 5;

        [Header("セグメント")]
        [Tooltip("通話の開始セグメントID")]
        public string startSegmentId = string.Empty;

        [Tooltip("通話のセグメントリスト")]
        public List<CallSegment> segments = new();

        [Header("条件")]
        [Tooltip("この通話が発生する条件")]
        public List<StoryCondition> triggerConditions = new();

        [Header("結果")]
        [Tooltip("通話終了時の効果")]
        public List<StoryEffect> onEndEffects = new();

        [Tooltip("不在着信時の効果")]
        public List<StoryEffect> onMissedEffects = new();

        [Header("メタデータ")]
        [Tooltip("この通話が重要かどうか（スキップ不可）")]
        public bool isCritical = false;

        [Tooltip("リピートコール可能か")]
        public bool canRepeat = false;

        /// <summary>
        /// ゲーム内時刻を文字列で取得
        /// </summary>
        public string GetFormattedTime()
        {
            int hours = incomingTimeMinutes / 60;
            int minutes = incomingTimeMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// 指定IDのセグメントを取得
        /// </summary>
        public CallSegment? GetSegment(string segmentId)
        {
            return segments.Find(s => s.segmentId == segmentId);
        }

        /// <summary>
        /// 開始セグメントを取得
        /// </summary>
        public CallSegment? GetStartSegment()
        {
            if (string.IsNullOrEmpty(startSegmentId))
            {
                return segments.Count > 0 ? segments[0] : null;
            }
            return GetSegment(startSegmentId);
        }
    }
}

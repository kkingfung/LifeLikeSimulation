#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 証拠の種類
    /// </summary>
    public enum EvidenceType
    {
        /// <summary>発言（誰かが言ったこと）</summary>
        Statement,

        /// <summary>時間情報（いつ何が起きたか）</summary>
        Timestamp,

        /// <summary>場所情報（どこで何が起きたか）</summary>
        Location,

        /// <summary>矛盾（2つの情報が食い違う）</summary>
        Contradiction,

        /// <summary>沈黙（答えなかったこと自体が情報）</summary>
        Silence,

        /// <summary>感情（声のトーン、態度）</summary>
        Emotion,

        /// <summary>関係性（人物間のつながり）</summary>
        Relationship,

        /// <summary>物理的証拠（音、背景音など）</summary>
        Physical
    }

    /// <summary>
    /// 証拠の信頼度
    /// </summary>
    public enum EvidenceReliability
    {
        /// <summary>未確認</summary>
        Unverified,

        /// <summary>疑わしい</summary>
        Doubtful,

        /// <summary>部分的に確認済み</summary>
        PartiallyVerified,

        /// <summary>確認済み</summary>
        Verified,

        /// <summary>虚偽と判明</summary>
        Disproven
    }

    /// <summary>
    /// 証拠データ
    /// ゲーム内で収集される情報の単位
    /// </summary>
    [Serializable]
    public class EvidenceData
    {
        [Header("基本情報")]
        [Tooltip("証拠の一意なID")]
        public string evidenceId = string.Empty;

        [Tooltip("証拠のタイトル")]
        public string title = string.Empty;

        [Tooltip("証拠の種類")]
        public EvidenceType evidenceType = EvidenceType.Statement;

        [Tooltip("証拠の内容（表示用）")]
        [TextArea(2, 4)]
        public string content = string.Empty;

        [Tooltip("証拠の詳細説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("出典")]
        [Tooltip("この証拠を提供した発信者ID")]
        public string sourceCallerId = string.Empty;

        [Tooltip("この証拠が得られた通話ID")]
        public string sourceCallId = string.Empty;

        [Tooltip("取得時刻（ゲーム内時間）")]
        public string timestamp = string.Empty;

        [Header("信頼性")]
        [Tooltip("証拠の信頼度")]
        public EvidenceReliability reliability = EvidenceReliability.Unverified;

        [Tooltip("この証拠が真実かどうか（内部フラグ）")]
        public bool isActuallyTrue = true;

        [Header("関連情報")]
        [Tooltip("関連する発信者ID")]
        public List<string> relatedCallerIds = new();

        [Tooltip("矛盾する証拠ID")]
        public List<string> contradictingEvidenceIds = new();

        [Tooltip("裏付ける証拠ID")]
        public List<string> supportingEvidenceIds = new();

        [Header("使用")]
        [Tooltip("この証拠をプレイヤーが発見済みか")]
        public bool isDiscovered = false;

        [Tooltip("この証拠を使用可能か")]
        public bool isUsable = true;

        [Tooltip("使用回数")]
        public int useCount = 0;
    }

    /// <summary>
    /// 証拠テンプレート（ScriptableObject）
    /// シナリオ作成時に使用
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvidence", menuName = "LifeLike/Operator/Evidence Template")]
    public class EvidenceTemplate : ScriptableObject
    {
        [Tooltip("証拠の基本データ")]
        public EvidenceData data = new();

        [Header("発見条件")]
        [Tooltip("この証拠が発見される条件")]
        public List<Conditions.StoryCondition> discoveryConditions = new();

        [Header("使用効果")]
        [Tooltip("この証拠を使用した時の効果")]
        public List<StoryEffect> useEffects = new();
    }
}

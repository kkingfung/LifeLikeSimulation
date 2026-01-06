#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 信頼の対象タイプ
    /// </summary>
    public enum TrustTargetType
    {
        /// <summary>オペレーター（プレイヤー）</summary>
        Operator,

        /// <summary>他の発信者</summary>
        OtherCaller,

        /// <summary>特定の情報/仮定</summary>
        Assumption,

        /// <summary>組織/機関</summary>
        Organization
    }

    /// <summary>
    /// 信頼レベル
    /// </summary>
    public enum TrustLevel
    {
        /// <summary>完全に不信</summary>
        Hostile = -3,

        /// <summary>不信</summary>
        Distrustful = -2,

        /// <summary>疑念</summary>
        Suspicious = -1,

        /// <summary>中立</summary>
        Neutral = 0,

        /// <summary>やや信頼</summary>
        Tentative = 1,

        /// <summary>信頼</summary>
        Trusting = 2,

        /// <summary>完全信頼</summary>
        Devoted = 3
    }

    /// <summary>
    /// 信頼関係のエッジ（一方向）
    /// </summary>
    [Serializable]
    public class TrustEdge
    {
        [Tooltip("信頼する側のID")]
        public string fromId = string.Empty;

        [Tooltip("信頼される側のID")]
        public string toId = string.Empty;

        [Tooltip("対象タイプ")]
        public TrustTargetType targetType = TrustTargetType.Operator;

        [Tooltip("現在の信頼値（-100 ～ 100）")]
        [Range(-100, 100)]
        public int trustValue = 0;

        [Tooltip("信頼レベル（trustValueから自動計算も可能）")]
        public TrustLevel trustLevel = TrustLevel.Neutral;

        [Tooltip("信頼の理由/履歴")]
        public List<string> trustHistory = new();

        /// <summary>
        /// 信頼値からレベルを計算
        /// </summary>
        public TrustLevel CalculateTrustLevel()
        {
            return trustValue switch
            {
                <= -75 => TrustLevel.Hostile,
                <= -50 => TrustLevel.Distrustful,
                <= -25 => TrustLevel.Suspicious,
                <= 25 => TrustLevel.Neutral,
                <= 50 => TrustLevel.Tentative,
                <= 75 => TrustLevel.Trusting,
                _ => TrustLevel.Devoted
            };
        }

        /// <summary>
        /// 信頼値を変更
        /// </summary>
        public void ModifyTrust(int delta, string reason)
        {
            trustValue = Mathf.Clamp(trustValue + delta, -100, 100);
            trustLevel = CalculateTrustLevel();

            if (!string.IsNullOrEmpty(reason))
            {
                trustHistory.Add($"{(delta >= 0 ? "+" : "")}{delta}: {reason}");
            }
        }
    }

    /// <summary>
    /// 仮定（発信者が信じていること）
    /// </summary>
    [Serializable]
    public class CallerAssumption
    {
        [Tooltip("仮定のID")]
        public string assumptionId = string.Empty;

        [Tooltip("仮定の内容")]
        [TextArea(1, 3)]
        public string content = string.Empty;

        [Tooltip("この仮定を持っている発信者ID")]
        public string holderCallerId = string.Empty;

        [Tooltip("この仮定が真実かどうか")]
        public bool isTrue = false;

        [Tooltip("この仮定への確信度（0-100）")]
        [Range(0, 100)]
        public int confidence = 50;

        [Tooltip("この仮定が崩れた時の効果")]
        public List<StoryEffect> onDisproven = new();
    }

    /// <summary>
    /// 信頼グラフの初期状態（ScriptableObject）
    /// シナリオごとに設定
    /// </summary>
    [CreateAssetMenu(fileName = "NewTrustGraph", menuName = "LifeLike/Operator/Trust Graph")]
    public class TrustGraphData : ScriptableObject
    {
        [Header("初期信頼関係")]
        [Tooltip("発信者間の初期信頼関係")]
        public List<TrustEdge> initialEdges = new();

        [Header("オペレーターへの初期信頼")]
        [Tooltip("各発信者のオペレーターへの初期信頼値")]
        public List<TrustEdge> initialOperatorTrust = new();

        [Header("仮定")]
        [Tooltip("発信者が持っている初期の仮定")]
        public List<CallerAssumption> initialAssumptions = new();

        /// <summary>
        /// 指定した発信者間の初期信頼を取得
        /// </summary>
        public TrustEdge? GetInitialTrust(string fromId, string toId)
        {
            return initialEdges.Find(e => e.fromId == fromId && e.toId == toId);
        }

        /// <summary>
        /// 指定した発信者のオペレーターへの初期信頼を取得
        /// </summary>
        public TrustEdge? GetInitialOperatorTrust(string callerId)
        {
            return initialOperatorTrust.Find(e => e.fromId == callerId);
        }
    }
}

#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data
{
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

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using UnityEngine;

namespace LifeLike.Services.TrustGraph
{
    /// <summary>
    /// 信頼グラフサービスの実装
    /// </summary>
    public class TrustGraphService : ITrustGraphService
    {
        private const string OperatorId = "_operator_";

        // fromId -> toId -> TrustEdge
        private readonly Dictionary<string, Dictionary<string, TrustEdge>> _trustGraph = new();
        private readonly Dictionary<string, CallerAssumption> _assumptions = new();

        public event Action<string, string, int, TrustLevel>? OnTrustChanged;
        public event Action<string, string, TrustLevel>? OnTrustThresholdCrossed;
        public event Action<CallerAssumption>? OnAssumptionDisproven;

        public void Initialize(TrustGraphData initialData)
        {
            Clear();

            // オペレーターへの初期信頼を設定
            foreach (var edge in initialData.initialOperatorTrust)
            {
                SetTrustEdge(edge.fromId, OperatorId, edge);
            }

            // 発信者間の初期信頼を設定
            foreach (var edge in initialData.initialEdges)
            {
                SetTrustEdge(edge.fromId, edge.toId, edge);
            }

            // 仮定を設定
            foreach (var assumption in initialData.initialAssumptions)
            {
                if (!string.IsNullOrEmpty(assumption.assumptionId))
                {
                    _assumptions[assumption.assumptionId] = assumption;
                }
            }

            Debug.Log($"[TrustGraphService] 初期化完了 - 信頼関係: {CountEdges()}, 仮定: {_assumptions.Count}");
        }

        public int GetOperatorTrust(string callerId)
        {
            return GetTrustValue(callerId, OperatorId);
        }

        public TrustLevel GetOperatorTrustLevel(string callerId)
        {
            var edge = GetTrustEdge(callerId, OperatorId);
            return edge?.trustLevel ?? TrustLevel.Neutral;
        }

        public int GetCallerTrust(string fromCallerId, string toCallerId)
        {
            return GetTrustValue(fromCallerId, toCallerId);
        }

        public TrustLevel GetCallerTrustLevel(string fromCallerId, string toCallerId)
        {
            var edge = GetTrustEdge(fromCallerId, toCallerId);
            return edge?.trustLevel ?? TrustLevel.Neutral;
        }

        public void ModifyOperatorTrust(string callerId, int delta, string reason)
        {
            ModifyTrust(callerId, OperatorId, TrustTargetType.Operator, delta, reason);
        }

        public void ModifyCallerTrust(string fromCallerId, string toCallerId, int delta, string reason)
        {
            ModifyTrust(fromCallerId, toCallerId, TrustTargetType.OtherCaller, delta, reason);
        }

        public IReadOnlyList<TrustEdge> GetAllTrustEdgesFor(string callerId)
        {
            var result = new List<TrustEdge>();

            // この発信者が信頼する相手
            if (_trustGraph.TryGetValue(callerId, out var outgoing))
            {
                result.AddRange(outgoing.Values);
            }

            // この発信者を信頼する相手
            foreach (var kvp in _trustGraph)
            {
                if (kvp.Key != callerId && kvp.Value.TryGetValue(callerId, out var edge))
                {
                    result.Add(edge);
                }
            }

            return result.AsReadOnly();
        }

        public IReadOnlyList<CallerAssumption> GetAssumptions(string callerId)
        {
            return _assumptions.Values
                .Where(a => a.holderCallerId == callerId)
                .ToList()
                .AsReadOnly();
        }

        public void ModifyAssumptionConfidence(string assumptionId, int delta)
        {
            if (!_assumptions.TryGetValue(assumptionId, out var assumption))
            {
                return;
            }

            assumption.confidence = Mathf.Clamp(assumption.confidence + delta, 0, 100);
            Debug.Log($"[TrustGraphService] 仮定 {assumptionId} の確信度を変更: {assumption.confidence}");

            // 確信度が0になったら自動的に否定
            if (assumption.confidence <= 0)
            {
                DisproveAssumption(assumptionId);
            }
        }

        public void DisproveAssumption(string assumptionId)
        {
            if (!_assumptions.TryGetValue(assumptionId, out var assumption))
            {
                return;
            }

            Debug.Log($"[TrustGraphService] 仮定が崩れた: {assumption.content}");
            OnAssumptionDisproven?.Invoke(assumption);

            // 効果を適用（StoryServiceと連携が必要）
            // assumption.onDisproven の効果は呼び出し元で処理
        }

        public IReadOnlyList<string> GetTrustHistory(string fromId, string toId)
        {
            var edge = GetTrustEdge(fromId, toId);
            return edge?.trustHistory.AsReadOnly() ?? new List<string>().AsReadOnly();
        }

        public void Clear()
        {
            _trustGraph.Clear();
            _assumptions.Clear();
            Debug.Log("[TrustGraphService] クリアしました。");
        }

        /// <summary>
        /// 信頼を変更する内部メソッド
        /// </summary>
        private void ModifyTrust(string fromId, string toId, TrustTargetType targetType, int delta, string reason)
        {
            var edge = GetOrCreateTrustEdge(fromId, toId, targetType);
            var oldLevel = edge.trustLevel;

            edge.ModifyTrust(delta, reason);

            Debug.Log($"[TrustGraphService] 信頼変更: {fromId} → {toId} ({delta:+#;-#;0}): {reason}");
            OnTrustChanged?.Invoke(fromId, toId, edge.trustValue, edge.trustLevel);

            // 信頼レベルが変わった場合
            if (oldLevel != edge.trustLevel)
            {
                Debug.Log($"[TrustGraphService] 信頼レベル変化: {oldLevel} → {edge.trustLevel}");
                OnTrustThresholdCrossed?.Invoke(fromId, toId, edge.trustLevel);
            }
        }

        /// <summary>
        /// 信頼値を取得
        /// </summary>
        private int GetTrustValue(string fromId, string toId)
        {
            var edge = GetTrustEdge(fromId, toId);
            return edge?.trustValue ?? 0;
        }

        /// <summary>
        /// 信頼エッジを取得
        /// </summary>
        private TrustEdge? GetTrustEdge(string fromId, string toId)
        {
            if (_trustGraph.TryGetValue(fromId, out var targets))
            {
                if (targets.TryGetValue(toId, out var edge))
                {
                    return edge;
                }
            }
            return null;
        }

        /// <summary>
        /// 信頼エッジを取得または作成
        /// </summary>
        private TrustEdge GetOrCreateTrustEdge(string fromId, string toId, TrustTargetType targetType)
        {
            if (!_trustGraph.TryGetValue(fromId, out var targets))
            {
                targets = new Dictionary<string, TrustEdge>();
                _trustGraph[fromId] = targets;
            }

            if (!targets.TryGetValue(toId, out var edge))
            {
                edge = new TrustEdge
                {
                    fromId = fromId,
                    toId = toId,
                    targetType = targetType,
                    trustValue = 0,
                    trustLevel = TrustLevel.Neutral
                };
                targets[toId] = edge;
            }

            return edge;
        }

        /// <summary>
        /// 信頼エッジを設定
        /// </summary>
        private void SetTrustEdge(string fromId, string toId, TrustEdge edge)
        {
            if (!_trustGraph.TryGetValue(fromId, out var targets))
            {
                targets = new Dictionary<string, TrustEdge>();
                _trustGraph[fromId] = targets;
            }
            targets[toId] = edge;
        }

        /// <summary>
        /// エッジ数をカウント
        /// </summary>
        private int CountEdges()
        {
            int count = 0;
            foreach (var targets in _trustGraph.Values)
            {
                count += targets.Count;
            }
            return count;
        }
    }
}

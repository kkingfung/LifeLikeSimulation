#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using UnityEngine;

namespace LifeLike.Services.Operator.Evidence
{
    /// <summary>
    /// 証拠管理サービスの実装
    /// </summary>
    public class EvidenceService : IEvidenceService
    {
        private readonly Dictionary<string, EvidenceTemplate> _templates = new();
        private readonly Dictionary<string, EvidenceData> _discoveredEvidence = new();
        private int _dynamicEvidenceCounter = 0;

        public IReadOnlyList<EvidenceData> DiscoveredEvidence =>
            _discoveredEvidence.Values.ToList().AsReadOnly();

        public event Action<EvidenceData>? OnEvidenceDiscovered;
        public event Action<EvidenceData>? OnEvidenceUpdated;
        public event Action<EvidenceData, EvidenceData>? OnContradictionFound;

        public void LoadEvidenceTemplates(IEnumerable<EvidenceTemplate> templates)
        {
            _templates.Clear();
            foreach (var template in templates)
            {
                if (!string.IsNullOrEmpty(template.data.evidenceId))
                {
                    _templates[template.data.evidenceId] = template;
                }
            }
            Debug.Log($"[EvidenceService] {_templates.Count}個の証拠テンプレートを読み込みました。");
        }

        public bool DiscoverEvidence(string evidenceId)
        {
            // 既に発見済み
            if (_discoveredEvidence.ContainsKey(evidenceId))
            {
                Debug.Log($"[EvidenceService] 証拠 {evidenceId} は既に発見済みです。");
                return false;
            }

            // テンプレートから証拠を作成
            if (!_templates.TryGetValue(evidenceId, out var template))
            {
                Debug.LogWarning($"[EvidenceService] 証拠テンプレート {evidenceId} が見つかりません。");
                return false;
            }

            // 証拠データをコピーして追加
            var evidence = CloneEvidenceData(template.data);
            evidence.isDiscovered = true;
            _discoveredEvidence[evidenceId] = evidence;

            Debug.Log($"[EvidenceService] 証拠を発見: {evidence.content}");
            OnEvidenceDiscovered?.Invoke(evidence);

            // 矛盾チェック
            CheckForContradictions(evidence);

            return true;
        }

        public EvidenceData CreateStatementEvidence(
            string sourceCallerId,
            string sourceCallId,
            string content,
            bool isActuallyTrue = true)
        {
            var evidenceId = $"dynamic_statement_{++_dynamicEvidenceCounter}";

            var evidence = new EvidenceData
            {
                evidenceId = evidenceId,
                evidenceType = EvidenceType.Statement,
                content = content,
                sourceCallerId = sourceCallerId,
                sourceCallId = sourceCallId,
                timestamp = DateTime.Now.ToString("HH:mm"),
                reliability = EvidenceReliability.Unverified,
                isActuallyTrue = isActuallyTrue,
                isDiscovered = true,
                isUsable = true
            };

            evidence.relatedCallerIds.Add(sourceCallerId);
            _discoveredEvidence[evidenceId] = evidence;

            Debug.Log($"[EvidenceService] 発言証拠を作成: {content}");
            OnEvidenceDiscovered?.Invoke(evidence);

            // 矛盾チェック
            CheckForContradictions(evidence);

            return evidence;
        }

        public bool UseEvidence(string evidenceId)
        {
            if (!_discoveredEvidence.TryGetValue(evidenceId, out var evidence))
            {
                return false;
            }

            if (!evidence.isUsable)
            {
                Debug.Log($"[EvidenceService] 証拠 {evidenceId} は使用できません。");
                return false;
            }

            evidence.useCount++;
            Debug.Log($"[EvidenceService] 証拠を使用: {evidence.content} (使用回数: {evidence.useCount})");

            return true;
        }

        public void UpdateReliability(string evidenceId, EvidenceReliability reliability)
        {
            if (!_discoveredEvidence.TryGetValue(evidenceId, out var evidence))
            {
                return;
            }

            var oldReliability = evidence.reliability;
            evidence.reliability = reliability;

            Debug.Log($"[EvidenceService] 証拠 {evidenceId} の信頼度を更新: {oldReliability} → {reliability}");
            OnEvidenceUpdated?.Invoke(evidence);
        }

        public bool CheckContradiction(string evidenceId1, string evidenceId2)
        {
            if (!_discoveredEvidence.TryGetValue(evidenceId1, out var evidence1) ||
                !_discoveredEvidence.TryGetValue(evidenceId2, out var evidence2))
            {
                return false;
            }

            // 既に矛盾として記録されているかチェック
            if (evidence1.contradictingEvidenceIds.Contains(evidenceId2))
            {
                return true;
            }

            // テンプレートで定義された矛盾をチェック
            if (_templates.TryGetValue(evidenceId1, out var template1))
            {
                if (template1.data.contradictingEvidenceIds.Contains(evidenceId2))
                {
                    RecordContradiction(evidence1, evidence2);
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<EvidenceData> GetEvidenceForCaller(string callerId)
        {
            return _discoveredEvidence.Values
                .Where(e => e.sourceCallerId == callerId || e.relatedCallerIds.Contains(callerId))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<EvidenceData> GetEvidenceByType(EvidenceType type)
        {
            return _discoveredEvidence.Values
                .Where(e => e.evidenceType == type)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<EvidenceData> GetUsableEvidence()
        {
            return _discoveredEvidence.Values
                .Where(e => e.isUsable)
                .ToList()
                .AsReadOnly();
        }

        public bool HasEvidence(string evidenceId)
        {
            return _discoveredEvidence.ContainsKey(evidenceId);
        }

        public void Clear()
        {
            _discoveredEvidence.Clear();
            _dynamicEvidenceCounter = 0;
            Debug.Log("[EvidenceService] 証拠をクリアしました。");
        }

        /// <summary>
        /// 新しい証拠と既存の証拠の矛盾をチェック
        /// </summary>
        private void CheckForContradictions(EvidenceData newEvidence)
        {
            foreach (var existing in _discoveredEvidence.Values)
            {
                if (existing.evidenceId == newEvidence.evidenceId) continue;

                // テンプレートで定義された矛盾
                if (_templates.TryGetValue(newEvidence.evidenceId, out var template))
                {
                    if (template.data.contradictingEvidenceIds.Contains(existing.evidenceId))
                    {
                        RecordContradiction(newEvidence, existing);
                    }
                }

                // 同じ内容で異なる真偽値を持つ発言の矛盾
                if (newEvidence.evidenceType == EvidenceType.Statement &&
                    existing.evidenceType == EvidenceType.Statement &&
                    newEvidence.isActuallyTrue != existing.isActuallyTrue &&
                    IsSimilarContent(newEvidence.content, existing.content))
                {
                    RecordContradiction(newEvidence, existing);
                }
            }
        }

        /// <summary>
        /// 矛盾を記録
        /// </summary>
        private void RecordContradiction(EvidenceData evidence1, EvidenceData evidence2)
        {
            if (!evidence1.contradictingEvidenceIds.Contains(evidence2.evidenceId))
            {
                evidence1.contradictingEvidenceIds.Add(evidence2.evidenceId);
            }
            if (!evidence2.contradictingEvidenceIds.Contains(evidence1.evidenceId))
            {
                evidence2.contradictingEvidenceIds.Add(evidence1.evidenceId);
            }

            Debug.Log($"[EvidenceService] 矛盾を発見: {evidence1.evidenceId} ⇔ {evidence2.evidenceId}");
            OnContradictionFound?.Invoke(evidence1, evidence2);
        }

        /// <summary>
        /// 内容が類似しているかの簡易チェック（実際はもっと高度なロジックが必要）
        /// </summary>
        private bool IsSimilarContent(string content1, string content2)
        {
            // 簡易的な実装。実際のゲームでは、タグやキーワードベースでチェック
            return false;
        }

        /// <summary>
        /// 証拠データをコピー
        /// </summary>
        private EvidenceData CloneEvidenceData(EvidenceData original)
        {
            return new EvidenceData
            {
                evidenceId = original.evidenceId,
                evidenceType = original.evidenceType,
                content = original.content,
                description = original.description,
                sourceCallerId = original.sourceCallerId,
                sourceCallId = original.sourceCallId,
                timestamp = original.timestamp,
                reliability = original.reliability,
                isActuallyTrue = original.isActuallyTrue,
                relatedCallerIds = new List<string>(original.relatedCallerIds),
                contradictingEvidenceIds = new List<string>(original.contradictingEvidenceIds),
                supportingEvidenceIds = new List<string>(original.supportingEvidenceIds),
                isDiscovered = original.isDiscovered,
                isUsable = original.isUsable,
                useCount = original.useCount
            };
        }
    }
}

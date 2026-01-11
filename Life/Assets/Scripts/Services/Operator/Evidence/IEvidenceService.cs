#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Operator.Evidence
{
    /// <summary>
    /// 証拠管理サービスのインターフェース
    /// </summary>
    public interface IEvidenceService
    {
        /// <summary>
        /// 発見済みの証拠リスト
        /// </summary>
        IReadOnlyList<EvidenceData> DiscoveredEvidence { get; }

        /// <summary>
        /// 証拠を発見した時のイベント
        /// </summary>
        event Action<EvidenceData>? OnEvidenceDiscovered;

        /// <summary>
        /// 証拠が更新された時のイベント（信頼度変更など）
        /// </summary>
        event Action<EvidenceData>? OnEvidenceUpdated;

        /// <summary>
        /// 矛盾が発見された時のイベント
        /// </summary>
        event Action<EvidenceData, EvidenceData>? OnContradictionFound;

        /// <summary>
        /// シナリオの証拠テンプレートを読み込む
        /// </summary>
        void LoadEvidenceTemplates(IEnumerable<EvidenceTemplate> templates);

        /// <summary>
        /// 証拠を発見する
        /// </summary>
        bool DiscoverEvidence(string evidenceId);

        /// <summary>
        /// 発言から証拠を作成する
        /// </summary>
        EvidenceData CreateStatementEvidence(
            string sourceCallerId,
            string sourceCallId,
            string content,
            bool isActuallyTrue = true);

        /// <summary>
        /// 証拠を使用する
        /// </summary>
        bool UseEvidence(string evidenceId);

        /// <summary>
        /// 証拠の信頼度を更新する
        /// </summary>
        void UpdateReliability(string evidenceId, EvidenceReliability reliability);

        /// <summary>
        /// 2つの証拠が矛盾するかチェック
        /// </summary>
        bool CheckContradiction(string evidenceId1, string evidenceId2);

        /// <summary>
        /// 指定した発信者に関連する証拠を取得
        /// </summary>
        IReadOnlyList<EvidenceData> GetEvidenceForCaller(string callerId);

        /// <summary>
        /// 指定した種類の証拠を取得
        /// </summary>
        IReadOnlyList<EvidenceData> GetEvidenceByType(EvidenceType type);

        /// <summary>
        /// 使用可能な証拠を取得
        /// </summary>
        IReadOnlyList<EvidenceData> GetUsableEvidence();

        /// <summary>
        /// 証拠を持っているかチェック
        /// </summary>
        bool HasEvidence(string evidenceId);

        /// <summary>
        /// 全ての証拠をクリア
        /// </summary>
        void Clear();
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Operator.TrustGraph
{
    /// <summary>
    /// 信頼グラフサービスのインターフェース
    /// 発信者間、発信者とオペレーター間の信頼関係を管理
    /// </summary>
    public interface ITrustGraphService
    {
        /// <summary>
        /// 信頼が変化した時のイベント
        /// </summary>
        event Action<string, string, int, TrustLevel>? OnTrustChanged;

        /// <summary>
        /// 信頼レベルが閾値を超えた時のイベント
        /// </summary>
        event Action<string, string, TrustLevel>? OnTrustThresholdCrossed;

        /// <summary>
        /// 仮定が崩れた時のイベント
        /// </summary>
        event Action<CallerAssumption>? OnAssumptionDisproven;

        /// <summary>
        /// 信頼グラフを初期化
        /// </summary>
        void Initialize(TrustGraphData initialData);

        /// <summary>
        /// 発信者のオペレーターへの信頼値を取得
        /// </summary>
        int GetOperatorTrust(string callerId);

        /// <summary>
        /// 発信者のオペレーターへの信頼レベルを取得
        /// </summary>
        TrustLevel GetOperatorTrustLevel(string callerId);

        /// <summary>
        /// 発信者間の信頼値を取得
        /// </summary>
        int GetCallerTrust(string fromCallerId, string toCallerId);

        /// <summary>
        /// 発信者間の信頼レベルを取得
        /// </summary>
        TrustLevel GetCallerTrustLevel(string fromCallerId, string toCallerId);

        /// <summary>
        /// オペレーターへの信頼を変更
        /// </summary>
        void ModifyOperatorTrust(string callerId, int delta, string reason);

        /// <summary>
        /// 発信者間の信頼を変更
        /// </summary>
        void ModifyCallerTrust(string fromCallerId, string toCallerId, int delta, string reason);

        /// <summary>
        /// 指定した発信者の全ての信頼関係を取得
        /// </summary>
        IReadOnlyList<TrustEdge> GetAllTrustEdgesFor(string callerId);

        /// <summary>
        /// 指定した発信者の仮定を取得
        /// </summary>
        IReadOnlyList<CallerAssumption> GetAssumptions(string callerId);

        /// <summary>
        /// 仮定の確信度を変更
        /// </summary>
        void ModifyAssumptionConfidence(string assumptionId, int delta);

        /// <summary>
        /// 仮定を否定する
        /// </summary>
        void DisproveAssumption(string assumptionId);

        /// <summary>
        /// 信頼履歴を取得
        /// </summary>
        IReadOnlyList<string> GetTrustHistory(string fromId, string toId);

        /// <summary>
        /// 全てクリア
        /// </summary>
        void Clear();
    }
}

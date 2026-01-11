#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Flag;

namespace LifeLike.Services.Operator.Flag
{
    /// <summary>
    /// フラグ管理サービスのインターフェース
    /// プレイヤーの行動パターンを追跡し、スコアを計算する
    /// </summary>
    public interface IFlagService
    {
        #region プロパティ

        /// <summary>
        /// 現在の夜ID
        /// </summary>
        string CurrentNightId { get; }

        /// <summary>
        /// フラグ定義データ
        /// </summary>
        NightFlagsDefinition? FlagsDefinition { get; }

        #endregion

        #region フラグ操作

        /// <summary>
        /// フラグを設定する
        /// </summary>
        /// <param name="flagId">フラグID</param>
        /// <param name="gameTimeMinutes">設定時のゲーム内時刻（オプション）</param>
        void SetFlag(string flagId, int gameTimeMinutes = 0);

        /// <summary>
        /// フラグをクリアする
        /// </summary>
        /// <param name="flagId">フラグID</param>
        void ClearFlag(string flagId);

        /// <summary>
        /// フラグの状態を取得する
        /// </summary>
        /// <param name="flagId">フラグID</param>
        /// <returns>フラグが設定されていればtrue</returns>
        bool GetFlag(string flagId);

        /// <summary>
        /// フラグの状態（詳細）を取得する
        /// </summary>
        /// <param name="flagId">フラグID</param>
        /// <returns>フラグ状態（存在しない場合はnull）</returns>
        FlagState? GetFlagState(string flagId);

        /// <summary>
        /// 複数のフラグを一度に設定する
        /// </summary>
        /// <param name="flagIds">フラグIDのリスト</param>
        /// <param name="gameTimeMinutes">設定時のゲーム内時刻</param>
        void SetFlags(IEnumerable<string> flagIds, int gameTimeMinutes = 0);

        /// <summary>
        /// 複数のフラグを一度にクリアする
        /// </summary>
        /// <param name="flagIds">フラグIDのリスト</param>
        void ClearFlags(IEnumerable<string> flagIds);

        #endregion

        #region スコア計算

        /// <summary>
        /// 指定カテゴリのスコアを計算する
        /// </summary>
        /// <param name="category">フラグカテゴリ</param>
        /// <returns>集計スコア</returns>
        int GetCategoryScore(FlagCategory category);

        /// <summary>
        /// 安心スコアを取得（reassurance_score）
        /// </summary>
        int ReassuranceScore { get; }

        /// <summary>
        /// 開示スコアを取得（disclosure_score）
        /// </summary>
        int DisclosureScore { get; }

        /// <summary>
        /// エスカレーションスコアを取得（escalation_score）
        /// </summary>
        int EscalationScore { get; }

        /// <summary>
        /// システム信頼度を取得（system_trust）
        /// アラインメントスコア
        /// </summary>
        int SystemTrust { get; }

        #endregion

        #region クエリ

        /// <summary>
        /// 設定されているすべてのフラグIDを取得
        /// </summary>
        IReadOnlyList<string> GetSetFlags();

        /// <summary>
        /// 指定カテゴリで設定されているフラグを取得
        /// </summary>
        /// <param name="category">フラグカテゴリ</param>
        IReadOnlyList<string> GetSetFlagsByCategory(FlagCategory category);

        /// <summary>
        /// すべてのフラグ状態を取得
        /// </summary>
        IReadOnlyDictionary<string, FlagState> GetAllFlagStates();

        /// <summary>
        /// すべてのフラグをリスト形式で取得
        /// </summary>
        List<FlagState> GetAllFlags();

        /// <summary>
        /// 永続化フラグをリスト形式で取得（夜をまたぐ用）
        /// </summary>
        List<FlagState> GetPersistentFlags();

        #endregion

        #region 永続化

        /// <summary>
        /// 現在の状態からスナップショットを作成
        /// </summary>
        NightFlagSnapshot CreateSnapshot();

        /// <summary>
        /// スナップショットから状態を復元
        /// </summary>
        /// <param name="snapshot">復元するスナップショット</param>
        void RestoreFromSnapshot(NightFlagSnapshot snapshot);

        /// <summary>
        /// 永続化フラグのみをエクスポート（夜をまたぐ用）
        /// </summary>
        NightFlagSnapshot ExportPersistentFlags();

        /// <summary>
        /// 永続化フラグをインポート（夜をまたぐ用）
        /// </summary>
        /// <param name="snapshot">前の夜からのスナップショット</param>
        void ImportPersistentFlags(NightFlagSnapshot snapshot);

        #endregion

        #region 初期化・クリア

        /// <summary>
        /// 新しい夜を初期化
        /// </summary>
        /// <param name="nightId">夜ID</param>
        /// <param name="flagsDefinition">フラグ定義データ</param>
        void Initialize(string nightId, NightFlagsDefinition flagsDefinition);

        /// <summary>
        /// すべてのフラグをクリア
        /// </summary>
        void ClearAllFlags();

        #endregion

        #region イベント

        /// <summary>
        /// フラグが変更されたときのイベント
        /// </summary>
        event Action<string, bool>? OnFlagChanged;

        /// <summary>
        /// カテゴリスコアが変更されたときのイベント
        /// </summary>
        event Action<FlagCategory, int>? OnScoreChanged;

        #endregion
    }
}

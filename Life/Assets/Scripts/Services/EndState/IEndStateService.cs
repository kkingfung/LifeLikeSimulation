#nullable enable
using System;
using LifeLike.Data;
using LifeLike.Data.EndState;

namespace LifeLike.Services.EndState
{
    /// <summary>
    /// エンドステート計算サービスのインターフェース
    /// フラグ状態からエンドステートを計算し、適切なエンディングを選択する
    /// </summary>
    public interface IEndStateService
    {
        #region プロパティ

        /// <summary>
        /// エンドステート定義データ
        /// </summary>
        EndStateDefinition? Definition { get; }

        /// <summary>
        /// 現在計算されたエンドステート
        /// </summary>
        EndStateType? CurrentEndState { get; }

        /// <summary>
        /// 被害者の生存状態
        /// </summary>
        bool? VictimSurvived { get; }

        /// <summary>
        /// 選択されたエンディングID
        /// </summary>
        string? SelectedEndingId { get; }

        #endregion

        #region 初期化

        /// <summary>
        /// エンドステート定義を読み込む
        /// </summary>
        /// <param name="definition">エンドステート定義データ</param>
        void Initialize(EndStateDefinition definition);

        #endregion

        #region 計算

        /// <summary>
        /// エンドステートを計算する
        /// </summary>
        /// <returns>計算されたエンドステート</returns>
        EndStateType CalculateEndState();

        /// <summary>
        /// 被害者の生存を計算する
        /// </summary>
        /// <param name="dispatchTimeMinutes">派遣時刻（分単位）、派遣していない場合はnull</param>
        /// <returns>生存していればtrue</returns>
        bool CalculateVictimSurvival(int? dispatchTimeMinutes);

        /// <summary>
        /// エンディングを選択する
        /// </summary>
        /// <param name="endState">エンドステート</param>
        /// <param name="victimSurvived">被害者の生存状態</param>
        /// <returns>エンディングID</returns>
        string SelectEnding(EndStateType endState, bool victimSurvived);

        /// <summary>
        /// 全ての計算を実行し、エンディングを決定する
        /// </summary>
        /// <param name="dispatchTimeMinutes">派遣時刻（分単位）、派遣していない場合はnull</param>
        /// <returns>選択されたエンディングID</returns>
        string DetermineEnding(int? dispatchTimeMinutes);

        #endregion

        #region クエリ

        /// <summary>
        /// 指定のエンドステートに到達可能かチェック
        /// </summary>
        /// <param name="endState">チェックするエンドステート</param>
        /// <returns>条件を満たしていればtrue</returns>
        bool CheckEndStateCondition(EndStateType endState);

        /// <summary>
        /// エンディングデータを取得する
        /// </summary>
        /// <param name="endingId">エンディングID</param>
        /// <returns>エンディングデータ（見つからない場合はnull）</returns>
        ScenarioEnding? GetEnding(string endingId);

        #endregion

        #region イベント

        /// <summary>
        /// エンドステートが計算されたときのイベント
        /// </summary>
        event Action<EndStateType>? OnEndStateCalculated;

        /// <summary>
        /// 被害者の生存が計算されたときのイベント
        /// </summary>
        event Action<bool>? OnVictimSurvivalCalculated;

        /// <summary>
        /// エンディングが選択されたときのイベント
        /// </summary>
        event Action<string, ScenarioEnding?>? OnEndingSelected;

        #endregion
    }
}

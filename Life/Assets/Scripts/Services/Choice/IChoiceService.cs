#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Choice
{
    /// <summary>
    /// 選択肢の表示と選択を管理するサービスのインターフェース
    /// </summary>
    public interface IChoiceService
    {
        /// <summary>
        /// 選択肢が表示中かどうか
        /// </summary>
        bool IsShowingChoices { get; }

        /// <summary>
        /// 時限選択の残り時間（秒）
        /// </summary>
        float RemainingTime { get; }

        /// <summary>
        /// 時限選択が進行中かどうか
        /// </summary>
        bool IsTimerActive { get; }

        /// <summary>
        /// 選択肢表示時のイベント
        /// </summary>
        event Action<IReadOnlyList<ChoiceData>>? OnChoicesPresented;

        /// <summary>
        /// 選択肢選択時のイベント
        /// </summary>
        event Action<ChoiceData>? OnChoiceSelected;

        /// <summary>
        /// タイマー更新時のイベント（残り時間）
        /// </summary>
        event Action<float>? OnTimerUpdated;

        /// <summary>
        /// タイムアウト時のイベント
        /// </summary>
        event Action? OnTimedOut;

        /// <summary>
        /// 選択肢を非表示にした時のイベント
        /// </summary>
        event Action? OnChoicesHidden;

        /// <summary>
        /// 選択肢を表示する
        /// </summary>
        /// <param name="choices">表示する選択肢のリスト</param>
        /// <param name="filterUnavailable">条件を満たさない選択肢を除外するかどうか</param>
        void PresentChoices(IEnumerable<ChoiceData> choices, bool filterUnavailable = false);

        /// <summary>
        /// 選択肢を選択する
        /// </summary>
        /// <param name="choice">選択した選択肢</param>
        void SelectChoice(ChoiceData choice);

        /// <summary>
        /// 選択肢を非表示にする
        /// </summary>
        void HideChoices();

        /// <summary>
        /// 選択肢が選択可能かどうかを確認する
        /// </summary>
        /// <param name="choice">確認する選択肢</param>
        /// <returns>選択可能な場合はtrue</returns>
        bool IsChoiceAvailable(ChoiceData choice);

        /// <summary>
        /// 利用可能な選択肢をフィルタリングして返す
        /// </summary>
        /// <param name="choices">元の選択肢リスト</param>
        /// <returns>利用可能な選択肢のリスト</returns>
        IReadOnlyList<ChoiceData> FilterAvailableChoices(IEnumerable<ChoiceData> choices);

        /// <summary>
        /// タイマーを開始する（時限選択用）
        /// </summary>
        /// <param name="duration">制限時間（秒）</param>
        void StartTimer(float duration);

        /// <summary>
        /// タイマーを停止する
        /// </summary>
        void StopTimer();
    }
}

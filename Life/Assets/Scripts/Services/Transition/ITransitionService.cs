#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Data;

namespace LifeLike.Services.Transition
{
    /// <summary>
    /// シーン間のトランジション（画面遷移演出）を管理するサービスのインターフェース
    /// </summary>
    public interface ITransitionService
    {
        /// <summary>
        /// トランジションが進行中かどうか
        /// </summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// トランジション開始時のイベント
        /// </summary>
        event Action? OnTransitionStarted;

        /// <summary>
        /// トランジション完了時のイベント
        /// </summary>
        event Action? OnTransitionCompleted;

        /// <summary>
        /// トランジションの中間点（フェードアウト完了時など）のイベント
        /// </summary>
        event Action? OnTransitionMidpoint;

        /// <summary>
        /// トランジションを実行する
        /// </summary>
        /// <param name="settings">トランジション設定</param>
        /// <param name="onMidpoint">中間点で実行するアクション（シーン切り替えなど）</param>
        Task ExecuteTransition(TransitionSettings settings, Action? onMidpoint = null);

        /// <summary>
        /// フェードアウトのみを実行する
        /// </summary>
        /// <param name="settings">トランジション設定</param>
        Task FadeOut(TransitionSettings settings);

        /// <summary>
        /// フェードインのみを実行する
        /// </summary>
        /// <param name="settings">トランジション設定</param>
        Task FadeIn(TransitionSettings settings);

        /// <summary>
        /// トランジションをスキップする
        /// </summary>
        void Skip();
    }
}

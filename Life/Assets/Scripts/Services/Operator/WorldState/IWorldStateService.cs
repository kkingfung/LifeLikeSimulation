#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Operator.WorldState
{
    /// <summary>
    /// 世界状態管理サービスのインターフェース
    /// 「実際に何が起きているか」を追跡する
    /// プレイヤーの認識とは独立して世界の真実を管理
    /// </summary>
    public interface IWorldStateService
    {
        /// <summary>
        /// 現在のゲーム内時刻（分単位）
        /// </summary>
        int CurrentTimeMinutes { get; }

        /// <summary>
        /// フォーマットされた現在時刻
        /// </summary>
        string FormattedTime { get; }

        /// <summary>
        /// シナリオが終了したかどうか
        /// </summary>
        bool IsScenarioEnded { get; }

        /// <summary>
        /// 時刻が変更された時のイベント
        /// </summary>
        event Action<int, string>? OnTimeChanged;

        /// <summary>
        /// 世界状態が変化した時のイベント
        /// </summary>
        event Action<WorldStateSnapshot>? OnWorldStateChanged;

        /// <summary>
        /// シナリオ終了時のイベント
        /// </summary>
        event Action<ScenarioEnding>? OnScenarioEnded;

        /// <summary>
        /// 通話がトリガーされるべき時のイベント
        /// </summary>
        event Action<CallData>? OnCallTriggered;

        /// <summary>
        /// シナリオを読み込む
        /// </summary>
        void LoadScenario(NightScenarioData scenario);

        /// <summary>
        /// 時間を進める（実時間での更新）
        /// </summary>
        void UpdateTime(float deltaTime);

        /// <summary>
        /// 世界状態を追加
        /// </summary>
        void AddWorldState(WorldStateSnapshot state);

        /// <summary>
        /// プレイヤーに世界状態を明かす
        /// </summary>
        void RevealStateToPlayer(string stateId);

        /// <summary>
        /// プレイヤーが知っている世界状態を取得
        /// </summary>
        IReadOnlyList<WorldStateSnapshot> GetKnownWorldStates();

        /// <summary>
        /// 全ての世界状態を取得（デバッグ用）
        /// </summary>
        IReadOnlyList<WorldStateSnapshot> GetAllWorldStates();

        /// <summary>
        /// エンディング条件をチェック
        /// </summary>
        ScenarioEnding? CheckEndingConditions();

        /// <summary>
        /// シナリオを終了させる
        /// </summary>
        void EndScenario(ScenarioEnding ending);

        /// <summary>
        /// シナリオを終了させる（EndStateServiceを使用してエンディングを自動決定）
        /// </summary>
        void FinalizeScenario();

        /// <summary>
        /// 一時停止
        /// </summary>
        void Pause();

        /// <summary>
        /// 再開
        /// </summary>
        void Resume();

        /// <summary>
        /// クリア
        /// </summary>
        void Clear();
    }
}

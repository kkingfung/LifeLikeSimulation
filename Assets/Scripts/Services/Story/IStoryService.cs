#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;
using LifeLike.Data.Conditions;

namespace LifeLike.Services.Story
{
    /// <summary>
    /// ストーリー進行を管理するサービスのインターフェース
    /// </summary>
    public interface IStoryService
    {
        /// <summary>
        /// 現在のシーンデータ
        /// </summary>
        StorySceneData? CurrentScene { get; }

        /// <summary>
        /// 現在のチャプター番号
        /// </summary>
        int CurrentChapter { get; }

        /// <summary>
        /// ゲームが進行中かどうか
        /// </summary>
        bool IsGameInProgress { get; }

        /// <summary>
        /// シーン変更時のイベント
        /// </summary>
        event Action<StorySceneData>? OnSceneChanged;

        /// <summary>
        /// ゲーム終了時のイベント（エンディング到達時）
        /// </summary>
        event Action<string>? OnGameEnded;

        /// <summary>
        /// 変数変更時のイベント
        /// </summary>
        event Action<string, object>? OnVariableChanged;

        /// <summary>
        /// 新しいゲームを開始する
        /// </summary>
        /// <param name="gameStateData">ゲーム状態データ</param>
        void StartNewGame(GameStateData gameStateData);

        /// <summary>
        /// 指定したシーンをロードする
        /// </summary>
        /// <param name="sceneId">シーンID</param>
        void LoadScene(string sceneId);

        /// <summary>
        /// デフォルトの次シーンに進む
        /// </summary>
        void ProceedToNextScene();

        /// <summary>
        /// 変数の値を設定する
        /// </summary>
        /// <typeparam name="T">値の型</typeparam>
        /// <param name="variableName">変数名</param>
        /// <param name="value">設定する値</param>
        void SetVariable<T>(string variableName, T value);

        /// <summary>
        /// 変数の値を取得する
        /// </summary>
        /// <typeparam name="T">値の型</typeparam>
        /// <param name="variableName">変数名</param>
        /// <returns>変数の値（存在しない場合はdefault）</returns>
        T? GetVariable<T>(string variableName);

        /// <summary>
        /// 変数の値を取得する（objectとして）
        /// </summary>
        /// <param name="variableName">変数名</param>
        /// <returns>変数の値（存在しない場合はnull）</returns>
        object? GetVariable(string variableName);

        /// <summary>
        /// 条件を評価する
        /// </summary>
        /// <param name="condition">評価する条件</param>
        /// <returns>条件を満たす場合はtrue</returns>
        bool EvaluateCondition(StoryCondition condition);

        /// <summary>
        /// 複数の条件をすべて評価する
        /// </summary>
        /// <param name="conditions">評価する条件のリスト</param>
        /// <returns>すべての条件を満たす場合はtrue</returns>
        bool EvaluateConditions(IEnumerable<StoryCondition> conditions);

        /// <summary>
        /// 効果を適用する
        /// </summary>
        /// <param name="effect">適用する効果</param>
        void ApplyEffect(StoryEffect effect);

        /// <summary>
        /// 複数の効果を適用する
        /// </summary>
        /// <param name="effects">適用する効果のリスト</param>
        void ApplyEffects(IEnumerable<StoryEffect> effects);

        /// <summary>
        /// すべての変数を取得する
        /// </summary>
        IReadOnlyDictionary<string, object> GetAllVariables();
    }
}

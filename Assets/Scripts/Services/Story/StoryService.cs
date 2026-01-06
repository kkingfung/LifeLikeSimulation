#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Services.Story
{
    /// <summary>
    /// ストーリー進行を管理するサービス
    /// </summary>
    public class StoryService : IStoryService, IDisposable
    {
        private GameStateData? _gameStateData;
        private readonly Dictionary<string, object> _variables = new();
        private StorySceneData? _currentScene;
        private bool _isDisposed;

        /// <inheritdoc/>
        public StorySceneData? CurrentScene => _currentScene;

        /// <inheritdoc/>
        public int CurrentChapter => _currentScene?.chapter ?? 0;

        /// <inheritdoc/>
        public bool IsGameInProgress => _gameStateData != null && _currentScene != null;

        /// <inheritdoc/>
        public event Action<StorySceneData>? OnSceneChanged;

        /// <inheritdoc/>
        public event Action<string>? OnGameEnded;

        /// <inheritdoc/>
        public event Action<string, object>? OnVariableChanged;

        /// <inheritdoc/>
        public void StartNewGame(GameStateData gameStateData)
        {
            if (gameStateData == null)
            {
                Debug.LogError("[StoryService] GameStateDataがnullです。");
                return;
            }

            _gameStateData = gameStateData;

            // 変数を初期化
            _variables.Clear();
            var initialVars = gameStateData.CreateInitialVariables();
            foreach (var kvp in initialVars)
            {
                _variables[kvp.Key] = kvp.Value;
            }

            Debug.Log($"[StoryService] 新しいゲームを開始。変数数: {_variables.Count}");

            // 最初のシーンをロード
            if (!string.IsNullOrEmpty(gameStateData.startSceneId))
            {
                LoadScene(gameStateData.startSceneId);
            }
            else
            {
                Debug.LogWarning("[StoryService] 開始シーンIDが設定されていません。");
            }
        }

        /// <inheritdoc/>
        public void LoadScene(string sceneId)
        {
            if (_gameStateData == null)
            {
                Debug.LogError("[StoryService] ゲームが開始されていません。");
                return;
            }

            var scene = _gameStateData.GetScene(sceneId);
            if (scene == null)
            {
                Debug.LogError($"[StoryService] シーンが見つかりません: {sceneId}");
                return;
            }

            // シーン条件をチェック
            if (scene.displayConditions.Count > 0 && !EvaluateConditions(scene.displayConditions))
            {
                Debug.LogWarning($"[StoryService] シーンの表示条件を満たしていません: {sceneId}");
                return;
            }

            _currentScene = scene;

            // シーン開始時の効果を適用
            if (scene.onEnterEffects.Count > 0)
            {
                ApplyEffects(scene.onEnterEffects);
            }

            Debug.Log($"[StoryService] シーンをロード: {scene.sceneName} ({sceneId})");
            OnSceneChanged?.Invoke(scene);

            // エンディングチェック
            if (scene.isEnding)
            {
                Debug.Log($"[StoryService] エンディングに到達: {scene.endingType}");
                OnGameEnded?.Invoke(scene.endingType);
            }
        }

        /// <inheritdoc/>
        public void ProceedToNextScene()
        {
            if (_currentScene == null)
            {
                Debug.LogWarning("[StoryService] 現在のシーンがありません。");
                return;
            }

            if (string.IsNullOrEmpty(_currentScene.defaultNextSceneId))
            {
                Debug.LogWarning("[StoryService] 次のシーンIDが設定されていません。");
                return;
            }

            LoadScene(_currentScene.defaultNextSceneId);
        }

        /// <inheritdoc/>
        public void SetVariable<T>(string variableName, T value)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                return;
            }

            object boxedValue = value!;
            _variables[variableName] = boxedValue;
            OnVariableChanged?.Invoke(variableName, boxedValue);
            Debug.Log($"[StoryService] 変数を設定: {variableName} = {value}");
        }

        /// <inheritdoc/>
        public T? GetVariable<T>(string variableName)
        {
            var value = GetVariable(variableName);
            if (value == null)
            {
                return default;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                Debug.LogWarning($"[StoryService] 変数の型変換に失敗: {variableName}");
                return default;
            }
        }

        /// <inheritdoc/>
        public object? GetVariable(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
            {
                return null;
            }

            return _variables.TryGetValue(variableName, out var value) ? value : null;
        }

        /// <inheritdoc/>
        public bool EvaluateCondition(StoryCondition condition)
        {
            if (condition == null)
            {
                return true;
            }

            var currentValue = GetVariable(condition.variableName);
            return condition.Evaluate(currentValue);
        }

        /// <inheritdoc/>
        public bool EvaluateConditions(IEnumerable<StoryCondition> conditions)
        {
            return conditions.All(EvaluateCondition);
        }

        /// <inheritdoc/>
        public void ApplyEffect(StoryEffect effect)
        {
            if (effect == null || string.IsNullOrEmpty(effect.variableName))
            {
                return;
            }

            var currentValue = GetVariable(effect.variableName);
            var newValue = effect.Apply(currentValue);
            _variables[effect.variableName] = newValue;
            OnVariableChanged?.Invoke(effect.variableName, newValue);
            Debug.Log($"[StoryService] 効果を適用: {effect.variableName} = {newValue}");
        }

        /// <inheritdoc/>
        public void ApplyEffects(IEnumerable<StoryEffect> effects)
        {
            foreach (var effect in effects)
            {
                ApplyEffect(effect);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> GetAllVariables()
        {
            return _variables;
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _variables.Clear();
            _currentScene = null;
            _gameStateData = null;
            OnSceneChanged = null;
            OnGameEnded = null;
            OnVariableChanged = null;

            _isDisposed = true;
        }
    }
}

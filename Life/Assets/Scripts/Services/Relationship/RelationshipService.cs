#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using LifeLike.Services.Core.Story;
using UnityEngine;

namespace LifeLike.Services.Relationship
{
    /// <summary>
    /// キャラクターとの関係性を管理するサービス
    /// </summary>
    public class RelationshipService : IRelationshipService, IDisposable
    {
        private readonly IStoryService _storyService;
        private readonly Dictionary<string, CharacterData> _characters = new();
        private bool _isDisposed;

        /// <inheritdoc/>
        public event Action<string, string, int>? OnRelationshipChanged;

        /// <inheritdoc/>
        public event Action<string, string, string>? OnThresholdReached;

        /// <summary>
        /// RelationshipServiceを初期化する
        /// </summary>
        /// <param name="storyService">ストーリーサービス（変数管理に使用）</param>
        public RelationshipService(IStoryService storyService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        /// <inheritdoc/>
        public void RegisterCharacter(CharacterData character)
        {
            if (character == null)
            {
                Debug.LogWarning("[RelationshipService] キャラクターがnullです。");
                return;
            }

            _characters[character.characterId] = character;

            // 各軸の初期値をStoryServiceに設定
            foreach (var axis in character.relationshipAxes)
            {
                var variableName = character.GetRelationshipVariableName(axis.axisId);
                if (_storyService.GetVariable(variableName) == null)
                {
                    _storyService.SetVariable(variableName, axis.initialValue);
                }
            }

            Debug.Log($"[RelationshipService] キャラクターを登録: {character.characterName}");
        }

        /// <inheritdoc/>
        public void RegisterCharacters(IEnumerable<CharacterData> characters)
        {
            foreach (var character in characters)
            {
                RegisterCharacter(character);
            }
        }

        /// <inheritdoc/>
        public int GetRelationship(string characterId, string axisId)
        {
            if (!TryGetCharacterAndAxis(characterId, axisId, out var character, out var axis))
            {
                return 0;
            }

            var variableName = character!.GetRelationshipVariableName(axisId);
            return _storyService.GetVariable<int>(variableName);
        }

        /// <inheritdoc/>
        public void SetRelationship(string characterId, string axisId, int value)
        {
            if (!TryGetCharacterAndAxis(characterId, axisId, out var character, out var axis))
            {
                return;
            }

            // 値をクランプ
            value = Mathf.Clamp(value, axis!.minValue, axis.maxValue);

            var variableName = character!.GetRelationshipVariableName(axisId);
            var oldValue = _storyService.GetVariable<int>(variableName);

            _storyService.SetVariable(variableName, value);
            Debug.Log($"[RelationshipService] {character.characterName}の{axis.axisName}: {oldValue} → {value}");

            OnRelationshipChanged?.Invoke(characterId, axisId, value);

            // 閾値チェック
            CheckThresholds(characterId, axisId, oldValue, value, axis);
        }

        /// <inheritdoc/>
        public void ModifyRelationship(string characterId, string axisId, int delta)
        {
            var currentValue = GetRelationship(characterId, axisId);
            SetRelationship(characterId, axisId, currentValue + delta);
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, int> GetAllRelationships(string characterId)
        {
            var result = new Dictionary<string, int>();

            if (!_characters.TryGetValue(characterId, out var character))
            {
                return result;
            }

            foreach (var axis in character.relationshipAxes)
            {
                result[axis.axisId] = GetRelationship(characterId, axis.axisId);
            }

            return result;
        }

        /// <inheritdoc/>
        public RelationshipAxis? GetRelationshipAxis(string characterId, string axisId)
        {
            if (!_characters.TryGetValue(characterId, out var character))
            {
                return null;
            }

            return character.relationshipAxes.FirstOrDefault(a => a.axisId == axisId);
        }

        /// <inheritdoc/>
        public float GetNormalizedRelationship(string characterId, string axisId)
        {
            if (!TryGetCharacterAndAxis(characterId, axisId, out _, out var axis))
            {
                return 0f;
            }

            var value = GetRelationship(characterId, axisId);
            var range = axis!.maxValue - axis.minValue;

            if (range <= 0)
            {
                return 0f;
            }

            return (float)(value - axis.minValue) / range;
        }

        /// <inheritdoc/>
        public void ResetAllRelationships()
        {
            foreach (var character in _characters.Values)
            {
                foreach (var axis in character.relationshipAxes)
                {
                    var variableName = character.GetRelationshipVariableName(axis.axisId);
                    _storyService.SetVariable(variableName, axis.initialValue);
                }
            }

            Debug.Log("[RelationshipService] すべての関係性をリセット");
        }

        /// <summary>
        /// キャラクターと軸を取得する
        /// </summary>
        private bool TryGetCharacterAndAxis(
            string characterId,
            string axisId,
            out CharacterData? character,
            out RelationshipAxis? axis)
        {
            character = null;
            axis = null;

            if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(axisId))
            {
                return false;
            }

            if (!_characters.TryGetValue(characterId, out character))
            {
                Debug.LogWarning($"[RelationshipService] キャラクターが見つかりません: {characterId}");
                return false;
            }

            axis = character.relationshipAxes.FirstOrDefault(a => a.axisId == axisId);
            if (axis == null)
            {
                Debug.LogWarning($"[RelationshipService] 軸が見つかりません: {axisId}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 閾値チェックを行う
        /// </summary>
        private void CheckThresholds(
            string characterId,
            string axisId,
            int oldValue,
            int newValue,
            RelationshipAxis axis)
        {
            // 最大値到達チェック
            if (oldValue < axis.maxValue && newValue >= axis.maxValue)
            {
                OnThresholdReached?.Invoke(characterId, axisId, "max");
            }

            // 最小値到達チェック
            if (oldValue > axis.minValue && newValue <= axis.minValue)
            {
                OnThresholdReached?.Invoke(characterId, axisId, "min");
            }

            // 50%到達チェック
            var midpoint = (axis.maxValue + axis.minValue) / 2;
            if (oldValue < midpoint && newValue >= midpoint)
            {
                OnThresholdReached?.Invoke(characterId, axisId, "mid_up");
            }
            else if (oldValue > midpoint && newValue <= midpoint)
            {
                OnThresholdReached?.Invoke(characterId, axisId, "mid_down");
            }
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _characters.Clear();
            OnRelationshipChanged = null;
            OnThresholdReached = null;

            _isDisposed = true;
        }
    }
}

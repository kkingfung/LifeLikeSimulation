#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data.Flag;
using UnityEngine;

namespace LifeLike.Services.Flag
{
    /// <summary>
    /// フラグ管理サービスの実装
    /// プレイヤーの行動パターンを追跡し、スコアを計算する
    /// </summary>
    public class FlagService : IFlagService
    {
        #region フィールド

        private string _currentNightId = string.Empty;
        private NightFlagsDefinition? _flagsDefinition;
        private readonly Dictionary<string, FlagState> _flagStates = new();
        private readonly Dictionary<FlagCategory, int> _cachedScores = new();
        private bool _scoresCacheValid = false;

        #endregion

        #region プロパティ

        public string CurrentNightId => _currentNightId;
        public NightFlagsDefinition? FlagsDefinition => _flagsDefinition;

        public int ReassuranceScore => GetCategoryScore(FlagCategory.Reassurance);
        public int DisclosureScore => GetCategoryScore(FlagCategory.Disclosure);
        public int EscalationScore => GetCategoryScore(FlagCategory.Escalation);
        public int SystemTrust => GetCategoryScore(FlagCategory.Alignment);

        #endregion

        #region イベント

        public event Action<string, bool>? OnFlagChanged;
        public event Action<FlagCategory, int>? OnScoreChanged;

        #endregion

        #region 初期化

        public void Initialize(string nightId, NightFlagsDefinition flagsDefinition)
        {
            _currentNightId = nightId;
            _flagsDefinition = flagsDefinition;
            _flagStates.Clear();
            _cachedScores.Clear();
            _scoresCacheValid = false;

            Debug.Log($"[FlagService] 初期化完了: {nightId}, フラグ定義数: {flagsDefinition.flagDefinitions.Count}");
        }

        public void ClearAllFlags()
        {
            var previousFlags = _flagStates.Keys.ToList();
            _flagStates.Clear();
            _scoresCacheValid = false;

            foreach (var flagId in previousFlags)
            {
                OnFlagChanged?.Invoke(flagId, false);
            }

            // 全カテゴリのスコア変更を通知
            foreach (FlagCategory category in Enum.GetValues(typeof(FlagCategory)))
            {
                OnScoreChanged?.Invoke(category, 0);
            }

            Debug.Log("[FlagService] 全フラグをクリアしました");
        }

        #endregion

        #region フラグ操作

        public void SetFlag(string flagId, int gameTimeMinutes = 0)
        {
            if (string.IsNullOrEmpty(flagId))
            {
                Debug.LogWarning("[FlagService] 空のフラグIDは設定できません");
                return;
            }

            // 既に設定されている場合はスキップ
            if (_flagStates.TryGetValue(flagId, out var existingState) && existingState.isSet)
            {
                return;
            }

            // フラグを設定
            _flagStates[flagId] = new FlagState
            {
                flagId = flagId,
                isSet = true,
                setTime = gameTimeMinutes
            };

            _scoresCacheValid = false;

            Debug.Log($"[FlagService] フラグ設定: {flagId} (時刻: {gameTimeMinutes})");
            OnFlagChanged?.Invoke(flagId, true);

            // 相互排他処理
            ApplyMutualExclusion(flagId);

            // スコア変更通知
            NotifyScoreChanges(flagId);
        }

        public void ClearFlag(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
            {
                return;
            }

            if (_flagStates.TryGetValue(flagId, out var state) && state.isSet)
            {
                state.isSet = false;
                _scoresCacheValid = false;

                Debug.Log($"[FlagService] フラグクリア: {flagId}");
                OnFlagChanged?.Invoke(flagId, false);

                // スコア変更通知
                NotifyScoreChanges(flagId);
            }
        }

        public bool GetFlag(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
            {
                return false;
            }

            return _flagStates.TryGetValue(flagId, out var state) && state.isSet;
        }

        public FlagState? GetFlagState(string flagId)
        {
            if (string.IsNullOrEmpty(flagId))
            {
                return null;
            }

            return _flagStates.TryGetValue(flagId, out var state) ? state : null;
        }

        public void SetFlags(IEnumerable<string> flagIds, int gameTimeMinutes = 0)
        {
            foreach (var flagId in flagIds)
            {
                SetFlag(flagId, gameTimeMinutes);
            }
        }

        public void ClearFlags(IEnumerable<string> flagIds)
        {
            foreach (var flagId in flagIds)
            {
                ClearFlag(flagId);
            }
        }

        #endregion

        #region 相互排他

        private void ApplyMutualExclusion(string flagId)
        {
            if (_flagsDefinition == null)
            {
                return;
            }

            var flagsToCancel = _flagsDefinition.GetCancelledFlags(flagId);
            foreach (var cancelFlagId in flagsToCancel)
            {
                if (GetFlag(cancelFlagId))
                {
                    Debug.Log($"[FlagService] 相互排他: {flagId} が {cancelFlagId} をキャンセル");
                    ClearFlag(cancelFlagId);
                }
            }
        }

        #endregion

        #region スコア計算

        public int GetCategoryScore(FlagCategory category)
        {
            if (_scoresCacheValid && _cachedScores.TryGetValue(category, out var cachedScore))
            {
                return cachedScore;
            }

            int score = CalculateCategoryScore(category);
            _cachedScores[category] = score;

            return score;
        }

        private int CalculateCategoryScore(FlagCategory category)
        {
            if (_flagsDefinition == null)
            {
                return 0;
            }

            int score = 0;
            var categoryFlags = _flagsDefinition.GetFlagsByCategory(category);

            foreach (var flagDef in categoryFlags)
            {
                if (GetFlag(flagDef.flagId))
                {
                    score += flagDef.weight;
                }
            }

            return score;
        }

        private void RecalculateAllScores()
        {
            _cachedScores.Clear();

            foreach (FlagCategory category in Enum.GetValues(typeof(FlagCategory)))
            {
                _cachedScores[category] = CalculateCategoryScore(category);
            }

            _scoresCacheValid = true;
        }

        private void NotifyScoreChanges(string flagId)
        {
            if (_flagsDefinition == null)
            {
                return;
            }

            var flagDef = _flagsDefinition.GetFlagDefinition(flagId);
            if (flagDef != null)
            {
                int newScore = GetCategoryScore(flagDef.category);
                OnScoreChanged?.Invoke(flagDef.category, newScore);
            }
        }

        #endregion

        #region クエリ

        public IReadOnlyList<string> GetSetFlags()
        {
            return _flagStates
                .Where(kvp => kvp.Value.isSet)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        public IReadOnlyList<string> GetSetFlagsByCategory(FlagCategory category)
        {
            if (_flagsDefinition == null)
            {
                return new List<string>();
            }

            var categoryFlags = _flagsDefinition.GetFlagsByCategory(category);
            return categoryFlags
                .Where(f => GetFlag(f.flagId))
                .Select(f => f.flagId)
                .ToList();
        }

        public IReadOnlyDictionary<string, FlagState> GetAllFlagStates()
        {
            return _flagStates;
        }

        #endregion

        #region 永続化

        public NightFlagSnapshot CreateSnapshot()
        {
            return NightFlagSnapshot.Create(_currentNightId, _flagStates, _flagsDefinition);
        }

        public void RestoreFromSnapshot(NightFlagSnapshot snapshot)
        {
            _currentNightId = snapshot.nightId;
            _flagStates.Clear();

            foreach (var state in snapshot.flagStates)
            {
                _flagStates[state.flagId] = state;
            }

            _scoresCacheValid = false;

            Debug.Log($"[FlagService] スナップショットから復元: {snapshot.nightId}, フラグ数: {snapshot.flagStates.Count}");
        }

        public NightFlagSnapshot ExportPersistentFlags()
        {
            if (_flagsDefinition == null)
            {
                return new NightFlagSnapshot { nightId = _currentNightId };
            }

            var persistentFlags = new Dictionary<string, FlagState>();

            foreach (var kvp in _flagStates)
            {
                var flagDef = _flagsDefinition.GetFlagDefinition(kvp.Key);
                if (flagDef != null && flagDef.persistsAcrossNights && kvp.Value.isSet)
                {
                    persistentFlags[kvp.Key] = kvp.Value;
                }
            }

            return NightFlagSnapshot.Create(_currentNightId, persistentFlags, _flagsDefinition);
        }

        public void ImportPersistentFlags(NightFlagSnapshot snapshot)
        {
            foreach (var state in snapshot.flagStates)
            {
                if (state.isSet)
                {
                    _flagStates[state.flagId] = state;
                }
            }

            _scoresCacheValid = false;

            Debug.Log($"[FlagService] 永続フラグをインポート: {snapshot.flagStates.Count}件");
        }

        #endregion
    }
}

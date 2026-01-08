#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;
using LifeLike.Data.Conditions;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Services.Flag;
using UnityEngine;

namespace LifeLike.Services.EndState
{
    /// <summary>
    /// エンドステート計算サービスの実装
    /// フラグ状態からエンドステートを計算し、適切なエンディングを選択する
    /// </summary>
    public class EndStateService : IEndStateService
    {
        #region フィールド

        private readonly IFlagService _flagService;
        private EndStateDefinition? _definition;
        private NightScenarioData? _scenarioData;
        private EndStateType? _currentEndState;
        private bool? _victimSurvived;
        private string? _selectedEndingId;

        #endregion

        #region コンストラクタ

        public EndStateService(IFlagService flagService)
        {
            _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));
        }

        #endregion

        #region プロパティ

        public EndStateDefinition? Definition => _definition;
        public EndStateType? CurrentEndState => _currentEndState;
        public bool? VictimSurvived => _victimSurvived;
        public string? SelectedEndingId => _selectedEndingId;

        #endregion

        #region イベント

        public event Action<EndStateType>? OnEndStateCalculated;
        public event Action<bool>? OnVictimSurvivalCalculated;
        public event Action<string, ScenarioEnding?>? OnEndingSelected;

        #endregion

        #region 初期化

        public void Initialize(EndStateDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _currentEndState = null;
            _victimSurvived = null;
            _selectedEndingId = null;

            Debug.Log($"[EndStateService] 初期化完了: エンドステート条件数: {definition.conditions.Count}");
        }

        /// <summary>
        /// シナリオデータを設定（エンディング情報取得用）
        /// </summary>
        public void SetScenarioData(NightScenarioData scenarioData)
        {
            _scenarioData = scenarioData;
        }

        #endregion

        #region エンドステート計算

        public EndStateType CalculateEndState()
        {
            if (_definition == null)
            {
                Debug.LogWarning("[EndStateService] 定義が設定されていません");
                return EndStateType.Contained;
            }

            var sortedConditions = _definition.GetSortedConditions();

            foreach (var condition in sortedConditions)
            {
                if (EvaluateEndStateCondition(condition))
                {
                    _currentEndState = condition.endStateType;
                    Debug.Log($"[EndStateService] エンドステート決定: {condition.endStateType} ({condition.description})");
                    OnEndStateCalculated?.Invoke(condition.endStateType);
                    return condition.endStateType;
                }
            }

            // どの条件にも合致しない場合はデフォルト
            _currentEndState = _definition.defaultEndState;
            Debug.Log($"[EndStateService] デフォルトエンドステート: {_definition.defaultEndState}");
            OnEndStateCalculated?.Invoke(_definition.defaultEndState);
            return _definition.defaultEndState;
        }

        private bool EvaluateEndStateCondition(EndStateCondition condition)
        {
            // スコア条件をすべてチェック
            foreach (var scoreCondition in condition.scoreConditions)
            {
                int currentScore = _flagService.GetCategoryScore(scoreCondition.category);
                if (!scoreCondition.Evaluate(currentScore))
                {
                    return false;
                }
            }

            // フラグ条件をすべてチェック
            foreach (var flagCondition in condition.flagConditions)
            {
                bool currentValue = _flagService.GetFlag(flagCondition.flagId);
                if (currentValue != flagCondition.requiredValue)
                {
                    return false;
                }
            }

            return true;
        }

        private object? GetVariableValue(string variableName, VariableType variableType)
        {
            // フラグサービスからフラグ値を取得
            if (variableType == VariableType.Boolean)
            {
                return _flagService.GetFlag(variableName);
            }

            // スコア関連の変数名をチェック
            return variableName.ToLower() switch
            {
                "reassurance_score" => _flagService.ReassuranceScore,
                "disclosure_score" => _flagService.DisclosureScore,
                "escalation_score" => _flagService.EscalationScore,
                "system_trust" => _flagService.SystemTrust,
                _ => _flagService.GetFlag(variableName) ? 1 : 0
            };
        }

        public bool CheckEndStateCondition(EndStateType endState)
        {
            if (_definition == null)
            {
                return false;
            }

            var condition = _definition.conditions.Find(c => c.endStateType == endState);
            if (condition == null)
            {
                return false;
            }

            return EvaluateEndStateCondition(condition);
        }

        #endregion

        #region 被害者生存計算

        public bool CalculateVictimSurvival(int? dispatchTimeMinutes)
        {
            if (_definition == null)
            {
                Debug.LogWarning("[EndStateService] 定義が設定されていません");
                _victimSurvived = false;
                return false;
            }

            var survival = _definition.victimSurvival;

            // 派遣が必要な場合
            if (survival.requiresDispatch)
            {
                // 派遣フラグをチェック
                bool dispatched = _flagService.GetFlag(survival.dispatchFlagId);

                if (!dispatched || dispatchTimeMinutes == null)
                {
                    Debug.Log("[EndStateService] 被害者死亡: 派遣なし");
                    _victimSurvived = false;
                    OnVictimSurvivalCalculated?.Invoke(false);
                    return false;
                }

                // 派遣時刻をチェック
                if (dispatchTimeMinutes.Value > survival.maxDispatchTimeMinutes)
                {
                    Debug.Log($"[EndStateService] 被害者死亡: 派遣が遅すぎた ({dispatchTimeMinutes} > {survival.maxDispatchTimeMinutes})");
                    _victimSurvived = false;
                    OnVictimSurvivalCalculated?.Invoke(false);
                    return false;
                }

                Debug.Log($"[EndStateService] 被害者生存: 派遣時刻 {dispatchTimeMinutes}");
                _victimSurvived = true;
                OnVictimSurvivalCalculated?.Invoke(true);
                return true;
            }

            // 派遣不要な場合（常に生存）
            _victimSurvived = true;
            OnVictimSurvivalCalculated?.Invoke(true);
            return true;
        }

        #endregion

        #region エンディング選択

        public string SelectEnding(EndStateType endState, bool victimSurvived)
        {
            if (_definition == null)
            {
                Debug.LogWarning("[EndStateService] 定義が設定されていません");
                return "ending_neutral";
            }

            string endingId = _definition.GetEndingId(endState, victimSurvived);
            _selectedEndingId = endingId;

            var ending = GetEnding(endingId);
            Debug.Log($"[EndStateService] エンディング選択: {endingId} (EndState: {endState}, Survived: {victimSurvived})");

            OnEndingSelected?.Invoke(endingId, ending);
            return endingId;
        }

        public string DetermineEnding(int? dispatchTimeMinutes)
        {
            // 1. 被害者生存を計算
            bool victimSurvived = CalculateVictimSurvival(dispatchTimeMinutes);

            // 2. エンドステートを計算
            EndStateType endState = CalculateEndState();

            // 3. エンディングを選択
            string endingId = SelectEnding(endState, victimSurvived);

            Debug.Log($"[EndStateService] エンディング決定完了: {endingId}");
            return endingId;
        }

        #endregion

        #region クエリ

        public ScenarioEnding? GetEnding(string endingId)
        {
            // まずEndStateDefinitionから検索
            if (_definition != null)
            {
                var ending = _definition.GetEndingById(endingId);
                if (ending != null)
                {
                    return ending;
                }
            }

            // フォールバック: シナリオデータから検索
            if (_scenarioData != null)
            {
                return _scenarioData.endings.Find(e => e.endingId == endingId);
            }

            return null;
        }

        #endregion
    }
}

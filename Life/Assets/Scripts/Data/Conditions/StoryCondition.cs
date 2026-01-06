#nullable enable
using System;
using UnityEngine;

namespace LifeLike.Data.Conditions
{
    /// <summary>
    /// 条件の比較演算子
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,              // ==
        NotEqual,           // !=
        GreaterThan,        // >
        GreaterThanOrEqual, // >=
        LessThan,           // <
        LessThanOrEqual     // <=
    }

    /// <summary>
    /// 条件の変数タイプ
    /// </summary>
    public enum VariableType
    {
        Integer,    // 整数値（好感度など）
        Boolean,    // フラグ
        String      // 文字列
    }

    /// <summary>
    /// ストーリー進行の条件を定義する
    /// 選択肢の表示条件やルート分岐条件に使用
    /// </summary>
    [Serializable]
    public class StoryCondition
    {
        [Tooltip("条件で参照する変数名")]
        public string variableName = string.Empty;

        [Tooltip("変数の型")]
        public VariableType variableType = VariableType.Integer;

        [Tooltip("比較演算子")]
        public ComparisonOperator comparisonOperator = ComparisonOperator.GreaterThanOrEqual;

        [Tooltip("整数値との比較値")]
        public int intValue;

        [Tooltip("真偽値との比較値")]
        public bool boolValue;

        [Tooltip("文字列との比較値")]
        public string stringValue = string.Empty;

        /// <summary>
        /// 条件を評価する
        /// </summary>
        /// <param name="currentValue">現在の変数値</param>
        /// <returns>条件を満たす場合はtrue</returns>
        public bool Evaluate(object? currentValue)
        {
            if (currentValue == null)
            {
                return false;
            }

            return variableType switch
            {
                VariableType.Integer => EvaluateInteger(Convert.ToInt32(currentValue)),
                VariableType.Boolean => EvaluateBoolean(Convert.ToBoolean(currentValue)),
                VariableType.String => EvaluateString(currentValue.ToString() ?? string.Empty),
                _ => false
            };
        }

        private bool EvaluateInteger(int current)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => current == intValue,
                ComparisonOperator.NotEqual => current != intValue,
                ComparisonOperator.GreaterThan => current > intValue,
                ComparisonOperator.GreaterThanOrEqual => current >= intValue,
                ComparisonOperator.LessThan => current < intValue,
                ComparisonOperator.LessThanOrEqual => current <= intValue,
                _ => false
            };
        }

        private bool EvaluateBoolean(bool current)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => current == boolValue,
                ComparisonOperator.NotEqual => current != boolValue,
                _ => false
            };
        }

        private bool EvaluateString(string current)
        {
            return comparisonOperator switch
            {
                ComparisonOperator.Equal => current == stringValue,
                ComparisonOperator.NotEqual => current != stringValue,
                _ => false
            };
        }
    }
}

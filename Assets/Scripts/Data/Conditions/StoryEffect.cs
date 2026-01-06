#nullable enable
using System;
using UnityEngine;

namespace LifeLike.Data.Conditions
{
    /// <summary>
    /// 効果の操作タイプ
    /// </summary>
    public enum EffectOperation
    {
        Set,        // 値を設定
        Add,        // 値を加算
        Subtract,   // 値を減算
        Toggle      // ブール値を反転
    }

    /// <summary>
    /// ストーリー選択時の効果を定義する
    /// 選択肢を選んだ時に変数を変更する
    /// </summary>
    [Serializable]
    public class StoryEffect
    {
        [Tooltip("変更する変数名")]
        public string variableName = string.Empty;

        [Tooltip("変数の型")]
        public VariableType variableType = VariableType.Integer;

        [Tooltip("操作タイプ")]
        public EffectOperation operation = EffectOperation.Add;

        [Tooltip("整数値の変更量または設定値")]
        public int intValue;

        [Tooltip("真偽値の設定値")]
        public bool boolValue;

        [Tooltip("文字列の設定値")]
        public string stringValue = string.Empty;

        /// <summary>
        /// 効果を適用した結果を返す
        /// </summary>
        /// <param name="currentValue">現在の変数値</param>
        /// <returns>適用後の値</returns>
        public object Apply(object? currentValue)
        {
            return variableType switch
            {
                VariableType.Integer => ApplyInteger(currentValue != null ? Convert.ToInt32(currentValue) : 0),
                VariableType.Boolean => ApplyBoolean(currentValue != null && Convert.ToBoolean(currentValue)),
                VariableType.String => ApplyString(),
                _ => currentValue ?? GetDefaultValue()
            };
        }

        private int ApplyInteger(int current)
        {
            return operation switch
            {
                EffectOperation.Set => intValue,
                EffectOperation.Add => current + intValue,
                EffectOperation.Subtract => current - intValue,
                _ => current
            };
        }

        private bool ApplyBoolean(bool current)
        {
            return operation switch
            {
                EffectOperation.Set => boolValue,
                EffectOperation.Toggle => !current,
                _ => current
            };
        }

        private string ApplyString()
        {
            // 文字列は常にSetのみ
            return stringValue;
        }

        /// <summary>
        /// 変数タイプに応じたデフォルト値を返す
        /// </summary>
        private object GetDefaultValue()
        {
            return variableType switch
            {
                VariableType.Integer => 0,
                VariableType.Boolean => false,
                VariableType.String => string.Empty,
                _ => 0
            };
        }
    }
}

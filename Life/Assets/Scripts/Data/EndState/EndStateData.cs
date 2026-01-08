#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using LifeLike.Data.Flag;
using UnityEngine;

namespace LifeLike.Data.EndState
{
    /// <summary>
    /// エンドステートの種類
    /// </summary>
    public enum EndStateType
    {
        // === Night01 共通 ===
        /// <summary>封じ込め — インシデントがプロトコル内で管理された</summary>
        Contained,

        /// <summary>露出 — 接続が作られ、真実が浮上</summary>
        Exposed,

        /// <summary>共犯 — オペレーターが隠蔽を可能にした</summary>
        Complicit,

        /// <summary>要注意 — オペレーターがレビュー対象としてマーク</summary>
        Flagged,

        /// <summary>吸収 — オペレーターが日常に消えた</summary>
        Absorbed,

        // === Night02 追加 ===
        /// <summary>警戒 — 危険を認識し、情報を収集した</summary>
        Vigilant,

        /// <summary>従順 — システムに従った</summary>
        Compliant,

        /// <summary>接続 — 断片を結びつけた</summary>
        Connected,

        /// <summary>孤立 — 誰も信用しなかった</summary>
        Isolated,

        /// <summary>日常 — 普通の夜として処理した</summary>
        Routine,

        // === Night03 追加 ===
        /// <summary>分かれ道 — 真理を助け、佐藤に情報を渡した</summary>
        Crossroads,

        /// <summary>介入 — 真理を助け、佐藤に情報を渡さなかった</summary>
        Intervention,

        /// <summary>開示 — 真理を助けず、佐藤に情報を渡した</summary>
        Disclosure,

        /// <summary>沈黙 — 真理を助けず、佐藤に情報を渡さなかった</summary>
        Silence
    }

    /// <summary>
    /// スコア条件
    /// </summary>
    [Serializable]
    public class ScoreCondition
    {
        [Tooltip("対象のフラグカテゴリ")]
        public FlagCategory category = FlagCategory.Escalation;

        [Tooltip("比較演算子")]
        public ComparisonOperator comparison = ComparisonOperator.GreaterThanOrEqual;

        [Tooltip("比較値")]
        public int value = 0;

        /// <summary>
        /// 条件を評価する
        /// </summary>
        public bool Evaluate(int currentScore)
        {
            return comparison switch
            {
                ComparisonOperator.Equal => currentScore == value,
                ComparisonOperator.NotEqual => currentScore != value,
                ComparisonOperator.GreaterThan => currentScore > value,
                ComparisonOperator.GreaterThanOrEqual => currentScore >= value,
                ComparisonOperator.LessThan => currentScore < value,
                ComparisonOperator.LessThanOrEqual => currentScore <= value,
                _ => false
            };
        }
    }

    /// <summary>
    /// フラグ条件（単純なboolフラグチェック用）
    /// </summary>
    [Serializable]
    public class FlagCondition
    {
        [Tooltip("フラグID")]
        public string flagId = string.Empty;

        [Tooltip("必要な値")]
        public bool requiredValue = true;
    }

    /// <summary>
    /// エンドステート条件
    /// </summary>
    [Serializable]
    public class EndStateCondition
    {
        [Tooltip("エンドステートの種類")]
        public EndStateType endStateType = EndStateType.Contained;

        [Tooltip("評価優先順位（低い方が先にチェック）")]
        public int priority = 100;

        [Tooltip("スコア条件（すべて満たす必要あり）")]
        public List<ScoreCondition> scoreConditions = new();

        [Tooltip("フラグ条件（すべて満たす必要あり）")]
        public List<FlagCondition> flagConditions = new();

        [Tooltip("説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;
    }

    /// <summary>
    /// エンディングマッピング
    /// エンドステート + 被害者生存 → エンディングID
    /// </summary>
    [Serializable]
    public class EndingMapping
    {
        [Tooltip("エンドステート")]
        public EndStateType endState = EndStateType.Contained;

        [Tooltip("被害者が生存している場合")]
        public string endingIdIfSurvived = string.Empty;

        [Tooltip("被害者が死亡している場合")]
        public string endingIdIfDied = string.Empty;

        [Tooltip("被害者の生存状態に関係なく適用（両方空の場合）")]
        public string endingIdRegardless = string.Empty;
    }

    /// <summary>
    /// 被害者生存条件
    /// </summary>
    [Serializable]
    public class VictimSurvivalCondition
    {
        [Tooltip("派遣が必要か")]
        public bool requiresDispatch = true;

        [Tooltip("派遣の最大許容時刻（分単位、これ以前なら生存）")]
        public int maxDispatchTimeMinutes = 169; // 02:49

        [Tooltip("派遣フラグID")]
        public string dispatchFlagId = "emergency_dispatched";
    }

}

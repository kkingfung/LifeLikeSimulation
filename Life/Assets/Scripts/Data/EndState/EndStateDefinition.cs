#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.EndState
{
    /// <summary>
    /// エンドステート定義データ
    /// </summary>
    [CreateAssetMenu(fileName = "EndStateDefinition", menuName = "LifeLike/Operator/End State Definition")]
    public class EndStateDefinition : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("夜のID")]
        public string nightId = string.Empty;

        [Header("エンドステート条件")]
        [Tooltip("エンドステートの条件リスト（優先順位順に評価）")]
        public List<EndStateCondition> conditions = new();

        [Header("エンディングマッピング")]
        [Tooltip("エンドステートとエンディングの対応")]
        public List<EndingMapping> endingMappings = new();

        [Header("被害者生存")]
        [Tooltip("被害者の生存条件")]
        public VictimSurvivalCondition victimSurvival = new();

        [Header("デフォルト")]
        [Tooltip("どの条件にも合致しない場合のエンドステート")]
        public EndStateType defaultEndState = EndStateType.Contained;

        [Tooltip("デフォルトのエンディングID")]
        public string defaultEndingId = "ending_neutral";

        [Header("エンディング")]
        [Tooltip("利用可能なエンディングリスト")]
        public List<ScenarioEnding> endings = new();

        /// <summary>
        /// 優先順位でソートされたエンドステート条件を取得
        /// </summary>
        public List<EndStateCondition> GetSortedConditions()
        {
            var sorted = new List<EndStateCondition>(conditions);
            sorted.Sort((a, b) => a.priority.CompareTo(b.priority));
            return sorted;
        }

        /// <summary>
        /// エンディングIDからエンディングを取得
        /// </summary>
        public ScenarioEnding? GetEndingById(string endingId)
        {
            return endings.Find(e => e.endingId == endingId);
        }

        /// <summary>
        /// エンドステートに対応するエンディングマッピングを取得
        /// </summary>
        public EndingMapping? GetEndingMapping(EndStateType endState)
        {
            return endingMappings.Find(m => m.endState == endState);
        }

        /// <summary>
        /// エンドステートと被害者生存状態からエンディングIDを取得
        /// </summary>
        public string GetEndingId(EndStateType endState, bool victimSurvived)
        {
            var mapping = GetEndingMapping(endState);
            if (mapping == null)
            {
                return defaultEndingId;
            }

            // 被害者生存状態に関係なく適用されるエンディング
            if (!string.IsNullOrEmpty(mapping.endingIdRegardless))
            {
                return mapping.endingIdRegardless;
            }

            // 被害者の生存状態によって分岐
            if (victimSurvived)
            {
                return !string.IsNullOrEmpty(mapping.endingIdIfSurvived)
                    ? mapping.endingIdIfSurvived
                    : defaultEndingId;
            }
            else
            {
                return !string.IsNullOrEmpty(mapping.endingIdIfDied)
                    ? mapping.endingIdIfDied
                    : defaultEndingId;
            }
        }
    }
}

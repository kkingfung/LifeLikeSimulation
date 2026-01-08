#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Flag
{
    /// <summary>
    /// 夜のフラグ定義データ
    /// </summary>
    [CreateAssetMenu(fileName = "NightFlagsDefinition", menuName = "LifeLike/Operator/Night Flags Definition")]
    public class NightFlagsDefinition : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("夜のID")]
        public string nightId = string.Empty;

        [Header("フラグ定義")]
        [Tooltip("この夜で使用するフラグの定義")]
        public List<FlagDefinition> flagDefinitions = new();

        [Header("相互排他ルール")]
        [Tooltip("フラグの相互排他ルール")]
        public List<MutualExclusionRule> mutualExclusionRules = new();

        /// <summary>
        /// フラグIDからフラグ定義を取得
        /// </summary>
        public FlagDefinition? GetFlagDefinition(string flagId)
        {
            return flagDefinitions.Find(f => f.flagId == flagId);
        }

        /// <summary>
        /// カテゴリに属するフラグ定義を取得
        /// </summary>
        public List<FlagDefinition> GetFlagsByCategory(FlagCategory category)
        {
            return flagDefinitions.FindAll(f => f.category == category);
        }

        /// <summary>
        /// フラグが設定されたときにクリアすべきフラグを取得
        /// </summary>
        public List<string> GetCancelledFlags(string flagId)
        {
            var result = new List<string>();

            // フラグ定義からのキャンセル
            var definition = GetFlagDefinition(flagId);
            if (definition != null)
            {
                result.AddRange(definition.cancelsFlags);
            }

            // 相互排他ルールからのキャンセル
            foreach (var rule in mutualExclusionRules)
            {
                if (rule.whenFlagSet == flagId)
                {
                    result.AddRange(rule.cancelFlags);
                }
            }

            return result;
        }
    }
}

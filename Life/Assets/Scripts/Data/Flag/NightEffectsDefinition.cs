#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Flag
{
    /// <summary>
    /// 夜間エフェクト定義
    /// </summary>
    [CreateAssetMenu(fileName = "NightEffects", menuName = "LifeLike/Operator/Night Effects")]
    public class NightEffectsDefinition : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("対象の夜ID")]
        public string targetNightId = string.Empty;

        [Tooltip("説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("エフェクトリスト")]
        [Tooltip("この夜に適用されるエフェクト")]
        public List<NightEffect> effects = new();

        /// <summary>
        /// 指定した夜の結果に基づいて適用されるエフェクトを取得
        /// </summary>
        public List<NightEffect> GetApplicableEffects(CrossNightState state)
        {
            var applicable = new List<NightEffect>();

            foreach (var effect in effects)
            {
                var nightResult = state.GetNightResult(effect.sourceNightId);
                if (nightResult == null) continue;

                // エンドステートが一致するか確認
                if (nightResult.endState != effect.requiredEndState) continue;

                // 必要フラグがすべて設定されているか確認
                bool allFlagsSet = true;
                foreach (var flagId in effect.requiredFlags)
                {
                    if (!state.IsPersistentFlagSet(flagId))
                    {
                        allFlagsSet = false;
                        break;
                    }
                }

                if (allFlagsSet)
                {
                    applicable.Add(effect);
                }
            }

            return applicable;
        }
    }
}

#nullable enable
using System.Collections.Generic;
using LifeLike.Data.Conditions;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 証拠テンプレート（ScriptableObject）
    /// シナリオ作成時に使用
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvidence", menuName = "LifeLike/Operator/Evidence Template")]
    public class EvidenceTemplate : ScriptableObject
    {
        [Tooltip("証拠の基本データ")]
        public EvidenceData data = new();

        [Header("発見条件")]
        [Tooltip("この証拠が発見される条件")]
        public List<Conditions.StoryCondition> discoveryConditions = new();

        [Header("使用効果")]
        [Tooltip("この証拠を使用した時の効果")]
        public List<StoryEffect> useEffects = new();
    }
}

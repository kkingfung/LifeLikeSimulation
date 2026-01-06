#nullable enable
using System;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// トランジションのタイプ
    /// </summary>
    public enum TransitionType
    {
        None,           // 即時切り替え
        FadeToBlack,    // 黒フェード
        FadeToWhite,    // 白フェード
        Crossfade,      // クロスフェード
        Custom          // カスタムアニメーション
    }

    /// <summary>
    /// トランジション設定
    /// シーン間の遷移演出を定義する
    /// </summary>
    [Serializable]
    public class TransitionSettings
    {
        [Tooltip("トランジションのタイプ")]
        public TransitionType type = TransitionType.FadeToBlack;

        [Tooltip("トランジションの長さ（秒）")]
        [Range(0f, 5f)]
        public float duration = 0.5f;

        [Tooltip("トランジションのイージングカーブ")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("カスタムトランジションのプレハブ（Customタイプの場合）")]
        public GameObject? customTransitionPrefab;

        /// <summary>
        /// デフォルトのトランジション設定を作成
        /// </summary>
        public static TransitionSettings Default => new TransitionSettings
        {
            type = TransitionType.FadeToBlack,
            duration = 0.5f,
            curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f)
        };

        /// <summary>
        /// 即時切り替えのトランジション設定を作成
        /// </summary>
        public static TransitionSettings Instant => new TransitionSettings
        {
            type = TransitionType.None,
            duration = 0f
        };
    }
}

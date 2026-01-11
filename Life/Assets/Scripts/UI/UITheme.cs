#nullable enable
using UnityEngine;

namespace LifeLike.UI
{
    /// <summary>
    /// UIテーマ設定を管理するScriptableObject
    /// 色、フォント、スタイルを一元管理
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "LifeLike/UI/Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("カラーパレット - 背景")]
        [Tooltip("最も暗い背景色（Result、Endingなど）")]
        public Color backgroundDarkest = new Color(0.03f, 0.04f, 0.06f, 1f);

        [Tooltip("暗い背景色（MainMenu、Operatorなど）")]
        public Color backgroundDark = new Color(0.08f, 0.08f, 0.12f, 1f);

        [Tooltip("パネル背景色")]
        public Color panelBackground = new Color(0.08f, 0.1f, 0.14f, 0.95f);

        [Tooltip("ボタン背景色")]
        public Color buttonBackground = new Color(0.12f, 0.14f, 0.2f, 1f);

        [Header("カラーパレット - アクセント")]
        [Tooltip("プライマリアクセント（シアン）")]
        public Color accentPrimary = new Color(0f, 0.9f, 1f, 1f);

        [Tooltip("セカンダリアクセント（ティール）")]
        public Color accentSecondary = new Color(0f, 0.6f, 0.7f, 1f);

        [Tooltip("警告色（ゴールド）")]
        public Color accentWarning = new Color(0.9f, 0.75f, 0.2f, 1f);

        [Tooltip("危険色（レッド）")]
        public Color accentDanger = new Color(0.8f, 0.2f, 0.2f, 1f);

        [Tooltip("成功色（グリーン）")]
        public Color accentSuccess = new Color(0.2f, 0.8f, 0.4f, 1f);

        [Header("カラーパレット - テキスト")]
        [Tooltip("プライマリテキスト（白）")]
        public Color textPrimary = new Color(1f, 1f, 1f, 1f);

        [Tooltip("セカンダリテキスト（グレー）")]
        public Color textSecondary = new Color(0.7f, 0.75f, 0.8f, 1f);

        [Tooltip("ミュートテキスト（暗いグレー）")]
        public Color textMuted = new Color(0.4f, 0.45f, 0.5f, 1f);

        [Header("カラーパレット - 状態")]
        [Tooltip("通話アクティブ")]
        public Color stateCallActive = new Color(0f, 0.5f, 0.55f, 0.9f);

        [Tooltip("着信中")]
        public Color stateIncoming = new Color(0f, 0.7f, 0.8f, 1f);

        [Tooltip("不在着信")]
        public Color stateMissedCall = new Color(0.8f, 0.2f, 0.2f, 1f);

        [Header("フォント設定")]
        [Tooltip("タイトル用フォント")]
        public Font? titleFont;

        [Tooltip("本文用フォント")]
        public Font? bodyFont;

        [Tooltip("モノスペースフォント（時計など）")]
        public Font? monoFont;

        [Header("フォントサイズ")]
        public int fontSizeTitle = 64;
        public int fontSizeHeading = 32;
        public int fontSizeSubheading = 24;
        public int fontSizeBody = 20;
        public int fontSizeSmall = 16;
        public int fontSizeTiny = 14;

        [Header("アニメーション設定")]
        [Tooltip("ホバー時の明るさ倍率")]
        [Range(1f, 1.5f)]
        public float hoverBrightness = 1.2f;

        [Tooltip("押下時の明るさ倍率")]
        [Range(0.5f, 1f)]
        public float pressBrightness = 0.8f;

        [Tooltip("フェード時間")]
        public float fadeTime = 0.1f;

        [Header("視覚効果設定")]
        [Tooltip("スキャンラインを有効にする")]
        public bool enableScanlines = true;

        [Tooltip("スキャンラインの強度")]
        [Range(0f, 0.5f)]
        public float scanlineIntensity = 0.1f;

        [Tooltip("ビネット効果を有効にする")]
        public bool enableVignette = true;

        [Tooltip("ビネットの強度")]
        [Range(0f, 1f)]
        public float vignetteIntensity = 0.3f;

        [Tooltip("CRTカーブ効果を有効にする")]
        public bool enableCRTCurve = false;

        /// <summary>
        /// デフォルトテーマを作成
        /// </summary>
        public static UITheme CreateDefault()
        {
            var theme = CreateInstance<UITheme>();
            // デフォルト値は上記のフィールド初期化で設定済み
            return theme;
        }

        /// <summary>
        /// エンディングタイプに応じたテーマカラーを取得
        /// </summary>
        public Color GetEndingColor(EndingType endingType)
        {
            return endingType switch
            {
                EndingType.Best => accentSuccess,
                EndingType.Good => accentPrimary,
                EndingType.Neutral => textSecondary,
                EndingType.Bad => accentDanger,
                _ => accentPrimary
            };
        }

        /// <summary>
        /// タイマー残り時間に応じた色を取得（1.0 = 満タン、0.0 = 空）
        /// </summary>
        public Color GetTimerColor(float normalizedTime)
        {
            if (normalizedTime > 0.5f)
            {
                // 緑→黄色
                return Color.Lerp(accentWarning, accentSuccess, (normalizedTime - 0.5f) * 2f);
            }
            else
            {
                // 黄色→赤
                return Color.Lerp(accentDanger, accentWarning, normalizedTime * 2f);
            }
        }
    }

    /// <summary>
    /// エンディングタイプ
    /// </summary>
    public enum EndingType
    {
        Best,
        Good,
        Neutral,
        Bad
    }
}

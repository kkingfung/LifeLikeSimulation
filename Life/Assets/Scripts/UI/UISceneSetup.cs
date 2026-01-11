#nullable enable
using UnityEngine;
using UnityEngine.UI;
using LifeLike.UI.Effects;

namespace LifeLike.UI
{
    /// <summary>
    /// シーンのUIセットアップを行うコンポーネント
    /// CRT効果、ボタンホバー効果、オーディオフィードバックを自動的に適用
    /// </summary>
    public class UISceneSetup : MonoBehaviour
    {
        [Header("テーマ設定")]
        [SerializeField] private UITheme? _theme;

        [Header("CRT効果設定")]
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private Canvas? _targetCanvas;

        [Header("自動適用設定")]
        [SerializeField] private bool _autoApplyButtonEffects = true;
        [SerializeField] private bool _autoApplyAudioFeedback = true;

        [Header("除外設定")]
        [SerializeField] private string[] _excludeButtonNames = new string[0];

        private GameObject? _crtOverlay;

        private void Start()
        {
            SetupTheme();
            SetupCRTEffect();

            if (_autoApplyButtonEffects)
            {
                ApplyButtonEffectsToAll();
            }

            if (_autoApplyAudioFeedback)
            {
                ApplyAudioFeedbackToAll();
            }
        }

        private void OnDestroy()
        {
            if (_crtOverlay != null)
            {
                Destroy(_crtOverlay);
            }
        }

        /// <summary>
        /// テーマを設定
        /// </summary>
        private void SetupTheme()
        {
            if (_theme != null)
            {
                UIThemeManager.Instance.Theme = _theme;
            }
        }

        /// <summary>
        /// CRT効果をセットアップ
        /// </summary>
        private void SetupCRTEffect()
        {
            if (!_enableCRTEffect) return;

            var canvas = _targetCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                UnityEngine.Debug.LogWarning("[UISceneSetup] Canvasが見つかりません。CRT効果をスキップします。");
                return;
            }

            // CRTオーバーレイを作成
            _crtOverlay = new GameObject("CRTOverlay");
            _crtOverlay.transform.SetParent(canvas.transform, false);

            var rectTransform = _crtOverlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var rawImage = _crtOverlay.AddComponent<RawImage>();
            rawImage.raycastTarget = false;

            var crtEffect = _crtOverlay.AddComponent<CRTEffect>();

            // テーマから設定を適用
            var theme = UIThemeManager.Instance.Theme;
            crtEffect.ApplyTheme(theme);

            // 最前面に配置
            _crtOverlay.transform.SetAsLastSibling();
        }

        /// <summary>
        /// すべてのボタンにホバー効果を適用
        /// </summary>
        private void ApplyButtonEffectsToAll()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            var theme = UIThemeManager.Instance.Theme;

            foreach (var button in buttons)
            {
                if (IsExcluded(button.gameObject.name)) continue;
                if (button.GetComponent<ButtonHoverEffect>() != null) continue;

                var hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
                hoverEffect.ApplyTheme(theme);
            }
        }

        /// <summary>
        /// すべてのボタンにオーディオフィードバックを適用
        /// </summary>
        private void ApplyAudioFeedbackToAll()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (var button in buttons)
            {
                if (IsExcluded(button.gameObject.name)) continue;
                if (button.GetComponent<ButtonAudioFeedback>() != null) continue;

                button.gameObject.AddComponent<ButtonAudioFeedback>();
            }
        }

        /// <summary>
        /// 除外対象かどうかをチェック
        /// </summary>
        private bool IsExcluded(string name)
        {
            foreach (var excludeName in _excludeButtonNames)
            {
                if (name.Contains(excludeName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 特定のボタンにアクセントカラーを設定
        /// </summary>
        public void SetButtonAccentColor(Button button, Color color)
        {
            var hoverEffect = button.GetComponent<ButtonHoverEffect>();
            if (hoverEffect != null)
            {
                hoverEffect.SetAccentColor(color);
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        /// <summary>
        /// CRT効果の有効/無効を切り替え
        /// </summary>
        public void SetCRTEffectEnabled(bool enabled)
        {
            if (_crtOverlay != null)
            {
                _crtOverlay.SetActive(enabled);
            }
        }

        /// <summary>
        /// タイトルテキストにタイプライター効果を適用
        /// </summary>
        public void ApplyTypewriterToText(Text text)
        {
            if (text.GetComponent<TypewriterEffect>() != null) return;

            var typewriter = text.gameObject.AddComponent<TypewriterEffect>();
            typewriter.StartTyping(text.text);
        }
    }
}

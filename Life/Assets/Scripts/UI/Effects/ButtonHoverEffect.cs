#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// ボタンホバー効果を提供するコンポーネント
    /// ホバー時の色変化、スケール変化、グロー効果を実現
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("色変化設定")]
        [SerializeField] private bool _enableColorChange = true;
        [SerializeField] private Color _normalColor = new Color(0.12f, 0.14f, 0.2f, 1f);
        [SerializeField] private Color _hoverColor = new Color(0.18f, 0.21f, 0.3f, 1f);
        [SerializeField] private Color _pressColor = new Color(0.08f, 0.1f, 0.15f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.1f, 0.1f, 0.12f, 0.5f);

        [Header("テキスト色変化設定")]
        [SerializeField] private bool _enableTextColorChange = true;
        [SerializeField] private Color _textNormalColor = Color.white;
        [SerializeField] private Color _textHoverColor = new Color(0f, 0.9f, 1f, 1f);

        [Header("スケール変化設定")]
        [SerializeField] private bool _enableScaleChange = true;
        [SerializeField] private float _hoverScale = 1.02f;
        [SerializeField] private float _pressScale = 0.98f;

        [Header("グロー効果設定")]
        [SerializeField] private bool _enableGlow = true;
        [SerializeField] private Color _glowColor = new Color(0f, 0.7f, 0.8f, 0.3f);
        [SerializeField] private Outline? _glowOutline;

        [Header("アニメーション設定")]
        [SerializeField] private float _transitionSpeed = 10f;

        private Button? _button;
        private Image? _image;
        private Text? _text;
        private Vector3 _originalScale;
        private bool _isHovered;

        // ターゲット値
        private Color _targetColor;
        private Color _targetTextColor;
        private Vector3 _targetScale;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<Text>();
            _originalScale = transform.localScale;

            // 初期状態を設定
            _targetColor = _normalColor;
            _targetTextColor = _textNormalColor;
            _targetScale = _originalScale;

            // グローアウトラインがない場合は作成
            if (_enableGlow && _glowOutline == null)
            {
                _glowOutline = GetComponent<Outline>();
                if (_glowOutline == null)
                {
                    _glowOutline = gameObject.AddComponent<Outline>();
                    _glowOutline.effectColor = new Color(0, 0, 0, 0);
                    _glowOutline.effectDistance = new Vector2(2, 2);
                }
            }
        }

        private void Update()
        {
            if (_button == null) return;

            // ボタンが無効の場合
            if (!_button.interactable)
            {
                _targetColor = _disabledColor;
                _targetScale = _originalScale;
                if (_glowOutline != null)
                {
                    _glowOutline.effectColor = Color.clear;
                }
            }

            // 色を補間
            if (_enableColorChange && _image != null)
            {
                _image.color = Color.Lerp(_image.color, _targetColor, Time.unscaledDeltaTime * _transitionSpeed);
            }

            // テキスト色を補間
            if (_enableTextColorChange && _text != null)
            {
                _text.color = Color.Lerp(_text.color, _targetTextColor, Time.unscaledDeltaTime * _transitionSpeed);
            }

            // スケールを補間
            if (_enableScaleChange)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * _transitionSpeed);
            }

            // グロー効果を補間
            if (_enableGlow && _glowOutline != null)
            {
                var targetGlow = _isHovered && _button.interactable ? _glowColor : Color.clear;
                _glowOutline.effectColor = Color.Lerp(_glowOutline.effectColor, targetGlow, Time.unscaledDeltaTime * _transitionSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            _isHovered = true;
            _targetColor = _hoverColor;
            _targetTextColor = _textHoverColor;
            _targetScale = _originalScale * _hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _targetColor = _normalColor;
            _targetTextColor = _textNormalColor;
            _targetScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            _targetColor = _pressColor;
            _targetScale = _originalScale * _pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isHovered)
            {
                _targetColor = _hoverColor;
                _targetScale = _originalScale * _hoverScale;
            }
            else
            {
                _targetColor = _normalColor;
                _targetScale = _originalScale;
            }
        }

        /// <summary>
        /// テーマから設定を適用
        /// </summary>
        public void ApplyTheme(UITheme theme)
        {
            _normalColor = theme.buttonBackground;
            _hoverColor = theme.buttonBackground * theme.hoverBrightness;
            _pressColor = theme.buttonBackground * theme.pressBrightness;
            _textHoverColor = theme.accentPrimary;
            _glowColor = new Color(theme.accentSecondary.r, theme.accentSecondary.g, theme.accentSecondary.b, 0.3f);
            _transitionSpeed = 1f / theme.fadeTime;
        }

        /// <summary>
        /// アクセントカラーを設定（特別なボタン用）
        /// </summary>
        public void SetAccentColor(Color color)
        {
            _normalColor = color;
            _hoverColor = color * 1.2f;
            _pressColor = color * 0.8f;
            _targetColor = _normalColor;
        }
    }
}

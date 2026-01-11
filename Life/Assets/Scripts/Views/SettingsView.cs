#nullable enable
using System.ComponentModel;
using LifeLike.Controllers;
using LifeLike.UI;
using LifeLike.UI.Effects;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// 設定画面のView
    /// サブシーンモード（オーバーレイ）と通常モード（フルシーン）の両方に対応
    /// CRT効果、ボタンホバー効果を含む
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private SettingsSceneController? _controller;

        [Header("Audio Settings")]
        [SerializeField] private Slider? _masterVolumeSlider;
        [SerializeField] private Slider? _bgmVolumeSlider;
        [SerializeField] private Slider? _sfxVolumeSlider;
        [SerializeField] private Slider? _voiceVolumeSlider;
        [SerializeField] private Toggle? _muteToggle;

        [Header("Display Settings")]
        [SerializeField] private Toggle? _fullscreenToggle;
        [SerializeField] private Dropdown? _resolutionDropdown;
        [SerializeField] private Dropdown? _qualityDropdown;

        [Header("Game Settings")]
        [SerializeField] private Slider? _textSpeedSlider;
        [SerializeField] private Toggle? _autoAdvanceToggle;
        [SerializeField] private Slider? _autoAdvanceDelaySlider;
        [SerializeField] private Toggle? _skipUnreadToggle;
        [SerializeField] private Dropdown? _languageDropdown;

        [Header("Buttons")]
        [SerializeField] private Button? _backButton;
        [SerializeField] private Button? _resetButton;

        [Header("UI Effects")]
        [SerializeField] private UITheme? _theme;
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private bool _enableButtonEffects = true;
        [SerializeField] private Canvas? _mainCanvas;

        [Header("UI References")]
        [SerializeField] private RectTransform? _settingsPanel;
        [SerializeField] private Text? _titleText;

        private SettingsViewModel? _viewModel;
        private bool _isUpdatingUI = false;
        private GameObject? _crtOverlay;
        private TypewriterEffect? _titleTypewriter;
        private SlideEffect? _panelSlide;
        private FadeEffect? _screenFade;

        private void Awake()
        {
            // コントローラーを検索
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<SettingsSceneController>();
            }

            if (_controller == null)
            {
                Debug.LogError("[SettingsView] SettingsSceneControllerが見つかりません。");
                return;
            }

            _viewModel = new SettingsViewModel();
        }

        private void Start()
        {
            if (_viewModel == null) return;

            // テーマを設定
            SetupTheme();

            // UI効果をセットアップ
            SetupUIEffects();

            // タイトルにタイプライター効果を適用
            SetupTitleEffect();

            SetupUI();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnBackRequested += OnBackRequested;
            UpdateUIFromViewModel();

            if (_controller?.IsSubsceneMode ?? false)
            {
                Debug.Log("[SettingsView] サブシーンモードで起動");
            }
        }

        private void Update()
        {
            // ESCキーで戻る
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnBackClicked();
            }
        }

        /// <summary>
        /// タイトルにタイプライター効果を適用
        /// </summary>
        private void SetupTitleEffect()
        {
            if (_titleText == null) return;

            _titleTypewriter = _titleText.GetComponent<TypewriterEffect>();
            if (_titleTypewriter == null)
            {
                _titleTypewriter = _titleText.gameObject.AddComponent<TypewriterEffect>();
            }
            _titleTypewriter.StartTyping("SETTINGS");
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
        /// UI効果をセットアップ
        /// </summary>
        private void SetupUIEffects()
        {
            bool isSubsceneMode = _controller?.IsSubsceneMode ?? false;

            // サブシーンモードの場合、CRT効果は親シーンにあるのでスキップ
            if (_enableCRTEffect && !isSubsceneMode)
            {
                SetupCRTEffect();
            }

            if (_enableButtonEffects)
            {
                SetupButtonEffects();
            }

            // パネルのスライド効果をセットアップ
            SetupPanelSlide();

            // 画面フェード効果をセットアップ（通常モードのみ）
            if (!isSubsceneMode)
            {
                SetupScreenFade();
            }
        }

        /// <summary>
        /// パネルのスライド効果をセットアップ
        /// </summary>
        private void SetupPanelSlide()
        {
            if (_settingsPanel == null) return;

            // CanvasGroupを追加
            var canvasGroup = _settingsPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _settingsPanel.gameObject.AddComponent<CanvasGroup>();
            }

            _panelSlide = _settingsPanel.GetComponent<SlideEffect>();
            if (_panelSlide == null)
            {
                _panelSlide = _settingsPanel.gameObject.AddComponent<SlideEffect>();
                _panelSlide.SetDirection(SlideEffect.SlideDirection.Down);
            }

            // パネルをスライドイン
            _panelSlide.SlideIn(0.3f);
        }

        /// <summary>
        /// 画面フェード効果をセットアップ
        /// </summary>
        private void SetupScreenFade()
        {
            var canvas = _mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null) return;

            var canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }

            _screenFade = canvas.GetComponent<FadeEffect>();
            if (_screenFade == null)
            {
                _screenFade = canvas.gameObject.AddComponent<FadeEffect>();
            }

            _screenFade.FadeIn(0.5f);
        }

        /// <summary>
        /// CRT効果をセットアップ
        /// </summary>
        private void SetupCRTEffect()
        {
            var canvas = _mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogWarning("[SettingsView] Canvasが見つかりません。CRT効果をスキップします。");
                return;
            }

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
            crtEffect.ApplyTheme(UIThemeManager.Instance.Theme);

            _crtOverlay.transform.SetAsLastSibling();
        }

        /// <summary>
        /// ボタン効果をセットアップ
        /// </summary>
        private void SetupButtonEffects()
        {
            var theme = UIThemeManager.Instance.Theme;

            AddButtonEffects(_backButton, theme);
            AddButtonEffects(_resetButton, theme, ButtonAudioFeedback.ClickSoundType.Warning);
        }

        /// <summary>
        /// ボタンにエフェクトを追加
        /// </summary>
        private void AddButtonEffects(Button? button, UITheme theme, ButtonAudioFeedback.ClickSoundType soundType = ButtonAudioFeedback.ClickSoundType.Default)
        {
            if (button == null) return;

            var hoverEffect = button.gameObject.GetComponent<ButtonHoverEffect>();
            if (hoverEffect == null)
            {
                hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
                hoverEffect.ApplyTheme(theme);
            }

            var audioFeedback = button.gameObject.GetComponent<ButtonAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = button.gameObject.AddComponent<ButtonAudioFeedback>();
                audioFeedback.SetClickSoundType(soundType);
            }
        }

        private void OnDestroy()
        {
            // CRTオーバーレイを破棄
            if (_crtOverlay != null)
            {
                Destroy(_crtOverlay);
            }

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnBackRequested -= OnBackRequested;
                _viewModel.Dispose();
            }
        }

        /// <summary>
        /// UIをセットアップする
        /// </summary>
        private void SetupUI()
        {
            // スライダー
            if (_masterVolumeSlider != null)
            {
                _masterVolumeSlider.minValue = 0f;
                _masterVolumeSlider.maxValue = 1f;
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (_bgmVolumeSlider != null)
            {
                _bgmVolumeSlider.minValue = 0f;
                _bgmVolumeSlider.maxValue = 1f;
                _bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.minValue = 0f;
                _sfxVolumeSlider.maxValue = 1f;
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }

            if (_voiceVolumeSlider != null)
            {
                _voiceVolumeSlider.minValue = 0f;
                _voiceVolumeSlider.maxValue = 1f;
                _voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }

            if (_muteToggle != null)
            {
                _muteToggle.onValueChanged.AddListener(OnMuteChanged);
            }

            if (_textSpeedSlider != null)
            {
                _textSpeedSlider.minValue = 0.5f;
                _textSpeedSlider.maxValue = 2f;
                _textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
            }

            if (_autoAdvanceDelaySlider != null)
            {
                _autoAdvanceDelaySlider.minValue = 0.5f;
                _autoAdvanceDelaySlider.maxValue = 10f;
                _autoAdvanceDelaySlider.onValueChanged.AddListener(OnAutoAdvanceDelayChanged);
            }

            // トグル
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (_autoAdvanceToggle != null)
            {
                _autoAdvanceToggle.onValueChanged.AddListener(OnAutoAdvanceChanged);
            }

            if (_skipUnreadToggle != null)
            {
                _skipUnreadToggle.onValueChanged.AddListener(OnSkipUnreadChanged);
            }

            // ドロップダウン
            if (_resolutionDropdown != null)
            {
                SetupResolutionDropdown();
                _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            if (_qualityDropdown != null)
            {
                SetupQualityDropdown();
                _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (_languageDropdown != null)
            {
                SetupLanguageDropdown();
                _languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            }

            // ボタン
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_resetButton != null)
            {
                _resetButton.onClick.AddListener(OnResetClicked);
            }
        }

        /// <summary>
        /// 解像度ドロップダウンをセットアップ
        /// </summary>
        private void SetupResolutionDropdown()
        {
            if (_resolutionDropdown == null) return;

            _resolutionDropdown.ClearOptions();
            var resolutions = Screen.resolutions;
            var options = new System.Collections.Generic.List<string>();

            foreach (var res in resolutions)
            {
                // Unity 6ではrefreshRateRatioはRefreshRate構造体なので.valueを使用
                double refreshRate = res.refreshRateRatio.value;
                options.Add($"{res.width} x {res.height} @ {refreshRate:F0}Hz");
            }

            if (options.Count == 0)
            {
                options.Add("1920 x 1080");
                options.Add("1280 x 720");
            }

            _resolutionDropdown.AddOptions(options);
        }

        /// <summary>
        /// 品質ドロップダウンをセットアップ
        /// </summary>
        private void SetupQualityDropdown()
        {
            if (_qualityDropdown == null) return;

            _qualityDropdown.ClearOptions();
            var names = QualitySettings.names;
            _qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(names));
        }

        /// <summary>
        /// 言語ドロップダウンをセットアップ
        /// </summary>
        private void SetupLanguageDropdown()
        {
            if (_languageDropdown == null) return;

            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Japanese",
                "English"
            });
        }

        /// <summary>
        /// ViewModelからUIを更新
        /// </summary>
        private void UpdateUIFromViewModel()
        {
            if (_viewModel == null) return;

            _isUpdatingUI = true;

            if (_masterVolumeSlider != null) _masterVolumeSlider.value = _viewModel.MasterVolume;
            if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = _viewModel.BgmVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = _viewModel.SfxVolume;
            if (_voiceVolumeSlider != null) _voiceVolumeSlider.value = _viewModel.VoiceVolume;
            if (_muteToggle != null) _muteToggle.isOn = _viewModel.IsMuted;

            if (_fullscreenToggle != null) _fullscreenToggle.isOn = _viewModel.IsFullscreen;
            if (_resolutionDropdown != null) _resolutionDropdown.value = _viewModel.ResolutionIndex;
            if (_qualityDropdown != null) _qualityDropdown.value = _viewModel.QualityIndex;

            if (_textSpeedSlider != null) _textSpeedSlider.value = _viewModel.TextSpeed;
            if (_autoAdvanceToggle != null) _autoAdvanceToggle.isOn = _viewModel.AutoAdvance;
            if (_autoAdvanceDelaySlider != null)
            {
                _autoAdvanceDelaySlider.value = _viewModel.AutoAdvanceDelay;
                _autoAdvanceDelaySlider.interactable = _viewModel.AutoAdvance;
            }
            if (_skipUnreadToggle != null) _skipUnreadToggle.isOn = _viewModel.SkipUnread;

            if (_languageDropdown != null)
            {
                int langIndex = _viewModel.Language == "Japanese" ? 0 : 1;
                _languageDropdown.value = langIndex;
            }

            _isUpdatingUI = false;
        }

        /// <summary>
        /// ViewModelのプロパティ変更時
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isUpdatingUI) return;
            UpdateUIFromViewModel();
        }

        // イベントハンドラ
        private void OnMasterVolumeChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.MasterVolume = value;
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.BgmVolume = value;
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.SfxVolume = value;
        }

        private void OnVoiceVolumeChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.VoiceVolume = value;
        }

        private void OnMuteChanged(bool value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.IsMuted = value;
        }

        private void OnFullscreenChanged(bool value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.IsFullscreen = value;
        }

        private void OnResolutionChanged(int index)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.ResolutionIndex = index;
        }

        private void OnQualityChanged(int index)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.QualityIndex = index;
        }

        private void OnTextSpeedChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.TextSpeed = value;
        }

        private void OnAutoAdvanceChanged(bool value)
        {
            if (!_isUpdatingUI && _viewModel != null)
            {
                _viewModel.AutoAdvance = value;

                // オート進行が無効な場合、遅延スライダーを無効化
                if (_autoAdvanceDelaySlider != null)
                {
                    _autoAdvanceDelaySlider.interactable = value;
                }
            }
        }

        private void OnAutoAdvanceDelayChanged(float value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.AutoAdvanceDelay = value;
        }

        private void OnSkipUnreadChanged(bool value)
        {
            if (!_isUpdatingUI && _viewModel != null) _viewModel.SkipUnread = value;
        }

        private void OnLanguageChanged(int index)
        {
            if (!_isUpdatingUI && _viewModel != null)
            {
                _viewModel.Language = index == 0 ? "Japanese" : "English";
            }
        }

        private void OnBackClicked()
        {
            _viewModel?.BackCommand.Execute(null);
        }

        private void OnResetClicked()
        {
            _viewModel?.ResetToDefaultCommand.Execute(null);
        }

        private void OnBackRequested()
        {
            _controller?.NavigateBack();
        }
    }
}

#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Controllers;
using LifeLike.Core.Services;
using LifeLike.Data.Localization;
using LifeLike.Services.Core.Localization;
using LifeLike.UI;
using LifeLike.UI.Effects;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// 結果画面のView
    /// クレジットとルートサマリーを表示
    /// CRT効果、タイプライター効果を含む
    /// </summary>
    public class ResultView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private ResultSceneController? _controller;

        [Header("Ending Info")]
        [SerializeField] private Text? _endingTitleText;
        [SerializeField] private Text? _endingDescriptionText;

        [Header("Chapter Results")]
        [SerializeField] private Transform? _chapterResultsContainer;
        [SerializeField] private GameObject? _chapterResultPrefab;

        [Header("Route Summary")]
        [SerializeField] private GameObject? _summaryPanel;
        [SerializeField] private Text? _routeSummaryText;

        [Header("Credits")]
        [SerializeField] private GameObject? _creditsPanel;
        [SerializeField] private ScrollRect? _creditsScrollRect;
        [SerializeField] private float _creditsScrollSpeed = 50f;

        [Header("Buttons")]
        [SerializeField] private Button? _showCreditsButton;
        [SerializeField] private Button? _showSummaryButton;
        [SerializeField] private Button? _returnToMenuButton;
        [SerializeField] private Button? _newGameButton;

        [Header("Button Labels (for localization)")]
        [SerializeField] private Text? _returnToMenuButtonText;
        [SerializeField] private Text? _newGameButtonText;

        [Header("UI Effects")]
        [SerializeField] private UITheme? _theme;
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private bool _enableButtonEffects = true;
        [SerializeField] private bool _enableTypewriterTitle = true;
        [SerializeField] private Canvas? _mainCanvas;

        private ResultViewModel? _viewModel;
        private ILocalizationService? _localizationService;
        private readonly List<GameObject> _chapterResultItems = new();
        private bool _isScrollingCredits;
        private GameObject? _crtOverlay;
        private TypewriterEffect? _titleTypewriter;
        private FadeEffect? _screenFade;
        private SlideEffect? _creditsPanelSlide;
        private SlideEffect? _summaryPanelSlide;

        private void Awake()
        {
            // コントローラーを検索
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<ResultSceneController>();
            }

            if (_controller == null)
            {
                Debug.LogError("[ResultView] ResultSceneControllerが見つかりません。");
            }

            // ローカライズサービスを取得
            _localizationService = ServiceLocator.Instance.Get<ILocalizationService>();
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged += OnLanguageChanged;
            }
        }

        private void InitializeServices()
        {
            if (_controller == null) return;

            if (_controller.OperatorSaveService == null)
            {
                Debug.LogError("[ResultView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new ResultViewModel(_controller.OperatorSaveService);
        }

        private void Start()
        {
            // サービスを初期化（ResultSceneController.Awake()が完了した後）
            InitializeServices();

            if (_viewModel == null) return;

            // テーマを設定
            SetupTheme();

            // UI効果をセットアップ
            SetupUIEffects();

            SetupUI();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnReturnToMenuRequested += OnReturnToMenuRequested;
            _viewModel.OnNewGameRequested += OnNewGameRequested;

            UpdateUI();

            // ローカライズテキストを適用
            ApplyLocalizedTexts();
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
            if (_enableCRTEffect)
            {
                SetupCRTEffect();
            }

            if (_enableButtonEffects)
            {
                SetupButtonEffects();
            }

            // 画面フェード効果をセットアップ
            SetupScreenFade();

            // パネルのスライド効果をセットアップ
            SetupPanelSlides();
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

            // ゆっくりフェードイン（結果画面は演出を強調）
            _screenFade.FadeIn(1.0f);
        }

        /// <summary>
        /// パネルのスライド効果をセットアップ
        /// </summary>
        private void SetupPanelSlides()
        {
            // クレジットパネル
            if (_creditsPanel != null)
            {
                var canvasGroup = _creditsPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _creditsPanel.AddComponent<CanvasGroup>();
                }

                _creditsPanelSlide = _creditsPanel.GetComponent<SlideEffect>();
                if (_creditsPanelSlide == null)
                {
                    _creditsPanelSlide = _creditsPanel.AddComponent<SlideEffect>();
                    _creditsPanelSlide.SetDirection(SlideEffect.SlideDirection.Up);
                }
            }

            // サマリーパネル
            if (_summaryPanel != null)
            {
                var canvasGroup = _summaryPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _summaryPanel.AddComponent<CanvasGroup>();
                }

                _summaryPanelSlide = _summaryPanel.GetComponent<SlideEffect>();
                if (_summaryPanelSlide == null)
                {
                    _summaryPanelSlide = _summaryPanel.AddComponent<SlideEffect>();
                    _summaryPanelSlide.SetDirection(SlideEffect.SlideDirection.Right);
                }
            }
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
                Debug.LogWarning("[ResultView] Canvasが見つかりません。CRT効果をスキップします。");
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

            AddButtonEffects(_showCreditsButton, theme);
            AddButtonEffects(_showSummaryButton, theme);
            AddButtonEffects(_returnToMenuButton, theme);
            AddButtonEffects(_newGameButton, theme, ButtonAudioFeedback.ClickSoundType.Confirm);
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

        private void Update()
        {
            // クレジット自動スクロール
            if (_isScrollingCredits && _creditsScrollRect != null)
            {
                var content = _creditsScrollRect.content;
                if (content != null)
                {
                    var pos = content.anchoredPosition;
                    pos.y += _creditsScrollSpeed * Time.deltaTime;
                    content.anchoredPosition = pos;

                    // スクロール終了チェック
                    float maxScroll = content.rect.height - _creditsScrollRect.viewport.rect.height;
                    if (pos.y >= maxScroll)
                    {
                        _isScrollingCredits = false;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // CRTオーバーレイを破棄
            if (_crtOverlay != null)
            {
                Destroy(_crtOverlay);
            }

            // ローカライズサービスのイベント購読を解除
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= OnLanguageChanged;
            }

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnReturnToMenuRequested -= OnReturnToMenuRequested;
                _viewModel.OnNewGameRequested -= OnNewGameRequested;
                _viewModel.Dispose();
            }

            ClearChapterResults();
        }

        /// <summary>
        /// 言語変更時のハンドラ
        /// </summary>
        private void OnLanguageChanged(Language language)
        {
            ApplyLocalizedTexts();
            UpdateChapterResults();
        }

        /// <summary>
        /// ローカライズテキストを適用
        /// </summary>
        private void ApplyLocalizedTexts()
        {
            if (_localizationService == null) return;

            SetLocalizedText(_returnToMenuButtonText, UILocalizationKeys.Result.ReturnToMenu);
            SetLocalizedText(_newGameButtonText, UILocalizationKeys.MainMenu.NewGame);
        }

        /// <summary>
        /// ローカライズテキストを設定するヘルパー
        /// </summary>
        private void SetLocalizedText(Text? textComponent, string key)
        {
            if (textComponent == null || _localizationService == null) return;
            textComponent.text = _localizationService.GetText(key);
        }

        private void SetupUI()
        {
            if (_showCreditsButton != null)
            {
                _showCreditsButton.onClick.AddListener(OnShowCreditsClicked);
            }

            if (_showSummaryButton != null)
            {
                _showSummaryButton.onClick.AddListener(OnShowSummaryClicked);
            }

            if (_returnToMenuButton != null)
            {
                _returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            }
        }

        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (_viewModel == null) return;

            UpdateEndingInfo();
            UpdateChapterResults();
            UpdateRouteSummary();
            UpdatePanelVisibility();
        }

        /// <summary>
        /// エンディング情報を更新
        /// </summary>
        private void UpdateEndingInfo()
        {
            if (_viewModel == null) return;

            if (_endingTitleText != null)
            {
                // タイプライター効果でタイトルを表示
                if (_enableTypewriterTitle)
                {
                    _titleTypewriter = _endingTitleText.GetComponent<TypewriterEffect>();
                    if (_titleTypewriter == null)
                    {
                        _titleTypewriter = _endingTitleText.gameObject.AddComponent<TypewriterEffect>();
                    }
                    _titleTypewriter.StartTyping(_viewModel.EndingTitle);
                }
                else
                {
                    _endingTitleText.text = _viewModel.EndingTitle;
                }
            }

            if (_endingDescriptionText != null)
            {
                _endingDescriptionText.text = _viewModel.EndingDescription;
            }
        }

        /// <summary>
        /// チャプター結果を更新
        /// </summary>
        private void UpdateChapterResults()
        {
            if (_viewModel == null) return;

            ClearChapterResults();

            if (_chapterResultsContainer == null || _chapterResultPrefab == null) return;

            foreach (var result in _viewModel.ChapterResults)
            {
                var itemObj = Instantiate(_chapterResultPrefab, _chapterResultsContainer);
                itemObj.name = $"ChapterResult_{result.nightId}";

                var texts = itemObj.GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                {
                    texts[0].text = result.title;
                }
                if (texts.Length > 1)
                {
                    string completedText = _localizationService?.GetText(UILocalizationKeys.ChapterSelect.Completed) ?? "Completed";
                    string lockedText = _localizationService?.GetText(UILocalizationKeys.ChapterSelect.Locked) ?? "Locked";
                    texts[1].text = result.isCompleted
                        ? result.endingTitle ?? completedText
                        : lockedText;
                }
                if (texts.Length > 2)
                {
                    texts[2].text = result.endState?.ToString() ?? "-";
                }

                // 完了状態に応じて色を変更
                var image = itemObj.GetComponent<Image>();
                if (image != null)
                {
                    image.color = result.isCompleted
                        ? new Color(0.2f, 0.8f, 0.2f, 0.5f)
                        : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }

                _chapterResultItems.Add(itemObj);
            }
        }

        /// <summary>
        /// ルートサマリーを更新
        /// </summary>
        private void UpdateRouteSummary()
        {
            if (_viewModel == null) return;

            if (_routeSummaryText != null)
            {
                _routeSummaryText.text = _viewModel.RouteSummary;
            }
        }

        /// <summary>
        /// パネルの表示状態を更新
        /// </summary>
        private void UpdatePanelVisibility()
        {
            if (_viewModel == null) return;

            bool showCredits = _viewModel.IsShowingCredits;

            // クレジットパネルの表示/非表示（スライド効果付き）
            if (_creditsPanelSlide != null)
            {
                if (showCredits && !_creditsPanelSlide.IsVisible)
                {
                    _creditsPanel?.SetActive(true);
                    _creditsPanelSlide.SlideIn();
                }
                else if (!showCredits && _creditsPanelSlide.IsVisible)
                {
                    _creditsPanelSlide.SlideOut();
                }
            }
            else if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(showCredits);
            }

            // サマリーパネルの表示/非表示（スライド効果付き）
            if (_summaryPanelSlide != null)
            {
                if (!showCredits && !_summaryPanelSlide.IsVisible)
                {
                    _summaryPanel?.SetActive(true);
                    _summaryPanelSlide.SlideIn();
                }
                else if (showCredits && _summaryPanelSlide.IsVisible)
                {
                    _summaryPanelSlide.SlideOut();
                }
            }
            else if (_summaryPanel != null)
            {
                _summaryPanel.SetActive(!showCredits);
            }

            // クレジット表示開始時にスクロール開始
            if (showCredits)
            {
                StartCreditsScroll();
            }
            else
            {
                _isScrollingCredits = false;
            }
        }

        /// <summary>
        /// クレジットスクロールを開始
        /// </summary>
        private void StartCreditsScroll()
        {
            if (_creditsScrollRect != null && _creditsScrollRect.content != null)
            {
                // スクロール位置をリセット
                _creditsScrollRect.content.anchoredPosition = Vector2.zero;
                _isScrollingCredits = true;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ResultViewModel.IsShowingCredits):
                    UpdatePanelVisibility();
                    break;
                case nameof(ResultViewModel.ChapterResults):
                    UpdateChapterResults();
                    break;
                case nameof(ResultViewModel.RouteSummary):
                    UpdateRouteSummary();
                    break;
                case nameof(ResultViewModel.EndingTitle):
                case nameof(ResultViewModel.EndingDescription):
                    UpdateEndingInfo();
                    break;
            }
        }

        private void OnShowCreditsClicked()
        {
            _viewModel?.ShowCreditsCommand.Execute(null);
        }

        private void OnShowSummaryClicked()
        {
            _viewModel?.ShowSummaryCommand.Execute(null);
        }

        private void OnReturnToMenuClicked()
        {
            _viewModel?.ReturnToMenuCommand.Execute(null);
        }

        private void OnNewGameClicked()
        {
            _viewModel?.NewGameCommand.Execute(null);
        }

        private void OnReturnToMenuRequested()
        {
            _controller?.NavigateToChapterSelect();
        }

        private void OnNewGameRequested()
        {
            _controller?.StartNewGame();
        }

        private void ClearChapterResults()
        {
            foreach (var item in _chapterResultItems)
            {
                if (item != null) Destroy(item);
            }
            _chapterResultItems.Clear();
        }
    }
}

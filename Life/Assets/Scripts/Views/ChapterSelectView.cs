#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Controllers;
using LifeLike.Data;
using LifeLike.UI;
using LifeLike.UI.Effects;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// チャプター選択画面のView
    /// フローチャート形式で夜の進行状況を表示
    /// CRT効果、ボタンホバー効果を含む
    /// </summary>
    public class ChapterSelectView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private ChapterSelectSceneController? _controller;

        [Header("Chapter Node UI")]
        [SerializeField] private Transform? _chapterNodesContainer;
        [SerializeField] private GameObject? _chapterNodePrefab;

        [Header("Selected Chapter Info")]
        [SerializeField] private GameObject? _selectedInfoPanel;
        [SerializeField] private Text? _selectedTitleText;
        [SerializeField] private Text? _selectedDescriptionText;
        [SerializeField] private Text? _selectedStateText;
        [SerializeField] private Text? _selectedEndingText;

        [Header("Progress Info")]
        [SerializeField] private Text? _progressText;
        [SerializeField] private Slider? _progressSlider;

        [Header("Buttons")]
        [SerializeField] private Button? _startButton;
        [SerializeField] private Button? _backButton;
        [SerializeField] private Text? _startButtonText;

        [Header("Visual Settings")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _inProgressColor = Color.yellow;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _selectedColor = Color.cyan;

        [Header("UI Effects")]
        [SerializeField] private UITheme? _theme;
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private bool _enableButtonEffects = true;
        [SerializeField] private Canvas? _mainCanvas;

        private ChapterSelectViewModel? _viewModel;
        private readonly List<GameObject> _chapterNodes = new();
        private readonly Dictionary<string, Button> _chapterButtons = new();
        private GameObject? _crtOverlay;
        private SlideEffect? _selectedInfoSlide;
        private FadeEffect? _screenFade;

        private void Awake()
        {
            // コントローラーを検索
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<ChapterSelectSceneController>();
            }

            if (_controller == null)
            {
                Debug.LogError("[ChapterSelectView] ChapterSelectSceneControllerが見つかりません。");
                return;
            }
        }

        /// <summary>
        /// ViewModelを初期化（Controllerのサービス取得後に呼ばれる）
        /// </summary>
        private void InitializeViewModel()
        {
            if (_controller == null) return;

            if (_controller.OperatorSaveService == null)
            {
                Debug.LogError("[ChapterSelectView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new ChapterSelectViewModel(_controller.OperatorSaveService);
        }

        private void Start()
        {
            // ViewModelを初期化（Start時点でControllerのAwakeは完了している）
            InitializeViewModel();

            if (_viewModel == null)
            {
                Debug.LogError("[ChapterSelectView] ViewModelの初期化に失敗しました。");
                return;
            }

            // テーマを設定
            SetupTheme();

            // UI効果をセットアップ
            SetupUIEffects();

            SetupUI();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnChapterStartRequested += OnChapterStartRequested;
            _viewModel.OnBackToMenuRequested += OnBackToMenuRequested;

            BuildChapterFlowchart();
            UpdateUI();
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

            // 選択情報パネルのスライド効果をセットアップ
            SetupSelectedInfoSlide();

            // 画面フェード効果をセットアップ
            SetupScreenFade();
        }

        /// <summary>
        /// 選択情報パネルのスライド効果をセットアップ
        /// </summary>
        private void SetupSelectedInfoSlide()
        {
            if (_selectedInfoPanel == null) return;

            // CanvasGroupを追加
            var canvasGroup = _selectedInfoPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _selectedInfoPanel.AddComponent<CanvasGroup>();
            }

            _selectedInfoSlide = _selectedInfoPanel.GetComponent<SlideEffect>();
            if (_selectedInfoSlide == null)
            {
                _selectedInfoSlide = _selectedInfoPanel.AddComponent<SlideEffect>();
                _selectedInfoSlide.SetDirection(SlideEffect.SlideDirection.Right);
            }
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
                Debug.LogWarning("[ChapterSelectView] Canvasが見つかりません。CRT効果をスキップします。");
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

            AddButtonEffects(_startButton, theme, ButtonAudioFeedback.ClickSoundType.Confirm);
            AddButtonEffects(_backButton, theme);
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
                _viewModel.OnChapterStartRequested -= OnChapterStartRequested;
                _viewModel.OnBackToMenuRequested -= OnBackToMenuRequested;
                _viewModel.Dispose();
            }

            ClearChapterNodes();
        }

        private void SetupUI()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        /// <summary>
        /// フローチャートを構築
        /// </summary>
        private void BuildChapterFlowchart()
        {
            if (_viewModel == null) return;

            ClearChapterNodes();

            var summary = _viewModel.ProgressSummary;

            // チャプターノードを生成
            for (int i = 0; i < summary.chapters.Count; i++)
            {
                var chapter = summary.chapters[i];
                CreateChapterNode(chapter, i);
            }
        }

        /// <summary>
        /// チャプターノードを作成
        /// </summary>
        private void CreateChapterNode(ChapterInfo chapter, int index)
        {
            if (_chapterNodesContainer == null || _chapterNodePrefab == null) return;

            var nodeObj = Instantiate(_chapterNodePrefab, _chapterNodesContainer);
            nodeObj.name = $"ChapterNode_{chapter.chapterId}";

            // 位置を設定（2行5列のグリッド）
            var rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int row = index / 5;
                int col = index % 5;
                float x = -400 + col * 200;
                float y = 100 - row * 200;
                rectTransform.anchoredPosition = new Vector2(x, y);
            }

            // ボタンを設定
            var button = nodeObj.GetComponent<Button>();
            if (button != null)
            {
                string chapterId = chapter.chapterId;
                button.onClick.AddListener(() => OnChapterNodeClicked(chapterId));
                _chapterButtons[chapterId] = button;

                // ボタンにホバー効果とオーディオフィードバックを追加
                if (_enableButtonEffects)
                {
                    AddButtonEffects(button, UIThemeManager.Instance.Theme);

                    // チャプターノードにポップ効果を追加
                    var popEffect = button.gameObject.GetComponent<ScalePopEffect>();
                    if (popEffect == null)
                    {
                        popEffect = button.gameObject.AddComponent<ScalePopEffect>();
                        popEffect.SetIconPopPreset();
                    }
                    // ノード生成時にポップイン
                    popEffect.PopIn(0.2f + index * 0.05f);
                }
            }

            // テキストを設定
            var texts = nodeObj.GetComponentsInChildren<Text>();
            if (texts.Length > 0)
            {
                texts[0].text = $"Night {index + 1:D2}";
            }
            if (texts.Length > 1)
            {
                texts[1].text = GetStateSymbol(chapter.state);
            }

            // 色を設定
            var image = nodeObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetStateColor(chapter.state);
            }

            _chapterNodes.Add(nodeObj);
        }

        /// <summary>
        /// UIを更新
        /// </summary>
        private void UpdateUI()
        {
            if (_viewModel == null) return;

            UpdateProgressInfo();
            UpdateSelectedChapterInfo();
            UpdateChapterNodeColors();
            UpdateButtons();
        }

        /// <summary>
        /// 進行状況を更新
        /// </summary>
        private void UpdateProgressInfo()
        {
            if (_viewModel == null) return;

            var summary = _viewModel.ProgressSummary;

            if (_progressText != null)
            {
                _progressText.text = $"進行状況: {summary.completedNights} / {summary.totalNights}";
            }

            if (_progressSlider != null)
            {
                _progressSlider.maxValue = summary.totalNights;
                _progressSlider.value = summary.completedNights;
            }
        }

        /// <summary>
        /// 選択中チャプター情報を更新
        /// </summary>
        private void UpdateSelectedChapterInfo()
        {
            if (_viewModel == null) return;

            var selected = _viewModel.SelectedChapter;
            bool hasSelection = selected != null;

            // スライド効果で表示/非表示
            if (_selectedInfoSlide != null)
            {
                if (hasSelection && !_selectedInfoSlide.IsVisible)
                {
                    _selectedInfoPanel?.SetActive(true);
                    _selectedInfoSlide.SlideIn();
                }
                else if (!hasSelection && _selectedInfoSlide.IsVisible)
                {
                    _selectedInfoSlide.SlideOut();
                }
            }
            else if (_selectedInfoPanel != null)
            {
                _selectedInfoPanel.SetActive(hasSelection);
            }

            if (hasSelection && selected != null)
            {
                if (_selectedTitleText != null)
                {
                    _selectedTitleText.text = selected.title;
                }

                if (_selectedDescriptionText != null)
                {
                    _selectedDescriptionText.text = selected.description;
                }

                if (_selectedStateText != null)
                {
                    _selectedStateText.text = GetStateText(selected.state);
                    _selectedStateText.color = GetStateColor(selected.state);
                }

                if (_selectedEndingText != null)
                {
                    if (selected.state == ChapterState.Completed && selected.endingTitle != null)
                    {
                        _selectedEndingText.text = $"結果: {selected.endingTitle}";
                        _selectedEndingText.gameObject.SetActive(true);
                    }
                    else
                    {
                        _selectedEndingText.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// チャプターノードの色を更新
        /// </summary>
        private void UpdateChapterNodeColors()
        {
            if (_viewModel == null) return;

            var summary = _viewModel.ProgressSummary;
            var selected = _viewModel.SelectedChapter;

            foreach (var chapter in summary.chapters)
            {
                if (_chapterButtons.TryGetValue(chapter.chapterId, out var button))
                {
                    var image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        if (selected != null && selected.chapterId == chapter.chapterId)
                        {
                            image.color = _selectedColor;
                        }
                        else
                        {
                            image.color = GetStateColor(chapter.state);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ボタンを更新
        /// </summary>
        private void UpdateButtons()
        {
            if (_viewModel == null) return;

            if (_startButton != null)
            {
                _startButton.interactable = _viewModel.CanStartChapter;
            }

            if (_startButtonText != null && _viewModel.SelectedChapter != null)
            {
                _startButtonText.text = _viewModel.SelectedChapter.state switch
                {
                    ChapterState.InProgress => "再開する",
                    ChapterState.Completed => "再プレイ",
                    _ => "開始する"
                };
            }
        }

        /// <summary>
        /// 状態に応じた色を取得
        /// </summary>
        private Color GetStateColor(ChapterState state)
        {
            return state switch
            {
                ChapterState.Locked => _lockedColor,
                ChapterState.Available => _availableColor,
                ChapterState.InProgress => _inProgressColor,
                ChapterState.Completed => _completedColor,
                _ => _lockedColor
            };
        }

        /// <summary>
        /// 状態に応じたシンボルを取得
        /// </summary>
        private string GetStateSymbol(ChapterState state)
        {
            return state switch
            {
                ChapterState.Locked => "🔒",
                ChapterState.Available => "○",
                ChapterState.InProgress => "▶",
                ChapterState.Completed => "✓",
                _ => "?"
            };
        }

        /// <summary>
        /// 状態テキストを取得
        /// </summary>
        private string GetStateText(ChapterState state)
        {
            return state switch
            {
                ChapterState.Locked => "未解放",
                ChapterState.Available => "プレイ可能",
                ChapterState.InProgress => "進行中",
                ChapterState.Completed => "完了",
                _ => "不明"
            };
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ChapterSelectViewModel.ProgressSummary):
                    BuildChapterFlowchart();
                    UpdateUI();
                    break;
                case nameof(ChapterSelectViewModel.SelectedChapter):
                case nameof(ChapterSelectViewModel.CanStartChapter):
                    UpdateUI();
                    break;
            }
        }

        private void OnChapterNodeClicked(string chapterId)
        {
            _viewModel?.SelectChapterCommand.Execute(chapterId);
        }

        private void OnStartClicked()
        {
            _viewModel?.StartChapterCommand.Execute(null);
        }

        private void OnBackClicked()
        {
            _viewModel?.BackCommand.Execute(null);
        }

        private void OnChapterStartRequested(int nightIndex)
        {
            _controller?.StartChapter(nightIndex);
        }

        private void OnBackToMenuRequested()
        {
            _controller?.NavigateToMainMenu();
        }

        private void ClearChapterNodes()
        {
            foreach (var node in _chapterNodes)
            {
                if (node != null) Destroy(node);
            }
            _chapterNodes.Clear();
            _chapterButtons.Clear();
        }
    }
}

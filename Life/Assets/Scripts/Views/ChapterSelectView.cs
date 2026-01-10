#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.Save;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// チャプター選択画面のView
    /// フローチャート形式で夜の進行状況を表示
    /// </summary>
    public class ChapterSelectView : MonoBehaviour
    {
        [Header("Chapter Node UI")]
        [SerializeField] private Transform? _chapterNodesContainer;
        [SerializeField] private GameObject? _chapterNodePrefab;

        [Header("Route Line UI")]
        [SerializeField] private Transform? _routeLinesContainer;
        [SerializeField] private GameObject? _routeLinePrefab;

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

        [Header("Scene Settings")]
        [SerializeField] private string _operatorSceneName = "Operator";
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        [Header("Visual Settings")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _inProgressColor = Color.yellow;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _selectedColor = Color.cyan;

        private ChapterSelectViewModel? _viewModel;
        private readonly List<GameObject> _chapterNodes = new();
        private readonly List<GameObject> _routeLines = new();
        private readonly Dictionary<string, Button> _chapterButtons = new();

        private void Awake()
        {
            var operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();

            if (operatorSaveService == null)
            {
                Debug.LogError("[ChapterSelectView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new ChapterSelectViewModel(operatorSaveService);
        }

        private void Start()
        {
            if (_viewModel == null) return;

            SetupUI();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnChapterStartRequested += OnChapterStartRequested;
            _viewModel.OnBackToMenuRequested += OnBackToMenuRequested;

            BuildChapterFlowchart();
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnChapterStartRequested -= OnChapterStartRequested;
                _viewModel.OnBackToMenuRequested -= OnBackToMenuRequested;
                _viewModel.Dispose();
            }

            ClearChapterNodes();
            ClearRouteLines();
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
            ClearRouteLines();

            var summary = _viewModel.ProgressSummary;

            // チャプターノードを生成
            for (int i = 0; i < summary.chapters.Count; i++)
            {
                var chapter = summary.chapters[i];
                CreateChapterNode(chapter, i);
            }

            // ルートラインを生成
            foreach (var route in summary.routes)
            {
                CreateRouteLine(route);
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
        /// ルートラインを作成
        /// </summary>
        private void CreateRouteLine(RouteBranch route)
        {
            if (_routeLinesContainer == null || _routeLinePrefab == null) return;

            var lineObj = Instantiate(_routeLinePrefab, _routeLinesContainer);
            lineObj.name = $"RouteLine_{route.routeId}";

            // ラインの色を設定
            var image = lineObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = route.isUnlocked ? _completedColor : _lockedColor;
            }

            _routeLines.Add(lineObj);
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

            if (_selectedInfoPanel != null)
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
                _startButtonText.text = _viewModel.SelectedChapter.state == ChapterState.InProgress
                    ? "再開する"
                    : "開始する";
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
            PlayerPrefs.SetInt("LifeLike_StartNightIndex", nightIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene(_operatorSceneName);
        }

        private void OnBackToMenuRequested()
        {
            SceneManager.LoadScene(_mainMenuSceneName);
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

        private void ClearRouteLines()
        {
            foreach (var line in _routeLines)
            {
                if (line != null) Destroy(line);
            }
            _routeLines.Clear();
        }
    }
}

#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Core.Services;
using LifeLike.Services.Save;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// 結果画面のView
    /// クレジットとルートサマリーを表示
    /// </summary>
    public class ResultView : MonoBehaviour
    {
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

        [Header("Scene Settings")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private string _operatorSceneName = "Operator";

        private ResultViewModel? _viewModel;
        private readonly List<GameObject> _chapterResultItems = new();
        private bool _isScrollingCredits;

        private void Awake()
        {
            var operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();

            if (operatorSaveService == null)
            {
                Debug.LogError("[ResultView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new ResultViewModel(operatorSaveService);
        }

        private void Start()
        {
            if (_viewModel == null) return;

            SetupUI();

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnReturnToMenuRequested += OnReturnToMenuRequested;
            _viewModel.OnNewGameRequested += OnNewGameRequested;

            UpdateUI();
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
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnReturnToMenuRequested -= OnReturnToMenuRequested;
                _viewModel.OnNewGameRequested -= OnNewGameRequested;
                _viewModel.Dispose();
            }

            ClearChapterResults();
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
                _endingTitleText.text = _viewModel.EndingTitle;
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
                    texts[1].text = result.isCompleted
                        ? result.endingTitle ?? "完了"
                        : "未完了";
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

            if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(showCredits);
            }

            if (_summaryPanel != null)
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
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void OnNewGameRequested()
        {
            // 新規ゲーム開始：Night 0から開始
            PlayerPrefs.SetInt("LifeLike_StartNightIndex", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene(_operatorSceneName);
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

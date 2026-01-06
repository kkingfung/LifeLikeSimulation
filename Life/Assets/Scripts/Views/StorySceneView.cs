#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.Choice;
using LifeLike.Services.Save;
using LifeLike.Services.Story;
using LifeLike.Services.Video;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LifeLike.Views
{
    /// <summary>
    /// ストーリーシーン画面のView
    /// </summary>
    public class StorySceneView : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private VideoPlayer? _videoPlayer;
        [SerializeField] private RawImage? _videoDisplay;
        [SerializeField] private RenderTexture? _renderTexture;

        [Header("UI - Choices")]
        [SerializeField] private RectTransform? _choiceContainer;
        [SerializeField] private GameObject? _choiceButtonPrefab;

        [Header("UI - Timer")]
        [SerializeField] private GameObject? _timerContainer;
        [SerializeField] private Slider? _timerSlider;
        [SerializeField] private Text? _timerText;

        [Header("UI - Controls")]
        [SerializeField] private Button? _skipButton;
        [SerializeField] private Button? _pauseButton;
        [SerializeField] private Button? _menuButton;
        [SerializeField] private Slider? _progressSlider;

        [Header("Settings")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private StorySceneViewModel? _viewModel;
        private readonly List<GameObject> _choiceButtons = new();

        private void Awake()
        {
            // サービスを取得
            var storyService = ServiceLocator.Instance.Get<IStoryService>();
            var videoService = ServiceLocator.Instance.Get<IVideoService>();
            var choiceService = ServiceLocator.Instance.Get<IChoiceService>();
            var saveService = ServiceLocator.Instance.Get<ISaveService>();

            if (storyService == null || videoService == null || choiceService == null || saveService == null)
            {
                Debug.LogError("[StorySceneView] 必要なサービスがありません。");
                return;
            }

            // VideoServiceを初期化
            if (_videoPlayer != null)
            {
                videoService.Initialize(_videoPlayer, _renderTexture);
            }

            // ViewModelを作成
            _viewModel = new StorySceneViewModel(storyService, videoService, choiceService, saveService);
        }

        private void Start()
        {
            if (_viewModel == null)
            {
                return;
            }

            // UIを初期化
            SetupUI();

            // ViewModelのイベントを購読
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnGameEnded += OnGameEnded;
            _viewModel.OnReturnToMenuRequested += OnReturnToMenuRequested;

            // 初期状態を反映
            UpdateUI();
        }

        private void Update()
        {
            // VideoServiceとChoiceServiceの更新
            if (_viewModel != null)
            {
                var videoService = ServiceLocator.Instance.Get<IVideoService>() as VideoService;
                videoService?.Update();

                var choiceService = ServiceLocator.Instance.Get<IChoiceService>() as ChoiceService;
                choiceService?.Update(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnGameEnded -= OnGameEnded;
                _viewModel.OnReturnToMenuRequested -= OnReturnToMenuRequested;
                _viewModel.Dispose();
            }

            ClearChoiceButtons();
        }

        /// <summary>
        /// UIをセットアップする
        /// </summary>
        private void SetupUI()
        {
            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(OnSkipClicked);
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(OnMenuClicked);
            }

            // 初期状態を非表示
            if (_timerContainer != null)
            {
                _timerContainer.SetActive(false);
            }

            if (_choiceContainer != null)
            {
                _choiceContainer.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// UIを更新する
        /// </summary>
        private void UpdateUI()
        {
            if (_viewModel == null)
            {
                return;
            }

            // スキップボタンの有効/無効
            if (_skipButton != null)
            {
                _skipButton.interactable = _viewModel.IsVideoPlaying;
            }

            // 進捗スライダー
            if (_progressSlider != null && _viewModel.VideoDuration > 0)
            {
                _progressSlider.value = (float)(_viewModel.VideoProgress / _viewModel.VideoDuration);
            }
        }

        /// <summary>
        /// 選択肢UIを更新する
        /// </summary>
        private void UpdateChoicesUI()
        {
            if (_viewModel == null || _choiceContainer == null || _choiceButtonPrefab == null)
            {
                return;
            }

            ClearChoiceButtons();

            if (!_viewModel.IsShowingChoices || _viewModel.CurrentChoices.Count == 0)
            {
                _choiceContainer.gameObject.SetActive(false);
                return;
            }

            _choiceContainer.gameObject.SetActive(true);

            foreach (var choice in _viewModel.CurrentChoices)
            {
                var buttonObj = Instantiate(_choiceButtonPrefab, _choiceContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<Text>();

                if (text != null)
                {
                    var choiceService = ServiceLocator.Instance.Get<IChoiceService>();
                    var isAvailable = choiceService?.IsChoiceAvailable(choice) ?? false;

                    if (isAvailable)
                    {
                        text.text = choice.choiceText;
                    }
                    else
                    {
                        text.text = !string.IsNullOrEmpty(choice.lockedText)
                            ? choice.lockedText
                            : $"[ロック] {choice.choiceText}";
                    }

                    if (button != null)
                    {
                        button.interactable = isAvailable;

                        // クリックイベントをキャプチャ
                        var capturedChoice = choice;
                        button.onClick.AddListener(() => OnChoiceClicked(capturedChoice));
                    }
                }

                _choiceButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// タイマーUIを更新する
        /// </summary>
        private void UpdateTimerUI()
        {
            if (_viewModel == null || _timerContainer == null)
            {
                return;
            }

            _timerContainer.SetActive(_viewModel.IsTimerActive);

            if (_viewModel.IsTimerActive)
            {
                if (_timerSlider != null)
                {
                    // タイマースライダーの更新（初期時間が必要なので仮で10秒とする）
                    var timedChoice = _viewModel.CurrentChoices.Count > 0
                        ? _viewModel.CurrentChoices[0]
                        : null;
                    var maxTime = timedChoice?.timeLimit ?? 10f;
                    _timerSlider.value = _viewModel.ChoiceTimer / maxTime;
                }

                if (_timerText != null)
                {
                    _timerText.text = $"{_viewModel.ChoiceTimer:F1}";
                }
            }
        }

        /// <summary>
        /// 選択肢ボタンをクリアする
        /// </summary>
        private void ClearChoiceButtons()
        {
            foreach (var button in _choiceButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            _choiceButtons.Clear();
        }

        /// <summary>
        /// ViewModelのプロパティ変更時の処理
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StorySceneViewModel.IsVideoPlaying):
                case nameof(StorySceneViewModel.VideoProgress):
                case nameof(StorySceneViewModel.VideoDuration):
                    UpdateUI();
                    break;

                case nameof(StorySceneViewModel.IsShowingChoices):
                case nameof(StorySceneViewModel.CurrentChoices):
                    UpdateChoicesUI();
                    break;

                case nameof(StorySceneViewModel.IsTimerActive):
                case nameof(StorySceneViewModel.ChoiceTimer):
                    UpdateTimerUI();
                    break;
            }
        }

        /// <summary>
        /// ゲーム終了時の処理
        /// </summary>
        private void OnGameEnded(string endingType)
        {
            Debug.Log($"[StorySceneView] エンディング: {endingType}");
            // TODO: エンディング画面を表示
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        /// <summary>
        /// メインメニューに戻る要求時の処理
        /// </summary>
        private void OnReturnToMenuRequested()
        {
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        /// <summary>
        /// スキップボタンクリック時の処理
        /// </summary>
        private void OnSkipClicked()
        {
            _viewModel?.SkipVideoCommand.Execute(null);
        }

        /// <summary>
        /// 一時停止ボタンクリック時の処理
        /// </summary>
        private void OnPauseClicked()
        {
            _viewModel?.TogglePauseCommand.Execute(null);
        }

        /// <summary>
        /// メニューボタンクリック時の処理
        /// </summary>
        private void OnMenuClicked()
        {
            _viewModel?.ReturnToMenu();
        }

        /// <summary>
        /// 選択肢クリック時の処理
        /// </summary>
        private void OnChoiceClicked(ChoiceData choice)
        {
            _viewModel?.SelectChoiceCommand.Execute(choice);
        }
    }
}

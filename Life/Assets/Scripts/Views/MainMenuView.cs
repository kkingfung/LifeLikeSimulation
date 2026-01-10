#nullable enable
using System.ComponentModel;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.Save;
using LifeLike.Services.Story;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// メインメニュー画面のView
    /// オペレーターモードとストーリーモードの両方に対応
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("Game Mode")]
        [SerializeField] private bool _useOperatorMode = true;

        [Header("UI References")]
        [SerializeField] private Button? _startButton;
        [SerializeField] private Button? _settingsButton;
        [SerializeField] private Button? _quitButton;
        [SerializeField] private Button? _deleteSaveButton;

        [Header("Info Display")]
        [SerializeField] private Text? _lastSaveText;
        [SerializeField] private Text? _currentNightText;
        [SerializeField] private Text? _titleText;
        [SerializeField] private Text? _versionText;

        [Header("Scene Settings")]
        [SerializeField] private string _chapterSelectSceneName = "ChapterSelect";
        [SerializeField] private string _operatorSceneName = "Operator";

        [Header("Story Mode Settings (Optional)")]
        [SerializeField] private GameStateData? _gameStateData;

        private MainMenuViewModel? _viewModel;

        private void Awake()
        {
            if (_useOperatorMode)
            {
                InitializeOperatorMode();
            }
            else
            {
                InitializeStoryMode();
            }
        }

        /// <summary>
        /// オペレーターモード用の初期化
        /// </summary>
        private void InitializeOperatorMode()
        {
            var operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();

            if (operatorSaveService == null)
            {
                Debug.LogError("[MainMenuView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new MainMenuViewModel(operatorSaveService);
        }

        /// <summary>
        /// ストーリーモード用の初期化
        /// </summary>
        private void InitializeStoryMode()
        {
            var storyService = ServiceLocator.Instance.Get<IStoryService>();
            var saveService = ServiceLocator.Instance.Get<ISaveService>();

            if (storyService == null || saveService == null || _gameStateData == null)
            {
                Debug.LogError("[MainMenuView] 必要なサービスまたはデータがありません。");
                return;
            }

            _viewModel = new MainMenuViewModel(storyService, saveService, _gameStateData);
        }

        private void Start()
        {
            if (_viewModel == null)
            {
                return;
            }

            // UIをセットアップ
            SetupUI();

            // ViewModelのイベントを購読
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnGameStartRequested += OnGameStartRequested;
            _viewModel.OnOpenSettingsRequested += OnOpenSettingsRequested;
            _viewModel.OnChapterSelectRequested += OnChapterSelectRequested;

            // 初期状態を反映
            UpdateUI();

            // タイトルを設定
            if (_titleText != null)
            {
                _titleText.text = _useOperatorMode ? "Operator: Night Signal" : "LifeLike";
            }

            // バージョンを設定
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnGameStartRequested -= OnGameStartRequested;
                _viewModel.OnOpenSettingsRequested -= OnOpenSettingsRequested;
                _viewModel.OnChapterSelectRequested -= OnChapterSelectRequested;
                _viewModel.Dispose();
            }
        }

        /// <summary>
        /// UIをセットアップする
        /// </summary>
        private void SetupUI()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (_deleteSaveButton != null)
            {
                _deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
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

            // 削除ボタンの有効/無効（セーブがある時のみ表示）
            if (_deleteSaveButton != null)
            {
                _deleteSaveButton.interactable = _viewModel.CanContinue;
                _deleteSaveButton.gameObject.SetActive(_viewModel.CanContinue);
            }

            // 最後のセーブ情報
            if (_lastSaveText != null)
            {
                _lastSaveText.text = _viewModel.LastSaveInfo;
                _lastSaveText.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.LastSaveInfo));
            }

            // 現在の夜情報（オペレーターモード用）
            if (_currentNightText != null)
            {
                _currentNightText.text = _viewModel.CurrentNightInfo;
                _currentNightText.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.CurrentNightInfo));
            }
        }

        /// <summary>
        /// ViewModelのプロパティ変更時の処理
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainMenuViewModel.CanContinue):
                case nameof(MainMenuViewModel.LastSaveInfo):
                case nameof(MainMenuViewModel.CurrentNightInfo):
                    UpdateUI();
                    break;
            }
        }

        /// <summary>
        /// ストーリーモードゲーム開始要求時の処理
        /// </summary>
        private void OnGameStartRequested()
        {
            SceneManager.LoadScene(_operatorSceneName);
        }

        /// <summary>
        /// 設定画面を開く要求時の処理
        /// </summary>
        private void OnOpenSettingsRequested()
        {
            Debug.Log("[MainMenuView] 設定画面は未実装です。");
        }

        /// <summary>
        /// チャプター選択画面を開く要求時の処理
        /// </summary>
        private void OnChapterSelectRequested()
        {
            SceneManager.LoadScene(_chapterSelectSceneName);
        }

        /// <summary>
        /// スタートボタンクリック時の処理
        /// </summary>
        private void OnStartClicked()
        {
            _viewModel?.StartCommand.Execute(null);
        }

        /// <summary>
        /// 設定ボタンクリック時の処理
        /// </summary>
        private void OnSettingsClicked()
        {
            _viewModel?.OpenSettingsCommand.Execute(null);
        }

        /// <summary>
        /// 終了ボタンクリック時の処理
        /// </summary>
        private void OnQuitClicked()
        {
            _viewModel?.QuitGameCommand.Execute(null);
        }

        /// <summary>
        /// セーブデータ削除ボタンクリック時の処理
        /// </summary>
        private void OnDeleteSaveClicked()
        {
            _viewModel?.DeleteSaveCommand.Execute(null);
        }
    }
}

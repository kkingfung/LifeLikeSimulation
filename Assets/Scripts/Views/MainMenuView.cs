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
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button? _newGameButton;
        [SerializeField] private Button? _continueButton;
        [SerializeField] private Button? _settingsButton;
        [SerializeField] private Button? _quitButton;
        [SerializeField] private Text? _lastSaveText;

        [Header("Settings")]
        [SerializeField] private string _storySceneName = "StoryScene";
        [SerializeField] private GameStateData? _gameStateData;

        private MainMenuViewModel? _viewModel;

        private void Awake()
        {
            // サービスを取得
            var storyService = ServiceLocator.Instance.Get<IStoryService>();
            var saveService = ServiceLocator.Instance.Get<ISaveService>();

            if (storyService == null || saveService == null || _gameStateData == null)
            {
                Debug.LogError("[MainMenuView] 必要なサービスまたはデータがありません。");
                return;
            }

            // ViewModelを作成
            _viewModel = new MainMenuViewModel(storyService, saveService, _gameStateData);
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
            _viewModel.OnGameStartRequested += OnGameStartRequested;
            _viewModel.OnOpenSettingsRequested += OnOpenSettingsRequested;

            // 初期状態を反映
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnGameStartRequested -= OnGameStartRequested;
                _viewModel.OnOpenSettingsRequested -= OnOpenSettingsRequested;
                _viewModel.Dispose();
            }
        }

        /// <summary>
        /// UIをセットアップする
        /// </summary>
        private void SetupUI()
        {
            if (_newGameButton != null)
            {
                _newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
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

            // コンティニューボタンの有効/無効
            if (_continueButton != null)
            {
                _continueButton.interactable = _viewModel.CanContinue;
            }

            // 最後のセーブ情報
            if (_lastSaveText != null)
            {
                _lastSaveText.text = _viewModel.LastSaveInfo;
                _lastSaveText.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.LastSaveInfo));
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
                    UpdateUI();
                    break;
            }
        }

        /// <summary>
        /// ゲーム開始要求時の処理
        /// </summary>
        private void OnGameStartRequested()
        {
            // ストーリーシーンに遷移
            SceneManager.LoadScene(_storySceneName);
        }

        /// <summary>
        /// 設定画面を開く要求時の処理
        /// </summary>
        private void OnOpenSettingsRequested()
        {
            // TODO: 設定画面を実装
            Debug.Log("[MainMenuView] 設定画面は未実装です。");
        }

        /// <summary>
        /// 新規ゲームボタンクリック時の処理
        /// </summary>
        private void OnNewGameClicked()
        {
            _viewModel?.NewGameCommand.Execute(null);
        }

        /// <summary>
        /// コンティニューボタンクリック時の処理
        /// </summary>
        private void OnContinueClicked()
        {
            _viewModel?.ContinueCommand.Execute(null);
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
    }
}

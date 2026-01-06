#nullable enable
using System;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Services.Save;
using LifeLike.Services.Story;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// メインメニュー画面のViewModel
    /// </summary>
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IStoryService _storyService;
        private readonly ISaveService _saveService;
        private readonly GameStateData _gameStateData;

        private bool _canContinue;
        private string _lastSaveInfo = string.Empty;

        /// <summary>
        /// コンティニュー可能かどうか
        /// </summary>
        public bool CanContinue
        {
            get => _canContinue;
            private set => SetProperty(ref _canContinue, value);
        }

        /// <summary>
        /// 最後のセーブ情報
        /// </summary>
        public string LastSaveInfo
        {
            get => _lastSaveInfo;
            private set => SetProperty(ref _lastSaveInfo, value);
        }

        /// <summary>
        /// 新規ゲーム開始コマンド
        /// </summary>
        public RelayCommand NewGameCommand { get; }

        /// <summary>
        /// コンティニューコマンド
        /// </summary>
        public RelayCommand ContinueCommand { get; }

        /// <summary>
        /// 設定画面を開くコマンド
        /// </summary>
        public RelayCommand OpenSettingsCommand { get; }

        /// <summary>
        /// ゲームを終了するコマンド
        /// </summary>
        public RelayCommand QuitGameCommand { get; }

        /// <summary>
        /// ゲーム開始要求時のイベント
        /// </summary>
        public event Action? OnGameStartRequested;

        /// <summary>
        /// 設定画面を開く要求時のイベント
        /// </summary>
        public event Action? OnOpenSettingsRequested;

        /// <summary>
        /// MainMenuViewModelを初期化する
        /// </summary>
        /// <param name="storyService">ストーリーサービス</param>
        /// <param name="saveService">セーブサービス</param>
        /// <param name="gameStateData">ゲーム状態データ</param>
        public MainMenuViewModel(
            IStoryService storyService,
            ISaveService saveService,
            GameStateData gameStateData)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _gameStateData = gameStateData ?? throw new ArgumentNullException(nameof(gameStateData));

            // コマンドを初期化
            NewGameCommand = new RelayCommand(ExecuteNewGame);
            ContinueCommand = new RelayCommand(ExecuteContinue, () => CanContinue);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            QuitGameCommand = new RelayCommand(ExecuteQuitGame);

            // セーブ情報を更新
            RefreshSaveInfo();
        }

        /// <summary>
        /// セーブ情報を更新する
        /// </summary>
        public void RefreshSaveInfo()
        {
            CanContinue = _saveService.HasSaveData;

            if (_saveService.LastSaveTime.HasValue)
            {
                LastSaveInfo = $"最後のセーブ: {_saveService.LastSaveTime.Value:yyyy/MM/dd HH:mm}";
            }
            else
            {
                LastSaveInfo = string.Empty;
            }

            ContinueCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 新規ゲームを開始する
        /// </summary>
        private void ExecuteNewGame()
        {
            Debug.Log("[MainMenuViewModel] 新規ゲームを開始");

            // 既存のセーブデータを削除（オプション）
            // _saveService.DeleteSave();

            // 新規ゲームを開始
            _storyService.StartNewGame(_gameStateData);
            OnGameStartRequested?.Invoke();
        }

        /// <summary>
        /// セーブデータからコンティニューする
        /// </summary>
        private void ExecuteContinue()
        {
            if (!_saveService.HasSaveData)
            {
                Debug.LogWarning("[MainMenuViewModel] セーブデータがありません。");
                return;
            }

            Debug.Log("[MainMenuViewModel] コンティニュー");

            // セーブデータをロード
            if (_saveService.Load())
            {
                // 保存されていたシーンをロード
                var sceneId = _saveService.GetSavedSceneId();
                if (!string.IsNullOrEmpty(sceneId))
                {
                    // GameStateDataを設定（変数は既にLoadで復元済み）
                    // StoryServiceに反映するため、シーンをロード
                    _storyService.LoadScene(sceneId);
                    OnGameStartRequested?.Invoke();
                }
            }
        }

        /// <summary>
        /// 設定画面を開く
        /// </summary>
        private void ExecuteOpenSettings()
        {
            Debug.Log("[MainMenuViewModel] 設定画面を開く");
            OnOpenSettingsRequested?.Invoke();
        }

        /// <summary>
        /// ゲームを終了する
        /// </summary>
        private void ExecuteQuitGame()
        {
            Debug.Log("[MainMenuViewModel] ゲームを終了");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OnGameStartRequested = null;
                OnOpenSettingsRequested = null;
            }

            base.Dispose(disposing);
        }
    }
}

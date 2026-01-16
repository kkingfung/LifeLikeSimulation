#nullable enable
using System;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Data.Localization;
using LifeLike.Services.Core.Localization;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Core.Story;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// メインメニュー画面のViewModel
    /// オペレーターモードとストーリーモードの両方に対応
    /// </summary>
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IStoryService? _storyService;
        private readonly ISaveService? _saveService;
        private readonly IOperatorSaveService? _operatorSaveService;
        private readonly ILocalizationService? _localizationService;
        private readonly GameStateData? _gameStateData;

        private bool _canContinue;
        private string _lastSaveInfo = string.Empty;
        private string _currentNightInfo = string.Empty;
        private bool _isOperatorMode = true;

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
        /// 現在の夜情報（オペレーターモード用）
        /// </summary>
        public string CurrentNightInfo
        {
            get => _currentNightInfo;
            private set => SetProperty(ref _currentNightInfo, value);
        }

        /// <summary>
        /// オペレーターモードかどうか
        /// </summary>
        public bool IsOperatorMode
        {
            get => _isOperatorMode;
            set => SetProperty(ref _isOperatorMode, value);
        }

        /// <summary>
        /// ゲーム開始コマンド（セーブがあればコンティニュー、なければ新規）
        /// </summary>
        public RelayCommand StartCommand { get; }

        /// <summary>
        /// 設定画面を開くコマンド
        /// </summary>
        public RelayCommand OpenSettingsCommand { get; }

        /// <summary>
        /// ゲームを終了するコマンド
        /// </summary>
        public RelayCommand QuitGameCommand { get; }

        /// <summary>
        /// セーブデータ削除コマンド
        /// </summary>
        public RelayCommand DeleteSaveCommand { get; }

        /// <summary>
        /// ゲーム開始要求時のイベント
        /// </summary>
        public event Action? OnGameStartRequested;

        /// <summary>
        /// 設定画面を開く要求時のイベント
        /// </summary>
        public event Action? OnOpenSettingsRequested;

        /// <summary>
        /// チャプター選択画面を開く要求時のイベント
        /// </summary>
        public event Action? OnChapterSelectRequested;

        /// <summary>
        /// MainMenuViewModelを初期化する（オペレーターモード用）
        /// </summary>
        /// <param name="operatorSaveService">オペレーターセーブサービス</param>
        /// <param name="localizationService">ローカライズサービス（オプション）</param>
        public MainMenuViewModel(IOperatorSaveService operatorSaveService, ILocalizationService? localizationService = null)
        {
            _operatorSaveService = operatorSaveService ?? throw new ArgumentNullException(nameof(operatorSaveService));
            _localizationService = localizationService;
            _isOperatorMode = true;

            // コマンドを初期化
            StartCommand = new RelayCommand(ExecuteStart);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            QuitGameCommand = new RelayCommand(ExecuteQuitGame);
            DeleteSaveCommand = new RelayCommand(ExecuteDeleteSave, () => CanContinue);

            // セーブ情報を更新
            RefreshSaveInfo();
        }

        /// <summary>
        /// MainMenuViewModelを初期化する（ストーリーモード用 - 後方互換性）
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
            _isOperatorMode = false;

            // コマンドを初期化
            StartCommand = new RelayCommand(ExecuteStart);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            QuitGameCommand = new RelayCommand(ExecuteQuitGame);
            DeleteSaveCommand = new RelayCommand(ExecuteDeleteSave, () => CanContinue);

            // セーブ情報を更新
            RefreshSaveInfo();
        }

        /// <summary>
        /// セーブ情報を更新する
        /// </summary>
        public void RefreshSaveInfo()
        {
            if (_isOperatorMode && _operatorSaveService != null)
            {
                RefreshOperatorSaveInfo();
            }
            else if (_saveService != null)
            {
                RefreshStorySaveInfo();
            }

            DeleteSaveCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// オペレーターモードのセーブ情報を更新
        /// </summary>
        private void RefreshOperatorSaveInfo()
        {
            if (_operatorSaveService == null) return;

            CanContinue = _operatorSaveService.HasSaveData;

            if (_operatorSaveService.HasSaveData)
            {
                int nightIndex = _operatorSaveService.GetCurrentNightIndex();
                var completedNights = _operatorSaveService.GetCompletedNights();

                // 全10夜クリア済みの場合
                if (completedNights.Count >= 10)
                {
                    CurrentNightInfo = GetLocalizedText(UILocalizationKeys.MainMenu.AllCleared, "全クリア (Night 10 / 10)");
                }
                else
                {
                    string template = GetLocalizedText(UILocalizationKeys.MainMenu.CurrentNight, "Night {0:D2} / 10");
                    CurrentNightInfo = string.Format(template, nightIndex + 1);
                }

                if (_operatorSaveService.LastSaveTime.HasValue)
                {
                    string template = GetLocalizedText(UILocalizationKeys.MainMenu.LastSave, "最後のセーブ: {0}");
                    LastSaveInfo = string.Format(template, _operatorSaveService.LastSaveTime.Value.ToString("yyyy/MM/dd HH:mm"));
                }
                else
                {
                    LastSaveInfo = string.Empty;
                }

                // 中断セーブがある場合は表示
                if (_operatorSaveService.HasMidNightSave)
                {
                    CurrentNightInfo += GetLocalizedText(UILocalizationKeys.MainMenu.MidNightSave, " (中断セーブあり)");
                }
            }
            else
            {
                CurrentNightInfo = string.Empty;
                LastSaveInfo = string.Empty;
            }
        }

        /// <summary>
        /// ローカライズテキストを取得するヘルパー
        /// </summary>
        private string GetLocalizedText(string key, string fallback)
        {
            if (_localizationService != null)
            {
                return _localizationService.GetText(key);
            }
            return fallback;
        }

        /// <summary>
        /// ストーリーモードのセーブ情報を更新
        /// </summary>
        private void RefreshStorySaveInfo()
        {
            if (_saveService == null) return;

            CanContinue = _saveService.HasSaveData;

            if (_saveService.LastSaveTime.HasValue)
            {
                string template = GetLocalizedText(UILocalizationKeys.MainMenu.LastSave, "最後のセーブ: {0}");
                LastSaveInfo = string.Format(template, _saveService.LastSaveTime.Value.ToString("yyyy/MM/dd HH:mm"));
            }
            else
            {
                LastSaveInfo = string.Empty;
            }
        }

        /// <summary>
        /// ゲームを開始する（セーブがあればコンティニュー、なければ新規）
        /// </summary>
        private void ExecuteStart()
        {
            if (_isOperatorMode && _operatorSaveService != null)
            {
                // オペレーターモード：チャプター選択画面へ遷移
                // セーブデータの有無に関わらず、チャプター選択画面で管理
                Debug.Log($"[MainMenuViewModel] ゲーム開始 (セーブデータ: {(_operatorSaveService.HasSaveData ? "あり" : "なし")})");
                OnChapterSelectRequested?.Invoke();
            }
            else if (_storyService != null && _gameStateData != null)
            {
                // ストーリーモード
                if (_saveService != null && _saveService.HasSaveData)
                {
                    // コンティニュー
                    Debug.Log("[MainMenuViewModel] コンティニュー");
                    if (_saveService.Load())
                    {
                        var sceneId = _saveService.GetSavedSceneId();
                        if (!string.IsNullOrEmpty(sceneId))
                        {
                            _storyService.LoadScene(sceneId);
                            OnGameStartRequested?.Invoke();
                        }
                    }
                }
                else
                {
                    // 新規ゲーム
                    Debug.Log("[MainMenuViewModel] 新規ゲームを開始");
                    _storyService.StartNewGame(_gameStateData);
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
        /// セーブデータを削除する
        /// </summary>
        private void ExecuteDeleteSave()
        {
            Debug.Log("[MainMenuViewModel] セーブデータを削除");

            if (_isOperatorMode && _operatorSaveService != null)
            {
                _operatorSaveService.DeleteSave();
            }
            else if (_saveService != null)
            {
                _saveService.DeleteSave();
            }

            RefreshSaveInfo();
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
                OnChapterSelectRequested = null;
            }

            base.Dispose(disposing);
        }
    }
}

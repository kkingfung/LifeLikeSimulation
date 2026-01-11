#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Core.Story;
using LifeLike.Services.Core.Video;
using LifeLike.Services.Operator.Choice;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// ストーリーシーン画面のViewModel
    /// </summary>
    public class StorySceneViewModel : ViewModelBase
    {
        private readonly IStoryService _storyService;
        private readonly IVideoService _videoService;
        private readonly IChoiceService _choiceService;
        private readonly ISaveService _saveService;

        private StorySceneData? _currentScene;
        private bool _isVideoPlaying;
        private bool _isVideoLoading;
        private bool _isShowingChoices;
        private double _videoProgress;
        private double _videoDuration;
        private float _choiceTimer;
        private bool _isTimerActive;
        private IReadOnlyList<ChoiceData> _currentChoices = Array.Empty<ChoiceData>();

        /// <summary>
        /// 現在のシーンデータ
        /// </summary>
        public StorySceneData? CurrentScene
        {
            get => _currentScene;
            private set => SetProperty(ref _currentScene, value);
        }

        /// <summary>
        /// 動画再生中かどうか
        /// </summary>
        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            private set => SetProperty(ref _isVideoPlaying, value);
        }

        /// <summary>
        /// 動画をロード中かどうか
        /// </summary>
        public bool IsVideoLoading
        {
            get => _isVideoLoading;
            private set => SetProperty(ref _isVideoLoading, value);
        }

        /// <summary>
        /// 選択肢表示中かどうか
        /// </summary>
        public bool IsShowingChoices
        {
            get => _isShowingChoices;
            private set => SetProperty(ref _isShowingChoices, value);
        }

        /// <summary>
        /// 動画の再生進捗（秒）
        /// </summary>
        public double VideoProgress
        {
            get => _videoProgress;
            private set => SetProperty(ref _videoProgress, value);
        }

        /// <summary>
        /// 動画の総再生時間（秒）
        /// </summary>
        public double VideoDuration
        {
            get => _videoDuration;
            private set => SetProperty(ref _videoDuration, value);
        }

        /// <summary>
        /// 選択肢タイマーの残り時間
        /// </summary>
        public float ChoiceTimer
        {
            get => _choiceTimer;
            private set => SetProperty(ref _choiceTimer, value);
        }

        /// <summary>
        /// タイマーがアクティブかどうか
        /// </summary>
        public bool IsTimerActive
        {
            get => _isTimerActive;
            private set => SetProperty(ref _isTimerActive, value);
        }

        /// <summary>
        /// 現在表示中の選択肢
        /// </summary>
        public IReadOnlyList<ChoiceData> CurrentChoices
        {
            get => _currentChoices;
            private set => SetProperty(ref _currentChoices, value);
        }

        /// <summary>
        /// 動画スキップコマンド
        /// </summary>
        public RelayCommand SkipVideoCommand { get; }

        /// <summary>
        /// 一時停止/再開コマンド
        /// </summary>
        public RelayCommand TogglePauseCommand { get; }

        /// <summary>
        /// 選択肢選択コマンド
        /// </summary>
        public RelayCommand<ChoiceData> SelectChoiceCommand { get; }

        /// <summary>
        /// ゲーム終了（エンディング到達）時のイベント
        /// </summary>
        public event Action<string>? OnGameEnded;

        /// <summary>
        /// メインメニューに戻る要求時のイベント
        /// </summary>
        public event Action? OnReturnToMenuRequested;

        /// <summary>
        /// StorySceneViewModelを初期化する
        /// </summary>
        public StorySceneViewModel(
            IStoryService storyService,
            IVideoService videoService,
            IChoiceService choiceService,
            ISaveService saveService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _choiceService = choiceService ?? throw new ArgumentNullException(nameof(choiceService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));

            // コマンドを初期化
            SkipVideoCommand = new RelayCommand(ExecuteSkipVideo, () => IsVideoPlaying);
            TogglePauseCommand = new RelayCommand(ExecuteTogglePause);
            SelectChoiceCommand = new RelayCommand<ChoiceData>(ExecuteSelectChoice);

            // イベントを購読
            SubscribeToEvents();
        }

        /// <summary>
        /// イベントを購読する
        /// </summary>
        private void SubscribeToEvents()
        {
            _storyService.OnSceneChanged += OnSceneChanged;
            _storyService.OnGameEnded += OnStoryGameEnded;

            _videoService.OnVideoStarted += OnVideoStarted;
            _videoService.OnVideoCompleted += OnVideoCompleted;
            _videoService.OnTimeUpdated += OnVideoTimeUpdated;

            _choiceService.OnChoicesPresented += OnChoicesPresented;
            _choiceService.OnChoiceSelected += OnChoiceSelected;
            _choiceService.OnTimerUpdated += OnTimerUpdated;
            _choiceService.OnTimedOut += OnTimedOut;
        }

        /// <summary>
        /// イベント購読を解除する
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            _storyService.OnSceneChanged -= OnSceneChanged;
            _storyService.OnGameEnded -= OnStoryGameEnded;

            _videoService.OnVideoStarted -= OnVideoStarted;
            _videoService.OnVideoCompleted -= OnVideoCompleted;
            _videoService.OnTimeUpdated -= OnVideoTimeUpdated;

            _choiceService.OnChoicesPresented -= OnChoicesPresented;
            _choiceService.OnChoiceSelected -= OnChoiceSelected;
            _choiceService.OnTimerUpdated -= OnTimerUpdated;
            _choiceService.OnTimedOut -= OnTimedOut;
        }

        /// <summary>
        /// シーン変更時の処理
        /// </summary>
        private async void OnSceneChanged(StorySceneData scene)
        {
            CurrentScene = scene;
            IsShowingChoices = false;
            CurrentChoices = Array.Empty<ChoiceData>();

            Debug.Log($"[StorySceneViewModel] シーン変更: {scene.sceneName}");

            // 動画を再生（VideoReference経由で非同期ロード）
            if (scene.HasVideo)
            {
                IsVideoLoading = true;
                var success = await _videoService.PlayAsync(scene.video);
                IsVideoLoading = false;

                if (!success)
                {
                    Debug.LogWarning("[StorySceneViewModel] 動画の再生に失敗しました。選択肢を表示します。");
                    ShowChoices();
                }
            }
            else
            {
                // 動画がない場合は選択肢を表示
                ShowChoices();
            }

            // オートセーブ
            _saveService.AutoSave();
        }

        /// <summary>
        /// ゲーム終了時の処理
        /// </summary>
        private void OnStoryGameEnded(string endingType)
        {
            Debug.Log($"[StorySceneViewModel] エンディング到達: {endingType}");
            OnGameEnded?.Invoke(endingType);
        }

        /// <summary>
        /// 動画再生開始時の処理
        /// </summary>
        private void OnVideoStarted()
        {
            IsVideoPlaying = true;
            VideoDuration = _videoService.Duration;
            SkipVideoCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 動画再生完了時の処理
        /// </summary>
        private void OnVideoCompleted()
        {
            IsVideoPlaying = false;
            SkipVideoCommand.RaiseCanExecuteChanged();

            // 選択肢を表示
            ShowChoices();
        }

        /// <summary>
        /// 動画再生時間更新時の処理
        /// </summary>
        private void OnVideoTimeUpdated(double time)
        {
            VideoProgress = time;

            // 選択肢表示タイミングをチェック
            if (CurrentScene != null &&
                !CurrentScene.ShowChoicesAtEnd &&
                CurrentScene.choiceAppearTime > 0 &&
                time >= CurrentScene.choiceAppearTime &&
                !IsShowingChoices)
            {
                ShowChoices();
            }
        }

        /// <summary>
        /// 選択肢を表示する
        /// </summary>
        private void ShowChoices()
        {
            if (CurrentScene == null || !CurrentScene.HasChoices)
            {
                // 選択肢がない場合はデフォルトの次シーンへ
                _storyService.ProceedToNextScene();
                return;
            }

            _choiceService.PresentChoices(CurrentScene.choices);
        }

        /// <summary>
        /// 選択肢表示時の処理
        /// </summary>
        private void OnChoicesPresented(IReadOnlyList<ChoiceData> choices)
        {
            CurrentChoices = choices;
            IsShowingChoices = true;
            Debug.Log($"[StorySceneViewModel] 選択肢表示: {choices.Count}個");
        }

        /// <summary>
        /// 選択肢選択時の処理
        /// </summary>
        private void OnChoiceSelected(ChoiceData choice)
        {
            IsShowingChoices = false;
            CurrentChoices = Array.Empty<ChoiceData>();
            Debug.Log($"[StorySceneViewModel] 選択: {choice.choiceText}");
        }

        /// <summary>
        /// タイマー更新時の処理
        /// </summary>
        private void OnTimerUpdated(float remainingTime)
        {
            ChoiceTimer = remainingTime;
            IsTimerActive = true;
        }

        /// <summary>
        /// タイムアウト時の処理
        /// </summary>
        private void OnTimedOut()
        {
            IsTimerActive = false;
            ChoiceTimer = 0;

            // タイムアウト時はデフォルトの次シーンへ
            _storyService.ProceedToNextScene();
        }

        /// <summary>
        /// 動画をスキップする
        /// </summary>
        private void ExecuteSkipVideo()
        {
            _videoService.Skip();
        }

        /// <summary>
        /// 一時停止/再開を切り替える
        /// </summary>
        private void ExecuteTogglePause()
        {
            if (_videoService.IsPlaying)
            {
                _videoService.Pause();
            }
            else if (_videoService.IsPaused)
            {
                _videoService.Resume();
            }
        }

        /// <summary>
        /// 選択肢を選択する
        /// </summary>
        private void ExecuteSelectChoice(ChoiceData? choice)
        {
            if (choice == null)
            {
                return;
            }

            if (!_choiceService.IsChoiceAvailable(choice))
            {
                Debug.LogWarning($"[StorySceneViewModel] 選択肢 {choice.choiceId} は選択できません。");
                return;
            }

            _choiceService.SelectChoice(choice);
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMenu()
        {
            // セーブしてからメニューに戻る
            _saveService.AutoSave();
            OnReturnToMenuRequested?.Invoke();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeFromEvents();
                OnGameEnded = null;
                OnReturnToMenuRequested = null;
            }

            base.Dispose(disposing);
        }
    }
}

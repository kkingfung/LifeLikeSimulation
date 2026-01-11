#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Operator.CallFlow;
using LifeLike.Services.Operator.Evidence;
using LifeLike.Services.Operator.TrustGraph;
using LifeLike.Services.Operator.WorldState;
using UnityEngine;

namespace LifeLike.ViewModels
{
    /// <summary>
    /// オペレーター画面のViewModel
    /// </summary>
    public class OperatorViewModel : ViewModelBase
    {
        private readonly ICallFlowService _callFlowService;
        private readonly IWorldStateService _worldStateService;
        private readonly IEvidenceService _evidenceService;
        private readonly ITrustGraphService _trustGraphService;
        private readonly ISaveService _saveService;

        private NightScenarioData? _currentScenario;
        private string _currentTime = "00:00";
        private CallData? _currentCall;
        private CallSegment? _currentSegment;
        private CallerData? _currentCaller;
        private bool _isCallActive;
        private bool _isShowingResponses;
        private bool _isPaused;
        private float _responseTimeRemaining;
        private IReadOnlyList<ResponseData> _availableResponses = Array.Empty<ResponseData>();
        private IReadOnlyList<CallData> _incomingCalls = Array.Empty<CallData>();
        private IReadOnlyList<EvidenceData> _discoveredEvidence = Array.Empty<EvidenceData>();
        private int _missedCallCount;

        /// <summary>
        /// 現在のゲーム内時刻
        /// </summary>
        public string CurrentTime
        {
            get => _currentTime;
            private set => SetProperty(ref _currentTime, value);
        }

        /// <summary>
        /// 現在の通話データ
        /// </summary>
        public CallData? CurrentCall
        {
            get => _currentCall;
            private set => SetProperty(ref _currentCall, value);
        }

        /// <summary>
        /// 現在のセグメント
        /// </summary>
        public CallSegment? CurrentSegment
        {
            get => _currentSegment;
            private set => SetProperty(ref _currentSegment, value);
        }

        /// <summary>
        /// 現在の発信者
        /// </summary>
        public CallerData? CurrentCaller
        {
            get => _currentCaller;
            private set => SetProperty(ref _currentCaller, value);
        }

        /// <summary>
        /// 通話中かどうか
        /// </summary>
        public bool IsCallActive
        {
            get => _isCallActive;
            private set => SetProperty(ref _isCallActive, value);
        }

        /// <summary>
        /// 応答選択中かどうか
        /// </summary>
        public bool IsShowingResponses
        {
            get => _isShowingResponses;
            private set => SetProperty(ref _isShowingResponses, value);
        }

        /// <summary>
        /// 一時停止中かどうか
        /// </summary>
        public bool IsPaused
        {
            get => _isPaused;
            private set => SetProperty(ref _isPaused, value);
        }

        /// <summary>
        /// 応答の残り時間
        /// </summary>
        public float ResponseTimeRemaining
        {
            get => _responseTimeRemaining;
            private set => SetProperty(ref _responseTimeRemaining, value);
        }

        /// <summary>
        /// 利用可能な応答
        /// </summary>
        public IReadOnlyList<ResponseData> AvailableResponses
        {
            get => _availableResponses;
            private set => SetProperty(ref _availableResponses, value);
        }

        /// <summary>
        /// 着信中の通話
        /// </summary>
        public IReadOnlyList<CallData> IncomingCalls
        {
            get => _incomingCalls;
            private set => SetProperty(ref _incomingCalls, value);
        }

        /// <summary>
        /// 発見済み証拠
        /// </summary>
        public IReadOnlyList<EvidenceData> DiscoveredEvidence
        {
            get => _discoveredEvidence;
            private set => SetProperty(ref _discoveredEvidence, value);
        }

        /// <summary>
        /// 不在着信数
        /// </summary>
        public int MissedCallCount
        {
            get => _missedCallCount;
            private set => SetProperty(ref _missedCallCount, value);
        }

        // コマンド
        public RelayCommand<string> AnswerCallCommand { get; }
        public RelayCommand<string> SelectResponseCommand { get; }
        public RelayCommand SelectSilenceCommand { get; }
        public RelayCommand HoldCallCommand { get; }
        public RelayCommand EndCallCommand { get; }
        public RelayCommand TogglePauseCommand { get; }

        // イベント
        public event Action<ScenarioEnding>? OnScenarioEnded;
        public event Action? OnReturnToMenuRequested;

        public OperatorViewModel(
            ICallFlowService callFlowService,
            IWorldStateService worldStateService,
            IEvidenceService evidenceService,
            ITrustGraphService trustGraphService,
            ISaveService saveService)
        {
            _callFlowService = callFlowService ?? throw new ArgumentNullException(nameof(callFlowService));
            _worldStateService = worldStateService ?? throw new ArgumentNullException(nameof(worldStateService));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _trustGraphService = trustGraphService ?? throw new ArgumentNullException(nameof(trustGraphService));
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));

            // コマンドを初期化
            AnswerCallCommand = new RelayCommand<string>(ExecuteAnswerCall);
            SelectResponseCommand = new RelayCommand<string>(ExecuteSelectResponse);
            SelectSilenceCommand = new RelayCommand(ExecuteSelectSilence, () => IsShowingResponses);
            HoldCallCommand = new RelayCommand(ExecuteHoldCall, () => IsCallActive);
            EndCallCommand = new RelayCommand(ExecuteEndCall, () => IsCallActive);
            TogglePauseCommand = new RelayCommand(ExecuteTogglePause);

            SubscribeToEvents();
        }

        /// <summary>
        /// シナリオを開始
        /// </summary>
        public void StartScenario(NightScenarioData scenario)
        {
            _currentScenario = scenario;

            _worldStateService.LoadScenario(scenario);
            _callFlowService.LoadScenario(scenario);

            CurrentTime = _worldStateService.FormattedTime;
            Debug.Log($"[OperatorViewModel] シナリオ開始: {scenario.title}");
        }

        /// <summary>
        /// 毎フレーム更新
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_isPaused || _worldStateService.IsScenarioEnded)
            {
                return;
            }

            _worldStateService.UpdateTime(deltaTime);

            // 応答タイマーの更新
            if (IsShowingResponses && _currentSegment != null && _currentSegment.responseTimeLimit > 0)
            {
                ResponseTimeRemaining -= deltaTime;
                if (ResponseTimeRemaining <= 0)
                {
                    OnResponseTimeout();
                }
            }
        }

        /// <summary>
        /// イベントを購読
        /// </summary>
        private void SubscribeToEvents()
        {
            _worldStateService.OnTimeChanged += OnTimeChanged;
            _worldStateService.OnScenarioEnded += OnWorldScenarioEnded;
            _worldStateService.OnCallTriggered += OnCallTriggered;

            _callFlowService.OnIncomingCall += OnIncomingCall;
            _callFlowService.OnCallStarted += OnCallStarted;
            _callFlowService.OnSegmentChanged += OnSegmentChanged;
            _callFlowService.OnResponsesPresented += OnResponsesPresented;
            _callFlowService.OnResponseSelected += OnResponseSelected;
            _callFlowService.OnCallEnded += OnCallEnded;
            _callFlowService.OnCallMissed += OnCallMissed;

            _evidenceService.OnEvidenceDiscovered += OnEvidenceDiscovered;
            _evidenceService.OnContradictionFound += OnContradictionFound;
        }

        /// <summary>
        /// イベント購読を解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            _worldStateService.OnTimeChanged -= OnTimeChanged;
            _worldStateService.OnScenarioEnded -= OnWorldScenarioEnded;
            _worldStateService.OnCallTriggered -= OnCallTriggered;

            _callFlowService.OnIncomingCall -= OnIncomingCall;
            _callFlowService.OnCallStarted -= OnCallStarted;
            _callFlowService.OnSegmentChanged -= OnSegmentChanged;
            _callFlowService.OnResponsesPresented -= OnResponsesPresented;
            _callFlowService.OnResponseSelected -= OnResponseSelected;
            _callFlowService.OnCallEnded -= OnCallEnded;
            _callFlowService.OnCallMissed -= OnCallMissed;

            _evidenceService.OnEvidenceDiscovered -= OnEvidenceDiscovered;
            _evidenceService.OnContradictionFound -= OnContradictionFound;
        }

        // イベントハンドラ
        private void OnTimeChanged(int timeMinutes, string formattedTime)
        {
            CurrentTime = formattedTime;
        }

        private void OnWorldScenarioEnded(ScenarioEnding ending)
        {
            Debug.Log($"[OperatorViewModel] シナリオ終了: {ending.title}");
            OnScenarioEnded?.Invoke(ending);
        }

        private void OnCallTriggered(CallData call)
        {
            _callFlowService.AddIncomingCall(call);
        }

        private void OnIncomingCall(CallData call)
        {
            IncomingCalls = _callFlowService.IncomingCalls;
            Debug.Log($"[OperatorViewModel] 着信: {call.caller?.displayName ?? "不明"}");
        }

        private void OnCallStarted(CallData call)
        {
            CurrentCall = call;
            CurrentCaller = call.caller;
            IsCallActive = true;
            IncomingCalls = _callFlowService.IncomingCalls;
            Debug.Log($"[OperatorViewModel] 通話開始: {call.caller?.displayName ?? "不明"}");
        }

        private void OnSegmentChanged(CallSegment segment)
        {
            CurrentSegment = segment;
            Debug.Log($"[OperatorViewModel] セグメント: {segment.segmentId}");
        }

        private void OnResponsesPresented(IReadOnlyList<ResponseData> responses)
        {
            AvailableResponses = responses;
            IsShowingResponses = true;

            if (_currentSegment != null && _currentSegment.responseTimeLimit > 0)
            {
                ResponseTimeRemaining = _currentSegment.responseTimeLimit;
            }

            SelectSilenceCommand.RaiseCanExecuteChanged();
        }

        private void OnResponseSelected(ResponseData response)
        {
            IsShowingResponses = false;
            AvailableResponses = Array.Empty<ResponseData>();
            SelectSilenceCommand.RaiseCanExecuteChanged();
        }

        private void OnCallEnded(CallData call, CallState state)
        {
            CurrentCall = null;
            CurrentCaller = null;
            CurrentSegment = null;
            IsCallActive = false;
            IsShowingResponses = false;
            AvailableResponses = Array.Empty<ResponseData>();

            HoldCallCommand.RaiseCanExecuteChanged();
            EndCallCommand.RaiseCanExecuteChanged();
            SelectSilenceCommand.RaiseCanExecuteChanged();

            // オートセーブ
            _saveService.AutoSave();
        }

        private void OnCallMissed(CallData call)
        {
            MissedCallCount = _callFlowService.GetMissedCallCount();
            IncomingCalls = _callFlowService.IncomingCalls;
            Debug.Log($"[OperatorViewModel] 不在着信: {call.caller?.displayName ?? "不明"}");
        }

        private void OnEvidenceDiscovered(EvidenceData evidence)
        {
            DiscoveredEvidence = _evidenceService.DiscoveredEvidence;
            Debug.Log($"[OperatorViewModel] 証拠発見: {evidence.content}");
        }

        private void OnContradictionFound(EvidenceData evidence1, EvidenceData evidence2)
        {
            Debug.Log($"[OperatorViewModel] 矛盾発見: {evidence1.evidenceId} ⇔ {evidence2.evidenceId}");
        }

        private void OnResponseTimeout()
        {
            if (_currentSegment != null && !string.IsNullOrEmpty(_currentSegment.timeoutResponseId))
            {
                _callFlowService.SelectResponse(_currentSegment.timeoutResponseId);
            }
            else
            {
                _callFlowService.SelectSilence();
            }
        }

        // コマンド実行
        private void ExecuteAnswerCall(string? callId)
        {
            if (string.IsNullOrEmpty(callId)) return;
            _callFlowService.AnswerCall(callId);
        }

        private void ExecuteSelectResponse(string? responseId)
        {
            if (string.IsNullOrEmpty(responseId)) return;
            _callFlowService.SelectResponse(responseId);
        }

        private void ExecuteSelectSilence()
        {
            _callFlowService.SelectSilence();
        }

        private void ExecuteHoldCall()
        {
            _callFlowService.HoldCall();
        }

        private void ExecuteEndCall()
        {
            _callFlowService.EndCall();
        }

        private void ExecuteTogglePause()
        {
            if (_isPaused)
            {
                _worldStateService.Resume();
                IsPaused = false;
            }
            else
            {
                _worldStateService.Pause();
                IsPaused = true;
            }
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMenu()
        {
            _saveService.AutoSave();
            OnReturnToMenuRequested?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnsubscribeFromEvents();
                OnScenarioEnded = null;
                OnReturnToMenuRequested = null;
            }
            base.Dispose(disposing);
        }
    }
}

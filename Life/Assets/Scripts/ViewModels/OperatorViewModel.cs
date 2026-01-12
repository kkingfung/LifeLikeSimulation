#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Core.Commands;
using LifeLike.Core.MVVM;
using LifeLike.Data;
using LifeLike.Data.Flag;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Operator.CallFlow;
using LifeLike.Services.Operator.Flag;
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
        private readonly IOperatorSaveService _operatorSaveService;
        private readonly IFlagService _flagService;

        // 名前が判明した発信者のID
        private readonly HashSet<string> _revealedCallerIds = new();

        private NightScenarioData? _currentScenario;
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
        /// 現在の発信者の表示名（名前が判明していない場合は「不明」）
        /// </summary>
        public string CurrentCallerDisplayName
        {
            get
            {
                if (_currentCaller == null) return "不明";
                if (_revealedCallerIds.Contains(_currentCaller.callerId))
                {
                    return _currentCaller.displayName;
                }
                return "不明";
            }
        }

        /// <summary>
        /// 発信者の名前が判明しているか
        /// </summary>
        public bool IsCurrentCallerRevealed => _currentCaller != null && _revealedCallerIds.Contains(_currentCaller.callerId);

        /// <summary>
        /// 指定した発信者の名前が判明しているか
        /// </summary>
        public bool IsCallerRevealed(string callerId) => _revealedCallerIds.Contains(callerId);

        /// <summary>
        /// 指定した発信者の表示名を取得（名前が判明していない場合は「不明」）
        /// </summary>
        public string GetCallerDisplayName(CallerData? caller)
        {
            if (caller == null) return "不明";
            if (_revealedCallerIds.Contains(caller.callerId))
            {
                return caller.displayName;
            }
            return "不明";
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
            IOperatorSaveService operatorSaveService,
            IFlagService flagService)
        {
            _callFlowService = callFlowService ?? throw new ArgumentNullException(nameof(callFlowService));
            _worldStateService = worldStateService ?? throw new ArgumentNullException(nameof(worldStateService));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _trustGraphService = trustGraphService ?? throw new ArgumentNullException(nameof(trustGraphService));
            _operatorSaveService = operatorSaveService ?? throw new ArgumentNullException(nameof(operatorSaveService));
            _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));

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
        /// シナリオを開始（コントローラーから呼ばれる場合）
        /// 注意: 通常はコントローラーがStartNight()で初期化するため、
        /// OperatorViewからは TriggerFirstCall() を使用する
        /// </summary>
        public void StartScenario(NightScenarioData scenario)
        {
            _currentScenario = scenario;

            _worldStateService.LoadScenario(scenario);
            _callFlowService.LoadScenario(scenario);

            Debug.Log($"[OperatorViewModel] シナリオ開始: {scenario.title}");

            // シミュレーションモード: 最初の通話をすぐに着信させる
            TriggerNextAvailableCall();
        }

        /// <summary>
        /// 最初の通話をトリガーする（コントローラーがシナリオを開始した後に呼ぶ）
        /// </summary>
        public void TriggerFirstCall()
        {
            Debug.Log("[OperatorViewModel] TriggerFirstCall");

            // シミュレーションモード: 最初の通話をすぐに着信させる
            TriggerNextAvailableCall();
        }

        /// <summary>
        /// シナリオが読み込まれているかチェックして、最初の通話をトリガーする
        /// </summary>
        public void EnsureFirstCallTriggered()
        {
            // 既に着信がある場合は何もしない
            if (_callFlowService.IncomingCalls.Count > 0)
            {
                return;
            }

            // 最初の通話をトリガー
            _callFlowService.TriggerNextCall();
        }

        /// <summary>
        /// サービスから現在の状態を取得して反映する
        /// Start()の実行順序により、イベント購読前に状態が変わっている可能性があるため
        /// </summary>
        public void RefreshFromServices()
        {
            // 着信リストを更新
            IncomingCalls = _callFlowService.IncomingCalls;

            // 証拠リストを更新
            DiscoveredEvidence = _evidenceService.DiscoveredEvidence;

            // 不在着信数を更新
            MissedCallCount = _callFlowService.GetMissedCallCount();

            Debug.Log($"[OperatorViewModel] RefreshFromServices - 着信数: {IncomingCalls.Count}");
        }

        /// <summary>
        /// 次の利用可能な通話をトリガーする
        /// </summary>
        private void TriggerNextAvailableCall()
        {
            // 現在の着信リストが空で、通話中でもない場合
            if (_callFlowService.IncomingCalls.Count == 0 && !IsCallActive)
            {
                // 次の通話をCallFlowServiceにリクエスト
                _callFlowService.TriggerNextCall();
            }
        }

        /// <summary>
        /// 発信者の名前を明らかにする
        /// </summary>
        public void RevealCaller(string callerId)
        {
            if (_revealedCallerIds.Add(callerId))
            {
                Debug.Log($"[OperatorViewModel] 発信者の名前が判明: {callerId}");
                OnPropertyChanged(nameof(CurrentCallerDisplayName));
                OnPropertyChanged(nameof(IsCurrentCallerRevealed));
            }
        }

        /// <summary>
        /// 現在の発信者の名前を明らかにする
        /// </summary>
        public void RevealCurrentCaller()
        {
            if (_currentCaller != null)
            {
                RevealCaller(_currentCaller.callerId);
            }
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

            // 注意: 自動時間進行は無効化
            // シミュレーションゲームでは、プレイヤーのアクションで時間が進む
            // _worldStateService.UpdateTime(deltaTime);

            // 応答タイマーの更新（時限応答がある場合のみ）
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

            // 通話中の発信者の名前を明らかにする（発信者が自己紹介した後）
            if (_currentCaller != null && !_revealedCallerIds.Contains(_currentCaller.callerId))
            {
                RevealCurrentCaller();
            }
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

            // 中断セーブ
            SaveMidNightProgress();

            // シミュレーションモード: 通話終了後、次の通話をトリガー
            TriggerNextAvailableCall();

            // 全ての通話が終了したかチェック
            CheckScenarioCompletion();
        }

        /// <summary>
        /// シナリオ完了をチェック
        /// </summary>
        private void CheckScenarioCompletion()
        {
            // 全ての通話が完了したかチェック
            if (_callFlowService.AreAllCallsCompleted() &&
                _callFlowService.IncomingCalls.Count == 0 &&
                !IsCallActive)
            {
                Debug.Log("[OperatorViewModel] 全ての通話が完了しました。シナリオ終了。");

                // エンディングを決定
                var ending = _worldStateService.CheckEndingConditions();
                if (ending != null)
                {
                    _worldStateService.EndScenario(ending);
                }
                else
                {
                    // デフォルトエンディング
                    _worldStateService.FinalizeScenario();
                }
            }
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
            SaveMidNightProgress();
            OnReturnToMenuRequested?.Invoke();
        }

        /// <summary>
        /// 中断セーブを実行
        /// </summary>
        private void SaveMidNightProgress()
        {
            if (_currentScenario == null) return;

            var currentFlags = _flagService.GetAllFlags();
            _operatorSaveService.SaveMidNight(_currentScenario.scenarioId, 0, currentFlags);
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

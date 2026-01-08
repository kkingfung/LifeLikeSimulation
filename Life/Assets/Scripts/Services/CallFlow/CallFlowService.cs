#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using LifeLike.Data.Conditions;
using LifeLike.Services.Clock;
using LifeLike.Services.Evidence;
using LifeLike.Services.Flag;
using LifeLike.Services.Story;
using LifeLike.Services.TrustGraph;
using UnityEngine;

namespace LifeLike.Services.CallFlow
{
    /// <summary>
    /// 通話フロー管理サービスの実装
    /// </summary>
    public class CallFlowService : ICallFlowService
    {
        private readonly IStoryService _storyService;
        private readonly IEvidenceService _evidenceService;
        private readonly ITrustGraphService _trustGraphService;
        private readonly IFlagService? _flagService;
        private readonly IClockService? _clockService;

        private NightScenarioData? _currentScenario;
        private CallData? _currentCall;
        private CallSegment? _currentSegment;
        private readonly List<CallData> _incomingCalls = new();
        private readonly List<CallData> _onHoldCalls = new();
        private readonly List<CallData> _callHistory = new();
        private readonly List<CallData> _missedCalls = new();

        public CallData? CurrentCall => _currentCall;
        public CallSegment? CurrentSegment => _currentSegment;
        public IReadOnlyList<CallData> IncomingCalls => _incomingCalls.AsReadOnly();
        public IReadOnlyList<CallData> OnHoldCalls => _onHoldCalls.AsReadOnly();
        public IReadOnlyList<CallData> CallHistory => _callHistory.AsReadOnly();

        public event Action<CallData>? OnIncomingCall;
        public event Action<CallData>? OnCallStarted;
        public event Action<CallSegment>? OnSegmentChanged;
        public event Action<IReadOnlyList<ResponseData>>? OnResponsesPresented;
        public event Action<ResponseData>? OnResponseSelected;
        public event Action<CallData, CallState>? OnCallEnded;
        public event Action<CallData>? OnCallMissed;

        /// <summary>
        /// コンストラクタ（後方互換性のため）
        /// </summary>
        public CallFlowService(
            IStoryService storyService,
            IEvidenceService evidenceService,
            ITrustGraphService trustGraphService)
            : this(storyService, evidenceService, trustGraphService, null, null)
        {
        }

        /// <summary>
        /// コンストラクタ（フラグ・時計サービス統合版）
        /// </summary>
        public CallFlowService(
            IStoryService storyService,
            IEvidenceService evidenceService,
            ITrustGraphService trustGraphService,
            IFlagService? flagService,
            IClockService? clockService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _trustGraphService = trustGraphService ?? throw new ArgumentNullException(nameof(trustGraphService));
            _flagService = flagService;
            _clockService = clockService;
        }

        public void LoadScenario(NightScenarioData scenario)
        {
            _currentScenario = scenario;
            Clear();

            // 証拠テンプレートを読み込み
            _evidenceService.LoadEvidenceTemplates(scenario.evidenceTemplates);

            Debug.Log($"[CallFlowService] シナリオを読み込みました: {scenario.title}");
        }

        public void AddIncomingCall(CallData call)
        {
            // 条件チェック
            if (!CheckConditions(call.triggerConditions))
            {
                Debug.Log($"[CallFlowService] 通話 {call.callId} は条件を満たしていません。");
                return;
            }

            _incomingCalls.Add(call);
            Debug.Log($"[CallFlowService] 着信: {call.caller?.displayName ?? "不明"} ({call.GetFormattedTime()})");
            OnIncomingCall?.Invoke(call);
        }

        public bool AnswerCall(string callId)
        {
            var call = _incomingCalls.Find(c => c.callId == callId);
            if (call == null)
            {
                Debug.LogWarning($"[CallFlowService] 着信 {callId} が見つかりません。");
                return false;
            }

            // 現在の通話があれば保留
            if (_currentCall != null)
            {
                HoldCall();
            }

            _incomingCalls.Remove(call);
            _currentCall = call;

            // 開始セグメントへ
            var startSegment = call.GetStartSegment();
            if (startSegment != null)
            {
                TransitionToSegment(startSegment);
            }

            Debug.Log($"[CallFlowService] 通話開始: {call.caller?.displayName ?? "不明"}");
            OnCallStarted?.Invoke(call);

            return true;
        }

        public bool HoldCall()
        {
            if (_currentCall == null)
            {
                return false;
            }

            _onHoldCalls.Add(_currentCall);
            Debug.Log($"[CallFlowService] 通話を保留: {_currentCall.caller?.displayName ?? "不明"}");

            _currentCall = null;
            _currentSegment = null;

            return true;
        }

        public bool ResumeCall(string callId)
        {
            var call = _onHoldCalls.Find(c => c.callId == callId);
            if (call == null)
            {
                return false;
            }

            // 現在の通話があれば保留
            if (_currentCall != null)
            {
                HoldCall();
            }

            _onHoldCalls.Remove(call);
            _currentCall = call;

            Debug.Log($"[CallFlowService] 通話再開: {call.caller?.displayName ?? "不明"}");
            OnCallStarted?.Invoke(call);

            return true;
        }

        public void EndCall()
        {
            if (_currentCall == null)
            {
                return;
            }

            var endedCall = _currentCall;
            _callHistory.Add(endedCall);

            // 終了時の効果を適用
            ApplyEffects(endedCall.onEndEffects);

            Debug.Log($"[CallFlowService] 通話終了: {endedCall.caller?.displayName ?? "不明"}");
            OnCallEnded?.Invoke(endedCall, CallState.Ended);

            _currentCall = null;
            _currentSegment = null;
        }

        public void SelectResponse(string responseId)
        {
            if (_currentSegment == null || _currentCall == null)
            {
                return;
            }

            var response = _currentSegment.responses.Find(r => r.responseId == responseId);
            if (response == null)
            {
                Debug.LogWarning($"[CallFlowService] 応答 {responseId} が見つかりません。");
                return;
            }

            if (!IsResponseAvailable(response))
            {
                Debug.LogWarning($"[CallFlowService] 応答 {responseId} は選択できません。");
                return;
            }

            ProcessResponse(response);
        }

        public void SelectSilence()
        {
            if (_currentSegment == null)
            {
                return;
            }

            // 沈黙の応答を探す
            var silenceResponse = _currentSegment.responses.Find(r => r.isSilence);
            if (silenceResponse != null)
            {
                ProcessResponse(silenceResponse);
            }
            else
            {
                // 沈黙が定義されていない場合、タイムアウトと同じ扱い
                var timeoutResponse = _currentSegment.responses.Find(r =>
                    r.responseId == _currentSegment.timeoutResponseId);
                if (timeoutResponse != null)
                {
                    ProcessResponse(timeoutResponse);
                }
            }
        }

        public IReadOnlyList<ResponseData> GetAvailableResponses()
        {
            if (_currentSegment == null)
            {
                return Array.Empty<ResponseData>();
            }

            return _currentSegment.responses
                .Where(IsResponseAvailable)
                .ToList()
                .AsReadOnly();
        }

        public bool IsResponseAvailable(ResponseData response)
        {
            // 条件チェック
            if (!CheckConditions(response.conditions))
            {
                return false;
            }

            // 必要な証拠チェック
            foreach (var evidenceId in response.requiredEvidenceIds)
            {
                if (!_evidenceService.HasEvidence(evidenceId))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetMissedCallCount()
        {
            return _missedCalls.Count;
        }

        public void Clear()
        {
            _currentCall = null;
            _currentSegment = null;
            _incomingCalls.Clear();
            _onHoldCalls.Clear();
            _callHistory.Clear();
            _missedCalls.Clear();
            Debug.Log("[CallFlowService] クリアしました。");
        }

        /// <summary>
        /// 着信をタイムアウトさせる（時間管理から呼ばれる）
        /// </summary>
        public void TimeoutIncomingCall(string callId)
        {
            var call = _incomingCalls.Find(c => c.callId == callId);
            if (call == null)
            {
                return;
            }

            _incomingCalls.Remove(call);
            _missedCalls.Add(call);

            // 不在着信時の効果を適用
            ApplyEffects(call.onMissedEffects);

            Debug.Log($"[CallFlowService] 不在着信: {call.caller?.displayName ?? "不明"}");
            OnCallMissed?.Invoke(call);
        }

        /// <summary>
        /// 応答を処理
        /// </summary>
        private void ProcessResponse(ResponseData response)
        {
            Debug.Log($"[CallFlowService] 応答選択: {response.displayText}");
            OnResponseSelected?.Invoke(response);

            // 効果を適用
            ApplyEffects(response.effects);

            // フラグを設定
            ProcessResponseFlags(response);

            // 派遣アクションの処理
            if (response.isDispatchAction)
            {
                ProcessDispatchAction();
            }

            // 信頼度への影響
            if (response.trustImpact != 0 && _currentCall?.caller != null)
            {
                _trustGraphService.ModifyOperatorTrust(
                    _currentCall.caller.callerId,
                    response.trustImpact,
                    response.displayText.ToString());
            }

            // 証拠を発見
            if (response.discoversEvidence && !string.IsNullOrEmpty(response.discoveredEvidenceId))
            {
                _evidenceService.DiscoverEvidence(response.discoveredEvidenceId);
            }

            // 証拠を提示
            if (response.presentsEvidence && !string.IsNullOrEmpty(response.evidenceIdToPresent))
            {
                _evidenceService.UseEvidence(response.evidenceIdToPresent);
            }

            // 通話終了
            if (response.endsCall)
            {
                EndCall();
                return;
            }

            // 次のセグメントへ
            if (!string.IsNullOrEmpty(response.nextSegmentId) && _currentCall != null)
            {
                var nextSegment = _currentCall.GetSegment(response.nextSegmentId);
                if (nextSegment != null)
                {
                    TransitionToSegment(nextSegment);
                }
            }
        }

        /// <summary>
        /// 応答に関連するフラグを処理
        /// </summary>
        private void ProcessResponseFlags(ResponseData response)
        {
            if (_flagService == null)
            {
                return;
            }

            int currentTime = _clockService?.CurrentTimeMinutes ?? 0;

            // フラグを設定
            if (response.setFlags != null && response.setFlags.Count > 0)
            {
                foreach (var flagId in response.setFlags)
                {
                    if (!string.IsNullOrEmpty(flagId))
                    {
                        _flagService.SetFlag(flagId, currentTime);
                        Debug.Log($"[CallFlowService] フラグ設定: {flagId}");
                    }
                }
            }

            // フラグをクリア
            if (response.clearFlags != null && response.clearFlags.Count > 0)
            {
                foreach (var flagId in response.clearFlags)
                {
                    if (!string.IsNullOrEmpty(flagId))
                    {
                        _flagService.ClearFlag(flagId);
                        Debug.Log($"[CallFlowService] フラグクリア: {flagId}");
                    }
                }
            }
        }

        /// <summary>
        /// 派遣アクションを処理
        /// </summary>
        private void ProcessDispatchAction()
        {
            if (_clockService == null || _flagService == null)
            {
                Debug.LogWarning("[CallFlowService] 派遣アクション: ClockServiceまたはFlagServiceが未設定");
                return;
            }

            // 既に派遣済みの場合はスキップ
            if (_clockService.HasDispatched)
            {
                Debug.Log("[CallFlowService] 派遣アクション: 既に派遣済み");
                return;
            }

            // 派遣を記録
            _clockService.RecordDispatch();
            int dispatchTime = _clockService.DispatchTimeMinutes ?? _clockService.CurrentTimeMinutes;

            // emergency_dispatchedフラグを設定
            _flagService.SetFlag("emergency_dispatched", dispatchTime);

            // 派遣時刻に応じたフラグを設定
            if (dispatchTime <= 161)  // 02:41以前
            {
                _flagService.SetFlag("dispatch_time_0241", dispatchTime);
            }
            else if (dispatchTime <= 169)  // 02:49以前
            {
                _flagService.SetFlag("dispatch_time_0249", dispatchTime);
            }

            Debug.Log($"[CallFlowService] 派遣アクション: {_clockService.FormatTime(dispatchTime)}に派遣を記録");
        }

        /// <summary>
        /// セグメントに遷移
        /// </summary>
        private void TransitionToSegment(CallSegment segment)
        {
            _currentSegment = segment;

            // 自動的に得られる証拠を発見
            foreach (var evidenceId in segment.autoDiscoveredEvidenceIds)
            {
                _evidenceService.DiscoverEvidence(evidenceId);
            }

            Debug.Log($"[CallFlowService] セグメント: {segment.segmentId}");
            OnSegmentChanged?.Invoke(segment);

            // 応答選択肢を表示
            var availableResponses = GetAvailableResponses();
            if (availableResponses.Count > 0)
            {
                OnResponsesPresented?.Invoke(availableResponses);
            }
        }

        /// <summary>
        /// 条件をチェック
        /// </summary>
        private bool CheckConditions(List<Data.Conditions.StoryCondition> conditions)
        {
            foreach (var condition in conditions)
            {
                if (!_storyService.EvaluateCondition(condition))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 効果を適用
        /// </summary>
        private void ApplyEffects(List<StoryEffect> effects)
        {
            foreach (var effect in effects)
            {
                _storyService.ApplyEffect(effect);
            }
        }
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Services.Core.Story;
using LifeLike.Services.Operator.Clock;
using LifeLike.Services.Operator.EndState;
using UnityEngine;

namespace LifeLike.Services.Operator.WorldState
{
    /// <summary>
    /// 世界状態管理サービスの実装
    /// </summary>
    public class WorldStateService : IWorldStateService
    {
        private readonly IStoryService _storyService;
        private readonly IEndStateService? _endStateService;
        private readonly IClockService? _clockService;

        private NightScenarioData? _currentScenario;
        private int _currentTimeMinutes;
        private float _accumulatedRealTime;
        private bool _isPaused;
        private bool _isScenarioEnded;

        private readonly List<WorldStateSnapshot> _worldStates = new();
        private readonly HashSet<string> _triggeredCallIds = new();

        public int CurrentTimeMinutes => _clockService?.CurrentTimeMinutes ?? _currentTimeMinutes;
        public string FormattedTime => _clockService?.FormattedTime ?? FormatTime(_currentTimeMinutes);
        public bool IsScenarioEnded => _isScenarioEnded;

        public event Action<int, string>? OnTimeChanged;
        public event Action<WorldStateSnapshot>? OnWorldStateChanged;
        public event Action<ScenarioEnding>? OnScenarioEnded;
        public event Action<CallData>? OnCallTriggered;

        /// <summary>
        /// コンストラクタ（後方互換性のため）
        /// </summary>
        public WorldStateService(IStoryService storyService)
            : this(storyService, null, null)
        {
        }

        /// <summary>
        /// コンストラクタ（EndStateService/ClockService統合版）
        /// </summary>
        public WorldStateService(
            IStoryService storyService,
            IEndStateService? endStateService,
            IClockService? clockService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            _endStateService = endStateService;
            _clockService = clockService;

            // ClockServiceの時刻変更イベントを購読
            if (_clockService != null)
            {
                _clockService.OnTimeChanged += OnClockTimeChanged;
                _clockService.OnTimeUp += OnClockTimeUp;
            }
        }

        /// <summary>
        /// ClockServiceの時刻変更イベントハンドラ
        /// </summary>
        private void OnClockTimeChanged(int timeMinutes, string formattedTime)
        {
            int oldTime = _currentTimeMinutes;
            _currentTimeMinutes = timeMinutes;

            OnTimeChanged?.Invoke(timeMinutes, formattedTime);

            // この間にトリガーされるべき通話をチェック
            CheckCallTriggers(oldTime, timeMinutes);
        }

        /// <summary>
        /// ClockServiceのシナリオ終了イベントハンドラ
        /// </summary>
        private void OnClockTimeUp()
        {
            if (!_isScenarioEnded)
            {
                FinalizeScenario();
            }
        }

        public void LoadScenario(NightScenarioData scenario)
        {
            Clear();
            _currentScenario = scenario;
            _currentTimeMinutes = scenario.startTimeMinutes;

            // 初期世界状態を読み込み
            foreach (var state in scenario.initialWorldStates)
            {
                _worldStates.Add(state);
            }

            Debug.Log($"[WorldStateService] シナリオ読み込み: {scenario.title}");
            Debug.Log($"[WorldStateService] 開始時刻: {FormattedTime}");
        }

        public void UpdateTime(float deltaTime)
        {
            if (_isPaused || _isScenarioEnded || _currentScenario == null)
            {
                return;
            }

            _accumulatedRealTime += deltaTime;

            // 実時間からゲーム内時間を計算
            float gameMinutesPerRealSecond = 1f / _currentScenario.realSecondsPerGameMinute;
            int gameMinutesToAdd = (int)(_accumulatedRealTime * gameMinutesPerRealSecond);

            if (gameMinutesToAdd > 0)
            {
                _accumulatedRealTime -= gameMinutesToAdd / gameMinutesPerRealSecond;
                AdvanceTime(gameMinutesToAdd);
            }
        }

        public void AddWorldState(WorldStateSnapshot state)
        {
            state.timeMinutes = _currentTimeMinutes;
            _worldStates.Add(state);

            Debug.Log($"[WorldStateService] 世界状態追加: {state.description}");
            OnWorldStateChanged?.Invoke(state);
        }

        public void RevealStateToPlayer(string stateId)
        {
            var state = _worldStates.Find(s => s.stateId == stateId);
            if (state != null && !state.isKnownToPlayer)
            {
                state.isKnownToPlayer = true;
                Debug.Log($"[WorldStateService] プレイヤーに明かした: {state.description}");
                OnWorldStateChanged?.Invoke(state);
            }
        }

        public IReadOnlyList<WorldStateSnapshot> GetKnownWorldStates()
        {
            return _worldStates
                .Where(s => s.isKnownToPlayer)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<WorldStateSnapshot> GetAllWorldStates()
        {
            return _worldStates.AsReadOnly();
        }

        public ScenarioEnding? CheckEndingConditions()
        {
            if (_currentScenario == null)
            {
                return null;
            }

            // EndStateServiceが利用可能な場合は、そちらを使用
            if (_endStateService != null)
            {
                int? dispatchTime = _clockService?.DispatchTimeMinutes;
                string endingId = _endStateService.DetermineEnding(dispatchTime);

                if (!string.IsNullOrEmpty(endingId))
                {
                    return _endStateService.GetEnding(endingId);
                }
            }

            // 従来のロジック（後方互換性）
            foreach (var ending in _currentScenario.endings)
            {
                bool allConditionsMet = true;
                foreach (var condition in ending.conditions)
                {
                    if (!_storyService.EvaluateCondition(condition))
                    {
                        allConditionsMet = false;
                        break;
                    }
                }

                if (allConditionsMet)
                {
                    return ending;
                }
            }

            return null;
        }

        public void EndScenario(ScenarioEnding ending)
        {
            if (_isScenarioEnded)
            {
                return;
            }

            _isScenarioEnded = true;
            Debug.Log($"[WorldStateService] シナリオ終了: {ending.title}");
            OnScenarioEnded?.Invoke(ending);
        }

        /// <summary>
        /// シナリオを終了させる（EndStateServiceを使用）
        /// </summary>
        public void FinalizeScenario()
        {
            if (_isScenarioEnded)
            {
                return;
            }

            ScenarioEnding? ending = null;

            // EndStateServiceを使用してエンディングを決定
            if (_endStateService != null)
            {
                int? dispatchTime = _clockService?.DispatchTimeMinutes;
                string endingId = _endStateService.DetermineEnding(dispatchTime);

                if (!string.IsNullOrEmpty(endingId))
                {
                    ending = _endStateService.GetEnding(endingId);

                    var endState = _endStateService.CurrentEndState;
                    var victimSurvived = _endStateService.VictimSurvived;

                    Debug.Log($"[WorldStateService] エンドステート: {endState}");
                    Debug.Log($"[WorldStateService] 被害者生存: {victimSurvived}");
                    Debug.Log($"[WorldStateService] エンディング: {endingId}");
                }
            }

            // フォールバック: 従来のロジック
            if (ending == null)
            {
                ending = CheckEndingConditions();
            }

            // さらにフォールバック: デフォルトエンディング
            if (ending == null && _currentScenario != null)
            {
                ending = _currentScenario.endings.FirstOrDefault();
            }

            if (ending != null)
            {
                EndScenario(ending);
            }
        }

        public void Pause()
        {
            _isPaused = true;
            Debug.Log("[WorldStateService] 一時停止");
        }

        public void Resume()
        {
            _isPaused = false;
            Debug.Log("[WorldStateService] 再開");
        }

        public void Clear()
        {
            _currentScenario = null;
            _currentTimeMinutes = 0;
            _accumulatedRealTime = 0;
            _isPaused = false;
            _isScenarioEnded = false;
            _worldStates.Clear();
            _triggeredCallIds.Clear();
            Debug.Log("[WorldStateService] クリアしました。");
        }

        /// <summary>
        /// 時間を進める
        /// </summary>
        private void AdvanceTime(int minutes)
        {
            int oldTime = _currentTimeMinutes;
            _currentTimeMinutes += minutes;

            // 時刻変更を通知
            OnTimeChanged?.Invoke(_currentTimeMinutes, FormattedTime);

            // この間にトリガーされるべき通話をチェック
            CheckCallTriggers(oldTime, _currentTimeMinutes);

            // シナリオ終了時刻をチェック
            CheckScenarioEnd();

            // エンディング条件をチェック
            var ending = CheckEndingConditions();
            if (ending != null)
            {
                EndScenario(ending);
            }
        }

        /// <summary>
        /// 通話のトリガーをチェック
        /// </summary>
        private void CheckCallTriggers(int fromTime, int toTime)
        {
            if (_currentScenario == null)
            {
                return;
            }

            foreach (var call in _currentScenario.calls)
            {
                // 既にトリガー済み
                if (_triggeredCallIds.Contains(call.callId))
                {
                    continue;
                }

                // 時刻範囲内かチェック
                if (call.incomingTimeMinutes > fromTime && call.incomingTimeMinutes <= toTime)
                {
                    _triggeredCallIds.Add(call.callId);
                    Debug.Log($"[WorldStateService] 通話トリガー: {call.caller?.displayName ?? "不明"} at {call.GetFormattedTime()}");
                    OnCallTriggered?.Invoke(call);
                }
            }
        }

        /// <summary>
        /// シナリオ終了時刻をチェック
        /// </summary>
        private void CheckScenarioEnd()
        {
            if (_currentScenario == null || _isScenarioEnded)
            {
                return;
            }

            // ClockServiceを使用している場合はそちらに任せる
            if (_clockService != null)
            {
                return;
            }

            // 終了時刻を過ぎたかチェック（日をまたぐ場合の処理）
            int endTime = _currentScenario.endTimeMinutes;
            int startTime = _currentScenario.startTimeMinutes;

            // 日をまたぐ場合（例：22:00開始、06:00終了）
            if (endTime < startTime)
            {
                // 翌日の時刻として扱う
                endTime += 1440; // +24時間
            }

            if (_currentTimeMinutes >= endTime)
            {
                FinalizeScenario();
            }
        }

        /// <summary>
        /// 時刻をフォーマット
        /// </summary>
        private string FormatTime(int timeMinutes)
        {
            int adjustedMinutes = timeMinutes % 1440;
            int hours = adjustedMinutes / 60;
            int minutes = adjustedMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }
    }
}

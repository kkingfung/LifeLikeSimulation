#nullable enable
using System;
using UnityEngine;

namespace LifeLike.Services.Clock
{
    /// <summary>
    /// ゲーム内時計サービスの実装
    /// ゲーム内時刻の管理と派遣タイミングの記録を担当
    /// </summary>
    public class ClockService : IClockService
    {
        #region フィールド

        private int _currentTimeMinutes;
        private int _startTimeMinutes;
        private int _endTimeMinutes;
        private float _realSecondsPerGameMinute = 2f;
        private float _accumulatedRealSeconds;
        private bool _isRunning;
        private bool _isPaused;
        private int? _dispatchTimeMinutes;

        #endregion

        #region プロパティ

        public int CurrentTimeMinutes => _currentTimeMinutes;
        public string FormattedTime => FormatTime(_currentTimeMinutes);
        public int StartTimeMinutes => _startTimeMinutes;
        public int EndTimeMinutes => _endTimeMinutes;
        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;
        public int? DispatchTimeMinutes => _dispatchTimeMinutes;
        public bool HasDispatched => _dispatchTimeMinutes.HasValue;
        public bool IsTimeUp => _currentTimeMinutes >= _endTimeMinutes;
        public int RemainingMinutes => Math.Max(0, _endTimeMinutes - _currentTimeMinutes);

        #endregion

        #region イベント

        public event Action<int, string>? OnTimeChanged;
        public event Action<int>? OnDispatchRecorded;
        public event Action? OnTimeUp;
        public event Action? OnClockStarted;
        public event Action? OnClockStopped;

        #endregion

        #region 初期化

        public void Initialize(int startTimeMinutes, int endTimeMinutes, float realSecondsPerGameMinute = 2f)
        {
            _startTimeMinutes = startTimeMinutes;
            _endTimeMinutes = endTimeMinutes;
            _realSecondsPerGameMinute = realSecondsPerGameMinute;
            _currentTimeMinutes = startTimeMinutes;
            _accumulatedRealSeconds = 0f;
            _isRunning = false;
            _isPaused = false;
            _dispatchTimeMinutes = null;

            Debug.Log($"[ClockService] 初期化: {FormatTime(startTimeMinutes)} - {FormatTime(endTimeMinutes)}, {realSecondsPerGameMinute}秒/分");
        }

        #endregion

        #region 時間操作

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _isPaused = false;
            _accumulatedRealSeconds = 0f;

            Debug.Log($"[ClockService] 時計開始: {FormattedTime}");
            OnClockStarted?.Invoke();
        }

        public void Pause()
        {
            if (!_isRunning || _isPaused)
            {
                return;
            }

            _isPaused = true;
            Debug.Log($"[ClockService] 一時停止: {FormattedTime}");
        }

        public void Resume()
        {
            if (!_isRunning || !_isPaused)
            {
                return;
            }

            _isPaused = false;
            Debug.Log($"[ClockService] 再開: {FormattedTime}");
        }

        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _isPaused = false;

            Debug.Log($"[ClockService] 時計停止: {FormattedTime}");
            OnClockStopped?.Invoke();
        }

        public void SetTime(int timeMinutes)
        {
            int previousTime = _currentTimeMinutes;
            _currentTimeMinutes = timeMinutes;
            _accumulatedRealSeconds = 0f;

            if (previousTime != _currentTimeMinutes)
            {
                Debug.Log($"[ClockService] 時刻設定: {FormatTime(previousTime)} → {FormattedTime}");
                OnTimeChanged?.Invoke(_currentTimeMinutes, FormattedTime);

                if (IsTimeUp)
                {
                    OnTimeUp?.Invoke();
                }
            }
        }

        public void AdvanceTime(int minutes)
        {
            if (minutes <= 0)
            {
                return;
            }

            int previousTime = _currentTimeMinutes;
            _currentTimeMinutes += minutes;

            Debug.Log($"[ClockService] 時刻を{minutes}分進める: {FormatTime(previousTime)} → {FormattedTime}");
            OnTimeChanged?.Invoke(_currentTimeMinutes, FormattedTime);

            if (IsTimeUp)
            {
                OnTimeUp?.Invoke();
            }
        }

        public void Update(float deltaTime)
        {
            if (!_isRunning || _isPaused || IsTimeUp)
            {
                return;
            }

            _accumulatedRealSeconds += deltaTime;

            // 蓄積された実時間がゲーム内1分に達したか
            while (_accumulatedRealSeconds >= _realSecondsPerGameMinute)
            {
                _accumulatedRealSeconds -= _realSecondsPerGameMinute;
                _currentTimeMinutes++;

                OnTimeChanged?.Invoke(_currentTimeMinutes, FormattedTime);

                if (IsTimeUp)
                {
                    Debug.Log($"[ClockService] 時間切れ: {FormattedTime}");
                    OnTimeUp?.Invoke();
                    Stop();
                    break;
                }
            }
        }

        #endregion

        #region 派遣記録

        public void RecordDispatch()
        {
            RecordDispatchAt(_currentTimeMinutes);
        }

        public void RecordDispatchAt(int timeMinutes)
        {
            // 最初の派遣のみ記録（複数回呼ばれても最初の時刻を保持）
            if (_dispatchTimeMinutes.HasValue)
            {
                Debug.Log($"[ClockService] 派遣は既に記録済み: {FormatTime(_dispatchTimeMinutes.Value)}");
                return;
            }

            _dispatchTimeMinutes = timeMinutes;
            Debug.Log($"[ClockService] 派遣記録: {FormatTime(timeMinutes)}");
            OnDispatchRecorded?.Invoke(timeMinutes);
        }

        public void ClearDispatchRecord()
        {
            _dispatchTimeMinutes = null;
            Debug.Log("[ClockService] 派遣記録をクリア");
        }

        #endregion

        #region ユーティリティ

        public string FormatTime(int timeMinutes)
        {
            // 24時間を超える場合（翌日）の処理
            int adjustedMinutes = timeMinutes;
            if (adjustedMinutes >= 1440)
            {
                adjustedMinutes %= 1440;
            }

            int hours = adjustedMinutes / 60;
            int minutes = adjustedMinutes % 60;
            return $"{hours:D2}:{minutes:D2}";
        }

        public int ParseTime(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
            {
                return 0;
            }

            var parts = timeString.Split(':');
            if (parts.Length != 2)
            {
                Debug.LogWarning($"[ClockService] 無効な時刻形式: {timeString}");
                return 0;
            }

            if (int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
            {
                return hours * 60 + minutes;
            }

            Debug.LogWarning($"[ClockService] 時刻のパースに失敗: {timeString}");
            return 0;
        }

        #endregion
    }
}

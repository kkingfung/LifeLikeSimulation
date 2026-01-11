#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// 時間制限付き応答用のタイマーバーコンポーネント
    /// 残り時間に応じて色が変化し、警告音を再生
    /// </summary>
    public class TimerBar : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Slider? _slider;
        [SerializeField] private Image? _fillImage;
        [SerializeField] private Text? _timerText;

        [Header("色設定")]
        [SerializeField] private Color _colorFull = new Color(0.2f, 0.8f, 0.4f, 1f);      // 緑
        [SerializeField] private Color _colorMid = new Color(0.9f, 0.75f, 0.2f, 1f);      // 黄色
        [SerializeField] private Color _colorLow = new Color(0.9f, 0.4f, 0.2f, 1f);       // オレンジ
        [SerializeField] private Color _colorCritical = new Color(0.9f, 0.2f, 0.2f, 1f);  // 赤

        [Header("警告設定")]
        [SerializeField, Range(0f, 1f)] private float _warningThreshold = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _criticalThreshold = 0.2f;
        [SerializeField] private bool _playTickSound = true;
        [SerializeField] private float _tickInterval = 1f;

        [Header("パルス効果")]
        [SerializeField] private bool _pulseOnCritical = true;
        [SerializeField] private float _pulseSpeed = 4f;

        private float _maxTime;
        private float _currentTime;
        private float _lastTickTime;
        private bool _isRunning;
        private PulseEffect? _pulseEffect;

        public event System.Action? OnTimerExpired;
        public event System.Action? OnWarningThreshold;
        public event System.Action? OnCriticalThreshold;

        /// <summary>
        /// 残り時間の割合（0-1）
        /// </summary>
        public float NormalizedTime => _maxTime > 0 ? _currentTime / _maxTime : 0f;

        /// <summary>
        /// タイマーが動作中かどうか
        /// </summary>
        public bool IsRunning => _isRunning;

        private void Awake()
        {
            if (_slider == null)
            {
                _slider = GetComponent<Slider>();
            }

            if (_fillImage == null && _slider != null)
            {
                _fillImage = _slider.fillRect?.GetComponent<Image>();
            }

            if (_pulseOnCritical)
            {
                _pulseEffect = GetComponent<PulseEffect>();
                if (_pulseEffect == null && _fillImage != null)
                {
                    _pulseEffect = _fillImage.gameObject.AddComponent<PulseEffect>();
                    _pulseEffect.SetTimerWarningPreset();
                    _pulseEffect.SetSpeed(_pulseSpeed);
                }
            }
        }

        private void Update()
        {
            if (!_isRunning) return;

            _currentTime -= Time.deltaTime;

            if (_currentTime <= 0)
            {
                _currentTime = 0;
                _isRunning = false;
                OnTimerExpired?.Invoke();
            }

            UpdateDisplay();

            // ティック音
            if (_playTickSound && _currentTime <= _maxTime * _criticalThreshold)
            {
                if (Time.time - _lastTickTime >= _tickInterval)
                {
                    _lastTickTime = Time.time;
                    UIAudioFeedback.Instance?.PlayTimerTick();
                }
            }
        }

        /// <summary>
        /// タイマーを開始
        /// </summary>
        public void StartTimer(float duration)
        {
            _maxTime = duration;
            _currentTime = duration;
            _isRunning = true;
            _lastTickTime = 0;

            if (_slider != null)
            {
                _slider.maxValue = duration;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// タイマーを停止
        /// </summary>
        public void StopTimer()
        {
            _isRunning = false;
            if (_pulseEffect != null)
            {
                _pulseEffect.StopPulse();
            }
        }

        /// <summary>
        /// タイマーを一時停止
        /// </summary>
        public void PauseTimer()
        {
            _isRunning = false;
        }

        /// <summary>
        /// タイマーを再開
        /// </summary>
        public void ResumeTimer()
        {
            if (_currentTime > 0)
            {
                _isRunning = true;
            }
        }

        /// <summary>
        /// タイマーをリセット
        /// </summary>
        public void ResetTimer()
        {
            _currentTime = _maxTime;
            _isRunning = false;
            UpdateDisplay();
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            float normalized = NormalizedTime;

            // スライダー値を更新
            if (_slider != null)
            {
                _slider.value = _currentTime;
            }

            // 色を更新
            if (_fillImage != null)
            {
                _fillImage.color = GetTimerColor(normalized);
            }

            // テキストを更新
            if (_timerText != null)
            {
                _timerText.text = $"{_currentTime:F1}s";
            }

            // クリティカル時のパルス効果
            if (_pulseEffect != null)
            {
                if (normalized <= _criticalThreshold && _isRunning)
                {
                    if (!_pulseEffect.enabled)
                    {
                        _pulseEffect.enabled = true;
                        _pulseEffect.StartPulse();
                    }
                }
                else
                {
                    _pulseEffect.StopPulse();
                    _pulseEffect.enabled = false;
                }
            }

            // 閾値イベント
            CheckThresholds(normalized);
        }

        /// <summary>
        /// 閾値チェック
        /// </summary>
        private bool _warningTriggered;
        private bool _criticalTriggered;

        private void CheckThresholds(float normalized)
        {
            if (normalized <= _warningThreshold && !_warningTriggered)
            {
                _warningTriggered = true;
                OnWarningThreshold?.Invoke();
            }

            if (normalized <= _criticalThreshold && !_criticalTriggered)
            {
                _criticalTriggered = true;
                OnCriticalThreshold?.Invoke();
                UIAudioFeedback.Instance?.PlayTimerUrgent();
            }
        }

        /// <summary>
        /// 残り時間に応じた色を取得
        /// </summary>
        private Color GetTimerColor(float normalized)
        {
            if (normalized > _warningThreshold)
            {
                // 緑→黄色
                float t = (normalized - _warningThreshold) / (1f - _warningThreshold);
                return Color.Lerp(_colorMid, _colorFull, t);
            }
            else if (normalized > _criticalThreshold)
            {
                // 黄色→オレンジ
                float t = (normalized - _criticalThreshold) / (_warningThreshold - _criticalThreshold);
                return Color.Lerp(_colorLow, _colorMid, t);
            }
            else
            {
                // オレンジ→赤
                float t = normalized / _criticalThreshold;
                return Color.Lerp(_colorCritical, _colorLow, t);
            }
        }

        /// <summary>
        /// 表示/非表示を設定
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (!visible)
            {
                StopTimer();
            }
        }
    }
}

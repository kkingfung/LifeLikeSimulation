#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// 数値をアニメーションでカウントアップ/ダウンするコンポーネント
    /// スコア表示やタイマー、ステータス表示に使用
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class NumberCounterEffect : MonoBehaviour
    {
        [Header("カウンター設定")]
        [SerializeField] private float _duration = 1f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _countOnEnable = false;

        [Header("表示フォーマット")]
        [SerializeField] private string _format = "{0:N0}";
        [SerializeField] private string _prefix = "";
        [SerializeField] private string _suffix = "";

        [Header("イージング")]
        [SerializeField] private AnimationCurve _countCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("追加効果")]
        [SerializeField] private bool _punchScaleOnChange = false;
        [SerializeField] private float _punchScale = 1.1f;
        [SerializeField] private float _punchDuration = 0.1f;

        [Header("サウンド")]
        [SerializeField] private bool _playTickSound = false;
        [SerializeField] private AudioSource? _audioSource;
        [SerializeField] private AudioClip? _tickSound;
        [SerializeField] private float _tickInterval = 0.05f;

        private Text? _text;
        private float _currentValue;
        private float _targetValue;
        private float _startValue;
        private Coroutine? _countCoroutine;
        private Coroutine? _punchCoroutine;
        private Vector3 _originalScale;
        private bool _isCounting;

        public event Action<float>? OnCountComplete;
        public event Action<float>? OnValueChanged;

        /// <summary>
        /// 現在の値
        /// </summary>
        public float CurrentValue => _currentValue;

        /// <summary>
        /// カウント中かどうか
        /// </summary>
        public bool IsCounting => _isCounting;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_countOnEnable)
            {
                CountTo(_targetValue);
            }
        }

        private void OnDisable()
        {
            StopCount();
        }

        /// <summary>
        /// 即座に値を設定
        /// </summary>
        public void SetValue(float value)
        {
            StopCount();
            _currentValue = value;
            _targetValue = value;
            UpdateDisplay();
        }

        /// <summary>
        /// 指定の値までカウント
        /// </summary>
        public void CountTo(float targetValue)
        {
            CountTo(targetValue, _duration);
        }

        /// <summary>
        /// 指定の値までカウント（時間指定）
        /// </summary>
        public void CountTo(float targetValue, float duration)
        {
            StopCount();
            _startValue = _currentValue;
            _targetValue = targetValue;
            _countCoroutine = StartCoroutine(CountCoroutine(duration));
        }

        /// <summary>
        /// 0から指定値までカウント
        /// </summary>
        public void CountFromZero(float targetValue)
        {
            _currentValue = 0f;
            CountTo(targetValue);
        }

        /// <summary>
        /// 指定量を加算してカウント
        /// </summary>
        public void AddValue(float delta)
        {
            CountTo(_targetValue + delta);
        }

        /// <summary>
        /// カウントを停止
        /// </summary>
        public void StopCount()
        {
            if (_countCoroutine != null)
            {
                StopCoroutine(_countCoroutine);
                _countCoroutine = null;
            }
            _isCounting = false;
        }

        /// <summary>
        /// カウントをスキップして最終値を表示
        /// </summary>
        public void SkipToEnd()
        {
            StopCount();
            _currentValue = _targetValue;
            UpdateDisplay();
            OnCountComplete?.Invoke(_currentValue);
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            if (_text == null) return;

            string formattedValue = string.Format(_format, _currentValue);
            _text.text = $"{_prefix}{formattedValue}{_suffix}";
        }

        /// <summary>
        /// カウントコルーチン
        /// </summary>
        private IEnumerator CountCoroutine(float duration)
        {
            _isCounting = true;
            float elapsed = 0f;
            float lastTickTime = 0f;
            float previousValue = _currentValue;

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = _countCurve.Evaluate(t);

                _currentValue = Mathf.Lerp(_startValue, _targetValue, curvedT);
                UpdateDisplay();

                // 値が変わったらイベント発火
                if (Mathf.Abs(_currentValue - previousValue) > 0.01f)
                {
                    OnValueChanged?.Invoke(_currentValue);

                    // パンチスケール
                    if (_punchScaleOnChange && Mathf.Abs(_currentValue - previousValue) > 1f)
                    {
                        TriggerPunchScale();
                    }

                    previousValue = _currentValue;
                }

                // ティック音
                if (_playTickSound && _audioSource != null && _tickSound != null)
                {
                    if (elapsed - lastTickTime >= _tickInterval)
                    {
                        _audioSource.PlayOneShot(_tickSound, 0.3f);
                        lastTickTime = elapsed;
                    }
                }

                yield return null;
            }

            _currentValue = _targetValue;
            UpdateDisplay();
            _isCounting = false;
            OnCountComplete?.Invoke(_currentValue);
        }

        /// <summary>
        /// パンチスケールをトリガー
        /// </summary>
        private void TriggerPunchScale()
        {
            if (_punchCoroutine != null)
            {
                StopCoroutine(_punchCoroutine);
            }
            _punchCoroutine = StartCoroutine(PunchScaleCoroutine());
        }

        /// <summary>
        /// パンチスケールコルーチン
        /// </summary>
        private IEnumerator PunchScaleCoroutine()
        {
            float elapsed = 0f;
            float halfDuration = _punchDuration * 0.5f;

            // 拡大
            while (elapsed < halfDuration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(1f, _punchScale, t);
                transform.localScale = _originalScale * scale;

                yield return null;
            }

            // 縮小
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = elapsed / halfDuration;
                float scale = Mathf.Lerp(_punchScale, 1f, t);
                transform.localScale = _originalScale * scale;

                yield return null;
            }

            transform.localScale = _originalScale;
        }

        /// <summary>
        /// スコア表示用のプリセット
        /// </summary>
        public void SetScorePreset()
        {
            _format = "{0:N0}";
            _prefix = "";
            _suffix = " pts";
            _duration = 1.5f;
            _punchScaleOnChange = true;
        }

        /// <summary>
        /// パーセント表示用のプリセット
        /// </summary>
        public void SetPercentPreset()
        {
            _format = "{0:F0}";
            _prefix = "";
            _suffix = "%";
            _duration = 0.5f;
        }

        /// <summary>
        /// 通貨表示用のプリセット
        /// </summary>
        public void SetCurrencyPreset()
        {
            _format = "{0:N0}";
            _prefix = "$";
            _suffix = "";
            _duration = 1f;
            _punchScaleOnChange = true;
        }

        /// <summary>
        /// タイマー表示用のプリセット（分:秒）
        /// </summary>
        public void SetTimerPreset()
        {
            _format = "{0:F0}";
            _duration = 0.3f;
        }

        /// <summary>
        /// 時間フォーマットで表示（分:秒）
        /// </summary>
        public void SetValueAsTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            if (_text != null)
            {
                _text.text = $"{_prefix}{mins:D2}:{secs:D2}{_suffix}";
            }
            _currentValue = seconds;
            _targetValue = seconds;
        }
    }
}

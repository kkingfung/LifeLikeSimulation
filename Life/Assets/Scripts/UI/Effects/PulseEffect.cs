#nullable enable
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にパルス（脈動）効果を適用するコンポーネント
    /// 着信表示や警告表示に使用
    /// </summary>
    public class PulseEffect : MonoBehaviour
    {
        public enum PulseType
        {
            Scale,
            Color,
            Alpha,
            All
        }

        [Header("パルス設定")]
        [SerializeField] private PulseType _pulseType = PulseType.All;
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _loop = true;

        [Header("スケールパルス")]
        [SerializeField] private float _scaleMin = 1.0f;
        [SerializeField] private float _scaleMax = 1.1f;
        [SerializeField] private float _scalePulseSpeed = 2f;

        [Header("色パルス")]
        [SerializeField] private Color _colorMin = Color.white;
        [SerializeField] private Color _colorMax = new Color(0f, 0.9f, 1f, 1f);
        [SerializeField] private float _colorPulseSpeed = 2f;

        [Header("アルファパルス")]
        [SerializeField] private float _alphaMin = 0.5f;
        [SerializeField] private float _alphaMax = 1.0f;
        [SerializeField] private float _alphaPulseSpeed = 2f;

        [Header("イージング")]
        [SerializeField] private AnimationCurve _pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 _originalScale;
        private Graphic? _graphic;
        private bool _isPulsing;
        private float _pulseTime;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _graphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                StartPulse();
            }
        }

        private void OnDisable()
        {
            StopPulse();
        }

        private void Update()
        {
            if (!_isPulsing) return;

            _pulseTime += Time.unscaledDeltaTime;

            // スケールパルス
            if (_pulseType == PulseType.Scale || _pulseType == PulseType.All)
            {
                float scaleT = Mathf.PingPong(_pulseTime * _scalePulseSpeed, 1f);
                float scale = Mathf.Lerp(_scaleMin, _scaleMax, _pulseCurve.Evaluate(scaleT));
                transform.localScale = _originalScale * scale;
            }

            // 色パルス
            if (_graphic != null && (_pulseType == PulseType.Color || _pulseType == PulseType.All))
            {
                float colorT = Mathf.PingPong(_pulseTime * _colorPulseSpeed, 1f);
                Color targetColor = Color.Lerp(_colorMin, _colorMax, _pulseCurve.Evaluate(colorT));

                // アルファを維持
                if (_pulseType != PulseType.All)
                {
                    targetColor.a = _graphic.color.a;
                }

                _graphic.color = targetColor;
            }

            // アルファパルス
            if (_graphic != null && _pulseType == PulseType.Alpha)
            {
                float alphaT = Mathf.PingPong(_pulseTime * _alphaPulseSpeed, 1f);
                float alpha = Mathf.Lerp(_alphaMin, _alphaMax, _pulseCurve.Evaluate(alphaT));
                var color = _graphic.color;
                color.a = alpha;
                _graphic.color = color;
            }

            // ループしない場合、1サイクルで停止
            if (!_loop)
            {
                float maxSpeed = Mathf.Max(_scalePulseSpeed, _colorPulseSpeed, _alphaPulseSpeed);
                if (_pulseTime >= 1f / maxSpeed)
                {
                    StopPulse();
                }
            }
        }

        /// <summary>
        /// パルスを開始
        /// </summary>
        public void StartPulse()
        {
            _isPulsing = true;
            _pulseTime = 0f;
        }

        /// <summary>
        /// パルスを停止してオリジナル状態に戻る
        /// </summary>
        public void StopPulse()
        {
            _isPulsing = false;
            transform.localScale = _originalScale;
        }

        /// <summary>
        /// 着信用のプリセット設定
        /// </summary>
        public void SetIncomingCallPreset()
        {
            _pulseType = PulseType.All;
            _scaleMin = 1.0f;
            _scaleMax = 1.05f;
            _scalePulseSpeed = 3f;
            _colorMin = new Color(0f, 0.5f, 0.55f, 1f);
            _colorMax = new Color(0f, 0.8f, 0.9f, 1f);
            _colorPulseSpeed = 3f;
            _loop = true;
        }

        /// <summary>
        /// 警告用のプリセット設定
        /// </summary>
        public void SetWarningPreset()
        {
            _pulseType = PulseType.Color;
            _colorMin = new Color(0.8f, 0.2f, 0.2f, 1f);
            _colorMax = new Color(1f, 0.4f, 0.4f, 1f);
            _colorPulseSpeed = 4f;
            _loop = true;
        }

        /// <summary>
        /// タイマー警告用のプリセット設定
        /// </summary>
        public void SetTimerWarningPreset()
        {
            _pulseType = PulseType.Alpha;
            _alphaMin = 0.6f;
            _alphaMax = 1.0f;
            _alphaPulseSpeed = 5f;
            _loop = true;
        }

        /// <summary>
        /// パルス速度を設定
        /// </summary>
        public void SetSpeed(float speed)
        {
            _scalePulseSpeed = speed;
            _colorPulseSpeed = speed;
            _alphaPulseSpeed = speed;
        }
    }
}

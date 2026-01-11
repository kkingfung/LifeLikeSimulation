#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にウォブル（ゼリー揺れ）効果を適用するコンポーネント
    /// ボタン押下時やアイテム選択時のフィードバックに使用
    /// </summary>
    public class WobbleEffect : MonoBehaviour
    {
        public enum WobbleAxis
        {
            Both,
            Horizontal,
            Vertical
        }

        [Header("ウォブル設定")]
        [SerializeField] private WobbleAxis _axis = WobbleAxis.Both;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _intensity = 0.15f;
        [SerializeField] private float _frequency = 8f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _playOnEnable = false;

        [Header("減衰設定")]
        [SerializeField] private AnimationCurve _decayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("位相設定")]
        [SerializeField] private float _xPhaseOffset = 0f;
        [SerializeField] private float _yPhaseOffset = Mathf.PI * 0.5f;

        private RectTransform? _rectTransform;
        private Vector3 _originalScale;
        private Coroutine? _wobbleCoroutine;
        private bool _isWobbling;

        public event Action? OnWobbleComplete;

        /// <summary>
        /// ウォブル中かどうか
        /// </summary>
        public bool IsWobbling => _isWobbling;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Wobble();
            }
        }

        private void OnDisable()
        {
            StopWobble();
        }

        /// <summary>
        /// ウォブルを開始
        /// </summary>
        public void Wobble()
        {
            Wobble(_intensity);
        }

        /// <summary>
        /// ウォブルを開始（強度指定）
        /// </summary>
        public void Wobble(float intensity)
        {
            if (_rectTransform == null) return;

            StopWobble();
            _wobbleCoroutine = StartCoroutine(WobbleCoroutine(intensity));
        }

        /// <summary>
        /// ウォブルを停止して元に戻る
        /// </summary>
        public void StopWobble()
        {
            if (_wobbleCoroutine != null)
            {
                StopCoroutine(_wobbleCoroutine);
                _wobbleCoroutine = null;
            }

            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale;
            }

            _isWobbling = false;
        }

        /// <summary>
        /// ウォブルコルーチン
        /// </summary>
        private IEnumerator WobbleCoroutine(float intensity)
        {
            if (_rectTransform == null) yield break;

            _isWobbling = true;
            float elapsed = 0f;

            while (elapsed < _duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = elapsed / _duration;
                float decay = _decayCurve.Evaluate(t);
                float angle = elapsed * _frequency * Mathf.PI * 2f;

                float scaleX = 1f;
                float scaleY = 1f;

                switch (_axis)
                {
                    case WobbleAxis.Both:
                        scaleX = 1f + Mathf.Sin(angle + _xPhaseOffset) * intensity * decay;
                        scaleY = 1f + Mathf.Sin(angle + _yPhaseOffset) * intensity * decay;
                        break;

                    case WobbleAxis.Horizontal:
                        scaleX = 1f + Mathf.Sin(angle + _xPhaseOffset) * intensity * decay;
                        break;

                    case WobbleAxis.Vertical:
                        scaleY = 1f + Mathf.Sin(angle + _yPhaseOffset) * intensity * decay;
                        break;
                }

                _rectTransform.localScale = new Vector3(
                    _originalScale.x * scaleX,
                    _originalScale.y * scaleY,
                    _originalScale.z
                );

                yield return null;
            }

            _rectTransform.localScale = _originalScale;
            _isWobbling = false;
            OnWobbleComplete?.Invoke();
        }

        /// <summary>
        /// ボタンクリック用のプリセット
        /// </summary>
        public void SetButtonClickPreset()
        {
            _axis = WobbleAxis.Both;
            _duration = 0.3f;
            _intensity = 0.08f;
            _frequency = 10f;
            _xPhaseOffset = 0f;
            _yPhaseOffset = Mathf.PI * 0.5f;
        }

        /// <summary>
        /// ゼリー用のプリセット（大きく揺れる）
        /// </summary>
        public void SetJellyPreset()
        {
            _axis = WobbleAxis.Both;
            _duration = 0.6f;
            _intensity = 0.2f;
            _frequency = 6f;
            _xPhaseOffset = 0f;
            _yPhaseOffset = Mathf.PI * 0.5f;
        }

        /// <summary>
        /// 着地用のプリセット（縦に押しつぶれる）
        /// </summary>
        public void SetLandingPreset()
        {
            _axis = WobbleAxis.Vertical;
            _duration = 0.4f;
            _intensity = 0.15f;
            _frequency = 8f;
        }

        /// <summary>
        /// 水平揺れ用のプリセット
        /// </summary>
        public void SetHorizontalShakePreset()
        {
            _axis = WobbleAxis.Horizontal;
            _duration = 0.3f;
            _intensity = 0.1f;
            _frequency = 12f;
        }

        /// <summary>
        /// 微細な振動用のプリセット
        /// </summary>
        public void SetSubtlePreset()
        {
            _axis = WobbleAxis.Both;
            _duration = 0.2f;
            _intensity = 0.03f;
            _frequency = 15f;
        }
    }
}

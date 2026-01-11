#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にバウンス（弾み）効果を適用するコンポーネント
    /// ボタンクリック時やアイテム獲得時に使用
    /// </summary>
    public class BounceEffect : MonoBehaviour
    {
        public enum BounceType
        {
            Scale,
            Position,
            Rotation
        }

        [Header("バウンス設定")]
        [SerializeField] private BounceType _bounceType = BounceType.Scale;
        [SerializeField] private float _duration = 0.4f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _playOnEnable = false;

        [Header("スケールバウンス")]
        [SerializeField] private float _scaleAmount = 0.2f;
        [SerializeField] private int _bounceCount = 2;

        [Header("位置バウンス")]
        [SerializeField] private Vector2 _bounceDirection = Vector2.up;
        [SerializeField] private float _bounceHeight = 20f;

        [Header("回転バウンス")]
        [SerializeField] private float _rotationAmount = 10f;

        [Header("弾性設定")]
        [SerializeField, Range(0f, 1f)] private float _elasticity = 0.5f;
        [SerializeField, Range(0.1f, 2f)] private float _stiffness = 1f;

        private RectTransform? _rectTransform;
        private Vector3 _originalScale;
        private Vector2 _originalPosition;
        private Quaternion _originalRotation;
        private Coroutine? _bounceCoroutine;
        private bool _isBouncing;

        public event Action? OnBounceComplete;

        /// <summary>
        /// バウンス中かどうか
        /// </summary>
        public bool IsBouncing => _isBouncing;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
                _originalPosition = _rectTransform.anchoredPosition;
                _originalRotation = _rectTransform.localRotation;
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Bounce();
            }
        }

        private void OnDisable()
        {
            StopBounce();
        }

        /// <summary>
        /// バウンスを開始
        /// </summary>
        public void Bounce()
        {
            if (_rectTransform == null) return;

            StopBounce();
            _bounceCoroutine = StartCoroutine(BounceCoroutine());
        }

        /// <summary>
        /// バウンスを停止して元に戻る
        /// </summary>
        public void StopBounce()
        {
            if (_bounceCoroutine != null)
            {
                StopCoroutine(_bounceCoroutine);
                _bounceCoroutine = null;
            }

            ResetToOriginal();
            _isBouncing = false;
        }

        /// <summary>
        /// 元の状態にリセット
        /// </summary>
        private void ResetToOriginal()
        {
            if (_rectTransform == null) return;

            _rectTransform.localScale = _originalScale;
            _rectTransform.anchoredPosition = _originalPosition;
            _rectTransform.localRotation = _originalRotation;
        }

        /// <summary>
        /// バウンスコルーチン
        /// </summary>
        private IEnumerator BounceCoroutine()
        {
            if (_rectTransform == null) yield break;

            _isBouncing = true;
            float elapsed = 0f;
            float frequency = _stiffness * Mathf.PI * 2f * _bounceCount / _duration;

            while (elapsed < _duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = elapsed / _duration;

                // 減衰振動の計算
                float decay = Mathf.Pow(1f - _elasticity, t * 10f);
                float oscillation = Mathf.Cos(frequency * t) * decay;

                switch (_bounceType)
                {
                    case BounceType.Scale:
                        float scale = 1f + _scaleAmount * oscillation;
                        _rectTransform.localScale = _originalScale * scale;
                        break;

                    case BounceType.Position:
                        float offset = _bounceHeight * Mathf.Abs(oscillation);
                        _rectTransform.anchoredPosition = _originalPosition + _bounceDirection.normalized * offset;
                        break;

                    case BounceType.Rotation:
                        float rotation = _rotationAmount * oscillation;
                        _rectTransform.localRotation = _originalRotation * Quaternion.Euler(0, 0, rotation);
                        break;
                }

                yield return null;
            }

            ResetToOriginal();
            _isBouncing = false;
            OnBounceComplete?.Invoke();
        }

        /// <summary>
        /// 弾性バウンス値を計算（外部利用可能）
        /// </summary>
        public static float CalculateElasticValue(float t, float elasticity = 0.5f, float stiffness = 1f)
        {
            float frequency = stiffness * Mathf.PI * 4f;
            float decay = Mathf.Pow(1f - elasticity, t * 10f);
            return Mathf.Cos(frequency * t) * decay;
        }

        /// <summary>
        /// ボタンクリック用のプリセット
        /// </summary>
        public void SetButtonClickPreset()
        {
            _bounceType = BounceType.Scale;
            _duration = 0.3f;
            _scaleAmount = 0.15f;
            _bounceCount = 2;
            _elasticity = 0.4f;
            _stiffness = 1.2f;
        }

        /// <summary>
        /// アイテム獲得用のプリセット
        /// </summary>
        public void SetItemPickupPreset()
        {
            _bounceType = BounceType.Scale;
            _duration = 0.5f;
            _scaleAmount = 0.3f;
            _bounceCount = 3;
            _elasticity = 0.6f;
            _stiffness = 1f;
        }

        /// <summary>
        /// 通知ポップアップ用のプリセット
        /// </summary>
        public void SetNotificationPreset()
        {
            _bounceType = BounceType.Position;
            _bounceDirection = Vector2.down;
            _duration = 0.4f;
            _bounceHeight = 15f;
            _bounceCount = 2;
            _elasticity = 0.5f;
        }
    }
}

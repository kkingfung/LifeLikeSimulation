#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にシェイク（振動）効果を適用するコンポーネント
    /// エラー表示や衝撃表現に使用
    /// </summary>
    public class ShakeEffect : MonoBehaviour
    {
        public enum ShakeType
        {
            Position,
            Rotation,
            Both
        }

        [Header("シェイク設定")]
        [SerializeField] private ShakeType _shakeType = ShakeType.Position;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private float _magnitude = 10f;
        [SerializeField] private float _rotationMagnitude = 5f;
        [SerializeField] private float _frequency = 25f;
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("減衰設定")]
        [SerializeField] private bool _enableDamping = true;
        [SerializeField] private AnimationCurve _dampingCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private RectTransform? _rectTransform;
        private Vector2 _originalPosition;
        private Quaternion _originalRotation;
        private Coroutine? _shakeCoroutine;
        private bool _isShaking;

        public event Action? OnShakeComplete;

        /// <summary>
        /// シェイク中かどうか
        /// </summary>
        public bool IsShaking => _isShaking;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalPosition = _rectTransform.anchoredPosition;
                _originalRotation = _rectTransform.localRotation;
            }
        }

        private void OnDisable()
        {
            StopShake();
        }

        /// <summary>
        /// シェイクを開始
        /// </summary>
        public void Shake()
        {
            Shake(_duration, _magnitude);
        }

        /// <summary>
        /// シェイクを開始（パラメータ指定）
        /// </summary>
        public void Shake(float duration, float magnitude)
        {
            if (_rectTransform == null) return;

            StopShake();
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        /// <summary>
        /// シェイクを停止して元の位置に戻る
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPosition;
                _rectTransform.localRotation = _originalRotation;
            }

            _isShaking = false;
        }

        /// <summary>
        /// シェイクコルーチン
        /// </summary>
        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            if (_rectTransform == null) yield break;

            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = elapsed / duration;
                float damping = _enableDamping ? _dampingCurve.Evaluate(t) : 1f;
                float currentMagnitude = magnitude * damping;

                // 高周波ノイズを生成
                float noise = elapsed * _frequency;

                // 位置シェイク
                if (_shakeType == ShakeType.Position || _shakeType == ShakeType.Both)
                {
                    float offsetX = (Mathf.PerlinNoise(noise, 0f) * 2f - 1f) * currentMagnitude;
                    float offsetY = (Mathf.PerlinNoise(0f, noise) * 2f - 1f) * currentMagnitude;
                    _rectTransform.anchoredPosition = _originalPosition + new Vector2(offsetX, offsetY);
                }

                // 回転シェイク
                if (_shakeType == ShakeType.Rotation || _shakeType == ShakeType.Both)
                {
                    float rotationOffset = (Mathf.PerlinNoise(noise * 0.5f, noise * 0.5f) * 2f - 1f)
                                          * _rotationMagnitude * damping;
                    _rectTransform.localRotation = _originalRotation * Quaternion.Euler(0, 0, rotationOffset);
                }

                yield return null;
            }

            // 元の位置に戻す
            _rectTransform.anchoredPosition = _originalPosition;
            _rectTransform.localRotation = _originalRotation;
            _isShaking = false;
            OnShakeComplete?.Invoke();
        }

        /// <summary>
        /// エラー表示用のプリセット
        /// </summary>
        public void SetErrorPreset()
        {
            _shakeType = ShakeType.Position;
            _duration = 0.4f;
            _magnitude = 8f;
            _frequency = 30f;
            _enableDamping = true;
        }

        /// <summary>
        /// 衝撃用のプリセット
        /// </summary>
        public void SetImpactPreset()
        {
            _shakeType = ShakeType.Both;
            _duration = 0.3f;
            _magnitude = 15f;
            _rotationMagnitude = 3f;
            _frequency = 40f;
            _enableDamping = true;
        }

        /// <summary>
        /// 軽い振動用のプリセット
        /// </summary>
        public void SetVibratePreset()
        {
            _shakeType = ShakeType.Position;
            _duration = 0.2f;
            _magnitude = 3f;
            _frequency = 50f;
            _enableDamping = false;
        }
    }
}

#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にフラッシュ効果を適用するコンポーネント
    /// 成功/失敗フィードバック、ダメージ表現、注目喚起に使用
    /// </summary>
    public class FlashEffect : MonoBehaviour
    {
        public enum FlashMode
        {
            Single,    // 1回フラッシュ
            Multiple,  // 複数回フラッシュ
            Loop       // ループ（手動停止まで）
        }

        [Header("フラッシュ設定")]
        [SerializeField] private FlashMode _mode = FlashMode.Single;
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private int _flashCount = 3;
        [SerializeField] private float _flashInterval = 0.1f;
        [SerializeField] private bool _useUnscaledTime = true;

        [Header("イージング")]
        [SerializeField] private AnimationCurve _flashCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("対象設定")]
        [SerializeField] private Graphic? _targetGraphic;

        private Graphic? _graphic;
        private Color _originalColor;
        private Coroutine? _flashCoroutine;
        private bool _isFlashing;

        public event Action? OnFlashComplete;

        /// <summary>
        /// フラッシュ中かどうか
        /// </summary>
        public bool IsFlashing => _isFlashing;

        private void Awake()
        {
            _graphic = _targetGraphic != null ? _targetGraphic : GetComponent<Graphic>();
            if (_graphic != null)
            {
                _originalColor = _graphic.color;
            }
        }

        private void OnDisable()
        {
            StopFlash();
        }

        /// <summary>
        /// フラッシュを開始
        /// </summary>
        public void Flash()
        {
            Flash(_flashColor);
        }

        /// <summary>
        /// 指定色でフラッシュを開始
        /// </summary>
        public void Flash(Color color)
        {
            StopFlash();
            _flashCoroutine = StartCoroutine(FlashCoroutine(color));
        }

        /// <summary>
        /// 白フラッシュ（成功表現）
        /// </summary>
        public void FlashWhite()
        {
            Flash(Color.white);
        }

        /// <summary>
        /// 赤フラッシュ（ダメージ/エラー表現）
        /// </summary>
        public void FlashRed()
        {
            Flash(new Color(1f, 0.2f, 0.2f, 1f));
        }

        /// <summary>
        /// 緑フラッシュ（成功表現）
        /// </summary>
        public void FlashGreen()
        {
            Flash(new Color(0.2f, 1f, 0.4f, 1f));
        }

        /// <summary>
        /// シアンフラッシュ（アクセント色）
        /// </summary>
        public void FlashCyan()
        {
            Flash(new Color(0f, 0.9f, 1f, 1f));
        }

        /// <summary>
        /// フラッシュを停止
        /// </summary>
        public void StopFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                _flashCoroutine = null;
            }

            if (_graphic != null)
            {
                _graphic.color = _originalColor;
            }

            _isFlashing = false;
        }

        /// <summary>
        /// フラッシュコルーチン
        /// </summary>
        private IEnumerator FlashCoroutine(Color flashColor)
        {
            if (_graphic == null) yield break;

            _isFlashing = true;
            _originalColor = _graphic.color;

            int count = _mode == FlashMode.Single ? 1 :
                       _mode == FlashMode.Multiple ? _flashCount :
                       int.MaxValue;

            int currentFlash = 0;

            while (currentFlash < count && (_mode == FlashMode.Loop || currentFlash < count))
            {
                // フラッシュイン
                float elapsed = 0f;
                while (elapsed < _flashDuration * 0.5f)
                {
                    float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    elapsed += deltaTime;

                    float t = elapsed / (_flashDuration * 0.5f);
                    float intensity = _flashCurve.Evaluate(t * 0.5f);
                    _graphic.color = Color.Lerp(_originalColor, flashColor, intensity);

                    yield return null;
                }

                // フラッシュアウト
                elapsed = 0f;
                while (elapsed < _flashDuration * 0.5f)
                {
                    float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    elapsed += deltaTime;

                    float t = elapsed / (_flashDuration * 0.5f);
                    float intensity = _flashCurve.Evaluate(0.5f + t * 0.5f);
                    _graphic.color = Color.Lerp(_originalColor, flashColor, intensity);

                    yield return null;
                }

                _graphic.color = _originalColor;
                currentFlash++;

                // インターバル
                if (currentFlash < count && _flashInterval > 0)
                {
                    if (_useUnscaledTime)
                        yield return new WaitForSecondsRealtime(_flashInterval);
                    else
                        yield return new WaitForSeconds(_flashInterval);
                }
            }

            _graphic.color = _originalColor;
            _isFlashing = false;
            OnFlashComplete?.Invoke();
        }

        /// <summary>
        /// ダメージ用のプリセット
        /// </summary>
        public void SetDamagePreset()
        {
            _mode = FlashMode.Multiple;
            _flashColor = new Color(1f, 0.2f, 0.2f, 1f);
            _flashDuration = 0.1f;
            _flashCount = 2;
            _flashInterval = 0.05f;
        }

        /// <summary>
        /// 成功用のプリセット
        /// </summary>
        public void SetSuccessPreset()
        {
            _mode = FlashMode.Single;
            _flashColor = new Color(0.2f, 1f, 0.4f, 1f);
            _flashDuration = 0.2f;
        }

        /// <summary>
        /// 警告用のプリセット
        /// </summary>
        public void SetWarningPreset()
        {
            _mode = FlashMode.Loop;
            _flashColor = new Color(1f, 0.7f, 0.2f, 1f);
            _flashDuration = 0.3f;
            _flashInterval = 0.2f;
        }

        /// <summary>
        /// ハイライト用のプリセット
        /// </summary>
        public void SetHighlightPreset()
        {
            _mode = FlashMode.Single;
            _flashColor = Color.white;
            _flashDuration = 0.15f;
        }
    }
}

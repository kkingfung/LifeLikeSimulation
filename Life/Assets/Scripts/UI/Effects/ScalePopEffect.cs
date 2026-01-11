#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にポップイン/アウト効果を適用するコンポーネント
    /// ダイアログやモーダルの表示に使用
    /// オーバーシュート付きのスケールアニメーション
    /// </summary>
    public class ScalePopEffect : MonoBehaviour
    {
        public enum PopState
        {
            Hidden,
            Visible,
            Animating
        }

        [Header("ポップ設定")]
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _popOnEnable = false;
        [SerializeField] private bool _startHidden = true;

        [Header("スケール設定")]
        [SerializeField] private float _hiddenScale = 0f;
        [SerializeField] private float _visibleScale = 1f;
        [SerializeField] private float _overshootScale = 1.1f;
        [SerializeField] private float _overshootRatio = 0.6f; // アニメーションの何割でオーバーシュートに達するか

        [Header("追加効果")]
        [SerializeField] private bool _fadeWithPop = true;
        [SerializeField] private float _hiddenAlpha = 0f;
        [SerializeField] private bool _deactivateOnHide = false;

        [Header("イージング")]
        [SerializeField] private AnimationCurve _popInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _popOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private RectTransform? _rectTransform;
        private CanvasGroup? _canvasGroup;
        private Vector3 _originalScale;
        private Coroutine? _popCoroutine;
        private PopState _state = PopState.Visible;

        public event Action? OnPopInComplete;
        public event Action? OnPopOutComplete;

        /// <summary>
        /// 現在の状態
        /// </summary>
        public PopState State => _state;

        /// <summary>
        /// 表示されているか
        /// </summary>
        public bool IsVisible => _state == PopState.Visible;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_rectTransform != null)
            {
                _originalScale = _rectTransform.localScale;
            }

            if (_startHidden)
            {
                SetHidden();
            }
        }

        private void OnEnable()
        {
            if (_popOnEnable && _startHidden)
            {
                PopIn();
            }
        }

        private void OnDisable()
        {
            StopPop();
        }

        /// <summary>
        /// ポップインを開始
        /// </summary>
        public void PopIn()
        {
            PopIn(_duration);
        }

        /// <summary>
        /// ポップインを開始（時間指定）
        /// </summary>
        public void PopIn(float duration)
        {
            if (_rectTransform == null) return;

            gameObject.SetActive(true);
            StopPop();
            _popCoroutine = StartCoroutine(PopInCoroutine(duration));
        }

        /// <summary>
        /// ポップアウトを開始
        /// </summary>
        public void PopOut()
        {
            PopOut(_duration);
        }

        /// <summary>
        /// ポップアウトを開始（時間指定）
        /// </summary>
        public void PopOut(float duration)
        {
            if (_rectTransform == null) return;

            StopPop();
            _popCoroutine = StartCoroutine(PopOutCoroutine(duration));
        }

        /// <summary>
        /// 表示/非表示をトグル
        /// </summary>
        public void Toggle()
        {
            if (_state == PopState.Visible)
            {
                PopOut();
            }
            else if (_state == PopState.Hidden)
            {
                PopIn();
            }
        }

        /// <summary>
        /// ポップを停止
        /// </summary>
        public void StopPop()
        {
            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
                _popCoroutine = null;
            }
        }

        /// <summary>
        /// 即座に表示状態に
        /// </summary>
        public void SetVisible()
        {
            StopPop();
            gameObject.SetActive(true);
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale * _visibleScale;
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
            _state = PopState.Visible;
        }

        /// <summary>
        /// 即座に非表示状態に
        /// </summary>
        public void SetHidden()
        {
            StopPop();
            if (_rectTransform != null)
            {
                _rectTransform.localScale = _originalScale * _hiddenScale;
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _hiddenAlpha;
            }
            _state = PopState.Hidden;

            if (_deactivateOnHide)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// ポップインコルーチン
        /// </summary>
        private IEnumerator PopInCoroutine(float duration)
        {
            if (_rectTransform == null) yield break;

            _state = PopState.Animating;
            float elapsed = 0f;

            _rectTransform.localScale = _originalScale * _hiddenScale;
            if (_canvasGroup != null && _fadeWithPop)
            {
                _canvasGroup.alpha = _hiddenAlpha;
            }

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = _popInCurve.Evaluate(t);

                // オーバーシュート付きスケール計算
                float scale;
                if (t < _overshootRatio)
                {
                    // オーバーシュートまで
                    float overshootT = t / _overshootRatio;
                    scale = Mathf.Lerp(_hiddenScale, _overshootScale, overshootT);
                }
                else
                {
                    // オーバーシュートから戻る
                    float settleT = (t - _overshootRatio) / (1f - _overshootRatio);
                    scale = Mathf.Lerp(_overshootScale, _visibleScale, settleT);
                }

                _rectTransform.localScale = _originalScale * scale;

                if (_canvasGroup != null && _fadeWithPop)
                {
                    _canvasGroup.alpha = Mathf.Lerp(_hiddenAlpha, 1f, curvedT);
                }

                yield return null;
            }

            _rectTransform.localScale = _originalScale * _visibleScale;
            if (_canvasGroup != null && _fadeWithPop)
            {
                _canvasGroup.alpha = 1f;
            }

            _state = PopState.Visible;
            OnPopInComplete?.Invoke();
        }

        /// <summary>
        /// ポップアウトコルーチン
        /// </summary>
        private IEnumerator PopOutCoroutine(float duration)
        {
            if (_rectTransform == null) yield break;

            _state = PopState.Animating;
            float elapsed = 0f;

            Vector3 startScale = _rectTransform.localScale;
            float startAlpha = _canvasGroup?.alpha ?? 1f;

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = _popOutCurve.Evaluate(t);

                float scale = Mathf.Lerp(_visibleScale, _hiddenScale, curvedT);
                _rectTransform.localScale = _originalScale * scale;

                if (_canvasGroup != null && _fadeWithPop)
                {
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, _hiddenAlpha, curvedT);
                }

                yield return null;
            }

            _rectTransform.localScale = _originalScale * _hiddenScale;
            if (_canvasGroup != null && _fadeWithPop)
            {
                _canvasGroup.alpha = _hiddenAlpha;
            }

            _state = PopState.Hidden;

            if (_deactivateOnHide)
            {
                gameObject.SetActive(false);
            }

            OnPopOutComplete?.Invoke();
        }

        /// <summary>
        /// ダイアログ用のプリセット
        /// </summary>
        public void SetDialogPreset()
        {
            _duration = 0.25f;
            _hiddenScale = 0.8f;
            _visibleScale = 1f;
            _overshootScale = 1.05f;
            _overshootRatio = 0.7f;
            _fadeWithPop = true;
        }

        /// <summary>
        /// アイコンポップ用のプリセット
        /// </summary>
        public void SetIconPopPreset()
        {
            _duration = 0.3f;
            _hiddenScale = 0f;
            _visibleScale = 1f;
            _overshootScale = 1.2f;
            _overshootRatio = 0.5f;
            _fadeWithPop = true;
        }

        /// <summary>
        /// 通知用のプリセット
        /// </summary>
        public void SetNotificationPreset()
        {
            _duration = 0.2f;
            _hiddenScale = 0.5f;
            _visibleScale = 1f;
            _overshootScale = 1.08f;
            _overshootRatio = 0.6f;
            _fadeWithPop = true;
        }
    }
}

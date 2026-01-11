#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にスライドイン/アウト効果を適用するコンポーネント
    /// パネルやメニューの表示/非表示に使用
    /// </summary>
    [AddComponentMenu("LifeLike/UI/Effects/Slide Effect")]
    public class SlideEffect : MonoBehaviour
    {
        public enum SlideDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        public enum SlideState
        {
            Hidden,
            Visible,
            Sliding
        }

        [Header("Slide Settings")]
        [SerializeField] private SlideDirection _slideDirection = SlideDirection.Left;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private float _slideDistance = 0f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _slideOnEnable = false;
        [SerializeField] private bool _startHidden = false;

        [Header("Easing")]
        [SerializeField] private AnimationCurve _slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Additional Effects")]
        [SerializeField] private bool _fadeWithSlide = true;
        [SerializeField] private float _fadeStartAlpha = 0f;

        private RectTransform? _rectTransform;
        private CanvasGroup? _canvasGroup;
        private Vector2 _visiblePosition;
        private Vector2 _hiddenPosition;
        private Coroutine? _slideCoroutine;
        private SlideState _state = SlideState.Visible;

        public event Action? OnSlideInComplete;
        public event Action? OnSlideOutComplete;

        public SlideState State => _state;
        public bool IsVisible => _state == SlideState.Visible;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_rectTransform != null)
            {
                _visiblePosition = _rectTransform.anchoredPosition;
                CalculateHiddenPosition();
            }

            if (_startHidden)
            {
                SetHidden();
            }
        }

        private void OnEnable()
        {
            if (_slideOnEnable && _startHidden)
            {
                SlideIn();
            }
        }

        private void OnDisable()
        {
            StopSlide();
        }

        private void CalculateHiddenPosition()
        {
            if (_rectTransform == null) return;

            float distance = _slideDistance;
            if (distance <= 0)
            {
                distance = _slideDirection switch
                {
                    SlideDirection.Left or SlideDirection.Right => _rectTransform.rect.width + 100f,
                    SlideDirection.Up or SlideDirection.Down => _rectTransform.rect.height + 100f,
                    _ => 500f
                };
            }

            Vector2 offset = _slideDirection switch
            {
                SlideDirection.Left => new Vector2(-distance, 0),
                SlideDirection.Right => new Vector2(distance, 0),
                SlideDirection.Up => new Vector2(0, distance),
                SlideDirection.Down => new Vector2(0, -distance),
                _ => Vector2.zero
            };

            _hiddenPosition = _visiblePosition + offset;
        }

        public void SlideIn()
        {
            SlideIn(_duration);
        }

        public void SlideIn(float duration)
        {
            if (_rectTransform == null) return;

            StopSlide();
            _slideCoroutine = StartCoroutine(SlideCoroutine(
                _hiddenPosition, _visiblePosition,
                _fadeStartAlpha, 1f,
                duration, _slideInCurve,
                () =>
                {
                    _state = SlideState.Visible;
                    OnSlideInComplete?.Invoke();
                }
            ));
        }

        public void SlideOut()
        {
            SlideOut(_duration);
        }

        public void SlideOut(float duration)
        {
            if (_rectTransform == null) return;

            StopSlide();
            _slideCoroutine = StartCoroutine(SlideCoroutine(
                _visiblePosition, _hiddenPosition,
                1f, _fadeStartAlpha,
                duration, _slideOutCurve,
                () =>
                {
                    _state = SlideState.Hidden;
                    OnSlideOutComplete?.Invoke();
                }
            ));
        }

        public void Toggle()
        {
            if (_state == SlideState.Visible)
            {
                SlideOut();
            }
            else if (_state == SlideState.Hidden)
            {
                SlideIn();
            }
        }

        public void StopSlide()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }
        }

        public void SetVisible()
        {
            StopSlide();
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _visiblePosition;
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
            _state = SlideState.Visible;
        }

        public void SetHidden()
        {
            StopSlide();
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _hiddenPosition;
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _fadeStartAlpha;
            }
            _state = SlideState.Hidden;
        }

        private IEnumerator SlideCoroutine(
            Vector2 fromPos, Vector2 toPos,
            float fromAlpha, float toAlpha,
            float duration, AnimationCurve curve,
            Action? onComplete)
        {
            if (_rectTransform == null) yield break;

            _state = SlideState.Sliding;
            float elapsed = 0f;

            _rectTransform.anchoredPosition = fromPos;
            if (_canvasGroup != null && _fadeWithSlide)
            {
                _canvasGroup.alpha = fromAlpha;
            }

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = curve.Evaluate(t);

                _rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, curvedT);

                if (_canvasGroup != null && _fadeWithSlide)
                {
                    _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, curvedT);
                }

                yield return null;
            }

            _rectTransform.anchoredPosition = toPos;
            if (_canvasGroup != null && _fadeWithSlide)
            {
                _canvasGroup.alpha = toAlpha;
            }

            onComplete?.Invoke();
        }

        public void SetDirection(SlideDirection direction)
        {
            _slideDirection = direction;
            CalculateHiddenPosition();
        }
    }
}

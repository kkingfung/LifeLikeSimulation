#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// UI要素にフェードイン/アウト効果を適用するコンポーネント
    /// 画面遷移やUI表示/非表示に使用
    /// </summary>
    public class FadeEffect : MonoBehaviour
    {
        public enum FadeDirection
        {
            In,
            Out
        }

        [Header("フェード設定")]
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private float _delay = 0f;
        [SerializeField] private bool _useUnscaledTime = true;
        [SerializeField] private bool _fadeOnEnable = false;
        [SerializeField] private FadeDirection _fadeOnEnableDirection = FadeDirection.In;
        [SerializeField] private bool _deactivateOnFadeOut = false;

        [Header("イージング")]
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


        private CanvasGroup? _canvasGroup;
        private Graphic? _graphic;
        private Coroutine? _fadeCoroutine;
        private bool _isFading;

        public event Action? OnFadeInComplete;
        public event Action? OnFadeOutComplete;

        /// <summary>
        /// フェード中かどうか
        /// </summary>
        public bool IsFading => _isFading;

        /// <summary>
        /// 現在のアルファ値
        /// </summary>
        public float Alpha
        {
            get
            {
                if (_canvasGroup != null) return _canvasGroup.alpha;
                if (_graphic != null) return _graphic.color.a;
                return 1f;
            }
            set
            {
                if (_canvasGroup != null) _canvasGroup.alpha = value;
                else if (_graphic != null)
                {
                    var color = _graphic.color;
                    color.a = value;
                    _graphic.color = color;
                }
            }
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _graphic = GetComponent<Graphic>();
            }
        }

        private void OnEnable()
        {
            if (_fadeOnEnable)
            {
                if (_fadeOnEnableDirection == FadeDirection.In)
                {
                    Alpha = 0f;
                    FadeIn();
                }
                else
                {
                    Alpha = 1f;
                    FadeOut();
                }
            }
        }

        private void OnDisable()
        {
            StopFade();
        }

        /// <summary>
        /// フェードインを開始
        /// </summary>
        public void FadeIn()
        {
            FadeIn(_duration);
        }

        /// <summary>
        /// フェードインを開始（時間指定）
        /// </summary>
        public void FadeIn(float duration)
        {
            StartFade(0f, 1f, duration, () => OnFadeInComplete?.Invoke());
        }

        /// <summary>
        /// フェードアウトを開始
        /// </summary>
        public void FadeOut()
        {
            FadeOut(_duration);
        }

        /// <summary>
        /// フェードアウトを開始（時間指定）
        /// </summary>
        public void FadeOut(float duration)
        {
            StartFade(1f, 0f, duration, () =>
            {
                OnFadeOutComplete?.Invoke();
                if (_deactivateOnFadeOut)
                {
                    gameObject.SetActive(false);
                }
            });
        }

        /// <summary>
        /// 指定のアルファ値にフェード
        /// </summary>
        public void FadeTo(float targetAlpha, float duration)
        {
            StartFade(Alpha, targetAlpha, duration, null);
        }

        /// <summary>
        /// フェードを停止
        /// </summary>
        public void StopFade()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _isFading = false;
        }

        /// <summary>
        /// 即座に表示
        /// </summary>
        public void ShowImmediate()
        {
            StopFade();
            Alpha = 1f;
        }

        /// <summary>
        /// 即座に非表示
        /// </summary>
        public void HideImmediate()
        {
            StopFade();
            Alpha = 0f;
            if (_deactivateOnFadeOut)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// フェードを開始
        /// </summary>
        private void StartFade(float from, float to, float duration, Action? onComplete)
        {
            StopFade();
            _fadeCoroutine = StartCoroutine(FadeCoroutine(from, to, duration, onComplete));
        }

        /// <summary>
        /// フェードコルーチン
        /// </summary>
        private IEnumerator FadeCoroutine(float from, float to, float duration, Action? onComplete)
        {
            _isFading = true;

            // ディレイ
            if (_delay > 0)
            {
                if (_useUnscaledTime)
                    yield return new WaitForSecondsRealtime(_delay);
                else
                    yield return new WaitForSeconds(_delay);
            }

            Alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curvedT = _fadeCurve.Evaluate(t);
                Alpha = Mathf.Lerp(from, to, curvedT);

                yield return null;
            }

            Alpha = to;
            _isFading = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// CanvasGroupを取得または作成
        /// </summary>
        public CanvasGroup GetOrCreateCanvasGroup()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            return _canvasGroup;
        }
    }
}

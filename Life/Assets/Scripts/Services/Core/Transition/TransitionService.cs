#nullable enable
using System;
using System.Threading.Tasks;
using LifeLike.Data;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Services.Core.Transition
{
    /// <summary>
    /// シーン間のトランジション（画面遷移演出）を管理するサービス
    /// </summary>
    public class TransitionService : ITransitionService, IDisposable
    {
        private CanvasGroup? _fadeCanvasGroup;
        private Image? _fadeImage;
        private bool _isTransitioning;
        private bool _skipRequested;
        private bool _isDisposed;

        /// <inheritdoc/>
        public bool IsTransitioning => _isTransitioning;

        /// <inheritdoc/>
        public event Action? OnTransitionStarted;

        /// <inheritdoc/>
        public event Action? OnTransitionCompleted;

        /// <inheritdoc/>
        public event Action? OnTransitionMidpoint;

        /// <summary>
        /// トランジション用のCanvasを初期化する
        /// </summary>
        /// <param name="fadeCanvasGroup">フェード用のCanvasGroup</param>
        /// <param name="fadeImage">フェード用のImage</param>
        public void Initialize(CanvasGroup fadeCanvasGroup, Image fadeImage)
        {
            _fadeCanvasGroup = fadeCanvasGroup;
            _fadeImage = fadeImage;

            // 初期状態は透明
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
                _fadeCanvasGroup.blocksRaycasts = false;
            }

            Debug.Log("[TransitionService] 初期化完了");
        }

        /// <inheritdoc/>
        public async Task ExecuteTransition(TransitionSettings settings, Action? onMidpoint = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[TransitionService] トランジションが既に進行中です。");
                return;
            }

            if (settings.type == TransitionType.None)
            {
                onMidpoint?.Invoke();
                return;
            }

            _isTransitioning = true;
            _skipRequested = false;
            OnTransitionStarted?.Invoke();

            try
            {
                // フェードアウト
                await FadeOut(settings);

                // 中間点
                OnTransitionMidpoint?.Invoke();
                onMidpoint?.Invoke();

                // 少し待機
                await Task.Delay(50);

                // フェードイン
                await FadeIn(settings);
            }
            finally
            {
                _isTransitioning = false;
                OnTransitionCompleted?.Invoke();
            }
        }

        /// <inheritdoc/>
        public async Task FadeOut(TransitionSettings settings)
        {
            if (_fadeCanvasGroup == null || _fadeImage == null)
            {
                return;
            }

            // フェード色を設定
            SetFadeColor(settings.type);

            _fadeCanvasGroup.blocksRaycasts = true;
            var duration = settings.duration / 2f;

            await AnimateFade(0f, 1f, duration, settings.curve);
        }

        /// <inheritdoc/>
        public async Task FadeIn(TransitionSettings settings)
        {
            if (_fadeCanvasGroup == null)
            {
                return;
            }

            var duration = settings.duration / 2f;
            await AnimateFade(1f, 0f, duration, settings.curve);

            _fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <inheritdoc/>
        public void Skip()
        {
            _skipRequested = true;
        }

        /// <summary>
        /// フェード色を設定する
        /// </summary>
        private void SetFadeColor(TransitionType type)
        {
            if (_fadeImage == null) return;

            _fadeImage.color = type switch
            {
                TransitionType.FadeToBlack => Color.black,
                TransitionType.FadeToWhite => Color.white,
                _ => Color.black
            };
        }

        /// <summary>
        /// フェードアニメーションを実行する
        /// </summary>
        private async Task AnimateFade(float from, float to, float duration, AnimationCurve curve)
        {
            if (_fadeCanvasGroup == null) return;

            var elapsed = 0f;

            while (elapsed < duration && !_skipRequested)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var curveValue = curve.Evaluate(t);
                _fadeCanvasGroup.alpha = Mathf.Lerp(from, to, curveValue);

                await Task.Yield();
            }

            _fadeCanvasGroup.alpha = to;
        }

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _fadeCanvasGroup = null;
            _fadeImage = null;
            OnTransitionStarted = null;
            OnTransitionCompleted = null;
            OnTransitionMidpoint = null;

            _isDisposed = true;
        }
    }
}

#nullable enable
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// テキストにタイプライター効果を適用するコンポーネント
    /// 1文字ずつ表示し、オペレーター端末の雰囲気を演出
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("タイピング設定")]
        [SerializeField] private float _charactersPerSecond = 30f;
        [SerializeField] private float _startDelay = 0.5f;
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _showCursor = true;
        [SerializeField] private string _cursorChar = "_";
        [SerializeField] private float _cursorBlinkRate = 0.5f;

        [Header("サウンド設定")]
        [SerializeField] private AudioSource? _audioSource;
        [SerializeField] private AudioClip? _typingSound;
        [SerializeField, Range(0f, 1f)] private float _typingSoundVolume = 0.3f;

        [Header("完了後設定")]
        [SerializeField] private bool _keepCursorAfterComplete = true;
        [SerializeField] private float _cursorHideDelay = 2f;

        private Text? _text;
        private string _fullText = string.Empty;
        private bool _isTyping;
        private bool _isCursorVisible;
        private Coroutine? _typingCoroutine;
        private Coroutine? _cursorCoroutine;

        public event Action? OnTypingStarted;
        public event Action? OnTypingComplete;

        /// <summary>
        /// タイピング中かどうか
        /// </summary>
        public bool IsTyping => _isTyping;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void Start()
        {
            if (_text != null && _playOnStart)
            {
                _fullText = _text.text;
                StartTyping(_fullText);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isTyping = false;
        }

        /// <summary>
        /// タイピング効果を開始
        /// </summary>
        public void StartTyping(string text)
        {
            if (_text == null) return;

            StopTyping();
            _fullText = text;
            _typingCoroutine = StartCoroutine(TypeText());
        }

        /// <summary>
        /// タイピングを停止して全文を表示
        /// </summary>
        public void CompleteImmediately()
        {
            StopTyping();
            if (_text != null)
            {
                _text.text = _fullText;
            }
            OnTypingComplete?.Invoke();

            if (_showCursor && _keepCursorAfterComplete)
            {
                _cursorCoroutine = StartCoroutine(BlinkCursor());
            }
        }

        /// <summary>
        /// タイピングを停止
        /// </summary>
        public void StopTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            if (_cursorCoroutine != null)
            {
                StopCoroutine(_cursorCoroutine);
                _cursorCoroutine = null;
            }

            _isTyping = false;
        }

        /// <summary>
        /// テキストを1文字ずつ表示するコルーチン
        /// </summary>
        private IEnumerator TypeText()
        {
            if (_text == null) yield break;

            _isTyping = true;
            _text.text = string.Empty;
            OnTypingStarted?.Invoke();

            yield return new WaitForSeconds(_startDelay);

            float charInterval = 1f / _charactersPerSecond;
            int charIndex = 0;

            while (charIndex < _fullText.Length)
            {
                char currentChar = _fullText[charIndex];
                _text.text = _fullText.Substring(0, charIndex + 1);

                if (_showCursor)
                {
                    _text.text += _cursorChar;
                }

                // タイピング音を再生
                if (_audioSource != null && _typingSound != null && !char.IsWhiteSpace(currentChar))
                {
                    _audioSource.PlayOneShot(_typingSound, _typingSoundVolume);
                }

                charIndex++;

                // 句読点で少し長めのポーズ
                float delay = charInterval;
                if (currentChar == '。' || currentChar == '.' || currentChar == '!' || currentChar == '?')
                {
                    delay *= 3f;
                }
                else if (currentChar == '、' || currentChar == ',')
                {
                    delay *= 1.5f;
                }

                yield return new WaitForSeconds(delay);
            }

            _isTyping = false;

            // カーソルを非表示にするか、点滅を開始
            if (_showCursor)
            {
                if (_keepCursorAfterComplete)
                {
                    _cursorCoroutine = StartCoroutine(BlinkCursor());
                }
                else
                {
                    _text.text = _fullText;
                }
            }

            OnTypingComplete?.Invoke();
        }

        /// <summary>
        /// カーソルを点滅させるコルーチン
        /// </summary>
        private IEnumerator BlinkCursor()
        {
            if (_text == null) yield break;

            float elapsed = 0f;
            bool hideCursor = !_keepCursorAfterComplete || _cursorHideDelay > 0;

            while (!hideCursor || elapsed < _cursorHideDelay)
            {
                _isCursorVisible = !_isCursorVisible;
                _text.text = _fullText + (_isCursorVisible ? _cursorChar : " ");

                yield return new WaitForSeconds(_cursorBlinkRate);
                elapsed += _cursorBlinkRate;
            }

            // カーソルを非表示
            _text.text = _fullText;
        }

        /// <summary>
        /// タイピング速度を設定
        /// </summary>
        public void SetTypingSpeed(float charactersPerSecond)
        {
            _charactersPerSecond = Mathf.Max(1f, charactersPerSecond);
        }
    }
}

#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// デジタル時計表示コンポーネント
    /// コロンの点滅、セグメント風表示などを実現
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class DigitalClockDisplay : MonoBehaviour
    {
        [Header("表示設定")]
        [SerializeField] private bool _show24Hour = true;
        [SerializeField] private bool _showSeconds = false;
        [SerializeField] private bool _blinkColon = true;
        [SerializeField] private float _blinkInterval = 0.5f;

        [Header("色設定")]
        [SerializeField] private Color _normalColor = new Color(0f, 0.9f, 1f, 1f);
        [SerializeField] private Color _dimColor = new Color(0f, 0.6f, 0.7f, 1f);

        [Header("グロー効果")]
        [SerializeField] private bool _enableGlow = true;
        [SerializeField] private Color _glowColor = new Color(0f, 0.5f, 0.6f, 0.5f);
        [SerializeField] private Shadow? _glowShadow;

        private Text? _text;
        private float _blinkTimer;
        private bool _colonVisible = true;
        private int _currentHour;
        private int _currentMinute;
        private int _currentSecond;

        public event Action<int, int>? OnTimeChanged;

        private void Awake()
        {
            _text = GetComponent<Text>();

            if (_enableGlow && _glowShadow == null)
            {
                _glowShadow = GetComponent<Shadow>();
                if (_glowShadow == null)
                {
                    _glowShadow = gameObject.AddComponent<Shadow>();
                    _glowShadow.effectColor = _glowColor;
                    _glowShadow.effectDistance = new Vector2(0, 0);
                }
            }
        }

        private void Update()
        {
            if (!_blinkColon) return;

            _blinkTimer += Time.unscaledDeltaTime;
            if (_blinkTimer >= _blinkInterval)
            {
                _blinkTimer = 0f;
                _colonVisible = !_colonVisible;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 時刻を設定（分単位）
        /// </summary>
        public void SetTime(int totalMinutes)
        {
            int hours = (totalMinutes / 60) % 24;
            int minutes = totalMinutes % 60;
            SetTime(hours, minutes, 0);
        }

        /// <summary>
        /// 時刻を設定
        /// </summary>
        public void SetTime(int hours, int minutes, int seconds = 0)
        {
            bool changed = _currentHour != hours || _currentMinute != minutes;

            _currentHour = hours;
            _currentMinute = minutes;
            _currentSecond = seconds;

            UpdateDisplay();

            if (changed)
            {
                OnTimeChanged?.Invoke(_currentHour, _currentMinute);
            }
        }

        /// <summary>
        /// フォーマット済み時刻文字列を設定
        /// </summary>
        public void SetTimeString(string timeString)
        {
            if (_text != null)
            {
                _text.text = timeString;
            }
        }

        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            if (_text == null) return;

            int displayHour = _currentHour;
            string ampm = "";

            if (!_show24Hour)
            {
                ampm = displayHour >= 12 ? " PM" : " AM";
                displayHour = displayHour % 12;
                if (displayHour == 0) displayHour = 12;
            }

            string colon = _colonVisible ? ":" : " ";

            if (_showSeconds)
            {
                _text.text = $"{displayHour:D2}{colon}{_currentMinute:D2}{colon}{_currentSecond:D2}{ampm}";
            }
            else
            {
                _text.text = $"{displayHour:D2}{colon}{_currentMinute:D2}{ampm}";
            }

            // グロー効果を更新
            if (_enableGlow && _glowShadow != null)
            {
                _glowShadow.effectColor = _colonVisible ? _glowColor : new Color(_glowColor.r, _glowColor.g, _glowColor.b, _glowColor.a * 0.5f);
            }
        }

        /// <summary>
        /// 点滅を有効/無効にする
        /// </summary>
        public void SetBlinkEnabled(bool enabled)
        {
            _blinkColon = enabled;
            if (!enabled)
            {
                _colonVisible = true;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// 緊急モードを設定（赤色で点滅）
        /// </summary>
        public void SetUrgentMode(bool urgent)
        {
            if (_text == null) return;

            if (urgent)
            {
                _text.color = new Color(1f, 0.3f, 0.3f, 1f);
                _blinkInterval = 0.25f;
            }
            else
            {
                _text.color = _normalColor;
                _blinkInterval = 0.5f;
            }
        }

        /// <summary>
        /// 現在の時刻を取得
        /// </summary>
        public (int hours, int minutes, int seconds) GetCurrentTime()
        {
            return (_currentHour, _currentMinute, _currentSecond);
        }
    }
}

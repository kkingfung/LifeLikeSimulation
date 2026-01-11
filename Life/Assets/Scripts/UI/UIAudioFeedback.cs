#nullable enable
using UnityEngine;

namespace LifeLike.UI
{
    /// <summary>
    /// UI操作のオーディオフィードバックを管理するシングルトン
    /// ボタンクリック、ホバー、通知などの音を再生
    /// </summary>
    public class UIAudioFeedback : MonoBehaviour
    {
        private static UIAudioFeedback? _instance;
        public static UIAudioFeedback? Instance => _instance;

        [Header("Audio Source")]
        [SerializeField] private AudioSource? _audioSource;

        [Header("ボタン音")]
        [SerializeField] private AudioClip? _buttonClick;
        [SerializeField] private AudioClip? _buttonHover;
        [SerializeField] private AudioClip? _buttonDisabled;

        [Header("通話音")]
        [SerializeField] private AudioClip? _incomingCall;
        [SerializeField] private AudioClip? _callAnswer;
        [SerializeField] private AudioClip? _callEnd;
        [SerializeField] private AudioClip? _callMissed;

        [Header("通知音")]
        [SerializeField] private AudioClip? _notification;
        [SerializeField] private AudioClip? _warning;
        [SerializeField] private AudioClip? _error;
        [SerializeField] private AudioClip? _success;

        [Header("タイピング音")]
        [SerializeField] private AudioClip? _keyPress;
        [SerializeField] private AudioClip? _keyPressAlt;

        [Header("その他")]
        [SerializeField] private AudioClip? _timerTick;
        [SerializeField] private AudioClip? _timerUrgent;
        [SerializeField] private AudioClip? _evidenceDiscovered;

        [Header("ボリューム設定")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _buttonVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _notificationVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _callVolume = 0.8f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSourceがない場合は作成
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            _audioSource.playOnAwake = false;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 音を再生
        /// </summary>
        private void PlaySound(AudioClip? clip, float volumeMultiplier = 1f)
        {
            if (_audioSource == null || clip == null) return;
            _audioSource.PlayOneShot(clip, _masterVolume * volumeMultiplier);
        }

        #region ボタン音

        public void PlayButtonClick()
        {
            PlaySound(_buttonClick, _buttonVolume);
        }

        public void PlayButtonHover()
        {
            PlaySound(_buttonHover, _buttonVolume * 0.5f);
        }

        public void PlayButtonDisabled()
        {
            PlaySound(_buttonDisabled, _buttonVolume * 0.3f);
        }

        #endregion

        #region 通話音

        public void PlayIncomingCall()
        {
            PlaySound(_incomingCall, _callVolume);
        }

        public void PlayCallAnswer()
        {
            PlaySound(_callAnswer, _callVolume);
        }

        public void PlayCallEnd()
        {
            PlaySound(_callEnd, _callVolume);
        }

        public void PlayCallMissed()
        {
            PlaySound(_callMissed, _callVolume);
        }

        #endregion

        #region 通知音

        public void PlayNotification()
        {
            PlaySound(_notification, _notificationVolume);
        }

        public void PlayWarning()
        {
            PlaySound(_warning, _notificationVolume);
        }

        public void PlayError()
        {
            PlaySound(_error, _notificationVolume);
        }

        public void PlaySuccess()
        {
            PlaySound(_success, _notificationVolume);
        }

        #endregion

        #region タイピング音

        public void PlayKeyPress()
        {
            var clip = Random.value > 0.5f ? _keyPress : _keyPressAlt;
            PlaySound(clip ?? _keyPress, _buttonVolume * 0.3f);
        }

        #endregion

        #region その他

        public void PlayTimerTick()
        {
            PlaySound(_timerTick, _buttonVolume * 0.4f);
        }

        public void PlayTimerUrgent()
        {
            PlaySound(_timerUrgent, _notificationVolume);
        }

        public void PlayEvidenceDiscovered()
        {
            PlaySound(_evidenceDiscovered, _notificationVolume);
        }

        #endregion

        #region ボリューム設定

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
        }

        public void SetButtonVolume(float volume)
        {
            _buttonVolume = Mathf.Clamp01(volume);
        }

        public void SetNotificationVolume(float volume)
        {
            _notificationVolume = Mathf.Clamp01(volume);
        }

        public void SetCallVolume(float volume)
        {
            _callVolume = Mathf.Clamp01(volume);
        }

        #endregion
    }
}

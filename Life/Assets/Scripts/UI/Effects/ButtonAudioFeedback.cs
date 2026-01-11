#nullable enable
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LifeLike.UI.Effects
{
    /// <summary>
    /// ボタンにオーディオフィードバックを追加するコンポーネント
    /// UIAudioFeedbackシングルトンを使用して音を再生
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonAudioFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public enum ClickSoundType
        {
            Default,
            Confirm,
            Cancel,
            Warning,
            None
        }

        [Header("サウンド設定")]
        [SerializeField] private ClickSoundType _clickSoundType = ClickSoundType.Default;
        [SerializeField] private bool _playHoverSound = true;
        [SerializeField] private bool _playDisabledSound = true;

        private Button? _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_playHoverSound) return;
            if (_button != null && !_button.interactable) return;

            UIAudioFeedback.Instance?.PlayButtonHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable)
            {
                if (_playDisabledSound)
                {
                    UIAudioFeedback.Instance?.PlayButtonDisabled();
                }
                return;
            }

            switch (_clickSoundType)
            {
                case ClickSoundType.Default:
                    UIAudioFeedback.Instance?.PlayButtonClick();
                    break;
                case ClickSoundType.Confirm:
                    UIAudioFeedback.Instance?.PlaySuccess();
                    break;
                case ClickSoundType.Cancel:
                    UIAudioFeedback.Instance?.PlayButtonClick();
                    break;
                case ClickSoundType.Warning:
                    UIAudioFeedback.Instance?.PlayWarning();
                    break;
                case ClickSoundType.None:
                    // 音を再生しない
                    break;
            }
        }

        /// <summary>
        /// クリック音タイプを設定
        /// </summary>
        public void SetClickSoundType(ClickSoundType type)
        {
            _clickSoundType = type;
        }
    }
}

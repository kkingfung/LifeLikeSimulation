#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Controllers;
using LifeLike.Data;
using LifeLike.Services.Core.Subscene;
using LifeLike.UI;
using LifeLike.UI.Effects;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// オペレーター画面のView
    /// 緊急通報センターのコンソールインターフェースを表示
    /// CRT効果、デジタル時計、ボタンホバー効果、タイマーバーを含む
    /// </summary>
    public class OperatorView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private OperatorSceneController? _controller;

        [Header("シナリオ設定")]
        [SerializeField] private NightScenarioData? _scenarioData;

        [Header("時計UI")]
        [SerializeField] private Text? _clockText;

        [Header("発信者情報UI")]
        [SerializeField] private GameObject? _callerInfoPanel;
        [SerializeField] private Text? _callerNameText;
        [SerializeField] private Text? _callerPhoneText;
        [SerializeField] private Image? _callerSilhouette;

        [Header("通話状態UI")]
        [SerializeField] private GameObject? _callActivePanel;
        [SerializeField] private Text? _callStatusText;
        [SerializeField] private Slider? _responseTimerSlider;

        [Header("セグメントUI")]
        [SerializeField] private Text? _callerDialogueText;
        [SerializeField] private GameObject? _dialoguePanel;

        [Header("応答選択UI")]
        [SerializeField] private GameObject? _responsePanel;
        [SerializeField] private Transform? _responseButtonContainer;
        [SerializeField] private Button? _responseButtonPrefab;
        [SerializeField] private Button? _silenceButton;

        [Header("着信リストUI")]
        [SerializeField] private GameObject? _incomingCallsPanel;
        [SerializeField] private Transform? _incomingCallContainer;
        [SerializeField] private Button? _incomingCallButtonPrefab;

        [Header("証拠パネルUI")]
        [SerializeField] private GameObject? _evidencePanel;
        [SerializeField] private Transform? _evidenceContainer;
        [SerializeField] private GameObject? _evidenceItemPrefab;

        [Header("コントロールUI")]
        [SerializeField] private Button? _holdButton;
        [SerializeField] private Button? _endCallButton;
        [SerializeField] private Button? _pauseButton;
        [SerializeField] private Text? _pauseButtonText;

        [Header("不在着信UI")]
        [SerializeField] private GameObject? _missedCallBadge;
        [SerializeField] private Text? _missedCallCountText;

        [Header("エンディングUI")]
        [SerializeField] private GameObject? _endingPanel;
        [SerializeField] private Text? _endingTitleText;
        [SerializeField] private Text? _endingDescriptionText;
        [SerializeField] private Button? _returnToMenuButton;

        [Header("UI Effects")]
        [SerializeField] private UITheme? _theme;
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private bool _enableButtonEffects = true;
        [SerializeField] private bool _enableDigitalClock = true;
        [SerializeField] private Canvas? _mainCanvas;

        private OperatorViewModel? _viewModel;
        private ISubsceneService? _subsceneService;
        private bool _wasPausedBeforeSettings;
        private readonly List<Button> _responseButtons = new();
        private readonly List<Button> _incomingCallButtons = new();
        private readonly List<GameObject> _evidenceItems = new();
        private GameObject? _crtOverlay;
        private DigitalClockDisplay? _digitalClock;
        private TimerBar? _responseTimer;
        private ShakeEffect? _missedCallShake;
        private FlashEffect? _evidencePanelFlash;
        private SlideEffect? _endingPanelSlide;
        private ScalePopEffect? _endingPanelPop;

        private void Awake()
        {
            // コントローラーを検索
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<OperatorSceneController>();
            }

            if (_controller == null)
            {
                Debug.LogError("[OperatorView] OperatorSceneControllerが見つかりません。");
                return;
            }

            // コントローラーからサービスを取得
            var callFlowService = _controller.CallFlowService;
            var worldStateService = _controller.WorldStateService;
            var evidenceService = _controller.EvidenceService;
            var trustGraphService = _controller.TrustGraphService;
            var saveService = _controller.SaveService;
            _subsceneService = _controller.SubsceneService;

            if (callFlowService == null || worldStateService == null ||
                evidenceService == null || trustGraphService == null || saveService == null)
            {
                Debug.LogError("[OperatorView] 必要なサービスがコントローラーにありません。");
                return;
            }

            // ViewModelを作成
            _viewModel = new OperatorViewModel(
                callFlowService,
                worldStateService,
                evidenceService,
                trustGraphService,
                saveService);

            // サブシーンが閉じられた時のイベントを購読
            if (_subsceneService != null)
            {
                _subsceneService.OnSubsceneClosed += OnSubsceneClosed;
            }
        }

        private void Start()
        {
            if (_viewModel == null)
            {
                return;
            }

            // テーマを設定
            SetupTheme();

            // UI効果をセットアップ
            SetupUIEffects();

            // UIをセットアップ
            SetupUI();

            // ViewModelのイベントを購読
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnScenarioEnded += OnScenarioEnded;
            _viewModel.OnReturnToMenuRequested += OnReturnToMenuRequested;

            // 初期状態を反映
            UpdateAllUI();

            // シナリオを開始
            if (_scenarioData != null)
            {
                _viewModel.StartScenario(_scenarioData);
            }
            else
            {
                Debug.LogWarning("[OperatorView] シナリオデータが設定されていません。");
            }
        }

        /// <summary>
        /// テーマを設定
        /// </summary>
        private void SetupTheme()
        {
            if (_theme != null)
            {
                UIThemeManager.Instance.Theme = _theme;
            }
        }

        /// <summary>
        /// UI効果をセットアップ
        /// </summary>
        private void SetupUIEffects()
        {
            if (_enableCRTEffect)
            {
                SetupCRTEffect();
            }

            if (_enableDigitalClock)
            {
                SetupDigitalClock();
            }

            if (_enableButtonEffects)
            {
                SetupButtonEffects();
            }

            SetupResponseTimer();

            // 追加のUI効果をセットアップ
            SetupAdditionalEffects();
        }

        /// <summary>
        /// CRT効果をセットアップ
        /// </summary>
        private void SetupCRTEffect()
        {
            var canvas = _mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogWarning("[OperatorView] Canvasが見つかりません。CRT効果をスキップします。");
                return;
            }

            _crtOverlay = new GameObject("CRTOverlay");
            _crtOverlay.transform.SetParent(canvas.transform, false);

            var rectTransform = _crtOverlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var rawImage = _crtOverlay.AddComponent<RawImage>();
            rawImage.raycastTarget = false;

            var crtEffect = _crtOverlay.AddComponent<CRTEffect>();
            crtEffect.ApplyTheme(UIThemeManager.Instance.Theme);

            _crtOverlay.transform.SetAsLastSibling();
        }

        /// <summary>
        /// デジタル時計をセットアップ
        /// </summary>
        private void SetupDigitalClock()
        {
            if (_clockText == null) return;

            _digitalClock = _clockText.GetComponent<DigitalClockDisplay>();
            if (_digitalClock == null)
            {
                _digitalClock = _clockText.gameObject.AddComponent<DigitalClockDisplay>();
            }
        }

        /// <summary>
        /// ボタン効果をセットアップ
        /// </summary>
        private void SetupButtonEffects()
        {
            var theme = UIThemeManager.Instance.Theme;

            AddButtonEffects(_holdButton, theme);
            AddButtonEffects(_endCallButton, theme, ButtonAudioFeedback.ClickSoundType.Cancel);
            AddButtonEffects(_pauseButton, theme);
            AddButtonEffects(_silenceButton, theme);
            AddButtonEffects(_returnToMenuButton, theme, ButtonAudioFeedback.ClickSoundType.Confirm);
        }

        /// <summary>
        /// ボタンにエフェクトを追加
        /// </summary>
        private void AddButtonEffects(Button? button, UITheme theme, ButtonAudioFeedback.ClickSoundType soundType = ButtonAudioFeedback.ClickSoundType.Default)
        {
            if (button == null) return;

            var hoverEffect = button.gameObject.GetComponent<ButtonHoverEffect>();
            if (hoverEffect == null)
            {
                hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
                hoverEffect.ApplyTheme(theme);
            }

            var audioFeedback = button.gameObject.GetComponent<ButtonAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = button.gameObject.AddComponent<ButtonAudioFeedback>();
                audioFeedback.SetClickSoundType(soundType);
            }
        }

        /// <summary>
        /// 応答タイマーをセットアップ
        /// </summary>
        private void SetupResponseTimer()
        {
            if (_responseTimerSlider == null) return;

            _responseTimer = _responseTimerSlider.GetComponent<TimerBar>();
            if (_responseTimer == null)
            {
                _responseTimer = _responseTimerSlider.gameObject.AddComponent<TimerBar>();
            }
        }

        /// <summary>
        /// 追加のUI効果をセットアップ
        /// </summary>
        private void SetupAdditionalEffects()
        {
            // 不在着信バッジのシェイク効果
            if (_missedCallBadge != null)
            {
                _missedCallShake = _missedCallBadge.GetComponent<ShakeEffect>();
                if (_missedCallShake == null)
                {
                    _missedCallShake = _missedCallBadge.AddComponent<ShakeEffect>();
                    _missedCallShake.SetVibratePreset();
                }
            }

            // 証拠パネルのフラッシュ効果
            if (_evidencePanel != null)
            {
                var panelImage = _evidencePanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    _evidencePanelFlash = _evidencePanel.GetComponent<FlashEffect>();
                    if (_evidencePanelFlash == null)
                    {
                        _evidencePanelFlash = _evidencePanel.AddComponent<FlashEffect>();
                        _evidencePanelFlash.SetSuccessPreset();
                    }
                }
            }

            // エンディングパネルのスライドとポップ効果
            if (_endingPanel != null)
            {
                var canvasGroup = _endingPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = _endingPanel.AddComponent<CanvasGroup>();
                }

                _endingPanelSlide = _endingPanel.GetComponent<SlideEffect>();
                if (_endingPanelSlide == null)
                {
                    _endingPanelSlide = _endingPanel.AddComponent<SlideEffect>();
                    _endingPanelSlide.SetDirection(SlideEffect.SlideDirection.Up);
                }

                _endingPanelPop = _endingPanel.GetComponent<ScalePopEffect>();
                if (_endingPanelPop == null)
                {
                    _endingPanelPop = _endingPanel.AddComponent<ScalePopEffect>();
                    _endingPanelPop.SetDialogPreset();
                }
            }
        }

        private void Update()
        {
            _viewModel?.Update(Time.deltaTime);

            // ESCキーで設定画面を開く/閉じる
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleEscapeKey();
            }
        }

        /// <summary>
        /// ESCキーが押された時の処理
        /// </summary>
        private void HandleEscapeKey()
        {
            // サブシーンが開いている場合は閉じる
            if (_subsceneService != null && _subsceneService.IsSubsceneOpen)
            {
                _subsceneService.CloseSubsceneAsync();
                return;
            }

            // 設定画面を開く
            OnSettingsClicked();
        }

        private void OnDestroy()
        {
            // CRTオーバーレイを破棄
            if (_crtOverlay != null)
            {
                Destroy(_crtOverlay);
            }

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnScenarioEnded -= OnScenarioEnded;
                _viewModel.OnReturnToMenuRequested -= OnReturnToMenuRequested;
                _viewModel.Dispose();
            }

            if (_subsceneService != null)
            {
                _subsceneService.OnSubsceneClosed -= OnSubsceneClosed;
            }

            ClearResponseButtons();
            ClearIncomingCallButtons();
            ClearEvidenceItems();
        }

        /// <summary>
        /// UIをセットアップ
        /// </summary>
        private void SetupUI()
        {
            // コントロールボタン
            if (_holdButton != null)
            {
                _holdButton.onClick.AddListener(OnHoldClicked);
            }

            if (_endCallButton != null)
            {
                _endCallButton.onClick.AddListener(OnEndCallClicked);
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (_silenceButton != null)
            {
                _silenceButton.onClick.AddListener(OnSilenceClicked);
            }

            if (_returnToMenuButton != null)
            {
                _returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
            }

            // 初期状態でエンディングパネルを非表示
            if (_endingPanel != null)
            {
                _endingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 全UIを更新
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateClockUI();
            UpdateCallerInfoUI();
            UpdateCallStatusUI();
            UpdateDialogueUI();
            UpdateResponsesUI();
            UpdateIncomingCallsUI();
            UpdateEvidenceUI();
            UpdateMissedCallsUI();
            UpdateControlButtonsUI();
        }

        /// <summary>
        /// ViewModelのプロパティ変更時
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(OperatorViewModel.CurrentTime):
                    UpdateClockUI();
                    break;

                case nameof(OperatorViewModel.CurrentCaller):
                case nameof(OperatorViewModel.IsCallActive):
                    UpdateCallerInfoUI();
                    UpdateCallStatusUI();
                    UpdateControlButtonsUI();
                    break;

                case nameof(OperatorViewModel.CurrentSegment):
                    UpdateDialogueUI();
                    break;

                case nameof(OperatorViewModel.AvailableResponses):
                case nameof(OperatorViewModel.IsShowingResponses):
                    UpdateResponsesUI();
                    break;

                case nameof(OperatorViewModel.ResponseTimeRemaining):
                    UpdateResponseTimerUI();
                    break;

                case nameof(OperatorViewModel.IncomingCalls):
                    UpdateIncomingCallsUI();
                    break;

                case nameof(OperatorViewModel.DiscoveredEvidence):
                    UpdateEvidenceUI();
                    break;

                case nameof(OperatorViewModel.MissedCallCount):
                    UpdateMissedCallsUI();
                    break;

                case nameof(OperatorViewModel.IsPaused):
                    UpdatePauseButtonUI();
                    break;
            }
        }

        #region UI更新メソッド

        private void UpdateClockUI()
        {
            if (_viewModel == null) return;

            // デジタル時計が有効な場合はそれを使用
            if (_digitalClock != null)
            {
                _digitalClock.SetTimeString(_viewModel.CurrentTime);
            }
            else if (_clockText != null)
            {
                _clockText.text = _viewModel.CurrentTime;
            }
        }

        private void UpdateCallerInfoUI()
        {
            if (_viewModel == null) return;

            bool hasActiveCaller = _viewModel.IsCallActive && _viewModel.CurrentCaller != null;

            if (_callerInfoPanel != null)
            {
                _callerInfoPanel.SetActive(hasActiveCaller);
            }

            if (hasActiveCaller && _viewModel.CurrentCaller != null)
            {
                if (_callerNameText != null)
                {
                    _callerNameText.text = _viewModel.CurrentCaller.displayName;
                }

                if (_callerPhoneText != null)
                {
                    _callerPhoneText.text = _viewModel.CurrentCaller.phoneNumber;
                }
            }
        }

        private void UpdateCallStatusUI()
        {
            if (_viewModel == null) return;

            if (_callActivePanel != null)
            {
                _callActivePanel.SetActive(_viewModel.IsCallActive);
            }

            if (_callStatusText != null)
            {
                if (_viewModel.IsCallActive)
                {
                    _callStatusText.text = "通話中";
                }
                else if (_viewModel.IncomingCalls.Count > 0)
                {
                    _callStatusText.text = "着信あり";
                }
                else
                {
                    _callStatusText.text = "待機中";
                }
            }
        }

        private void UpdateDialogueUI()
        {
            if (_viewModel == null) return;

            bool hasDialogue = _viewModel.CurrentSegment?.media != null;

            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(hasDialogue);
            }

            if (hasDialogue && _callerDialogueText != null && _viewModel.CurrentSegment?.media != null)
            {
                _callerDialogueText.text = _viewModel.CurrentSegment.media.dialogueText.ToString();
            }
        }

        private void UpdateResponsesUI()
        {
            if (_viewModel == null) return;

            bool showResponses = _viewModel.IsShowingResponses && _viewModel.AvailableResponses.Count > 0;

            if (_responsePanel != null)
            {
                _responsePanel.SetActive(showResponses);
            }

            // 応答ボタンを再生成
            ClearResponseButtons();

            if (showResponses && _responseButtonContainer != null && _responseButtonPrefab != null)
            {
                var theme = UIThemeManager.Instance.Theme;

                foreach (var response in _viewModel.AvailableResponses)
                {
                    var button = Instantiate(_responseButtonPrefab, _responseButtonContainer);
                    button.gameObject.SetActive(true);

                    var buttonText = button.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = response.displayText.ToString();
                    }

                    // クロージャーで正しいresponseIdをキャプチャ
                    string responseId = response.responseId;
                    button.onClick.AddListener(() => OnResponseClicked(responseId));

                    // ボタンにホバー効果とオーディオフィードバックを追加
                    if (_enableButtonEffects)
                    {
                        AddButtonEffects(button, theme, ButtonAudioFeedback.ClickSoundType.Confirm);

                        // 応答ボタンにウォブル効果を追加（クリック時）
                        var wobbleEffect = button.gameObject.GetComponent<WobbleEffect>();
                        if (wobbleEffect == null)
                        {
                            wobbleEffect = button.gameObject.AddComponent<WobbleEffect>();
                            wobbleEffect.SetButtonClickPreset();
                        }

                        // ボタンにポップイン効果を追加
                        var popEffect = button.gameObject.GetComponent<ScalePopEffect>();
                        if (popEffect == null)
                        {
                            popEffect = button.gameObject.AddComponent<ScalePopEffect>();
                            popEffect.SetNotificationPreset();
                        }
                        popEffect.PopIn(0.15f);
                    }

                    _responseButtons.Add(button);
                }
            }

            // 沈黙ボタンの状態
            if (_silenceButton != null)
            {
                _silenceButton.gameObject.SetActive(showResponses);
            }
        }

        private void UpdateResponseTimerUI()
        {
            if (_viewModel == null || _responseTimerSlider == null) return;

            if (_viewModel.IsShowingResponses && _viewModel.CurrentSegment?.responseTimeLimit > 0)
            {
                _responseTimerSlider.gameObject.SetActive(true);
                _responseTimerSlider.maxValue = _viewModel.CurrentSegment.responseTimeLimit;
                _responseTimerSlider.value = _viewModel.ResponseTimeRemaining;
            }
            else
            {
                _responseTimerSlider.gameObject.SetActive(false);
            }
        }

        private void UpdateIncomingCallsUI()
        {
            if (_viewModel == null) return;

            bool hasIncomingCalls = _viewModel.IncomingCalls.Count > 0;

            if (_incomingCallsPanel != null)
            {
                _incomingCallsPanel.SetActive(hasIncomingCalls);
            }

            // 着信ボタンを再生成
            ClearIncomingCallButtons();

            if (hasIncomingCalls && _incomingCallContainer != null && _incomingCallButtonPrefab != null)
            {
                var theme = UIThemeManager.Instance.Theme;

                foreach (var call in _viewModel.IncomingCalls)
                {
                    var button = Instantiate(_incomingCallButtonPrefab, _incomingCallContainer);
                    button.gameObject.SetActive(true);

                    var buttonText = button.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = call.caller?.displayName ?? "不明な発信者";
                    }

                    string callId = call.callId;
                    button.onClick.AddListener(() => OnAnswerCallClicked(callId));

                    // ボタンにホバー効果とオーディオフィードバックを追加
                    if (_enableButtonEffects)
                    {
                        AddButtonEffects(button, theme, ButtonAudioFeedback.ClickSoundType.Confirm);

                        // 着信ボタンにパルス効果を追加
                        var pulseEffect = button.gameObject.GetComponent<PulseEffect>();
                        if (pulseEffect == null)
                        {
                            pulseEffect = button.gameObject.AddComponent<PulseEffect>();
                            pulseEffect.SetIncomingCallPreset();
                            pulseEffect.StartPulse();
                        }
                    }

                    _incomingCallButtons.Add(button);
                }
            }
        }

        private int _previousEvidenceCount = 0;

        private void UpdateEvidenceUI()
        {
            if (_viewModel == null) return;

            bool hasEvidence = _viewModel.DiscoveredEvidence.Count > 0;

            if (_evidencePanel != null)
            {
                _evidencePanel.SetActive(hasEvidence);

                // 新しい証拠が追加されたらフラッシュ
                if (_viewModel.DiscoveredEvidence.Count > _previousEvidenceCount && _previousEvidenceCount > 0)
                {
                    _evidencePanelFlash?.FlashCyan();
                }
            }

            _previousEvidenceCount = _viewModel.DiscoveredEvidence.Count;

            // 証拠アイテムを再生成
            ClearEvidenceItems();

            if (hasEvidence && _evidenceContainer != null && _evidenceItemPrefab != null)
            {
                foreach (var evidence in _viewModel.DiscoveredEvidence)
                {
                    var item = Instantiate(_evidenceItemPrefab, _evidenceContainer);
                    item.SetActive(true);

                    var itemText = item.GetComponentInChildren<Text>();
                    if (itemText != null)
                    {
                        itemText.text = evidence.content.ToString();
                    }

                    _evidenceItems.Add(item);
                }
            }
        }

        private int _previousMissedCallCount = 0;

        private void UpdateMissedCallsUI()
        {
            if (_viewModel == null) return;

            bool hasMissedCalls = _viewModel.MissedCallCount > 0;

            if (_missedCallBadge != null)
            {
                _missedCallBadge.SetActive(hasMissedCalls);

                // 新しい不在着信があればシェイク
                if (_viewModel.MissedCallCount > _previousMissedCallCount)
                {
                    _missedCallShake?.Shake();
                }
            }

            if (_missedCallCountText != null)
            {
                _missedCallCountText.text = _viewModel.MissedCallCount.ToString();
            }

            _previousMissedCallCount = _viewModel.MissedCallCount;
        }

        private void UpdateControlButtonsUI()
        {
            if (_viewModel == null) return;

            if (_holdButton != null)
            {
                _holdButton.interactable = _viewModel.IsCallActive;
            }

            if (_endCallButton != null)
            {
                _endCallButton.interactable = _viewModel.IsCallActive;
            }
        }

        private void UpdatePauseButtonUI()
        {
            if (_viewModel == null) return;

            if (_pauseButtonText != null)
            {
                _pauseButtonText.text = _viewModel.IsPaused ? "再開" : "一時停止";
            }
        }

        #endregion

        #region クリーンアップ

        private void ClearResponseButtons()
        {
            foreach (var button in _responseButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _responseButtons.Clear();
        }

        private void ClearIncomingCallButtons()
        {
            foreach (var button in _incomingCallButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _incomingCallButtons.Clear();
        }

        private void ClearEvidenceItems()
        {
            foreach (var item in _evidenceItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            _evidenceItems.Clear();
        }

        #endregion

        #region イベントハンドラ

        private void OnResponseClicked(string responseId)
        {
            _viewModel?.SelectResponseCommand.Execute(responseId);
        }

        private void OnAnswerCallClicked(string callId)
        {
            _viewModel?.AnswerCallCommand.Execute(callId);
        }

        private void OnSilenceClicked()
        {
            _viewModel?.SelectSilenceCommand.Execute(null);
        }

        private void OnHoldClicked()
        {
            _viewModel?.HoldCallCommand.Execute(null);
        }

        private void OnEndCallClicked()
        {
            _viewModel?.EndCallCommand.Execute(null);
        }

        private void OnPauseClicked()
        {
            _viewModel?.TogglePauseCommand.Execute(null);
        }

        private void OnReturnToMenuClicked()
        {
            _viewModel?.ReturnToMenu();
        }

        private void OnScenarioEnded(ScenarioEnding ending)
        {
            // エンディング画面を表示（アニメーション付き）
            if (_endingPanel != null)
            {
                _endingPanel.SetActive(true);

                // ポップ効果でエンディングパネルを表示
                if (_endingPanelPop != null)
                {
                    _endingPanelPop.PopIn();
                }
                else if (_endingPanelSlide != null)
                {
                    _endingPanelSlide.SlideIn();
                }
            }

            if (_endingTitleText != null)
            {
                _endingTitleText.text = ending.title;

                // タイトルにタイプライター効果を追加
                var typewriter = _endingTitleText.GetComponent<TypewriterEffect>();
                if (typewriter == null)
                {
                    typewriter = _endingTitleText.gameObject.AddComponent<TypewriterEffect>();
                }
                typewriter.StartTyping(ending.title);
            }

            if (_endingDescriptionText != null)
            {
                _endingDescriptionText.text = ending.description;
            }

            // 他のUIを非表示
            if (_responsePanel != null) _responsePanel.SetActive(false);
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
            if (_incomingCallsPanel != null) _incomingCallsPanel.SetActive(false);
        }

        private void OnReturnToMenuRequested()
        {
            _controller?.ReturnToMainMenu();
        }

        /// <summary>
        /// 設定ボタンクリック時
        /// </summary>
        private async void OnSettingsClicked()
        {
            if (_subsceneService == null || _controller == null)
            {
                Debug.LogWarning("[OperatorView] SubsceneServiceまたはControllerが利用できません。");
                return;
            }

            // 現在のポーズ状態を保存
            _wasPausedBeforeSettings = _viewModel?.IsPaused ?? false;

            // ポーズしていない場合はポーズする
            if (!_wasPausedBeforeSettings)
            {
                _viewModel?.TogglePauseCommand.Execute(null);
            }

            // 設定画面をサブシーンとして開く
            await _subsceneService.OpenSubsceneAsync(_controller.SettingsScene);
        }

        /// <summary>
        /// サブシーンが閉じられた時
        /// </summary>
        private void OnSubsceneClosed(string sceneName)
        {
            if (_controller == null || sceneName != _controller.SettingsScene.SceneName) return;

            // 設定画面を開く前にポーズしていなかった場合は再開
            if (!_wasPausedBeforeSettings && _viewModel != null && _viewModel.IsPaused)
            {
                _viewModel.TogglePauseCommand.Execute(null);
            }

            Debug.Log("[OperatorView] 設定画面から復帰");
        }

        #endregion
    }
}

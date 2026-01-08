#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.CallFlow;
using LifeLike.Services.Clock;
using LifeLike.Services.EndState;
using LifeLike.Services.Evidence;
using LifeLike.Services.Flag;
using LifeLike.Services.Save;
using LifeLike.Services.TrustGraph;
using LifeLike.Services.WorldState;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// オペレーター画面のView
    /// 緊急通報センターのコンソールインターフェースを表示
    /// </summary>
    public class OperatorView : MonoBehaviour
    {
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

        [Header("シーン設定")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private OperatorViewModel? _viewModel;
        private readonly List<Button> _responseButtons = new();
        private readonly List<Button> _incomingCallButtons = new();
        private readonly List<GameObject> _evidenceItems = new();

        private void Awake()
        {
            // サービスを取得
            var callFlowService = ServiceLocator.Instance.Get<ICallFlowService>();
            var worldStateService = ServiceLocator.Instance.Get<IWorldStateService>();
            var evidenceService = ServiceLocator.Instance.Get<IEvidenceService>();
            var trustGraphService = ServiceLocator.Instance.Get<ITrustGraphService>();
            var saveService = ServiceLocator.Instance.Get<ISaveService>();

            if (callFlowService == null || worldStateService == null ||
                evidenceService == null || trustGraphService == null || saveService == null)
            {
                Debug.LogError("[OperatorView] 必要なサービスがありません。");
                return;
            }

            // ViewModelを作成
            _viewModel = new OperatorViewModel(
                callFlowService,
                worldStateService,
                evidenceService,
                trustGraphService,
                saveService);
        }

        private void Start()
        {
            if (_viewModel == null)
            {
                return;
            }

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

        private void Update()
        {
            _viewModel?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnScenarioEnded -= OnScenarioEnded;
                _viewModel.OnReturnToMenuRequested -= OnReturnToMenuRequested;
                _viewModel.Dispose();
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
            if (_clockText != null && _viewModel != null)
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

                    _incomingCallButtons.Add(button);
                }
            }
        }

        private void UpdateEvidenceUI()
        {
            if (_viewModel == null) return;

            bool hasEvidence = _viewModel.DiscoveredEvidence.Count > 0;

            if (_evidencePanel != null)
            {
                _evidencePanel.SetActive(hasEvidence);
            }

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

        private void UpdateMissedCallsUI()
        {
            if (_viewModel == null) return;

            bool hasMissedCalls = _viewModel.MissedCallCount > 0;

            if (_missedCallBadge != null)
            {
                _missedCallBadge.SetActive(hasMissedCalls);
            }

            if (_missedCallCountText != null)
            {
                _missedCallCountText.text = _viewModel.MissedCallCount.ToString();
            }
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
            // エンディング画面を表示
            if (_endingPanel != null)
            {
                _endingPanel.SetActive(true);
            }

            if (_endingTitleText != null)
            {
                _endingTitleText.text = ending.title;
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
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        #endregion
    }
}

#nullable enable
using System.Collections.Generic;
using LifeLike.Core.Scene;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.UI.Debug;
using LifeLike.Services.Core.Localization;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Operator.CallFlow;
using LifeLike.Services.Operator.EndState;
using LifeLike.Services.Operator.Evidence;
using LifeLike.Services.Operator.Flag;
using LifeLike.Services.Operator.TrustGraph;
using LifeLike.Services.Operator.WorldState;
using LifeLike.Services.Core.Subscene;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// オペレーターシーンのコントローラー
    /// 夜の進行を管理し、各サービスを初期化する
    /// </summary>
    public class OperatorSceneController : SceneControllerBase
    {
        [Header("Night Data")]
        [SerializeField] private List<NightDataSet> _nightDataSets = new();

        [Header("Scene Settings")]
        [SerializeField] private SceneReference _mainMenuScene = new();
        [SerializeField] private SceneReference _resultScene = new();
        [SerializeField] private SceneReference _chapterSelectScene = new();
        [SerializeField] private SceneReference _settingsScene = new();

        [Header("Debug")]
        [SerializeField] private DebugPanel? _debugPanel;

        /// <summary>
        /// 夜ごとのデータセット
        /// </summary>
        [System.Serializable]
        public class NightDataSet
        {
            public string nightId = string.Empty;
            public NightScenarioData? scenarioData;
            public NightFlagsDefinition? flagsDefinition;
            public EndStateDefinition? endStateDefinition;
        }

        // サービス参照
        private IFlagService? _flagService;
        private IEndStateService? _endStateService;
        private IOperatorSaveService? _operatorSaveService;
        private ISaveService? _saveService;
        private ICallFlowService? _callFlowService;
        private IWorldStateService? _worldStateService;
        private IEvidenceService? _evidenceService;
        private ITrustGraphService? _trustGraphService;
        private ISubsceneService? _subsceneService;
        private IDialogueLocalizationService? _dialogueLocalizationService;

        /// <summary>
        /// CallFlowサービス
        /// </summary>
        public ICallFlowService? CallFlowService => _callFlowService;

        /// <summary>
        /// WorldStateサービス
        /// </summary>
        public IWorldStateService? WorldStateService => _worldStateService;

        /// <summary>
        /// Evidenceサービス
        /// </summary>
        public IEvidenceService? EvidenceService => _evidenceService;

        /// <summary>
        /// TrustGraphサービス
        /// </summary>
        public ITrustGraphService? TrustGraphService => _trustGraphService;

        /// <summary>
        /// Operator Saveサービス（夜進行用）
        /// </summary>
        public IOperatorSaveService? OperatorSaveService => _operatorSaveService;

        /// <summary>
        /// Flagサービス
        /// </summary>
        public IFlagService? FlagService => _flagService;

        /// <summary>
        /// Saveサービス（汎用）
        /// </summary>
        public ISaveService? SaveService => _saveService;

        /// <summary>
        /// Subsceneサービス
        /// </summary>
        public ISubsceneService? SubsceneService => _subsceneService;

        /// <summary>
        /// ダイアログローカライズサービス
        /// </summary>
        public IDialogueLocalizationService? DialogueLocalizationService => _dialogueLocalizationService;

        /// <summary>
        /// 設定シーンへの参照
        /// </summary>
        public SceneReference SettingsScene => _settingsScene;

        // 現在の状態
        private int _currentNightIndex = 0;
        private NightDataSet? _currentNightData;
        private bool _isNightActive = false;

        /// <summary>
        /// 現在の夜インデックス
        /// </summary>
        public int CurrentNightIndex => _currentNightIndex;

        /// <summary>
        /// 現在の夜ID
        /// </summary>
        public string CurrentNightId => _currentNightData?.nightId ?? string.Empty;

        /// <summary>
        /// 現在の夜のシナリオデータ
        /// </summary>
        public NightScenarioData? CurrentScenarioData => _currentNightData?.scenarioData;

        /// <summary>
        /// 夜がアクティブかどうか
        /// </summary>
        public bool IsNightActive => _isNightActive;

        private void Awake()
        {
            // サービスを取得
            _flagService = GetService<IFlagService>();
            _endStateService = GetService<IEndStateService>();
            _operatorSaveService = GetService<IOperatorSaveService>();
            _saveService = GetService<ISaveService>();
            _callFlowService = GetService<ICallFlowService>();
            _worldStateService = GetService<IWorldStateService>();
            _evidenceService = GetService<IEvidenceService>();
            _trustGraphService = GetService<ITrustGraphService>();
            _subsceneService = GetService<ISubsceneService>();
            _dialogueLocalizationService = GetService<IDialogueLocalizationService>();

            // デバッグパネルのイベントを購読
            if (_debugPanel != null)
            {
                _debugPanel.OnNightJumpRequested += OnDebugNightJump;
            }
        }

        private void Start()
        {
            // メインメニューから渡された夜インデックスを取得
            int startNightIndex = PlayerPrefs.GetInt("LifeLike_StartNightIndex", 0);
            PlayerPrefs.DeleteKey("LifeLike_StartNightIndex");

            // セーブデータから夜インデックスを取得（優先）
            if (_operatorSaveService != null && _operatorSaveService.HasSaveData)
            {
                startNightIndex = _operatorSaveService.GetCurrentNightIndex();
            }

            // 夜を開始
            StartNight(startNightIndex);
        }

        private void OnDestroy()
        {
            if (_debugPanel != null)
            {
                _debugPanel.OnNightJumpRequested -= OnDebugNightJump;
            }

            // WorldStateServiceのイベントを解除
            if (_worldStateService != null)
            {
                _worldStateService.OnScenarioEnded -= OnScenarioEnded;
            }
        }

        /// <summary>
        /// 指定した夜を開始
        /// </summary>
        public void StartNight(int nightIndex)
        {
            if (nightIndex < 0 || nightIndex >= _nightDataSets.Count)
            {
                Debug.LogError($"[OperatorSceneController] 無効な夜インデックス: {nightIndex}");
                return;
            }

            _currentNightIndex = nightIndex;
            _currentNightData = _nightDataSets[nightIndex];

            Debug.Log($"[OperatorSceneController] 夜を開始: {_currentNightData.nightId}");

            // ダイアログ翻訳データを読み込み
            if (_dialogueLocalizationService != null)
            {
                _dialogueLocalizationService.LoadNightTranslation(_currentNightData.nightId);
            }

            // フラグサービスを初期化
            if (_flagService != null && _currentNightData.flagsDefinition != null)
            {
                _flagService.Initialize(_currentNightData.nightId, _currentNightData.flagsDefinition);

                // 前の夜からの永続フラグを復元
                if (_operatorSaveService != null)
                {
                    var persistentFlags = _operatorSaveService.GetPersistentFlags();
                    foreach (var flag in persistentFlags)
                    {
                        _flagService.SetFlag(flag.flagId, flag.setTime);
                    }
                }
            }

            // エンドステートサービスを初期化
            if (_endStateService != null && _currentNightData.endStateDefinition != null)
            {
                _endStateService.Initialize(_currentNightData.endStateDefinition);
            }

            // WorldStateServiceを初期化
            if (_worldStateService != null && _currentNightData.scenarioData != null)
            {
                _worldStateService.LoadScenario(_currentNightData.scenarioData);
                _worldStateService.OnScenarioEnded -= OnScenarioEnded;
                _worldStateService.OnScenarioEnded += OnScenarioEnded;
            }

            // CallFlowServiceを初期化
            if (_callFlowService != null && _currentNightData.scenarioData != null)
            {
                _callFlowService.LoadScenario(_currentNightData.scenarioData);

                // シミュレーションモード: 最初の通話をトリガー
                _callFlowService.TriggerNextCall();
            }

            _isNightActive = true;
        }

        /// <summary>
        /// 現在の夜を終了
        /// </summary>
        public void EndCurrentNight()
        {
            if (!_isNightActive || _currentNightData == null)
            {
                return;
            }

            _isNightActive = false;

            // エンドステートを決定
            var endState = _endStateService?.CalculateEndState() ?? EndStateType.UncertainDawn;
            Debug.Log($"[OperatorSceneController] 夜終了: {CurrentNightId}, EndState: {endState}");

            // セーブ
            SaveCurrentState(endState);

            // 次の夜へ、または全夜終了
            if (_currentNightIndex < _nightDataSets.Count - 1)
            {
                _currentNightIndex++;
                StartNight(_currentNightIndex);
            }
            else
            {
                Debug.Log("[OperatorSceneController] 全ての夜が終了しました。");
                OnAllNightsCompleted();
            }
        }

        /// <summary>
        /// 現在の状態をセーブ
        /// </summary>
        private void SaveCurrentState(EndStateType endState)
        {
            if (_operatorSaveService == null || _flagService == null) return;

            var persistentFlags = _flagService.GetPersistentFlags();
            _operatorSaveService.SaveNightResult(CurrentNightId, endState, persistentFlags);
        }

        /// <summary>
        /// 中断セーブを作成
        /// </summary>
        public void SaveMidNight()
        {
            if (_operatorSaveService == null || _flagService == null) return;

            var currentFlags = _flagService.GetAllFlags();
            _operatorSaveService.SaveMidNight(CurrentNightId, 0, currentFlags);
        }

        /// <summary>
        /// シナリオ終了時のコールバック
        /// </summary>
        private void OnScenarioEnded(ScenarioEnding ending)
        {
            Debug.Log($"[OperatorSceneController] シナリオ終了: {ending.title}");
            EndCurrentNight();
        }

        /// <summary>
        /// 全ての夜が終了した時の処理
        /// </summary>
        private void OnAllNightsCompleted()
        {
            // 結果画面へ遷移
            NavigateTo(_resultScene);
        }

        /// <summary>
        /// メインメニューに戻る
        /// </summary>
        public void ReturnToMainMenu()
        {
            // 中断セーブを作成
            if (_isNightActive)
            {
                SaveMidNight();
            }

            NavigateTo(_mainMenuScene);
        }

        /// <summary>
        /// チャプター選択画面に戻る
        /// </summary>
        public void ReturnToChapterSelect()
        {
            // 中断セーブを作成
            if (_isNightActive)
            {
                SaveMidNight();
            }

            NavigateTo(_chapterSelectScene);
        }

        /// <summary>
        /// デバッグ用：指定した夜にジャンプ
        /// </summary>
        private void OnDebugNightJump(int nightIndex)
        {
            Debug.Log($"[OperatorSceneController] デバッグジャンプ: Night {nightIndex + 1}");
            StartNight(nightIndex);
        }
    }
}

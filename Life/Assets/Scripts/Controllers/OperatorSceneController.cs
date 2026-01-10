#nullable enable
using System.Collections.Generic;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.UI.Debug;
using LifeLike.Services.CallFlow;
using LifeLike.Services.Clock;
using LifeLike.Services.EndState;
using LifeLike.Services.Evidence;
using LifeLike.Services.Flag;
using LifeLike.Services.Save;
using LifeLike.Services.TrustGraph;
using LifeLike.Services.WorldState;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LifeLike.Controllers
{
    /// <summary>
    /// オペレーターシーンのコントローラー
    /// NightControllerとOperatorViewを橋渡しし、夜の進行を管理
    /// </summary>
    public class OperatorSceneController : MonoBehaviour
    {
        [Header("Night Data")]
        [SerializeField] private List<NightDataSet> _nightDataSets = new();

        [Header("Scene Settings")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private string _resultSceneName = "Result";
        [SerializeField] private string _chapterSelectSceneName = "ChapterSelect";

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
        private IClockService? _clockService;
        private IEndStateService? _endStateService;
        private IOperatorSaveService? _operatorSaveService;
        private ICallFlowService? _callFlowService;
        private IWorldStateService? _worldStateService;
        private IEvidenceService? _evidenceService;
        private ITrustGraphService? _trustGraphService;

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
            _flagService = ServiceLocator.Instance.Get<IFlagService>();
            _clockService = ServiceLocator.Instance.Get<IClockService>();
            _endStateService = ServiceLocator.Instance.Get<IEndStateService>();
            _operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();
            _callFlowService = ServiceLocator.Instance.Get<ICallFlowService>();
            _worldStateService = ServiceLocator.Instance.Get<IWorldStateService>();
            _evidenceService = ServiceLocator.Instance.Get<IEvidenceService>();
            _trustGraphService = ServiceLocator.Instance.Get<ITrustGraphService>();

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

            // 時計サービスを初期化
            if (_clockService != null && _currentNightData.scenarioData != null)
            {
                _clockService.Initialize(
                    _currentNightData.scenarioData.startTimeMinutes,
                    _currentNightData.scenarioData.endTimeMinutes,
                    _currentNightData.scenarioData.realSecondsPerGameMinute
                );
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
            if (_operatorSaveService == null || _flagService == null || _clockService == null) return;

            var currentFlags = _flagService.GetAllFlags();
            _operatorSaveService.SaveMidNight(CurrentNightId, _clockService.CurrentTimeMinutes, currentFlags);
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
            SceneManager.LoadScene(_resultSceneName);
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

            SceneManager.LoadScene(_mainMenuSceneName);
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

            SceneManager.LoadScene(_chapterSelectSceneName);
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

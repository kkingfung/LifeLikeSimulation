#nullable enable
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Services.Clock;
using LifeLike.Services.EndState;
using LifeLike.Services.Flag;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// Night01シナリオのシーンコントローラー
    /// シナリオ開始時にサービスを初期化し、必要なデータを読み込む
    /// </summary>
    public class Night01SceneController : MonoBehaviour
    {
        [Header("シナリオデータ")]
        [SerializeField] private NightScenarioData? _scenarioData;

        [Header("フラグ定義")]
        [SerializeField] private NightFlagsDefinition? _flagsDefinition;

        [Header("エンドステート定義")]
        [SerializeField] private EndStateDefinition? _endStateDefinition;

        [Header("時計設定")]
        [Tooltip("シナリオ開始時刻（分単位、例：137 = 02:17）")]
        [SerializeField] private int _startTimeMinutes = 137;

        [Tooltip("シナリオ終了時刻（分単位、例：360 = 06:00）")]
        [SerializeField] private int _endTimeMinutes = 360;

        [Tooltip("ゲーム内1分あたりの実時間（秒）")]
        [SerializeField] private float _realSecondsPerGameMinute = 2f;

        private void Awake()
        {
            InitializeServices();
        }

        /// <summary>
        /// サービスを初期化
        /// </summary>
        private void InitializeServices()
        {
            // FlagServiceを初期化
            var flagService = ServiceLocator.Instance.Get<IFlagService>();
            if (flagService != null && _flagsDefinition != null)
            {
                flagService.Initialize(_flagsDefinition.nightId, _flagsDefinition);
                Debug.Log($"[Night01SceneController] FlagService初期化: {_flagsDefinition.nightId}");
            }
            else
            {
                Debug.LogWarning("[Night01SceneController] FlagServiceまたはFlagsDefinitionがありません。");
            }

            // EndStateServiceを初期化
            var endStateService = ServiceLocator.Instance.Get<IEndStateService>();
            if (endStateService != null && _endStateDefinition != null)
            {
                endStateService.Initialize(_endStateDefinition);
                Debug.Log($"[Night01SceneController] EndStateService初期化: {_endStateDefinition.nightId}");
            }
            else
            {
                Debug.LogWarning("[Night01SceneController] EndStateServiceまたはEndStateDefinitionがありません。");
            }

            // ClockServiceを初期化
            var clockService = ServiceLocator.Instance.Get<IClockService>();
            if (clockService != null)
            {
                clockService.Initialize(_startTimeMinutes, _endTimeMinutes, _realSecondsPerGameMinute);
                clockService.Start();
                Debug.Log($"[Night01SceneController] ClockService初期化: {clockService.FormattedTime}");
            }
            else
            {
                Debug.LogWarning("[Night01SceneController] ClockServiceがありません。");
            }

            Debug.Log("[Night01SceneController] サービス初期化完了");
        }

        private void OnDestroy()
        {
            // ClockServiceの時間進行を停止
            var clockService = ServiceLocator.Instance.Get<IClockService>();
            clockService?.Stop();
        }

#if UNITY_EDITOR
        /// <summary>
        /// エディタ用：アセット参照の検証
        /// </summary>
        private void OnValidate()
        {
            if (_scenarioData == null)
            {
                Debug.LogWarning("[Night01SceneController] ScenarioDataが設定されていません。");
            }

            if (_flagsDefinition == null)
            {
                Debug.LogWarning("[Night01SceneController] FlagsDefinitionが設定されていません。");
            }

            if (_endStateDefinition == null)
            {
                Debug.LogWarning("[Night01SceneController] EndStateDefinitionが設定されていません。");
            }
        }
#endif
    }
}

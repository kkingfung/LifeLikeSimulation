#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Core.Services;
using LifeLike.Data;
// Core Services
using LifeLike.Services.Core.AssetBundle;
using LifeLike.Services.Core.Audio;
using LifeLike.Services.Core.Localization;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Core.Story;
using LifeLike.Services.Core.Subscene;
using LifeLike.Services.Core.Transition;
using LifeLike.Services.Core.Video;
// Operator Services
using LifeLike.Services.Operator.CallFlow;
using LifeLike.Services.Operator.Choice;
using LifeLike.Services.Operator.Clock;
using LifeLike.Services.Operator.EndState;
using LifeLike.Services.Operator.Evidence;
using LifeLike.Services.Operator.Flag;
using LifeLike.Services.Operator.TrustGraph;
using LifeLike.Services.Operator.WorldState;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Core
{
    /// <summary>
    /// ゲームの初期化を行うBootstrapクラス
    /// Bootstrapシーンに配置し、最初に実行される
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Transition UI")]
        [SerializeField] private CanvasGroup? _fadeCanvasGroup;
        [SerializeField] private Image? _fadeImage;

        [Header("Audio Settings")]
        [SerializeField] private UnityEngine.Audio.AudioMixer? _audioMixer;
        [SerializeField] private AudioSource? _bgmSource;
        [SerializeField] private AudioSource? _sfxSource;
        [SerializeField] private AudioSource? _voiceSource;

        [Header("AssetBundle Settings")]
        [Tooltip("AssetBundleのダウンロード元ベースURL（空の場合はStreamingAssetsを使用）")]
        [SerializeField] private string _assetBundleBaseUrl = string.Empty;

        [Header("Operator Mode Settings")]
        [Tooltip("初期信頼グラフデータ")]
        [SerializeField] private TrustGraphData? _trustGraphData;

        [Header("Settings")]
        [SerializeField] private SceneReference _mainMenuScene = new();
        [SerializeField] private bool _loadMainMenuOnStart = true;

        private static bool _isInitialized = false;

        private void Awake()
        {
            // 重複初期化を防ぐ
            if (_isInitialized)
            {
                Debug.Log("[GameBootstrap] 既に初期化されています。");
                Destroy(gameObject);
                return;
            }

            // このオブジェクトをシーン間で維持
            DontDestroyOnLoad(gameObject);

            // サービスを初期化
            InitializeServices();

            _isInitialized = true;
            Debug.Log("[GameBootstrap] 初期化完了");
        }

        private void Start()
        {
            if (_loadMainMenuOnStart && _mainMenuScene.IsValid)
            {
                _mainMenuScene.LoadScene();
            }
        }

        /// <summary>
        /// サービスを初期化して登録する
        /// </summary>
        private void InitializeServices()
        {
            // ServiceLocatorをリセット
            ServiceLocator.ResetInstance();

            // LocalizationServiceを最初に作成・登録（他のサービスから参照される可能性があるため）
            var localizationService = new LocalizationService();
            ServiceLocator.Instance.Register<ILocalizationService>(localizationService);

            // SubsceneServiceを作成・登録（サブシーン管理用）
            var subsceneService = new SubsceneService();
            ServiceLocator.Instance.Register<ISubsceneService>(subsceneService);

            // AssetBundleServiceを作成・登録
            var assetBundleService = new AssetBundleService();
            if (!string.IsNullOrEmpty(_assetBundleBaseUrl))
            {
                assetBundleService.BaseUrl = _assetBundleBaseUrl;
            }
            ServiceLocator.Instance.Register<IAssetBundleService>(assetBundleService);

            // StoryServiceを作成・登録
            var storyService = new StoryService();
            ServiceLocator.Instance.Register<IStoryService>(storyService);

            // VideoServiceを作成・登録
            var videoService = new VideoService();
            ServiceLocator.Instance.Register<IVideoService>(videoService);

            // ChoiceServiceを作成・登録（StoryServiceに依存）
            var choiceService = new ChoiceService(storyService);
            ServiceLocator.Instance.Register<IChoiceService>(choiceService);

            // SaveServiceを作成・登録（StoryServiceに依存）
            var saveService = new SaveService(storyService);
            ServiceLocator.Instance.Register<ISaveService>(saveService);

            // TransitionServiceを作成・登録
            var transitionService = new TransitionService();
            if (_fadeCanvasGroup != null && _fadeImage != null)
            {
                transitionService.Initialize(_fadeCanvasGroup, _fadeImage);
            }
            ServiceLocator.Instance.Register<ITransitionService>(transitionService);

            // AudioServiceを作成・登録
            var audioService = new AudioService();
            if (_audioMixer != null && _bgmSource != null && _sfxSource != null && _voiceSource != null)
            {
                audioService.Initialize(_audioMixer, _bgmSource, _sfxSource, _voiceSource);
            }
            ServiceLocator.Instance.Register<IAudioService>(audioService);

            // === Night Signal Services (Flag/EndState/Clock) ===
            // これらは他のOperator Modeサービスより先に作成する必要がある

            // FlagServiceを作成・登録
            var flagService = new FlagService();
            ServiceLocator.Instance.Register<IFlagService>(flagService);

            // ClockServiceを作成・登録
            var clockService = new ClockService();
            ServiceLocator.Instance.Register<IClockService>(clockService);

            // EndStateServiceを作成・登録（FlagServiceに依存）
            var endStateService = new EndStateService(flagService);
            ServiceLocator.Instance.Register<IEndStateService>(endStateService);

            // === Operator Mode Services ===

            // EvidenceServiceを作成・登録
            var evidenceService = new EvidenceService();
            ServiceLocator.Instance.Register<IEvidenceService>(evidenceService);

            // TrustGraphServiceを作成・登録
            var trustGraphService = new TrustGraphService();
            if (_trustGraphData != null)
            {
                trustGraphService.Initialize(_trustGraphData);
            }
            ServiceLocator.Instance.Register<ITrustGraphService>(trustGraphService);

            // WorldStateServiceを作成・登録（StoryService, EndStateService, ClockServiceに依存）
            var worldStateService = new WorldStateService(storyService, endStateService, clockService);
            ServiceLocator.Instance.Register<IWorldStateService>(worldStateService);

            // CallFlowServiceを作成・登録（複数サービスに依存、FlagService/ClockService統合）
            var callFlowService = new CallFlowService(
                storyService,
                evidenceService,
                trustGraphService,
                flagService,
                clockService);
            ServiceLocator.Instance.Register<ICallFlowService>(callFlowService);

            // OperatorSaveServiceを作成・登録（夜間進行のセーブ/ロード用）
            var operatorSaveService = new OperatorSaveService();
            ServiceLocator.Instance.Register<IOperatorSaveService>(operatorSaveService);

            Debug.Log("[GameBootstrap] すべてのサービスを登録しました。");
        }

        private void OnDestroy()
        {
            // アプリケーション終了時にサービスをクリア
            if (_isInitialized)
            {
                ServiceLocator.Instance.Clear();
                _isInitialized = false;
            }
        }

        /// <summary>
        /// 初期化状態をリセットする（主にテスト用）
        /// </summary>
        public static void ResetInitialization()
        {
            _isInitialized = false;
            ServiceLocator.ResetInstance();
        }
    }
}

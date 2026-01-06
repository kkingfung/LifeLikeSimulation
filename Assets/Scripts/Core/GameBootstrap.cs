#nullable enable
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.AssetBundle;
using LifeLike.Services.CallFlow;
using LifeLike.Services.Choice;
using LifeLike.Services.Evidence;
using LifeLike.Services.Relationship;
using LifeLike.Services.Save;
using LifeLike.Services.Story;
using LifeLike.Services.Transition;
using LifeLike.Services.Localization;
using LifeLike.Services.TrustGraph;
using LifeLike.Services.Video;
using LifeLike.Services.WorldState;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LifeLike.Core
{
    /// <summary>
    /// ゲームの初期化を行うBootstrapクラス
    /// Bootstrapシーンに配置し、最初に実行される
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Game Data")]
        [SerializeField] private GameStateData? _gameStateData;

        [Header("Transition UI")]
        [SerializeField] private CanvasGroup? _fadeCanvasGroup;
        [SerializeField] private Image? _fadeImage;

        [Header("AssetBundle Settings")]
        [Tooltip("AssetBundleのダウンロード元ベースURL（空の場合はStreamingAssetsを使用）")]
        [SerializeField] private string _assetBundleBaseUrl = string.Empty;

        [Header("Operator Mode Settings")]
        [Tooltip("初期信頼グラフデータ")]
        [SerializeField] private TrustGraphData? _trustGraphData;

        [Header("Settings")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
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
            if (_loadMainMenuOnStart && !string.IsNullOrEmpty(_mainMenuSceneName))
            {
                SceneManager.LoadScene(_mainMenuSceneName);
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

            // RelationshipServiceを作成・登録（StoryServiceに依存）
            var relationshipService = new RelationshipService(storyService);
            ServiceLocator.Instance.Register<IRelationshipService>(relationshipService);

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

            // WorldStateServiceを作成・登録（StoryServiceに依存）
            var worldStateService = new WorldStateService(storyService);
            ServiceLocator.Instance.Register<IWorldStateService>(worldStateService);

            // CallFlowServiceを作成・登録（複数サービスに依存）
            var callFlowService = new CallFlowService(storyService, evidenceService, trustGraphService);
            ServiceLocator.Instance.Register<ICallFlowService>(callFlowService);

            // GameStateDataが設定されていれば、キャラクターを登録
            if (_gameStateData != null)
            {
                relationshipService.RegisterCharacters(_gameStateData.characters);
            }

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

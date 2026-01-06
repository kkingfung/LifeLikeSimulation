#nullable enable
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Services.AssetBundle;
using LifeLike.Services.Choice;
using LifeLike.Services.Relationship;
using LifeLike.Services.Save;
using LifeLike.Services.Story;
using LifeLike.Services.Transition;
using LifeLike.Services.Video;
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

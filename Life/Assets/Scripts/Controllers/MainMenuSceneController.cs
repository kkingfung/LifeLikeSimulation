#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Data;
using LifeLike.Services.Core.Save;
using LifeLike.Services.Core.Story;
using LifeLike.Services.Core.Subscene;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// メインメニューシーンのコントローラー
    /// シーンの初期化とサービス管理を担当
    /// </summary>
    public class MainMenuSceneController : SceneControllerBase
    {
        [Header("Game Mode")]
        [SerializeField] private bool _useOperatorMode = true;

        [Header("Story Mode Settings (Optional)")]
        [SerializeField] private GameStateData? _gameStateData;

        [Header("Scene Settings")]
        [SerializeField] private SceneReference _chapterSelectScene = new();
        [SerializeField] private SceneReference _operatorScene = new();
        [SerializeField] private SceneReference _settingsScene = new();

        // サービス参照
        private IOperatorSaveService? _operatorSaveService;
        private IStoryService? _storyService;
        private ISaveService? _saveService;
        private ISubsceneService? _subsceneService;

        /// <summary>
        /// オペレーターモードかどうか
        /// </summary>
        public bool UseOperatorMode => _useOperatorMode;

        /// <summary>
        /// ゲーム状態データ（ストーリーモード用）
        /// </summary>
        public GameStateData? GameStateData => _gameStateData;

        /// <summary>
        /// オペレーターセーブサービス
        /// </summary>
        public IOperatorSaveService? OperatorSaveService => _operatorSaveService;

        /// <summary>
        /// ストーリーサービス
        /// </summary>
        public IStoryService? StoryService => _storyService;

        /// <summary>
        /// セーブサービス
        /// </summary>
        public ISaveService? SaveService => _saveService;

        private void Awake()
        {
            // サービスを取得
            TryGetService(out _subsceneService);

            if (_useOperatorMode)
            {
                TryGetService(out _operatorSaveService);
            }
            else
            {
                TryGetService(out _storyService);
                TryGetService(out _saveService);
            }
        }

        /// <summary>
        /// チャプター選択画面へ遷移
        /// </summary>
        public void NavigateToChapterSelect()
        {
            NavigateTo(_chapterSelectScene);
        }

        /// <summary>
        /// オペレーター画面へ遷移
        /// </summary>
        public void NavigateToOperator()
        {
            NavigateTo(_operatorScene);
        }

        /// <summary>
        /// 設定画面へ遷移
        /// </summary>
        public void NavigateToSettings()
        {
            NavigateTo(_settingsScene);
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        public void QuitGame()
        {
            QuitApplication();
        }
    }
}

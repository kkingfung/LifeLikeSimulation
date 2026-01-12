#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Services.Core.Save;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// 結果画面シーンのコントローラー
    /// シーンの初期化とサービス管理を担当
    /// </summary>
    public class ResultSceneController : SceneControllerBase
    {
        [Header("Scene Settings")]
        [SerializeField] private SceneReference _mainMenuScene = new();
        [SerializeField] private SceneReference _chapterSelectScene = new();
        [SerializeField] private SceneReference _operatorScene = new();

        // サービス参照
        private IOperatorSaveService? _operatorSaveService;

        /// <summary>
        /// オペレーターセーブサービス
        /// </summary>
        public IOperatorSaveService? OperatorSaveService => _operatorSaveService;

        private void Awake()
        {
            TryGetService(out _operatorSaveService);
        }

        /// <summary>
        /// メインメニューへ戻る
        /// </summary>
        public void NavigateToMainMenu()
        {
            NavigateTo(_mainMenuScene);
        }

        /// <summary>
        /// チャプター選択画面へ戻る
        /// </summary>
        public void NavigateToChapterSelect()
        {
            NavigateTo(_chapterSelectScene);
        }

        /// <summary>
        /// 新規ゲームを開始（Night 0から）
        /// </summary>
        public void StartNewGame()
        {
            PlayerPrefs.SetInt("LifeLike_StartNightIndex", 0);
            PlayerPrefs.Save();
            NavigateTo(_operatorScene);
        }
    }
}

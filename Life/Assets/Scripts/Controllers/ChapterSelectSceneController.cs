#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Services.Core.Save;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// チャプター選択シーンのコントローラー
    /// シーンの初期化とサービス管理を担当
    /// </summary>
    public class ChapterSelectSceneController : SceneControllerBase
    {
        [Header("Scene Settings")]
        [SerializeField] private SceneReference _operatorScene = new();
        [SerializeField] private SceneReference _mainMenuScene = new();

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
        /// 指定したチャプターを開始
        /// </summary>
        /// <param name="nightIndex">夜のインデックス</param>
        public void StartChapter(int nightIndex)
        {
            PlayerPrefs.SetInt("LifeLike_StartNightIndex", nightIndex);
            PlayerPrefs.Save();
            NavigateTo(_operatorScene);
        }

        /// <summary>
        /// メインメニューへ戻る
        /// </summary>
        public void NavigateToMainMenu()
        {
            NavigateTo(_mainMenuScene);
        }
    }
}

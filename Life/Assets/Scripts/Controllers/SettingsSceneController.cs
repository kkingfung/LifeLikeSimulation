#nullable enable
using LifeLike.Core.Scene;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// 設定画面シーンのコントローラー
    /// シーンの初期化とサービス管理を担当
    /// </summary>
    public class SettingsSceneController : SceneControllerBase
    {
        [Header("Navigation")]
        [SerializeField] private SceneReference _mainMenuScene = new();

        /// <summary>
        /// サブシーンモードかどうか（現在は常にfalse）
        /// </summary>
        public bool IsSubsceneMode => false;

        /// <summary>
        /// 戻る処理を実行（メインメニューへ遷移）
        /// </summary>
        public void NavigateBack()
        {
            NavigateTo(_mainMenuScene);
        }
    }
}

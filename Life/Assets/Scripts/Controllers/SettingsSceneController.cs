#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Services.Core.Subscene;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// 設定画面シーンのコントローラー
    /// シーンの初期化とサービス管理を担当
    /// サブシーンモード（オーバーレイ）と通常モード（フルシーン）の両方に対応
    /// </summary>
    public class SettingsSceneController : SceneControllerBase
    {
        [Header("Navigation")]
        [SerializeField] private SceneReference _mainMenuScene = new();

        // サービス参照
        private ISubsceneService? _subsceneService;
        private bool _isSubsceneMode;

        /// <summary>
        /// サブシーンサービス
        /// </summary>
        public ISubsceneService? SubsceneService => _subsceneService;

        /// <summary>
        /// サブシーンモードかどうか
        /// </summary>
        public bool IsSubsceneMode => _isSubsceneMode;

        private void Awake()
        {
            _subsceneService = GetService<ISubsceneService>();
            _isSubsceneMode = _subsceneService?.IsOpenedAsSubscene(CurrentSceneName) ?? false;

            if (_isSubsceneMode)
            {
                Debug.Log("[SettingsSceneController] サブシーンモードで起動");
            }
        }

        /// <summary>
        /// 戻る処理を実行
        /// サブシーンモードの場合はサブシーンを閉じ、通常モードの場合はメインメニューへ遷移
        /// </summary>
        public async void NavigateBack()
        {
            if (_isSubsceneMode && _subsceneService != null)
            {
                await _subsceneService.CloseSubsceneAsync();
            }
            else
            {
                NavigateTo(_mainMenuScene);
            }
        }
    }
}

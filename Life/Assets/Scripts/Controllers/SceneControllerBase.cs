#nullable enable
using LifeLike.Core.Scene;
using LifeLike.Core.Services;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// シーンコントローラーの基底クラス
    /// 共通のサービス取得とシーン遷移機能を提供
    /// </summary>
    public abstract class SceneControllerBase : MonoBehaviour
    {
        [Header("Current Scene")]
        [SerializeField] private SceneReference _currentScene = new();

        /// <summary>
        /// このコントローラーが配置されているシーンの参照
        /// </summary>
        public SceneReference CurrentScene => _currentScene;

        /// <summary>
        /// このコントローラーが配置されているシーンの名前
        /// </summary>
        public string CurrentSceneName => _currentScene.SceneName;

        /// <summary>
        /// ServiceLocatorのインスタンス（便利プロパティ）
        /// </summary>
        protected ServiceLocator Services => ServiceLocator.Instance;

        /// <summary>
        /// サービスを取得する
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <returns>サービスのインスタンス、または null</returns>
        protected T? GetService<T>() where T : class
        {
            return Services.Get<T>();
        }

        /// <summary>
        /// サービスを取得し、取得できなかった場合はエラーログを出力
        /// </summary>
        /// <typeparam name="T">サービスのインターフェース型</typeparam>
        /// <param name="service">取得したサービス</param>
        /// <returns>サービスが取得できた場合は true</returns>
        protected bool TryGetService<T>(out T? service) where T : class
        {
            service = Services.Get<T>();
            if (service == null)
            {
                Debug.LogError($"[{GetType().Name}] {typeof(T).Name}が見つかりません。");
                return false;
            }
            return true;
        }

        /// <summary>
        /// シーン遷移を実行
        /// </summary>
        /// <param name="sceneReference">遷移先シーン</param>
        protected void NavigateTo(SceneReference sceneReference)
        {
            sceneReference.LoadScene();
        }

        /// <summary>
        /// ゲームを終了
        /// </summary>
        protected void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

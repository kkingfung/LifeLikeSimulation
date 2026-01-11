#nullable enable
using UnityEngine;

namespace LifeLike.UI
{
    /// <summary>
    /// UIテーマを管理するシングルトンマネージャー
    /// シーン間で共有され、テーマ設定へのアクセスを提供
    /// </summary>
    public class UIThemeManager : MonoBehaviour
    {
        private static UIThemeManager? _instance;
        public static UIThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIThemeManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("UIThemeManager");
                        _instance = go.AddComponent<UIThemeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private UITheme? _theme;

        /// <summary>
        /// 現在のテーマ
        /// </summary>
        public UITheme Theme
        {
            get
            {
                if (_theme == null)
                {
                    // テーマが設定されていない場合はデフォルトを使用
                    _theme = UITheme.CreateDefault();
                    UnityEngine.Debug.LogWarning("[UIThemeManager] テーマが設定されていません。デフォルトテーマを使用します。");
                }
                return _theme;
            }
            set => _theme = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// テーマをロードして設定
        /// </summary>
        public void LoadTheme(UITheme theme)
        {
            _theme = theme;
            UnityEngine.Debug.Log($"[UIThemeManager] テーマをロードしました: {theme.name}");
        }

        /// <summary>
        /// Resourcesからテーマをロード
        /// </summary>
        public void LoadThemeFromResources(string themeName = "DefaultTheme")
        {
            var theme = Resources.Load<UITheme>($"Themes/{themeName}");
            if (theme != null)
            {
                LoadTheme(theme);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[UIThemeManager] テーマが見つかりません: {themeName}");
            }
        }
    }
}

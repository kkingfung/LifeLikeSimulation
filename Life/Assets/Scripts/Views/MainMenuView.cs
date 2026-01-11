#nullable enable
using System.ComponentModel;
using LifeLike.Controllers;
using LifeLike.UI;
using LifeLike.UI.Effects;
using LifeLike.ViewModels;
using UnityEngine;
using UnityEngine.UI;

namespace LifeLike.Views
{
    /// <summary>
    /// メインメニュー画面のView
    /// オペレーターモードとストーリーモードの両方に対応
    /// CRT効果、ボタンホバー効果、タイプライター効果を含む
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private MainMenuSceneController? _controller;

        [Header("UI References")]
        [SerializeField] private Button? _startButton;
        [SerializeField] private Button? _continueButton;
        [SerializeField] private Button? _settingsButton;
        [SerializeField] private Button? _quitButton;
        [SerializeField] private Button? _deleteSaveButton;

        [Header("Info Display")]
        [SerializeField] private Text? _lastSaveText;
        [SerializeField] private Text? _currentNightText;
        [SerializeField] private Text? _titleText;
        [SerializeField] private Text? _versionText;

        [Header("UI Effects")]
        [SerializeField] private UITheme? _theme;
        [SerializeField] private bool _enableCRTEffect = true;
        [SerializeField] private bool _enableTypewriterTitle = true;
        [SerializeField] private bool _enableButtonEffects = true;
        [SerializeField] private Canvas? _mainCanvas;

        private MainMenuViewModel? _viewModel;
        private GameObject? _crtOverlay;
        private TypewriterEffect? _titleTypewriter;
        private FadeEffect? _screenFade;
        private ShakeEffect? _deleteSaveShake;

        private void Awake()
        {
            // コントローラーを検索
            if (_controller == null)
            {
                _controller = FindFirstObjectByType<MainMenuSceneController>();
            }

            if (_controller == null)
            {
                Debug.LogError("[MainMenuView] MainMenuSceneControllerが見つかりません。");
                return;
            }

            // コントローラーからサービスを取得してViewModelを作成
            if (_controller.UseOperatorMode)
            {
                InitializeOperatorMode();
            }
            else
            {
                InitializeStoryMode();
            }
        }

        /// <summary>
        /// オペレーターモード用の初期化
        /// </summary>
        private void InitializeOperatorMode()
        {
            if (_controller?.OperatorSaveService == null)
            {
                Debug.LogError("[MainMenuView] IOperatorSaveServiceが見つかりません。");
                return;
            }

            _viewModel = new MainMenuViewModel(_controller.OperatorSaveService);
        }

        /// <summary>
        /// ストーリーモード用の初期化
        /// </summary>
        private void InitializeStoryMode()
        {
            if (_controller?.StoryService == null || _controller.SaveService == null || _controller.GameStateData == null)
            {
                Debug.LogError("[MainMenuView] 必要なサービスまたはデータがありません。");
                return;
            }

            _viewModel = new MainMenuViewModel(_controller.StoryService, _controller.SaveService, _controller.GameStateData);
        }

        private void Start()
        {
            if (_viewModel == null)
            {
                return;
            }

            // テーマを設定
            SetupTheme();

            // UI効果をセットアップ
            SetupUIEffects();

            // UIをセットアップ
            SetupUI();

            // ViewModelのイベントを購読
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.OnGameStartRequested += OnGameStartRequested;
            _viewModel.OnOpenSettingsRequested += OnOpenSettingsRequested;
            _viewModel.OnChapterSelectRequested += OnChapterSelectRequested;

            // 初期状態を反映
            UpdateUI();

            // タイトルを設定（タイプライター効果付き）
            SetupTitle();

            // バージョンを設定
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }
        }

        /// <summary>
        /// テーマを設定
        /// </summary>
        private void SetupTheme()
        {
            if (_theme != null)
            {
                UIThemeManager.Instance.Theme = _theme;
            }
        }

        /// <summary>
        /// UI効果をセットアップ
        /// </summary>
        private void SetupUIEffects()
        {
            // CRT効果を追加
            if (_enableCRTEffect)
            {
                SetupCRTEffect();
            }

            // ボタンにホバー効果とオーディオフィードバックを追加
            if (_enableButtonEffects)
            {
                SetupButtonEffects();
            }

            // 画面フェード効果をセットアップ
            SetupScreenFade();

            // 削除ボタンのシェイク効果をセットアップ
            SetupDeleteSaveShake();
        }

        /// <summary>
        /// 画面フェード効果をセットアップ
        /// </summary>
        private void SetupScreenFade()
        {
            var canvas = _mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null) return;

            // フェード用のCanvasGroupを追加
            var canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }

            _screenFade = canvas.GetComponent<FadeEffect>();
            if (_screenFade == null)
            {
                _screenFade = canvas.gameObject.AddComponent<FadeEffect>();
            }

            // シーン開始時にフェードイン
            _screenFade.FadeIn(0.5f);
        }

        /// <summary>
        /// 削除ボタンのシェイク効果をセットアップ
        /// </summary>
        private void SetupDeleteSaveShake()
        {
            if (_deleteSaveButton == null) return;

            _deleteSaveShake = _deleteSaveButton.GetComponent<ShakeEffect>();
            if (_deleteSaveShake == null)
            {
                _deleteSaveShake = _deleteSaveButton.gameObject.AddComponent<ShakeEffect>();
                _deleteSaveShake.SetErrorPreset();
            }
        }

        /// <summary>
        /// CRT効果をセットアップ
        /// </summary>
        private void SetupCRTEffect()
        {
            var canvas = _mainCanvas;
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                Debug.LogWarning("[MainMenuView] Canvasが見つかりません。CRT効果をスキップします。");
                return;
            }

            // CRTオーバーレイを作成
            _crtOverlay = new GameObject("CRTOverlay");
            _crtOverlay.transform.SetParent(canvas.transform, false);

            var rectTransform = _crtOverlay.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var rawImage = _crtOverlay.AddComponent<RawImage>();
            rawImage.raycastTarget = false;

            var crtEffect = _crtOverlay.AddComponent<CRTEffect>();

            // テーマから設定を適用
            var theme = UIThemeManager.Instance.Theme;
            crtEffect.ApplyTheme(theme);

            // 最前面に配置
            _crtOverlay.transform.SetAsLastSibling();
        }

        /// <summary>
        /// ボタン効果をセットアップ
        /// </summary>
        private void SetupButtonEffects()
        {
            var theme = UIThemeManager.Instance.Theme;

            // 各ボタンにホバー効果を追加
            AddButtonEffects(_startButton, theme);
            AddButtonEffects(_continueButton, theme);
            AddButtonEffects(_settingsButton, theme);
            AddButtonEffects(_quitButton, theme);
            AddButtonEffects(_deleteSaveButton, theme, ButtonAudioFeedback.ClickSoundType.Warning);
        }

        /// <summary>
        /// ボタンにエフェクトを追加
        /// </summary>
        private void AddButtonEffects(Button? button, UITheme theme, ButtonAudioFeedback.ClickSoundType soundType = ButtonAudioFeedback.ClickSoundType.Default)
        {
            if (button == null) return;

            // ホバー効果を追加
            var hoverEffect = button.gameObject.GetComponent<ButtonHoverEffect>();
            if (hoverEffect == null)
            {
                hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
                hoverEffect.ApplyTheme(theme);
            }

            // オーディオフィードバックを追加
            var audioFeedback = button.gameObject.GetComponent<ButtonAudioFeedback>();
            if (audioFeedback == null)
            {
                audioFeedback = button.gameObject.AddComponent<ButtonAudioFeedback>();
                audioFeedback.SetClickSoundType(soundType);
            }
        }

        /// <summary>
        /// タイトルを設定（タイプライター効果付き）
        /// </summary>
        private void SetupTitle()
        {
            if (_titleText == null) return;

            var titleContent = (_controller?.UseOperatorMode ?? true) ? "OPERATOR\nNIGHT SIGNAL" : "LIFELIKE";

            if (_enableTypewriterTitle)
            {
                _titleTypewriter = _titleText.GetComponent<TypewriterEffect>();
                if (_titleTypewriter == null)
                {
                    _titleTypewriter = _titleText.gameObject.AddComponent<TypewriterEffect>();
                }
                _titleTypewriter.StartTyping(titleContent);
            }
            else
            {
                _titleText.text = titleContent;
            }
        }

        private void OnDestroy()
        {
            // CRTオーバーレイを破棄
            if (_crtOverlay != null)
            {
                Destroy(_crtOverlay);
            }

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.OnGameStartRequested -= OnGameStartRequested;
                _viewModel.OnOpenSettingsRequested -= OnOpenSettingsRequested;
                _viewModel.OnChapterSelectRequested -= OnChapterSelectRequested;
                _viewModel.Dispose();
            }
        }

        /// <summary>
        /// UIをセットアップする
        /// </summary>
        private void SetupUI()
        {
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartClicked);
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (_deleteSaveButton != null)
            {
                _deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
            }
        }

        /// <summary>
        /// UIを更新する
        /// </summary>
        private void UpdateUI()
        {
            if (_viewModel == null)
            {
                return;
            }

            // コンティニューボタンの有効/無効（セーブがある時のみ有効）
            if (_continueButton != null)
            {
                _continueButton.interactable = _viewModel.CanContinue;
            }

            // 削除ボタンの有効/無効（セーブがある時のみ表示）
            if (_deleteSaveButton != null)
            {
                _deleteSaveButton.interactable = _viewModel.CanContinue;
                _deleteSaveButton.gameObject.SetActive(_viewModel.CanContinue);
            }

            // 最後のセーブ情報
            if (_lastSaveText != null)
            {
                _lastSaveText.text = _viewModel.LastSaveInfo;
                _lastSaveText.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.LastSaveInfo));
            }

            // 現在の夜情報（オペレーターモード用）
            if (_currentNightText != null)
            {
                _currentNightText.text = _viewModel.CurrentNightInfo;
                _currentNightText.gameObject.SetActive(!string.IsNullOrEmpty(_viewModel.CurrentNightInfo));
            }
        }

        /// <summary>
        /// ViewModelのプロパティ変更時の処理
        /// </summary>
        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainMenuViewModel.CanContinue):
                case nameof(MainMenuViewModel.LastSaveInfo):
                case nameof(MainMenuViewModel.CurrentNightInfo):
                    UpdateUI();
                    break;
            }
        }

        /// <summary>
        /// ストーリーモードゲーム開始要求時の処理
        /// </summary>
        private void OnGameStartRequested()
        {
            _controller?.NavigateToOperator();
        }

        /// <summary>
        /// 設定画面を開く要求時の処理
        /// </summary>
        private void OnOpenSettingsRequested()
        {
            _controller?.NavigateToSettings();
        }

        /// <summary>
        /// チャプター選択画面を開く要求時の処理
        /// </summary>
        private void OnChapterSelectRequested()
        {
            _controller?.NavigateToChapterSelect();
        }

        /// <summary>
        /// スタートボタンクリック時の処理
        /// </summary>
        private void OnStartClicked()
        {
            _viewModel?.StartCommand.Execute(null);
        }

        /// <summary>
        /// コンティニューボタンクリック時の処理
        /// </summary>
        private void OnContinueClicked()
        {
            // セーブデータがある場合のみ実行
            if (_viewModel != null && _viewModel.CanContinue)
            {
                _viewModel.StartCommand.Execute(null);
            }
        }

        /// <summary>
        /// 設定ボタンクリック時の処理
        /// </summary>
        private void OnSettingsClicked()
        {
            _viewModel?.OpenSettingsCommand.Execute(null);
        }

        /// <summary>
        /// 終了ボタンクリック時の処理
        /// </summary>
        private void OnQuitClicked()
        {
            _viewModel?.QuitGameCommand.Execute(null);
        }

        /// <summary>
        /// セーブデータ削除ボタンクリック時の処理
        /// </summary>
        private void OnDeleteSaveClicked()
        {
            // 削除前にシェイク効果で警告
            _deleteSaveShake?.Shake();
            _viewModel?.DeleteSaveCommand.Execute(null);
        }
    }
}

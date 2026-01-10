#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using LifeLike.Core.Services;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Services.Clock;
using LifeLike.Services.EndState;
using LifeLike.Services.Flag;
using LifeLike.Services.Save;
using UnityEngine;

namespace LifeLike.UI.Debug
{
    /// <summary>
    /// 開発者向けデバッグパネル
    /// 夜間進行、フラグ操作、時間操作、セーブデータ管理を提供
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F12;
        [SerializeField] private bool _enableInReleaseBuild = false;

        [Header("Window Settings")]
        [SerializeField] private float _windowWidth = 400f;
        [SerializeField] private float _windowHeight = 500f;

        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        private Rect _windowRect;

        // サービス参照
        private IFlagService? _flagService;
        private IClockService? _clockService;
        private IEndStateService? _endStateService;
        private IOperatorSaveService? _operatorSaveService;

        // UI状態
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Night", "Flags", "Time", "Save", "EndState" };

        // 入力フィールド
        private string _flagIdInput = string.Empty;
        private string _jumpToTimeInput = string.Empty;

        // キャッシュ
        private List<FlagState> _cachedFlags = new();
        private float _lastFlagRefresh = 0f;
        private const float FLAG_REFRESH_INTERVAL = 0.5f;

        /// <summary>
        /// デバッグパネルの表示状態
        /// </summary>
        public bool IsVisible => _isVisible;

        /// <summary>
        /// 夜ジャンプイベント
        /// </summary>
        public event Action<int>? OnNightJumpRequested;

        private void Awake()
        {
            // リリースビルドでは無効化（設定による）
            if (!Application.isEditor && !UnityEngine.Debug.isDebugBuild && !_enableInReleaseBuild)
            {
                enabled = false;
                return;
            }

            _windowRect = new Rect(10, 10, _windowWidth, _windowHeight);
        }

        private void Start()
        {
            // サービス取得
            _flagService = ServiceLocator.Instance.Get<IFlagService>();
            _clockService = ServiceLocator.Instance.Get<IClockService>();
            _endStateService = ServiceLocator.Instance.Get<IEndStateService>();
            _operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();

            UnityEngine.Debug.Log("[DebugPanel] 初期化完了");
        }

        private void Update()
        {
            // トグルキーでパネル表示切替
            if (Input.GetKeyDown(_toggleKey))
            {
                _isVisible = !_isVisible;
                UnityEngine.Debug.Log($"[DebugPanel] 表示状態: {_isVisible}");
            }

            // フラグキャッシュの更新
            if (_isVisible && Time.time - _lastFlagRefresh > FLAG_REFRESH_INTERVAL)
            {
                RefreshFlagCache();
                _lastFlagRefresh = Time.time;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            // ウィンドウスタイルを設定
            GUI.skin.window.fontSize = 14;
            GUI.skin.button.fontSize = 12;
            GUI.skin.label.fontSize = 12;
            GUI.skin.textField.fontSize = 12;

            _windowRect = GUI.Window(0, _windowRect, DrawDebugWindow, "Debug Panel (F12 to close)");
        }

        /// <summary>
        /// デバッグウィンドウを描画
        /// </summary>
        private void DrawDebugWindow(int windowId)
        {
            GUILayout.BeginVertical();

            // タブ選択
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            GUILayout.Space(10);

            // スクロールビュー開始
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0:
                    DrawNightTab();
                    break;
                case 1:
                    DrawFlagsTab();
                    break;
                case 2:
                    DrawTimeTab();
                    break;
                case 3:
                    DrawSaveTab();
                    break;
                case 4:
                    DrawEndStateTab();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // ウィンドウをドラッグ可能に
            GUI.DragWindow(new Rect(0, 0, _windowWidth, 20));
        }

        /// <summary>
        /// 夜間進行タブ
        /// </summary>
        private void DrawNightTab()
        {
            GUILayout.Label("=== Night Control ===", GUI.skin.box);
            GUILayout.Space(5);

            // 現在の夜を表示
            int currentNight = _operatorSaveService?.GetCurrentNightIndex() ?? 0;
            GUILayout.Label($"Current Night: Night{currentNight + 1:D2}");
            GUILayout.Space(10);

            // 夜選択
            GUILayout.Label("Jump to Night:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 5; i++)
            {
                if (GUILayout.Button($"N{i + 1:D2}"))
                {
                    JumpToNight(i);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (int i = 5; i < 10; i++)
            {
                if (GUILayout.Button($"N{i + 1:D2}"))
                {
                    JumpToNight(i);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 完了した夜の一覧
            GUILayout.Label("=== Completed Nights ===", GUI.skin.box);
            var completedNights = _operatorSaveService?.GetCompletedNights() ?? new List<string>();
            if (completedNights.Count == 0)
            {
                GUILayout.Label("(No completed nights)");
            }
            else
            {
                foreach (var nightId in completedNights)
                {
                    var endState = _operatorSaveService?.GetNightEndState(nightId);
                    GUILayout.Label($"  {nightId}: {endState?.ToString() ?? "Unknown"}");
                }
            }
        }

        /// <summary>
        /// フラグタブ
        /// </summary>
        private void DrawFlagsTab()
        {
            GUILayout.Label("=== Flag Control ===", GUI.skin.box);
            GUILayout.Space(5);

            // フラグ設定
            GUILayout.Label("Set Flag:");
            GUILayout.BeginHorizontal();
            _flagIdInput = GUILayout.TextField(_flagIdInput, GUILayout.Width(200));
            if (GUILayout.Button("Set"))
            {
                SetFlag(_flagIdInput);
            }
            if (GUILayout.Button("Clear"))
            {
                ClearFlag(_flagIdInput);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 現在のフラグ一覧
            GUILayout.Label("=== Active Flags ===", GUI.skin.box);
            var activeFlags = _cachedFlags.Where(f => f.isSet).ToList();
            if (activeFlags.Count == 0)
            {
                GUILayout.Label("(No active flags)");
            }
            else
            {
                foreach (var flag in activeFlags)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"  {flag.flagId}", GUILayout.Width(200));
                    GUILayout.Label($"@{flag.setTime}min");
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        ClearFlag(flag.flagId);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(10);

            // 全フラグクリア
            if (GUILayout.Button("Clear All Flags"))
            {
                ClearAllFlags();
            }
        }

        /// <summary>
        /// 時間タブ
        /// </summary>
        private void DrawTimeTab()
        {
            GUILayout.Label("=== Time Control ===", GUI.skin.box);
            GUILayout.Space(5);

            // 現在時刻
            int currentTime = _clockService?.CurrentTimeMinutes ?? 0;
            string formattedTime = _clockService?.FormattedTime ?? "--:--";
            GUILayout.Label($"Current Time: {formattedTime} ({currentTime} minutes)");
            GUILayout.Space(10);

            // 時間スキップ
            GUILayout.Label("Skip Time (minutes):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+5"))
            {
                SkipTime(5);
            }
            if (GUILayout.Button("+15"))
            {
                SkipTime(15);
            }
            if (GUILayout.Button("+30"))
            {
                SkipTime(30);
            }
            if (GUILayout.Button("+60"))
            {
                SkipTime(60);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 特定時刻にジャンプ
            GUILayout.Label("Jump to Time (minutes from midnight):");
            GUILayout.BeginHorizontal();
            _jumpToTimeInput = GUILayout.TextField(_jumpToTimeInput, GUILayout.Width(100));
            if (GUILayout.Button("Jump"))
            {
                if (int.TryParse(_jumpToTimeInput, out int targetTime))
                {
                    JumpToTime(targetTime);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 時間操作
            GUILayout.Label("Time Control:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_clockService?.IsRunning == true ? "Pause" : "Resume"))
            {
                ToggleTimePause();
            }
            GUILayout.EndHorizontal();

            // 時間速度
            GUILayout.Space(10);
            float timeScale = Time.timeScale;
            GUILayout.Label($"Time Scale: {timeScale:F1}x");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x"))
            {
                Time.timeScale = 0.5f;
            }
            if (GUILayout.Button("1x"))
            {
                Time.timeScale = 1f;
            }
            if (GUILayout.Button("2x"))
            {
                Time.timeScale = 2f;
            }
            if (GUILayout.Button("5x"))
            {
                Time.timeScale = 5f;
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// セーブタブ
        /// </summary>
        private void DrawSaveTab()
        {
            GUILayout.Label("=== Save Data ===", GUI.skin.box);
            GUILayout.Space(5);

            // セーブ状態
            bool hasSave = _operatorSaveService?.HasSaveData ?? false;
            GUILayout.Label($"Has Save Data: {(hasSave ? "Yes" : "No")}");

            if (hasSave)
            {
                var lastSaveTime = _operatorSaveService?.LastSaveTime;
                GUILayout.Label($"Last Save: {lastSaveTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}");

                bool hasMidNight = _operatorSaveService?.HasMidNightSave ?? false;
                GUILayout.Label($"Mid-Night Save: {(hasMidNight ? "Yes" : "No")}");
            }

            GUILayout.Space(10);

            // セーブ操作
            GUILayout.Label("=== Operations ===", GUI.skin.box);

            if (GUILayout.Button("Force Auto-Save"))
            {
                ForceAutoSave();
            }

            GUILayout.Space(5);

            // 危険な操作
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("DELETE ALL SAVE DATA"))
            {
                if (Event.current.shift)
                {
                    DeleteAllSaveData();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[DebugPanel] Shift+クリックでセーブデータを削除");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Label("(Hold Shift + Click to confirm)");
        }

        /// <summary>
        /// エンドステートタブ
        /// </summary>
        private void DrawEndStateTab()
        {
            GUILayout.Label("=== EndState ===", GUI.skin.box);
            GUILayout.Space(5);

            // 現在のエンドステート判定
            var currentEndState = _endStateService?.CalculateEndState();
            GUILayout.Label($"Current EndState: {currentEndState?.ToString() ?? "None"}");

            GUILayout.Space(10);

            // 強制エンドステート設定
            GUILayout.Label("Force End Night with State:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Best"))
            {
                ForceEndNight(EndStateType.TruthDawn);
            }
            if (GUILayout.Button("Good"))
            {
                ForceEndNight(EndStateType.InvestigationContinues);
            }
            if (GUILayout.Button("Neutral"))
            {
                ForceEndNight(EndStateType.UncertainDawn);
            }
            if (GUILayout.Button("Bad"))
            {
                ForceEndNight(EndStateType.IntoDarkness);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 各夜のエンドステート
            GUILayout.Label("=== All Night EndStates ===", GUI.skin.box);
            for (int i = 0; i < 10; i++)
            {
                string nightId = $"night_{i + 1:D2}";
                var endState = _operatorSaveService?.GetNightEndState(nightId);
                string status = endState?.ToString() ?? "(not completed)";
                GUILayout.Label($"  Night{i + 1:D2}: {status}");
            }
        }

        #region Actions

        private void JumpToNight(int nightIndex)
        {
            UnityEngine.Debug.Log($"[DebugPanel] Night{nightIndex + 1:D2}にジャンプ要求");
            OnNightJumpRequested?.Invoke(nightIndex);
        }

        private void SetFlag(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return;

            int currentTime = _clockService?.CurrentTimeMinutes ?? 0;
            _flagService?.SetFlag(flagId, currentTime);
            UnityEngine.Debug.Log($"[DebugPanel] フラグ設定: {flagId}");
            RefreshFlagCache();
        }

        private void ClearFlag(string flagId)
        {
            if (string.IsNullOrEmpty(flagId)) return;

            _flagService?.ClearFlag(flagId);
            UnityEngine.Debug.Log($"[DebugPanel] フラグクリア: {flagId}");
            RefreshFlagCache();
        }

        private void ClearAllFlags()
        {
            _flagService?.ClearAllFlags();
            UnityEngine.Debug.Log("[DebugPanel] 全フラグをクリア");
            RefreshFlagCache();
        }

        private void SkipTime(int minutes)
        {
            _clockService?.AdvanceTime(minutes);
            UnityEngine.Debug.Log($"[DebugPanel] {minutes}分スキップ");
        }

        private void JumpToTime(int targetMinutes)
        {
            _clockService?.SetTime(targetMinutes);
            UnityEngine.Debug.Log($"[DebugPanel] 時刻を{targetMinutes}分に設定");
        }

        private void ToggleTimePause()
        {
            if (_clockService?.IsRunning == true)
            {
                _clockService.Pause();
                UnityEngine.Debug.Log("[DebugPanel] 時間を一時停止");
            }
            else
            {
                _clockService?.Resume();
                UnityEngine.Debug.Log("[DebugPanel] 時間を再開");
            }
        }

        private void ForceAutoSave()
        {
            // 現在のフラグを取得して保存
            var persistentFlags = _flagService?.GetPersistentFlags() ?? new List<FlagState>();
            var currentEndState = _endStateService?.CalculateEndState() ?? EndStateType.UncertainDawn;
            int currentNight = _operatorSaveService?.GetCurrentNightIndex() ?? 0;
            string nightId = $"night_{currentNight + 1:D2}";

            // 中断セーブ
            int currentTime = _clockService?.CurrentTimeMinutes ?? 0;
            var currentFlags = _flagService?.GetAllFlags() ?? new List<FlagState>();
            _operatorSaveService?.SaveMidNight(nightId, currentTime, currentFlags);

            UnityEngine.Debug.Log("[DebugPanel] 強制オートセーブ完了");
        }

        private void DeleteAllSaveData()
        {
            _operatorSaveService?.DeleteSave();
            UnityEngine.Debug.Log("[DebugPanel] セーブデータを削除しました");
        }

        private void ForceEndNight(EndStateType endState)
        {
            int currentNight = _operatorSaveService?.GetCurrentNightIndex() ?? 0;
            string nightId = $"night_{currentNight + 1:D2}";
            var persistentFlags = _flagService?.GetPersistentFlags() ?? new List<FlagState>();

            _operatorSaveService?.SaveNightResult(nightId, endState, persistentFlags);
            UnityEngine.Debug.Log($"[DebugPanel] Night{currentNight + 1:D2}を{endState}で強制終了");
        }

        private void RefreshFlagCache()
        {
            _cachedFlags = _flagService?.GetAllFlags() ?? new List<FlagState>();
        }

        #endregion
    }
}

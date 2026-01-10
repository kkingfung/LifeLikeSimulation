#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Core.Services;
using LifeLike.Data;
using LifeLike.Data.EndState;
using LifeLike.Data.Flag;
using LifeLike.Services.Clock;
using LifeLike.Services.EndState;
using LifeLike.Services.Flag;
using LifeLike.Services.Save;
using UnityEngine;

namespace LifeLike.Controllers
{
    /// <summary>
    /// 夜の進行を管理するコントローラー
    /// Night01-10のシナリオデータを動的にロードし、
    /// 夜間の進行とセーブデータを管理する
    /// </summary>
    public class NightController : MonoBehaviour
    {
        [Header("Night Data")]
        [SerializeField] private List<NightData> _nightDataList = new();

        [Header("Current State")]
        [SerializeField] private int _currentNightIndex = 0;

        /// <summary>
        /// 夜ごとのデータセット
        /// </summary>
        [Serializable]
        public class NightData
        {
            public string nightId = string.Empty;
            public NightScenarioData? scenarioData;
            public NightFlagsDefinition? flagsDefinition;
            public EndStateDefinition? endStateDefinition;
        }

        /// <summary>
        /// 現在の夜のインデックス (0-9)
        /// </summary>
        public int CurrentNightIndex => _currentNightIndex;

        /// <summary>
        /// 現在の夜のID (night_01 - night_10)
        /// </summary>
        public string CurrentNightId => _currentNightIndex < _nightDataList.Count
            ? _nightDataList[_currentNightIndex].nightId
            : string.Empty;

        /// <summary>
        /// 現在の夜のシナリオデータ
        /// </summary>
        public NightScenarioData? CurrentScenarioData => _currentNightIndex < _nightDataList.Count
            ? _nightDataList[_currentNightIndex].scenarioData
            : null;

        /// <summary>
        /// 夜が終了したときのイベント
        /// </summary>
        public event Action<int, EndStateType>? OnNightEnded;

        /// <summary>
        /// 全ての夜が終了したときのイベント
        /// </summary>
        public event Action? OnAllNightsCompleted;

        private IFlagService? _flagService;
        private IEndStateService? _endStateService;
        private IClockService? _clockService;
        private IOperatorSaveService? _operatorSaveService;

        private void Awake()
        {
            // サービスを取得
            _flagService = ServiceLocator.Instance.Get<IFlagService>();
            _endStateService = ServiceLocator.Instance.Get<IEndStateService>();
            _clockService = ServiceLocator.Instance.Get<IClockService>();
            _operatorSaveService = ServiceLocator.Instance.Get<IOperatorSaveService>();
        }

        private void Start()
        {
            // セーブデータがあればロード
            if (_operatorSaveService != null && _operatorSaveService.HasSaveData)
            {
                LoadFromSave();
            }
            else
            {
                // 新規ゲーム開始
                StartNight(0);
            }
        }

        /// <summary>
        /// 指定した夜を開始する
        /// </summary>
        public void StartNight(int nightIndex)
        {
            if (nightIndex < 0 || nightIndex >= _nightDataList.Count)
            {
                Debug.LogError($"[NightController] 無効な夜インデックス: {nightIndex}");
                return;
            }

            _currentNightIndex = nightIndex;
            var nightData = _nightDataList[nightIndex];

            Debug.Log($"[NightController] 夜を開始: {nightData.nightId}");

            // フラグサービスを初期化
            if (_flagService != null && nightData.flagsDefinition != null)
            {
                _flagService.Initialize(nightData.nightId, nightData.flagsDefinition);

                // 前の夜からのフラグを復元
                if (_operatorSaveService != null)
                {
                    var persistentFlags = _operatorSaveService.GetPersistentFlags();
                    foreach (var flag in persistentFlags)
                    {
                        _flagService.SetFlag(flag.flagId, flag.setTime);
                    }
                }
            }

            // エンドステートサービスを初期化
            if (_endStateService != null && nightData.endStateDefinition != null)
            {
                _endStateService.Initialize(nightData.endStateDefinition);
            }

            // 時計サービスを初期化
            if (_clockService != null && nightData.scenarioData != null)
            {
                _clockService.Initialize(
                    nightData.scenarioData.startTimeMinutes,
                    nightData.scenarioData.endTimeMinutes,
                    nightData.scenarioData.realSecondsPerGameMinute
                );
                _clockService.Start();
            }
        }

        /// <summary>
        /// 現在の夜を終了する
        /// </summary>
        public void EndCurrentNight()
        {
            if (_endStateService == null || _flagService == null)
            {
                Debug.LogError("[NightController] サービスが初期化されていません。");
                return;
            }

            // エンドステートを決定
            var endState = _endStateService.CalculateEndState();
            Debug.Log($"[NightController] 夜終了: {CurrentNightId}, EndState: {endState}");

            // セーブ
            SaveCurrentState(endState);

            // イベント発火
            OnNightEnded?.Invoke(_currentNightIndex, endState);

            // 次の夜へ
            if (_currentNightIndex < _nightDataList.Count - 1)
            {
                _currentNightIndex++;
                StartNight(_currentNightIndex);
            }
            else
            {
                // 全ての夜が終了
                Debug.Log("[NightController] 全ての夜が終了しました。");
                OnAllNightsCompleted?.Invoke();
            }
        }

        /// <summary>
        /// 現在の状態をセーブする
        /// </summary>
        private void SaveCurrentState(EndStateType endState)
        {
            if (_operatorSaveService == null || _flagService == null) return;

            // 永続フラグを取得
            var persistentFlags = _flagService.GetPersistentFlags();

            // 夜の結果を保存
            _operatorSaveService.SaveNightResult(CurrentNightId, endState, persistentFlags);
        }

        /// <summary>
        /// セーブデータからロードする
        /// </summary>
        private void LoadFromSave()
        {
            if (_operatorSaveService == null) return;

            _currentNightIndex = _operatorSaveService.GetCurrentNightIndex();
            Debug.Log($"[NightController] セーブデータからロード: Night {_currentNightIndex + 1}");

            StartNight(_currentNightIndex);
        }

        /// <summary>
        /// 新規ゲームを開始する（セーブデータをクリア）
        /// </summary>
        public void StartNewGame()
        {
            _operatorSaveService?.DeleteSave();
            _currentNightIndex = 0;
            StartNight(0);
        }

        /// <summary>
        /// 指定した夜にジャンプする（デバッグ用）
        /// </summary>
        public void DebugJumpToNight(int nightIndex)
        {
            Debug.Log($"[NightController] デバッグ: Night {nightIndex + 1} にジャンプ");
            StartNight(nightIndex);
        }
    }
}

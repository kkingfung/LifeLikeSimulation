#nullable enable
using System;

namespace LifeLike.Services.Clock
{
    /// <summary>
    /// ゲーム内時計サービスのインターフェース
    /// ゲーム内時刻の管理と派遣タイミングの記録を担当
    /// </summary>
    public interface IClockService
    {
        #region プロパティ

        /// <summary>
        /// 現在のゲーム内時刻（分単位）
        /// 例: 02:17 = 137 (2 * 60 + 17)
        /// </summary>
        int CurrentTimeMinutes { get; }

        /// <summary>
        /// フォーマットされた現在時刻（HH:MM形式）
        /// </summary>
        string FormattedTime { get; }

        /// <summary>
        /// シナリオの開始時刻（分単位）
        /// </summary>
        int StartTimeMinutes { get; }

        /// <summary>
        /// シナリオの終了時刻（分単位）
        /// </summary>
        int EndTimeMinutes { get; }

        /// <summary>
        /// 時計が動いているか
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 一時停止中か
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// 派遣が記録された時刻（記録されていない場合はnull）
        /// </summary>
        int? DispatchTimeMinutes { get; }

        /// <summary>
        /// 派遣が記録されているか
        /// </summary>
        bool HasDispatched { get; }

        #endregion

        #region 初期化

        /// <summary>
        /// 時計を初期化する
        /// </summary>
        /// <param name="startTimeMinutes">開始時刻（分単位）</param>
        /// <param name="endTimeMinutes">終了時刻（分単位）</param>
        /// <param name="realSecondsPerGameMinute">1ゲーム内分あたりの実時間（秒）</param>
        void Initialize(int startTimeMinutes, int endTimeMinutes, float realSecondsPerGameMinute = 2f);

        #endregion

        #region 時間操作

        /// <summary>
        /// 時計を開始する
        /// </summary>
        void Start();

        /// <summary>
        /// 時計を一時停止する
        /// </summary>
        void Pause();

        /// <summary>
        /// 時計を再開する
        /// </summary>
        void Resume();

        /// <summary>
        /// 時計を停止する
        /// </summary>
        void Stop();

        /// <summary>
        /// 時刻を設定する
        /// </summary>
        /// <param name="timeMinutes">設定する時刻（分単位）</param>
        void SetTime(int timeMinutes);

        /// <summary>
        /// 時刻を進める
        /// </summary>
        /// <param name="minutes">進める分数</param>
        void AdvanceTime(int minutes);

        /// <summary>
        /// フレーム更新（MonoBehaviourから呼び出す）
        /// </summary>
        /// <param name="deltaTime">前フレームからの経過時間</param>
        void Update(float deltaTime);

        #endregion

        #region 派遣記録

        /// <summary>
        /// 現在時刻で派遣を記録する
        /// </summary>
        void RecordDispatch();

        /// <summary>
        /// 指定時刻で派遣を記録する
        /// </summary>
        /// <param name="timeMinutes">派遣時刻（分単位）</param>
        void RecordDispatchAt(int timeMinutes);

        /// <summary>
        /// 派遣記録をクリアする
        /// </summary>
        void ClearDispatchRecord();

        #endregion

        #region ユーティリティ

        /// <summary>
        /// 分単位の時刻をHH:MM形式にフォーマットする
        /// </summary>
        /// <param name="timeMinutes">時刻（分単位）</param>
        /// <returns>フォーマットされた時刻文字列</returns>
        string FormatTime(int timeMinutes);

        /// <summary>
        /// HH:MM形式の時刻を分単位に変換する
        /// </summary>
        /// <param name="timeString">時刻文字列（HH:MM形式）</param>
        /// <returns>分単位の時刻</returns>
        int ParseTime(string timeString);

        /// <summary>
        /// シナリオが終了時刻に達したかチェック
        /// </summary>
        bool IsTimeUp { get; }

        /// <summary>
        /// 残り時間（分単位）
        /// </summary>
        int RemainingMinutes { get; }

        #endregion

        #region イベント

        /// <summary>
        /// 時刻が変更されたときのイベント
        /// </summary>
        event Action<int, string>? OnTimeChanged;

        /// <summary>
        /// 派遣が記録されたときのイベント
        /// </summary>
        event Action<int>? OnDispatchRecorded;

        /// <summary>
        /// シナリオ終了時刻に達したときのイベント
        /// </summary>
        event Action? OnTimeUp;

        /// <summary>
        /// 時計が開始されたときのイベント
        /// </summary>
        event Action? OnClockStarted;

        /// <summary>
        /// 時計が停止されたときのイベント
        /// </summary>
        event Action? OnClockStopped;

        #endregion
    }
}

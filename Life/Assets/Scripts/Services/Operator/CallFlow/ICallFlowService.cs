#nullable enable
using System;
using System.Collections.Generic;
using LifeLike.Data;

namespace LifeLike.Services.Operator.CallFlow
{
    /// <summary>
    /// 通話フロー管理サービスのインターフェース
    /// </summary>
    public interface ICallFlowService
    {
        /// <summary>
        /// 現在アクティブな通話
        /// </summary>
        CallData? CurrentCall { get; }

        /// <summary>
        /// 現在のセグメント
        /// </summary>
        CallSegment? CurrentSegment { get; }

        /// <summary>
        /// 着信中の通話リスト
        /// </summary>
        IReadOnlyList<CallData> IncomingCalls { get; }

        /// <summary>
        /// 保留中の通話リスト
        /// </summary>
        IReadOnlyList<CallData> OnHoldCalls { get; }

        /// <summary>
        /// 通話履歴
        /// </summary>
        IReadOnlyList<CallData> CallHistory { get; }

        /// <summary>
        /// 新しい着信が発生した時のイベント
        /// </summary>
        event Action<CallData>? OnIncomingCall;

        /// <summary>
        /// 通話が開始された時のイベント
        /// </summary>
        event Action<CallData>? OnCallStarted;

        /// <summary>
        /// セグメントが変更された時のイベント
        /// </summary>
        event Action<CallSegment>? OnSegmentChanged;

        /// <summary>
        /// 応答選択肢が表示される時のイベント
        /// </summary>
        event Action<IReadOnlyList<ResponseData>>? OnResponsesPresented;

        /// <summary>
        /// 応答が選択された時のイベント
        /// </summary>
        event Action<ResponseData>? OnResponseSelected;

        /// <summary>
        /// 通話が終了した時のイベント
        /// </summary>
        event Action<CallData, CallState>? OnCallEnded;

        /// <summary>
        /// 不在着信が発生した時のイベント
        /// </summary>
        event Action<CallData>? OnCallMissed;

        /// <summary>
        /// シナリオを読み込む
        /// </summary>
        void LoadScenario(NightScenarioData scenario);

        /// <summary>
        /// 着信を追加（時間管理から呼ばれる）
        /// </summary>
        void AddIncomingCall(CallData call);

        /// <summary>
        /// 着信に応答
        /// </summary>
        bool AnswerCall(string callId);

        /// <summary>
        /// 通話を保留
        /// </summary>
        bool HoldCall();

        /// <summary>
        /// 保留解除
        /// </summary>
        bool ResumeCall(string callId);

        /// <summary>
        /// 通話を終了
        /// </summary>
        void EndCall();

        /// <summary>
        /// 応答を選択
        /// </summary>
        void SelectResponse(string responseId);

        /// <summary>
        /// 沈黙を選択
        /// </summary>
        void SelectSilence();

        /// <summary>
        /// 現在の応答選択肢を取得
        /// </summary>
        IReadOnlyList<ResponseData> GetAvailableResponses();

        /// <summary>
        /// 指定の応答が選択可能かチェック
        /// </summary>
        bool IsResponseAvailable(ResponseData response);

        /// <summary>
        /// 不在着信の数を取得
        /// </summary>
        int GetMissedCallCount();

        /// <summary>
        /// 全てクリア
        /// </summary>
        void Clear();
    }
}

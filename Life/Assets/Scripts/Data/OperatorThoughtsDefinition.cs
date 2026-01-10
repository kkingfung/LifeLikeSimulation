#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data
{
    /// <summary>
    /// 夜ごとのオペレーター思考定義
    /// </summary>
    [CreateAssetMenu(fileName = "OperatorThoughts", menuName = "LifeLike/Operator/Operator Thoughts")]
    public class OperatorThoughtsDefinition : ScriptableObject
    {
        [Header("基本情報")]
        [Tooltip("夜のID")]
        public string nightId = string.Empty;

        [Tooltip("説明")]
        [TextArea(2, 4)]
        public string description = string.Empty;

        [Header("思考リスト")]
        [Tooltip("この夜のオペレーター思考リスト")]
        public List<OperatorThought> thoughts = new();

        /// <summary>
        /// 指定IDの思考を取得
        /// </summary>
        public OperatorThought? GetThought(string thoughtId)
        {
            return thoughts.Find(t => t.thoughtId == thoughtId);
        }

        /// <summary>
        /// 指定通話終了後に表示する思考を取得
        /// </summary>
        public List<OperatorThought> GetThoughtsAfterCall(string callId)
        {
            return thoughts.FindAll(t =>
                t.timing == ThoughtTiming.AfterCall &&
                t.relatedCallId == callId);
        }

        /// <summary>
        /// 指定セグメント終了後に表示する思考を取得
        /// </summary>
        public List<OperatorThought> GetThoughtsAfterSegment(string callId, string segmentId)
        {
            return thoughts.FindAll(t =>
                t.timing == ThoughtTiming.AfterSegment &&
                t.relatedCallId == callId &&
                t.triggerSegmentId == segmentId);
        }

        /// <summary>
        /// 夜の終了時に表示する思考を取得
        /// </summary>
        public List<OperatorThought> GetEndOfNightThoughts()
        {
            return thoughts.FindAll(t => t.timing == ThoughtTiming.EndOfNight);
        }

        /// <summary>
        /// 指定フラグでトリガーされる思考を取得
        /// </summary>
        public List<OperatorThought> GetThoughtsByTriggerFlag(string flagId)
        {
            return thoughts.FindAll(t =>
                t.timing == ThoughtTiming.OnFlagSet &&
                t.triggerFlagId == flagId);
        }

        /// <summary>
        /// 指定時刻に表示する思考を取得
        /// </summary>
        public List<OperatorThought> GetThoughtsAtTime(int timeMinutes)
        {
            return thoughts.FindAll(t =>
                t.timing == ThoughtTiming.AtTime &&
                t.triggerTimeMinutes == timeMinutes);
        }

        /// <summary>
        /// 優先度でソートされた思考リストを取得
        /// </summary>
        public List<OperatorThought> GetSortedThoughts()
        {
            var sorted = new List<OperatorThought>(thoughts);
            sorted.Sort((a, b) => b.priority.CompareTo(a.priority)); // 高い優先度が先
            return sorted;
        }
    }
}

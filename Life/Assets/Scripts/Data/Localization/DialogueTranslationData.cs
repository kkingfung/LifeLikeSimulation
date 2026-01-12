#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// セリフの翻訳データ（JSON用の簡易形式）
    /// </summary>
    [Serializable]
    public class LineTranslation
    {
        /// <summary>日本語（オリジナル）</summary>
        public string ja = string.Empty;

        /// <summary>英語</summary>
        public string en = string.Empty;

        /// <summary>簡体中文</summary>
        public string zh_CN = string.Empty;

        /// <summary>繁体中文</summary>
        public string zh_TW = string.Empty;

        /// <summary>韓国語</summary>
        public string ko = string.Empty;

        /// <summary>
        /// 指定言語のテキストを取得（フォールバック付き）
        /// </summary>
        public string GetText(Language language)
        {
            var text = language switch
            {
                Language.Japanese => ja,
                Language.English => en,
                Language.ChineseSimplified => zh_CN,
                Language.ChineseTraditional => zh_TW,
                Language.Korean => ko,
                _ => en
            };

            // 空の場合は日本語にフォールバック
            if (string.IsNullOrEmpty(text)) text = ja;
            // それでも空なら英語
            if (string.IsNullOrEmpty(text)) text = en;

            return text;
        }

        /// <summary>
        /// LocalizedStringに変換
        /// </summary>
        public LocalizedString ToLocalizedString()
        {
            var localized = new LocalizedString { defaultLanguage = Language.Japanese };

            if (!string.IsNullOrEmpty(ja))
                localized.SetText(Language.Japanese, ja);
            if (!string.IsNullOrEmpty(en))
                localized.SetText(Language.English, en);
            if (!string.IsNullOrEmpty(zh_CN))
                localized.SetText(Language.ChineseSimplified, zh_CN);
            if (!string.IsNullOrEmpty(zh_TW))
                localized.SetText(Language.ChineseTraditional, zh_TW);
            if (!string.IsNullOrEmpty(ko))
                localized.SetText(Language.Korean, ko);

            return localized;
        }

        /// <summary>
        /// 日本語テキストのみで作成
        /// </summary>
        public static LineTranslation FromJapanese(string japaneseText)
        {
            return new LineTranslation { ja = japaneseText };
        }
    }

    /// <summary>
    /// セグメントの翻訳データ
    /// </summary>
    [Serializable]
    public class SegmentTranslation
    {
        /// <summary>セグメントID</summary>
        public string segmentId = string.Empty;

        /// <summary>発信者のセリフ（複数行）</summary>
        public List<LineTranslation> callerLines = new();

        /// <summary>応答選択肢の翻訳（responseId -> 翻訳）</summary>
        public List<ResponseTranslation> responses = new();
    }

    /// <summary>
    /// 応答選択肢の翻訳データ
    /// </summary>
    [Serializable]
    public class ResponseTranslation
    {
        /// <summary>応答ID</summary>
        public string responseId = string.Empty;

        /// <summary>表示テキスト</summary>
        public LineTranslation displayText = new();

        /// <summary>実際の発言（異なる場合）</summary>
        public LineTranslation? actualText;
    }

    /// <summary>
    /// 通話の翻訳データ
    /// </summary>
    [Serializable]
    public class CallTranslation
    {
        /// <summary>通話ID</summary>
        public string callId = string.Empty;

        /// <summary>タイトル</summary>
        public LineTranslation title = new();

        /// <summary>説明</summary>
        public LineTranslation description = new();

        /// <summary>セグメントの翻訳</summary>
        public List<SegmentTranslation> segments = new();

        /// <summary>
        /// セグメントIDで翻訳を取得
        /// </summary>
        public SegmentTranslation? GetSegmentTranslation(string segmentId)
        {
            return segments.Find(s => s.segmentId == segmentId);
        }
    }

    /// <summary>
    /// 発信者の翻訳データ
    /// </summary>
    [Serializable]
    public class CallerTranslation
    {
        /// <summary>発信者ID</summary>
        public string callerId = string.Empty;

        /// <summary>表示名</summary>
        public LineTranslation displayName = new();

        /// <summary>本名（判明後）</summary>
        public LineTranslation? realName;

        /// <summary>プロフィール</summary>
        public LineTranslation? profile;
    }

    /// <summary>
    /// 証拠の翻訳データ
    /// </summary>
    [Serializable]
    public class EvidenceTranslation
    {
        /// <summary>証拠ID</summary>
        public string evidenceId = string.Empty;

        /// <summary>内容</summary>
        public LineTranslation content = new();

        /// <summary>説明</summary>
        public LineTranslation? description;
    }

    /// <summary>
    /// オペレーターの思考の翻訳データ
    /// </summary>
    [Serializable]
    public class ThoughtTranslation
    {
        /// <summary>思考ID</summary>
        public string thoughtId = string.Empty;

        /// <summary>内容</summary>
        public LineTranslation content = new();
    }

    /// <summary>
    /// エンディングの翻訳データ
    /// </summary>
    [Serializable]
    public class EndingTranslation
    {
        /// <summary>エンディングID</summary>
        public string endingId = string.Empty;

        /// <summary>タイトル</summary>
        public LineTranslation title = new();

        /// <summary>説明</summary>
        public LineTranslation description = new();
    }

    /// <summary>
    /// 一夜分の翻訳データ
    /// </summary>
    [Serializable]
    public class NightTranslationData
    {
        /// <summary>シナリオID（例: "night01"）</summary>
        public string scenarioId = string.Empty;

        /// <summary>シナリオタイトル</summary>
        public LineTranslation title = new();

        /// <summary>シナリオ説明</summary>
        public LineTranslation description = new();

        /// <summary>発信者の翻訳</summary>
        public List<CallerTranslation> callers = new();

        /// <summary>通話の翻訳</summary>
        public List<CallTranslation> calls = new();

        /// <summary>証拠の翻訳</summary>
        public List<EvidenceTranslation> evidence = new();

        /// <summary>思考の翻訳</summary>
        public List<ThoughtTranslation> thoughts = new();

        /// <summary>エンディングの翻訳</summary>
        public List<EndingTranslation> endings = new();

        /// <summary>
        /// 通話IDで翻訳を取得
        /// </summary>
        public CallTranslation? GetCallTranslation(string callId)
        {
            return calls.Find(c => c.callId == callId);
        }

        /// <summary>
        /// 発信者IDで翻訳を取得
        /// </summary>
        public CallerTranslation? GetCallerTranslation(string callerId)
        {
            return callers.Find(c => c.callerId == callerId);
        }

        /// <summary>
        /// 証拠IDで翻訳を取得
        /// </summary>
        public EvidenceTranslation? GetEvidenceTranslation(string evidenceId)
        {
            return evidence.Find(e => e.evidenceId == evidenceId);
        }
    }

    /// <summary>
    /// 翻訳データのルートコンテナ（全夜分）
    /// </summary>
    [Serializable]
    public class DialogueTranslationDatabase
    {
        /// <summary>各夜の翻訳データ</summary>
        public List<NightTranslationData> nights = new();

        /// <summary>
        /// シナリオIDで夜の翻訳を取得
        /// </summary>
        public NightTranslationData? GetNightTranslation(string scenarioId)
        {
            return nights.Find(n => n.scenarioId == scenarioId);
        }
    }
}

#nullable enable
using System.Collections.Generic;

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// UIローカライズデータを提供するクラス
    /// コードベースでローカライズデータを定義
    /// </summary>
    public static class UILocalizationData
    {
        /// <summary>
        /// 全てのUIローカライズエントリを取得
        /// </summary>
        public static Dictionary<string, LocalizedString> GetAllEntries()
        {
            var entries = new Dictionary<string, LocalizedString>();

            // Settings画面
            AddEntry(entries, UILocalizationKeys.Settings.Title, "SETTINGS", "SETTINGS");
            AddEntry(entries, UILocalizationKeys.Settings.MasterVolume, "マスター音量", "Master Volume");
            AddEntry(entries, UILocalizationKeys.Settings.BgmVolume, "BGM音量", "BGM Volume");
            AddEntry(entries, UILocalizationKeys.Settings.SfxVolume, "効果音量", "SFX Volume");
            AddEntry(entries, UILocalizationKeys.Settings.VoiceVolume, "ボイス音量", "Voice Volume");
            AddEntry(entries, UILocalizationKeys.Settings.MuteAll, "全ミュート", "Mute All");
            AddEntry(entries, UILocalizationKeys.Settings.Fullscreen, "フルスクリーン", "Fullscreen");
            AddEntry(entries, UILocalizationKeys.Settings.Resolution, "解像度", "Resolution");
            AddEntry(entries, UILocalizationKeys.Settings.Quality, "品質", "Quality");
            AddEntry(entries, UILocalizationKeys.Settings.TextSpeed, "テキスト速度", "Text Speed");
            AddEntry(entries, UILocalizationKeys.Settings.AutoAdvance, "オートモード", "Auto Advance");
            AddEntry(entries, UILocalizationKeys.Settings.AutoAdvanceDelay, "オート待ち時間", "Auto Advance Delay");
            AddEntry(entries, UILocalizationKeys.Settings.SkipUnread, "未読スキップ", "Skip Unread");
            AddEntry(entries, UILocalizationKeys.Settings.Language, "言語", "Language");
            AddEntry(entries, UILocalizationKeys.Settings.Back, "戻る", "Back");
            AddEntry(entries, UILocalizationKeys.Settings.ResetToDefault, "初期設定に戻す", "Reset to Default");

            // 共通ボタン
            AddEntry(entries, UILocalizationKeys.Common.Back, "戻る", "Back");
            AddEntry(entries, UILocalizationKeys.Common.Confirm, "確認", "Confirm");
            AddEntry(entries, UILocalizationKeys.Common.Cancel, "キャンセル", "Cancel");
            AddEntry(entries, UILocalizationKeys.Common.Yes, "はい", "Yes");
            AddEntry(entries, UILocalizationKeys.Common.No, "いいえ", "No");
            AddEntry(entries, UILocalizationKeys.Common.Ok, "OK", "OK");
            AddEntry(entries, UILocalizationKeys.Common.Apply, "適用", "Apply");
            AddEntry(entries, UILocalizationKeys.Common.Reset, "リセット", "Reset");
            AddEntry(entries, UILocalizationKeys.Common.Close, "閉じる", "Close");

            // メインメニュー
            AddEntry(entries, UILocalizationKeys.MainMenu.Title, "OPERATOR: NIGHT SIGNAL", "OPERATOR: NIGHT SIGNAL");
            AddEntry(entries, UILocalizationKeys.MainMenu.NewGame, "新規ゲーム", "New Game");
            AddEntry(entries, UILocalizationKeys.MainMenu.Continue, "続きから", "Continue");
            AddEntry(entries, UILocalizationKeys.MainMenu.Settings, "設定", "Settings");
            AddEntry(entries, UILocalizationKeys.MainMenu.Quit, "終了", "Quit");

            // チャプター選択
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Title, "夜を選択", "Select Night");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Night, "第{0}夜", "Night {0}");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Locked, "ロック中", "Locked");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Completed, "クリア済み", "Completed");

            // 結果画面
            AddEntry(entries, UILocalizationKeys.Result.Title, "結果", "Result");
            AddEntry(entries, UILocalizationKeys.Result.NextNight, "次の夜へ", "Next Night");
            AddEntry(entries, UILocalizationKeys.Result.ReturnToMenu, "メニューに戻る", "Return to Menu");
            AddEntry(entries, UILocalizationKeys.Result.Replay, "リプレイ", "Replay");

            // オペレーター画面
            AddEntry(entries, UILocalizationKeys.Operator.Settings, "設定", "Settings");
            AddEntry(entries, UILocalizationKeys.Operator.ChapterSelect, "夜選択", "Chapter Select");
            AddEntry(entries, UILocalizationKeys.Operator.Hold, "保留", "Hold");
            AddEntry(entries, UILocalizationKeys.Operator.EndCall, "切断", "End Call");
            AddEntry(entries, UILocalizationKeys.Operator.Pause, "一時停止", "Pause");
            AddEntry(entries, UILocalizationKeys.Operator.Resume, "再開", "Resume");
            AddEntry(entries, UILocalizationKeys.Operator.Silence, "沈黙", "Silence");
            AddEntry(entries, UILocalizationKeys.Operator.ReturnToMenu, "メニューに戻る", "Return to Menu");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Active, "通話中", "On Call");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Incoming, "着信あり", "Incoming Call");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Waiting, "待機中", "Waiting");
            AddEntry(entries, UILocalizationKeys.Operator.UnknownCaller, "不明な発信者", "Unknown Caller");

            return entries;
        }

        /// <summary>
        /// エントリを追加するヘルパーメソッド
        /// </summary>
        private static void AddEntry(Dictionary<string, LocalizedString> entries, string key, string japanese, string english)
        {
            var localizedString = new LocalizedString
            {
                key = key,
                defaultLanguage = Language.Japanese
            };
            localizedString.SetText(Language.Japanese, japanese);
            localizedString.SetText(Language.English, english);
            entries[key] = localizedString;
        }
    }
}

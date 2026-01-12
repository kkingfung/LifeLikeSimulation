#nullable enable

namespace LifeLike.Data.Localization
{
    /// <summary>
    /// UIローカライズ用のキー定義
    /// </summary>
    public static class UILocalizationKeys
    {
        // Settings画面
        public static class Settings
        {
            public const string Title = "ui.settings.title";
            public const string MasterVolume = "ui.settings.master_volume";
            public const string BgmVolume = "ui.settings.bgm_volume";
            public const string SfxVolume = "ui.settings.sfx_volume";
            public const string VoiceVolume = "ui.settings.voice_volume";
            public const string MuteAll = "ui.settings.mute_all";
            public const string Fullscreen = "ui.settings.fullscreen";
            public const string Resolution = "ui.settings.resolution";
            public const string Quality = "ui.settings.quality";
            public const string TextSpeed = "ui.settings.text_speed";
            public const string AutoAdvance = "ui.settings.auto_advance";
            public const string AutoAdvanceDelay = "ui.settings.auto_advance_delay";
            public const string SkipUnread = "ui.settings.skip_unread";
            public const string Language = "ui.settings.language";
            public const string Back = "ui.settings.back";
            public const string ResetToDefault = "ui.settings.reset_to_default";
        }

        // 共通ボタン
        public static class Common
        {
            public const string Back = "ui.common.back";
            public const string Confirm = "ui.common.confirm";
            public const string Cancel = "ui.common.cancel";
            public const string Yes = "ui.common.yes";
            public const string No = "ui.common.no";
            public const string Ok = "ui.common.ok";
            public const string Apply = "ui.common.apply";
            public const string Reset = "ui.common.reset";
            public const string Close = "ui.common.close";
        }

        // メインメニュー
        public static class MainMenu
        {
            public const string Title = "ui.mainmenu.title";
            public const string NewGame = "ui.mainmenu.new_game";
            public const string Continue = "ui.mainmenu.continue";
            public const string Settings = "ui.mainmenu.settings";
            public const string Quit = "ui.mainmenu.quit";
        }

        // チャプター選択
        public static class ChapterSelect
        {
            public const string Title = "ui.chapter_select.title";
            public const string Night = "ui.chapter_select.night";
            public const string Locked = "ui.chapter_select.locked";
            public const string Completed = "ui.chapter_select.completed";
            public const string Available = "ui.chapter_select.available";
            public const string InProgress = "ui.chapter_select.in_progress";
            public const string Start = "ui.chapter_select.start";
            public const string Resume = "ui.chapter_select.resume";
        }

        // 結果画面
        public static class Result
        {
            public const string Title = "ui.result.title";
            public const string NextNight = "ui.result.next_night";
            public const string ReturnToMenu = "ui.result.return_to_menu";
            public const string Replay = "ui.result.replay";
        }

        // オペレーター画面
        public static class Operator
        {
            public const string Settings = "ui.operator.settings";
            public const string ChapterSelect = "ui.operator.chapter_select";
            public const string Hold = "ui.operator.hold";
            public const string EndCall = "ui.operator.end_call";
            public const string Pause = "ui.operator.pause";
            public const string Resume = "ui.operator.resume";
            public const string Silence = "ui.operator.silence";
            public const string ReturnToMenu = "ui.operator.return_to_menu";
            public const string CallStatus_Active = "ui.operator.call_status.active";
            public const string CallStatus_Incoming = "ui.operator.call_status.incoming";
            public const string CallStatus_Waiting = "ui.operator.call_status.waiting";
            public const string UnknownCaller = "ui.operator.unknown_caller";
        }
    }
}

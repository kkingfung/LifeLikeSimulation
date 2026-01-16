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
            public const string LastSave = "ui.mainmenu.last_save";
            public const string CurrentNight = "ui.mainmenu.current_night";
            public const string AllCleared = "ui.mainmenu.all_cleared";
            public const string MidNightSave = "ui.mainmenu.mid_night_save";
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
            public const string ResultPrefix = "ui.chapter_select.result_prefix";
        }

        // 結果画面
        public static class Result
        {
            public const string Title = "ui.result.title";
            public const string NextNight = "ui.result.next_night";
            public const string ReturnToMenu = "ui.result.return_to_menu";
            public const string Replay = "ui.result.replay";
            public const string Incomplete = "ui.result.incomplete";
            public const string IncompleteDesc = "ui.result.incomplete_desc";
            public const string DefaultEnding = "ui.result.default_ending";
            public const string DefaultEndingDesc = "ui.result.default_ending_desc";

            // Final endings
            public const string TruthDawn = "ui.result.ending.truth_dawn";
            public const string TruthDawnDesc = "ui.result.ending.truth_dawn_desc";
            public const string InvestigationContinues = "ui.result.ending.investigation_continues";
            public const string InvestigationContinuesDesc = "ui.result.ending.investigation_continues_desc";
            public const string IntoDarkness = "ui.result.ending.into_darkness";
            public const string IntoDarknessDesc = "ui.result.ending.into_darkness_desc";
            public const string UncertainDawn = "ui.result.ending.uncertain_dawn";
            public const string UncertainDawnDesc = "ui.result.ending.uncertain_dawn_desc";

            // Night endings (per night)
            // Night01
            public const string Contained = "ui.result.ending.contained";
            public const string Exposed = "ui.result.ending.exposed";
            public const string Complicit = "ui.result.ending.complicit";
            public const string Flagged = "ui.result.ending.flagged";
            public const string Absorbed = "ui.result.ending.absorbed";
            // Night02
            public const string Vigilant = "ui.result.ending.vigilant";
            public const string Compliant = "ui.result.ending.compliant";
            public const string Connected = "ui.result.ending.connected";
            public const string Isolated = "ui.result.ending.isolated";
            public const string Routine = "ui.result.ending.routine";
            // Night03
            public const string Crossroads = "ui.result.ending.crossroads";
            public const string Intervention = "ui.result.ending.intervention";
            public const string Disclosure = "ui.result.ending.disclosure";
            public const string Silence = "ui.result.ending.silence";
            // Night04
            public const string WitnessConnected = "ui.result.ending.witness_connected";
            public const string WitnessOnly = "ui.result.ending.witness_only";
            public const string ConnectedOnly = "ui.result.ending.connected_only";
            public const string Neither = "ui.result.ending.neither";
            // Night05
            public const string VoiceReached = "ui.result.ending.voice_reached";
            public const string VoiceDistant = "ui.result.ending.voice_distant";
            public const string VoiceLost = "ui.result.ending.voice_lost";
            // Night06
            public const string StormPrepared = "ui.result.ending.storm_prepared";
            public const string StormAware = "ui.result.ending.storm_aware";
            public const string StormDistant = "ui.result.ending.storm_distant";
            public const string StormUnaware = "ui.result.ending.storm_unaware";
            // Night07
            public const string MisakiProtected = "ui.result.ending.misaki_protected";
            public const string MisakiSafeUnaware = "ui.result.ending.misaki_safe_unaware";
            public const string MisakiTaken = "ui.result.ending.misaki_taken";
            public const string CollapseWitnessed = "ui.result.ending.collapse_witnessed";
            // Night08
            public const string TruthSeeker = "ui.result.ending.truth_seeker";
            public const string InformedCaution = "ui.result.ending.informed_caution";
            public const string SilentWitness = "ui.result.ending.silent_witness";
            public const string UnawareSurvivor = "ui.result.ending.unaware_survivor";
            // Night09
            public const string FullAlliance = "ui.result.ending.full_alliance";
            public const string ActiveAlliance = "ui.result.ending.active_alliance";
            public const string PassiveTruth = "ui.result.ending.passive_truth";
            public const string WhistleblowerSaved = "ui.result.ending.whistleblower_saved";
            public const string WhistleblowerEndangered = "ui.result.ending.whistleblower_endangered";
            public const string MisakiDiscovered = "ui.result.ending.misaki_discovered";
            public const string TruthRevealed = "ui.result.ending.truth_revealed";
            public const string UncertainFuture = "ui.result.ending.uncertain_future";

            // Route summary
            public const string YourRoute = "ui.result.your_route";
            public const string RouteTruth = "ui.result.route_truth";
            public const string RouteDarkness = "ui.result.route_darkness";
            public const string RouteLight = "ui.result.route_light";
            public const string RouteShadow = "ui.result.route_shadow";
            public const string RouteNeutral = "ui.result.route_neutral";
            public const string BestChoices = "ui.result.best_choices";
            public const string NeutralChoices = "ui.result.neutral_choices";
            public const string BadChoices = "ui.result.bad_choices";
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
            public const string PrivateNumber = "ui.operator.private_number";
        }
    }
}

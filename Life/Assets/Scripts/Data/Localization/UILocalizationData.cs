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
            AddEntry(entries, UILocalizationKeys.Settings.Title, "SETTINGS", "SETTINGS", "设置", "設定", "설정");
            AddEntry(entries, UILocalizationKeys.Settings.MasterVolume, "マスター音量", "Master Volume", "主音量", "主音量", "마스터 볼륨");
            AddEntry(entries, UILocalizationKeys.Settings.BgmVolume, "BGM音量", "BGM Volume", "背景音乐音量", "背景音樂音量", "BGM 볼륨");
            AddEntry(entries, UILocalizationKeys.Settings.SfxVolume, "効果音量", "SFX Volume", "音效音量", "音效音量", "효과음 볼륨");
            AddEntry(entries, UILocalizationKeys.Settings.VoiceVolume, "ボイス音量", "Voice Volume", "语音音量", "語音音量", "음성 볼륨");
            AddEntry(entries, UILocalizationKeys.Settings.MuteAll, "全ミュート", "Mute All", "全部静音", "全部靜音", "전체 음소거");
            AddEntry(entries, UILocalizationKeys.Settings.Fullscreen, "フルスクリーン", "Fullscreen", "全屏", "全螢幕", "전체 화면");
            AddEntry(entries, UILocalizationKeys.Settings.Resolution, "解像度", "Resolution", "分辨率", "解析度", "해상도");
            AddEntry(entries, UILocalizationKeys.Settings.Quality, "品質", "Quality", "画质", "畫質", "품질");
            AddEntry(entries, UILocalizationKeys.Settings.TextSpeed, "テキスト速度", "Text Speed", "文字速度", "文字速度", "텍스트 속도");
            AddEntry(entries, UILocalizationKeys.Settings.AutoAdvance, "オートモード", "Auto Advance", "自动前进", "自動前進", "자동 진행");
            AddEntry(entries, UILocalizationKeys.Settings.AutoAdvanceDelay, "オート待ち時間", "Auto Advance Delay", "自动前进延迟", "自動前進延遲", "자동 진행 지연");
            AddEntry(entries, UILocalizationKeys.Settings.SkipUnread, "未読スキップ", "Skip Unread", "跳过未读", "跳過未讀", "읽지 않은 내용 건너뛰기");
            AddEntry(entries, UILocalizationKeys.Settings.Language, "言語", "Language", "语言", "語言", "언어");
            AddEntry(entries, UILocalizationKeys.Settings.Back, "戻る", "Back", "返回", "返回", "뒤로");
            AddEntry(entries, UILocalizationKeys.Settings.ResetToDefault, "初期設定に戻す", "Reset to Default", "恢复默认设置", "恢復預設設定", "기본값으로 재설정");

            // 共通ボタン
            AddEntry(entries, UILocalizationKeys.Common.Back, "戻る", "Back", "返回", "返回", "뒤로");
            AddEntry(entries, UILocalizationKeys.Common.Confirm, "確認", "Confirm", "确认", "確認", "확인");
            AddEntry(entries, UILocalizationKeys.Common.Cancel, "キャンセル", "Cancel", "取消", "取消", "취소");
            AddEntry(entries, UILocalizationKeys.Common.Yes, "はい", "Yes", "是", "是", "예");
            AddEntry(entries, UILocalizationKeys.Common.No, "いいえ", "No", "否", "否", "아니요");
            AddEntry(entries, UILocalizationKeys.Common.Ok, "OK", "OK", "确定", "確定", "확인");
            AddEntry(entries, UILocalizationKeys.Common.Apply, "適用", "Apply", "应用", "套用", "적용");
            AddEntry(entries, UILocalizationKeys.Common.Reset, "リセット", "Reset", "重置", "重置", "초기화");
            AddEntry(entries, UILocalizationKeys.Common.Close, "閉じる", "Close", "关闭", "關閉", "닫기");

            // メインメニュー
            AddEntry(entries, UILocalizationKeys.MainMenu.Title, "OPERATOR: NIGHT SIGNAL", "OPERATOR: NIGHT SIGNAL", "OPERATOR: NIGHT SIGNAL", "OPERATOR: NIGHT SIGNAL", "OPERATOR: NIGHT SIGNAL");
            AddEntry(entries, UILocalizationKeys.MainMenu.NewGame, "新規ゲーム", "New Game", "新游戏", "新遊戲", "새 게임");
            AddEntry(entries, UILocalizationKeys.MainMenu.Continue, "続きから", "Continue", "继续", "繼續", "계속하기");
            AddEntry(entries, UILocalizationKeys.MainMenu.Settings, "設定", "Settings", "设置", "設定", "설정");
            AddEntry(entries, UILocalizationKeys.MainMenu.Quit, "終了", "Quit", "退出", "退出", "종료");

            // チャプター選択
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Title, "夜を選択", "Select Night", "选择夜晚", "選擇夜晚", "밤 선택");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Night, "第{0}夜", "Night {0}", "第{0}夜", "第{0}夜", "{0}번째 밤");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Locked, "ロック中", "Locked", "未解锁", "未解鎖", "잠김");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Completed, "クリア済み", "Completed", "已完成", "已完成", "완료");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Available, "プレイ可能", "Available", "可游玩", "可遊玩", "플레이 가능");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.InProgress, "進行中", "In Progress", "进行中", "進行中", "진행 중");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Start, "開始する", "Start", "开始", "開始", "시작");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Resume, "再開する", "Resume", "继续", "繼續", "재개");

            // 結果画面
            AddEntry(entries, UILocalizationKeys.Result.Title, "結果", "Result", "结果", "結果", "결과");
            AddEntry(entries, UILocalizationKeys.Result.NextNight, "次の夜へ", "Next Night", "下一夜", "下一夜", "다음 밤");
            AddEntry(entries, UILocalizationKeys.Result.ReturnToMenu, "メニューに戻る", "Return to Menu", "返回菜单", "返回選單", "메뉴로 돌아가기");
            AddEntry(entries, UILocalizationKeys.Result.Replay, "リプレイ", "Replay", "重玩", "重玩", "다시 플레이");

            // オペレーター画面
            AddEntry(entries, UILocalizationKeys.Operator.Settings, "設定", "Settings", "设置", "設定", "설정");
            AddEntry(entries, UILocalizationKeys.Operator.ChapterSelect, "夜選択", "Chapter Select", "选择夜晚", "選擇夜晚", "밤 선택");
            AddEntry(entries, UILocalizationKeys.Operator.Hold, "保留", "Hold", "保持", "保留", "보류");
            AddEntry(entries, UILocalizationKeys.Operator.EndCall, "切断", "End Call", "挂断", "掛斷", "통화 종료");
            AddEntry(entries, UILocalizationKeys.Operator.Pause, "一時停止", "Pause", "暂停", "暫停", "일시 정지");
            AddEntry(entries, UILocalizationKeys.Operator.Resume, "再開", "Resume", "继续", "繼續", "재개");
            AddEntry(entries, UILocalizationKeys.Operator.Silence, "沈黙", "Silence", "沉默", "沉默", "침묵");
            AddEntry(entries, UILocalizationKeys.Operator.ReturnToMenu, "メニューに戻る", "Return to Menu", "返回菜单", "返回選單", "메뉴로 돌아가기");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Active, "通話中", "On Call", "通话中", "通話中", "통화 중");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Incoming, "着信あり", "Incoming Call", "来电", "來電", "수신 전화");
            AddEntry(entries, UILocalizationKeys.Operator.CallStatus_Waiting, "待機中", "Waiting", "等待中", "等待中", "대기 중");
            AddEntry(entries, UILocalizationKeys.Operator.UnknownCaller, "不明な発信者", "Unknown Caller", "未知来电", "未知來電", "알 수 없는 발신자");

            return entries;
        }

        /// <summary>
        /// エントリを追加するヘルパーメソッド（日本語、英語、簡体中文、繁体中文、韓国語）
        /// </summary>
        private static void AddEntry(
            Dictionary<string, LocalizedString> entries,
            string key,
            string japanese,
            string english,
            string chineseSimplified = "",
            string chineseTraditional = "",
            string korean = "")
        {
            var localizedString = new LocalizedString
            {
                key = key,
                defaultLanguage = Language.Japanese
            };
            localizedString.SetText(Language.Japanese, japanese);
            localizedString.SetText(Language.English, english);

            if (!string.IsNullOrEmpty(chineseSimplified))
            {
                localizedString.SetText(Language.ChineseSimplified, chineseSimplified);
            }
            if (!string.IsNullOrEmpty(chineseTraditional))
            {
                localizedString.SetText(Language.ChineseTraditional, chineseTraditional);
            }
            if (!string.IsNullOrEmpty(korean))
            {
                localizedString.SetText(Language.Korean, korean);
            }

            entries[key] = localizedString;
        }
    }
}

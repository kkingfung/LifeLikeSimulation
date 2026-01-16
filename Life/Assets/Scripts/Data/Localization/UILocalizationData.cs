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
            AddEntry(entries, UILocalizationKeys.MainMenu.LastSave, "最後のセーブ: {0}", "Last Save: {0}", "最后保存: {0}", "最後保存: {0}", "마지막 저장: {0}");
            AddEntry(entries, UILocalizationKeys.MainMenu.CurrentNight, "Night {0:D2} / 10", "Night {0:D2} / 10", "Night {0:D2} / 10", "Night {0:D2} / 10", "Night {0:D2} / 10");
            AddEntry(entries, UILocalizationKeys.MainMenu.AllCleared, "全クリア (Night 10 / 10)", "All Cleared (Night 10 / 10)", "全部通关 (Night 10 / 10)", "全部通關 (Night 10 / 10)", "올 클리어 (Night 10 / 10)");
            AddEntry(entries, UILocalizationKeys.MainMenu.MidNightSave, " (中断セーブあり)", " (Mid-save available)", " (有中途保存)", " (有中途保存)", " (중간 저장 있음)");

            // チャプター選択
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Title, "夜を選択", "Select Night", "选择夜晚", "選擇夜晚", "밤 선택");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Night, "第{0}夜", "Night {0}", "第{0}夜", "第{0}夜", "{0}번째 밤");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Locked, "ロック中", "Locked", "未解锁", "未解鎖", "잠김");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Completed, "クリア済み", "Completed", "已完成", "已完成", "완료");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Available, "プレイ可能", "Available", "可游玩", "可遊玩", "플레이 가능");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.InProgress, "進行中", "In Progress", "进行中", "進行中", "진행 중");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Start, "開始する", "Start", "开始", "開始", "시작");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.Resume, "再開する", "Resume", "继续", "繼續", "재개");
            AddEntry(entries, UILocalizationKeys.ChapterSelect.ResultPrefix, "結果:", "Result:", "结果:", "結果:", "결과:");

            // 結果画面
            AddEntry(entries, UILocalizationKeys.Result.Title, "結果", "Result", "结果", "結果", "결과");
            AddEntry(entries, UILocalizationKeys.Result.NextNight, "次の夜へ", "Next Night", "下一夜", "下一夜", "다음 밤");
            AddEntry(entries, UILocalizationKeys.Result.ReturnToMenu, "メニューに戻る", "Return to Menu", "返回菜单", "返回選單", "메뉴로 돌아가기");
            AddEntry(entries, UILocalizationKeys.Result.Replay, "リプレイ", "Replay", "重玩", "重玩", "다시 플레이");
            AddEntry(entries, UILocalizationKeys.Result.Incomplete, "未完了", "Incomplete", "未完成", "未完成", "미완료");
            AddEntry(entries, UILocalizationKeys.Result.IncompleteDesc, "全ての夜を完了してください。", "Please complete all nights.", "请完成所有夜晚。", "請完成所有夜晚。", "모든 밤을 완료해 주세요.");
            AddEntry(entries, UILocalizationKeys.Result.DefaultEnding, "終幕", "The End", "终幕", "終幕", "종막");
            AddEntry(entries, UILocalizationKeys.Result.DefaultEndingDesc, "物語は終わりを迎えた。", "The story has come to an end.", "故事已经结束。", "故事已經結束。", "이야기가 끝났습니다.");

                        // Final endings
            AddEntry(entries, UILocalizationKeys.Result.TruthDawn, "真実の夜明け", "Dawn of Truth", "真相的黎明", "真相的黎明", "진실의 새벽");
            AddEntry(entries, UILocalizationKeys.Result.TruthDawnDesc, "全ての真実が明らかになった。\nあなたの選択は、闇に光を灯した。\n夜明けは、新たな始まりを告げる。", "All truth has been revealed.\nYour choices brought light to the darkness.\nDawn heralds a new beginning.", "所有真相都已揭晓。\n你的选择为黑暗带来了光明。\n黎明预示着新的开始。", "所有真相都已揭曉。\n你的選擇為黑暗帶來了光明。\n黎明預示著新的開始。", "모든 진실이 밝혀졌습니다.\n당신의 선택이 어둠에 빛을 비추었습니다.\n새벽은 새로운 시작을 알립니다.");
            AddEntry(entries, UILocalizationKeys.Result.InvestigationContinues, "調査は続く", "Investigation Continues", "调查仍在继续", "調查仍在繼續", "조사는 계속된다");
            AddEntry(entries, UILocalizationKeys.Result.InvestigationContinuesDesc, "真実の一部は明らかになった。\nしかし、まだ多くの謎が残されている。\n調査は続く——いつか全てが明らかになる日まで。", "Part of the truth has been revealed.\nHowever, many mysteries remain.\nThe investigation continues—until the day all is revealed.", "部分真相已被揭露。\n然而，许多谜团仍然存在。\n调查还在继续——直到一切真相大白的那一天。", "部分真相已被揭露。\n然而，許多謎團仍然存在。\n調查還在繼續——直到一切真相大白的那一天。", "진실의 일부가 밝혀졌습니다.\n하지만 아직 많은 미스터리가 남아 있습니다.\n조사는 계속됩니다—모든 것이 밝혀지는 그날까지.");
            AddEntry(entries, UILocalizationKeys.Result.IntoDarkness, "闇の中へ", "Into Darkness", "坠入黑暗", "墜入黑暗", "어둠 속으로");
            AddEntry(entries, UILocalizationKeys.Result.IntoDarknessDesc, "真実は闇に葬られた。\nあなたの選択は、影を深くした。\n夜は、まだ終わらない。", "The truth was buried in darkness.\nYour choices deepened the shadows.\nThe night is not yet over.", "真相被埋葬在黑暗中。\n你的选择加深了阴影。\n夜晚还没有结束。", "真相被埋葬在黑暗中。\n你的選擇加深了陰影。\n夜晚還沒有結束。", "진실은 어둠 속에 묻혔습니다.\n당신의 선택이 그림자를 더 깊게 만들었습니다.\n밤은 아직 끝나지 않았습니다.");
            AddEntry(entries, UILocalizationKeys.Result.UncertainDawn, "不確かな夜明け", "Uncertain Dawn", "不确定的黎明", "不確定的黎明", "불확실한 새벽");
            AddEntry(entries, UILocalizationKeys.Result.UncertainDawnDesc, "夜は明けた。\nしかし、何が正しかったのかは分からない。\n不確かな夜明けの中、日常は続く。", "Dawn has broken.\nBut what was right remains unknown.\nIn this uncertain dawn, life goes on.", "夜已经过去。\n但什么是正确的仍然未知。\n在这不确定的黎明中，日常继续。", "夜已經過去。\n但什麼是正確的仍然未知。\n在這不確定的黎明中，日常繼續。", "밤이 지나갔습니다.\n하지만 무엇이 옳았는지는 알 수 없습니다.\n불확실한 새벽 속에서 일상은 계속됩니다.");

            // Night endings
            // Night01
            AddEntry(entries, UILocalizationKeys.Result.Contained, "封じ込め", "Contained", "封锁", "封鎖", "봉쇄");
            AddEntry(entries, UILocalizationKeys.Result.Exposed, "露出", "Exposed", "暴露", "暴露", "노출");
            AddEntry(entries, UILocalizationKeys.Result.Complicit, "共犯", "Complicit", "共犯", "共犯", "공범");
            AddEntry(entries, UILocalizationKeys.Result.Flagged, "要注意", "Flagged", "被标记", "被標記", "주의 대상");
            AddEntry(entries, UILocalizationKeys.Result.Absorbed, "吸収", "Absorbed", "被吸收", "被吸收", "흡수됨");
            // Night02
            AddEntry(entries, UILocalizationKeys.Result.Vigilant, "警戒", "Vigilant", "警惕", "警惕", "경계");
            AddEntry(entries, UILocalizationKeys.Result.Compliant, "従順", "Compliant", "顺从", "順從", "순응");
            AddEntry(entries, UILocalizationKeys.Result.Connected, "接続", "Connected", "连接", "連接", "연결됨");
            AddEntry(entries, UILocalizationKeys.Result.Isolated, "孤立", "Isolated", "孤立", "孤立", "고립");
            AddEntry(entries, UILocalizationKeys.Result.Routine, "日常", "Routine", "日常", "日常", "일상");
            // Night03
            AddEntry(entries, UILocalizationKeys.Result.Crossroads, "分かれ道", "Crossroads", "十字路口", "十字路口", "갈림길");
            AddEntry(entries, UILocalizationKeys.Result.Intervention, "介入", "Intervention", "介入", "介入", "개입");
            AddEntry(entries, UILocalizationKeys.Result.Disclosure, "開示", "Disclosure", "公开", "公開", "공개");
            AddEntry(entries, UILocalizationKeys.Result.Silence, "沈黙", "Silence", "沉默", "沉默", "침묵");
            // Night04
            AddEntry(entries, UILocalizationKeys.Result.WitnessConnected, "証人と接続", "Witness Connected", "与证人连接", "與證人連接", "증인과 연결");
            AddEntry(entries, UILocalizationKeys.Result.WitnessOnly, "証人", "Witness Only", "仅证人", "僅證人", "증인만");
            AddEntry(entries, UILocalizationKeys.Result.ConnectedOnly, "接続のみ", "Connected Only", "仅连接", "僅連接", "연결만");
            AddEntry(entries, UILocalizationKeys.Result.Neither, "どちらもなし", "Neither", "两者皆无", "兩者皆無", "둘 다 아님");
            // Night05
            AddEntry(entries, UILocalizationKeys.Result.VoiceReached, "届いた声", "Voice Reached", "声音传达", "聲音傳達", "닿은 목소리");
            AddEntry(entries, UILocalizationKeys.Result.VoiceDistant, "遠い声", "Voice Distant", "遥远的声音", "遙遠的聲音", "먼 목소리");
            AddEntry(entries, UILocalizationKeys.Result.VoiceLost, "消えた声", "Voice Lost", "消失的声音", "消失的聲音", "사라진 목소리");
            // Night06
            AddEntry(entries, UILocalizationKeys.Result.StormPrepared, "嵐への備え", "Storm Prepared", "风暴准备", "風暴準備", "폭풍 대비");
            AddEntry(entries, UILocalizationKeys.Result.StormAware, "嵐の予感", "Storm Aware", "风暴预感", "風暴預感", "폭풍 예감");
            AddEntry(entries, UILocalizationKeys.Result.StormDistant, "遠い雷鳴", "Storm Distant", "远处雷鸣", "遠處雷鳴", "먼 천둥");
            AddEntry(entries, UILocalizationKeys.Result.StormUnaware, "静かな午後", "Storm Unaware", "平静的午后", "平靜的午後", "고요한 오후");
            // Night07
            AddEntry(entries, UILocalizationKeys.Result.MisakiProtected, "小さな光", "Misaki Protected", "小小的光", "小小的光", "작은 빛");
            AddEntry(entries, UILocalizationKeys.Result.MisakiSafeUnaware, "守られた秘密", "Misaki Safe Unaware", "被守护的秘密", "被守護的秘密", "지켜진 비밀");
            AddEntry(entries, UILocalizationKeys.Result.MisakiTaken, "崩壊", "Misaki Taken", "崩溃", "崩潰", "붕괴");
            AddEntry(entries, UILocalizationKeys.Result.CollapseWitnessed, "崩壊の夜", "Collapse Witnessed", "崩溃之夜", "崩潰之夜", "붕괴의 밤");
            // Night08
            AddEntry(entries, UILocalizationKeys.Result.TruthSeeker, "真実を追う者", "Truth Seeker", "追寻真相者", "追尋真相者", "진실을 쫓는 자");
            AddEntry(entries, UILocalizationKeys.Result.InformedCaution, "慎重な知識", "Informed Caution", "谨慎的知识", "謹慎的知識", "신중한 지식");
            AddEntry(entries, UILocalizationKeys.Result.SilentWitness, "沈黙の証人", "Silent Witness", "沉默的证人", "沉默的證人", "침묵의 증인");
            AddEntry(entries, UILocalizationKeys.Result.UnawareSurvivor, "無知な生存者", "Unaware Survivor", "无知的幸存者", "無知的倖存者", "모르는 생존자");
            // Night09
            AddEntry(entries, UILocalizationKeys.Result.FullAlliance, "完全な同盟", "Full Alliance", "完全同盟", "完全同盟", "완전한 동맹");
            AddEntry(entries, UILocalizationKeys.Result.ActiveAlliance, "積極的な協力", "Active Alliance", "积极合作", "積極合作", "적극적인 협력");
            AddEntry(entries, UILocalizationKeys.Result.PassiveTruth, "真実を知った沈黙", "Passive Truth", "知晓真相的沉默", "知曉真相的沉默", "진실을 아는 침묵");
            AddEntry(entries, UILocalizationKeys.Result.WhistleblowerSaved, "告発者を救った", "Whistleblower Saved", "救了告发者", "救了告發者", "고발자를 구함");
            AddEntry(entries, UILocalizationKeys.Result.WhistleblowerEndangered, "告発者の危機", "Whistleblower Endangered", "告发者的危机", "告發者的危機", "고발자의 위기");
            AddEntry(entries, UILocalizationKeys.Result.MisakiDiscovered, "美咲の存在を知った", "Misaki Discovered", "发现了美咲的存在", "發現了美咲的存在", "미사키의 존재를 알게 됨");
            AddEntry(entries, UILocalizationKeys.Result.TruthRevealed, "真実が明らかに", "Truth Revealed", "真相大白", "真相大白", "진실이 밝혀짐");
            AddEntry(entries, UILocalizationKeys.Result.UncertainFuture, "不確かな未来", "Uncertain Future", "不确定的未来", "不確定的未來", "불확실한 미래");

            // Route summary
            AddEntry(entries, UILocalizationKeys.Result.YourRoute, "あなたのルート:", "Your Route:", "你的路线:", "你的路線:", "당신의 루트:");
            AddEntry(entries, UILocalizationKeys.Result.RouteTruth, "真実の道", "Path of Truth", "真相之路", "真相之路", "진실의 길");
            AddEntry(entries, UILocalizationKeys.Result.RouteDarkness, "闇の道", "Path of Darkness", "黑暗之路", "黑暗之路", "어둠의 길");
            AddEntry(entries, UILocalizationKeys.Result.RouteLight, "光への道", "Path to Light", "通往光明之路", "通往光明之路", "빛을 향한 길");
            AddEntry(entries, UILocalizationKeys.Result.RouteShadow, "影への道", "Path to Shadow", "通往阴影之路", "通往陰影之路", "그림자를 향한 길");
            AddEntry(entries, UILocalizationKeys.Result.RouteNeutral, "中立の道", "Neutral Path", "中立之路", "中立之路", "중립의 길");
            AddEntry(entries, UILocalizationKeys.Result.BestChoices, "最善の選択: {0}回", "Best choices: {0}", "最佳选择: {0}次", "最佳選擇: {0}次", "최선의 선택: {0}회");
            AddEntry(entries, UILocalizationKeys.Result.NeutralChoices, "中立の選択: {0}回", "Neutral choices: {0}", "中立选择: {0}次", "中立選擇: {0}次", "중립적 선택: {0}회");
            AddEntry(entries, UILocalizationKeys.Result.BadChoices, "過ちの選択: {0}回", "Mistakes: {0}", "错误选择: {0}次", "錯誤選擇: {0}次", "실수한 선택: {0}회");

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
            AddEntry(entries, UILocalizationKeys.Operator.PrivateNumber, "非通知", "Private Number", "隐藏号码", "隱藏號碼", "발신자 표시 제한");

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

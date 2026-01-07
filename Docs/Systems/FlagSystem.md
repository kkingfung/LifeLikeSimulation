# フラグシステム — エンジニアリング仕様書

**ステータス:** エンジニアリング対応
**最終更新:** 2026-01-07

---

## デザイン哲学

フラグは**不可視、最小限、強力**。

ダイアログ選択を表すものではない。
システムが追跡する**行動パターン**を表す。

プレイヤーはフラグ名を見ることがない。
プレイヤーは結果だけを見る。

---

## コアフラグカテゴリ

### 1. 安心フラグ
**追跡内容:** オペレーターは偽りの安心を与えたか？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `early_reassurance` | 家族に「きっと大丈夫」と伝えた | +1 |
| `suggested_wait_family` | 家族に「もう少し待って」と伝えた | +1 |
| `double_reassurance` | 家族に「電池切れかも」と伝えた | +1 |
| `triple_reassurance` | 家族コールバックで再び「大丈夫」 | +2 |
| `family_delayed_by_reassurance` | 家族が安心して行動を遅らせた | +2 |
| `yamada_protected` | ドライバーに「きっと大丈夫」と伝えた | +2 |
| `offered_excuse` | ドライバーに「動物かも」と言った | +2 |
| `dismissed_concern` | ドライバーの懸念を軽視した | +1 |
| `reassurance_given` | 一般的な安心を与えた | +1 |
| `empty_comfort` | 空虚な慰めを与えた | +1 |

**集計:** `reassurance_score` (0-15スケール)

**システム使用:**
- 高スコア → 家族のフォローアップ通話がトリガー
- 高スコア → エンディング計算が罪悪感方向に重み付け
- 高スコア + 悪い結果 → 「共犯者」エンディングへの経路

---

### 2. 開示フラグ
**追跡内容:** オペレーターは通話間で情報を共有したか？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `disclosed_to_driver` | ドライバーに遺体発見を言及した | +3 |
| `confirmed_location_to_driver` | ドライバーに歩道橋を確認した | +2 |
| `disclosed_to_family` | 家族にインシデントを言及した | +2 |
| `disclosed_body_to_family` | 家族に遺体発見を伝えた | +3 |
| `gave_location_to_family` | 家族に歩道橋の場所を伝えた | +2 |
| `full_disclosure_family` | 家族に全情報を開示した | +3 |
| `footbridge_connection` | 複数の通話で歩道橋を接続した | +1 |
| `showed_recognition` | 場所への認識を示した | +1 |
| `showed_recognition_family` | 家族に場所の認識を示した | +1 |
| `hinted_to_nakamura` | 中村に確認中と示唆した | +1 |
| `hinted_to_family` | 家族に確認中と示唆した | +1 |
| `clothing_mismatch` | 服装矛盾を記録した（伏線） | +1 |

**集計:** `disclosure_score` (0-15スケール)

**システム使用:**
- 高スコア → 主任の通話がより批判的に
- 高スコア → ドライバーがコールバックしない（逃走）
- 高スコア → 「真実の露呈」エンディングへの経路

---

### 3. エスカレーションフラグ
**追跡内容:** オペレーターは行動したか、先送りしたか？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `emergency_dispatched` | 緊急サービスを派遣した | +3 |
| `immediate_dispatch` | 即座に救急を手配した | +3 |
| `driver_led_dispatch` | ドライバー情報から派遣 | +2 |
| `suggested_police` | 警察への連絡を提案した | +2 |
| `redirected_to_police` | 発信者を警察に誘導した | +2 |
| `family_sent_to_police` | 家族を警察に送った | +2 |
| `suggested_return` | ドライバーに現場復帰を提案 | +1 |
| `yamada_returning` | ドライバーが現場に戻る | +2 |
| `suggested_search` | 家族に捜索を提案した | +1 |
| `family_mobilized` | 家族が動き出した | +1 |
| `suggested_wait` | 待機を提案した | -1 |
| `delayed_response` | 対応を遅らせた | -1 |
| `dismissed_tanaka` | 警備員の報告を軽視した | -2 |
| `pressured_tanaka` | 警備員にプレッシャーをかけて切断 | -2 |

**集計:** `escalation_score` (-5から+15スケール)

**システム使用:**
- プラススコア → 被害者の生存可能性が上がる
- マイナススコア → 時間切れ
- スコアが対応時間計算を決定

---

### 4. アラインメントフラグ（システム信頼度）
**追跡内容:** オペレーターはリスクか資産か？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `aligned_language` | 主任の言葉遣いの提案を受け入れた | +2 |
| `aligned_separation` | プロトコルに従って通話を分離した | +1 |
| `resisted_rewrite` | 言葉遣いの変更に抵抗した | -1 |
| `challenged_system` | 主任の意図を疑問視した | -2 |
| `defensive_response` | 質問された際に防御的になった | -3 |

**集計:** `system_trust` (-10から+10スケール)

**システム使用:**
- 高信頼度 → 主任がすぐに退出、自律性が維持される
- 低信頼度 → 将来のシナリオで監視が増加する可能性
- 非常に低い → 将来の夜で解雇エンディングの可能性

---

## フラグ相互作用ルール

### 複合条件

```
IF reassurance_score >= 5 AND escalation_score <= 0
THEN trigger: ending_accomplice_pathway
```

```
IF disclosure_score >= 4 AND escalation_score >= 3
THEN trigger: ending_truth_pathway
```

```
IF system_trust <= -5
THEN trigger: supervisor_extended_call
```

### 相互排他性

一部のフラグは他をキャンセルする:

| 設定された場合 | キャンセル |
|----------------|------------|
| `emergency_dispatched` | `delayed_response` |
| `disclosed_to_driver` | `yamada_protected` |
| `aligned_language` | `resisted_rewrite` |
| `yamada_returning` | `yamada_protected` |

---

## 実装ノート

### ストレージ構造

```csharp
public class NightFlags
{
    // 個別フラグ
    public Dictionary<string, bool> BoolFlags { get; } = new();
    public Dictionary<string, int> IntFlags { get; } = new();

    // 集計スコア（計算）
    public int ReassuranceScore => CalculateAggregate("reassurance_");
    public int DisclosureScore => CalculateAggregate("disclosure_");
    public int EscalationScore => CalculateAggregate("escalation_");
    public int SystemTrust => CalculateAggregate("aligned_") - CalculateAggregate("resisted_");
}
```

### フラグ設定ポイント

フラグは以下のタイミングで設定される:
1. **応答選択** — プレイヤーがダイアログオプションを選択した時
2. **沈黙タイムアウト** — プレイヤーが応答せずに待った時
3. **通話終了** — 通話行動の総合評価
4. **セグメント移行** — 通話中の行動追跡

### フラグの可視性

| プレイヤーへ | システムへ |
|--------------|------------|
| 表示されない | 常に追跡 |
| 結果を感じる | パターンを分析 |
| 数値なし | 完全なメトリクス |

---

## デバッグモード（開発専用）

テスト用:
```
/flags show — 現在のすべてのフラグを表示
/flags set [flag_id] [value] — フラグを手動設定
/flags reset — すべてのフラグをクリア
/flags export — JSONにエクスポート
```

---

## 第1夜特有フラグ

### 証拠フラグ
情報の収集・接続を追跡:

| フラグID | トリガー |
|----------|----------|
| `evidence_logged` | 証拠を記録した |
| `route_noted` | 経路情報を記録した |
| `dark_hoodie_noted` | 暗いパーカーを記録した |
| `location_matched` | 場所の一致を認識した |
| `reflective_connection` | 反射材への言及を接続した |

### 矛盾検出フラグ
話の変化を追跡:

| フラグID | トリガー |
|----------|----------|
| `story_inconsistency_detected` | 話の矛盾を検出した |
| `noted_inconsistency` | 矛盾を指摘した |
| `accepted_revision` | 修正された話を受け入れた |
| `pressed_inconsistency` | 矛盾を追及した |
| `called_out_location` | 場所の変化を指摘した |
| `called_out_emotion` | 感情の変化を指摘した |

### 組織関連フラグ（伏線）
後の夜で意味を持つ:

| フラグID | トリガー |
|----------|----------|
| `sato_name_recorded` | 佐藤浩二の名前を記録 |
| `sato_contradiction_noted` | 佐藤の矛盾を記録 |
| `pressed_sato` | 佐藤を追及した |
| `sato_hung_up_quickly` | 佐藤が急いで切断 |
| `questioned_sato` | 佐藤に通報の理由を聞いた |
| `family_claimed_reflective` | 偽の兄が反射ジャケットと主張 |
| `clothing_uncertain` | 服装情報に不確実性 |

### 派遣タイミングフラグ

| フラグID | トリガー | 被害者への影響 |
|----------|----------|----------------|
| `dispatch_time_0241` | 02:41に派遣 | 生存（最良） |
| `dispatch_time_0249` | 02:49に派遣 | 生存（良好） |

---

## 夜をまたぐ永続化

一部のフラグはシナリオをまたいで永続化する:

| フラグ | 永続化 |
|--------|--------|
| `system_trust` | 次の夜に引き継ぎ |
| `total_reassurances` | 累計カウント |
| `total_escalations` | 累計カウント |
| `supervisor_warnings` | 累計カウント |

これにより以下が可能:
- キャリア軌道の追跡
- 難易度スケーリング
- 長期的な結果の配信

---

## バージョン履歴

| バージョン | 日付 | 変更内容 |
|------------|------|----------|
| 1.0 | 2026-01-07 | 初期フラグシステム仕様書 |
| 1.1 | 2026-01-07 | 日本語翻訳 |
| 1.2 | 2026-01-07 | Night01スクリプトとの整合性確認、フラグ名更新 |

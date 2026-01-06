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
| `reassurance_family_01` | 家族に「おそらく遅延しているだけ」と伝えた | +1 |
| `reassurance_family_02` | 家族に「もう少し待って」と伝えた | +1 |
| `reassurance_driver_01` | ドライバーに「おそらく何でもない」と伝えた | +2 |
| `reassurance_driver_02` | ドライバーに「あそこは動物が多い」と伝えた | +2 |

**集計:** `reassurance_score` (0-10スケール)

**システム使用:**
- 高スコア → 家族のフォローアップ通話がトリガー
- 高スコア → エンディング計算が罪悪感方向に重み付け
- 高スコア + 悪い結果 → 「共犯者」エンディングへの経路

---

### 2. 開示フラグ
**追跡内容:** オペレーターは通話間で情報を共有したか？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `disclosed_body_to_driver` | ドライバーに遺体発見を言及した | +3 |
| `disclosed_driver_to_family` | 家族にドライバーの通話を言及した | +2 |
| `disclosed_location_cross` | 2つ以上の通話で歩道橋を接続した | +1 |
| `disclosed_clothing_match` | 反射材の服装パターンに言及した | +1 |

**集計:** `disclosure_score` (0-10スケール)

**システム使用:**
- 高スコア → 主任の通話がより批判的に
- 高スコア → ドライバーがコールバックしない（逃走）
- 高スコア → 「真実の露呈」エンディングへの経路

---

### 3. エスカレーションフラグ
**追跡内容:** オペレーターは行動したか、先送りしたか？

| フラグID | トリガー | 重み |
|----------|----------|------|
| `escalated_emergency` | 緊急サービスを派遣した | +3 |
| `escalated_police` | 警察の関与を提案した | +2 |
| `escalated_supervisor` | 積極的に主任にフラグを立てた | +1 |
| `deferred_action` | 待機/監視を提案した | -1 |
| `refused_action` | 明示的に行動を拒否した | -2 |

**集計:** `escalation_score` (-5から+10スケール)

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
| `escalated_emergency` | `deferred_action` |
| `disclosed_body_to_driver` | `reassurance_driver_01` |
| `aligned_language` | `resisted_rewrite` |

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

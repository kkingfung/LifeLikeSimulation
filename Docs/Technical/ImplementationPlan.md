# 技術実装計画 — 第1夜バーティカルスライス

**ステータス:** 実装中
**最終更新:** 2026-01-08

---

## 既存アーキテクチャ分析

### 現在実装済み

| レイヤー | コンポーネント | 状態 |
|----------|----------------|------|
| **Data** | `CallData`, `CallSegment`, `ResponseData` | ✅ 完成 |
| **Data** | `CallerData`, `CallerPersonality` | ✅ 完成 |
| **Data** | `NightScenarioData`, `ScenarioEnding` | ✅ 完成 |
| **Data** | `StoryCondition`, `StoryEffect` | ✅ 完成 |
| **Data** | `EvidenceData` | ✅ 完成 |
| **Data** | `LocalizedString`, `DialogueDatabase` | ✅ 完成 |
| **Services** | `IStoryService` / `StoryService` | ✅ 完成 |
| **Services** | `IWorldStateService` / `WorldStateService` | ✅ 完成 |
| **Services** | `ICallFlowService` / `CallFlowService` | ✅ 完成 |
| **Services** | `IEvidenceService` / `EvidenceService` | ✅ 完成 |
| **Core** | `ServiceLocator`, `ViewModelBase` | ✅ 完成 |

### 不足しているコンポーネント

| レイヤー | コンポーネント | 必要な理由 |
|----------|----------------|------------|
| **Data** | `FlagData` | フラグ定義と重みのデータ構造 |
| **Data** | `EndStateData` | エンドステート定義と条件 |
| **Services** | `IFlagService` / `FlagService` | フラグの設定・集計・永続化 |
| **Services** | `IEndStateService` / `EndStateService` | エンドステート計算とエンディング選択 |
| **Services** | `IClockService` / `ClockService` | ゲーム内時刻の専用管理（派遣タイミング） |

---

## 新規実装コンポーネント

### 1. FlagData.cs

```csharp
namespace LifeLike.Data
{
    /// <summary>
    /// フラグのカテゴリ
    /// </summary>
    public enum FlagCategory
    {
        Reassurance,    // 安心フラグ
        Disclosure,     // 開示フラグ
        Escalation,     // エスカレーションフラグ
        Alignment,      // アラインメントフラグ
        Evidence,       // 証拠フラグ
        Foreshadowing,  // 伏線フラグ
        Event           // イベントフラグ
    }

    /// <summary>
    /// フラグ定義
    /// </summary>
    [Serializable]
    public class FlagDefinition
    {
        public string flagId;
        public FlagCategory category;
        public string description;
        public int weight;  // スコア計算時の重み
        public bool persistsAcrossNights;  // 夜をまたぐか
    }

    /// <summary>
    /// 夜のフラグ状態
    /// </summary>
    [CreateAssetMenu(fileName = "NightFlags", menuName = "LifeLike/Operator/Night Flags")]
    public class NightFlagsData : ScriptableObject
    {
        public List<FlagDefinition> flagDefinitions;
    }
}
```

### 2. EndStateData.cs

```csharp
namespace LifeLike.Data
{
    /// <summary>
    /// エンドステートの種類
    /// </summary>
    public enum EndStateType
    {
        Contained,  // 封じ込め
        Exposed,    // 露出
        Complicit,  // 共犯
        Flagged,    // 要注意
        Absorbed    // 吸収
    }

    /// <summary>
    /// エンドステート条件
    /// </summary>
    [Serializable]
    public class EndStateCondition
    {
        public EndStateType endStateType;
        public int priority;  // 評価優先順位（低い方が先）
        public List<ScoreCondition> scoreConditions;
        public List<StoryCondition> flagConditions;
    }

    /// <summary>
    /// スコア条件
    /// </summary>
    [Serializable]
    public class ScoreCondition
    {
        public FlagCategory category;
        public ComparisonOperator comparison;
        public int value;
    }
}
```

### 3. IFlagService.cs / FlagService.cs

```csharp
namespace LifeLike.Services.Flag
{
    public interface IFlagService
    {
        // フラグ操作
        void SetFlag(string flagId, bool value = true);
        bool GetFlag(string flagId);
        void ClearFlag(string flagId);

        // スコア計算
        int GetCategoryScore(FlagCategory category);
        int GetReassuranceScore();
        int GetDisclosureScore();
        int GetEscalationScore();
        int GetSystemTrust();

        // 相互排他処理
        void ApplyMutualExclusion(string flagId);

        // イベント
        event Action<string, bool> OnFlagChanged;
        event Action<FlagCategory, int> OnScoreChanged;

        // 永続化
        NightFlagSnapshot CreateSnapshot();
        void RestoreFromSnapshot(NightFlagSnapshot snapshot);
        void ClearAllFlags();
    }
}
```

### 4. IEndStateService.cs / EndStateService.cs

```csharp
namespace LifeLike.Services.EndState
{
    public interface IEndStateService
    {
        // エンドステート計算
        EndStateType CalculateEndState();

        // 被害者生存計算
        bool CalculateVictimSurvival(int dispatchTime);

        // エンディング選択
        string SelectEnding(EndStateType endState, bool victimSurvived);

        // エンディング取得
        ScenarioEnding? GetEnding(string endingId);

        // イベント
        event Action<EndStateType> OnEndStateCalculated;
        event Action<string> OnEndingSelected;
    }
}
```

### 5. IClockService.cs / ClockService.cs

```csharp
namespace LifeLike.Services.Clock
{
    public interface IClockService
    {
        // 現在時刻
        int CurrentTimeMinutes { get; }
        string FormattedTime { get; }

        // 派遣タイミング
        int? DispatchTime { get; }
        void RecordDispatchTime();

        // 時間操作
        void SetTime(int minutes);
        void AdvanceTime(int minutes);
        void StartRealTimeProgression(float realSecondsPerGameMinute);
        void StopRealTimeProgression();

        // イベント
        event Action<int, string> OnTimeChanged;
        event Action<int> OnDispatchRecorded;
    }
}
```

---

## ResponseDataの拡張

既存の`ResponseData`にフラグ設定機能を追加：

```csharp
// ResponseData.cs への追加
[Header("フラグ")]
[Tooltip("設定するフラグID")]
public List<string> setFlags = new();

[Tooltip("クリアするフラグID")]
public List<string> clearFlags = new();
```

---

## 実装順序

### フェーズ1: コアシステム ✅ 完了

1. ✅ **FlagData.cs** — フラグ定義のデータ構造
2. ✅ **IFlagService.cs** — フラグサービスインターフェース
3. ✅ **FlagService.cs** — フラグサービス実装
4. ✅ **フラグ相互排他ルール** — 実装

### フェーズ2: エンドステート ✅ 完了

1. ✅ **EndStateData.cs** — エンドステート定義
2. ✅ **IEndStateService.cs** — エンドステートサービスインターフェース
3. ✅ **EndStateService.cs** — エンドステート計算ロジック

### フェーズ3: 時計システム ✅ 完了

1. ✅ **IClockService.cs** — 時計サービスインターフェース
2. ✅ **ClockService.cs** — 派遣タイミング記録機能

### フェーズ4: 統合 ✅ 完了

1. ✅ **ResponseData拡張** — フラグ設定フィールド追加
2. ✅ **CallFlowService統合** — 応答選択時のフラグ設定
3. ✅ **WorldStateService統合** — エンディング計算連携
4. ✅ **GameBootstrap更新** — 新サービス登録

### フェーズ5: Night01データ作成 ✅ 完了

1. ✅ **Night01シナリオJSON** — Night01_Scenario.json作成
2. ✅ **発信者データ** — 7人分のCallerData（Night01_Callers.json）
3. ✅ **通話データ** — 9通話分のCallData（Calls/Call01〜09_*.json）
4. ✅ **フラグ定義** — Night01_FlagDefinitions.json作成
5. ✅ **エンディング定義** — 7エンディング設定（Night01_EndStateDefinition.json）

---

## Night01データ構造

### 発信者（CallerData）

| ID | 表示名 | 役割 |
|----|--------|------|
| `suzuki_ichiro` | 鈴木一郎 | 退職者 |
| `nakamura_kenta` | 中村健太 | シフト主任 |
| `sato_koji` | 佐藤浩二 | 偽の同僚（組織） |
| `tanaka_mamoru` | 田中守 | 警備員 |
| `yamada_misaki` | 山田美咲 | ドライバー |
| `hayashi_kenji` | 林健二 | 偽の兄（組織） |
| `kudo_shinji` | 工藤真司 | 当直主任 |

### 通話（CallData）

| ID | 発信者 | トリガー時刻 |
|----|--------|--------------|
| `call_retirement` | suzuki_ichiro | 02:17 |
| `call_shift` | nakamura_kenta | 02:23 |
| `call_coworker` | sato_koji | 02:31 |
| `call_discovery` | tanaka_mamoru | 02:41 |
| `call_hit` | yamada_misaki | 02:49 |
| `call_missing` | hayashi_kenji | 02:57 |
| `call_callback_driver` | yamada_misaki | 03:05 |
| `call_callback_family` | hayashi_kenji | 03:12 |
| `call_supervisor` | kudo_shinji | 03:18 |

### フラグ定義（抜粋）

```json
{
  "reassurance": [
    {"id": "early_reassurance", "weight": 1},
    {"id": "yamada_protected", "weight": 2},
    {"id": "family_delayed_by_reassurance", "weight": 2}
  ],
  "disclosure": [
    {"id": "disclosed_to_driver", "weight": 3},
    {"id": "disclosed_to_family", "weight": 2},
    {"id": "footbridge_connection", "weight": 1}
  ],
  "escalation": [
    {"id": "emergency_dispatched", "weight": 3},
    {"id": "immediate_dispatch", "weight": 3},
    {"id": "yamada_returning", "weight": 2}
  ]
}
```

---

## テスト計画

### ユニットテスト

1. **FlagService**
   - フラグ設定/取得
   - スコア計算
   - 相互排他ルール

2. **EndStateService**
   - 各エンドステートの条件判定
   - 優先順位の正確性
   - エンディングマッピング

3. **ClockService**
   - 時間進行
   - 派遣タイミング記録

### 統合テスト

1. **フルプレイスルー** — 各エンディングへの到達確認
2. **フラグ累積** — 複数選択肢でのスコア計算
3. **タイミング** — 派遣時刻と被害者生存の関係

---

## ファイル配置

### スクリプト（実装済み）
```
Life/Assets/Scripts/
├── Data/
│   ├── Flag/
│   │   ├── FlagData.cs
│   │   └── NightFlagsDefinition.cs      # ScriptableObject
│   └── EndState/
│       ├── EndStateData.cs
│       └── EndStateDefinition.cs        # ScriptableObject
├── Services/
│   ├── Flag/
│   │   ├── IFlagService.cs
│   │   └── FlagService.cs
│   ├── EndState/
│   │   ├── IEndStateService.cs
│   │   └── EndStateService.cs
│   └── Clock/
│       ├── IClockService.cs
│       └── ClockService.cs
├── ViewModels/
│   └── OperatorViewModel.cs             # オペレーターコンソールViewModel
├── Views/
│   └── OperatorView.cs                  # オペレーターコンソールUI View
├── Controllers/
│   └── Night01SceneController.cs        # Night01シーン初期化
└── Editor/
    └── Night01DataImporter.cs           # JSONインポートエディタツール
```

### Night01 JSONデータ（作成済み）
```
Life/Assets/Data/Night01/
├── Night01_Scenario.json          # シナリオ定義、通話スケジュール
├── Night01_Callers.json           # 7人の発信者データ
├── Night01_FlagDefinitions.json   # 65+フラグ定義、相互排他ルール
├── Night01_EndStateDefinition.json # 5エンドステート、7エンディング
└── Calls/
    ├── Call01_Retirement.json     # 鈴木一郎（ウォームアップ）
    ├── Call02_Shift.json          # 中村健太（林遥の不在報告）
    ├── Call03_Coworker.json       # 佐藤浩二（偽の同僚、組織）
    ├── Call04_Discovery.json      # 田中守（遺体発見、派遣機会）
    ├── Call05_Hit.json            # 山田美咲（ドライバーの告白）
    ├── Call06_Missing.json        # 林健二（偽の兄、服装矛盾）
    ├── Call07_CallbackDriver.json # 山田美咲（話の修正）
    ├── Call08_CallbackFamily.json # 林健二（責任追及）
    └── Call09_Supervisor.json     # 工藤真司（システムの圧力）
```

---

## 次のアクション

### 完了済み
1. ✅ FlagData.cs を作成
2. ✅ IFlagService.cs を作成
3. ✅ FlagService.cs を作成
4. ✅ EndStateData.cs を作成
5. ✅ IEndStateService.cs を作成
6. ✅ EndStateService.cs を作成
7. ✅ IClockService.cs を作成
8. ✅ ClockService.cs を作成
9. ✅ ResponseData を拡張
10. ✅ GameBootstrap に新サービスを登録
11. ✅ Night01 JSONデータを作成

### 次のステップ
1. ✅ CallFlowService統合 — 応答選択時のフラグ設定処理
2. ✅ WorldStateService統合 — エンディング計算連携
3. ✅ JSONデータをUnity ScriptableObjectに変換するエディタツール作成（Night01DataImporter）
4. [ ] 統合テスト — 各エンディングへの到達確認

### フェーズ6: UI実装 ✅ 完了

1. ✅ **OperatorViewModel.cs** — オペレーターコンソールViewModel
2. ✅ **OperatorView.cs** — オペレーターコンソールUI View
3. ✅ **Night01SceneController.cs** — シーン初期化コントローラー

### フェーズ7: Night02データ作成 ✅ 完了

1. ✅ **Night02_Scenario.json** — シナリオ定義（「静かな追跡」）
2. ✅ **Night02_Callers.json** — 7人の発信者（新規4人、再登場3人）
3. ✅ **Night02_FlagDefinitions.json** — 59フラグ定義
4. ✅ **Night02_EndStateDefinition.json** — 5エンドステート、5エンディング
5. ✅ **通話データ（9通話）** — Call01〜Call09

---

## Night02 JSONデータ

```
Life/Assets/Data/Night02/
├── Night02_Scenario.json          # シナリオ定義、通話スケジュール
├── Night02_Callers.json           # 7人の発信者データ
├── Night02_FlagDefinitions.json   # 59フラグ定義、相互排他ルール
├── Night02_EndStateDefinition.json # 5エンドステート、5エンディング
└── Calls/
    ├── Call01_Elderly.json        # 常連老人（ウォームアップ）
    ├── Call02_SupervisorRecords.json # 工藤主任（記録確認）
    ├── Call03_MizunoFirst.json    # 水野沙織（林遥の友人）
    ├── Call04_Insurance.json      # 佐藤浩二（保険調査員として再登場）
    ├── Call05_GuardStrange.json   # 田中守（奇妙な指示）
    ├── Call06_Anonymous.json      # 匿名の警告
    ├── Call07_HRYamada.json       # 山本理香（山田美咲について）
    ├── Call08_MizunoSecond.json   # 水野沙織2回目（監視の気配）
    └── Call09_SupervisorFollowup.json # 工藤主任（追加確認）
```

---

## バージョン履歴

| バージョン | 日付 | 変更内容 |
|------------|------|----------|
| 1.0 | 2026-01-07 | 初期実装計画 |
| 1.1 | 2026-01-07 | フェーズ1-4完了、Night01 JSONデータ作成完了 |
| 1.2 | 2026-01-08 | サービス統合完了、Night01DataImporterエディタツール作成、UI実装（OperatorViewModel/View、Night01SceneController）完了 |
| 1.3 | 2026-01-08 | Night02「静かな追跡」JSONデータ作成完了（シナリオ、発信者7人、フラグ59個、エンドステート5種、通話9本） |

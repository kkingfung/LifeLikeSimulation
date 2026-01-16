# LifeLike Simulation

実写動画を使用したインタラクティブドラマゲームを作成するためのUnityフレームワーク。分岐するストーリー、複数のエンディング、様々なジャンル（恋愛、ミステリー、ホラーなど）に対応。

<img src="/Result.PNG" width="600">

## ゲームコンセプト: "Operator: Night Signal"

夜間緊急オペレーターとして電話を受け、10夜にわたる物語を体験する「責任シミュレーター」。

- **One Operator, Many Calls, One Truth** - プレイヤーはデスクから離れず、声だけで世界を把握
- **通話は不完全** - 偏見あり、時に虚偽。真実は語られるのではなく、浮かび上がる
- **嘘をつくことも選択肢** - 良い/悪いのラベルなし、結果のみ

## 主な機能

### ゲームプレイ
- **動画再生**: 実写動画クリップのシームレスな再生
- **選択システム**: 通常選択、時限選択、ステータスベース選択
- **分岐ストーリー**: 10夜のシナリオ、60以上のエンディングバリエーション
- **信頼グラフ**: 発信者との関係性が選択肢と結末に影響
- **証拠システム**: 矛盾の発見、証拠の提示による会話の分岐

### 多言語対応 (Multi-Language Support)

**5言語完全対応:**

| 言語 | Language | 対応状況 |
|------|----------|----------|
| 日本語 | Japanese | ✅ 完全対応（デフォルト） |
| English | English | ✅ 完全対応 |
| 简体中文 | Chinese (Simplified) | ✅ 完全対応 |
| 繁體中文 | Chinese (Traditional) | ✅ 完全対応 |
| 한국어 | Korean | ✅ 完全対応 |

**ローカライズの範囲:**
- **UIテキスト**: メニュー、設定、ボタン、ステータス表示など全UI要素
- **シナリオ**: 全10夜の通話内容、選択肢、応答テキスト
- **キャラクター名**: 発信者の表示名
- **エンディング**: 結果画面のタイトルと説明文
- **システムメッセージ**: 通話ステータス、プログレス表示など

**技術仕様:**
- `ILocalizationService`: UI・システムテキストのローカライズ
- `IDialogueLocalizationService`: シナリオ・台詞のローカライズ
- JSONベースの翻訳ファイル（`Resources/Translations/Night01_Translations.json`等）
- ゲーム内設定画面からリアルタイムで言語切り替え可能

### 技術機能
- **AssetBundle対応**: サーバーからのダウンロードまたはローカル読み込み
- **オートセーブ**: 自動セーブシステム
- **トランジション設定**: 黒フェード、白フェード、クロスフェードなど
- **MVVM + ServiceLocator**: 拡張性の高いアーキテクチャ

## 動作環境

- Unity 6000.x (Unity 6)
- Videoモジュール有効

## プロジェクト構成

```
Life/Assets/
├── Scenes/
│   ├── Bootstrap.unity         # 初期化シーン
│   ├── MainMenu.unity          # メインメニュー
│   ├── ChapterSelect.unity     # 夜選択画面
│   ├── Operator.unity          # オペレーターゲーム画面
│   ├── Result.unity            # 結果画面
│   └── Settings.unity          # 設定画面
├── Scripts/
│   ├── Core/                   # MVVM基底クラス、ServiceLocator、Bootstrap
│   ├── Controllers/            # シーンコントローラー
│   ├── Services/               # Story, Video, CallFlow, Evidence, Trust, Localization等
│   ├── Data/                   # ScriptableObject（Caller, Call, Evidence等）
│   ├── ViewModels/             # UI ViewModel
│   └── Views/                  # uGUI View
├── Data/
│   ├── Night01/ ~ Night10/     # 各夜のシナリオデータ（JSON）
│   └── ...
├── Resources/
│   └── Translations/           # 各夜の翻訳データ（JSON）
│       ├── Night01_Translations.json
│       ├── Night02_Translations.json
│       └── ...
├── UI/Prefabs/                 # UIプレハブ
└── Videos/                     # 動画ファイル（またはAssetBundle使用）
```

## クイックスタート

### 1. Bootstrapシーンのセットアップ

1. `Bootstrap`という名前の新しいシーンを作成
2. 空のGameObjectに`GameBootstrap`コンポーネントを追加
3. `GameStateData`アセットを作成して割り当て
4. Build Settingsで`Bootstrap`を最初のシーンに設定

### 2. シナリオデータの作成

シナリオデータはJSON形式で管理され、多言語に対応しています:

```json
{
  "title": {
    "ja": "第一夜：深夜の通報",
    "en": "Night 1: Midnight Calls",
    "zh_CN": "第一夜：深夜的报警",
    "zh_TW": "第一夜：深夜的報警",
    "ko": "첫째 밤: 한밤중의 신고"
  }
}
```

### 3. 動画ソースの設定

動画は3つのソースから読み込み可能:

| ソース | 用途 |
|--------|------|
| `Local` | VideoClipを直接参照（開発時） |
| `AssetBundle` | サーバーからダウンロード（本番環境） |
| `StreamingAssets` | ビルド済みローカルファイル |

### 4. ゲームの実行

1. Bootstrapシーンを開く
2. Playボタンを押す
3. 自動的にMainMenuが読み込まれる

## アーキテクチャ

**MVVM + Controller + ServiceLocator**パターンを使用:

```
Controllers (Scene) → Views (UI) → ViewModels → Services → Data (ScriptableObjects/JSON)
```

### 主要サービス一覧

| サービス | 役割 |
|----------|------|
| `ICallFlowService` | 通話フロー管理 |
| `IEvidenceService` | 証拠の発見・使用・矛盾検出 |
| `ITrustGraphService` | 信頼関係の追跡 |
| `IWorldStateService` | ゲーム時間と世界状態 |
| `ILocalizationService` | UIローカライズ |
| `IDialogueLocalizationService` | シナリオローカライズ |
| `IVideoService` | 動画再生（ローカル + AssetBundle） |
| `IAudioService` | BGM/SFX/Voice管理 |
| `ISaveService` | オートセーブ管理 |
| `ITransitionService` | 画面遷移演出 |

## 多言語対応の実装

### UIローカライズ

`UILocalizationData.cs`にてコードベースで定義:

```csharp
AddEntry(entries, UILocalizationKeys.MainMenu.NewGame,
    "新規ゲーム",      // Japanese
    "New Game",        // English
    "新游戏",          // Chinese Simplified
    "新遊戲",          // Chinese Traditional
    "새 게임");        // Korean
```

### シナリオローカライズ

各夜の翻訳ファイル（`Resources/Translations/NightXX_Translations.json`）:

```json
{
  "calls": [
    {
      "callId": "call_noise",
      "title": {
        "ja": "大きな音がしたんです",
        "en": "I Heard a Loud Noise",
        "zh_CN": "我听到很大的声音",
        "zh_TW": "我聽到很大的聲音",
        "ko": "큰 소리가 났어요"
      }
    }
  ]
}
```

### 言語の切り替え

```csharp
var localizationService = ServiceLocator.Instance.Get<ILocalizationService>();
localizationService.SetLanguage(Language.English);
```

## エンディングシステム

10夜を通じて、選択によって異なるエンディングに到達:

| エンディング種別 | 説明 |
|------------------|------|
| `TruthRevealed` | 真相解明 |
| `DamageMinimized` | 被害最小化 |
| `SomeoneSaved` | 誰かを救った |
| `SomeoneAbandoned` | 誰かを見捨てた |
| `BecameAccomplice` | 共犯者になった |
| `EveryoneSaved` | 全員を救った |
| `NooneSaved` | 誰も救えなかった |

「成功」エンディングは悲劇的かもしれない。「失敗」は道徳的に正しいかもしれない。

## ドキュメント

| ファイル | 内容 |
|----------|------|
| [CLAUDE.md](CLAUDE.md) | 技術ドキュメント |
| [SCENARIOS.md](SCENARIOS.md) | 全10夜のシナリオ詳細 |
| [CHARACTERS.md](CHARACTERS.md) | 登場人物と関係図 |

## 開発ステータス

### 完了済み
- ✅ 全6シーンのフレームワーク
- ✅ MVVM + ServiceLocatorアーキテクチャ
- ✅ 20以上のコアサービス
- ✅ 通話フロー・証拠・信頼グラフシステム
- ✅ 10夜分のシナリオデータ
- ✅ **5言語対応（日本語・英語・簡体中文・繁体中文・韓国語）**
- ✅ オーディオシステム（BGM/SFX/Voice）

### 開発中
- 🔄 Phase 2: 音声追加
- 🔄 Phase 3: 動画通話

## ライセンス

[ライセンスをここに記載]

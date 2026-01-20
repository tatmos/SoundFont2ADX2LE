# SoundFont2ADX2LE

## タイトル
SoundFont2ADX2LE（SoundFont → ADX2LE 変換ツール）

## 概要
SoundFont（.sf2）をCRI Atom Craft（ADX2LE）向けの素材に変換するUnityプロジェクトです。
SoundFont内のサンプルをWAV化し、プリセット単位でキューシート／キューを構成する
`.atmcunit` / `.materialinfo` を生成します。MIDIからコールバック付きキューを
作る補助機能も含まれます。

参考: <http://qiita.com/tatmos/items/cfd625c7a4363a3dfe97>

## 構成
- `Assets/`
  - `SF2ADX2LE.cs`: SoundFont読込・WAV書き出し・キューシート構築の中核。
  - `MakeWave.cs`: 16bit PCM WAVとループ情報（smplチャンク）生成。
  - `MakeAtomCraftData.cs`: Atom Craft用の`.atmcunit` / `.materialinfo`生成。
  - `MIDI2ADX2LE.cs`: MIDI読み込みとノートタイミング抽出。
  - `MakeAtomCraftDataFromMidi.cs`: MIDIノートをCallbackイベント化。
- `MIDI/`
  - MIDIサンプル配置先（デフォルト`MIDI/siokarabushi.mid`）。
- `ProjectSettings/`
  - Unityプロジェクト設定。

## 使い方
### 1) SoundFont → ADX2LE変換
1. SoundFontファイルをプロジェクト直下に配置し、`inputSfPath`に相対パスを指定します。
   - 例: `SF/Famicom.sf2`
2. Unityで任意のGameObjectに`SF2ADX2LE`をアタッチし、`SfToAdx2ConvMain()`を実行します。
3. `output_wav/`配下に以下が生成されます。
   - WAVファイル（サンプルごと）
   - WorkUnit（`.atmcunit` / `.materialinfo`）

### 2) MIDI → コールバックキュー生成
1. MIDIファイルを`MIDI/`配下に配置し、`midiFilePath`を指定します。
2. `MIDI2ADX2LE`をアタッチし、`Midi2Adx2LeMain()`を実行します。
3. ノートタイミングからCallbackイベントを持つキューシートが生成されます。

> 注意: MIDIはフォーマット0、トラック数1のみ対応です。

## 主な機能
- SoundFont（SF2）解析（プリセット／インストゥルメント／サンプル）とWAV抽出。
- ループ情報付きWAV書き出し（smplチャンク付与）。
- プリセット単位のキューシート生成とノートレンジ別トラック構成。
- Atom Craft用WorkUnit（.atmcunit / .materialinfo）出力。
- MIDIノートタイミングのCallbackイベント化。

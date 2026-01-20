# SoundFont2ADX2LE

## タイトル
SoundFont2ADX2LE

## 概要
SoundFont（.sf2）からCRI Atom Craft（ADX2LE）向けの素材とワークユニット（.atmcunit / .materialinfo）を生成するためのUnityプロジェクトです。SoundFont内のサンプルをWAV化し、プリセット単位でキューシート／キューを構成するデータを作成します。MIDIからコールバック付きキューを生成する補助機能も含まれています。 【F:Assets/SF2ADX2LE.cs†L401-L709】【F:Assets/MakeAtomCraftData.cs†L19-L140】【F:Assets/MakeAtomCraftDataFromMidi.cs†L16-L146】

参考: <http://qiita.com/tatmos/items/cfd625c7a4363a3dfe97>

## 構成
- `Assets/`
  - `SF2ADX2LE.cs`: SoundFont読込・WAV書き出し・キューシート構築の中核ロジック。 【F:Assets/SF2ADX2LE.cs†L1-L709】
  - `MakeWave.cs`: 16bit PCM WAVとループ情報（smplチャンク）を生成。 【F:Assets/MakeWave.cs†L1-L167】
  - `MakeAtomCraftData.cs`: SoundFont由来のキューシートからAtom Craft用の`.atmcunit`/`.materialinfo`を生成。 【F:Assets/MakeAtomCraftData.cs†L12-L214】
  - `MIDI2ADX2LE.cs`: MIDIの読み込みとタイミング情報の抽出。 【F:Assets/MIDI2ADX2LE.cs†L1-L221】
  - `MakeAtomCraftDataFromMidi.cs`: MIDIのノート情報をコールバックイベントに変換したキューを生成。 【F:Assets/MakeAtomCraftDataFromMidi.cs†L1-L279】
- `MIDI/`
  - MIDIサンプル配置先（デフォルト`MIDI/siokarabushi.mid`を参照）。 【F:Assets/MIDI2ADX2LE.cs†L9-L20】
- `ProjectSettings/`
  - Unityプロジェクト設定。

## 使い方
### SoundFont → ADX2LE変換
1. SoundFontファイルをプロジェクト直下に配置し、`inputSfPath`に相対パスを設定します（例: `SF/Famicom.sf2`）。 【F:Assets/SF2ADX2LE.cs†L401-L450】
2. Unityで任意のGameObjectに`SF2ADX2LE`をアタッチし、`SfToAdx2ConvMain()`を実行します（例: ボタン/エディタ拡張/Playモード呼び出し）。 【F:Assets/SF2ADX2LE.cs†L401-L709】
3. `output_wav/`配下にWAVとWorkUnit（`.atmcunit` / `.materialinfo`）が生成されます。 【F:Assets/SF2ADX2LE.cs†L401-L709】【F:Assets/MakeAtomCraftData.cs†L19-L214】

### MIDI → コールバックキュー生成
1. MIDIファイルを`MIDI/`配下に配置し、`midiFilePath`を設定します。 【F:Assets/MIDI2ADX2LE.cs†L9-L20】
2. `MIDI2ADX2LE`をアタッチし、`Midi2Adx2LeMain()`を実行します。 【F:Assets/MIDI2ADX2LE.cs†L18-L60】
3. MIDIのノートタイミングからCallbackイベントを持つキューシートが生成されます。 【F:Assets/MakeAtomCraftDataFromMidi.cs†L118-L279】

> 注意: MIDIはフォーマット0、トラック数1のみ対応しています。 【F:Assets/MIDI2ADX2LE.cs†L84-L118】

## 主な機能
- SoundFont（SF2）の解析（プリセット・インストゥルメント・サンプル）とWAV抽出。 【F:Assets/SF2ADX2LE.cs†L401-L709】
- ループ情報付きWAV書き出し（smplチャンク付与）。 【F:Assets/MakeWave.cs†L19-L167】
- プリセットごとのキューシート生成とノートレンジ別トラック構成。 【F:Assets/SF2ADX2LE.cs†L547-L709】【F:Assets/MakeAtomCraftData.cs†L63-L214】
- Atom Craft用WorkUnit（.atmcunit / .materialinfo）出力。 【F:Assets/MakeAtomCraftData.cs†L19-L214】
- MIDIノートタイミングをCallbackイベントに変換するキュー生成。 【F:Assets/MakeAtomCraftDataFromMidi.cs†L118-L279】

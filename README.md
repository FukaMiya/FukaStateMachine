## 概要
HybridStateMachineはUnity向けの軽量かつ柔軟なステートマシンライブラリです。
「ポーリング（Pull型）」と「イベント駆動（Push型）」をシームレスに統合し、メソッドチェーンによる直感的な記述と、DI（依存性注入）への柔軟な対応を特徴としています。

## 特徴
- ハイブリッド駆動: `Update()`による条件監視（Pull）と、`Fire()`によるイベント通知（Push）を1つのステートマシン内で混在可能。アクションゲームの入力応答性と、ゲーム進行の条件管理を両立できます。

- Fluent API: `idle.To(jump).On(Event.Jump).When(() => IsGrounded).Build();` のように自然言語に近い形で遷移ロジックを記述可能です。

- 型安全なイベント: Enumを使用して型安全にイベントを管理しつつ、内部的にはハッシュ値（int）で高速に処理。UnityのAnimation Event（int/string）との連携も容易です。

- DI・Factory対応: 依存関係を持つステートの生成を制御でき、VContainerなどのDIコンテナや手動DIとスムーズに連携できます。

- Context（データ渡し）: 遷移時に`int`や`class`などのデータをステートに渡すことが可能です。

- Mermaid可視化: 構築したステートマシンをMermaid記法で出力し、遷移図として可視化可能です。

## 使い方
リポジトリ内の`Assets/StateMachine`フォルダを、Unityプロジェクトの任意の場所に配置してください。
使用時には`using HybridStateMachine;`を追記してください。
具体的な使い方は[GameEntryPoint.cs](Assets/GameEntryPoint.cs)を確認してください。Demo.unityから実行することができます。

## ライセンス
[MIT License](LICENSE)
# Tank AI Game

ノードベースで戦車のAIを構築できるゲームのプロトタイプです。
Carnage Heart や Gladiabots に着想を得て開発しています。

---

## 概要

本プロジェクトでは、プレイヤーがノードを組み合わせることで
戦車の行動ロジック（AI）を視覚的に構築できるシステムを実装しています。

Unity（C#）とUI Toolkitを用いて、
独自のノードエディタおよびAI実行システムを開発しました。

---

## 主な機能

* ノードベースAI構築システム
* UI Toolkitによるカスタムノードエディタ
* ノードの接続・編集・削除機能
* AIロジックのJSON保存／読み込み
* プレイヤーAIと敵AIの分離設計

---

## AIシステム

以下のようなノードを組み合わせて行動を制御します。

* IfEnemyAhead
* IfTurretAimed
* IfWallAhead
* MoveForward
* TurnLeft / TurnRight
* Fire

ノードグラフはJSONとして保存され、
ランタイムで逐次実行されます。

---

## 工夫した点

* ノードエディタをUI Toolkitで一から実装
* パン・ズーム対応のグラフ操作
* ノード接続の制約（1入力1出力）設計
* 実行中ノードの可視化（デバッグ機能）
* フレームごとの実行ステップ制御

---

## 技術スタック

* Unity
* C#
* UI Toolkit
* JSON

---

## 今後の予定

* 5vs5の戦車バトル対応
* ノードの種類追加
* AIの高度化
* 視覚的なデバッグ機能の強化

---

## ステータス

🚧 開発中（プロトタイプ段階）

---

## クレジット

戦車画像素材：
WW2 Pixel Top View Tanks by jimhatama
https://jimhatama.itch.io/ww2-pixel-top-view-tanks

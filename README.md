# サンプル
<img src="https://github.com/mirabarukaso/EMES/raw/main/sample1.jpg" width=50% height=50%>  

>KISS公式ブログに記載されているMOD利用規約
>
>MODはKISSサポート対象外です。
>MODを利用するに当たり、問題が発生しても製作者・KISSは一切の責任を負いかねます。
>カスタムオーダーメイド3D2を購入されている方のみが利用できます。
>カスタムオーダーメイド3D2上で表示する目的以外の利用は禁止します。
>※以上の規約は予告なく変更する場合があります。
>
>http://kisskiss.tv/kiss/diary.php?no=558
-----------------------------------------------------------------------------

# これは○○○○を置き換えるプラグインです

## 使い方
フォルダの中身をしばりすのCOM3D2(_5)\Sybaris\UnityInjectorフォルダに入れて下さい。   
又は   
フォルダの中身をBepinexのCOM3D2_5\BepInEx\plugins\EnhancedMaidEditSceneフォルダに入れて下さい。  

エディットモードにてＦ７キー（変更可）を押すとGUIが起動します。  
コントロールを押した場合に複数のメイドを選択します。  

#### エラーが発生した場合は、config/emes.xmlを削除する。  

## 設定ファイル（初回起動時の自動作成）
EMES.xml  
EMES_YotogiANM.dat

## 指ポーズファイル
EMES_FingerPose.txt

## シーンファイル
EMES_MaidScenceData.xml


## 改造、再配布、二次配布について
オープンソースです、複製・改変・再配布は自己責任でご自由にどうぞ。  
再配布、二次配布にはすべてのファイルが含まれている必要があります。  
**GNU GPL3**  

## ホットキー
ホットキーは単一または複雑なマルチキーコンボを受け入れます。  
チェックを外して、「適用」をクリックして変更を確認します。  
キーエラーが発生した場合、デフォルトは自動的にロードされます。  

黒は1回押すと有効、もう一度ボタンを押すと無効になります。  
青いは左手で単一キーを押しながら右手のマウスでクリック。  

“h_”はホールドを意味します  
例1　space　		Spaceキーを押します  
例2　h_alt+f7　		Altキーを押しながらF7キーを押します  
例3　z+x　		zキーとキーxを同時に押します  
  
注意：あまりにも多くのハンドルは、FPSが低下する可能性があります。  

## 「.asset_bg」舶来小物・背景
参照 COM3D2.Modloader（非必須）  
鏡			Mod/Mirror_props  
水     Mod/waterbeds  
小物   Mod/AssertBG  
背景	 Mod/AssertBG/Backgrounds  

・問題点  
・・背景を取得＝自給自足(「.asset_bg」+「.asset_bg_shader」必須)  
・・実際の背景として読み込まれませんです、VRモードで問題が発生する可能性があります  
・・・ModLoaderをインストールしなかった場合  
・・・The referenced script on this Behaviour (Game Object 'rect_mirror') is missing!  
・・・鏡を必要としなくても、それは無視する  

## 「複数尻尾✕自動IKチェーン」サンプル  
・台座「一本釣り」    
・・https://www.dropbox.com/s/xln3gvy7qw8vodb/Daiza_Ippon_v1.0.7z?dl=0    
<img src="https://github.com/mirabarukaso/EMES/raw/main/sample_image/daiza.jpg" width=50% height=30%>    

・「ナナチ全年齢 ver1.2」  
・・https://www.dropbox.com/s/1ajm2hxqn0brhd5/Nanachi_A_v1.2.7z?dl=0    
<img src="https://github.com/mirabarukaso/EMES/raw/main/sample_image/nanachi.jpg" width=50% height=30%>  

・「雪玉」  
・・https://www.dropbox.com/s/pnmoxxkwsgoepwu/SnowBall.7z?dl=0  


## 「自動IKチェーン」が正常に動作しない場合があります  

## カメラ移動
| キー 1 | キー 2 | 説明 |
| :---         |     :---:      |          ---: |
| Ｈ＿Ｃｔｒｌ  | 矢印キー（上下左右）     | 上下左右移動    |
| Ｈ＿Ｃｔｒｌ  | Ｈｏｍｅ／Ｅｎｄ       | 垂直移動      |
| Ｈ＿Ｃｔｒｌ	|	  ー				           |     スピードダウン    |   
| Ｈ＿Ｃｔｒｌ	|	  ＝				           |     スピードアップ  |     
| Ｈ＿Ａｌｔ	  |	 矢印キー（上下）		    |   垂直回転    |   
| Ｈ＿Ａｌｔ	   |	 矢印キー（左右）		  |     水平回転    |   
| Ｈ＿Ａｌｔ	   |	 Ｈｏｍｅ／Ｅｎｄ		  |     面回転    |   
| Ｈ＿Ａｌｔ	   |	 Ｉｎｓｅｒｔ			     |   メイドを視界に移動    |   
| Ｈ＿Ａｌｔ	   |	 Ｄｅｌｅｔｅ			     |   カメラリセット    |   
| Ｈ＿Ｓｈｉｆｔ	|	 矢印キー（上下）		   |    カメラの距離    |   
| Ｈ＿Ｓｈｉｆｔ	|	 Ｈｏｍｅ／Ｅｎｄ		    |   カメラの視野    |   
| Ｓ						 |                        |   スクリーンショット   |   

## パーツ移動
| キー 1 | キー 2 | 説明 |
| :---         |     :---:      |          ---: |
| Ｚ						 |  | 	上下左右移動
| Ｘ						 |  | 	回転
| Ｃ						 |  | 	サイズ調整
| Ｄ						 |  | 	削除
| Ｈ＿Ｃｔｒｌ	 | 	Ａ	 | 			プレハブを有効にする


## BepinExに移行
・フォルダを作成  
・・COM3D2(_5)\BepInEx\plugins\EnhancedMaidEditScene  
・・COM3D2(_5)\BepInEx\plugins\EnhancedMaidEditScene\config  
  
・コピー  
・・COM3D2(_5)\BepInEx\plugins\EnhancedMaidEditScene\BepInEx.COM3D2(5).EnhancedMaidEditScene.Plugin.dll  
・・COM3D2(_5)\BepInEx\plugins\EnhancedMaidEditScene\config\EMES_FingerPose.txt  
  
・削除  
・・COM3D2(_5)\Sybaris\UnityInjector\COM3D2(5).EnhancedMaidEditScene.Plugin.dll  
・・COM3D2(_5)\Sybaris\UnityInjector\config\EMES_YotogiANM.dat  
  
・移動  
・・「COM3D2(_5)\Sybaris\UnityInjector\config」フォルダを  
・・・EMES.ini（INI設定はXMLに継承されます）  
・・・EMES_FingerPose.txt  
・・・EMES_MaidScenceData.xml  
・・「COM3D2_5\BepInEx\plugins\EnhancedMaidEditScene\config」に移動  

## EMES_MaidScenceData.xml
v1.0  v0.7.1.0  
v1.1  v0.8.0.0  
v1.2  v1.0.0.0  
v1.3  v1.1.0.0  

## パーツドラッグまたは回転できません
パーツ名には「_yure_」が含まれます  
・解決  
 ①「サブ展開」  
 ②「髪Hit」のチェックを外して「ボディヒット」を無効にする  
 
##マテリアルプロパティの変更
「道具」➞「素材」  
キーワードと値を使用してマテリアル プロパティを変更する  

##ランタイムモデル　エクスポートｘインポート(RTME/RTMI)
「骨格」➞「模型」  
これは実験的な機能です、精度はほぼ100%（例外「歯」「側髪」など）  
・RTME  
・・現在のメイドをテクスチャ付きのモデル(OBJ+MTL)ファイルとしてエクスポート  
・・「Blender v2.79b」でインポートすることをおすすめします  
・RTMI  
・・エクスポートしたモデル（OBJ+MTL）がインポート  
・出力フォルダ  
・・BepinEx: \BepInEx\plugins\EnhancedMaidEditScene\RTME\  
・・しばりす: \Sybaris\UnityInjector\RTME\  

## 更新履歴
### 2024 08.22 Ver.1.2.1.1
・バグを修正した   
・ダンスを追加   
・カスタムオーダーメイド3D2本体Ver2.36.2対応     
・カスタムオーダーメイド3D2.5本体Ver3.36.2対応     

### 2022 12.09 Ver 1.2.0.1
・機能追加  
・・「骨格」➞「模型」  
・・・ランタイムモデルのインポート(RTMI)  
・・・ランタイムモデルエクスポート(RTME)  
・・「道具」➞「素材」  
・・・マテリアルプロパティの変更  
・・「複数尻尾」　　  
・・アルゴリズム追加「効率優先」　　  
・VRモードで動作確認済み  
・バグを修正した  
・・キャラクター雇用時のヌルポインタバグ  
・・「複数尻尾」（BIP01パーツはできません）　　  
・カスタムオーダーメイド3D2本体Ver2.25.1対応  
・カスタムオーダーメイド3D2.5本体Ver3.25.1対応  

### 2022 9.18 Ver 1.1.0.1  
・機能追加  
・・骨格  
・・・「こぶし」「指を屈する」  
・・・「複数尻尾」選択を解除できます  
・・・「その他」 BoneSlider Lite  
・・環境  
・・・「シーン情報」に「カメラの位置✕５」を追加  
・・・「カメラ」「UIを完全に非表示」  
・・・「シェーダー」に「ビネット」を追加  
・・道具  
・・・「サブパーツ」を展開  
・・・背景移動  
・・・「メイドパーツ」移動  
・既存のEMES_MaidScenceData.xmlが自動的にアップグレードします（１．２➞１．３）  
・いくつかのバグを修正した  
・・ポーズを保存した後の名前が混乱  
・BepInEx 5.4.21.0対応  
・BepInExとSybarisの間で互換性があります  
・カスタムオーダーメイド3D2本体Ver2.23.0対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・・BepInEx.COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.23.0対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  
・・BepInEx.COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.12.22 Ver 1.0.0.0
・機能追加  
・・道具  
・・・外部のPNGを読み込む  
・・・一部のアイテムでシャドウとシェーダーを変更できるようになりました  
・・・スケーリングを単一軸で調整できるようになりました  
・・環境  
・・・「シーン情報」は「道具（部屋、SS、自席、画像）」を「保存」できます  
・・・「シーン情報」は「読み込み」と「書き出し」できます  
・・・「マイルームカスタムアイテム」の影を自動追加  
・設定ファイルはXML形式になりました  
・いくつかのバグを修正した  
・BepInEx 5.4.17.0対応  
・BepInExとSybarisの間で互換性があります  
・カスタムオーダーメイド3D2本体Ver2.10.0対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・・BepInEx.COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.10.0対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  
・・BepInEx.COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.10.31 Ver 0.9.0.0
・機能追加  
・・最大ハンドル数６４制限が解除されました  
・・ポーズ/アイテム/部屋のプレビューアイコンが自動的に生成されます  
・・ポーズを保存すると、アイコンが自動的に生成されます  
・・「道具タブ」  
・・・複数回ロードできます  
・・・「デスクアイテム」を使用できます  
・・・「マイルームカスタムアイテム」を使用できます  
・・・「不明」小道具を自動的に列挙する  
・スクリーンショット（ホットキー：「ｓ」、デフォルト：オフ）  
・IK解除するとポーズが自動同期されます  
・いくつかのバグを修正した  
・カスタムオーダーメイド3D2本体Ver2.8.0対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.8.0対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.8.12 Ver 0.8.1.0
・修正「カメラホットキーの設定」  
・カスタムオーダーメイド3D2本体Ver2.4.0対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.4.1対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.8.6 Ver 0.8.0.0
・機能追加  
・・顔  
・・・選択した表情データ（カスタム）を自動同期（それは1秒遅れで表示されます）  
・・骨格  
・・・「ポーズ」  
・・・・「ポーズコピー」  
・・・「重力」  
・・・「複数尻尾」Lite  
・・・・「自動IKチェーン」  
・・・新しいボディを切り替えた後、IKデータを自動更新  
・・環境  
・・・「カメラ位置保存、読み込み」  
・・設定  
・・・「カメラ移動」  
・いくつかのバグを修正した  
・既存のEMES.INIファイルが自動的に削除されます  
・既存のEMES_MaidScenceData.xmlが自動的にアップグレードします  
・カスタムオーダーメイド3D2本体Ver2.3.1対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.3.1対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.7.16 Ver 0.7.2.0
・いくつかのバグを修正した  
・カスタムオーダーメイド3D2本体Ver2.2.0対応  
・・COM3D2.EnhancedMaidEditScene.Plugin.dll  
・カスタムオーダーメイド3D2.5本体Ver3.2.1対応  
・・COM3D25.EnhancedMaidEditScene.Plugin.dll  

### 2021.7.11 Ver 0.7.1.0
・Serializableの代わりにxmlを使用する  
・・EMES_MaidScenceData.xml  

### 2021.7.11 Ver 0.7.0.0
・回転ハンドルを追加します「目、胸」  
・機能追加  
・ロード中にポーズ、位置、回転を保持する  
・「小物」ボーンバインドの位置選択  
・「シーン情報保存と読み込み（メイド、カメラ、シェーダー）」  
・「カスタム注視点」  
・「夜伽ポーズ（全）」（夜伽ポーズキャッシュの更新には数分かかる場合があります、落ち着いて）  
・「メッセージウィンドウ」  
・バグ修正  
・・「assert_bg」の読み込みコードを書き直しました  
・・ボディを変更するとIKが自動的に更新されます  
・・いくつかのバグを修正した  
・エラーが発生した場合の自動終了  

### 2021.6.28 Ver 0.6.0.0
・機能追加  
・・「.asset_bg」ロード  
・・「環境」「舶来小物・背景」  
・・「公式ダンス」「カスタムダンス」  
・・（自分が持っていないダンスはできません）  
・ホットキー追加  
・いくつかのバグを修正した  
・カスタムオーダーメイド3D2.5本体Ver3.10対応  
  
### 2021.6.20 Ver 0.5.0.0
・機能追加  
・・外部ファイルから手ポーズをロードする  
・・「マスク」名前を変更「付属品」  
・・・「付属品」「ボディー」  
・・「環境」「パーティクル・ 小物」  
・ホットキー追加  

### 2021.6.18 Ver 0.4.1.0
・バグ修正「ポーズホットキー」  

### 2021.6.18 Ver 0.4.0.0
・機能追加  
・・IK（インバースキネマティクス）  
・・ハンドポーズ（次のリリースでは、ファイルに入れます）  
・・ホットキー  
・・・設定保存  
・・ANMポーズを保存  
・いくつかのバグを修正した  

### 2021.6.14 Ver 0.3.0.0
・最初のリリース  
・カスタムオーダーメイド3D2本体Ver1.63対応  

[@Mirabarukaso](https://twitter.com/Mirabarukaso)

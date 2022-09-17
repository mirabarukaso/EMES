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
#### 又は
フォルダの中身をBepinexのCOM3D2_5\BepInEx\plugins\EnhancedMaidEditSceneフォルダに入れて下さい。  
エディットモードにてＦ７キー（変更可）を押すとGUIが起動します。  
コントロールを押した場合に複数のメイドを選択します。  
エラーが発生した場合は、config/emes.xmlを削除する。  

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
**販売禁止**  

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
・「ナナチ全年齢 ver1.2」  
・・https://ux.getuploader.com/COM3D2/download/28  
・「雪玉」  
・・https://ux.getuploader.com/COM3D2/download/19		  

## 「自動IKチェーン」が正常に動作しない場合があります  

## カメラ移動
| （変更可） | （変更可） |  
| :---         |     :---:      |          ---: |  
| Ｈ＿Ｃｔｒｌ	|	  矢印キー（上下左右） | 	上下左右移動    |   
| Ｈ＿Ｃｔｒｌ	|	  Ｈｏｍｅ／Ｅｎｄ		 |     垂直移動    |   
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

# サンプル
<img src="https://github.com/mirabarukaso/EMES/raw/main/sample1.jpg" width=50% height=50%>  
<img src="https://github.com/mirabarukaso/EMES/raw/main/sample2.jpg" width=50% height=50%>

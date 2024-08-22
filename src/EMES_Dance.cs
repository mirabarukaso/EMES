using System;
using System.Collections.Generic;
using System.Linq;
#if SYBARIS

#endif
#if BEPINEX
using BepInEx;
#endif

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
	public class EMES_Dance
	{
		public class ActionDataList
		{
			public bool bVisible = false;
			public int iIndex = 0;
			public string sBGMName;
			public string sANM;
			public int iCharaNum;
			public string sOtherInfo = string.Empty;
			public bool bAbsoluteANMName = false;
		};

		public static class Dance
		{
			public static List<DanceData> List { get { return _dance_data_list; } private set { _dance_data_list = value; } }
			public static Dictionary<string, ActionDataList> Data { get { return _dance_action_data; } private set { _dance_action_data = value; } }
			public static List<string> BGM { get { return _bgm_ogg_list; } }
			public static List<string> SE { get { return _se_ogg_list; } }

			public static Maid[] Dancer = new Maid[4] { null, null, null, null };
			public static int DancerCount = 4;

			public enum DancerPosition
			{
				センター,
				左,
				右,
				後,
				EOL
			};

			private static Dictionary<string, ActionDataList> _dance_action_data = new Dictionary<string, ActionDataList>()
			{			
				//ok
				{ "ドキドキ ☆ Fallin' Love",  new ActionDataList{ sBGMName = "DokiDokiFallinLove_short", sANM = "dance_cm3d2_001_f", iCharaNum = 3, sOtherInfo ="dokidokifallinlove_short_inst,dokidokifallinlove_short_misato_kara,dokidokifallinlove_short_nao_kara,dokidokifallinlove_short_sasaki_kara"}},
				{ "ドキドキ ☆ Fallin' Love-in劇場",  new ActionDataList{ sBGMName = "DokiDokiFallinLove_Short", sANM = "dance_cm3d2_001_f", iCharaNum = 3, sOtherInfo ="dokidokifallinlove_short_inst,dokidokifallinlove_short_misato_kara,dokidokifallinlove_short_nao_kara,dokidokifallinlove_short_sasaki_kara"}},
				//ok
				{ "entrance to you",  new ActionDataList{ sBGMName = "EntranceToYou_short", sANM = "dance_cm3d_001_f", iCharaNum = 1}},
				{ "entrance to you-in劇場",  new ActionDataList { sBGMName = "EntranceToYou_short", sANM = "dance_cm3d_001_f", iCharaNum = 1}},
				//ok
				{ "scarlet leap", new ActionDataList { sBGMName = "scarlet leap_short", sANM = "dance_cm3d_002_end_f", iCharaNum = 1, sOtherInfo = "scarlet leap_short_kara_1"}},
				{ "scarlet leap-in劇場", new ActionDataList { sBGMName = "scarlet leap_short", sANM = "dance_cm3d_002_end_f", iCharaNum = 1, sOtherInfo = "scarlet leap_short_kara_1"}},
				//ok
				{ "rhythmix to you", new ActionDataList {  sBGMName = "RhythmixToYou", sANM = "dance_cm3d_003_sp2_f", iCharaNum = 1, sOtherInfo = "rhythmixtoyou_kara"}},
				{ "rhythmix to you-in劇場", new ActionDataList{ sBGMName = "RhythmixToYou", sANM = "dance_cm3d_003_sp2_f", iCharaNum = 1, sOtherInfo = "rhythmixtoyou_kara"}},
				//ok
				{ "Can Know Two Close", new ActionDataList{ sBGMName = "canknowtwoclose_short", sANM = "dance_cm3d_004_kano_f", iCharaNum = 1, sOtherInfo = "canknowtwoclose_short_kara"}},
				{ "Can Know Two Close-in劇場", new ActionDataList{ sBGMName = "canknowtwoclose_short", sANM = "dance_cm3d_004_kano_f", iCharaNum = 1, sOtherInfo = "canknowtwoclose_short_kara"}},
				//ok
				{ "Night Magic Fire", new ActionDataList{ sBGMName = "nightmagicfire_short", sANM = "dance_cm3d21_001_nmf_f", iCharaNum = 3, sOtherInfo = "nightmagicfire_short_kara,nightmagicfire_ayane_short,nightmagicfire_nakae_short,nightmagicfire_nao_short"}},
				{ "Night Magic Fire-in劇場", new ActionDataList{ sBGMName = "nightmagicfire_short", sANM = "dance_cm3d21_001_nmf_f", iCharaNum = 3, sOtherInfo = "nightmagicfire_short_kara,nightmagicfire_ayane_short,nightmagicfire_nakae_short,nightmagicfire_nao_short"}},
				//ok
				{ "Blooming∞Dreaming！",   new ActionDataList{ sBGMName = "bloomingdreaming_short", sANM = "dance_cm3d21_002_bid_f", iCharaNum = 3, sOtherInfo = "bloomingdreaming_short _kara,bloomingdreaming_ayane_short,bloomingdreaming_nakae_short,bloomingdreaming_nao_short"}},
				{ "Blooming∞Dreaming！-in劇場", new ActionDataList{ sBGMName = "bloomingdreaming_short", sANM = "dance_cm3d21_002_bid_f", iCharaNum = 3, sOtherInfo = "bloomingdreaming_short _kara,bloomingdreaming_ayane_short,bloomingdreaming_nakae_short,bloomingdreaming_nao_short"}},
				//ok
				{ "キミに愛情でりぃしゃす", new ActionDataList{ sBGMName = "kiminiaijodelicious_short", sANM = "dance_cm3d21_003_kad_f", iCharaNum = 3, sOtherInfo = "kimini_aijo_delicious_short_kara,kimini_aijo_delicious_ayane_short,kimini_aijo_delicious_nakae_short,kimini_aijo_delicious_nao_short"}},
				{ "キミに愛情でりぃしゃす-in劇場", new ActionDataList{ sBGMName = "kiminiaijodelicious_short", sANM = "dance_cm3d21_003_kad_f", iCharaNum = 3, sOtherInfo = "kimini_aijo_delicious_short_kara,kimini_aijo_delicious_ayane_short,kimini_aijo_delicious_nakae_short,kimini_aijo_delicious_nao_short"}},
				//ok
				{ "Luminus Moment", new ActionDataList{ sBGMName = "LuminousMoment_short", sANM = "dance_cm3d21_004_lm_f", iCharaNum = 3}},
				{ "Luminus Moment-in劇場", new ActionDataList{ sBGMName = "LuminousMoment_short", sANM = "dance_cm3d21_004_lm_f", iCharaNum = 3}},
				//ok  dance_cm3d21_005_moe_MikeSet_f1
				{ "Melody Of Empire-背景ムービー",  new ActionDataList{ sBGMName = "MelodyOfEmpire_short", sANM = "dance_cm3d21_005_moe_f", iCharaNum = 3}},
				{ "Melody Of Empire-キャラムービー", new ActionDataList{ sBGMName = "MelodyOfEmpire_short", sANM = "dance_cm3d21_005_moe_f", iCharaNum = 3}},
				{ "Melody Of Empire-in劇場", new ActionDataList{ sBGMName = "MelodyOfEmpire_short", sANM = "dance_cm3d21_005_moe_f", iCharaNum = 3}},
				//ok
				{ "さくらうららか、はらひらり", new ActionDataList{ sBGMName = "sakuraurarakaharahirari_short", sANM = "dance_cm3d21_fanbook_001_sakura_f", iCharaNum = 1}},
				{ "さくらうららか、はらひらり-in劇場", new ActionDataList{ sBGMName = "sakuraurarakaharahirari_short", sANM = "dance_cm3d21_fanbook_001_sakura_f", iCharaNum = 1}},
				//ok
				{ "stellar my tears", new ActionDataList{ sBGMName = "stellarmytears_short", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1, sOtherInfo = "stellarmytears_short_misato_kara,stellarmytears_short_nao_kara,stellarmytears_short_sasaki_kara", bAbsoluteANMName = true}},
				{ "stellar my tears-in劇場", new ActionDataList{ sBGMName = "stellarmytears_short", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1, sOtherInfo = "stellarmytears_short_misato_kara,stellarmytears_short_nao_kara,stellarmytears_short_sasaki_kara", bAbsoluteANMName = true}},
				{ "stellar my tears ver.nao", new ActionDataList{ sBGMName = "StellarMyTears_short", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},
				{ "stellar my tears ver.nao -in劇場", new ActionDataList{ sBGMName = "StellarMyTears_short", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},
				{ "stellar my tears ver.美郷あき", new ActionDataList{ sBGMName = "StellarMyTears_short_misatosama", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},
				{ "stellar my tears ver.美郷あき -in劇場", new ActionDataList{ sBGMName = "StellarMyTears_short_misatosama", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},
				{ "stellar my tears ver.佐咲紗花", new ActionDataList{ sBGMName = "StellarMyTears_short_sasakisama", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},
				{ "stellar my tears ver.佐咲紗花 -in劇場", new ActionDataList{ sBGMName = "StellarMyTears_short_sasakisama", sANM = "dance_cm3d2_002_smt_f", iCharaNum = 1}},

				//ok
				{ "mainly priority", new ActionDataList{ sBGMName = "MainlyPriority_short", sANM = "dance_cm3d21_006_mp_f",iCharaNum = 1}},
				{ "mainly priority-in劇場", new ActionDataList{ sBGMName = "MainlyPriority_short", sANM = "dance_cm3d21_006_mp_f", iCharaNum = 1}},
				//ok
				{ "secret deep blue", new ActionDataList{ sBGMName = "SecretDeepBlue_short", sANM = "dance_cm3d21_007_sdb_f", iCharaNum = 3}},
				{ "secret deep blue-in劇場", new ActionDataList{ sBGMName = "SecretDeepBlue_short", sANM = "dance_cm3d21_007_sdb_f",iCharaNum = 3}},
				//ok custom NG official
				{ "fusionic addiction",  new ActionDataList{ sBGMName = "FusionicAddiction_short_Pole", sANM = "dance_cm3d21_Pole_001_fa_f", iCharaNum = 1}},
				//NG custom ok official
				//dance_cm3d21_008_1oy_c01_02_f1.anm
				//dance_cm3d21_008_1oy_c02_01_f1.anm
				//dance_cm3d21_008_1oy_c03_02_f1.anm
				//dance_cm3d21_008_1oy_c04_01_f1.anm
				//dance_cm3d21_008_1oy_c05_02_f1.anm
				//dance_cm3d21_008_1oy_c06_01_f1.anm
				//dance_cm3d21_008_1oy_c07_02_f1.anm
				{ "1st only you ver.nao", new ActionDataList{ sBGMName = "1stonlyyou_short", sANM = "dance_cm3d21_008_1oy_c01_02_f1", iCharaNum = 2, sOtherInfo = "1stOnlyYou_short_misonoo", bAbsoluteANMName = true}},
				{ "1st only you ver.nao-劇場", new ActionDataList{ sBGMName = "1stonlyyou_short", sANM = "dance_cm3d21_008_1oy_c01_02_f1", iCharaNum = 2, sOtherInfo = "1stOnlyYou_short_misonoo", bAbsoluteANMName = true}},
				{ "1st only you ver.御苑生メイ", new ActionDataList{ sBGMName = "1stOnlyYou_short_misonoo", sANM = "dance_cm3d21_008_1oy_c01_02_f1", iCharaNum = 2, sOtherInfo = "1stonlyyou_short", bAbsoluteANMName = true}},
				{ "1st only you ver.御苑生メイ-劇場", new ActionDataList{ sBGMName = "1stOnlyYou_short_misonoo", sANM = "dance_cm3d21_008_1oy_c01_02_f1", iCharaNum = 2, sOtherInfo = "1stonlyyou_short", bAbsoluteANMName = true}},		
				//ok
				{ "candy girl", new ActionDataList{ sBGMName = "CandyGirl_Short", sANM = "dance_cmo_001_cg_f", iCharaNum = 3}},
				{ "candy girl -in劇場", new ActionDataList{ sBGMName = "CandyGirl_Short", sANM = "dance_cmo_001_cg_f", iCharaNum = 3}},
				//ok
				{ "タイヨウパラダイス", new ActionDataList{ sBGMName = "TaiyouParadise_short", sANM = "dance_cm3d21_009_tp_f", iCharaNum = 3}},
				{ "タイヨウパラダイス-in劇場", new ActionDataList{ sBGMName = "TaiyouParadise_short", sANM = "dance_cm3d21_009_tp_f", iCharaNum = 3}},
				//ok
				{ "remember to dearest", new ActionDataList{ sBGMName = "RememberToDearest_short", sANM = "dance_cm3d21_010_rtd_f", iCharaNum = 3}},
				{ "remember to dearest-in劇場", new ActionDataList{ sBGMName = "RememberToDearest_short", sANM = "dance_cm3d21_010_rtd_f", iCharaNum = 3}},
				{ "remember to dearest(失敗ver.)", new ActionDataList{ sBGMName = "remembertodearest_short_shippai", sANM = "dance_cm3d21_010_rtd-B_f", iCharaNum = 3}},
				//ok
				{ "恋しちゃったみたい", new ActionDataList{ sBGMName = "koishichattamitai_2mix_short", sANM = "dance_cm3d21_011_koi_f", iCharaNum = 3}},
				//ok
				{ "happy!happy!スキャンダル!!", new ActionDataList{ sBGMName = "happyhappyscandal_short", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4, sOtherInfo = "happyhappyscandal_short_nao_kara,happy_happy_scandal_misato_kara,happy_happy_scandal_sasaki_kara"}},
				{ "happy!happy!スキャンダル!!-in劇場", new ActionDataList{ sBGMName = "happyhappyscandal_short", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4, sOtherInfo = "happyhappyscandal_short_nao_kara,happy_happy_scandal_misato_kara,happy_happy_scandal_sasaki_kara"}},
				{ "happy!happy!スキャンダル!! ver.nao", new ActionDataList{ sBGMName = "HappyHappyScandal_Short", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				{ "happy!happy!スキャンダル!! ver.nao -in劇場", new ActionDataList{ sBGMName = "HappyHappyScandal_Short", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				{ "happy!happy!スキャンダル!! ver.美郷あき", new ActionDataList{ sBGMName = "HappyHappyScandal_Short_misato", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				{ "happy!happy!スキャンダル!! ver.美郷あき -in劇場", new ActionDataList{ sBGMName = "HappyHappyScandal_Short_misato", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				{ "happy!happy!スキャンダル!! ver.佐咲紗花", new ActionDataList{ sBGMName = "HappyHappyScandal_Short_sasaki", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				{ "happy!happy!スキャンダル!! ver.佐咲紗花 -in劇場", new ActionDataList{ sBGMName = "HappyHappyScandal_Short_sasaki", sANM = "dance_cm3d2_003_hs_f", iCharaNum = 4}},
				//ok
				{ "レグルスの涙", new ActionDataList{ sBGMName = "COM3D2_regulus_no_namida_short", sANM = "dance_cm3d21_013_rn_f", iCharaNum = 1}},
				{ "レグルスの涙-in劇場", new ActionDataList{ sBGMName = "COM3D2_regulus_no_namida_short", sANM = "dance_cm3d21_013_rn_f", iCharaNum = 1}},
				//ok
				{ "sweet sweet everyday", new ActionDataList{ sBGMName = "SweetSweetEveryday_short", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday-in劇場", new ActionDataList{ sBGMName = "SweetSweetEveryday_short", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.nao", new ActionDataList{ sBGMName = "SweetSweetEveryday_short", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.nao -in劇場", new ActionDataList{ sBGMName = "SweetSweetEveryday_short", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.美郷あき", new ActionDataList{ sBGMName = "SweetSweetEveryday_short_misato", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.美郷あき -in劇場", new ActionDataList{ sBGMName = "SweetSweetEveryday_short_misato", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.佐咲紗花", new ActionDataList{ sBGMName = "SweetSweetEveryday_short_sasaki", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				{ "sweet sweet everyday ver.佐咲紗花 -in劇場", new ActionDataList{ sBGMName = "SweetSweetEveryday_short_sasaki", sANM = "dance_cm3d2_004_sse_f", iCharaNum = 3}},
				//ok
				{ "speed up mind", new ActionDataList{ sBGMName = "COM3D2GP01+Fb_speedupmind_short", sANM = "dance_cm3d21_016_sum_f", iCharaNum = 2}},
				{ "speed up mind-in劇場", new ActionDataList{ sBGMName = "COM3D2GP01+Fb_speedupmind_short", sANM = "dance_cm3d21_016_sum_f", iCharaNum = 2}},
				//ok
				{ "改革的ハートグラフィー ver.nao", new ActionDataList{ sBGMName = "kaikakutekiheartgraphy_short1", sANM = "dance_cm3d2_005_khg_f", iCharaNum = 1, sOtherInfo = "kaikakutekiheartgraphy_short2", bAbsoluteANMName = true}},
				{ "改革的ハートグラフィー ver.nao-in劇場", new ActionDataList{ sBGMName = "kaikakutekiheartgraphy_short1", iCharaNum = 1, sOtherInfo = "kaikakutekiheartgraphy_short2", bAbsoluteANMName = true}},
				{ "改革的ハートグラフィー ver.小春めう", new ActionDataList{ sBGMName = "kaikakutekiheartgraphy_short2", sANM = "dance_cm3d2_005_khg_f", iCharaNum = 1, sOtherInfo = "kaikakutekiheartgraphy_short1", bAbsoluteANMName = true}},
				{ "改革的ハートグラフィー ver.小春めう-in劇場", new ActionDataList{ sBGMName = "KaikakutekiHeartGraphy_short2", sANM = "dance_cm3d2_005_khg_f", iCharaNum = 1, sOtherInfo = "kaikakutekiheartgraphy_short1", bAbsoluteANMName = true}},
			};

			private static List<DanceData> _dance_data_list = new List<DanceData>()
			{

			};

			private static List<string> _bgm_ogg_list = new List<string>()
			{
				"bgm001",
				"bgm002",
				"bgm003",
				"bgm003_old",
				"bgm004",
				"bgm005",
				"bgm006",
				"bgm007",
				"bgm007_old",
				"bgm008",
				"bgm009",
				"bgm010",
				"bgm011",
				"bgm012",
				"bgm013",
				"bgm014",
				"bgm014_old",
				"bgm015",
				"bgm016",
				"bgm017",
				"bgm018",
				"bgm019",
				"bgm020",
				"bgm021",
				"bgm022",
				"bgm022_old",
				"bgm023",
				"bgm023_old",
				"bgm024",
				"bgm025",
				"bgm026",
				"bgm027",
				"bgm028",
				"bgm028_old",
				"bgm029",
				"bgm029_old",
				"bgm031",
				"bgm032",
				"bgm033",
				"bgm034",
				"bgm035",
				"bgm036",
				"bgm039",
				"bgm040",
				"bgm041",
				"bgm042",
				"bgm043",
				"bgm044",
				"bgm045",
				"bgm046",
				"bgm047",
				"bgm048",
				"bgm049",
				"bgm050",
				"bgm051",
				"bgm052"
			};

			private static List<string> _se_ogg_list = new List<string>()
			{
				"se001",
				"se002",
				"se003",
				"se004",
				"se005",
				"se006",
				"se007",
				"se008",
				"se009",
				"se010",
				"se011",
				"se012",
				"se013",
				"se014",
				"se015",
				"se016",
				"se017",
				"se018",
				"se019",
				"se020",
				"se021",
				"se022",
				"se023",
				"se024",
				"se025",
				"se026",
				"se027",
				"se028",
				"se029",
				"se030",
				"se031",
				"se032",
				"se033",
				"se034",
				"se035",
				"se036",
				"se037",
				"se038",
				"se039",
				"se040",
				"se041",
				"se042",
				"se043",
				"se044",
				"se045",
				"se046",
				"se047",
				"se048",
				"se049",
				"se050",
				"se051",
				"se052",
				"se053",
				"se054",
				"se055",
				"se056",
				"se057",
				"se058",
				"se059",
				"se060",
				"se061",
				"se062",
				"se063",
				"se064",
				"se065",
				"se066",
				"se067",
				"se068",
				"se069",
				"se070",
				"se071",
				"se072",
				"se073",
				"se074"
			};
		}

		public void Dance_InitDanceDataList()
        {
#if DEBUG
			Debuginfo.Log("Dance_InitDanceDataList()", 2);
#endif
			Dance.List.Clear();
			HashSet<int> enabled_id_list = new HashSet<int>();
			Action<string> action = delegate (string file_name)
			{
				file_name += ".nei";
				if (!GameUty.FileSystem.IsExistentFile(file_name))
				{
					return;
				}
				using (AFileBase afileBase2 = GameUty.FileSystem.FileOpen(file_name))
				{
					using (CsvParser csvParser2 = new CsvParser())
					{
						bool condition2 = csvParser2.Open(afileBase2);
						NDebug.Assert(condition2, file_name + "\nopen failed.");
						for (int l = 1; l < csvParser2.max_cell_y; l++)
						{
							if (csvParser2.IsCellToExistData(0, l))
							{
								int cellAsInteger2 = csvParser2.GetCellAsInteger(0, l);
								if (!enabled_id_list.Contains(cellAsInteger2))
								{
									enabled_id_list.Add(cellAsInteger2);
								}
							}
						}
					}
				}
			};
			action("dance_enabled_list");
			for (int i = 0; i < GameUty.PathList.Count; i++)
			{
				action("dance_enabled_list_" + GameUty.PathList[i]);
			}
#if DEBUG
			//StreamWriter streamWriter = new StreamWriter("dancelist.txt");
#endif

			DanceSelect.dance_data_list_ = new List<DanceData>();
			int Index = 0;
			using (AFileBase afileBase = GameUty.FileSystem.FileOpen("dance_setting.nei"))
			{
				using (CsvParser csvParser = new CsvParser())
				{
					bool condition = csvParser.Open(afileBase);
					NDebug.Assert(condition, "file open error[dance_setting.nei]");
					for (int j = 1; j < csvParser.max_cell_y; j++)
					{
						if (csvParser.IsCellToExistData(0, j))
						{
							int num = 0;
							int cellAsInteger = csvParser.GetCellAsInteger(num++, j);
							if (enabled_id_list.Contains(cellAsInteger))
							{
								DanceData danceData = new DanceData();
								danceData.title = csvParser.GetCellAsString(num++, j);
								danceData.title = danceData.title.Replace("《改行》", "\n");
								danceData.title_font_size = csvParser.GetCellAsInteger(num++, j);
								danceData.title_offset_y = csvParser.GetCellAsInteger(num++, j);
								danceData.select_chara_num = csvParser.GetCellAsInteger(num++, j);
								danceData.scene_name = csvParser.GetCellAsString(num++, j);
								danceData.commentary_text = csvParser.GetCellAsString(num++, j);
								danceData.commentary_text = danceData.commentary_text.Replace("《改行》", "\n");
								danceData.sample_image_name = csvParser.GetCellAsString(num++, j);
								danceData.bgm_file_name = csvParser.GetCellAsString(num++, j);
								danceData.preset_name = new List<string>();
								for (int k = 0; k < danceData.select_chara_num; k++)
								{
									danceData.preset_name.Add(string.Empty);
								}
								string cellAsString = csvParser.GetCellAsString(num++, j);
								if (!string.IsNullOrEmpty(cellAsString))
								{
									string[] array = cellAsString.Split(new char[]
									{
									','
									});
									NDebug.Assert(array.Length <= danceData.preset_name.Count, "ダンス[" + danceData.title + "]のプリセット人数設定が\n多すぎます");
									int num2 = 0;
									while (num2 < danceData.preset_name.Count && num2 < array.Length)
									{
										danceData.preset_name[num2] = array[num2].Trim();
										num2++;
									}
								}
								if(true == Dance.Data.ContainsKey(danceData.title))
                                {
									Dance.Data[danceData.title].bVisible = true;
									Dance.Data[danceData.title].iIndex = Index;
									Dance.List.Add(danceData);
									Index++;
								}
								else
                                {
									Debuginfo.Warning("新しいダンスデータが見つかりました！ title = " + danceData.title + " scene_name = " + danceData.scene_name, 0);
								}

								/*
								 * { "<ダンス名 danceData.title>", new ActionDataList{ sBGMName = "<danceData.bgm_file_name>", sANM = "<ANM fileName>", iCharaNum = <ダンス人数 1>, }}, 
								 * 
								 * もし自分で新しいダンスを追加したいのであれば、改造した 「Assembly-CSharp.dll 」が必要です。
								 * DnSpyで 「LoadAniClipNative 」を検索し、以下の行を追加して再コンパイルしてください。
								 * これで新しいダンスをロードしたときに 「ANM 」の情報が得られます。
								 * 
									public static AnimationClip LoadAniClipNative(AFileSystemBase fileSystem, string fileName, bool load_l_mune_anime, bool load_r_mune_anime, bool no_chara = false)
									{
										AnimationClip result = null;
										using (AFileBase afileBase = GameUty.FileOpen(fileName, fileSystem))
										{
											if (afileBase == null || !afileBase.IsValid())
											{
												Debug.LogError(fileName + " はありませんでした。");
												return null;
											}

											// ここ
											Debug.Log("LoadAniClipNative fileName = [" + fileName + "]");	
											Debug.Log("LoadAniClipNative afileBase = [" + afileBase + "]");　
											Debug.Log("LoadAniClipNative load_l_mune_anime = [" + load_l_mune_anime.ToString() + "]");
											Debug.Log("LoadAniClipNative load_r_mune_anime = [" + load_r_mune_anime.ToString() + "]");
											//〆

											result = GameMain.Instance.AnmParse.LoadAnmClip(afileBase, load_l_mune_anime, load_r_mune_anime, no_chara);
										}
										return result;
									}
								 */
#if DEBUG
								Debuginfo.Log("danceData.title = [" + danceData.title + "]  bgm_file_name = [" + danceData.bgm_file_name + "]   scene_name = [" + danceData.scene_name + "]", 2);
#endif
							}
						}
					}
				}
			}

			foreach(KeyValuePair<string, ActionDataList> dance in Dance.Data.ToList())
            {
				if(false == dance.Value.bVisible)
                {
#if DEBUG
					Debuginfo.Log("無効なダンスデータを削除します " + dance.Key, 2);
#endif
					Dance.Data.Remove(dance.Key);
				}
            }
		}

		public void Dance_Finalized()
        {
#if DEBUG
			Debuginfo.Log("EMES_Dance Finalize ...", 2);
#endif
			Dance.List.Clear();
#if DEBUG
			Debuginfo.Log("EMES_Dance Finalized Done", 2);
#endif
		}
	}
}




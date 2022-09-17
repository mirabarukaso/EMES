using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class EMES_Yotogi
    {
        private EMES Super;
        public EMES_Yotogi(EMES super)
        {
            Super = super;
        }

        #region perfab/particle
        public static class Perfab
        {
            public static List<string> Nyou { get { return _Nyou; } }
            public static List<string> Sio { get { return _Sio; } }
            public static List<string> Seieki { get { return _Seieki; } }
            public static List<string> Enema { get { return _Enema; } }
            public static List<string> Toiki { get { return _Toiki; } }
            public static List<string> Steam { get { return _Steam; } }

            private static List<string> _Nyou = new List<string>()
            {
                "pNyou_cm3D2", 
                "pNyouE_com3D2"
            };

            private static List<string> _Sio = new List<string>()
            {
                "pSio2_cm3D2", 
                "pSio_cm3D2"
            };

            private static List<string> _Seieki = new List<string>()
            {
                "pSeieki_tare", 
                "pSeieki_naka",
                "pSeieki",
                "pSeieki2"
            };

            private static List<string> _Enema = new List<string>()
            {
                "pEnemaBurst_com3D2",
                "pEnemaBurst02_com3D2",
                "pEnemaLeak_com3D2"
            };

            private static List<string> _Toiki = new List<string>()
            {
                "pToiki"
            };

            private static List<string> _Steam = new List<string>()
            {
                "pSteam001_cm3D2",
                "pSteam002_cm3D2",
                "pSteamBlack"
            };
        }

        public void Yotogi_StartNyo(bool bAll, string sPrefabName)
        {
            if (true == bAll)
            {
                for (int i = 0; i < Super.Window.CurrentMaidsStockID.Count; i++)
                {
                    Super.Window.CurrentMaidsList[i].AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina",  new Vector3(0f, float.Parse(Super.settingsXml.fNyodouY), 0.011f), new Vector3(20.0f, -180.0f, 180.0f));
                }
            }
            else
            {
                Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina", new Vector3(0f, float.Parse(Super.settingsXml.fNyodouY), 0.011f), new Vector3(20.0f, -180.0f, 180.0f));
            }
            GameMain.Instance.SoundMgr.PlaySe("SE011.ogg", false);
        }

        public void Yotogi_StartSio(bool bAll, string sPrefabName)
        {
            if (true == bAll)
            {
                for (int i = 0; i < Super.Window.CurrentMaidsStockID.Count; i++)
                {
                    Super.Window.CurrentMaidsList[i].AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina", new Vector3(0f, float.Parse(Super.settingsXml.fChitsuY), -0.01f), new Vector3(0f, 180.0f, 0f));
                }
            }
            else
            {
                Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina", new Vector3(0f, float.Parse(Super.settingsXml.fChitsuY), -0.01f), new Vector3(0f, 180.0f, 0f));
            }
        }

        public void Yotogi_StartSeieki(string sPrefabName)
        {
            Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina", new Vector3(0f, float.Parse(Super.settingsXml.fChitsuY), 0.011f), new Vector3(0.0f, 180.0f, 0.0f));
        }

        public void Yotogi_StartEnema(string sPrefabName)
        {
            Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "_IK_vagina", new Vector3(0f, float.Parse(Super.settingsXml.fKetsuY), 0.011f), new Vector3(-20.0f, -180.0f, 180.0f));
        }

        public void Yotogi_StartToiki(string sPrefabName)
        {
            Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "Mouth", new Vector3(0f, float.Parse(Super.settingsXml.fToikiY), float.Parse(Super.settingsXml.fToikiZ)), new Vector3(0f, 0f, -30f));
        }

        public void Yotogi_StartSteam(string sPrefabName)
        {
            Super.Window.CurrentSelectedMaid.AddPrefab("Particle/" + sPrefabName, sPrefabName, "Bip01 Head", new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 90f));
        }
        #endregion

        #region ANM methods        
        public Dictionary<string, string> Yotogi_Category = new Dictionary<string, string>()
        {
            { "hanyou", "汎用" },
            { "yuri_aibu2", "奻＠愛撫" },
            { "yuri_kunni2", "奻＠クンニ" },
            { "yuri_vibe", "奻＠ヴィべ" },
            { "yuri_soutouvibe", "奻＠双頭ヴィべ" },
            //{ "harem_houshi_aibu", "嫐＠奉仕愛撫" },
            { "harem_kaiawase", "嫐＠貝合わせ" },
            { "harem_kasane", "嫐＠重ね" },
            { "harem_seijyoui", "嫐＠正常位" },
            { "harem_kouhaii", "嫐＠後背位" },
            { "harem_haimenritui", "嫐＠背面立位" },
            { "harem_haimenzai", "嫐＠背面座位" },
            { "ran3p_aibu", "嬲＠愛撫" },
            { "ran3p_housi", "嬲＠奉仕" },            
            { "ran3p_2ana", "嬲＠２穴" },           
            { "ran3p_ude", "嬲＠腕" },
            { "ran3p_osae", "嬲＠押さえ" },
            { "ran3p_waki", "嬲＠腋" },
            { "ran3p_bed", "嬲＠ベッド" },
            { "ran3p_seijyoui", "嬲＠正常位" },
            { "ran3p_kouhaii", "嬲＠後背位" },
            { "ran4p_", "女男男男" },
            { "self", "セルフ" },
            { "sixnine", "６９" },
            { "fera", "フェラー" },
            { "vibe", "ヴィべ" },
            { "aibu", "愛撫" },
            { "settai", "接待" },
            { "anal_name", "穴舐め" },
            { "name", "舐め" },
            { "arai", "洗い" },
            { "rosyutu", "露出" },            
            { "tati", "立ち" },
            { "sumata", "素股" },
            { "tekoki", "手コキ" },           
            { "umanori", "馬乗り" },
            { "furo", "フロー" },
            { "matuba", "松葉崩し" },
            { "paizuri", "パイずり" },
            { "amayakasi", "甘やかし"},
            { "asikoki", "足こき" },
            { "sokui", "そくい" },
            { "dildo", "ディルド" },
            { "onani", "オナニー" },
            { "onahokoki", "オナホコキ" },
            { "ositaosi", "押し倒し" },
            { "inu", "犬" },
            { "poseizi", "ポースいじ" },
            { "pose", "ポース" },
            { "sukebeisu", "スケベ椅子" },
            { "isu", "椅子" },
            { "toilet", "トイレ" },           
            { "mokuba", "木馬" },
            { "turusi", "つるし" },
            { "kousokudai", "拘束台" },
            { "kousoku", "拘束" },
            { "jyouou", "女王" },
            { "manguri", "まんぐり" },
            //{ "misetuke", "見せ付け" },
            { "mzi", "Ｍ字" },
            { "seijyoui", "正常位" },
            { "kouhaii", "後背位" },
            { "kijyoui", "背面騎乗位" },
            { "haimenzai", "背面座位" },
            { "haimenritui", "背面立位" },
            { "haimenekiben", "背面駅弁" },
            { "kakaekomizai", "抱え込み座位" },
            { "ritui", "立位" },
            { "taimenekiben", "対面駅弁" },
            { "taimenzai", "対面座位" },
            { "zzz___etc___", "etc" }
        };

        public Dictionary<string, List<string>> Yotogi_data;


        public void Yotogi_RefreshCache()
        {
#if SYBARIS
            Debuginfo.Warning("夜伽ポーズキャッシュを更新すると、これには数分かかることがあります、落ち着いて．．．", 0);    
#endif

#if BEPINEX
            //BepInExできません　(；ﾟДﾟ)
            //バッファの長さ制限のバグ？
            const string longMessage1 = "夜伽ポーズキャッシュを更新すると、";
            const string longMessage2 = "これには数分かかることがあります、";
            const string longMessage3 = "落ち着いて．．．";
            Debuginfo.Warning(longMessage1, 0);
            Debuginfo.Warning(longMessage2, 0);
            Debuginfo.Warning(longMessage3, 0);
#endif
            Yotogi_RefreshCacheNow();

            Debuginfo.Warning("更新が完了しました", 0);
#if DEBUG
            /*
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("crc_amayakasi_taimen_sex_1_m.anm"))
            {
                using (MemoryStream streamANM = new MemoryStream(afileBase.ReadAll()))
                {
                    File.WriteAllBytes("crc_amayakasi_taimen_sex_1_m.anm", Yotogi_ExportANM(Yotogi_RecompileANM(Yotogi_DecodeANM(streamANM))));
                }
            }

            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("crc_amayakasi_taimen_sex_1_f.anm"))
            {
                using (MemoryStream streamANM = new MemoryStream(afileBase.ReadAll()))
                {
                    File.WriteAllBytes("crc_amayakasi_taimen_sex_1_f.anm", Yotogi_ExportANM(Yotogi_RecompileANM(Yotogi_DecodeANM(streamANM))));
                }
            }
            //*/
#endif
        }

        public void Yotogi_RefreshCacheNow()
        {
            string[] list1 = GameUty.FileSystem.GetList("motion/ero_sex/sex/", AFileSystemBase.ListType.AllFile);
            string[] list2 = GameUty.FileSystem.GetList("motion/ero_com/sex/", AFileSystemBase.ListType.AllFile);
            string[] list3 = GameUty.FileSystem.GetList("motion/ero_com_gp003/sex/", AFileSystemBase.ListType.AllFile);
            List<string> listANM = new List<string>();
            listANM.Clear();
            listANM.AddRange(list1);
            listANM.AddRange(list2);
            listANM.AddRange(list3);

            Debuginfo.Log("合計ANMファイル総数: " + listANM.Count.ToString(), 1);

            Yotogi_data = new Dictionary<string, List<string>>();
            foreach(KeyValuePair<string,string> cat in Yotogi_Category)
            {
                Yotogi_data.Add(cat.Value, new List<string>());
            }

#if DEBUG
            int fileCount = 0;
#endif
            bool bContinue = false;
            int index = 0;
            foreach(string anm in listANM)
            {
#if DEBUG
                fileCount++;
                if(0 == fileCount%1000)
                {
                    Debuginfo.Log(fileCount.ToString() + "/" + listANM.Count.ToString(), 1);
                }
#endif
                bContinue = false;
                if (false == anm.Contains(".anm"))
                {
                    continue;
                }
                if (true == anm.Contains("crc_") || true == anm.Contains("dance_"))
                {
                    continue;
                }
                foreach (KeyValuePair<string, string> cat in Yotogi_Category)
                {
                    if(true == anm.Contains(cat.Key))
                    {
                        index = anm.LastIndexOf(@"\");
                        if (index >= 0)
                        {
                            Yotogi_data[cat.Value].Add(anm.Replace(anm.Substring(0, index + 1), "").Replace(".anm", ""));
                        }
                        else
                        {
                            Yotogi_data[cat.Value].Add(anm.Replace(".anm", ""));
                        }
                        bContinue = true;
                        break;
                    }
                }
                if (true == bContinue)
                {
                    continue;
                }

                index = anm.LastIndexOf(@"\");
                if (index >= 0)
                {
                    Yotogi_data["etc"].Add(anm.Replace(anm.Substring(0, index + 1), "").Replace(".anm", ""));
                }
                else
                {
                    Yotogi_data["etc"].Add(anm.Replace(".anm", ""));
                }
            }
#if DEBUG
            Debuginfo.Log(fileCount.ToString() + "/" + listANM.Count.ToString(), 1);
#endif

            foreach (KeyValuePair<string, string> cat in Yotogi_Category.ToList())
            {
                if (0 == Yotogi_data[cat.Value].Count)
                {
#if DEBUG
                    Debuginfo.Log(cat.Value + " = 0", 1);
#endif
                    Yotogi_data[cat.Value].Add("無");
                }
            }
#if SYBARIS
            string ExportFileName = Directory.GetCurrentDirectory() + @"\Sybaris\UnityInjector\Config\EMES_YotogiANM.dat";
#endif
#if BEPINEX
            string ExportFileName = Directory.GetCurrentDirectory() + @"\BepInEx\plugins\EnhancedMaidEditScene\Config\EMES_YotogiANM.dat";
#endif
            
            if (true == File.Exists(ExportFileName))
            {
                Debuginfo.Log("削除: " + ExportFileName, 1);
                File.Delete(ExportFileName);
            }

            Debuginfo.Log("書き込み: " + ExportFileName, 1);
            MemoryStream YotogiANM = EMES.SerializeToStream(Yotogi_data);
            byte[] CFB_Byte = YotogiANM.ToArray();
            FileStream CFB_FS = new FileStream(ExportFileName, FileMode.OpenOrCreate);
            CFB_FS.Write(CFB_Byte, 0, CFB_Byte.Length);
            CFB_FS.Close();
        }

        public void Yotogi_LoadCache()
        {
#if SYBARIS
            string ExportFileName = Directory.GetCurrentDirectory() + @"\Sybaris\UnityInjector\Config\EMES_YotogiANM.dat";
#endif
#if BEPINEX
            string ExportFileName = Directory.GetCurrentDirectory() + @"\BepInEx\plugins\EnhancedMaidEditScene\Config\EMES_YotogiANM.dat";
#endif
            if (false == File.Exists(ExportFileName))
            {
                Debuginfo.Log("ファイルがありません: " + ExportFileName, 1);
                Yotogi_RefreshCache();
                return;
            }

            Debuginfo.Log("ファイルの読み取り: " + ExportFileName, 1);
            long Size = new FileInfo(ExportFileName).Length;
            byte[] Byte = new byte[Size + 1];
            FileStream FS = new FileStream(ExportFileName, FileMode.Open);
            FS.Read(Byte, 0, Byte.Length);
            FS.Close();

            MemoryStream stream = new MemoryStream(Byte);
            Yotogi_data = (Dictionary<string, List<string>>)EMES.DeserializeFromStream(stream);

            Debuginfo.Log("読み取り完了しました", 1);
        }
#endregion

#region ANM Recomiplier methods
        public AnimationState Yotogi_LoadAnime(Maid maid, string tag, byte[] byte_data, bool additive, bool loop)
        {
            if (maid.body0.m_Bones == null)
            {
                Debug.LogError("未だキャラがロードさていません。" + maid.status.callName);
                return null;
            }
#if DEBUG
            Debuginfo.Log("tag=" + tag +"  maid =" +maid.status.callName, 2);
#endif

           Animation animation = maid.body0.m_Bones.GetComponent<Animation>();
            AnimationClip clip = animation.GetClip(tag);
            clip = ImportCM.LoadAniClipNative(byte_data, true, true, false);
            animation.AddClip(clip, tag);
            if (tag.Contains("_l_"))
            {
                for (int i = 2; i <= 8; i++)
                {
                    if (tag.Contains("_l_" + i.ToString() + "_"))
                    {
                        animation[tag].layer = i;
                        break;
                    }
                }
            }
#if COM3D25
            EMES.SetFieldValue<TBody, bool>(maid.body0,"IsAnimationChange", (tag != maid.body0.LastAnimeFN));
#endif
            maid.body0.LastAnimeFN = tag;
            AnimationState animationState = animation[tag];
            if (additive)
            {
                animationState.blendMode = AnimationBlendMode.Additive;
            }
            else
            {
                animationState.blendMode = AnimationBlendMode.Blend;
            }
            if (loop)
            {
                animationState.wrapMode = WrapMode.Loop;
            }
            else
            {
                animationState.wrapMode = WrapMode.Once;
            }
            animationState.speed = 1f;
            animationState.time = 0f;
            animationState.weight = 0f;
            animationState.enabled = false;
            return animationState;
        }

        [Serializable]
        public class Yotogi_FrameInfo
        {
            public int FrameID;
            public int X;
            public int Y;
            public int Z;
        }
        [Serializable]
        public class Yotogi_BoneInfo
        {
            public string BonePath;
            public List<byte> ChannelID;
            public List<int> ChannelDataCount;
            public Dictionary<int, List<Yotogi_FrameInfo>> Frame;
        }
        [Serializable]
        public class Yotogi_ANM
        {
            public string Header = "CM3D2_ANIM";
            public int Version = 1001;
            public byte global_flag = 0x1;
            public List<Yotogi_BoneInfo> Bones;
        }

        public Yotogi_ANM Yotogi_DecodeANM(MemoryStream ANM)
        {
            Yotogi_ANM dANM = new Yotogi_ANM();
            using (BinaryReader binaryReader = new BinaryReader(ANM))
            {
                binaryReader.BaseStream.Position = 0;
                dANM.Header = binaryReader.ReadString();
                if ("CM3D2_ANIM" != dANM.Header)
                {
#if DEBUG
                    Debuginfo.Error("CM3D2_ANIM !="+ dANM.Header);
#endif
                    return null;
                }
                dANM.Version = binaryReader.ReadInt32();
                dANM.global_flag = binaryReader.ReadByte();
                dANM.Bones = new List<Yotogi_BoneInfo>();
                while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                {
                    int Index = 0;
                    Yotogi_BoneInfo bi = new Yotogi_BoneInfo();
                    bi.BonePath = binaryReader.ReadString();
                    if (null == bi.BonePath)
                    {
                        break;
                    }
                    bi.ChannelID = new List<byte>();
                    bi.ChannelDataCount = new List<int>();
                    bi.Frame = new Dictionary<int, List<Yotogi_FrameInfo>>();
                    while (true)
                    {
                        byte ChannelID = binaryReader.ReadByte();
                        if (ChannelID <= 1)
                        {
                            break;
                        }
                        bi.ChannelID.Add(ChannelID);
                        bi.ChannelDataCount.Add(binaryReader.ReadInt32());
                        bi.Frame.Add(Index, new List<Yotogi_FrameInfo>());
                        for (int i = 0; i < bi.ChannelDataCount[Index]; i++)
                        {
                            Yotogi_FrameInfo fi = new Yotogi_FrameInfo();
                            fi.FrameID = binaryReader.ReadInt32();
                            fi.X = binaryReader.ReadInt32();
                            fi.Y = binaryReader.ReadInt32();
                            fi.Z = binaryReader.ReadInt32();
                            bi.Frame[Index].Add(fi);
                        }
                        Index++;
                    }
                    dANM.Bones.Add(bi);
                }
#if DEBUG
                Debuginfo.Log("デコードが完了しました", 2);
#endif
            }
            return dANM;
        }

        public Yotogi_ANM Yotogi_RecompileANM(Yotogi_ANM ANM)
        {
            for(int i = 0; i < ANM.Bones.Count; i++)
            {
                if (true == ANM.Bones[i].BonePath.Contains("Man"))
                {
#if DEBUG
                    //Debuginfo.Log("Convert " + ANM.Bones[i].BonePath + " to " + ANM.Bones[i].BonePath.Replace("ManBip", "Bip01"), 2);
#endif
                    ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("ManBip", "Bip01");


                    if("Bip01/Bip01 Spine" == ANM.Bones[i].BonePath)
                    {
                        ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("Bip01/Bip01 Spine", "Bip01/Bip01 Spine/Bip01 Spine0a");
                    }

                    if(true == ANM.Bones[i].BonePath.Contains("Spine/"))
                    {
#if DEBUG
                        //Debuginfo.Log("Convert " + ANM.Bones[i].BonePath + " to " + ANM.Bones[i].BonePath.Replace("Spine", "Spine0a"), 2);
#endif
                        ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("Spine/", "Spine0a/");
                    }

                    if (true == ANM.Bones[i].BonePath.Contains("Spine2"))
                    {
#if DEBUG
                        //Debuginfo.Log("Convert " + ANM.Bones[i].BonePath + " to " + ANM.Bones[i].BonePath.Replace("Spine2", "Spine1a"), 2);
#endif
                        ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("Spine2", "Spine1a");
                    }

                    if (true == ANM.Bones[i].BonePath.Contains("Bip01/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a"))
                    {
#if DEBUG
                        //Debuginfo.Log("Convert " + ANM.Bones[i].BonePath + " to " + ANM.Bones[i].BonePath.Replace("Bip01/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a", "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a"), 2);
#endif
                        ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("Bip01/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a", "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a");
                    }

                    /*
                    if (true == ANM.Bones[i].BonePath.Contains("Bip01/Bip01 Spine/Bip01 Spine1/Bip01 Spine1a"))
                    {
#if DEBUG
                        //Debuginfo.Log("Convert " + ANM.Bones[i].BonePath + " to " + ANM.Bones[i].BonePath.Replace("Bip01/Bip01 Spine/Bip01 Spine1/Bip01 Spine1a", "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a"), 2);
#endif
                        ANM.Bones[i].BonePath = ANM.Bones[i].BonePath.Replace("Bip01/Bip01 Spine/Bip01 Spine1/Bip01 Spine1a", "Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a");
                    }
                    //*/
                }
            }
#if DEBUG
            Debuginfo.Log("再コンパイルが完了しました", 2);
#endif
            return ANM;
        }

        public byte[] Yotogi_ExportANM(Yotogi_ANM ANM)
        {
            Action <BinaryWriter, Yotogi_ANM> write_bone_data = write_bone_data = delegate (BinaryWriter w, Yotogi_ANM target_bone_data)
            {
                w.Write(ANM.global_flag);
                foreach(Yotogi_BoneInfo bi in ANM.Bones)
                {
                    w.Write(bi.BonePath);
                    for (int i = 0; i < bi.ChannelID.Count; i++)
                    {
                        w.Write(bi.ChannelID[i]);
                        w.Write(bi.ChannelDataCount[i]);
                        for (int j = 0; j < bi.ChannelDataCount[i]; j++)
                        {
                            w.Write(bi.Frame[i][j].FrameID);
                            w.Write(bi.Frame[i][j].X);
                            w.Write(bi.Frame[i][j].Y);
                            w.Write(bi.Frame[i][j].Z);
                        }
                    }
                    w.Write((byte)1);
                }
            };

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(ANM.Header);
            binaryWriter.Write(ANM.Version);
            write_bone_data(binaryWriter, ANM);
            binaryWriter.Write((byte)0);
            binaryWriter.Close();
            memoryStream.Close();
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();
#if DEBUG
            Debuginfo.Log("エクスポートが完了しました " + result.Length, 2);
#endif
            return result;
        }
#endregion

#region Performer methods
        public enum YotogiRole
        {
            男1,           
            女1,
            女2,
            男2,
            男3
        };

        public enum YotogiState
        {
            娚,
            奻,
            嬲,
            嫐,
            女男男男
        };
        public Dictionary<YotogiRole, Maid> YotogiRoleList { get; private set; }
        public YotogiState YotogiLastState = YotogiState.娚;

        public void Yotogi_InitPerformer(bool m1, bool f1, bool m2, bool f2, YotogiState s)
        {
            if(null != YotogiRoleList)
            {
                YotogiRoleList.Clear();
            }
            YotogiRoleList = new Dictionary<YotogiRole, Maid>();
            if (true == m1 && true == m2 && true == f1 && true == f2 && YotogiState.女男男男 == s)
            {
                YotogiRoleList.Add(YotogiRole.女1, null);
                YotogiRoleList.Add(YotogiRole.男1, null);
                YotogiRoleList.Add(YotogiRole.男2, null);
                YotogiRoleList.Add(YotogiRole.男3, null);
            }
            else
            {

                if (true == m1)
                    YotogiRoleList.Add(YotogiRole.男1, null);

                if (true == f1)
                    YotogiRoleList.Add(YotogiRole.女1, null);

                if (true == m2)
                    YotogiRoleList.Add(YotogiRole.男2, null);

                if (true == f2)
                    YotogiRoleList.Add(YotogiRole.女2, null);
            }
            YotogiLastState = s;
        }

        public void Yotogi_ClearRole(YotogiRole p)
        {
            if(true == YotogiRoleList.ContainsKey(p))
                YotogiRoleList[p] = null;
        }

        public void Yotogi_SetRole(YotogiRole p, Maid maid)
        {
            if (true == YotogiRoleList.ContainsKey(p))
            {
                foreach (KeyValuePair<YotogiRole, Maid> Key in YotogiRoleList.ToList())
                {
                    if (Key.Key == p)
                    {
                        YotogiRoleList[p] = maid;
                    }
                    else
                    {
                        if (YotogiRoleList[Key.Key] == maid)
                        {
                            Yotogi_ClearRole(Key.Key);
                        }
                    }
                }
            }
        }

        public bool Yotogi_Perform(Maid maid, string ANM, bool loop)
        {
            if(true == Super.Window.GetYotogiCrcTranslate())
            {
                if(true == maid.IsCrcBody)
                {
                    return Yotogi_Perform(maid, "crc_" + ANM, loop, true);
                }
            }
            return Yotogi_Perform(maid, ANM, loop, true);
        }

        public bool Yotogi_Perform(Maid maid, string ANM, bool loop, bool play)
        {
            if (true == ANM.Contains("_m"))
            {
                using (AFileBase afileBase = GameUty.FileSystem.FileOpen(ANM))
                {
                    using (MemoryStream streamANM = new MemoryStream(afileBase.ReadAll()))
                    {
                        Yotogi_LoadAnime(maid, ANM, Yotogi_ExportANM(Yotogi_RecompileANM(Yotogi_DecodeANM(streamANM))), false, loop);
                    }
                }
            }
            else
            {
                using (AFileBase afileBase = GameUty.FileSystem.FileOpen(ANM))
                {
                    Yotogi_LoadAnime(maid, ANM, afileBase.ReadAll(), false, loop);
                }
            }
            
            if(true == play)
                maid.body0.m_Bones.GetComponent<Animation>().Play(ANM);

            return true;
        }

        public void Yotogi_PerformFM(string ANM, bool loop, bool syncPosRot)
        {
            if (null == YotogiRoleList[YotogiRole.女1] || null == YotogiRoleList[YotogiRole.男1])
            {
                Debuginfo.Warning("人手不足　(´・ω・｀)", 0);
                return;
            }

            string mANM = null;
            string fANM = null;
            if (true == ANM.Contains("_m"))
            {
                fANM = ANM.Replace("_m", "_f");
                mANM = ANM;
            }
            else if (true == ANM.Contains("_f"))
            {
                fANM = ANM;
                mANM = ANM.Replace("_f", "_m");
            }

            if (true == GameUty.FileSystem.IsExistentFile(fANM) && true == GameUty.FileSystem.IsExistentFile(mANM))
            {
                Yotogi_Perform(YotogiRoleList[YotogiRole.女1], fANM, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男1], mANM, loop);

                if (true == syncPosRot)
                {
                    YotogiRoleList[YotogiRole.女1].SetPos(YotogiRoleList[YotogiRole.男1].GetPos());
                    YotogiRoleList[YotogiRole.女1].SetRot(YotogiRoleList[YotogiRole.男1].GetRot());
                }

                YotogiRoleList[YotogiRole.女1].body0.m_Bones.GetComponent<Animation>().Play(fANM);
                YotogiRoleList[YotogiRole.男1].body0.m_Bones.GetComponent<Animation>().Play(mANM);
            }
            else
            {
                Debuginfo.Log("プレイを開始できません", 0);
                Debuginfo.Log("fANM="+ GameUty.FileSystem.IsExistentFile(fANM), 1);
                Debuginfo.Log("mANM=" + GameUty.FileSystem.IsExistentFile(mANM), 1);
            }
        }

        public void Yotogi_PerformFF(string ANM, bool loop, bool syncPosRot)
        {
            if (null == YotogiRoleList[YotogiRole.女1] || null == YotogiRoleList[YotogiRole.女2])
            {
                Debuginfo.Warning("人手不足　(´・ω・｀)", 0);
                return;
            }

            string fANM1 = null;
            string fANM2 = null;
            if (true == ANM.Contains("_f"))
            {
                fANM1 = ANM;
                fANM2 = ANM.Replace("_f", "_f2");
            }
            else if (true == ANM.Contains("_f2"))
            {
                fANM1 = ANM.Replace("_f2", "_f");
                fANM2 = ANM;
            }

            if (true == GameUty.FileSystem.IsExistentFile(fANM1) && true == GameUty.FileSystem.IsExistentFile(fANM2))
            {
                Yotogi_Perform(YotogiRoleList[YotogiRole.女1], fANM1, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.女2], fANM2, loop);

                if (true == syncPosRot)
                {
                    YotogiRoleList[YotogiRole.女2].SetPos(YotogiRoleList[YotogiRole.女1].GetPos());
                    YotogiRoleList[YotogiRole.女2].SetRot(YotogiRoleList[YotogiRole.女1].GetRot());
                }

                YotogiRoleList[YotogiRole.女1].body0.m_Bones.GetComponent<Animation>().Play(fANM1);
                YotogiRoleList[YotogiRole.女2].body0.m_Bones.GetComponent<Animation>().Play(fANM2);
            }
            else
            {
                Debuginfo.Log("プレイを開始できません", 0);
                Debuginfo.Log("fANM1=" + GameUty.FileSystem.IsExistentFile(fANM1), 1);
                Debuginfo.Log("fANM2=" + GameUty.FileSystem.IsExistentFile(fANM2), 1);
            }
        }

        public void Yotogi_PerformMFM(string ANM, bool loop, bool syncPosRot)
        {
            if (null == YotogiRoleList[YotogiRole.女1] || null == YotogiRoleList[YotogiRole.男1] || null == YotogiRoleList[YotogiRole.男2])
            {
                Debuginfo.Warning("人手不足　(´・ω・｀)", 0);
                return;
            }

            string mANM1 = null;
            string mANM2 = null;
            string fANM = null;
            if (true == ANM.Contains("_m"))
            {              
                mANM1 = ANM;
                mANM2 = ANM.Replace("_m", "_m2");
                fANM = ANM.Replace("_m", "_f");
            }
            else if (true == ANM.Contains("_m2"))
            {
                mANM1 = ANM.Replace("_m2", "_m");
                mANM2 = ANM;
                fANM = ANM.Replace("_m2", "_f");
            }
            else if (true == ANM.Contains("_f"))
            {
                mANM1 = ANM.Replace("_f", "_m");
                mANM2 = ANM.Replace("_f", "_m2"); 
                fANM = ANM;
            }

            if (true == GameUty.FileSystem.IsExistentFile(fANM) && true == GameUty.FileSystem.IsExistentFile(mANM1) && true == GameUty.FileSystem.IsExistentFile(mANM2))
            {
                Yotogi_Perform(YotogiRoleList[YotogiRole.女1], fANM, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男1], mANM1, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男2], mANM2, loop);

                if (true == syncPosRot)
                {
                    YotogiRoleList[YotogiRole.女1].SetPos(YotogiRoleList[YotogiRole.男1].GetPos());
                    YotogiRoleList[YotogiRole.女1].SetRot(YotogiRoleList[YotogiRole.男1].GetRot());

                    YotogiRoleList[YotogiRole.男2].SetPos(YotogiRoleList[YotogiRole.男1].GetPos());
                    YotogiRoleList[YotogiRole.男2].SetRot(YotogiRoleList[YotogiRole.男1].GetRot());
                }

                YotogiRoleList[YotogiRole.女1].body0.m_Bones.GetComponent<Animation>().Play(fANM);
                YotogiRoleList[YotogiRole.男1].body0.m_Bones.GetComponent<Animation>().Play(mANM1);
                YotogiRoleList[YotogiRole.男2].body0.m_Bones.GetComponent<Animation>().Play(mANM2);
            }
            else
            {
                Debuginfo.Log("プレイを開始できません", 0);
                Debuginfo.Log("fANM=" + GameUty.FileSystem.IsExistentFile(fANM), 1);
                Debuginfo.Log("mANM1=" + GameUty.FileSystem.IsExistentFile(mANM1), 1);
                Debuginfo.Log("mANM2=" + GameUty.FileSystem.IsExistentFile(mANM2), 1);
            }
        }

        public void Yotogi_PerformFMF(string ANM, bool loop, bool syncPosRot)
        {
            if (null == YotogiRoleList[YotogiRole.女1] || null == YotogiRoleList[YotogiRole.男1] || null == YotogiRoleList[YotogiRole.女2])
            {
                Debuginfo.Warning("人手不足　(´・ω・｀)", 0);
                return;
            }

            string mANM = null;
            string fANM1 = null;
            string fANM2 = null;
            if (true == ANM.Contains("_m"))
            {
                mANM = ANM;
                fANM1 = ANM.Replace("_m", "_f");
                fANM2 = ANM.Replace("_m", "_f2");
            }
            else if (true == ANM.Contains("_f"))
            {
                mANM = ANM.Replace("_f", "_m");
                fANM1 = ANM;
                fANM2 = ANM.Replace("_f", "_f2");
            }
            else if (true == ANM.Contains("_f2"))
            {
                mANM = ANM.Replace("_f2", "_m");
                fANM1 = ANM.Replace("_f2", "_f");
                fANM2 = ANM;
            }

            if (true == GameUty.FileSystem.IsExistentFile(fANM1) && true == GameUty.FileSystem.IsExistentFile(fANM2) && true == GameUty.FileSystem.IsExistentFile(mANM))
            {
                Yotogi_Perform(YotogiRoleList[YotogiRole.女1], fANM1, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.女2], fANM2, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男1], mANM, loop);

                if (true == syncPosRot)
                {
                    YotogiRoleList[YotogiRole.女1].SetPos(YotogiRoleList[YotogiRole.男1].GetPos());
                    YotogiRoleList[YotogiRole.女1].SetRot(YotogiRoleList[YotogiRole.男1].GetRot());

                    YotogiRoleList[YotogiRole.女2].SetPos(YotogiRoleList[YotogiRole.男1].GetPos());
                    YotogiRoleList[YotogiRole.女2].SetRot(YotogiRoleList[YotogiRole.男1].GetRot());
                }

                YotogiRoleList[YotogiRole.女1].body0.m_Bones.GetComponent<Animation>().Play(fANM1);              
                YotogiRoleList[YotogiRole.女2].body0.m_Bones.GetComponent<Animation>().Play(fANM2);
                YotogiRoleList[YotogiRole.男1].body0.m_Bones.GetComponent<Animation>().Play(mANM);
            }
            else
            {
                Debuginfo.Log("プレイを開始できません", 0);
                Debuginfo.Log("mANM=" + GameUty.FileSystem.IsExistentFile(mANM), 1);
                Debuginfo.Log("fANM1=" + GameUty.FileSystem.IsExistentFile(fANM1), 1);
                Debuginfo.Log("fANM2=" + GameUty.FileSystem.IsExistentFile(fANM2), 1);
            }
        }

        public void Yotogi_PerformFMMM(string ANM, bool loop, bool syncPosRot)
        {
            if (null == YotogiRoleList[YotogiRole.女1] || null == YotogiRoleList[YotogiRole.男1] || null == YotogiRoleList[YotogiRole.男2] || null == YotogiRoleList[YotogiRole.男3])
            {
                Debuginfo.Warning("人人人人手不足　(；ﾟДﾟ)", 0);
                return;
            }

            string fANM = null;
            string mANM1 = null;
            string mANM2 = null;
            string mANM3 = null;

            if (true == ANM.Contains("_f"))
            {
                fANM = ANM;
                mANM1 = ANM.Replace("_f", "_m");
                mANM2 = ANM.Replace("_f", "_m2");
                mANM3 = ANM.Replace("_f", "_m3");
            }
            else if (true == ANM.Contains("_m"))
            {
                fANM = ANM.Replace("_m", "_f");
                mANM1 = ANM;
                mANM2 = ANM.Replace("_m", "_m2");
                mANM3 = ANM.Replace("_m", "_m3");
            }
            else if (true == ANM.Contains("_m2"))
            {
                fANM = ANM.Replace("_m2", "_f");
                mANM1 = ANM.Replace("_m2", "_m");
                mANM2 = ANM;
                mANM3 = ANM.Replace("_m2", "_m3");
            }
            else if (true == ANM.Contains("_m3"))
            {
                fANM = ANM.Replace("_m3", "_f");
                mANM1 = ANM.Replace("_m3", "_m");
                mANM2 = ANM.Replace("_m3", "_m2");
                mANM3 = ANM;
            }

            if (true == GameUty.FileSystem.IsExistentFile(fANM) && true == GameUty.FileSystem.IsExistentFile(mANM1) && true == GameUty.FileSystem.IsExistentFile(mANM2) && true == GameUty.FileSystem.IsExistentFile(mANM3))
            {
                Yotogi_Perform(YotogiRoleList[YotogiRole.女1], fANM, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男1], mANM1, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男2], mANM2, loop);
                Yotogi_Perform(YotogiRoleList[YotogiRole.男3], mANM3, loop);

                if (true == syncPosRot)
                {
                    YotogiRoleList[YotogiRole.男1].SetPos(YotogiRoleList[YotogiRole.女1].GetPos());
                    YotogiRoleList[YotogiRole.男1].SetRot(YotogiRoleList[YotogiRole.女1].GetRot());

                    YotogiRoleList[YotogiRole.男2].SetPos(YotogiRoleList[YotogiRole.女1].GetPos());
                    YotogiRoleList[YotogiRole.男2].SetRot(YotogiRoleList[YotogiRole.女1].GetRot());

                    YotogiRoleList[YotogiRole.男3].SetPos(YotogiRoleList[YotogiRole.女1].GetPos());
                    YotogiRoleList[YotogiRole.男3].SetRot(YotogiRoleList[YotogiRole.女1].GetRot());
                }

                YotogiRoleList[YotogiRole.女1].body0.m_Bones.GetComponent<Animation>().Play(fANM);
                YotogiRoleList[YotogiRole.男1].body0.m_Bones.GetComponent<Animation>().Play(mANM1);
                YotogiRoleList[YotogiRole.男2].body0.m_Bones.GetComponent<Animation>().Play(mANM2);
                YotogiRoleList[YotogiRole.男3].body0.m_Bones.GetComponent<Animation>().Play(mANM3);
            }
            else
            {
                Debuginfo.Log("プレイを開始できません", 0);
                Debuginfo.Log("fANM=" + GameUty.FileSystem.IsExistentFile(fANM), 1);
                Debuginfo.Log("mANM1=" + GameUty.FileSystem.IsExistentFile(mANM1), 1);
                Debuginfo.Log("mANM2=" + GameUty.FileSystem.IsExistentFile(mANM2), 1);
                Debuginfo.Log("mANM3=" + GameUty.FileSystem.IsExistentFile(mANM3), 1);
            }
        }
#endregion
    }
}


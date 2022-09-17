using System;
using System.Collections.Generic;
using System.IO;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class EMES_Pose
    {
        private EMES Super;
        public EMES_Pose(EMES super)
        {
            Super = super;
        }

        public class PoseData
        {
            public string ShowName { get; set; }
            public long id;
            public string category;
            public string name;
            public string direct_file;
            public bool is_loop;
            public string call_script_fil;
            public string call_script_label;
            public bool is_mod;
            public bool is_mypose;
            public bool use_animekey_mune_l;
            public bool use_animekey_mune_r;
            public bool is_man_pose;

            public string iconTexName;
            public string targetBoneName;
            public AFileSystemBase fileSystem;
        };
        public Dictionary<string, List<PoseData>> Pose_DataList;
        public string Pose_firstCatagory { get; private set; }

        public void Pose_UpdateCustomMAidPoseANM(string sPoseFileName)
        {
#if DEBUG
            Debuginfo.Log("Pose_UpdateCustomMAidPoseANM() " + sPoseFileName, 2);
#endif
            PoseData photoMotionData = new PoseData()
            {
                id = -1,
                category = "カスタムポーズ",
                name = sPoseFileName,
                direct_file = sPoseFileName + ".anm",
                is_loop = false,
                call_script_fil = string.Empty,
                call_script_label = string.Empty,
                is_mod = false,
                is_mypose = true,
                ShowName = sPoseFileName,
                iconTexName = sPoseFileName + "_icon.tex"
            };
            Pose_DataList["カスタムポーズ"].Add(photoMotionData);
        }

        private void Pose_InitOfficialMaidPoseANM_Edit()
        {
            HashSet<int> hashSet = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "edit_pose_enabled_list", ref hashSet);
            HashSet<int> idHash = new HashSet<int>();
            HashSet<string> texNameHash = new HashSet<string>();

            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("edit_pose.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("edit_pose.nei open failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (csvParser.IsCellToExistData(0, i))
                            {
                                int num = 0;
                                PoseData item = new PoseData();
                                int id = csvParser.GetCellAsInteger(num++, i);
                                if (hashSet.Contains(id))
                                {
                                    if (!idHash.Contains(id))
                                    {
                                        idHash.Add(id);
                                        item.id = id;
                                        item.name = "";
                                        item.direct_file = "";
                                        item.iconTexName = Path.ChangeExtension(csvParser.GetCellAsString(num++, i).ToString(), "tex");
                                        if (!texNameHash.Contains(item.iconTexName))
                                        {
                                            texNameHash.Add(item.iconTexName);
                                            item.category = "エディット";
                                            item.targetBoneName = csvParser.GetCellAsString(num++, i);
                                            item.call_script_fil = Path.ChangeExtension(csvParser.GetCellAsString(num++, i).ToString(), "ks");
                                            item.call_script_label = csvParser.GetCellAsString(num++, i);
                                            item.fileSystem = GameUty.FileSystem;
                                            item.ShowName = item.call_script_label;
                                            if (false == Pose_DataList.ContainsKey(item.category))
                                            {
                                                if(string.Empty == Pose_firstCatagory)
                                                {
                                                    Pose_firstCatagory = item.category;
                                                }
                                                Pose_DataList.Add(item.category, new List<PoseData>());
                                            }
                                            Pose_DataList[item.category].Add(item);
#if DEBUG
                                            Debuginfo.Log("id=" + item.id + " category=" + item.category + " iconTexName=" + item.iconTexName + " call_script_fil:" + item.call_script_fil + " call_script_label=" + item.call_script_label, 2);
#endif
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int poseCount = 0;
            Action<string> action = delegate (string fileName)
            {
                if (!GameUty.FileSystemOld.IsExistentFile(fileName))
                {
                    return;
                }
                using (AFileBase afileBase2 = GameUty.FileSystemOld.FileOpen(fileName))
                {
                    using (CsvParser csvParser2 = new CsvParser())
                    {
                        bool condition2 = csvParser2.Open(afileBase2);
                        if (false == condition2)
                        {
                            Debuginfo.Error(fileName + " open failed.");
                        }
                        else
                        {
                            for (int j = 1; j < csvParser2.max_cell_y; j++)
                            {
                                if (csvParser2.IsCellToExistData(0, j))
                                {
                                    int cell_x = 0;
                                    int id = 0;
                                    PoseData item2 = new PoseData();
                                    item2.name = "";
                                    item2.direct_file = "";
                                    item2.iconTexName = Path.ChangeExtension(csvParser2.GetCellAsString(cell_x++, j).ToString(), "tex");
                                    if (!texNameHash.Contains(item2.iconTexName))
                                    {
                                        texNameHash.Add(item2.iconTexName);
                                        item2.category = "エディット";
                                        item2.targetBoneName = csvParser2.GetCellAsString(cell_x++, j);
                                        item2.call_script_fil = Path.ChangeExtension(csvParser2.GetCellAsString(cell_x++, j).ToString(), "ks");
                                        item2.call_script_label = csvParser2.GetCellAsString(cell_x++, j);
                                        if (csvParser2.IsCellToExistData(cell_x, j))
                                        {
                                            id = csvParser2.GetCellAsInteger(cell_x, j) + 910000;
                                            item2.id = id;
                                        }
                                        else
                                        {
                                            id = 900000 + poseCount++;
                                            item2.id = id;
                                        }
                                        item2.ShowName = item2.call_script_label;
                                        if (!idHash.Contains(id))
                                        {
                                            idHash.Add(id);
                                            item2.fileSystem = GameUty.FileSystemOld;
                                            if (false == Pose_DataList.ContainsKey(item2.category))
                                            {
                                                Pose_DataList.Add(item2.category, new List<PoseData>());
                                            }
                                            Pose_DataList[item2.category].Add(item2);
#if DEBUG
                                            Debuginfo.Log("id=" + item2.id + " category=" + item2.category + " call_script_fil:" + item2.call_script_fil + " call_script_label=" + item2.call_script_label, 2);
#endif
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            foreach (string str in GameUty.PathListOld)
            {
                action("edit_pose_" + str + ".nei");
            }
        }

        private void Pose_InitOfficialMaidPoseANM_Stage()
        {
            HashSet<int> hashSet = new HashSet<int>();
            wf.CsvCommonIdManager.ReadEnabledIdList(wf.CsvCommonIdManager.FileSystemType.Normal, true, "phot_motion_enabled_list", ref hashSet);
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("phot_motion_list.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("phot_motion_list.nei open failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (csvParser.IsCellToExistData(0, i) && hashSet.Contains(csvParser.GetCellAsInteger(0, i)))
                            {
                                int num = 0;
                                PoseData photoMotionData = new PoseData();
                                photoMotionData.id = (long)csvParser.GetCellAsInteger(num++, i);
                                photoMotionData.category = csvParser.GetCellAsString(num++, i);
                                photoMotionData.name = csvParser.GetCellAsString(num++, i);
                                photoMotionData.direct_file = csvParser.GetCellAsString(num++, i);
                                photoMotionData.is_loop = (csvParser.GetCellAsString(num++, i) == "○");
                                photoMotionData.call_script_fil = csvParser.GetCellAsString(num++, i);
                                photoMotionData.call_script_label = csvParser.GetCellAsString(num++, i);
                                photoMotionData.is_mod = false;
                                string cellAsString = csvParser.GetCellAsString(num++, i);
                                bool flag = csvParser.GetCellAsString(num++, i) == "○";
                                photoMotionData.use_animekey_mune_l = (photoMotionData.use_animekey_mune_r = flag);
                                photoMotionData.is_man_pose = (csvParser.GetCellAsString(num++, i) == "○");
                                photoMotionData.ShowName = photoMotionData.name;
                                photoMotionData.fileSystem = GameUty.FileSystemOld;
                                if (string.IsNullOrEmpty(cellAsString) || PluginData.IsEnabled(cellAsString))
                                {
                                    if("エディット" == photoMotionData.category)
                                    {
                                        photoMotionData.category = "エディット";
                                        bool bContinue = false;
                                        foreach(PoseData pd in Pose_DataList["エディット"])
                                        {
                                            if(true == photoMotionData.call_script_label.Contains(pd.ShowName))
                                            {
#if DEBUG
                                                Debuginfo.Log("名前を変更 " + pd.ShowName + " >> " + photoMotionData.ShowName, 2);
#endif
                                                pd.ShowName = photoMotionData.ShowName;
                                                bContinue = true;
                                                break;
                                            }
                                        }

                                        if (true == bContinue)
                                            continue;
                                    }
                                    //if (false == photoMotionData.category.Contains("男"))
                                    {
                                        if (false == Pose_DataList.ContainsKey(photoMotionData.category))
                                        {
                                            Pose_DataList.Add(photoMotionData.category, new List<PoseData>());
                                        }
                                        Pose_DataList[photoMotionData.category].Add(photoMotionData);
#if DEBUG
                                        Debuginfo.Log("id=" + photoMotionData.id + " category=" + photoMotionData.category + " name:" + photoMotionData.name, 2);
#endif
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Action<string, List<string>> CheckModFile = null;
            CheckModFile = delegate (string path, List<string> result_list)
            {
                string[] files = Directory.GetFiles(path);
                for (int n = 0; n < files.Length; n++)
                {
                    if (Path.GetExtension(files[n]) == ".anm")
                    {
                        result_list.Add(files[n]);
                    }
                }
                string[] directories = Directory.GetDirectories(path);
                for (int num4 = 0; num4 < directories.Length; num4++)
                {
                    CheckModFile(directories[num4], result_list);
                }
            };
            List<string> list = new List<string>();
            CheckModFile(PhotoWindowManager.path_photo_mod_motion, list);
            wf.CRC32 crc = new wf.CRC32();
            for (int j = 0; j < list.Count; j++)
            {
                long num2 = 0L;
                try
                {
                    using (FileStream fileStream = new FileStream(list[j], FileMode.Open, FileAccess.Read))
                    {
                        byte[] array = new byte[fileStream.Length];
                        fileStream.Read(array, 0, array.Length);
                        uint num3 = crc.ComputeChecksum(array);
                        num2 = (long)(-1 + num3);
                    }
                }
                catch
                {
                }
                if (-1 <= num2)
                {
                    string fileName = Path.GetFileName(list[j]);
                    PoseData photoMotionData2 = new PoseData();
                    photoMotionData2.id = num2;
                    photoMotionData2.category = "Mod";
                    photoMotionData2.name = Path.GetFileNameWithoutExtension(fileName);
                    photoMotionData2.direct_file = list[j];
                    photoMotionData2.is_loop = false;
                    photoMotionData2.call_script_fil = string.Empty;
                    photoMotionData2.call_script_label = string.Empty;
                    photoMotionData2.is_mod = true;
                    photoMotionData2.ShowName = photoMotionData2.name;
                    if (false == photoMotionData2.category.Contains("男") && "エディット" != photoMotionData2.category)
                    {
                        if (false == Pose_DataList.ContainsKey(photoMotionData2.category))
                        {
                            Pose_DataList.Add(photoMotionData2.category, new List<PoseData>());
                        }
                        Pose_DataList[photoMotionData2.category].Add(photoMotionData2);
#if DEBUG
                        Debuginfo.Log("MOD>> id=" + photoMotionData2.id + " category=" + photoMotionData2.category + " name:" + photoMotionData2.name, 2);
#endif
                    }
                }
            }

            Action<string, List<string>> action = delegate (string path, List<string> result_list)
            {
                string[] files = Directory.GetFiles(path);
                for (int n = 0; n < files.Length; n++)
                {
                    if (Path.GetExtension(files[n]) == ".anm")
                    {
                        result_list.Add(files[n]);
                    }
                }
            };
            list.Clear();

            action(PhotoModePoseSave.folder_path, list);
            foreach (string fullpath in list)
            {
                string fileName = Path.GetFileName(fullpath);
                PoseData photoMotionData = new PoseData();
                photoMotionData.id = fileName.GetHashCode();
                photoMotionData.category = "マイポーズ";
                photoMotionData.name = Path.GetFileNameWithoutExtension(fileName);
                photoMotionData.direct_file = fullpath;
                photoMotionData.is_loop = false;
                photoMotionData.call_script_fil = string.Empty;
                photoMotionData.call_script_label = string.Empty;
                photoMotionData.is_mod = false;
                photoMotionData.is_mypose = true;
                photoMotionData.ShowName = photoMotionData.name;
                byte[] array = new byte[2];
                try
                {
                    using (FileStream fileStream = new FileStream(photoMotionData.direct_file, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader binaryReader = new BinaryReader(fileStream))
                        {
                            string b = binaryReader.ReadString();
                            if ("COM3D2_ANIM" != b)
                            {
                                continue;
                            }
                            int num = binaryReader.ReadInt32();
                            if (1001 <= num)
                            {
                                fileStream.Seek(-2L, SeekOrigin.End);
                                fileStream.Read(array, 0, array.Length);
                                photoMotionData.use_animekey_mune_l = (array[0] != 0);
                                photoMotionData.use_animekey_mune_r = (array[1] != 0);
                            }
                            else
                            {
                                photoMotionData.use_animekey_mune_l = (photoMotionData.use_animekey_mune_r = false);
                            }
                        }
                    }
                }
                catch
                {
                }
                if (false == Pose_DataList.ContainsKey(photoMotionData.category))
                {
                    Pose_DataList.Add(photoMotionData.category, new List<PoseData>());
                }
                Pose_DataList[photoMotionData.category].Add(photoMotionData);
#if DEBUG
                Debuginfo.Log("マイポーズ>> id=" + photoMotionData.id + " direct_file=" + photoMotionData.direct_file + " name:" + photoMotionData.name, 2);
#endif
            }
        }

        private void Pose_InitCustomMaidPoseANM()
        {
#if DEBUG
            Debuginfo.Log("InitCustomMaidPoseANM()", 2);
#endif
            Pose_DataList.Add("カスタムポーズ", new List<PoseData>());

            string ANMFilesDirectory = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory;
            string[] ANMFiles1 = Directory.GetFiles(ANMFilesDirectory, "*.anm");
            foreach (string ANM in ANMFiles1)
            {
                string[] list = ANM.Split('\\');
                string name = list[list.Length - 1];
                PoseData photoMotionData = new PoseData()
                {
                    id = -1,
                    category = "カスタムポーズ",
                    name = Path.GetFileNameWithoutExtension(name).Replace(".anm", ""),
                    direct_file = ANM,
                    is_loop = false,
                    call_script_fil = string.Empty,
                    call_script_label = string.Empty,
                    is_mod = false,
                    is_mypose = true,
                    ShowName = name,
                    iconTexName = Path.GetFileNameWithoutExtension(name).Replace(".anm", "") + "_icon.tex"
                };
                Pose_DataList["カスタムポーズ"].Add(photoMotionData);
            }
        }

        public void Pose_InitMaidPoseANM()
        {
#if DEBUG
            Debuginfo.Log("InitOfficialMaidPoseANM()", 2);
#endif
            Pose_DataList = new Dictionary<string, List<PoseData>>();
            Pose_firstCatagory = string.Empty;

#if DEBUG
            Debuginfo.Log("Pose_InitOfficialMaidPoseANM_Edit()", 2);
#endif
            Pose_InitOfficialMaidPoseANM_Edit();
#if DEBUG
            Debuginfo.Log("Pose_InitOfficialMaidPoseANM_Stage()", 2);
#endif
            Pose_InitOfficialMaidPoseANM_Stage();
#if DEBUG
            Debuginfo.Log("Pose_InitCustomMaidPoseANM()", 2);
#endif
            Pose_InitCustomMaidPoseANM();

            foreach (KeyValuePair<string, List<PoseData>> category in Pose_DataList)
            {
                if(0 == category.Value.Count)
                {
                    PoseData photoMotionData = new PoseData();
                    photoMotionData.id = -2;
                    photoMotionData.category = "category.Key";
                    photoMotionData.name = "無";
                    photoMotionData.direct_file = string.Empty;
                    photoMotionData.is_loop = false;
                    photoMotionData.call_script_fil = string.Empty;
                    photoMotionData.call_script_label = string.Empty;
                    photoMotionData.is_mod = false;
                    photoMotionData.is_mypose = false;
                    photoMotionData.ShowName = photoMotionData.name;
                    Pose_DataList[category.Key].Add(photoMotionData);
                }
            }
        }

        public void Pose_Finalized()
        {
#if DEBUG
            Debuginfo.Log("EMES_Pose Finalize ...", 2);
#endif
            Pose_DataList.Clear();
            Pose_DataList = null;
            Pose_firstCatagory = string.Empty;
#if DEBUG
            Debuginfo.Log("EMES_Pose Finalized Done", 2);
#endif
        }
    }
}

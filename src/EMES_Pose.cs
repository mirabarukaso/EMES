using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

        public enum PoseMethod
        {
            ALL = 0,
            Head,
            Upper,
            Lower,
            Fingers,
            Toes,
            LeftFingers,
            RightFingers,
            LeftToes,
            RightToes
        };

        private readonly List<string> MaidHeadBones = new List<string>()
        {
            "Bip01 Neck", "Bip01 Head"            
        };

        private readonly List<string> MaidUpperBones = new List<string>()
        {
            "Bip01 Spine", "Bip01 Spine0a", "Bip01 Spine1", "Bip01 Spine1a", //"Bip01 Neck", "Bip01 Head",
            "Bip01 L Clavicle", "Bip01 L UpperArm", "Bip01 L Forearm", "Bip01 L Hand"//,
        };

        private readonly List<string> MaidLowerBones = new List<string>()
        {
            "Bip01",
            "Bip01 Pelvis", 
            "Bip01 L Thigh", "Bip01 L Calf",  "Bip01 L Foot",
        };

        private readonly List<string> MaidLeftFilgerBones = new List<string>()
        {           
            "Bip01 L Finger0", "Bip01 L Finger01", "Bip01 L Finger02",
            "Bip01 L Finger1", "Bip01 L Finger11", "Bip01 L Finger12",
            "Bip01 L Finger2", "Bip01 L Finger21", "Bip01 L Finger22",
            "Bip01 L Finger3", "Bip01 L Finger31", "Bip01 L Finger32",
            "Bip01 L Finger4", "Bip01 L Finger41", "Bip01 L Finger42"
        };

        private readonly List<string> MaidLeftToeBones = new List<string>()
        {
            "Bip01 L Toe0", "Bip01 L Toe01",
            "Bip01 L Toe1", "Bip01 L Toe11",
            "Bip01 L Toe2", "Bip01 L Toe21"
        };

        //参照：BoneSlider BoneParam.xml
        //原作者：夜勤D @YakinKazuya
        private readonly Dictionary<string, Vector3> MaidBoneParamZero = new Dictionary<string, Vector3>()
        {
            {"Bip01", new Vector3(0, 90, 90)},
            {"Bip01 Head", new Vector3(0, 0, 15.8494f)},
            {"Bip01 Neck", new Vector3(0, 0, 342.2753f)},

            {"Bip01 Spine1a", new Vector3(0, 0, 0)},
            {"Bip01 Spine1", new Vector3(0, 0, 0)},
            {"Bip01 Spine0a", new Vector3(0, 0, 0)},
            {"Bip01 Spine", new Vector3(0, 90, 90)},
            {"Bip01 Pelvis", new Vector3(0, 90, 90)},

            {"Bip01 L Thigh", new Vector3(0, 180, 0)},
            {"Bip01 L Calf", new Vector3(0, 0, 0)},
            {"Bip01 L Foot", new Vector3(0, 0, 0)},

            {"Bip01 L Toe0", new Vector3(-0.5248f, 12.8727f, -79.9091f)},
            {"Bip01 L Toe01", new Vector3(0, 0, 9.4069f)},

            {"Bip01 L Toe1", new Vector3(3.5193f, -4.6723f, -76.5831f)},
            {"Bip01 L Toe11", new Vector3(0, 0, 0)},

            {"Bip01 L Toe2", new Vector3(2.1073f, -1.896f, -73.169f)},
            {"Bip01 L Toe21", new Vector3(0, 0, 3.8861f)},

            {"Bip01 R Thigh", new Vector3(0, 180, 0)},
            {"Bip01 R Calf", new Vector3(0, 0, 0)},
            {"Bip01 R Foot", new Vector3(0, 0, 0)},

            {"Bip01 R Toe0", new Vector3(0.5203f, -13.8727f, 280.0909f)},
            {"Bip01 R Toe01", new Vector3(0, 0, 9.4069f)},

            {"Bip01 R Toe1", new Vector3(3.5193f, 4.6723f, -76.5831f)},
            {"Bip01 R Toe11", new Vector3(0, 0, 0)},

            {"Bip01 R Toe2", new Vector3(-2.1073f, 2.896f, -73.169f)},
            {"Bip01 R Toe21", new Vector3(0, 0, 3.8861f)},

            {"Bip01 L Clavicle", new Vector3(0, -270, 180)},
            {"Bip01 L UpperArm", new Vector3(0, -78, 0)},
            {"Bip01 L Forearm", new Vector3(0, 0, 0)},
            {"Bip01 L Hand", new Vector3(-90, 0, 0)},

            {"Bip01 L Finger0", new Vector3(50, -55, 50)},
            {"Bip01 L Finger01", new Vector3(0, 0, 0)},
            {"Bip01 L Finger02", new Vector3(0, 0, 0)},

            {"Bip01 L Finger1", new Vector3(-1.7799f, 6.6295f, 0)},
            {"Bip01 L Finger11", new Vector3(0, 0, -8.4914f)},
            {"Bip01 L Finger12", new Vector3(0, 0, 0)},

            {"Bip01 L Finger2", new Vector3(0.1171f, -0.324f, 0.3687f)},
            {"Bip01 L Finger21", new Vector3(0, 0, -15.69f)},
            {"Bip01 L Finger22", new Vector3(0, 0, 0)},

            {"Bip01 L Finger3", new Vector3(0.9134f, -1.2506f, -9.6762f)},
            {"Bip01 L Finger31", new Vector3(0, 0, -12.6351f)},
            {"Bip01 L Finger32", new Vector3(0, 0, 1.0027f)},

            {"Bip01 L Finger4", new Vector3(1.9802f, -2.6158f, 9.2883f)},
            {"Bip01 L Finger41", new Vector3(0, 0, -41.4269f)},
            {"Bip01 L Finger42", new Vector3(0, 0, 3.8928f)},

            {"Bip01 R Clavicle", new Vector3(0, 270, 180)},
            {"Bip01 R UpperArm", new Vector3(0, 78, 0)},
            {"Bip01 R Forearm", new Vector3(0, 0, 0)},
            {"Bip01 R Hand", new Vector3(90, 0, 0)},

            {"Bip01 R Finger0", new Vector3(-50, 55, 50)},
            {"Bip01 R Finger01", new Vector3(0, 0, 0)},
            {"Bip01 R Finger02", new Vector3(0, 0, 0)},

            {"Bip01 R Finger1", new Vector3(1.7799f, -6.6295f, 0)},
            {"Bip01 R Finger11", new Vector3(0, 0, -8.4914f)},
            {"Bip01 R Finger12", new Vector3(0, 0, 0)},

            {"Bip01 R Finger2", new Vector3(-0.1171f, 0.324f, 0.3687f)},
            {"Bip01 R Finger21", new Vector3(0, 0, -15.69f)},
            {"Bip01 R Finger22", new Vector3(0, 0, 0)},

            {"Bip01 R Finger3", new Vector3(-0.9134f, 1.2506f, -9.6762f)},
            {"Bip01 R Finger31", new Vector3(0, 0, -12.6351f)},
            {"Bip01 R Finger32", new Vector3(0, 0, 1.0027f)},

            {"Bip01 R Finger4", new Vector3(-1.9802f, 2.6158f, 9.2883f)},
            {"Bip01 R Finger41", new Vector3(0, 0, -41.4269f)},
            {"Bip01 R Finger42", new Vector3(0, 0, 3.8928f)}
        };
        /*
        private readonly Dictionary<string, Vector3> MaidFingersParamMin = new Dictionary<string, Vector3>()
        {
            {"Bip01 L Finger0", new Vector3(-90, -150, -150)},
            {"Bip01 L Finger01", new Vector3(-10, -10, -70)},
            {"Bip01 L Finger02", new Vector3(-10, -10, -90)},

            {"Bip01 L Finger1", new Vector3(-20, -20, -100)},
            {"Bip01 L Finger11", new Vector3(-10, -10, -110)},
            {"Bip01 L Finger12", new Vector3(-10, -10, -90)},

            {"Bip01 L Finger2", new Vector3(-20, -20, -100)},
            {"Bip01 L Finger21", new Vector3(-10, -10, -110)},
            {"Bip01 L Finger22", new Vector3(-10, -10, -90)},

            {"Bip01 L Finger3", new Vector3(-20, -20, -100)},
            {"Bip01 L Finger31", new Vector3(-10, -10, -110)},
            {"Bip01 L Finger32", new Vector3(-10, -10, -90)},

            {"Bip01 L Finger4", new Vector3(-20, -20, -100)},
            {"Bip01 L Finger41", new Vector3(-10, -10, -90)},
            {"Bip01 L Finger42", new Vector3(-10, -10, -90)},
            
            //同じですねぇ
            {"Bip01 R Finger0", new Vector3(-90, -150, -150)},
            {"Bip01 R Finger01", new Vector3(-10, -10, -70)},
            {"Bip01 R Finger02", new Vector3(-10, -10, -90)},

            {"Bip01 R Finger1", new Vector3(-20, -20, -100)},
            {"Bip01 R Finger11", new Vector3(-10, -10, -110)},
            {"Bip01 R Finger12", new Vector3(-10, -10, -90)},

            {"Bip01 R Finger2", new Vector3(-20, -20, -100)},
            {"Bip01 R Finger21", new Vector3(-10, -10, -110)},
            {"Bip01 R Finger22", new Vector3(-10, -10, -90)},

            {"Bip01 R Finger3", new Vector3(-20, -20, -100)},
            {"Bip01 R Finger31", new Vector3(-10, -10, -110)},
            {"Bip01 R Finger32", new Vector3(-10, -10, -90)},

            {"Bip01 R Finger4", new Vector3(-20, -20, -100)},
            {"Bip01 R Finger41", new Vector3(-10, -10, -90)},
            {"Bip01 R Finger42", new Vector3(-10, -10, -90)}
        };

        private readonly Dictionary<string, Vector3> MaidFingersParamMax = new Dictionary<string, Vector3>()
        {
            {"Bip01 L Finger0", new Vector3(90, 150, 150)},
            {"Bip01 L Finger01", new Vector3(10, 10, 20)},
            {"Bip01 L Finger02", new Vector3(10, 10, 20)},

            {"Bip01 L Finger1", new Vector3(20, 20, 55)},
            {"Bip01 L Finger11", new Vector3(10, 10, 10)},
            {"Bip01 L Finger12", new Vector3(10, 10, 10)},

            {"Bip01 L Finger2", new Vector3(20, 20, 55)},
            {"Bip01 L Finger21", new Vector3(10, 10, 10)},
            {"Bip01 L Finger22", new Vector3(10, 10, 10)},

            {"Bip01 L Finger3", new Vector3(20, 20, 55)},
            {"Bip01 L Finger31", new Vector3(10, 10, 10)},
            {"Bip01 L Finger32", new Vector3(10, 10, 10)},

            {"Bip01 L Finger4", new Vector3(20, 20, 55)},
            {"Bip01 L Finger41", new Vector3(10, 10, 42)},
            {"Bip01 L Finger42", new Vector3(10, 10, 15)},
            
            //同じですねぇ
            {"Bip01 R Finger0", new Vector3(90, 150, 150)},
            {"Bip01 R Finger01", new Vector3(10, 10, 20)},
            {"Bip01 R Finger02", new Vector3(10, 10, 20)},

            {"Bip01 R Finger1", new Vector3(20, 20, 55)},
            {"Bip01 R Finger11", new Vector3(10, 10, 10)},
            {"Bip01 R Finger12", new Vector3(10, 10, 10)},

            {"Bip01 R Finger2", new Vector3(20, 20, 55)},
            {"Bip01 R Finger21", new Vector3(10, 10, 10)},
            {"Bip01 R Finger22", new Vector3(10, 10, 10)},

            {"Bip01 R Finger3", new Vector3(20, 20, 55)},
            {"Bip01 R Finger31", new Vector3(10, 10, 10)},
            {"Bip01 R Finger32", new Vector3(10, 10, 10)},

            {"Bip01 R Finger4", new Vector3(20, 20, 55)},
            {"Bip01 R Finger41", new Vector3(10, 10, 42)},
            {"Bip01 R Finger42", new Vector3(10, 10, 15)}
        };

        private readonly Dictionary<string, Vector3> MaidToesParamMin = new Dictionary<string, Vector3>()
        {

        };

        private readonly Dictionary<string, Vector3> MaidToesParamMax = new Dictionary<string, Vector3>()
        {

        };
        */
        private Dictionary<string, Vector3> MaidBoneParam;

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

        public void Pose_Init()
        {
#if DEBUG
            Debuginfo.Log("Pose_Init()", 2);
#endif
            Pose_DataList = new Dictionary<string, List<PoseData>>();
            Pose_firstCatagory = string.Empty;
            MaidBoneParam = new Dictionary<string, Vector3>(MaidBoneParamZero);

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
            MaidBoneParam = null;
#if DEBUG
            Debuginfo.Log("EMES_Pose Finalized Done", 2);
#endif
        }

#region Maid Mirror
        public void Pose_MirrorAll(Maid maid, PoseMethod method)
        {
#if DEBUG
            Debuginfo.Log("Pose_MirrorAll PoseMethod = " + method, 2);
#endif
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            switch(method)
            {
                case PoseMethod.ALL:
                    foreach (string sBone in MaidHeadBones)
                        Pose_MirrorBone(maid, sBone);

                    foreach (string sBone in MaidUpperBones)
                        Pose_MirrorBone(maid, sBone);

                    foreach (string sBone in MaidLowerBones)
                        Pose_MirrorBone(maid, sBone);

                    foreach (string sBone in MaidLeftFilgerBones)
                        Pose_MirrorBone(maid, sBone);

                    foreach (string sBone in MaidLeftToeBones)
                        Pose_MirrorBone(maid, sBone);
                    break;
                case PoseMethod.Upper:
                    foreach (string sBone in MaidHeadBones)
                        Pose_MirrorBone(maid, sBone);

                    foreach (string sBone in MaidUpperBones)
                        Pose_MirrorBone(maid, sBone);
                    break;
                case PoseMethod.Lower:
                    foreach (string sBone in MaidLowerBones)
                        Pose_MirrorBone(maid, sBone);
                    break;
                case PoseMethod.Fingers:
                    foreach (string sBone in MaidLeftFilgerBones)
                        Pose_MirrorBone(maid, sBone);
                    break;
                case PoseMethod.Toes:
                    foreach (string sBone in MaidLeftToeBones)
                        Pose_MirrorBone(maid, sBone);
                    break;
            }
        }

        private void Pose_RotateBone(Maid maid, Transform trBone, float x, float y, float z)
        {
            string sBone = trBone.name;

            if (sBone == "Bip01 L Hand" || sBone == "Bip01 R Hand")
            {
                trBone.localRotation = Quaternion.identity;
                trBone.localRotation *= Quaternion.AngleAxis(z, Vector3.forward);
                trBone.localRotation *= Quaternion.AngleAxis(y, Vector3.up);
                trBone.localRotation *= Quaternion.AngleAxis(x, Vector3.right);
            }
            else
            {
                trBone.localRotation = Quaternion.identity;
                trBone.localRotation *= Quaternion.AngleAxis(z, Vector3.forward);
                trBone.localRotation *= Quaternion.AngleAxis(x, Vector3.right);
                trBone.localRotation *= Quaternion.AngleAxis(y, Vector3.up);
            }
        }

        private void Pose_MirrorBone(Maid maid, string sBone)
        {
            if(true == sBone.Equals("Bip01") && false == Super.Window.isMirrorBip01Checked())
            {
                return;
            }

            Transform trBase = maid.body0.m_Bones.transform;
            Transform trBone = CMT.SearchObjName(trBase, sBone, true);

            if (sBone.Contains("L") || sBone.Contains("R"))
            {
                string mir_sBone = sBone.Contains("L") ? sBone.Replace("L", "R") : sBone.Replace("R", "L");
                Transform trMirBone = CMT.SearchObjName(trBase, mir_sBone, true);

                float x;
                float y;
                float z;
                float mir_x;
                float mir_y;
                float mir_z;
                float tmp_x;
                float tmp_y;
                float tmp_z;

                FingerPose.Calc_trBone2Param(trBone, out x, out y, out z);
                FingerPose.Calc_trBone2Param(trMirBone, out mir_x, out mir_y, out mir_z);

                tmp_x = -x;
                x = -mir_x;
                mir_x = tmp_x;

                tmp_y = -y;
                y = -mir_y;
                mir_y = tmp_y;

                tmp_z = z;
                z = mir_z;
                mir_z = tmp_z;


                Pose_RotateBone(maid, trBone, x, y, z);
                Pose_RotateBone(maid, trMirBone, mir_x, mir_y, mir_z);
            }
            else
            {
                float x;
                float y;
                float z;
                FingerPose.Calc_trBone2Param(trBone, out x, out y, out z);

                if (sBone == "Bip01")
                {
                    x = -x;
                    z = -z + 180;
                }
                else
                {
                    if (sBone == "Bip01 Spine" || sBone == "Bip01 Pelvis")
                    {
                        z = -z + 180;
                        y = -y + 180;
                    }
                    else
                    {
                        x = -x;
                        y = -y;
                    }
                }

                Pose_RotateBone(maid, trBone, x, y, z);
            }
        }
        #endregion

#region Maid Pose
        public void Pose_Reset(Maid maid, PoseMethod method)
        {
#if DEBUG
            Debuginfo.Log("Pose_Reset PoseMethod = " + method, 2);
#endif

            Transform trBase = maid.body0.m_Bones.transform;
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            Dictionary<string, Vector3> MaidBoneList = new Dictionary<string, Vector3>();

            switch (method)
            {
                case PoseMethod.ALL:
                    foreach (string sBone in MaidHeadBones)
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);

                    foreach (string sBone in MaidUpperBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        if (sBone.Contains("L"))
                            MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }

                    foreach (string sBone in MaidLowerBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        if (sBone.Contains("L"))
                            MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }

                    foreach (string sBone in MaidLeftFilgerBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }

                    foreach (string sBone in MaidLeftToeBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }
                    break;
                case PoseMethod.Head:
                    foreach (string sBone in MaidHeadBones)
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                    break;
                case PoseMethod.Upper:
                    foreach (string sBone in MaidHeadBones)
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);

                    foreach (string sBone in MaidUpperBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        if(sBone.Contains("L"))
                            MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }
                    break;
                case PoseMethod.Lower:
                    foreach (string sBone in MaidLowerBones)
                    {
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                        if (sBone.Contains("L"))
                            MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    }
                    break;
                case PoseMethod.LeftFingers:
                    foreach (string sBone in MaidLeftFilgerBones)
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                    break;
                case PoseMethod.RightFingers:
                    foreach (string sBone in MaidLeftFilgerBones)
                        MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    break;
                case PoseMethod.LeftToes:
                    foreach (string sBone in MaidLeftToeBones)
                        MaidBoneList.Add(sBone, MaidBoneParamZero[sBone]);
                    break;
                case PoseMethod.RightToes:
                    foreach (string sBone in MaidLeftToeBones)
                        MaidBoneList.Add(sBone.Replace("L", "R"), MaidBoneParamZero[sBone.Replace("L", "R")]);
                    break;
            }

            Transform trBone;
            foreach (KeyValuePair<string, Vector3> sBone in MaidBoneList)
            {
                trBone = CMT.SearchObjName(trBase, sBone.Key, true);
                Pose_RotateBone(maid, trBone, sBone.Value.x, sBone.Value.y, sBone.Value.z);
            }            
        }

        public void Pose_ResetSelected(Maid maid)
        {
            Transform trBase = maid.body0.m_Bones.transform;
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            Action<string> resetBones = delegate (string sBone)
            {
                if (true == Super.Window.GetBonesSelected(sBone))
                {
                    Transform trBone = CMT.SearchObjName(trBase, sBone, true);
                    Pose_RotateBone(maid, trBone, MaidBoneParamZero[sBone].x, MaidBoneParamZero[sBone].y, MaidBoneParamZero[sBone].z);
                }
            };

            foreach (string sBone in MaidHeadBones)
            {
                resetBones(sBone);
            }

            foreach (string sBone in MaidUpperBones)
            {
                resetBones(sBone);
                if(true == sBone.Contains(" L "))
                    resetBones(sBone.Replace(" L ", " R "));
            }

            foreach (string sBone in MaidLowerBones)
            {
                if (true == sBone.Equals("Bip01"))
                    continue;

                resetBones(sBone);
                if (true == sBone.Contains(" L "))
                    resetBones(sBone.Replace(" L ", " R "));
            }
        }

        public void Pose_Copy(Maid maid)
        {
            Transform trBase = maid.body0.m_Bones.transform;
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            Transform trBone;
            float x;
            float y;
            float z;
            foreach (KeyValuePair<string, Vector3> sBone in MaidBoneParamZero)
            {
                trBone = CMT.SearchObjName(trBase, sBone.Key, true);
                FingerPose.Calc_trBone2Param(trBone, out x, out y, out z);
                MaidBoneParam[sBone.Key] = new Vector3(x, y, z);
            }
        }

        public void Pose_Paste(Maid maid)
        {
            Transform trBase = maid.body0.m_Bones.transform;
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            Transform trBone;
            foreach (KeyValuePair<string, Vector3> sBone in MaidBoneParam)
            {
                trBone = CMT.SearchObjName(trBase, sBone.Key, true);
                Pose_RotateBone(maid, trBone, sBone.Value.x, sBone.Value.y, sBone.Value.z);
            }
        }
        #endregion

#region Maid Finger Toe Open and Close
        public void Pose_FingerOpenAndClose(Maid maid, int iOpen, int iFirst, bool isRight)
        {
            Transform trBase = maid.body0.m_Bones.transform;
            Transform trBone;
            string sTrBone;
            
            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            foreach (string sBone in MaidLeftFilgerBones)
            {            
                if (true == isRight)
                {
                    sTrBone = sBone.Replace(" L ", " R ");
                }
                else
                {
                    sTrBone = sBone;
                }

                if (false == Super.Window.GetFingerToeLock(sTrBone))
                {
                    trBone = CMT.SearchObjName(trBase, sTrBone, true);
                    FingerBlendCustom.Blend(maid, trBone, iOpen, iFirst);
                }
            }
        }

        public void Pose_ToeOpenAndClose(Maid maid, int iOpen, int iFirst, bool isRight)
        {
            Transform trBase = maid.body0.m_Bones.transform;
            Transform trBone;
            string sTrBone;

            maid.body0.m_Bones.GetComponent<Animation>().Stop();

            foreach (string sBone in MaidLeftToeBones)
            {
                if (true == isRight)
                {
                    sTrBone = sBone.Replace(" L ", " R ");
                }
                else
                {
                    sTrBone = sBone;
                }

                if (false == Super.Window.GetFingerToeLock(sTrBone))
                {
                    trBone = CMT.SearchObjName(trBase, sTrBone, true);
                    FingerBlendCustom.Blend(maid, trBone, iOpen, iFirst);
                }
            }
        }
        #endregion
    }
}



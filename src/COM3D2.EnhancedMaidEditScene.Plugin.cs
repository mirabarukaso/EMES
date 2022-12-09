using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
#if SYBARIS
using UnityInjector;
using UnityInjector.Attributes;
#endif
#if BEPINEX
using BepInEx;
using BepInEx.Logging;
using System.Xml;
#endif

//バージョン　a.b.c.d
//a　メイン
//b　サブ
//c　スペシャルリリース
//d　改造リリース
#if SYBARIS
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyTitle("Enhanced Maid Edit Scene")]
#endif
[assembly: AssemblyCopyright("Free @Mirabarukaso")]

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
#if SYBARIS
    [PluginFilter("COM3D2x64"), PluginFilter("COM3D2OHx64")]
    [PluginName("EnhancedMaidEditScene"), PluginVersion("1.2.0.0")]
#endif
#if BEPINEX
    [BepInProcess("COM3D2x64"), BepInProcess("COM3D2OHx64")]
    [BepInPlugin("org.bepinex.plugins.enhancedmaideditscene", "Enhanced Maid Edit Scene", "1.2.0.1")]
#endif

#if SYBARIS
    public class EMES : PluginBase
#endif
#if BEPINEX
    public class EMES : BaseUnityPlugin
#endif
    {
#region Constants
        public const string PluginName = "EnhancedMaidEditScene"; 
#if SYBARIS
        public const string PluginVersion = "1.2.0.0";
#endif
#if BEPINEX
        public const string PluginVersion = "1.2.0.1";
#endif

        private readonly int iSceneEdit = 5; //メイン版エディットモード
        private readonly int iSceneTitle = 9; //タイトル画面
        private readonly int iScenePhoto = 26; //メイン版公式撮影モード
        private readonly int iSceneEditCBL = 4; //CBL版エディットモード
        //private readonly int iSceneWarning = 9; //CBL版起動時警告画面
        private readonly int iScenePhotoCBL = 21; //CBL版公式撮影モード
        private readonly float TimePerInit = 1.00f;

        private readonly bool bForceIniRemove = false; 
#endregion

#region Variables
        private int sceneLevel;
        private bool bCBLMode = false;
        private bool bInitCompleted = false;
        private bool bIniLoaded = false;
        private bool bRequestLoadMaids = false;
        private bool bRequestScaleHandles = false;

        private int ActivedMaidCount = 1;
        private float Xwarp = 0.4f;

        private bool bReloadingMaid = false;
        private int CustomMaidIndexID;

        public EMES_MaidIK MaidIK;
        public EMES_Yotogi Yotogi;
        public EMES_Pose Pose;
        public EMES_Items Items;
        public EMES_Dance Dance;
        public EMES_MaidParts Parts;

        public EMES_Window Window;
        public SettingsXML settingsXml;
        public CameraPlus camPlus;
        public ExtraShader exShader;

        public EMES_SceneManagement sceneManagement;
        public MaidTailsLite maidTails;
        #endregion

        EMES()
        {
#if BEPINEX
            Debuginfo.Init();
#endif
            MaidIK = new EMES_MaidIK(this);
            Yotogi = new EMES_Yotogi(this);
            Pose = new EMES_Pose(this);
            Items = new EMES_Items(this);
            Dance = new EMES_Dance();
            Parts = new EMES_MaidParts(this);
        }

#region MonoBehaviour methods
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            Finalized();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void SetReloadingMaid(bool bTrigger, int iCustomMaidIndexID)
        {
            bInitCompleted = !bTrigger;
            bReloadingMaid = bTrigger;
            CustomMaidIndexID = iCustomMaidIndexID;
            Window.ToggleWindow(!bTrigger);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            int level = scene.buildIndex;

            if (iSceneTitle == level) //9
            {
                if (false == bIniLoaded)
                {
                    SetConfigurefile();
                    if (settingsXml.DebugLogLevel > 2)
                        settingsXml.DebugLogLevel = 2;

                    Debuginfo.settingLevel = settingsXml.DebugLogLevel;
                    Debuginfo.Log("デバッグログレベル = " + settingsXml.DebugLogLevel, 0);

#if DEBUG
                    Yotogi.Yotogi_RefreshCache();
#else
                    if(true == settingsXml.bYotogiRefreshCache)
                    {
                        settingsXml.bYotogiRefreshCache = false;
                        SaveConfigurefile();
                        Yotogi.Yotogi_RefreshCache();
                    }
                    else
                    {
                        Yotogi.Yotogi_LoadCache();
                    }
#endif
                    FingerPose.LoadFingerPose(GetConfigDirectory());
                    FingerBlendCustom.LoadFingerBlendData();
                    bIniLoaded = true;
                }

                //CBL版か通常版かこのタイミングで判断させる
                if (File.Exists(Directory.GetCurrentDirectory() + "COM3D2OH.exe"))
                {
                    bCBLMode = true;
                }
                else
                {
                    bCBLMode = false;
                }
            }

            //SceneLevel == 4(CBL版エディットモード)  と　SceneLevel21 == 21（CBL版公式撮影モード） も追加
            if (level != sceneLevel && sceneLevel == GetPhotoModeSceneNo())
            {
                Finalized();
            }

            if (level == GetEditModeSceneNo())
            {
                StartCoroutine(InitCoroutine());
            }

            sceneLevel = level;
        }

        public void Update()
        {
            if (true == bInitCompleted)
            {
                if (sceneLevel == GetEditModeSceneNo() && false == bRequestLoadMaids)
                {
                    Window.ProcessShortcurtHotkey();
                    Items.Items_SyncPosRotFromHandle();

                    maidTails.ProcessHandle();
                    if (true == maidTails.CheckShippoChange(Window.CurrentSelectedMaid))
                    {
                        Window.MaidTails_DisableAutoIK();
                        Window.MaidTails_Init();
                    }

                    if (true == Window.GetCameraMovement())
                    {
                        camPlus.CameraKeyProcess(settingsXml, Window.CurrentSelectedMaid, Window.CurrentSelectedMaid.body0.transform);
                    }
                }
                else if (sceneLevel != GetEditModeSceneNo())
                {
                    Finalized();
                }
            }
            else
            {
                if(true == bReloadingMaid)
                {
                    if (true == GameMain.Instance.CharacterMgr.IsBusy())
                    {
                        Debuginfo.Log("リロード中...", 0);
                    }
                    else
                    {
                        Window.ReloadedMaid(CustomMaidIndexID);
                        SetReloadingMaid(false, -1);
                        Debuginfo.Log("リロード完了", 0);
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (true == bInitCompleted)
            {
                if (true == bRequestLoadMaids)
                {
                    ProcessMaidsLoading();
                }
                else if(false == MaidIK.IsLockIK())
                {
                    EMES_Window.HandleSelectMode hsMode = Window.GetMaidHandleSelectMode();
                    int iID = Window.GetCurrentMaidStockID();
                    if (EMES_Window.HandleSelectMode.None != hsMode)
                    {
                        for (int i = 0; i < Window.CurrentMaidsStockID.Count; i++)
                        {
                            if (false == Window.CheckMaidSelectMode(hsMode, i, iID))
                                continue;

                            Window.CheckIfNeedAddCustomFaceBlend(Window.CurrentSelectedMaid);

                            if (null == MaidIK.MaidsIK[Window.CurrentMaidsStockID[i]].handleEx[EMES_MaidIK.BoneType.Root].GetParentBone())
                            {
                                MaidIK.IK_Init(Window.CurrentMaidsList[i].status.guid, Window.CurrentMaidsStockID[i]);
                            }

                            bool bBip01Dragged = false;
                            //パフォーマンスの向上
                            MaidIK.IK_CheckInvisible(Window.CurrentMaidsStockID[i]);
                            bBip01Dragged = MaidIK.IK_SyncFromHandle(Window.CurrentMaidsStockID[i]);
                            MaidIK.IK_Porc(Window.CurrentMaidsStockID[i]);

                            if (true == bBip01Dragged && Window.CurrentSelectedMaid == GameMain.Instance.CharacterMgr.GetStockMaid(Window.CurrentMaidsStockID[i]))
                            {
                                Window.MaidOffset_UpdateOffsetInfo(Window.CurrentSelectedMaid);
                            }
                        }
                    }

                    if (true == maidTails.bIsAutoIK)
                    {
                        foreach (EMES_MaidIK.EMES_IK.IK ik in maidTails.AutoIK)
                        {
                            if (true == MaidIK.IK_SyncHandle(Window.CurrentSelectedMaid, ik.handle))
                            {
                                Vector3 pos = ik.handle.GetParentBone().position + ik.handle.DeltaVector();
                                ik.GetIKCMO.Porc(ik.hip, ik.knee, ik.ankle, pos, Vector3.zero);
                            }
                        }
                    }
                }
            }
        }
#endregion

#region Public methods
        public void RequestScaleHandles(float fValue)
        {
            MaidIK.LockIK();
            Window.LockBusy();
            bRequestScaleHandles = true;
            StartCoroutine(StartScaleHandles(fValue));
        }

        public void RequestLoadMaids()
        {
            MaidIK.LockIK();
            Window.LockBusy();

            GameMain.Instance.CharacterMgr.VisibleAll(false);
            if (Window.CurrentMaidsStockID.Count > 0)
            {
                bRequestLoadMaids = true;
            }

            ActivedMaidCount = 1;
            Xwarp = 0.4f;

            Window.CurrentMaidsList[0] = GameMain.Instance.CharacterMgr.CharaVisible(0, true, false);
        }

        public int GetPhotoModeSceneNo()
        {
            return (bCBLMode ? iScenePhotoCBL : iScenePhoto);
        }

        public int GetEditModeSceneNo()
        {
            return (bCBLMode ? iSceneEditCBL : iSceneEdit);
        }

        public void PerformPose(Maid maid, EMES_Pose.PoseData poseData)
        {
#if COM3D25
            GameMain.Instance.ScriptMgr.StopMotionScript();
            if (maid == null || maid.body0 == null || maid.body0.m_Bones == null || maid.IsBusy)
            {
                return;
            }

            HandFootIKCtrl ikctrl = maid.fullBodyIK.GetIKCtrl<HandFootIKCtrl>(FullBodyIKMgr.IKEffectorType.Foot_L);
            HandFootIKCtrl ikctrl2 = maid.fullBodyIK.GetIKCtrl<HandFootIKCtrl>(FullBodyIKMgr.IKEffectorType.Foot_R);
            
            if (!maid.boMAN)
            {
                maid.body0.SetMuneYureLWithEnable(Window.GetMuneYureCheckBox());
                maid.body0.SetMuneYureRWithEnable(Window.GetMuneYureCheckBox());
                if (ikctrl)
                {
                    ikctrl.isSetHighAngle = true;
                }
                if (ikctrl2)
                {
                    ikctrl2.isSetHighAngle = true;
                }
            }

            if (!string.IsNullOrEmpty(poseData.direct_file))
            {
                maid.fullBodyIK.AllIKDetach(0f);
                if (!poseData.is_mod && !poseData.is_mypose)
                {
                    maid.CrossFade(poseData.direct_file, false, poseData.is_loop, false, 0f, 1f);
                }
                else
                {
                    byte[] array = new byte[0];
                    try
                    {
                        using (FileStream fileStream = new FileStream(poseData.direct_file, FileMode.Open, FileAccess.Read))
                        {
                            array = new byte[fileStream.Length];
                            fileStream.Read(array, 0, array.Length);
                        }
                    }
                    catch
                    {
                    }
                    if (0 < array.Length)
                    {
                        if (ikctrl)
                        {
                            ikctrl.isSetHighAngle = false;
                        }
                        if (ikctrl2)
                        {
                            ikctrl2.isSetHighAngle = false;
                        }
                        maid.body0.CrossFade(poseData.id.ToString(), array, false, poseData.is_loop, false, 0f, 1f);
                        maid.SetAutoTwistAll(true);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(poseData.call_script_fil) && !string.IsNullOrEmpty(poseData.call_script_label))
            {
                ScriptManager scriptMgr = GameMain.Instance.ScriptMgr;
                if (poseData.fileSystem != GameUty.FileSystem)
                {
                    scriptMgr.compatibilityMode = true;
                    scriptMgr.is_motion_blend = false;
                }
                else
                {
                    scriptMgr.compatibilityMode = false;
                    scriptMgr.is_motion_blend = true;
                }

                CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
                int sloat = 0;
                for (int j = 0; j < characterMgr.GetMaidCount(); j++)
                {
                    if (maid == characterMgr.GetMaid(j))
                    {
                        sloat = j;
                        break;
                    }
                }
                GameMain.Instance.ScriptMgr.LoadMotionScript(sloat, false, poseData.call_script_fil, poseData.call_script_label, maid.status.guid, string.Empty, true, true, false, false);
            }
#else
            GameMain.Instance.ScriptMgr.StopMotionScript();
            if (maid == null || maid.body0 == null || maid.body0.m_Bones == null || maid.IsBusy)
            {
                return;
            }
            if (!maid.boMAN)
            {
                maid.body0.MuneYureL((float)((!Window.GetMuneYureCheckBox()) ? 1 : 0));
                maid.body0.MuneYureR((float)((!Window.GetMuneYureCheckBox()) ? 1 : 0));
                maid.body0.jbMuneL.enabled = Window.GetMuneYureCheckBox();
                maid.body0.jbMuneR.enabled = Window.GetMuneYureCheckBox();
            }
            if (!string.IsNullOrEmpty(poseData.direct_file))
            {
                maid.IKTargetToBone("左手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, IKCtrlData.IKExecTiming.Normal);
                maid.IKTargetToBone("右手", null, "無し", Vector3.zero, IKCtrlData.IKAttachType.Point, false, false, IKCtrlData.IKExecTiming.Normal);
                if (!poseData.is_mod && !poseData.is_mypose)
                {
                    maid.CrossFade(poseData.direct_file, false, poseData.is_loop, false, 0f, 1f);
                }
                else
                {
                    byte[] array = new byte[0];
                    try
                    {
                        using (FileStream fileStream = new FileStream(poseData.direct_file, FileMode.Open, FileAccess.Read))
                        {
                            array = new byte[fileStream.Length];
                            fileStream.Read(array, 0, array.Length);
                        }
                    }
                    catch
                    {
                    }
                    if (0 < array.Length)
                    {
                        maid.body0.CrossFade(poseData.id.ToString(), array, false, poseData.is_loop, false, 0f, 1f);
                        Maid.AutoTwist[] array2 = new Maid.AutoTwist[]
                        {
                        Maid.AutoTwist.ShoulderL,
                        Maid.AutoTwist.ShoulderR,
                        Maid.AutoTwist.WristL,
                        Maid.AutoTwist.WristR,
                        Maid.AutoTwist.ThighL,
                        Maid.AutoTwist.ThighR
                        };
                        for (int i = 0; i < array2.Length; i++)
                        {
                            maid.SetAutoTwist(array2[i], true);
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(poseData.call_script_fil) && !string.IsNullOrEmpty(poseData.call_script_label))
            {
                CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
                int sloat = 0;
                for (int j = 0; j < characterMgr.GetMaidCount(); j++)
                {
                    if (maid == characterMgr.GetMaid(j))
                    {
                        sloat = j;
                        break;
                    }
                }
                GameMain.Instance.ScriptMgr.LoadMotionScript(sloat, false, poseData.call_script_fil, poseData.call_script_label, maid.status.guid, string.Empty, true, true, false);
            }
#endif
        }

        public void PerformPose(Maid maid, string ANM)
        {
            maid.SetAutoTwistAll(true);
            if (ANM.StartsWith("dance_"))
            {
                maid.CrossFade(ANM, false, false, false, 0f, 1f);
            }
            else
            {
                if (false == maid.body0.m_Bones.GetComponent<Animation>().GetClip(ANM))
                {
                    try
                    {
                        using (AFileBase aFileBase = GameUty.FileOpen(ANM))
                        {
                            if (true == aFileBase.IsValid())
                            {
                                maid.body0.LoadAnime(ANM, GameUty.FileSystem, ANM, false, Window.GetMaidPoseStopAnimeCheckBox());
                            }
                        }
                    }
                    catch 
                    {
#if DEBUG
                        Debuginfo.Log("Load ANM using System.IO >> " + ANM, 2);
#endif
                        byte[] poseANMfromFile = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() + settingsXml.ANMFilesDirectory + ANM);
                        Yotogi.Yotogi_LoadAnime(maid, ANM, poseANMfromFile, false, Window.GetMaidPoseStopAnimeCheckBox());
                    }
                }
                maid.body0.m_Bones.GetComponent<Animation>().Play(ANM);
            }
        }

        public void SaveConfigurefile()
        {
            //ここでファイル読み込み処理
            string sFileName = GetConfigDirectory() + "EMES.xml";

            //ファイルがない場合は作成
            if (!File.Exists(sFileName) && null == settingsXml)
            {
                settingsXml = new SettingsXML();
            }
            ConfigureFileHelper.SaveSettings(settingsXml, sFileName);
        }
#endregion

#region Private methods
        private IEnumerator StartScaleHandles(float fValue)
        {
            while (bRequestScaleHandles = ScaleHandles(fValue))
            {
                yield return new WaitForSeconds(TimePerInit);
            }
            MaidIK.UnLockIK();
            Window.UnlockBusy();
        }

        private bool ScaleHandles(float fValue)
        {
            try
            {                
                foreach (KeyValuePair<int, EMES_MaidIK.EMES_IK> ik in MaidIK.MaidsIK)
                {
                    if (false == ik.Value.maid.Visible)
                        continue;
                    Dictionary<EMES_MaidIK.BoneType, HandleEx> handles = ik.Value.handleEx;
                    foreach (KeyValuePair<EMES_MaidIK.BoneType, HandleEx> handle in handles)
                    {
                        handle.Value.Scale = fValue;
                    }
                }
            }
            catch (Exception ex) { Debuginfo.Error("ScaleHandles() -> " + ex); return true; }
            return false;
        }

        private IEnumerator InitCoroutine()
        {
            while (!(bInitCompleted = Initialize()))
            {
                yield return new WaitForSeconds(TimePerInit);
            }
            Debuginfo.Log("Initialization complete.", 0);
        }

        private bool Initialize()
        {
            try
            {
                Pose.Pose_Init();
                Items.Items_PreInit();
                Dance.Dance_InitDanceDataList();

                exShader = new ExtraShader();
                camPlus = exShader.GetParent();
                camPlus.SetupInstance(this);
                sceneManagement = new EMES_SceneManagement(GetConfigDirectory());
               
                maidTails = new MaidTailsLite();
                maidTails.SetupInstance(this);
                maidTails.DFS_Init();

                Window = new EMES_Window();
                bInitCompleted = Window.Init(this, maidTails);                
            }
            catch (Exception ex) 
            {
                Debuginfo.Error("initialize() -> " + ex);
                Finalized();
                return false; 
            }

            return true;
        }

        private void SetConfigurefile() //BoneSliderから参照
        {
            //ここでiniファイル読み込み処理
            string fileName = GetConfigDirectory() + "EMES.ini";

            //iniファイルがない場合はxmlファイル読み込み処理
            if (!File.Exists(fileName))
            {
                fileName = GetConfigDirectory() + "EMES.xml";
                
                if (!File.Exists(fileName))
                {
                    Debuginfo.Log("新しいXMLファイルを作成する", 0);
                    settingsXml = new SettingsXML();
                }
                else
                {
                    settingsXml = ConfigureFileHelper.LoadSettings(fileName, bForceIniRemove);                    
                }
            }
            else
            {
                settingsXml = ConfigureFileHelper.Read<SettingsXML>("setting", fileName);
                if (PluginVersion != settingsXml.VERSION && true == bForceIniRemove)
                {
                    Debuginfo.Warning("ini ファイルの衝突検出、自動的に初期設定に戻す", 0);
                    settingsXml = null;
                }
                else
                {
                    ConfigureFileHelper.FixSettings(settingsXml);
                }
                Debuginfo.Warning("EMES: xmlに移行する", 0);
                File.Delete(fileName);
            }

            SaveConfigurefile();
        }

        private void ProcessMaidsLoading()
        {
            if (true == bInitCompleted)
            {
                if (true == GameMain.Instance.CharacterMgr.IsBusy())
                {
                    Debuginfo.Log("CharacterMgrはビジー状態です、しばらくお待ちください", 1);
                }
                else
                {
                    if (ActivedMaidCount < Window.CurrentMaidsStockID.Count)
                    {
                        Debuginfo.Log("読み込んでいます...", 1);
                        Window.CurrentMaidsList[ActivedMaidCount] = GameMain.Instance.CharacterMgr.Activate(ActivedMaidCount, Window.CurrentMaidsStockID[ActivedMaidCount], false, false);
                        Window.CurrentMaidsList[ActivedMaidCount] = GameMain.Instance.CharacterMgr.CharaVisible(ActivedMaidCount, true, false);
                        ActivedMaidCount++;
                    }
                    else
                    {
                        bRequestLoadMaids = false;
                        Vector3 maidPos = Window.CurrentMaidsList[0].GetPos();
                        if (true == Window.GetMaidLoadingKeepPose() && true == Window.LastMaidsGUID.Contains(Window.CurrentMaidsList[0].status.guid))
                        {
                            
                        }
                        else
                        {
                            PerformPose(Window.CurrentMaidsList[0], Pose.Pose_DataList[Pose.Pose_firstCatagory][0]);
                        }

                        for (int Index = 1; Index < ActivedMaidCount; Index++)    
                        {
                            if (true == Window.GetMaidLoadingKeepPosRot() && true == Window.LastMaidsGUID.Contains(Window.CurrentMaidsList[Index].status.guid))
                            {

                            }
                            else
                            {
                                if (0 == (Index % 2))
                                {
                                    Window.CurrentMaidsList[Index].SetPos(new Vector3(maidPos.x + Xwarp, maidPos.y, maidPos.z));
                                    Xwarp += 0.4f;
                                }
                                else
                                {
                                    Window.CurrentMaidsList[Index].SetPos(new Vector3(maidPos.x - Xwarp, maidPos.y, maidPos.z));
                                }
                            }

                            if (true == Window.GetMaidLoadingKeepPose() && true == Window.LastMaidsGUID.Contains(Window.CurrentMaidsList[Index].status.guid))
                            {

                            }
                            else
                            {
                                PerformPose(Window.CurrentMaidsList[Index], Pose.Pose_DataList[Pose.Pose_firstCatagory][0]);
                            }
                            MaidIK.IK_Init(Window.CurrentMaidsList[Index], Window.CurrentMaidsStockID[Index]);
                        }
                        MaidIK.IK_Collect();
                        Debuginfo.Log("ロード完了", 0);
                        GameMain.Instance.MainCamera.FadeIn(1f, false, null, true, true, new Color());
                        MaidIK.UnLockIK();
                        Window.UnlockBusy();
                        Window.SwitchToCurrentSelectedMaids();
                        Window.MaidPoseCopy_Init();
                        Window.ToggleWindow(true);
                        GC.Collect();
                    }
                }
            }
        }

        public void Finalized()
        {
            if (false == bInitCompleted)
                return;

            Debuginfo.Log("EMES finalize...", 0);
            bInitCompleted = false;

            if (null != Window)
            {
                Window.ToggleWindow(false);
                Window.Finalized();
                Window.Dispose();
                Window = null;
            }

            camPlus.CameraPlus_Finalized();
            exShader = null;
            camPlus = null;

            sceneManagement.Finalized();
            sceneManagement = null;

            maidTails.Finalized();
            maidTails = null;

            Parts.Parts_Finalized();
            Dance.Dance_Finalized();
            MaidIK.IK_Finalized();
            Items.Items_Finalized();
            Pose.Pose_Finalized();
            //Yotogi
            Debuginfo.Log("EMES finalize... all DONE!", 0);
        }

        public string GetConfigDirectory()
        {
#if SYBARIS
            //Sybaris環境かどうかチェック
            if (File.Exists(Directory.GetCurrentDirectory() + @"\opengl32.dll") && Directory.Exists(Directory.GetCurrentDirectory() + @"\Sybaris"))
            {
                return Directory.GetCurrentDirectory() + @"\Sybaris\UnityInjector\Config\";
            }
#endif

#if BEPINEX
            //BepInEx環境かどうかチェック
            if (File.Exists(Directory.GetCurrentDirectory() + @"\winhttp.dll") && Directory.Exists(Directory.GetCurrentDirectory() + @"\BepInEx\plugins\EnhancedMaidEditScene\Config"))
            {
                return Directory.GetCurrentDirectory() + @"\BepInEx\plugins\EnhancedMaidEditScene\Config\";
            }
#endif
            else
            {
                Debuginfo.Error("環境チェックに失敗しました");
            }

            return Directory.GetCurrentDirectory() + @"\";
        }
#endregion

#region Utility methods
        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }

        internal static Transform FindParent(Transform tr, string s) { return FindParent(tr.gameObject, s).transform; }

        internal static GameObject FindParent(GameObject go, string name)
        {
            if (go == null) return null;

            Transform _parent = go.transform.parent;
            while (_parent)
            {
                if (_parent.name == name) return _parent.gameObject;
                _parent = _parent.parent;
            }

            return null;
        }

        internal static Transform FindChild(Transform tr, string s) { return FindChild(tr.gameObject, s).transform; }
        internal static GameObject FindChild(GameObject go, string s)
        {
            if (go == null) return null;
            GameObject target = null;

            foreach (Transform tc in go.transform)
            {
                if (tc.gameObject.name == s) return tc.gameObject;
                target = FindChild(tc.gameObject, s);
                if (target) return target;
            }

            return null;
        }

        internal static Transform FindChildByTag(Transform tr, string s) { return FindChildByTag(tr.gameObject, s).transform; }
        internal static GameObject FindChildByTag(GameObject go, string s)
        {
            if (go == null) return null;
            GameObject target = null;

            foreach (Transform tc in go.transform)
            {
                if (tc.gameObject.name.Contains(s)) return tc.gameObject;
                target = FindChild(tc.gameObject, s);
                if (target) return target;
            }

            return null;
        }


        internal static void SetChild(GameObject parent, GameObject child)
        {
            child.layer = parent.layer;
            child.transform.parent = parent.transform;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.rotation = Quaternion.identity;
        }

        internal static GameObject SetCloneChild(GameObject parent, GameObject orignal, string name)
        {
            GameObject clone = UnityEngine.Object.Instantiate(orignal) as GameObject;
            if (!clone) return null;

            clone.name = name;
            SetChild(parent, clone);

            return clone;
        }

        internal static void ReleaseChild(GameObject child)
        {
            child.transform.parent = null;
            child.SetActive(false);
        }

        internal static void DestoryChild(GameObject parent, string name)
        {
            GameObject child = FindChild(parent, name);
            if (child)
            {
                child.transform.parent = null;
                GameObject.Destroy(child);
            }
        }

        internal static UIAtlas FindAtlas(string s)
        {
            return ((new List<UIAtlas>(Resources.FindObjectsOfTypeAll<UIAtlas>())).FirstOrDefault(a => a.name == s));
        }

        internal static void WriteTrans(string s)
        {
            GameObject go = GameObject.Find(s);
            if (!go) return;

            WriteTrans(go.transform, 0, null);
        }
        internal static void WriteTrans(Transform t) { WriteTrans(t, 0, null); }
        internal static void WriteTrans(Transform t, int level, StreamWriter writer)
        {
            if (level == 0) writer = new StreamWriter(@".\" + t.name + @".txt", false);
            if (writer == null) return;

            string s = "";
            for (int i = 0; i < level; i++) s += "    ";
            writer.WriteLine(s + level + "," + t.name);
            foreach (Transform tc in t)
            {
                WriteTrans(tc, level + 1, writer);
            }

            if (level == 0) writer.Close();
        }

        internal static void WriteChildrenComponent(GameObject go)
        {
            WriteComponent(go);

            foreach (Transform tc in go.transform)
            {
                WriteChildrenComponent(tc.gameObject);
            }
        }

        internal static void WriteComponent(GameObject go)
        {
            Component[] compos = go.GetComponents<Component>();
            foreach (Component c in compos) { Debuginfo.Log(go.name + ":" + c.GetType().Name, 2); }
        }

        internal static FieldInfo GetFieldInfo<T>(string name)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            return typeof(T).GetField(name, bindingAttr);
        }

        internal static TResult GetFieldValue<T, TResult>(T inst, string name)
        {
            TResult result;
            if (inst == null)
            {
                result = default(TResult);
            }
            else
            {
                FieldInfo fieldInfo = GetFieldInfo<T>(name);
                if (fieldInfo == null)
                {
                    result = default(TResult);
                }
                else
                {
                    result = (TResult)((object)fieldInfo.GetValue(inst));
                }
            }
            return result;
        }

        internal static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value)
        {
            GetFieldInfo<TType>(name).SetValue(instance, value);
        }

        internal static void ResetProp(Maid maid, MPN idx)
        {
            MaidProp[] fieldValue = GetFieldValue<Maid, MaidProp[]>(maid, "m_aryMaidProp");
            MaidProp maidProp = fieldValue[(int)idx];
            if (maidProp.nTempFileNameRID != 0 && maidProp.nFileNameRID != maidProp.nTempFileNameRID)
            {
                maidProp.boDut = true;
                maidProp.strTempFileName = string.Empty;
                maidProp.nTempFileNameRID = 0;
                maidProp.boTempDut = false;
            }
        }

        internal static GameObject[] FindGameObjectsInLayer(int layer)
        {
            var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == layer)
                {
                    goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///     文字列が数値であるかどうかを返します。</summary>
        /// <param name="stTarget">
        ///     検査対象となる文字列。<param>
        /// <returns>
        ///     指定した文字列が数値であれば true。それ以外は false。</returns>
        /// -----------------------------------------------------------------------------
        internal static bool IsNumeric(string stTarget)
        {
            double nullNumber;

            return double.TryParse(
                stTarget,
                System.Globalization.NumberStyles.Any,
                null,
                out nullNumber
            );
        }
        #endregion
    }

#region DebugInfo
    //Debuginfo.Logの代わりにLoginfo.Logを使う
    public static class Debuginfo
    {
        public static int settingLevel = 0;
        public static string premessage = "[EMES]";
        private static string lastmessage = "";
        //_messageLV 0：常に表示 1：公開デバッグモード用メッセージ 2：個人テスト用メッセージ

#if BEPINEX
        private static ManualLogSource myLogSource;

        public static void Init()
        {
            myLogSource = new ManualLogSource("EMES");
            BepInEx.Logging.Logger.Sources.Add(myLogSource);
        }
#endif

        public static void Log(string _message, int _messageLv)
        {
            if (true == lastmessage.Equals(_message))
            {
                return;
            }
            else
            {
                lastmessage = null;
                lastmessage = _message.Clone().ToString();
            }

            if (_messageLv <= settingLevel)
            {
#if SYBARIS
                Debug.Log(premessage + _message);
#endif
#if BEPINEX
                myLogSource.LogInfo(_message);
#endif
            }
        }

        public static void Warning(string _message, int _messageLv)
        {
            if (true == lastmessage.Equals(_message))
            {
                return;
            }
            else
            {
                lastmessage = null;
                lastmessage = _message.Clone().ToString();
            }

            if (_messageLv <= settingLevel)
            {
#if SYBARIS
                Debug.LogWarning(premessage + _message);
#endif
#if BEPINEX
                myLogSource.LogWarning(_message);
#endif
            }
        }

        public static void Error(string _message)
        {
#if SYBARIS
            Debug.LogError(premessage + _message);
#endif
#if BEPINEX
            myLogSource.LogError(_message);
#endif
        }
    }
#endregion

}


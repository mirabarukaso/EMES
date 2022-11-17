using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public partial class EMES_Window : Form
    {
        #region Variables
        private readonly int GP01FaceVersion = 110;

        private EMES Super;
        private MaidTailsLite MT;
        private bool bShowWindow = false;
        private bool ignoreShowWindow = false;
        private bool bIsBusy = false;
        private bool bSyncFaceBlendTrackBar = false;
        private System.Threading.Timer FaceBlendTimer = null;

        public Maid[] CurrentMaidsList = new Maid[18];
        public List<int> CurrentMaidsStockID { get; private set; }
        public List<string> LastMaidsGUID { get; private set; }

        private int OrignalSelectedMaidStockID = 0;
        public Maid CurrentSelectedMaid { get; private set; }
        private int CurrentSelectedMaidIndex = 0;
        private List<string> ActivedSettingsHotkey;
        private Dictionary<Maid, string> Dance_AllOther;

        public List<HandleEx> CamPlus_SubLightHandleList;

        private class BackupMaidPosRotScale
        {
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Scale;
        };
        private BackupMaidPosRotScale MaidOffset_BackupMaidPosRotScale;

        public enum HandleSelectMode
        {
            All,
            Current,
            Others,
            None
        };

        enum LastPerformedPose
        {
            None,
            Official,
            ANM,
            Dance,
            Yotogi
        };
        private LastPerformedPose lastPerformedPose = LastPerformedPose.None;

        enum Settings_HotkeyMaid
        {
            Head,
            Neck,
            FingerX,
            FingerX1,
            FingerX2,
            ArmIK,
            LegIK,
            RClavicle,
            LClavicle,
            Hide,
            MaidPos,
            MaidRot
        };

        enum Settings_HotkeyItem
        {
            ItemReloadParticle,
            ItemReset,
            ItemPos,
            ItemRot,
            ItemSize,
            ItemDelete,
        };

        enum Settings_HotkeyDance
        {
            DanceStart,
            DanceAllOtherStart
        };

        enum Settings_HotkeyCamera
        {
            CameraQuickSave,
            CameraQuickLoad,
            CameraScreenShot
        };

        enum Settings_HotkeyCameraMovement
        {
            CameraMoveLeft,
            CameraMoveRight,
            CameraMoveForward,
            CameraMoveBackward,
            CameraMoveUp,
            CameraMoveDown,

            CameraRotateHorizontalLeft,
            CameraRotateHorizontalRight,
            CameraRotateVerticalUp,
            CameraRotateVerticalDown,
            CameraRotateLeft,
            CameraRotateRight,
            CameraResetToMaid,

            CameraDistanceClose,
            CameraDistanceFar,
            CameraFieldOfViewWider,
            CameraFieldOfViewNarrower,
            CameraMovementFaster,
            CameraMovementSlower,
            CameraMoveMaidToView
        };

        private readonly Dictionary<string, string> HotkeyToHandle = new Dictionary<string, string>()
        {
            //{"Settings_Hotkey" + "MaidPos" + "_CheckBox", "Offset_PositionCheckbox" },
            //{"Settings_Hotkey" + "MaidRot" + "_CheckBox", "Offset_RotationCheckbox" },
            {"Settings_Hotkey" + "Head" + "_CheckBox", "Bip01_Head" },
            {"Settings_Hotkey" + "Neck" + "_CheckBox", "Bip01_Neck" },
            {"Settings_Hotkey" + "FingerX" + "_CheckBox", "Bip01_L_Finger0|Bip01_L_Finger1|Bip01_L_Finger2|Bip01_L_Finger3|Bip01_L_Finger4|Bip01_R_Finger0|Bip01_R_Finger1|Bip01_R_Finger2|Bip01_R_Finger3|Bip01_R_Finger4" },
            {"Settings_Hotkey" + "FingerX1" + "_CheckBox", "Bip01_L_Finger01|Bip01_L_Finger11|Bip01_L_Finger21|Bip01_L_Finger31|Bip01_L_Finger41|Bip01_R_Finger01|Bip01_R_Finger11|Bip01_R_Finger21|Bip01_R_Finger31|Bip01_R_Finger41" },
            {"Settings_Hotkey" + "FingerX2" + "_CheckBox", "Bip01_L_Finger02|Bip01_L_Finger12|Bip01_L_Finger22|Bip01_L_Finger32|Bip01_L_Finger42|Bip01_R_Finger02|Bip01_R_Finger12|Bip01_R_Finger22|Bip01_R_Finger32|Bip01_R_Finger42" },
            {"Settings_Hotkey" + "ArmIK" + "_CheckBox", "LeftArmIKCheckBox|RightArmIKCheckBox" },
            {"Settings_Hotkey" + "LegIK" + "_CheckBox", "LeftLegIKCheckBox|RightRegIKCheckBox" },
            {"Settings_Hotkey" + "RClavicle" + "_CheckBox", "Bip01_R_Clavicle" },
            {"Settings_Hotkey" + "LClavicle" + "_CheckBox", "Bip01_L_Clavicle" },
            {"Settings_Hotkey" + "Hide" + "_CheckBox", "ALL" }
        };

        public enum SceneOptions
        {
            PrimitiveType = 0,
            ReceiveShadows,
            ShadowCastingMode,
            Shader,
            Maxium
        };
        #endregion

        #region Unity methods
        static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if ((c < '0' || c > '9'))
                {
                    if ('-' == c || '.' == c)
                        return true;
                    return false;
                }
            }

            return true;
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

        private static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            UnityEngine.Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        private static Texture2D duplicateTexture(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            UnityEngine.Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
        #endregion

        #region DEBUG_METHOD

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
#if DEBUG
            string[] list = GameUty.FileSystem.GetList(textBox1.Text, (AFileSystemBase.ListType)comboBox1.SelectedIndex);

            Debuginfo.Log(textBox1.Text, 2);
            foreach (string s in list)
            {
                Debuginfo.Log(s, 2);
            }
#endif
        }

        #endregion

        #region Public methods
        public EMES_Window()
        {
            InitializeComponent();
        }

        public void Finalized()
        {
#if DEBUG
            Debuginfo.Log("EMES_Window Finalize ...", 2);
#endif
            MaidOffset_BackupMaidPosRotScale = null;

            Shader_DOF_Enable_checkBox.Checked = false;
            Super.exShader.Reset_GlobalFogData();
            Shader_Fog_Enable_checkBox.Checked = false;
            Shader_Sepia_Enable_checkbox.Checked = false;
            Super.exShader.Reset_BloomData();
            Shader_Blur_Enable_checkbox.Checked = false;
            Super.exShader.Reset_VignettingData();
            MessageWindow_enable_checkBox.Checked = false;
            Light_Main_Reset_button_Click(this, EventArgs.Empty);

            string selectedName = SubLight_index_comboBox.SelectedItem.ToString();
            if ("無" == selectedName)
            {
                SubLight_index_comboBox.Items.Clear();
            }
            else
            {
                for (int i = SubLight_index_comboBox.Items.Count - 1; i >= 0; i--)
                {
                    //Super.IK_RemoveSubLightPoint(Super.camPlus.SubLightList[i].name);
                    Super.MaidIK.IK_RemoveSubLightPoint(CamPlus_SubLightHandleList[i]);
                    Super.camPlus.RemoveSubLight(Super.camPlus.SubLightList[i]);
                }
            }
#if DEBUG
            Debuginfo.Log("EMES_Window Finalized Done", 2);
#endif

            CamPlus_SubLightHandleList = null;
        }

        public bool Init(EMES EMES_instance, MaidTailsLite MT_instance)
        {
            Super = EMES_instance;
            MT = MT_instance;
#if DEBUG
            Debuginfo.Log("Loading hand pose", 2);
#endif
            LockBusy();
            int count = FingerPose.GetDataCount();
            RightHandPoseComboBox.Items.Clear();
            LeftHandPoseComboBox.Items.Clear();
            for (int i = 1; i <= count; i++)
            {
                RightHandPoseComboBox.Items.Add(i.ToString());
                LeftHandPoseComboBox.Items.Add(i.ToString());
            }
            RightHandPoseComboBox.Items.Add("無");
            LeftHandPoseComboBox.Items.Add("無");
            RightHandPoseComboBox.SelectedIndex = count;
            LeftHandPoseComboBox.SelectedIndex = count;
            UnlockBusy();
#if DEBUG
            Debuginfo.Log("Loading hand pose DONE", 2);
            Debuginfo.Log("Loading items", 2);
#endif
            LockBusy();
            Items_WindowInit();
            UnlockBusy();
#if DEBUG
            Debuginfo.Log("Loading items DONE", 2);
            Debuginfo.Log("Loading Dance_WindowInit", 2);
#endif
            Dance_WindowInit(false);

#if DEBUG
            Debuginfo.Log("Loading Dance_WindowInit DONE", 2);
            Debuginfo.Log("Loading Yotogi_WindowInit", 2);
#endif
            Yotogi_WindowInit();
#if DEBUG
            Debuginfo.Log("Loading Yotogi_WindowInit DONE", 2);
            Debuginfo.Log("Loading Settings_HotkeyMaid", 2);
#endif
            Settings_GUIHotkey_TextBox.Text = Super.settingsXml.ToggleKey;
            LockBusy();
            ActivedSettingsHotkey = new List<string>();
            Settings_DebugLevel_ComboBox.SelectedIndex = Super.settingsXml.DebugLogLevel;
            foreach (string Key in Enum.GetNames(typeof(Settings_HotkeyMaid)))
            {
                bool bKey = GetFieldValue<SettingsXML, bool>(Super.settingsXml, "bHotkey" + Key);
                string sKey = GetFieldValue<SettingsXML, string>(Super.settingsXml, "sHotkey" + Key);
                (Controls.Find("Settings_Hotkey" + Key + "_CheckBox", true)[0] as CheckBox).Checked = bKey;
                (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = sKey;

                if (true == bKey)
                {
                    if (false == Key.Contains("Maid"))
                        ActivedSettingsHotkey.Add(Key);
                    (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Enabled = false;
                }
            }
            Offset_PositionCheckbox.Enabled = !Settings_HotkeyMaidPos_CheckBox.Checked;
            Offset_RotationCheckbox.Enabled = !Settings_HotkeyMaidRot_CheckBox.Checked;
#if DEBUG
            Debuginfo.Log("Loading Settings_HotkeyMaid DONE", 2);
            Debuginfo.Log("Loading Settings_Hotkeys", 2);
#endif

            LoadingHotkeyCheckBox(typeof(Settings_HotkeyItem));
            LoadingHotkeyCheckBox(typeof(Settings_HotkeyDance));

            Settings_HotkeyCameraScreenShotNoUI_CheckBox.Checked = Super.settingsXml.bHotkeyCameraScreenShotNoUI;
            LoadingHotkeyCheckBox(typeof(Settings_HotkeyCamera));

            Settings_HotkeyCameraMovement_checkBox.Checked = Super.settingsXml.bHotkeyCameraMovement;
            LoadingHotkeyTextBox(typeof(Settings_HotkeyCameraMovement), true);

            if (true == Super.settingsXml.MaidTailsUseDFS)
            {
                MaidTails_BoneEnumMethod_Enum_radioButton.Checked = false;
                MaidTails_BoneEnumMethod_DFS_radioButton.Checked = true;
                Super.settingsXml.MaidTailsSpecialMarkMethodIgnore = true;
                Settings_MaidTails_SpecialMark_groupBox.Enabled = false;
            }
            else
            {
                MaidTails_BoneEnumMethod_Enum_radioButton.Checked = true;
                MaidTails_BoneEnumMethod_DFS_radioButton.Checked = false;
                Settings_MaidTails_SpecialMark_groupBox.Enabled = true;
            }

            if (true == Super.settingsXml.MaidTailsSpecialMarkMethodIgnore)
            {
                MaidTails_SpecialMarkMethod_ForceEnum_radioButton.Checked = false;
                MaidTails_SpecialMarkMethod_Ignore_radioButton.Checked = true;
            }
            else
            {
                MaidTails_SpecialMarkMethod_ForceEnum_radioButton.Checked = true;
                MaidTails_SpecialMarkMethod_Ignore_radioButton.Checked = false;
            }
#if DEBUG
            Debuginfo.Log("Loading Settings_Hotkeys DONE", 2);
            Debuginfo.Log("Loading maid pose", 2);
#endif

            MaidPose_Category_ComboBox.Items.Clear();
            foreach (KeyValuePair<string, List<EMES_Pose.PoseData>> category in Super.Pose.Pose_DataList)
            {
                MaidPose_Category_ComboBox.Items.Add(category.Key);
            }
            MaidPose_Category_ComboBox.SelectedIndex = 0;
            MaidPose_Category_ComboBox.Select();

            MaidPose_List_ComboBox.DataSource = new BindingSource(Super.Pose.Pose_DataList[Super.Pose.Pose_firstCatagory], null);
            MaidPose_List_ComboBox.DisplayMember = "ShowName";
            MaidPose_List_ComboBox.ValueMember = "Value";
            UnlockBusy();
#if DEBUG
            Debuginfo.Log("Loading maid pose DONE", 2);
            Debuginfo.Log("Loading backgrounds", 2);
#endif
            BackGroundComboBox.Items.Clear();
            foreach (EMES_Items.PhotoBGData bg in Super.Items.Items_BGDataList)
            {
                BackGroundComboBox.Items.Add(bg.category + ":" + bg.name);
            }
#if DEBUG
            Debuginfo.Log("Loading backgrounds DONE", 2);
            Debuginfo.Log("Loading CameraPlus_WindowInit", 2);
#endif
            CamPlus_SubLightHandleList = new List<HandleEx>();
            CameraPlus_WindowInit();
#if DEBUG
            Debuginfo.Log("Loading CameraPlus_WindowInit Done", 2);
            Debuginfo.Log("Loading SceneManagement_WindowInit", 2);
#endif
            SceneManagement_WindowInit();
#if DEBUG
            Debuginfo.Log("Loading SceneManagement_WindowInit Done", 2);
            Debuginfo.Log("Loading MaidImageList/CurrentMaidImageList", 2);
#endif

            MaidImageList.Images.Clear();
            MaidListView.Clear();
            MaidListView.SmallImageList = MaidImageList;
            MaidListView.LargeImageList = MaidImageList;

            CurrentMaidImageList.Images.Clear();
            CurrentMaidListView.Clear();
            CurrentMaidListView.SmallImageList = CurrentMaidImageList;
            CurrentMaidListView.LargeImageList = CurrentMaidImageList;

            CurrentMaidsList[0] = GameMain.Instance.CharacterMgr.GetMaid(0);
            LastMaidsGUID = new List<string>();
            CurrentMaidsStockID = new List<int>();

            int Index = 0;
            MaidListView.BeginUpdate();
            CurrentMaidListView.BeginUpdate();
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                string MaidName = maid.status.lastName + "\n" + maid.status.firstName;
                Image MaidThumbIcon = GetMaidThumbIcon(maid);
                MaidImageList.Images.Add(Index.ToString(), MaidThumbIcon);
                MaidListView.Items.Add(new ListViewItem
                {
                    ImageKey = Index.ToString(),
                    Name = Index.ToString(),
                    Text = MaidName
                });

                if (true == maid.Visible && true == maid.isActiveAndEnabled)
                {
#if DEBUG
                    Debuginfo.Log("Found " + maid.status.lastName + " " + maid.status.firstName + " at Index = " + Index.ToString(), 2);
#endif
                    MaidListView.Items[Index].Selected = true;
                    OrignalSelectedMaidStockID = Index;
                    CurrentMaidsStockID.Add(OrignalSelectedMaidStockID);
                    LastMaidsGUID.Add(maid.status.guid);

                    CurrentMaidImageList.Images.Add("0", MaidThumbIcon);
                    CurrentMaidListView.Items.Add(new ListViewItem
                    {
                        ImageKey = "0",
                        Name = "0|" + Index.ToString(),
                        Text = MaidName
                    });
                }
                Index++;
            }
            MaidListView.EndUpdate();
            CurrentMaidListView.EndUpdate();
#if DEBUG
            Debuginfo.Log("Loading MaidImageList/CurrentMaidImageList DONE", 2);
            Debuginfo.Log("Setup current maid", 2);
#endif
            SetupSelectedMaid(0);
#if DEBUG
            Debuginfo.Log("Setup current maid DONE", 2);
            Debuginfo.Log("IK_Init", 2);
#endif
            Super.MaidIK.IK_Init(CurrentSelectedMaid, GetCurrentMaidStockID());

#if DEBUG
            Debuginfo.Log("IK_Init DONE", 2);
            Debuginfo.Log("IK_WindowInit", 2);
#endif
            IK_WindowInit();
#if DEBUG
            Debuginfo.Log("IK_WindowInit DONE", 2);
            Debuginfo.Log("Items_Init", 2);
#endif
            Super.Items.Items_Init();
            Items_RealtimeLoadThumbIcon_checkBox.Checked = Super.settingsXml.bCreatePreviewIcon;
            MaidOffset_BackupMaidPosRotScale = new BackupMaidPosRotScale()
            {
                Pos = new Vector3(0, 0, 0),
                Rot = new Vector3(0, 0, 0),
                Scale = new Vector3(1, 1, 1)
            };
#if DEBUG
            Debuginfo.Log("Items_Init DONE", 2);
            Debuginfo.Log("INIT ALL DONE", 2);
#endif
            return true;
        }

        public void ToggleWindow(bool bShow)
        {
            if (true == ignoreShowWindow)
                return;

            if (true == bShow)
            {
                Show();
                bShowWindow = true;
                //VR mode
                System.Windows.Forms.Cursor.Show();
            }
            else
            {
                Hide();
                bShowWindow = false;
            }
        }

        public void UnlockBusy()
        {
            bIsBusy = false;
        }
        public void LockBusy()
        {
            bIsBusy = true;
        }

        public bool IsBusy()
        {
            return bIsBusy;
        }

        public bool GetWindowStatus()
        {
            return bShowWindow;
        }

        public Image GetMaidThumbIcon(Maid maid)
        {
            byte[] bStreamPng;
            try
            {
                bStreamPng = maid.GetThumIcon().EncodeToPNG();
            }
            catch(Exception e)
            {
#if DEBUG
                Debuginfo.Log("新規キャラ？　Error = " + e, 2);
#endif
                Texture2D texture = new Texture2D(128, 128, TextureFormat.RGB24, false);
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                        texture.SetPixel(x, y, UnityEngine.Color.white);

                bStreamPng = texture.EncodeToPNG();
            }

            MemoryStream mStream = new MemoryStream();
            mStream.Write(bStreamPng, 0, Convert.ToInt32(bStreamPng.Length));

            return Image.FromStream(mStream);
        }

        public Image GetMaidThumbCard(Maid maid)
        {
            byte[] bStreamPng = maid.GetThumCard().EncodeToPNG();
            MemoryStream mStream = new MemoryStream();
            mStream.Write(bStreamPng, 0, Convert.ToInt32(bStreamPng.Length));

            return Image.FromStream(mStream);
        }

        public bool GetMaidPoseStopAnimeCheckBox()
        {
            return MaidPoseLoopAnimeCheckBox.Checked;
        }

        public bool GetMuneYureCheckBox()
        {
            return MuneYureCheckBox.Checked;
        }

        public int GetCurrentMaidStockID()
        {
            return CurrentMaidsStockID[CurrentSelectedMaidIndex];
        }

        public void ProcessShortcurtHotkey()
        {
            if (FlexKeycode.GetMultipleKeyDown(Super.settingsXml.ToggleKey))
            {
                ToggleWindow(!GetWindowStatus());
            }

            if (true == Super.settingsXml.bHotkeyMaidPos)
            {
                if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyMaidPos) && false == Offset_PositionCheckbox.Checked)
                {
                    Offset_PositionCheckbox.Checked = true;
                    Offset_PositionCheckbox.Enabled = false;
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyMaidPos) && true == Offset_PositionCheckbox.Checked)
                {
                    Offset_PositionCheckbox.Checked = false;
                    Offset_PositionCheckbox.Enabled = false;
                }
            }

            if (true == Super.settingsXml.bHotkeyMaidRot)
            {
                if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyMaidRot) && false == Offset_RotationCheckbox.Checked)
                {
                    Offset_RotationCheckbox.Checked = true;
                    Offset_RotationCheckbox.Enabled = false;
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyMaidRot) && true == Offset_RotationCheckbox.Checked)
                {
                    Offset_RotationCheckbox.Checked = false;
                    Offset_RotationCheckbox.Enabled = false;
                }
            }

            if (0 == ActivedSettingsHotkey.Count)
                return;

            if (true == IsBusy())
            {
                return;
            }

            /*
            HandleSelectMode handleSelectMode = GetMaidHandleSelectMode();
            if (HandleSelectMode.None == handleSelectMode || HandleSelectMode.Others == handleSelectMode)
            {
                return;
            }
            */

            foreach (string Key in ActivedSettingsHotkey)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(GetFieldValue<SettingsXML, string>(Super.settingsXml, "sHotkey" + Key)))
                {
#if DEBUG
                    Debuginfo.Log(Key + " 押された", 2);
#endif
                    string HandleCheckBox = HotkeyToHandle["Settings_Hotkey" + Key + "_CheckBox"];
                    if (true == HandleCheckBox.Contains("|"))
                    {
                        string[] HandleCheckBoxList = HandleCheckBox.Split('|');
                        foreach (string handleCB in HandleCheckBoxList)
                        {
                            (Controls.Find(handleCB, true)[0] as CheckBox).Checked = true;
                        }
                    }
                    else if ("ALL" == HandleCheckBox)
                    {
                        /*
                        EMES.EMES_IK ik = Super.MaidsIK[GetCurrentMaidStockID()];
                        if (true == Super.IK_DetachAll(ik.maidStockID))
                        {
                            Dictionary<EMES.BoneType, HandleEx> handles = ik.handleEx;
                            foreach (KeyValuePair<EMES.BoneType, HandleEx> handle in handles)
                            {
                                handle.Value.Visible = false;
                            }
                            ik.bInvisible = true;
                        }
                        SyncAllHandleExCheckBox();
                        MaidTails_DisableAutoIK();
                        */

                        HideAllMaidsHandle_Click(this, EventArgs.Empty);
                    }
                    else
                    {
                        (Controls.Find(HandleCheckBox, true)[0] as CheckBox).Checked = !(Controls.Find(HandleCheckBox, true)[0] as CheckBox).Checked;
                    }
                }
            }

            //Dance
            if (true == Settings_HotkeyDanceStart_CheckBox.Checked)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyDanceStart))
                {
#if DEBUG
                    Debuginfo.Log(Super.settingsXml.sHotkeyDanceStart + " 押された", 2);
#endif
                    Dance_StartAll();
                }
            }

            if (true == Settings_HotkeyDanceAllOtherStart_CheckBox.Checked)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyDanceAllOtherStart))
                {
#if DEBUG
                    Debuginfo.Log(Super.settingsXml.sHotkeyDanceAllOtherStart + " 押された", 2);
#endif
                    Dance_StartAllOther();
                }
            }

            //Camera
            if (true == Settings_HotkeyCameraQuickLoad_CheckBox.Checked)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyCameraQuickLoad))
                {
#if DEBUG
                    Debuginfo.Log(Super.settingsXml.sHotkeyCameraQuickLoad + " 押された", 2);
#endif
                    Super.camPlus.CameraQuickLoad();
                }
            }

            if (true == Settings_HotkeyCameraQuickSave_CheckBox.Checked)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyCameraQuickSave))
                {
#if DEBUG
                    Debuginfo.Log(Super.settingsXml.sHotkeyCameraQuickSave + " 押された", 2);
#endif
                    Super.camPlus.CameraQuickSave();
                }
            }

            if (true == Settings_HotkeyCameraScreenShot_CheckBox.Checked)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyCameraScreenShot))
                {
#if DEBUG
                    Debuginfo.Log(Super.settingsXml.sHotkeyCameraScreenShot + " 押された、スクリーンショット noUI=" + Settings_HotkeyCameraScreenShotNoUI_CheckBox.Checked, 2);
#endif
                    GameMain.Instance.MainCamera.ScreenShot(Settings_HotkeyCameraScreenShotNoUI_CheckBox.Checked);
                }
            }
        }

        public void Items_UpdateCurrentHandleCount()
        {
            PrefabItem_Count_Label.Text = "ハンドル数： " + Super.Items.GetItemHandleCount().ToString();

            LockBusy();
            if (Super.Items.Items_ItemHandle.Count > 0)
            {
                Items_HandledObjects_listBox.DataSource = new BindingSource(Super.Items.Items_ItemHandle, null);
                Items_HandledObjects_listBox.ValueMember = "Key";
                Items_HandledObjects_listBox.DisplayMember = "Value";
            }
            else
            {
                Items_HandledObjects_listBox.ClearSelected();
                Items_HandledObjects_listBox.DataSource = null;
                Items_HandledObjects_listBox.SelectedIndex = -1;
                Items_ClearSubItems();
            }
            UnlockBusy();

            Items_UpdateCurrentHandleFunc();
        }

        public void Items_ClearSubItems()
        {
            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                Items_List_tabControl.SelectedTab = Items_list_tabPage1;
                Items_List_tabControl.Select();
            }
            
            Items_SubObjects_TempDisableMaidHandle_checkBox.Checked = false;

            LockBusy();
            Super.Items.Items_Sub_RemoveAll();
            Items_SubObjects_listBox.DataSource = null;
            Items_SubObjects_listBox.SelectedIndex = -1;
            Items_SubObjects_listBox.ClearSelected();
            UnlockBusy();
        }

        public void SwitchToCurrentSelectedMaids()
        {
            LastMaidsGUID.Clear();
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                LastMaidsGUID.Add(CurrentMaidsList[i].status.guid);
            }
            CurrentSelectedMaidsRadioButton.Checked = true;
            CurrentSelectedMaidsRadioButton_Click(CurrentSelectedMaidsRadioButton, EventArgs.Empty);
        }

        public bool GetYotogiNonStop()
        {
            return Yotogi_NonStop_checkBox.Checked;
        }

        public bool GetYotogiCrcTranslate()
        {
            return Yotogi_CrcTranslate_checkBox.Checked;
        }

        public bool GetMaidLoadingKeepPose()
        {
            return MaidLoading_KeepPose_checkBox.Checked;
        }

        public bool GetMaidLoadingKeepPosRot()
        {
            return MaidLoading_KeepPosRot_checkBox.Checked;
        }

        public void Refresh_SubLight_list()
        {
            LockBusy();
            SubLight_index_comboBox.Items.Clear();
            if (0 == Super.camPlus.SubLightList.Count)
            {
                SubLight_index_comboBox.Items.Add("無");
            }
            else
            {
                for (int Index = 0; Index < Super.camPlus.SubLightList.Count; Index++)
                {
                    SubLight_index_comboBox.Items.Add((Index + 1).ToString());
                }
            }
            SubLight_index_comboBox.SelectedIndex = 0;
            UnlockBusy();
            SubLight_index_comboBox.Select();
        }

        public void TryAddCustomFaceBlend(int StockID)
        {
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(StockID);
            AddCustomFaceBlend(maid);
        }

        public void MaidPoseCopy_Init()
        {
            LockBusy();
            int Index = 0;
            MaidPose_Source_comboBox.Items.Clear();
            if (CurrentMaidsStockID.Count > 1)
            {
                for (Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                {
                    if (CurrentMaidsList[Index] == CurrentSelectedMaid)
                    {
                        continue;
                    }
                    MaidPose_Source_comboBox.Items.Add(Index.ToString() + ":" + CurrentMaidsList[Index].status.lastName + " " + CurrentMaidsList[Index].status.firstName);
                }
                MaidPose_Copy_Button.Enabled = true;
            }
            else
            {
                MaidPose_Source_comboBox.Items.Add("無");
                MaidPose_Copy_Button.Enabled = false;
            }
            MaidPose_Source_comboBox.SelectedIndex = 0;
            MaidPose_Source_comboBox.Select();
            UnlockBusy();
        }

        public void MaidTails_DisableAutoIK()
        {
            LockBusy();
            MaidTails_TryCreateIK_checkBox.Checked = false;
            UnlockBusy();

            Super.MaidIK.IK_AutoIK_Deatch();
        }

        public void MaidTails_Init()
        {
            LockBusy();
            Super.maidTails.DestoryTailHandle();

            MaidTails_listBox.Items.Clear();
            Dictionary<string, Transform> boneData = MT.GetBoneTails();
            if (boneData.Count > 0)
            {
                MaidTails_listBox.DataSource = new BindingSource(boneData, null);
                MaidTails_listBox.ValueMember = "Value";
                MaidTails_listBox.DisplayMember = "Key";
            }
            else
            {
                MaidTails_Cat_groupBox.Visible = false;
                MaidTails_groupBox.Visible = false;
            }

            MaidTails_ShowPos_checkBox.Checked = false;
            MaidTails_ShowRot_checkBox.Checked = false;
            MaidTails_listBox.SelectedIndex = -1;
            MaidTails_listBox.Select();
            UnlockBusy();
        }

        public void SetMaidTailsBoneScale(float scale)
        {
            MaidTails_Scale_textBox.Text = scale.ToString("F4");
        }

        public void ReloadCurrentMaidInfo()
        {
            SetSceneEditMaid(Super.Window.CurrentSelectedMaidIndex, Super.Window.MaidSwitch_KeepCameraPosRot_checkBox.Checked);
            SetupSelectedMaid(CurrentSelectedMaidIndex);
        }

        public bool GetCameraMovement()
        {
            return Settings_HotkeyCameraMovement_checkBox.Checked;
        }

        public bool Items_isItemHandleMethodAll()
        {
            return Items_All_radioButton.Checked;
        }

        public bool Items_isItemHandleMethodAllorSingle()
        {
            return Items_All_radioButton.Checked || Items_Single_radioButton.Checked;
        }

        public void Items_UpdateItemHandleScaleInfo(GameObject gameObject)
        {
            Items_Handle_Scale_textBox.Text = ((gameObject.transform.localScale.x + gameObject.transform.localScale.y + gameObject.transform.localScale.z) / 3).ToString("F4");

            Items_Handle_ScaleX_textBox.Text = (gameObject.transform.localScale.x).ToString("F4");
            Items_Handle_ScaleY_textBox.Text = (gameObject.transform.localScale.y).ToString("F4");
            Items_Handle_ScaleZ_textBox.Text = (gameObject.transform.localScale.z).ToString("F4");

            Vector3 rot = gameObject.transform.rotation.eulerAngles;
            Items_Handle_RotX_textBox.Text = (rot.x).ToString("F4");
            Items_Handle_RotY_textBox.Text = (rot.y).ToString("F4");
            Items_Handle_RotZ_textBox.Text = (rot.z).ToString("F4");

            Vector3 pos = gameObject.transform.position;
            Items_Handle_PosX_textBox.Text = (pos.x).ToString("F4");
            Items_Handle_PosY_textBox.Text = (pos.y).ToString("F4");
            Items_Handle_PosZ_textBox.Text = (pos.z).ToString("F4");
        }

        public void CamPlus_UpdateFOV()
        {
            CameraPos_FOV_textbox.Text = GameMain.Instance.MainCamera.camera.fieldOfView.ToString("F4");
        }

        public HandleSelectMode GetMaidHandleSelectMode()
        {
            if (true == MaidHandleSelectModle_All_radioButton.Checked)
                return HandleSelectMode.All;
            else if (true == MaidHandleSelectModle_Current_radioButton.Checked)
                return HandleSelectMode.Current;
            else if (true == MaidHandleSelectModle_Others_radioButton.Checked)
                return HandleSelectMode.Others;

            return HandleSelectMode.None;
        }

        public bool CheckMaidSelectMode(HandleSelectMode hsMode, int iIndex, int iID)
        {
            if (HandleSelectMode.All != hsMode)
            {
                if (HandleSelectMode.None == hsMode)
                {
                    return false;
                }
                else if (HandleSelectMode.Current == hsMode)
                {
                    if (CurrentMaidsStockID[iIndex] != iID)
                        return false;
                }
                else if (HandleSelectMode.Others == hsMode)
                {
                    if (CurrentMaidsStockID[iIndex] == iID)
                        return false;
                }
            }

            return true;
        }

        public string GetCurrentMainTab()
        {
            return MainTab_tabControl.SelectedTab.Name;
        }

        public string GetCurrentMaidPoseAndPartsTab()
        {
            return MaidPoseAndParts_tabControl.SelectedTab.Name;
        }

        public int GetItemsHandleScaleFactor()
        {
            return Items_Handle_ScaleFactor_comboBox.SelectedIndex;
        }

        public void MaidOffset_UpdateOffsetInfo(Maid maid)
        {
            Vector3 Pos = maid.GetPos();
            Vector3 Rot = maid.GetRot();
            float Scale = maid.body0.transform.localScale.x;

            MaidOffset_PosX_textBox.Text = Pos.x.ToString("F4");
            MaidOffset_PosY_textBox.Text = Pos.y.ToString("F4");
            MaidOffset_PosZ_textBox.Text = Pos.z.ToString("F4");

            MaidOffset_RotX_textBox.Text = Rot.x.ToString("F4");
            MaidOffset_RotY_textBox.Text = Rot.y.ToString("F4");
            MaidOffset_RotZ_textBox.Text = Rot.z.ToString("F4");

            MaidOffset_Scale_textBox.Text = Scale.ToString("F4");
        }

        public bool GetFingerToeLock(string sBone)
        {
            bool ret = false;

            if (false == Blend_LockNone_radioButton.Checked)
            {
                CheckBox cb = Controls.Find(sBone.Replace(" ", "_"), true)[0] as CheckBox;
                if (null != cb)
                {
                    ret = cb.Checked;
                }

                if (false == Blend_LockSelected_radioButton.Checked)
                {
                    ret = !ret;
                }
            }
            return ret;
        }

        public bool GetBonesSelected(string sBone)
        {
            bool ret = false;

            CheckBox cb = Controls.Find(sBone.Replace(" ", "_"), true)[0] as CheckBox;
            if (null != cb)
            {
                ret = cb.Checked;
            }

            return ret;
        }

        public void UpdateCameraFOVinfo()
        {
            SetTrackBarValue(CameraPos_FOV_trackBar, (int)(GameMain.Instance.MainCamera.camera.fieldOfView * 10.0f));
            CameraPos_FOV_textbox.Text = GameMain.Instance.MainCamera.camera.fieldOfView.ToString("F1");
        }

        public bool isItems_list_tabPage1()
        {
            return (Items_list_tabPage1 == Items_List_tabControl.SelectedTab);
        }

        public bool issubItems_list_tabPage2()
        {
            return (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab);
        }

        public bool isMirrorBip01Checked()
        {
            return MaidMirror_Bip01_checkBox.Checked;
        }

        public void ReloadedMaid(int MaidIndex)
        {
            SetSceneEditMaid(MaidIndex, true);
            SetupSelectedMaid(MaidIndex);
        }

        public void CheckIfNeedAddCustomFaceBlend(Maid maid)
        {
            if (null == maid)
            {
                return;
            }

            try
            {
                if (false == maid.body0.Face.morph.dicBlendSet.ContainsKey("カスタム"))
                {
                    AddCustomFaceBlend(maid);
                }
            }
            catch(Exception e)
            {
                Debuginfo.Warning("ボディスワップによるエラーの可能性があります、心配しないで\n" + e, 1);
            }
            
        }
        #endregion

        #region Private methods
        private void LoadingHotkeyCheckBox(Type type)
        {
#if DEBUG
            Debuginfo.Log("Loading " + type.ToString(), 2);
#endif
            foreach (string Key in Enum.GetNames(type))
            {
                if ("BreakPoint" == Key)
                    continue;

                bool bKey = GetFieldValue<SettingsXML, bool>(Super.settingsXml, "bHotkey" + Key);
                string sKey = GetFieldValue<SettingsXML, string>(Super.settingsXml, "sHotkey" + Key);
                (Controls.Find("Settings_Hotkey" + Key + "_CheckBox", true)[0] as CheckBox).Checked = bKey;
                (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = sKey;

                if (true == bKey)
                {
                    (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Enabled = false;
                }
            }
#if DEBUG
            Debuginfo.Log("Loading " + type.ToString() + " Done", 2);
#endif
        }

        private void LoadingHotkeyTextBox(Type type, bool bDisableEdit)
        {
#if DEBUG
            Debuginfo.Log("Loading " + type.ToString(), 2);
#endif
            foreach (string Key in Enum.GetNames(type))
            {
                if ("BreakPoint" == Key)
                    continue;

                string sKey = GetFieldValue<SettingsXML, string>(Super.settingsXml, "sHotkey" + Key);
                (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = sKey;

                if (true == bDisableEdit)
                {
                    (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Enabled = false;
                }
            }
#if DEBUG
            Debuginfo.Log("Loading " + type.ToString() + " Done", 2);
#endif
        }

        private void Dance_WindowInit(bool bRefreshOnly)
        {
            if (null == Dance_AllOther)
            {
                Dance_AllOther = new Dictionary<Maid, string>();
            }
            else
            {
                Dance_AllOther.Clear();
            }

            LockBusy();
            if (false == bRefreshOnly)
            {
                Dance_List_ComboBox.Items.Clear();
                foreach (KeyValuePair<string, EMES_Dance.ActionDataList> dance in EMES_Dance.Dance.Data)
                {
                    if (true == dance.Value.bVisible)
                    {
                        Dance_List_ComboBox.Items.Add(dance.Value.iCharaNum.ToString() + "人：" + dance.Key);
                    }
                }
                Dance_List_ComboBox.SelectedIndex = 0;
                Dance_List_ComboBox.Select();

                Dance_BGM_ComboBox.Items.Clear();
                foreach (string Key in EMES_Dance.Dance.BGM)
                {
                    Dance_BGM_ComboBox.Items.Add(Key);
                }
                Dance_BGM_ComboBox.SelectedIndex = 0;
                Dance_BGM_ComboBox.Select();
            }

            Dance_SubPose_ComboBox.Items.Clear();
            string[] DancerPosition = Enum.GetNames(typeof(EMES_Dance.Dance.DancerPosition));
            for (int i = 0; i < (int)EMES_Dance.Dance.DancerPosition.EOL; i++)
            {
                Dance_SubPose_ComboBox.Items.Add(DancerPosition[i]);
                EMES_Dance.Dance.Dancer[i] = null;
            }
            Dance_SubPose_ComboBox.SelectedIndex = 0;
            Dance_SubPose_ComboBox.Select();

            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            Dance_SubBGM_ComboBox.Items.Clear();
            Dance_SubBGM_ComboBox.Items.Add(EMES_Dance.Dance.Data[DanceName].sBGMName);
            if (string.Empty != EMES_Dance.Dance.Data[DanceName].sOtherInfo)
            {
                string[] SubBGM = EMES_Dance.Dance.Data[DanceName].sOtherInfo.Split(',');
                foreach (string bgm in SubBGM)
                {
                    Dance_SubBGM_ComboBox.Items.Add(bgm);
                }
            }
            Dance_SubBGM_ComboBox.SelectedIndex = 0;
            Dance_SubBGM_ComboBox.Select();

            UnlockBusy();
        }

        private void Items_WindowInit()
        {
            Items_HandledObjects_listBox.Items.Clear();
            Items_Handle_ScaleFactor_comboBox.SelectedIndex = 0;
            Items_LoadImage_Shadow_comboBox.SelectedIndex = 2;
            Items_LoadImage_Shader_comboBox.SelectedIndex = 2;
            Items_LoadImage_Type_comboBox.SelectedIndex = 5;

            Items_SubObjects_listBox.Items.Clear();

            //prefabs
            Prefab_Nyou_comboBox.Items.Clear();
            foreach (string Key in EMES_Yotogi.Perfab.Nyou)
            {
                Prefab_Nyou_comboBox.Items.Add(Key);
            }
            Prefab_Nyou_comboBox.SelectedIndex = 0;
            Prefab_Nyou_comboBox.Select();

            Prefab_Sio_comboBox.Items.Clear();
            foreach (string Key in EMES_Yotogi.Perfab.Sio)
            {
                Prefab_Sio_comboBox.Items.Add(Key);
            }
            Prefab_Sio_comboBox.SelectedIndex = 0;
            Prefab_Sio_comboBox.Select();

            Prefab_Seieki_comboBox.Items.Clear();
            foreach (string Key in EMES_Yotogi.Perfab.Seieki)
            {
                Prefab_Seieki_comboBox.Items.Add(Key);
            }
            Prefab_Seieki_comboBox.SelectedIndex = 0;
            Prefab_Seieki_comboBox.Select();

            Prefab_Enema_comboBox.Items.Clear();
            foreach (string Key in EMES_Yotogi.Perfab.Enema)
            {
                Prefab_Enema_comboBox.Items.Add(Key);
            }
            Prefab_Enema_comboBox.SelectedIndex = 0;
            Prefab_Enema_comboBox.Select();

            Prefab_Steam_comboBox.Items.Clear();
            foreach (string Key in EMES_Yotogi.Perfab.Steam)
            {
                Prefab_Steam_comboBox.Items.Add(Key);
            }
            Prefab_Steam_comboBox.SelectedIndex = 0;
            Prefab_Steam_comboBox.Select();

            Prefab_Category_comboBox.Items.Clear();
            foreach (string cat in Enum.GetNames(typeof(EMES_Items.PerfabItems.Category)))
            {
                Prefab_Category_comboBox.Items.Add(cat);
            }
            Prefab_Category_comboBox.SelectedIndex = (int)EMES_Items.PerfabItems.Category.家具;
            Prefab_Category_comboBox.Select();

            Prefab_Items_comboBox.DataSource = new BindingSource(Super.Items.Items_perfabItems.Items["家具"], null);
            Prefab_Items_comboBox.DisplayMember = "Key";
            Prefab_Items_comboBox.ValueMember = "Value";

            //MyCustomRoomObjects
            MyRoomCustomObjects_Category_comboBox.Items.Clear();
            foreach (string key in Super.Items.myCustomRoomObject.Data.Keys.ToList())
            {
                MyRoomCustomObjects_Category_comboBox.Items.Add(key);
            }
            MyRoomCustomObjects_Category_comboBox.SelectedIndex = 0;
            MyRoomCustomObjects_Category_comboBox.Select();

            MyRoomCustomObjects_Items_comboBox.DataSource = new BindingSource(Super.Items.myCustomRoomObject.Data[MyRoomCustomObjects_Category_comboBox.SelectedItem.ToString()], null);
            MyRoomCustomObjects_Items_comboBox.DisplayMember = "Value";
            MyRoomCustomObjects_Items_comboBox.ValueMember = "Key";

            //desk
            DeskObjects_Category_comboBox.Items.Clear();
            foreach (string key in Super.Items.Items_DeskItemData.Keys.ToList())
            {
                DeskObjects_Category_comboBox.Items.Add(key);
            }
            DeskObjects_Category_comboBox.SelectedIndex = 0;
            DeskObjects_Category_comboBox.Select();

            DeskObjects_Items_comboBox.DataSource = new BindingSource(Super.Items.Items_DeskItemData[DeskObjects_Category_comboBox.SelectedItem.ToString()], null);
            DeskObjects_Items_comboBox.DisplayMember = "Value";
            DeskObjects_Items_comboBox.ValueMember = "Key";
        }

        private void SceneManagement_WindowInit()
        {
            LockBusy();
            Scene_Select_ComboBox.Items.Clear();
            if (Super.sceneManagement.MaidSceneData.Count > 0)
            {
                foreach (EMES_SceneManagement.SceneDataNew sd in Super.sceneManagement.MaidSceneData)
                {
                    if (true == sd.ScreenShotName.Contains("EMES_screen_"))
                        Scene_Select_ComboBox.Items.Add(sd.Maids.Count.ToString() + "人：" + sd.DateTime);
                    else
                        Scene_Select_ComboBox.Items.Add(sd.Maids.Count.ToString() + "人：" + sd.ScreenShotName);
                }

                using (MemoryStream mStream = new MemoryStream())
                {
                    mStream.Write(Super.sceneManagement.MaidSceneData[0].ScreenShot, 0, Convert.ToInt32(Super.sceneManagement.MaidSceneData[0].ScreenShot.Length));
                    Scene_PictureBox.Image = Image.FromStream(mStream);
                }
            }
            else
            {
                Scene_Select_ComboBox.Items.Add("無");
            }

            Scene_Select_ComboBox.SelectedIndex = 0;
            Scene_Select_ComboBox.Select();
            UnlockBusy();
        }

        private void SceneManagement_UpdateWindow(EMES_SceneManagement.SceneDataNew sd, string sShowName)
        {
            string selectedName = Scene_Select_ComboBox.SelectedItem.ToString();
            if ("無" == selectedName)
            {
                Scene_Select_ComboBox.Items.Clear();
            }
            LockBusy();
            if (null == sShowName)
            {
                if (true == sd.ScreenShotName.Contains("EMES_screen_"))
                    Scene_Select_ComboBox.Items.Add(sd.Maids.Count.ToString() + "人：" + sd.DateTime);
                else
                    Scene_Select_ComboBox.Items.Add(sd.Maids.Count.ToString() + "人：" + sd.ScreenShotName);
            }
            else
            {
                Scene_Select_ComboBox.Items.Add(sd.Maids.Count.ToString() + "人：" + sShowName);
            }

            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(sd.ScreenShot, 0, Convert.ToInt32(sd.ScreenShot.Length));
                Scene_PictureBox.Image = Image.FromStream(mStream);
            }

            Scene_Select_ComboBox.SelectedIndex = Scene_Select_ComboBox.Items.Count - 1;
            Scene_Select_ComboBox.Select();
            UnlockBusy();
        }

        private void SetupSelectedMaid(int maidIndex)
        {
            CurrentSelectedMaid = CurrentMaidsList[maidIndex];
            CurrentSelectedMaidIndex = maidIndex;
            CurrentSelectedMaidPicture.Image = GetMaidThumbIcon(CurrentSelectedMaid);
            CurrentSelectedMaidNameLabel.Text = CurrentSelectedMaid.status.firstName + "\n" + CurrentSelectedMaid.status.lastName;

            List<string> FaceBlendList = new List<string>();
            foreach (KeyValuePair<string, float[]> Face in CurrentSelectedMaid.body0.Face.morph.dicBlendSet)
            {
                FaceBlendList.Add(Face.Key);
            }
            LockBusy();
            CurrentMaidFaceBlendComboBox.Items.Clear();
            int Index = 0;
            foreach (string Name in FaceBlendList)
            {
                CurrentMaidFaceBlendComboBox.Items.Add(Name);
                if ("通常" == Name)
                    CurrentMaidFaceBlendComboBox.SelectedIndex = Index;
                Index++;
            }
            MaidPose_FaceLock_checkBox.Checked = CurrentSelectedMaid.GetLockHeadAndEye();
            UnlockBusy();
            CurrentMaidFaceBlendComboBox.Select();
            AddCustomFaceBlend(CurrentSelectedMaid);
            openMouth_checkBox.Checked = CurrentSelectedMaid.boMouthOpen;
            openMouthLookTooth_checkBox.Checked = CurrentSelectedMaid.boLookTooth;
#if DEBUG
            Debuginfo.Log("SetupSelectedMaid >>>> SyncAllHandleExCheckBox ", 2);
#endif
            SyncAllHandleExCheckBox();
#if DEBUG
            Debuginfo.Log("SetupSelectedMaid >>>> SyncAllHandleExCheckBox DONE", 2);
            Debuginfo.Log("SyncMaidGravity", 2);
#endif
            SyncMaidGravity();
#if DEBUG
            Debuginfo.Log("SyncMaidGravity DONE", 2);
            Debuginfo.Log("MaidPose_Source_comboBox", 2);
#endif
            MaidPoseCopy_Init();
#if DEBUG
            Debuginfo.Log("MaidPose_Source_comboBox DONE", 2);
            Debuginfo.Log("MaidTails Init", 2);
#endif
            MaidTails_Init();
            MaidPartsEdit_Enable_checkBox.Checked = false;
#if DEBUG
            Debuginfo.Log("MaidTails Init DONE", 2);
            Debuginfo.Log("MaidOffset_UpdateOffsetInfo", 2);
#endif
            MaidOffset_UpdateOffsetInfo(CurrentSelectedMaid);
#if DEBUG
            Debuginfo.Log("MaidOffset_UpdateOffsetInfo DONE", 2);
#endif
        }

        private Dictionary<string, float> CreateFaceBlend(Maid maid, string BlendSetName)
        {
            Dictionary<string, float> faceBlend = new Dictionary<string, float>();
            foreach (DictionaryEntry entry in maid.body0.Face.morph.hash)
            {
                Debuginfo.Log("key =" + entry.Key + "   keyV = " + entry.Value + "   value=" + maid.body0.Face.morph.dicBlendSet[BlendSetName][int.Parse(entry.Value.ToString())], 2);
                faceBlend.Add(entry.Key.ToString(), maid.body0.Face.morph.dicBlendSet[BlendSetName][int.Parse(entry.Value.ToString())]);
            }

            return faceBlend;
        }

        private void SetTrackBarValue(TrackBar tb, int Value)
        {
            if (Value > tb.Maximum)
            {
#if DEBUG
                Debuginfo.Warning(tb.Name + " Value = " + Value.ToString() + " > Maximum " + tb.Maximum.ToString(), 2);
#endif
                Value = tb.Maximum;
            }
            else if (Value < tb.Minimum)
            {
#if DEBUG
                Debuginfo.Warning(tb.Name + " Value = " + Value.ToString() + " < Minimum " + tb.Minimum.ToString(), 2);
#endif
                Value = tb.Minimum;
            }

            tb.Value = Value;
        }

        private void SetTrackBarValues(Dictionary<string, float> faceBlend)
        {
            SetTrackBarValue(eyebig_trackBar, (int)(faceBlend["eyebig"] * 1000));
            SetTrackBarValue(eyeclose_trackBar, (int)(faceBlend["eyeclose"] * 1000));
            SetTrackBarValue(eyeclose2_trackBar, (int)(faceBlend["eyeclose2"] * 1000));
            SetTrackBarValue(eyeclose3_trackBar, (int)(faceBlend["eyeclose3"] * 1000));
            SetTrackBarValue(eyeclose5_trackBar, (int)(faceBlend["eyeclose5"] * 1000));
            SetTrackBarValue(eyeclose6_trackBar, (int)(faceBlend["eyeclose6"] * 1000));

            SetTrackBarValue(hitomih_trackBar, (int)(faceBlend["hitomih"] * 1000));
            SetTrackBarValue(hitomis_trackBar, (int)(faceBlend["hitomis"] * 1000));
            hoho_checkBox.Checked = (int)(faceBlend["hoho"]) != 0;
            hohos_checkBox.Checked = (int)(faceBlend["hohos"]) != 0;
            hohol_checkBox.Checked = (int)(faceBlend["hohol"]) != 0;
            hoho2_checkBox.Checked = (int)(faceBlend["hoho2"]) != 0;

            SetTrackBarValue(mayuha_trackBar, (int)(faceBlend["mayuha"] * 1000));
            SetTrackBarValue(mayuup_trackBar, (int)(faceBlend["mayuup"] * 1000));
            SetTrackBarValue(mayuv_trackBar, (int)(faceBlend["mayuv"] * 1000));
            SetTrackBarValue(mayuvhalf_trackBar, (int)(faceBlend["mayuvhalf"] * 1000));
            SetTrackBarValue(mayuw_trackBar, (int)(faceBlend["mayuw"] * 1000));

            SetTrackBarValue(moutha_trackBar, (int)(faceBlend["moutha"] * 1000));
            SetTrackBarValue(mouthc_trackBar, (int)(faceBlend["mouthc"] * 1000));
            SetTrackBarValue(mouthdw_trackBar, (int)(faceBlend["mouthdw"] * 1000));
            SetTrackBarValue(mouthhe_trackBar, (int)(faceBlend["mouthhe"] * 1000));
            SetTrackBarValue(mouthi_trackBar, (int)(faceBlend["mouthi"] * 1000));
            SetTrackBarValue(mouths_trackBar, (int)(faceBlend["mouths"] * 1000));
            SetTrackBarValue(mouthup_trackBar, (int)(faceBlend["mouthup"] * 1000));
            SetTrackBarValue(mouthuphalf_trackBar, (int)(faceBlend["mouthuphalf"] * 1000));

            SetTrackBarValue(tangopen_trackBar, (int)(faceBlend["tangopen"] * 1000));
            SetTrackBarValue(tangout_trackBar, (int)(faceBlend["tangout"] * 1000));
            SetTrackBarValue(tangup_trackBar, (int)(faceBlend["tangup"] * 1000));

            SetTrackBarValue(namida_trackBar, (int)(faceBlend["namida"] * 1000));
            if (0 != faceBlend["nosefook"])
                nosefook_checkBox.Checked = true;
            else
                nosefook_checkBox.Checked = false;

            shock_checkBox.Checked = (int)(faceBlend["shock"]) != 0;
            SetTrackBarValue(tear1_trackBar, (int)(faceBlend["tear1"] * 1000));
            SetTrackBarValue(tear2_trackBar, (int)(faceBlend["tear2"] * 1000));
            SetTrackBarValue(tear3_trackBar, (int)(faceBlend["tear3"] * 1000));
            SetTrackBarValue(toothoff_trackBar, (int)(faceBlend["toothoff"] * 1000));
            SetTrackBarValue(yodare_trackBar, (int)(faceBlend["yodare"] * 1000));

            //GP01
            if (CurrentSelectedMaid.body0.Face.PartsVersion >= GP01FaceVersion)
            {
                SetTrackBarValue(eyeclose7_trackBar, (int)(faceBlend["eyeclose7"] * 1000));
                SetTrackBarValue(eyeclose8_trackBar, (int)(faceBlend["eyeclose8"] * 1000));
                SetTrackBarValue(mouthfera_trackBar, (int)(faceBlend["mouthfera"] * 1000));
                SetTrackBarValue(mouthferar_trackBar, (int)(faceBlend["mouthferar"] * 1000));
            }
        }

        private Dictionary<string, float> GetFaceBlendValue()
        {
            Dictionary<string, float> faceBlend = new Dictionary<string, float>();
            faceBlend.Add("eyebig", eyebig_trackBar.Value);
            faceBlend.Add("eyeclose", eyeclose_trackBar.Value);
            faceBlend.Add("eyeclose2", eyeclose2_trackBar.Value);
            faceBlend.Add("eyeclose3", eyeclose3_trackBar.Value);
            faceBlend.Add("eyeclose5", eyeclose5_trackBar.Value);
            faceBlend.Add("eyeclose6", eyeclose6_trackBar.Value);

            faceBlend.Add("hitomih", hitomih_trackBar.Value);
            faceBlend.Add("hitomis", hitomis_trackBar.Value);
            faceBlend.Add("hoho", hoho_checkBox.Checked ? 1000 : 0);
            faceBlend.Add("hoho2", hoho2_checkBox.Checked ? 1000 : 0);
            faceBlend.Add("hohol", hohol_checkBox.Checked ? 1000 : 0);
            faceBlend.Add("hohos", hohos_checkBox.Checked ? 1000 : 0);

            faceBlend.Add("mayuha", mayuha_trackBar.Value);
            faceBlend.Add("mayuup", mayuup_trackBar.Value);
            faceBlend.Add("mayuv", mayuv_trackBar.Value);
            faceBlend.Add("mayuvhalf", mayuvhalf_trackBar.Value);
            faceBlend.Add("mayuw", mayuw_trackBar.Value);

            faceBlend.Add("moutha", moutha_trackBar.Value);
            faceBlend.Add("mouthc", mouthc_trackBar.Value);
            faceBlend.Add("mouthdw", mouthdw_trackBar.Value);
            faceBlend.Add("mouthhe", mouthhe_trackBar.Value);
            faceBlend.Add("mouthi", mouthi_trackBar.Value);
            faceBlend.Add("mouths", mouths_trackBar.Value);
            faceBlend.Add("mouthup", mouthup_trackBar.Value);
            faceBlend.Add("mouthuphalf", mouthuphalf_trackBar.Value);

            faceBlend.Add("tangopen", tangopen_trackBar.Value);
            faceBlend.Add("tangout", tangout_trackBar.Value);
            faceBlend.Add("tangup", tangup_trackBar.Value);

            faceBlend.Add("namida", namida_trackBar.Value);
            faceBlend.Add("nosefook", nosefook_checkBox.Checked ? 1f : 0f);
            faceBlend.Add("shock", shock_checkBox.Checked ? 1000 : 0);
            faceBlend.Add("tear1", tear1_trackBar.Value);
            faceBlend.Add("tear2", tear2_trackBar.Value);
            faceBlend.Add("tear3", tear3_trackBar.Value);
            faceBlend.Add("toothoff", toothoff_trackBar.Value);
            faceBlend.Add("yodare", yodare_trackBar.Value);

            //GP01
            if (CurrentSelectedMaid.body0.Face.PartsVersion >= GP01FaceVersion)
            {
                faceBlend.Add("eyeclose7", eyeclose7_trackBar.Value);
                faceBlend.Add("eyeclose8", eyeclose8_trackBar.Value);
                faceBlend.Add("mouthfera", mouthfera_trackBar.Value);
                faceBlend.Add("mouthferar", mouthferar_trackBar.Value);
            }
            return faceBlend;
        }

        private int GetFaceBlendIndex(string Name)
        {
            if (true == CurrentMaidFaceBlendComboBox.Items.Contains(Name))
            {
                return CurrentMaidFaceBlendComboBox.Items.IndexOf(Name);
            }

            return CurrentMaidFaceBlendComboBox.Items.IndexOf("カスタム");
        }

        private void SetSelectFaceBlend(string Name)
        {
            if (null == Name)
            {
                CurrentMaidFaceBlendComboBox.SelectedIndex = GetFaceBlendIndex("カスタム");
            }
            else if (true == CurrentMaidFaceBlendComboBox.Items.Contains(Name))
            {
                CurrentMaidFaceBlendComboBox.SelectedIndex = GetFaceBlendIndex(Name);
            }
            else
            {
                CurrentMaidFaceBlendComboBox.SelectedIndex = GetFaceBlendIndex("カスタム");
            }
        }

        private void AddNewFaceBlend(string Name)
        {
            if (false == CurrentMaidFaceBlendComboBox.Items.Contains(Name))
            {
                CurrentMaidFaceBlendComboBox.Items.Add(Name);
            }
            else
            {
                AddNewFaceBlend(Name + "1");
            }
        }

        private void AddCustomFaceBlend(Maid maid)
        {
            if (null == maid)
            {
                return;
            }

            if (false == maid.body0.Face.morph.dicBlendSet.ContainsKey("カスタム"))
            {
#if DEBUG
                Debuginfo.Log("Add NewBlendSet カスタム for " + maid.status.callName, 2);
#endif
                maid.body0.Face.morph.NewBlendSet("カスタム");
                AddNewFaceBlend("カスタム");

                string FaceBlendDirectory = Directory.GetCurrentDirectory() + @"\Mod\MultipleMaidsPose\";
                string[] FaceBlendFiles = Directory.GetFiles(FaceBlendDirectory, "*.cfb");
                string BlendSetName;

                foreach (string Name in FaceBlendFiles)
                {
                    string[] ShortName = Name.Split('\\');
                    BlendSetName = ShortName[ShortName.Length - 1];
                    if (false == maid.body0.Face.morph.dicBlendSet.ContainsKey(BlendSetName))
                    {
                        maid.body0.Face.morph.NewBlendSet(BlendSetName);
                        AddNewFaceBlend(BlendSetName);
                    }
#if DEBUG
                    Debuginfo.Log("Loading CFB FileName=" + Name, 2);
#endif
                    long CFB_Size = new FileInfo(Name).Length;
                    byte[] CFB_Byte = new byte[CFB_Size + 1];
                    FileStream CFB_FS = new FileStream(Name, FileMode.OpenOrCreate);
                    CFB_FS.Read(CFB_Byte, 0, CFB_Byte.Length);
                    CFB_FS.Close();

                    using (MemoryStream CFB_stream = new MemoryStream(CFB_Byte))
                    {
                        Dictionary<string, float> faceBlend = (Dictionary<string, float>)EMES.DeserializeFromStream(CFB_stream);
                        foreach (KeyValuePair<string, float> fb in faceBlend)
                        {
                            maid.body0.Face.morph.SetValueBlendSet(BlendSetName, fb.Key, fb.Value * 100);
                        }
                    }
                }

                LoadCustomMaidFaceBlendFiles();
            }
        }
        private void LoadCustomMaidFaceBlendFiles()
        {
            string FaceBlendDirectory = Directory.GetCurrentDirectory() + @"\Mod\MultipleMaidsPose\";
            string[] FaceBlendFiles = Directory.GetFiles(FaceBlendDirectory, "*.cfb");

            foreach (string Name in FaceBlendFiles)
            {
                string[] ShortName = Name.Split('\\');

                if (false == CurrentMaidFaceBlendComboBox.Items.Contains(ShortName[ShortName.Length - 1]))
                {
                    CurrentMaidFaceBlendComboBox.Items.Add(ShortName[ShortName.Length - 1]);
                }
            }
        }

        private void DeleteFaceBlend(string Name)
        {
            if (true == CurrentMaidFaceBlendComboBox.Items.Contains(Name))
            {
                CurrentMaidFaceBlendComboBox.Items.Remove(Name);
            }
        }

        private void SyneFaceBlendToTrackBar()
        {
            Dictionary<string, float> faceBlend = new Dictionary<string, float>();
            string FaceBlendName = CurrentMaidFaceBlendComboBox.SelectedItem.ToString();
            foreach (DictionaryEntry entry in CurrentSelectedMaid.body0.Face.morph.hash)
            {
#if DEBUG
                //Debuginfo.Log("key =" + entry.Key + "   keyV = " + entry.Value + "   value=" + CurrentSelectedMaid.body0.Face.morph.dicBlendSet[FaceBlendName][int.Parse(entry.Value.ToString())], 2);
#endif
                faceBlend.Add(entry.Key.ToString(), CurrentSelectedMaid.body0.Face.morph.dicBlendSet[FaceBlendName][int.Parse(entry.Value.ToString())]);
                CurrentSelectedMaid.body0.Face.morph.dicBlendSet["カスタム"][int.Parse(entry.Value.ToString())] = CurrentSelectedMaid.body0.Face.morph.dicBlendSet[FaceBlendName][int.Parse(entry.Value.ToString())];
            }
            LockBusy();
            SetTrackBarValues(faceBlend);
            UnlockBusy();
        }

        private void SetItemChangeTemp(Maid maid, string[] key, string[] value, bool bSet)
        {
            if (true == bSet)
            {
                for (int Index = 0; Index < key.Length; Index++)
                {
                    maid.ItemChangeTemp(key[Index], value[Index]);
                }
            }
            else
            {
                for (int Index = 0; Index < key.Length; Index++)
                {
                    ResetProp(maid, (MPN)System.Enum.Parse(typeof(MPN), key[Index]));
                }
            }
            maid.AllProcPropSeqStart();
        }

        private void ResetClothSpecialProp()
        {
            LockBusy();
            PanzMizugiZurashiCheckBox.Checked = false;
            SkirtUpCheckBox.Checked = false;
            SkirtBehindFlipCheckBox.Checked = false;
            UnlockBusy();
        }

        private void SyncAllHandleExCheckBox()
        {
            if (false == Super.MaidIK.IsIKInitCompleted() || true == Super.MaidIK.IsLockIK())
            {
                return;
            }

            LockBusy();
            Dictionary<EMES_MaidIK.BoneType, HandleEx> handles = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].handleEx;
            foreach (KeyValuePair<EMES_MaidIK.BoneType, HandleEx> handle in handles)
            {
                if (true == handle.Key.ToString().Contains("Hip"))
                {
                    continue;
                }
                else if (true == handle.Key.ToString().Contains("Offset"))
                {
                    Offset_PositionCheckbox.Checked = handle.Value.Visible;
                    Offset_PositionCheckbox.Enabled = true;
                }
                else if (true == handle.Key.ToString().Contains("Root"))
                {
                    if (true == handle.Value.IK_GetHandleKunPosotionMode())
                    {
                        Bip01_PositionCheckBox.Checked = handle.Value.Visible;
                        Bip01_RotationCheckbox.Checked = false;

                    }
                    else
                    {
                        Bip01_PositionCheckBox.Checked = false;
                        Bip01_RotationCheckbox.Checked = handle.Value.Visible;
                    }
                }
                else if (true == handle.Key.ToString().Contains("Eye"))
                {
                    if (true == handle.Key.ToString().Contains("L"))
                        MaidFace_ShowEyesHandleL_checkBox.Checked = handle.Value.Visible;
                    else
                        MaidFace_ShowEyesHandleR_checkBox.Checked = handle.Value.Visible;
                }
                else if (true == handle.Key.ToString().Contains("Mune"))
                {
                    if (true == handle.Key.ToString().Contains("L"))
                        Mune_L_checkBox.Checked = handle.Value.Visible;
                    else
                        Mune_R_checkBox.Checked = handle.Value.Visible;
                }
                else
                {
                    if (HandleEx.IKMODE.UniversalIK != handle.Value.IKmode)
                    {
                        (Controls.Find("Bip01_" + handle.Key, true)[0] as CheckBox).Checked = handle.Value.Visible;
                        (Controls.Find("Bip01_" + handle.Key, true)[0] as CheckBox).Enabled = true;
                    }
                    else
                    {
                        (Controls.Find("Bip01_" + handle.Key, true)[0] as CheckBox).Checked = false;
                        (Controls.Find("Bip01_" + handle.Key, true)[0] as CheckBox).Enabled = false;
                    }
                }
            }

            //IK      
            RightArmIKCheckBox.Checked = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKRA;
            LeftArmIKCheckBox.Checked = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKLA;
            RightRegIKCheckBox.Checked = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKRL;
            LeftLegIKCheckBox.Checked = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKLL;

            //指先
            Bip01_R_Finger.Checked = false;
            Bip01_L_Finger.Checked = false;
            Bip01_R_Toe.Checked = false;
            Bip01_L_Toe.Checked = false;
            RightHandPoseComboBox.SelectedIndex = RightHandPoseComboBox.Items.Count - 1;
            LeftHandPoseComboBox.SelectedIndex = LeftHandPoseComboBox.Items.Count - 1;
            UnlockBusy();
        }

        private void ToggleFingerToeComboBox(string BipName, bool bChecked, bool bEnable)
        {
            int max = 5;
            if (true == BipName.Contains("Toe"))
                max = 3;

            LockBusy();
            for (int i = 0; i < max; i++)
            {
                CheckBox a = (Controls.Find(BipName + i.ToString(), true)[0] as CheckBox);
                CheckBox b = (Controls.Find(BipName + i.ToString() + "1", true)[0] as CheckBox);
                a.Checked = bChecked;
                a.Enabled = bEnable;
                b.Checked = bChecked;
                b.Enabled = bEnable;
                if (5 == max)
                {
                    CheckBox c = (Controls.Find(BipName + i.ToString() + "2", true)[0] as CheckBox);
                    c.Checked = bChecked;
                    c.Enabled = bEnable;
                }
            }
            UnlockBusy();
        }

        private void TogglePoseComboBox(string sender, bool bChecked, bool bEnable)
        {
            string Key = sender[0].ToString();
            if (true == sender.Contains("Arm"))
            {
                CheckBox hand = (Controls.Find("Bip01_" + Key + "_Hand", true)[0] as CheckBox);
                CheckBox forearm = (Controls.Find("Bip01_" + Key + "_Forearm", true)[0] as CheckBox);
                CheckBox upperarm = (Controls.Find("Bip01_" + Key + "_UpperArm", true)[0] as CheckBox);
                CheckBox clavicle = (Controls.Find("Bip01_" + Key + "_Clavicle", true)[0] as CheckBox);
                hand.Checked = bChecked;
                hand.Enabled = bEnable;
                forearm.Checked = bChecked;
                forearm.Enabled = bEnable;
                upperarm.Checked = bChecked;
                upperarm.Enabled = bEnable;
                clavicle.Checked = bChecked;
                clavicle.Enabled = bEnable;
            }
            else
            {
                CheckBox foot = (Controls.Find("Bip01_" + Key + "_Foot", true)[0] as CheckBox);
                CheckBox calf = (Controls.Find("Bip01_" + Key + "_Calf", true)[0] as CheckBox);
                CheckBox thigh = (Controls.Find("Bip01_" + Key + "_Thigh", true)[0] as CheckBox);
                foot.Checked = bChecked;
                foot.Enabled = bEnable;
                calf.Checked = bChecked;
                calf.Enabled = bEnable;
                thigh.Checked = bChecked;
                thigh.Enabled = bEnable;
            }
        }

        private void DelPrefabByClass(Maid maid, Type type, string senderName, bool bNoCategoryCheck)
        {
#if DEBUG
            Debuginfo.Log("Try DelPrefab >>> " + senderName, 2);
#endif
            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (PropertyInfo property in propertyInfos)
            {
                foreach (string Key in (List<string>)property.GetValue(type, null))
                {
                    if (true == Key.Contains(senderName) || true == bNoCategoryCheck)
                    {
#if DEBUG
                        Debuginfo.Log("DelPrefab " + Key, 2);
#endif
                        maid.DelPrefab(Key);
                    }
                }
            }
        }

        private void Dance_AutoSelectDancer(int CharaNum)
        {
            bool MaidsReady = true;
            if (1 == CharaNum)
            {
                EMES_Dance.Dance.Dancer[1] = null;
                EMES_Dance.Dance.Dancer[2] = null;
                EMES_Dance.Dance.Dancer[3] = null;
            }

            for (int i = 0; i < CharaNum; i++)
            {
                if (null == EMES_Dance.Dance.Dancer[i])
                {
                    MaidsReady = false;
                    break;
                }
            }

            bool MaidNotInUse = true;
            if (false == MaidsReady)
            {
                for (int Index = 0; Index < CharaNum; Index++)
                {
                    if (null != EMES_Dance.Dance.Dancer[Index])
                        continue;

                    for (int i = 0; i < CurrentMaidsStockID.Count; i++)
                    {
                        MaidNotInUse = true;
                        for (int j = 0; j < CharaNum; j++)
                        {
                            if (EMES_Dance.Dance.Dancer[j] == CurrentMaidsList[i])
                            {
                                MaidNotInUse = false;
                            }
                        }

                        if (true == MaidNotInUse)
                        {
                            EMES_Dance.Dance.Dancer[Index] = CurrentMaidsList[i];
                            Dance_SubPose_ComboBox.Items[Index] = Enum.GetNames(typeof(EMES_Dance.Dance.DancerPosition))[Index] + " ： " + CurrentMaidsList[i].status.callName;
                            break;
                        }
                    }
                }
            }

            for (int j = 0; j < EMES_Dance.Dance.DancerCount; j++)
            {
                if (null == EMES_Dance.Dance.Dancer[j])
                    continue;

                if (true == Dance_AllOther.ContainsKey(EMES_Dance.Dance.Dancer[j]))
                {
                    Dance_AllOther.Remove(EMES_Dance.Dance.Dancer[j]);
                }
            }

#if DEBUG
            Debuginfo.Log("EMES_Dance.Dance.Dancer[0] = " + EMES_Dance.Dance.Dancer[0].status.callName, 2);

            if (null != EMES_Dance.Dance.Dancer[1])
                Debuginfo.Log("EMES_Dance.Dance.Dancer[1] = " + EMES_Dance.Dance.Dancer[1].status.callName, 2);

            if (null != EMES_Dance.Dance.Dancer[2])
                Debuginfo.Log("EMES_Dance.Dance.Dancer[2] = " + EMES_Dance.Dance.Dancer[2].status.callName, 2);

            if (null != EMES_Dance.Dance.Dancer[3])
                Debuginfo.Log("EMES_Dance.Dance.Dancer[3] = " + EMES_Dance.Dance.Dancer[3].status.callName, 2);

#endif
        }

        private void Dance_StartAllOther()
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int CharaNum = EMES_Dance.Dance.Data[DanceName].iCharaNum;

            Dance_AllOther.Clear();
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                if (true == Dance_SubPoseAll_CheckBox.Checked)
                {
                    if (false == EMES_Dance.Dance.Data[DanceName].bAbsoluteANMName)
                    {
                        Dance_AllOther.Add(CurrentMaidsList[i], CurrentMaidsList[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + (Dance_SubPose_ComboBox.SelectedIndex + 1).ToString() + ".anm");
                    }
                    else
                    {
                        Dance_AllOther.Add(CurrentMaidsList[i], CurrentMaidsList[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + ".anm");
                    }
                }
                else
                {
                    if (false == EMES_Dance.Dance.Data[DanceName].bAbsoluteANMName)
                    {
                        Super.PerformPose(CurrentMaidsList[i], CurrentMaidsList[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + (Dance_SubPose_ComboBox.SelectedIndex + 1).ToString() + ".anm");
                    }
                    else
                    {
                        Super.PerformPose(CurrentMaidsList[i], CurrentMaidsList[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + ".anm");
                    }
                }
            }
            if (true == Dance_SubPoseAll_CheckBox.Checked)
            {
                for (int j = 0; j < EMES_Dance.Dance.DancerCount; j++)
                {
                    if (null == EMES_Dance.Dance.Dancer[j])
                        continue;

                    if (true == Dance_AllOther.ContainsKey(EMES_Dance.Dance.Dancer[j]))
                    {
                        Dance_AllOther.Remove(EMES_Dance.Dance.Dancer[j]);
                    }
                }
                Debuginfo.Log("スタンバイダンス", 1);
                GameMain.Instance.SoundMgr.PlaySe(EMES_Dance.Dance.SE[0] + ".ogg", false);
            }
            lastPerformedPose = LastPerformedPose.ANM;
        }

        private void Dance_StartAll()
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int CharaNum = EMES_Dance.Dance.Data[DanceName].iCharaNum;

            if (null == EMES_Dance.Dance.Dancer[0] || null == EMES_Dance.Dance.Dancer[1] || null == EMES_Dance.Dance.Dancer[2] || null == EMES_Dance.Dance.Dancer[3])
                Dance_AutoSelectDancer(CharaNum);

            if (false == Dance_NoBGM_CheckBox.Checked)
                GameMain.Instance.SoundMgr.PlayDanceBGM(Dance_SubBGM_ComboBox.SelectedItem + ".ogg", 0f, false);

            for (int i = 0; i < CharaNum; i++)
            {
                if (null == EMES_Dance.Dance.Dancer[i])
                    continue;

                if (i > 0)
                {
                    EMES_Dance.Dance.Dancer[i].SetPos(EMES_Dance.Dance.Dancer[0].GetPos());
                    EMES_Dance.Dance.Dancer[i].SetRot(EMES_Dance.Dance.Dancer[0].GetRot());
                }
#if DEBUG
                Debuginfo.Log("[" + i.ToString() + "] " + EMES_Dance.Dance.Dancer[i].status.firstName + " " + EMES_Dance.Dance.Dancer[i].status.lastName, 2);
#endif
                if (false == EMES_Dance.Dance.Data[DanceName].bAbsoluteANMName)
                {
                    Super.PerformPose(EMES_Dance.Dance.Dancer[i], EMES_Dance.Dance.Dancer[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + (i + 1).ToString() + ".anm");
                }
                else
                {
                    Super.PerformPose(EMES_Dance.Dance.Dancer[i], EMES_Dance.Dance.Dancer[i].IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + ".anm");
                }

                lastPerformedPose = LastPerformedPose.ANM;
            }

            if (true == Dance_SubPoseAll_CheckBox.Checked)
            {
                foreach (KeyValuePair<Maid, string> Key in Dance_AllOther)
                {
                    Super.PerformPose(Key.Key, Key.Value);
                }
            }
        }

        private void Yotogi_WindowInit()
        {
            LockBusy();
            Yotogi_Category_comboBox.Items.Clear();
            foreach (KeyValuePair<string, string> cat in Super.Yotogi.Yotogi_Category)
            {
                Yotogi_Category_comboBox.Items.Add(cat.Value);
            }
            Yotogi_Category_comboBox.SelectedIndex = 0;
            Yotogi_Category_comboBox.Select();

            BindingSource bs = new BindingSource();
            bs.DataSource = Super.Yotogi.Yotogi_data["汎用"];
            Yotogi_Item_comboBox.DataSource = bs;
            Yotogi_Item_comboBox.SelectedIndex = 0;
            Yotogi_Item_comboBox.Select();

            Super.Yotogi.Yotogi_InitPerformer(true, true, false, false, EMES_Yotogi.YotogiState.娚);
            Yotogi_Role_comboBox.Items.Clear();
            foreach (KeyValuePair<EMES_Yotogi.YotogiRole, Maid> Key in Super.Yotogi.YotogiRoleList)
            {
                Yotogi_Role_comboBox.Items.Add(Key.Key.ToString());
            }
            Yotogi_Role_comboBox.SelectedIndex = 0;
            Yotogi_Role_comboBox.Select();
            UnlockBusy();
        }

        private void CameraPlus_ShaderWindowInit()
        {
#if DEBUG
            Debuginfo.Log("Load Bloom", 2);
#endif
            int screenBlendMode;
            int quality;
            int sepBlurSpread;
            int bloomBlurIterations;
            Super.exShader.Get_BloomData(out screenBlendMode, out quality, out sepBlurSpread, out bloomBlurIterations);
            Shader_Bloom_screenBlendMode_comboBox.SelectedIndex = screenBlendMode;
            Shader_Bloom_screenBlendMode_comboBox.Select();
            Shader_Bloom_BloomQuality_comboBox.SelectedIndex = quality;
            Shader_Bloom_BloomQuality_comboBox.Select();
            SetTrackBarValue(Shader_Bloom_sepBlurSpread_trackBar, sepBlurSpread);
            SetTrackBarValue(Shader_Bloom_bloomBlurIterations_trackBar, bloomBlurIterations);

#if DEBUG
            Debuginfo.Log("Load Blur", 2);
#endif
            bool blurEnable;
            int blurSize;
            int blurIterations;
            Super.exShader.Get_BlurData(out blurEnable, out blurSize, out blurIterations);
            Shader_Blur_Enable_checkbox.Checked = blurEnable;
            SetTrackBarValue(Shader_Blur_blurSize_trackBar, blurSize);
            SetTrackBarValue(Shader_Blur_blurIterations_trackBar, blurIterations);

#if DEBUG
            Debuginfo.Log("Load DOF", 2);
#endif
            bool DOFEnable;
            bool visualizeFocus;
            int focalLength;
            int focalSize;
            int aperture;
            int maxBlurSize;
            int blurType;
            Super.exShader.Get_DOFData(out DOFEnable, out visualizeFocus, out focalLength, out focalSize, out aperture, out maxBlurSize, out blurType);
            Shader_DOF_Enable_checkBox.Checked = DOFEnable;
            Shader_DOF_visualizeFocus_checkBox.Checked = visualizeFocus;
            SetTrackBarValue(Shader_DOF_focalLength_trackBar, focalLength);
            SetTrackBarValue(Shader_DOF_focalSize_trackBar, focalSize);
            SetTrackBarValue(Shader_DOF_aperture_trackBar, aperture);
            SetTrackBarValue(Shader_DOF_maxBlurSize_trackBar, maxBlurSize);
            Shader_DOF_blurType_comboBox.SelectedIndex = blurType;
            Shader_DOF_blurType_comboBox.Select();

#if DEBUG
            Debuginfo.Log("Load Fog", 2);
#endif
            bool bFOGEnable;
            int startDistance;
            int globalDensity;
            int heightScale;
            int height;
            System.Drawing.Color globalFogColor;
            Super.exShader.Get_GlobalFogData(out bFOGEnable, out startDistance, out globalDensity, out heightScale, out height, out globalFogColor);
            Shader_Fog_Enable_checkBox.Checked = bFOGEnable;
            SetTrackBarValue(Shader_Fog_startDistance_trackBar, startDistance);
            SetTrackBarValue(Shader_Fog_globalDensity_trackBar, globalDensity);
            SetTrackBarValue(Shader_Fog_heightScale_trackBar, heightScale);
            SetTrackBarValue(Shader_Fog_height_trackBar, height);
            Shader_Fog_globalFogColor_pictureBox.BackColor = globalFogColor;


#if DEBUG
            Debuginfo.Log("Load Sepia", 2);
#endif
            bool bSepiaEnable;
            Super.exShader.Get_SepiaData(out bSepiaEnable);
            Shader_Sepia_Enable_checkbox.Checked = bSepiaEnable;

#if DEBUG
            Debuginfo.Log("Load Vignetting", 2);
#endif
            bool bVignettingEnable;
            int iVignettingintensity;
            int iVignettingchromaticAberration;
            int iVignettingblurSpread;
            int iVignettingblur;
            Super.exShader.Get_VignettingData(out bVignettingEnable, out iVignettingintensity, out iVignettingchromaticAberration, out iVignettingblurSpread, out iVignettingblur);
            Shader_Vignette_Enable_checkBox.Checked = bVignettingEnable;
            SetTrackBarValue(Shader_Vignetting_intensity_trackBar, iVignettingintensity);
            SetTrackBarValue(Shader_Vignetting_chromaticAberration_trackBar, iVignettingchromaticAberration);
            SetTrackBarValue(Shader_Vignetting_blurSpread_trackBar, iVignettingblurSpread);
            SetTrackBarValue(Shader_Vignetting_blur_trackBar, iVignettingblur);
        }

        private void CameraPlus_WindowInit()
        {
            LockBusy();
            CameraPlus_ShaderWindowInit();
#if DEBUG
            Debuginfo.Log("Setup subLight", 2);
#endif
            SetTrackBarValue(Light_Main_X_trackBar, 40 * 100);
            SetTrackBarValue(Light_Main_Y_trackBar, 180 * 100);
            SetTrackBarValue(Light_Main_Brightness_trackBar, (int)(0.95f * 10000));
            SetTrackBarValue(Light_Main_Shadow_trackBar, (int)(0.098f * 10000));
            Light_Main_Color_pictureBox.BackColor = System.Drawing.Color.White;

            SubLight_type_comboBox.SelectedIndex = 0;
            SubLight_type_comboBox.Select();

            SubLight_index_comboBox.Items.Clear();
            SubLight_index_comboBox.Items.Add("無");
            SubLight_index_comboBox.SelectedIndex = 0;
            SubLight_index_comboBox.Select();

            MessageWindow_maidname_comboBox.SelectedIndex = 3;
            MessageWindow_maidname_comboBox.Select();
            UnlockBusy();
        }

        private void IK_WindowInit()
        {
            LockBusy();
            MaidTails_TryCreateIKChain_comboBox.SelectedIndex = 0;
            MaidTails_TryCreateIKChain_comboBox.Select();

            Prefab_Bone_comboBox.Items.Clear();
            foreach (string Key in Enum.GetNames(typeof(EMES_MaidIK.IKBoneBinding)))
            {
                Prefab_Bone_comboBox.Items.Add(Key.ToString());
            }
            Prefab_Bone_comboBox.SelectedIndex = 0;
            Prefab_Bone_comboBox.Select();
            UnlockBusy();
        }

        private void SetSceneEditMaid(int MaidIndex, bool bDoNotMoveCamera)
        {
            SceneEdit componentSceneEdit = GameObject.Find("__SceneEdit__").GetComponent<SceneEdit>();
            SetFieldValue<SceneEdit, Maid>(componentSceneEdit, "m_maid", CurrentMaidsList[MaidIndex]);
            if (false == bDoNotMoveCamera)
                componentSceneEdit.PartsTypeCamera(MPN.stkg);
#if DEBUG
            Debuginfo.Log("SceneEdit.m_maid=" + SceneEdit.Instance.maid.status.firstName + " " + SceneEdit.Instance.maid.status.lastName, 2);
#endif
        }

        private void SyncMaidGravity()
        {
            float x = 0;
            float y = 0;
            float z = 0;
            float sy = CurrentSelectedMaid.body0.BoneHitHeightY;

            for (int index = 0; index < CurrentSelectedMaid.body0.goSlot.Count; index++)
            {

                if (CurrentSelectedMaid.body0.goSlot[index].obj != null)
                {
                    DynamicBone component2 = CurrentSelectedMaid.body0.goSlot[index].obj.GetComponent<DynamicBone>();
                    if (component2 != null && component2.enabled)
                    {
                        x = component2.m_Gravity.x;
                        y = component2.m_Gravity.y;
                        z = component2.m_Gravity.z;
                        break;
                    }
                }
            }

            Vector3 softG = new Vector3(x, y, z) * -10f;

            LockBusy();
            Gravity_x_textBox.Text = x.ToString();
            Gravity_x_trackBar.Value = (int)(x * 1000);
            Gravity_y_textBox.Text = y.ToString();
            Gravity_y_trackBar.Value = (int)(y * 1000);
            Gravity_z_textBox.Text = z.ToString();
            Gravity_z_trackBar.Value = (int)(z * 1000);
            Gravity_sy_textBox.Text = sy.ToString();
            Gravity_sy_trackBar.Value = (int)(sy * 1000);
            UnlockBusy();
        }

        private void UpdateMaidGravity(int Index)
        {
            float x = ((float)Gravity_x_trackBar.Value / 1000);
            float y = ((float)Gravity_y_trackBar.Value / 1000);
            float z = ((float)Gravity_z_trackBar.Value / 1000);
            float sy = ((float)Gravity_sy_trackBar.Value / 1000);

            Vector3 softG = new Vector3(x, y, z) * -0.1f;

            foreach (TBody.SlotID slotID in Enum.GetValues(typeof(TBody.SlotID)))
            {
                string sSlotName = Enum.GetName(typeof(TBody.SlotID), slotID);

                if (true == sSlotName.Equals("end"))
                    break;

                if (true == CurrentMaidsList[Index].body0.GetSlotLoaded(slotID))
                {
                    DynamicBone component2 = CurrentMaidsList[Index].body0.GetSlot((int)slotID).obj.GetComponent<DynamicBone>();
                    if (null != component2)
                    {
                        if (true == component2.enabled)
                        {
                            component2.m_Gravity = new Vector3(softG.x, softG.y, softG.z);
                        }
                    }
                }
            }
            
            CurrentMaidsList[Index].body0.BoneHitHeightY = sy;
        }

        private void SyncIKtoANM(Maid maid)
        {
#if DEBUG
            Debuginfo.Log("IKポーズ自動同期", 2);
#endif
            CacheBoneDataArray cacheBoneDataArray = maid.gameObject.AddComponent<CacheBoneDataArray>();
            cacheBoneDataArray.CreateCache(maid.body0.GetBone("Bip01"));
            byte[] anmBinary = cacheBoneDataArray.GetAnmBinary(true, true);

            Super.Yotogi.Yotogi_LoadAnime(maid, maid.status.guid + "_IK.anm", anmBinary, false, false);
            maid.body0.m_Bones.GetComponent<Animation>().Play(maid.status.guid + "_IK.anm");
        }

        private GameObject Items_GetCurrentMaidBone()
        {
            switch (Prefab_Bone_comboBox.SelectedItem.ToString())
            {
                case "無し":
                    return null;
                case "頭":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 Head", true).gameObject;
                case "背中":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 Spine1a", true).gameObject;
                case "左手":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 L Hand", true).gameObject;
                case "右手":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 R Hand", true).gameObject;
                case "左足":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 L Foot", true).gameObject;
                case "右足":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "Bip01 R Foot", true).gameObject;
                case "XXX前":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "_IK_vagina", true).gameObject;
                case "XXX後":
                    return CMT.SearchObjName(CurrentSelectedMaid.body0.transform, "_IK_anal", true).gameObject;
            }

            return null;
        }

        private void Items_UpdateDynamicBoneSataus()
        {
            LockBusy();
            HandleEx posHandle = (HandleEx)Items_SubObjects_listBox.SelectedValue;
            DynamicBone dynamicBone = posHandle.parentBone.GetComponent<DynamicBone>();
            if (null != dynamicBone)
            {
#if DEBUG
                Debuginfo.Log(posHandle.parentBone.name + ": DynamicBone.enabled = " + dynamicBone.enabled, 2);
#endif
                Items_SubObjects_DynamicBone_checkBox.Checked = dynamicBone.enabled;
            }
            UnlockBusy();
        }

        private void Items_UpdateCurrentHandleFunc()
        {
            Action<bool> action_ToggleEnable = delegate (bool bTrigger)
            {
                Items_LoadImage_Projection_checkBox.Enabled = bTrigger;
                Items_LoadImage_Shadow_comboBox.Enabled = bTrigger;
                Items_LoadImage_Shader_comboBox.Enabled = bTrigger;
            };

            Action<GameObject> action_UpdateLoadImageGameObject = delegate (GameObject gameObject)
            {
                Renderer render = gameObject.GetComponent<Renderer>();
                LockBusy();
                if (null == render)
                {
                    action_ToggleEnable(false);
                }
                else
                {
                    action_ToggleEnable(true);
                    Items_LoadImage_Projection_checkBox.Checked = render.receiveShadows;
                    Items_LoadImage_Shadow_comboBox.SelectedIndex = (int)render.shadowCastingMode;
                    Items_LoadImage_Shadow_comboBox.Select();
                }
                UnlockBusy();
            };

            Action<HandleEx> action_UpdateMaterialInfo = delegate (HandleEx posHandle)
            {
                GameObject gameObject = posHandle.parentBone;
                Renderer render = gameObject.GetComponent<Renderer>();
                if (null != render)
                {
                    if (null != render.material.shader)
                    {
                        string shaderName = render.material.shader.name;
                        if (true == Items_LoadImage_Shader_comboBox.Items.Contains(shaderName))
                        {
                            Items_LoadImage_Shader_comboBox.SelectedIndex = Items_LoadImage_Shader_comboBox.Items.IndexOf(shaderName);
                            Items_LoadImage_Shader_comboBox.Select();
                        }
                    }

                    Items_LoadImage_Projection_checkBox.Checked = render.receiveShadows;
                    Items_LoadImage_Shadow_comboBox.SelectedIndex = (int)render.shadowCastingMode;
                    Items_LoadImage_Shadow_comboBox.Select();
                }

                if (true == posHandle.sItemName.Contains("D>"))
                {
                    Items_SubObjects_DynamicBone_checkBox.Visible = true;
                }
                else
                {
                    Items_SubObjects_DynamicBone_checkBox.Visible = false;
                }
            };

            Action<bool, bool> action_DisableFunctions = delegate (bool bTab, bool bVisible)
            {
                Items_Settings_tabControl.Enabled = bTab;

                LockBusy();
                Items_Handle_Visible_checkBox.Checked = bVisible;
                UnlockBusy();

                Items_SubObjects_DynamicBone_checkBox.Visible = false;
            };

            Action action_DisableAll = delegate ()
            {
                Items_Control_groupBox.Enabled = false;
                Items_Settings_tabControl.Enabled = false;
                Items_Handle_Visible_checkBox.Enabled = false;
            };

            Action action_ReloadMaidParts = delegate ()
            {
#if DEBUG
                Debuginfo.Log("Items_UpdateCurrentHandleFunc() posHandle.goHandleMasterObject == null  再初期化", 2);
#endif
                action_DisableAll();
                if (true == Super.Parts.Init(CurrentSelectedMaid))
                {
                    Items_UpdateCurrentHandleCount();
                }
            };

            if (Items_list_tabPage1 == Items_List_tabControl.SelectedTab)
            {
                if (Items_HandledObjects_listBox.SelectedItems.Count > 0)
                {
                    Super.Items.Items_UpdateSelectedHandleExList(Items_HandledObjects_listBox.SelectedItems);

                    Items_Control_groupBox.Enabled = true;
                    Items_Handle_Remove_Button.Enabled = true;

                    HandleEx posHandle = (HandleEx)Items_HandledObjects_listBox.SelectedValue;
                    if (false == posHandle.CheckParentAlive())
                    {
                        action_ReloadMaidParts();
                    }
                    else
                    {
                        if (1 == Items_HandledObjects_listBox.SelectedItems.Count)
                        {
                            Items_UpdateItemHandleScaleInfo(posHandle.parentBone);

                            if (true == posHandle.sCategory.Equals("効果") || true == posHandle.sCategory.Equals("ボディー"))
                            {
                                Items_Handle_Active_Button.Enabled = true;
                                Items_Handle_Visible_checkBox.Enabled = false;
                            }
                            else if (true == posHandle.sCategory.Equals("MaidPartsHandle"))
                            {
                                Items_Handle_Active_Button.Enabled = false;
                                Items_Handle_Visible_checkBox.Enabled = false;
                            }
                            else
                            {
                                Items_Handle_Active_Button.Enabled = false;
                                Items_Handle_Visible_checkBox.Enabled = true;
                            }

                            if (false == posHandle.sCategory.Equals("WildHandle"))
                            {
                                action_UpdateLoadImageGameObject(posHandle.parentBone);
                                action_ToggleEnable(true);
                            }
                            else
                            {
                                action_ToggleEnable(false);
                            }

                            Items_Settings_tabControl.Enabled = true;

                            LockBusy();
                            Items_Handle_Visible_checkBox.Checked = posHandle.parentBone.activeSelf;
                            action_UpdateMaterialInfo(posHandle);
                            UnlockBusy();
                        }
                        else
                        {
                            action_DisableFunctions(false, true);
                        }
                    }
                }
                else
                {
                    action_DisableAll();
                }
            }
            else if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                Items_Handle_Remove_Button.Enabled = false;

                if (Items_SubObjects_listBox.SelectedItems.Count > 0)
                {
                    HandleEx posHandle = (HandleEx)Items_SubObjects_listBox.SelectedValue;
                    if (false == posHandle.CheckParentAlive())
                    {
                        Items_ClearSubItems();
                        action_ReloadMaidParts();
                    }
                    else
                    {
                        Super.Items.Items_Sub_UpdateSelectedHandleExList(Items_SubObjects_listBox.SelectedItems);
                        if (1 == Items_HandledObjects_listBox.SelectedItems.Count)
                        {
                            GameObject gameObject = posHandle.parentBone;
                            Items_UpdateItemHandleScaleInfo(gameObject);

                            Items_Handle_Active_Button.Enabled = false;
                            Items_Handle_Visible_checkBox.Enabled = true;
                            Items_Settings_tabControl.Enabled = true;

                            action_UpdateLoadImageGameObject(gameObject);

                            LockBusy();
                            Items_Handle_Visible_checkBox.Checked = gameObject.activeSelf;
                            action_UpdateMaterialInfo(posHandle);
                            UnlockBusy();

                            Items_UpdateDynamicBoneSataus();
                        }
                        else
                        {
                            action_DisableFunctions(false, true);
                        }
                    }
                }
                else
                {
                    action_DisableAll();
                }
            }
        }

        private void UpdatePictureBox(Texture icon, PictureBox pBox)
        {
            UpdatePictureBox((DeCompress(icon as Texture2D)).EncodeToPNG(), pBox);
        }

        private void UpdatePictureBox(Texture2D icon, PictureBox pBox)
        {
            UpdatePictureBox(icon.EncodeToPNG(), pBox);
        }

        private void UpdatePictureBox(byte[] bStream, PictureBox pBox)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(bStream, 0, Convert.ToInt32(bStream.Length));
                pBox.Image = Image.FromStream(mStream);
            }
        }

        private void LoadTexToPictureBox(string sFileName, PictureBox pBox)
        {
            if (true == GameUty.FileSystem.IsExistentFile(sFileName))
            {
                UpdatePictureBox(DeCompress(ImportCM.CreateTexture(sFileName)), pBox);
            }
            else
            {
                pBox.Image = pBox.InitialImage;
            }
        }

        private void LoadPngFileToPictureBox(string sFileName, PictureBox pBox)
        {
            string sFullPath = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory + sFileName;

#if DEBUG
            Debuginfo.Log("Load Png " + sFullPath, 2);
#endif
            if (true == File.Exists(sFullPath))
            {
                UpdatePictureBox(System.IO.File.ReadAllBytes(sFullPath), pBox);
            }
            else
            {
#if DEBUG
                Debuginfo.Log("!" + sFileName, 2);
#endif
                pBox.Image = pBox.InitialImage;
            }
        }

        private void LoadTexFileToPictureBox(string sFileName, PictureBox pBox)
        {
            string sFullPath = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory + sFileName;

            if (sFileName.Contains("_icon.tex"))
            {
                if (true == GameUty.IsExistFile(sFullPath))
                {
#if DEBUG
                    Debuginfo.Log("Load Tex " + sFullPath, 2);
#endif
                    UpdatePictureBox(ImportCM.LoadTexture(GameUty.FileSystem, sFileName, true).CreateTexture2D(), pBox);
                }
                else
                {
                    if (true == File.Exists(sFullPath))
                    {
                        UpdatePictureBox(TexPngTools.TexCM3D2ToPngFromFile(sFullPath), pBox);
                    }
                    else
                    {
                        sFileName = sFileName.Replace("_icon.tex", "_icon.png");
                        LoadPngFileToPictureBox(sFileName, pBox);
                    }
                }
            }
            else if (sFileName.Contains("_icon.png"))
            {
                LoadPngFileToPictureBox(sFileName, pBox);
            }
            else
            {
#if DEBUG
                Debuginfo.Log("!" + sFileName, 2);
#endif
                pBox.Image = pBox.InitialImage;
            }
        }

        private void TryCreateSSIconToPictureBox(PictureBox pBox)
        {
            string senderCategory = Prefab_Category_comboBox.SelectedItem.ToString();
            string prefix = "Prefab/";

            KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu> selectedItem = (KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu>)Prefab_Items_comboBox.SelectedItem;
            string selectedItemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].name;
            EMES_Items.PhotoBGObjectData_Odogu selectedItemValue = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key];
            string sShowName = selectedItem.Key;

            if ("無" == selectedItemName)
            {
                pBox.Image = pBox.InitialImage;
                return;
            }
#if DEBUG
            Debuginfo.Log("senderCategory = " + senderCategory, 2);
            Debuginfo.Log("selectedItem.Key = " + selectedItem.Key, 2);
            Debuginfo.Log("Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].name = " + selectedItemName, 2);
#endif
            Vector3 pos = new Vector3(0f, 0f, -20f);
            Vector3 rot = new Vector3(0f, 0f, 0f);
            GameObject goItem = null;

            if ("効果" == senderCategory)
            {
                prefix = "Prefab/Particle/";
            }
            else if ("パーティクル" == senderCategory)
            {

            }
            else if ("手持品" == senderCategory)
            {
                prefix = "model/handitem/";
                rot = new Vector3(-90f, 0f, 0f);
                goItem = Super.Items.Items_CreatShadow(prefix, selectedItemName, pos, rot, null, null);
            }
            else if ("その他２" == senderCategory)
            {
                rot = pos;
                if ("Fish" == Prefab_Items_comboBox.SelectedItem.ToString() || true == Prefab_Items_comboBox.SelectedItem.ToString().Contains("Mob"))
                {
                    rot = new Vector3(-90f, 0f, 0f);
                }
                goItem = Super.Items.Items_CreatShadow(prefix, selectedItemName, pos, rot, null, null);
            }
            else if ("ボディー" == senderCategory)
            {
                prefix = "Prefab/Particle/";
            }
            else if ("鏡" == senderCategory || "水" == senderCategory)
            {
                prefix = "";
            }
            else if ("小物" == senderCategory)
            {
                string itemName = itemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].assert_bg;
                rot = new Vector3(0f, 0f, 0f);
                goItem = Super.Items.Items_CreatShadow(prefix, itemName, pos, rot, selectedItemValue, null);
            }
            else
            {
                string itemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].create_prefab_name;
                if (true == string.IsNullOrEmpty(itemName))
                {
                    itemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].create_asset_bundle_name;
                    if (true == string.IsNullOrEmpty(itemName))
                    {
                        itemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].assert_bg;
                    }

                    switch (senderCategory)
                    {
                        case "その他":
                        case "グルメ":
                            rot = new Vector3(0f, 0f, 0f);
                            break;
                    }
                    goItem = Super.Items.Items_CreatShadow(prefix, itemName, pos, rot, selectedItemValue, null);
                }
                else
                {
                    rot = new Vector3(-90f, 0f, 0f);

                    switch (senderCategory)
                    {
                        case "家具":
                            if (true == selectedItem.Key.Contains("イス"))
                            {
                                rot = new Vector3(0f, 0f, 0f);
                            }
                            break;
                        case "パーティクル":
                            rot = new Vector3(0f, 0f, 0f);
                            pos = CurrentSelectedMaid.GetPos();
                            break;
                    }
                    goItem = Super.Items.Items_CreatShadow(prefix, itemName, pos, rot, null, null);
                }
            }

            CreatePreviewIcon(goItem, pBox, true);
        }

        private void TryCreateDeskIconToPictureBox(PictureBox pBox)
        {
            string senderCategory = DeskObjects_Category_comboBox.SelectedItem.ToString();
            string prefix = "Prefab/";
            KeyValuePair<EMES_Items.DeskItemData, string> selectedItem = (KeyValuePair<EMES_Items.DeskItemData, string>)DeskObjects_Items_comboBox.SelectedItem;

            if ("無" == selectedItem.Value)
            {
                pBox.Image = pBox.InitialImage;
                return;
            }

#if DEBUG
            Debuginfo.Log("senderCategory = " + senderCategory, 2);
            Debuginfo.Log("selectedItem.Key = " + selectedItem.Key, 2);
            Debuginfo.Log("selectedItem.Value = " + selectedItem.Value, 2);
#endif

            Vector3 pos = new Vector3(0f, 0f, -20f);
            Vector3 rot = new Vector3(0f, -90f, 0f);
            GameObject goItem = null;

            if (true == string.IsNullOrEmpty(selectedItem.Key.asset_name))
            {
                goItem = Super.Items.Items_CreatShadow(prefix, selectedItem.Key.prefab_name, pos, rot, null, selectedItem.Key);
            }
            else
            {
                rot = new Vector3(-90f, -90f, 0f);
                goItem = Super.Items.Items_CreatShadow(prefix, selectedItem.Key.asset_name, pos, rot, null, selectedItem.Key);
            }

            CreatePreviewIcon(goItem, pBox, true);
        }

        private void CreatePreviewIcon(GameObject goItem, PictureBox pBox, bool bDestoryObject)
        {
            if (null != goItem)
            {
#if DEBUG
                Debuginfo.Log("Try posethumshot", 2);
#endif
                RuntimePreviewGenerator.BackgroundColor = new UnityEngine.Color(0, 0, 0, 0);
                int thumbnailSize = 128;

                Texture2D texture2D = RuntimePreviewGenerator.GenerateModelPreview(goItem.transform, thumbnailSize, thumbnailSize);
                if (null != texture2D)
                {
                    Texture2D texture2DCopy = duplicateTexture(texture2D);
#if DEBUG
                    Debuginfo.Log("Try EncodeToPNG to " + pBox.Name, 2);
#endif
                    UpdatePictureBox(texture2DCopy, pBox);
                }
                else
                {
#if DEBUG
                    Debuginfo.Warning("RuntimePreviewGenerator return NULL Texture2D", 0);
#endif
                    pBox.Image = pBox.InitialImage;
                }
                if (true == bDestoryObject)
                    UnityEngine.Object.DestroyImmediate(goItem);
            }
            else
            {
#if DEBUG
                Debuginfo.Warning("NULL GameObject", 0);
#endif
                pBox.Image = pBox.InitialImage;
            }
        }

        private void MaidPosAutoCenter()
        {
#if DEBUG
            Debuginfo.Log(CurrentSelectedMaid.status.callName + "をルートセンターに調整する ", 2);
#endif
            Transform trBone = CMT.SearchObjName(CurrentSelectedMaid.transform, "Bip01", true);
            trBone.localPosition = new Vector3(0, 0, 0);
        }

        private void MaidPoseCreateIcon(string ANMName, string FullPathPngFile)
        {
            int renderSize = 128;
            ThumShot posethumshot = GameMain.Instance.ThumCamera.GetComponent<ThumShot>();
            Transform transform = CMT.SearchObjName(CurrentSelectedMaid.transform, "Bip01 HeadNub", true);
            if (transform != null)
            {
                posethumshot.transform.position = transform.TransformPoint(transform.localPosition + new Vector3(0.7f, 1.6f, 0f));
                posethumshot.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 90f);
            }
            else
            {
                Debuginfo.Error("サムネイルを取ろうとしましたがメイドが居ません。");
                return;
            }

            //ここの間で他のメイドがいたら消す処理を加える
            //メイドの表示状態を記録しておく
            //ここで消す
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                if (CurrentMaidsList[i] != CurrentSelectedMaid)
                    CurrentMaidsList[i].Visible = false;
            }

            Camera poseshotthumCamera = posethumshot.gameObject.GetComponent<Camera>();
            poseshotthumCamera.fieldOfView = 50f;

            //撮影
            RenderTexture m_rtThumCard = new RenderTexture(renderSize, renderSize, 24, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Bilinear,
                antiAliasing = 8
            };
            RenderTexture m_rtThumCard2 = new RenderTexture(renderSize, renderSize, 0, RenderTextureFormat.ARGB32);

            Texture2D posetex = posethumshot.RenderThum(poseshotthumCamera, m_rtThumCard, m_rtThumCard2, new Size<int>(renderSize, renderSize), true);
            byte[] bytesPng = posetex.EncodeToPNG();
            File.WriteAllBytes(FullPathPngFile, TexPngTools.PngToTexCM3D2(bytesPng));

            //ここで消してたメイドを元に戻す
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                if (CurrentMaidsList[i] != CurrentSelectedMaid)
                    CurrentMaidsList[i].Visible = true;
            }

            Debuginfo.Log(ANMName + "エクスポートが完了しました", 0);
        }

        private void SetHandleOptions(HandleEx handle, bool receiveShadows)
        {
            if ("WildHandle" == handle.sCategory)
            {
                return;
            }

            bool bRendererInChild = false;
            Renderer render = handle.parentBone.GetComponent<Renderer>();
            if (null == render)
            {
                UnityEngine.Component[] componentsChildren = handle.parentBone.GetComponentsInChildren(typeof(UnityEngine.Component));
                for (int i = 0; i < componentsChildren.Length; i++)
                {
                    render = componentsChildren[i].gameObject.GetComponent<Renderer>();
                    if (null != render)
                    {
                        render.receiveShadows = receiveShadows;
                        bRendererInChild = true;
                    }
                }
            }

            if (false == bRendererInChild)
                render.receiveShadows = receiveShadows;

            if (true == string.Equals("無", handle.sOptions))
            {
                handle.SetOptions("-1|" + (render.receiveShadows ? "1" : "0") + "|" + ((int)render.shadowCastingMode).ToString() + "|無");
            }
            else
            {
                string[] options = handle.sOptions.Split('|');
                handle.SetOptions(options[(int)SceneOptions.PrimitiveType] + "|" + (render.receiveShadows ? "1" : "0") + "|" +
                                  ((int)render.shadowCastingMode).ToString() + "|" +
                                  options[(int)SceneOptions.Shader]);
            }
        }

        private void SetHandleOptions(HandleEx handle, ShadowCastingMode shadowCastingMode)
        {
            if ("WildHandle" == handle.sCategory)
            {
                return;
            }

            bool bRendererInChild = false;
            Renderer render = handle.parentBone.GetComponent<Renderer>();
            if (null == render)
            {
                UnityEngine.Component[] componentsChildren = handle.parentBone.GetComponentsInChildren(typeof(UnityEngine.Component));
                for (int i = 0; i < componentsChildren.Length; i++)
                {
                    render = componentsChildren[i].gameObject.GetComponent<Renderer>();
                    if (null != render)
                    {
                        render.shadowCastingMode = shadowCastingMode;
                        bRendererInChild = true;
                    }
                }
            }

            if (false == bRendererInChild)
                render.shadowCastingMode = shadowCastingMode;

            if (true == string.Equals("無", handle.sOptions))
            {
                handle.SetOptions("-1|" + (render.receiveShadows ? "1" : "0") + "|" + ((int)render.shadowCastingMode).ToString() + "|無");
            }
            else
            {
                string[] options = handle.sOptions.Split('|');
                handle.SetOptions(options[(int)SceneOptions.PrimitiveType] + "|" + options[(int)SceneOptions.ReceiveShadows] + "|" +
                                  ((int)render.shadowCastingMode).ToString() + "|" +
                                  options[(int)SceneOptions.Shader]);
            }
        }

        private void SetHandleOptions(HandleEx handle, string sShaderName)
        {
            if ("WildHandle" == handle.sCategory)
            {
                return;
            }

            if (true == string.Equals("無", sShaderName))
            {
                return;
            }

            bool bRendererInChild = false;
            Renderer render = handle.parentBone.GetComponent<Renderer>();
            if (null == render)
            {
                UnityEngine.Component[] componentsChildren = handle.parentBone.GetComponentsInChildren(typeof(UnityEngine.Component));
                for (int i = 0; i < componentsChildren.Length; i++)
                {
                    render = componentsChildren[i].gameObject.GetComponent<Renderer>();
                    if (null != render)
                    {
                        Shader shader = Shader.Find(sShaderName);
                        if (null != shader)
                        {
                            render.material.shader = shader;
                        }
                        else
                        {
                            sShaderName = "無";
                        }
                        bRendererInChild = true;
                    }
                }
            }

            if (false == bRendererInChild)
            {
                Shader shader = Shader.Find(sShaderName);
                if (null != shader)
                {
#if DEBUG
                    Debuginfo.Log("get  render.material.shader = " + render.material.shader.name + " >> " + shader, 0);
#endif 
                    render.material.shader = shader;
                }
                else
                {
                    sShaderName = "無";
                }
            }

            if (true == string.Equals("無", handle.sOptions))
            {
                handle.SetOptions("-1|" + (render.receiveShadows ? "1" : "0") + "|" + ((int)render.shadowCastingMode).ToString() + "|無");
            }
            else
            {
                string[] options = handle.sOptions.Split('|');
                handle.SetOptions(options[(int)SceneOptions.PrimitiveType] + "|" + options[(int)SceneOptions.ReceiveShadows] + "|" +
                                  options[(int)SceneOptions.ShadowCastingMode] + "|" +
                                  sShaderName);
            }
        }

        private bool CheckSubItems(HandleEx handle)
        {
            if (true == handle.sCategory.Equals("WildHandle") || true == handle.sCategory.Equals("ExternalImage"))
                return false;

            bool ret = false;
#if DEBUG
            Debuginfo.Log("Load sub objects....   " + DateTime.Now, 0);
#endif

            Component[] com = handle.parentBone.GetComponentsInChildren(typeof(Component));

            this.Enabled = false;
            Super.Items.Items_Sub_RemoveAll();

            for (int i = 0; i < com.Length; i++)
            {               
                Type tCom = com[i].GetType();
#if DEBUG
                Debuginfo.Log("Type [" + tCom + "] >>  [" + (i + 1).ToString() + "] >>> " + com[i].name, 0);
#endif
                string guid = System.Guid.NewGuid().ToString();
                if (typeof(Transform) == tCom)  
                {
                    if (false == com[i].name.ToLower().EndsWith("nub")                //不必要
                            && false == com[i].name.StartsWith("_SM_")
                            && false == com[i].name.StartsWith("Arm")
                            && false == com[i].name.StartsWith("Hip_")
                            && false == com[i].name.StartsWith("Foretwist")
                            && false == com[i].name.StartsWith("Uppertwist")
                            && false == com[i].name.StartsWith("Kata_")
                            && false == com[i].name.StartsWith("Mune")
                            && false == com[i].name.StartsWith("momo")
                            && false == com[i].name.StartsWith("Reg")
                            && false == com[i].name.StartsWith("Hara")
                            && false == com[i].name.StartsWith("Skirt_P_")  //DynamicSkirtBone BoneHair3.DynamicUpdate()なエラーの回避
                            //&& false == com[i].name.Contains("_yure_")
                            && false == com[i].name.Contains("_SCL_")
                            && false == com[i].name.Contains("Bip")
                            && false == com[i].name.Contains("_HIDE_")     
                            && false == com[i].name.ToLower().Contains("twist")           //不必要 
                            && false == com[i].name.ToLower().Contains("_pos")            //不必要                            
                            && false == com[i].name.ToLower().EndsWith("_end")  //不必要
                            && false == com[i].name.EndsWith("_DO_NOT_ENUM_")   //不必要
                            //&& false == com[i].name.Equals("base")              //不必要
                            //&& false == com[i].name.Equals("center")
                            //&& false == com[i].name.Equals("center2")
                            //&& false == com[i].name.Equals(handle.parentBone.name)
                            )
                    {
                        if (true == com[i].name.Contains("_yure_skirt"))    //制御不能なボーンを非表示
                        {
                            if (true == com[i].name.Contains("_A_"))
                                Super.Items.Items_Sub_CreateHandle(guid + "_" + com[i].name, "[" + (i + 1).ToString() + "]T>" + com[i].name, "SubItemHandle", com[i].gameObject);
                        }
                        else
                        {
                            Super.Items.Items_Sub_CreateHandle(guid + "_" + com[i].name, "[" + (i + 1).ToString() + "]T>" + com[i].name, "SubItemHandle", com[i].gameObject);
                        }
                    }
                }
                else if (true == tCom.ToString().StartsWith("Dynamic"))
                {
                    if (typeof(DynamicBone) == tCom)    //髪、尻尾．．．
                        Super.Items.Items_Sub_CreateHandle(guid + "_" + com[i].name, "[" + (i + 1).ToString() + "]D>" + com[i].name, "SubItemHandle", com[i].gameObject);
                }
                else //MeshRenderer  Renderer
                {
                    Renderer render = com[i].gameObject.GetComponent<Renderer>();
                    if (null != render)
                    {
                        Super.Items.Items_Sub_CreateHandle(guid + "_" + com[i].name, "[" + (i + 1).ToString() + "]S>" + com[i].name, "SubItemHandle", com[i].gameObject);
                    }
                }
            }

            if (Super.Items.Items_Sub_ItemHandle.Count > 0)
            {
                ret = true;
                Items_SubObjects_listBox.DataSource = new BindingSource(Super.Items.Items_Sub_ItemHandle, null);
                Items_SubObjects_listBox.DisplayMember = "Value";
                Items_SubObjects_listBox.ValueMember = "Key";
            }
            else
            {
                Items_SubObjects_listBox.DataSource = null;
                Items_SubObjects_listBox.SelectedIndex = -1;
                Items_SubObjects_listBox.ClearSelected();
            }
            this.Enabled = true;
#if DEBUG
            Debuginfo.Log("Load sub objects.... ret = " + ret + "   " + DateTime.Now, 0);
#endif
            return ret;
        }

        private void SetSubItemOptions(GameObject goComponent, bool receiveShadows)
        {
            Renderer render = goComponent.GetComponent<Renderer>();
            if (null != render)
                render.receiveShadows = receiveShadows;
        }

        private void SetSubItemOptions(GameObject goComponent, ShadowCastingMode shadowCastingMode)
        {
            Renderer render = goComponent.GetComponent<Renderer>();
            if (null != render)
                render.shadowCastingMode = shadowCastingMode;
        }

        private void SetSubItemOptions(GameObject goComponent, string sShaderName)
        {
            if (true == string.Equals("無", sShaderName))
            {
                return;
            }

            Renderer render = goComponent.GetComponent<Renderer>();
            if (null != render)
            {
                Shader shader = Shader.Find(sShaderName);
                if (null != shader)
                {
                    render.material.shader = shader;
                }
            }
        }

        private void MaidOffset_CopyMaidOffset(Maid maid)
        {
            if (null == MaidOffset_BackupMaidPosRotScale)
            {
                MaidOffset_BackupMaidPosRotScale = new BackupMaidPosRotScale();
            }

            MaidOffset_BackupMaidPosRotScale.Pos = maid.GetPos();
            MaidOffset_BackupMaidPosRotScale.Rot = maid.GetRot();
            MaidOffset_BackupMaidPosRotScale.Scale = maid.body0.transform.localScale;
            Super.Pose.Pose_Copy(CurrentSelectedMaid);
            Super.maidTails.CopyBonesInfoAll();
        }

        private void MaidOffset_PasteMaidOffset(Maid maid)
        {
            if (null == MaidOffset_BackupMaidPosRotScale)
            {
                MaidOffset_BackupMaidPosRotScale = new BackupMaidPosRotScale()
                {
                    Pos = new Vector3(0, 0, 0),
                    Rot = new Vector3(0, 0, 0),
                    Scale = new Vector3(1, 1, 1)
                };
            }

            if (true == MaidOffset_Pos_checkBox.Checked)
            {
                maid.SetPos(MaidOffset_BackupMaidPosRotScale.Pos);
            }

            if (true == MaidOffset_Rot_checkBox.Checked)
            {
                maid.SetRot(MaidOffset_BackupMaidPosRotScale.Rot);
            }

            if (true == MaidOffset_Scale_checkBox.Checked)
            {
                maid.body0.transform.localScale = MaidOffset_BackupMaidPosRotScale.Scale;
            }

            if (true == MaidOffset_Pose_checkBox.Checked)
            {
                Super.Pose.Pose_Paste(CurrentSelectedMaid);
                Super.maidTails.PasteBonesInfoAll();
            }
        }

        private void SetSlotBodyHit(bool bTrigger)
        {
            if (true == IsBusy())
                return;

            string sSlotName = Items_HandledObjects_listBox.SelectedItem.ToString().Split('_')[1].Replace("]", "");
            if (true == sSlotName.Contains("hair"))
            {
                string[] sSlotID = Enum.GetNames(typeof(TBody.SlotID));
                if (true == sSlotID.Contains(sSlotName))
                {
                    int iSlotID = sSlotID.ToList().IndexOf(sSlotName);
                    TBodySkin tbs = CurrentSelectedMaid.body0.GetSlot(iSlotID);
                    if (null != tbs)
                    {
                        EMES.SetFieldValue<TBoneHair_, bool>(tbs.bonehair, "m_bEnable", bTrigger);
                    }
                }
            }
        }

        private void GetSlotBodyHit(CheckBox cb)
        {
            string sSlotName = Items_HandledObjects_listBox.SelectedItem.ToString().Split('_')[1].Replace("]", "");
            if (true == sSlotName.Contains("hair"))
            {
                string[] sSlotID = Enum.GetNames(typeof(TBody.SlotID));
                if (true == sSlotID.Contains(sSlotName))
                {
                    int iSlotID = sSlotID.ToList().IndexOf(sSlotName);
                    TBodySkin tbs = CurrentSelectedMaid.body0.GetSlot(iSlotID);
                    if (null != tbs)
                    {
                        cb.Visible = true;
                        cb.Checked = EMES.GetFieldValue<TBoneHair_, bool>(tbs.bonehair, "m_bEnable");
                    }
                }
            }
            else
            {
                cb.Visible = false;
                cb.Checked = false;
            }
        }
        #endregion
    }
}

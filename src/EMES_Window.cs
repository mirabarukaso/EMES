using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = System.Random;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public partial class EMES_Window : Form
    {
        #region InputBox Class
        public static class InputBox
        {
            public static string ShowDialog(string text, string title)
            {
                return ShowDialog(text, title, string.Empty);
            }

            public static string ShowDialog(string text, string title, string def_text)
            {
                Form prompt = new Form()
                {
                    Width = 264,
                    Height = 144,
                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
                    Text = title,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label()
                { Left = 8, Top = 12, Width = 240, Text = text };
                TextBox textBox = new TextBox()
                { Left = 8, Top = 40, Width = 240, TabIndex = 0, TabStop = true, Text = def_text };
                Button ok = new Button()
                { Text = "はい", Left = 8, Width = 80, Top = 72, TabIndex = 1, TabStop = true };
                Button cancel = new Button()
                { Text = "いいえ", Left = 168, Width = 80, Top = 72, TabIndex = 2, TabStop = true };

                ok.Click += (sender, e) => { prompt.DialogResult = DialogResult.OK; prompt.Close(); };
                cancel.Click += (sender, e) => { prompt.DialogResult = DialogResult.Cancel; prompt.Close(); };
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.TopMost = true;
                prompt.ImeMode = ImeMode.Inherit;

                if (DialogResult.OK == prompt.ShowDialog())
                {
                    if (textBox.Text.Length == 0)
                    {
                        Random rnd = new Random(DateTime.Now.Millisecond);
                        string Name = "カスタム" + rnd.Next(10000);
                        return Name;
                    }
                    return textBox.Text.Clone().ToString();
                }

                return "";
            }
        }
        #endregion

        #region FirstRun
        private bool bFirstRun = false;
        private void EMES_Window_Shown(object sender, EventArgs e)
        {
            if(false == bFirstRun)
            {
                bFirstRun = true;

                LoadTexToPictureBox(Super.Pose.Pose_DataList[Super.Pose.Pose_firstCatagory][0].iconTexName, MaidPose_Icon_pictureBox);
                KeyValuePair<int, string> item = (KeyValuePair<int, string>)MyRoomCustomObjects_Items_comboBox.Items[0];
                UpdatePictureBox(Super.Items.myCustomRoomObject.basicDatas[item.Key].GetThumbnail(), SS_pictureBox);
            }
        }
        #endregion

        #region VS Auto Private methods
        private void EMES_Window_Load(object sender, EventArgs e)
        {
#if COM3D25
            Text = EMES.PluginName + " " + EMES.PluginVersion + " for COM3D2.5";
#else
            Text = EMES.PluginName + " " + EMES.PluginVersion +" for COM3D2";
#endif
            AutoScaleMode = AutoScaleMode.Dpi;

            PrefabItem_Count_Label.Text = "ハンドル数： 0";
            MaidPose_Icon_pictureBox.Image = MaidPose_Icon_pictureBox.InitialImage;
            SS_pictureBox.Image = SS_pictureBox.InitialImage;
            BackGroundPictureBox.Image = BackGroundPictureBox.InitialImage;
            subItems_list_tabPage2.Enabled = false;

#if DEBUG
            Debug_groupBox.Visible = true;
#else
            Debug_groupBox.Visible = false;
#endif

#if COM3D25
#else
            Yotogi_CrcTranslate_checkBox.Visible = false;
            Yotogi_CrcTranslate_checkBox.Checked = false;
#endif
        }

        private void EMES_Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            bShowWindow = false;
        }

        private void RevertCurrentMaidList_Click(object sender, EventArgs e)
        {
            int Index = 0;
            foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList())
            {
                string MaidName = maid.status.lastName + "\n" + maid.status.firstName;
                if (true == maid.Visible && true == maid.isActiveAndEnabled)
                {
                    Maid CurrentMaid = GameMain.Instance.CharacterMgr.GetMaid(0);
#if DEBUG
                    Debuginfo.Log("Found " + maid.status.lastName + " " + maid.status.firstName + " at Index = " + Index.ToString(), 2);
                    Debuginfo.Log("CurrentMaid " + CurrentMaid.status.lastName + " " + CurrentMaid.status.firstName, 2);
#endif
                    MaidListView.Items[Index].Selected = true;
                }
                else
                {
                    MaidListView.Items[Index].Selected = false;
                }
                Index++;
            }

            if (false == MaidListView.Items[OrignalSelectedMaidStockID].Selected)
            {
                MaidListView.Items[OrignalSelectedMaidStockID].Selected = true;
            }

            MaidListView.Select();
        }

        private void LoadSelectedMaids_Click(object sender, EventArgs e)
        {
            int ActiveSlot = 0;
            GameMain.Instance.MainCamera.FadeOut(0f, false, null, true, default(UnityEngine.Color));
            ToggleWindow(false);
            Dance_WindowInit(true);

            if (true == MT.bIsAutoIK)
            {
                MaidTails_DisableAutoIK();
            }

            for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
            {
                Super.MaidIK.IK_DetachAll(CurrentMaidsStockID[Index]);
                Super.MaidIK.IK_SetAllHandleVisible(CurrentMaidsStockID[Index], false);
            }

            CurrentMaidListView.BeginUpdate();
            CurrentMaidImageList.Images.Clear();
            CurrentMaidListView.Clear();
            CurrentMaidsStockID.Clear();

            CurrentMaidsStockID.Add(OrignalSelectedMaidStockID);
            CurrentMaidImageList.Images.Add(ActiveSlot.ToString(), MaidImageList.Images[OrignalSelectedMaidStockID]);
            CurrentMaidListView.Items.Add(new ListViewItem
            {
                ImageKey = ActiveSlot.ToString(),
                Name = ActiveSlot.ToString() + "|" + OrignalSelectedMaidStockID.ToString(),
                Text = MaidListView.Items[OrignalSelectedMaidStockID].Text
            });
            SetSceneEditMaid(ActiveSlot, MaidSwitch_KeepCameraPosRot_checkBox.Checked);
            SetupSelectedMaid(ActiveSlot);

#if DEBUG
            Debuginfo.Log("Add " + MaidListView.Items[OrignalSelectedMaidStockID].Text.Replace("\n", " ") + " at " + ActiveSlot.ToString(), 2);
#endif
            ActiveSlot++;

            for (int Index = 0; Index < MaidListView.Items.Count; Index++)
            {
                if (OrignalSelectedMaidStockID == Index)
                {
                    continue;
                }

                if(true == MaidListView.Items[Index].Selected)
                {
                    CurrentMaidsStockID.Add(Index);
                    CurrentMaidImageList.Images.Add(ActiveSlot.ToString(), MaidImageList.Images[Index]);
                    CurrentMaidListView.Items.Add(new ListViewItem
                    {
                        ImageKey = ActiveSlot.ToString(),
                        Name = ActiveSlot.ToString() + "|" + Index.ToString(),
                        Text = MaidListView.Items[Index].Text
                    });

                    if (ActiveSlot > 17)
                    {
                        Debuginfo.Warning("最大メイド数は18です", 1);
                        Debuginfo.Warning("private Maid[] m_gcActiveMaid = new Maid[18]", 1);
                    }
                    else
                    {
#if DEBUG
                        Debuginfo.Log("Add " + MaidListView.Items[Index].Text + " at "+ ActiveSlot.ToString(), 2);
#endif
                        ActiveSlot++;
                    }
                }
            }
            CurrentMaidListView.EndUpdate();

            MaidListView.Items[OrignalSelectedMaidStockID].Selected = true;
            MaidListView.Select();
            Super.RequestLoadMaids();
        }

        private void MaidListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(false == MaidListView.Items[OrignalSelectedMaidStockID].Selected)
            {
                MaidListView.Items[OrignalSelectedMaidStockID].Selected = true;
            }
        }

        private void CurrentMaidListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void AllMaidsRadioButton_Click(object sender, EventArgs e)
        {
            CurrentSelectedMaidsRadioButton.Checked = false;
            MaidListView.Visible = true;
            MaidListView.Visible = true;
            CurrentMaidListView.Visible = false;
            RevertCurrentMaidList.Visible = true;
            LoadSelectedMaids.Visible = true;
        }

        private void CurrentSelectedMaidsRadioButton_Click(object sender, EventArgs e)
        {
            AllMaidsRadioButton.Checked = false;
            MaidListView.Visible = false;
            CurrentMaidListView.Visible = true;
            RevertCurrentMaidList.Visible = false;
            LoadSelectedMaids.Visible = false;
        }

        private void CurrentMaidListView_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem Item = CurrentMaidListView.SelectedItems[0];
            int MaidIndex = int.Parse(Item.Name.Split('|')[0]);
            SetSceneEditMaid(MaidIndex, MaidSwitch_KeepCameraPosRot_checkBox.Checked);
            SetupSelectedMaid(MaidIndex);
        }

        private void RemoveImportedBackGroundCheckBox_Click(object sender, EventArgs e)
        {
            LockBusy();
            DisableBackGroundCheckBox.Checked = false;
            Super.Items.Items_RemoveCategory("舶来背景");
            Super.Items.bAssertBgLoaded = false;
            UnlockBusy();

            Items_UpdateCurrentHandleCount();
        }

        private void BackGroundComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Index = BackGroundComboBox.SelectedIndex;
            string senderCategory = Super.Items.Items_BGDataList[Index].category;
            if (true == senderCategory.Equals("舶来背景"))
            {
                string bgName = Super.Items.Items_BGDataList[Index].name;
                if (true == Super.Items.Items_perfabItems.Items["舶来背景"].ContainsKey(bgName))
                {
                    LockBusy();
                    if (false == Prefab_Category_comboBox.SelectedText.Equals("舶来背景"))
                    {
                        Prefab_Category_comboBox.SelectedIndex = (int)EMES_Items.PerfabItems.Category.舶来背景;
                        Prefab_Category_comboBox.Select();
                        Prefab_Items_comboBox.DataSource = new BindingSource(Super.Items.Items_perfabItems.Items["舶来背景"], null);
                    }
                    Prefab_Items_comboBox.SelectedIndex = Index - Super.Items.AssertBgStartIndex;
                    Prefab_Items_comboBox.Select();
                    GameMain.Instance.BgMgr.BgObject.SetActive(false);
                    UnlockBusy();
                    DisableBackGroundCheckBox.Checked = true;
                    PrefabItem_Button_Click(PrefabItem_Add_Button, EventArgs.Empty);
                    Super.Items.bAssertBgLoaded = true;
                    BackGroundComboBox.Select();

                    KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu> selectedItem = (KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu>)Prefab_Items_comboBox.SelectedItem;
                    EMES_Items.PhotoBGObjectData_Odogu selectedItemValue = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key];
                    GameObject goItem = Super.Items.Items_CreatShadow("Prefab/", Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].assert_bg, new Vector3(0,0,0), new Vector3(0,0,0), selectedItemValue, null);
                    CreatePreviewIcon(goItem, BackGroundPictureBox, true);
                }
            }
            else
            {
                if (true == Super.Items.bAssertBgLoaded && "舶来背景" == Super.Items.Items_BGDataList[Index].category)
                {
                    RemoveImportedBackGroundCheckBox_Click(sender, e);
                }

                if (true == Super.Items.Items_BGDataList[Index].category.Equals("マイルーム"))
                {
                    GameMain.Instance.BgMgr.ChangeBgMyRoom(Super.Items.Items_BGDataList[Index].id);
                }
                else
                {
                    GameMain.Instance.BgMgr.ChangeBg(Super.Items.Items_BGDataList[Index].create_prefab_name);
                }


                if (true == Super.Items.Items_BGDataList[Index].category.Equals("編"))
                {
#if DEBUG
                    Debuginfo.Log("BG Icon = " + Super.Items.Items_BGDataList[Index].icon, 2);
#endif
                    LoadTexToPictureBox(Super.Items.Items_BGDataList[Index].icon, BackGroundPictureBox);
                }
                else
                {
#if DEBUG
                    Debuginfo.Log("BgMgr Icon = " + GameMain.Instance.BgMgr.GetBGName(), 2);
#endif
                    CreatePreviewIcon(GameMain.Instance.BgMgr.current_bg_object, BackGroundPictureBox, false);
                }

                Super.Items.Items_RemoveCategory("RoomAsItem");
                Super.Items.Items_CreatHandle(Super.Items.Items_BGDataList[Index].id, Super.Items.Items_BGDataList[Index].name, "RoomAsItem", GameMain.Instance.BgMgr.BgObject);
                Items_UpdateCurrentHandleCount();
            }
        }

        private void MaidPose_CenterMaid_button_Click(object sender, EventArgs e)
        {
            MaidPosAutoCenter();
        }

        private void MaidPose_Pause_Button_Click(object sender, EventArgs e)
        {
            CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Stop();
        }

        private void OfficialMaidPoseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Category = MaidPose_Category_ComboBox.SelectedItem.ToString();
            
            MaidPose_List_ComboBox.DataSource = new BindingSource(Super.Pose.Pose_DataList[Category], null);

            if ("カスタムポーズ" == Category)
            {
                MaidPose_UpdateIcon_Button.Enabled = true;
                MaidPose_ExportIcon_Button.Enabled = true;
            }
            else
            {
                MaidPose_UpdateIcon_Button.Enabled = false;
                MaidPose_ExportIcon_Button.Enabled = false;
            }
        }

        private void AnmMaidPoseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string Category = MaidPose_Category_ComboBox.SelectedItem.ToString();
            if ("カスタムポーズ" == Category)
            {
                LoadTexFileToPictureBox(Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].iconTexName, MaidPose_Icon_pictureBox);
            }
            else
            {
                LoadTexToPictureBox(Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].iconTexName, MaidPose_Icon_pictureBox);
            }

            if (true == IsBusy())
                return;

            if (true == MaidPose_AutoPlay_checkBox.Checked)
            {
                MaidPose_Play_Button_Click(sender, e);
            }
        }

        private void MaidPose_AutoPlay_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            MaidPose_Play_Button.Enabled = !MaidPose_AutoPlay_checkBox.Checked;
        }

        private void MaidHandleSelectModle_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            LockBusy();
            if (true == MaidHandleSelectModle_All_radioButton.Checked)
            {
                MaidHandleSelectModle_Current_radioButton.Checked = false;
                MaidHandleSelectModle_Others_radioButton.Checked = false;
                MaidHandleSelectModle_None_radioButton.Checked = false;
            }
            else if (true == MaidHandleSelectModle_Current_radioButton.Checked)
            {
                MaidHandleSelectModle_All_radioButton.Checked = false;
                MaidHandleSelectModle_Others_radioButton.Checked = false;
                MaidHandleSelectModle_None_radioButton.Checked = false;
            }
            else if (true == MaidHandleSelectModle_Others_radioButton.Checked)
            {
                MaidHandleSelectModle_All_radioButton.Checked = false;
                MaidHandleSelectModle_Current_radioButton.Checked = false;
                MaidHandleSelectModle_None_radioButton.Checked = false;
            }
            else
            {
                MaidHandleSelectModle_All_radioButton.Checked = false;
                MaidHandleSelectModle_Current_radioButton.Checked = false;
                MaidHandleSelectModle_Others_radioButton.Checked = false;
            }
            UnlockBusy();
        }

        private void MaidPose_Play_Button_Click(object sender, EventArgs e)
        {
            string Category = MaidPose_Category_ComboBox.SelectedItem.ToString();
            if ("カスタムポーズ" == Category)
            {
                Super.PerformPose(CurrentSelectedMaid, Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].name + ".anm");
            }
            else if (true == Category.Contains("男"))
            {
                string ANM = Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].direct_file;
#if DEBUG
                Debuginfo.Log("Yotogi_DecodeANM " + ANM, 2);
#endif
                using (AFileBase afileBase = GameUty.FileSystem.FileOpen(ANM))
                {
                    using (MemoryStream streamANM = new MemoryStream(afileBase.ReadAll()))
                    {
                        Super.Yotogi.Yotogi_LoadAnime(CurrentSelectedMaid, ANM, Super.Yotogi.Yotogi_ExportANM(Super.Yotogi.Yotogi_RecompileANM(Super.Yotogi.Yotogi_DecodeANM(streamANM))), false, MaidPoseLoopAnimeCheckBox.Checked);
                        CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Play(ANM);
                    }
                }
            }
            else
            {
                Super.PerformPose(CurrentSelectedMaid, Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex]);
            }
        }

        private void MaidPose_UpdateIcon_Button_Click(object sender, EventArgs e)
        {
            string Category = MaidPose_Category_ComboBox.SelectedItem.ToString();
            string ANMDirectory = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory;
            string ANMName = Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].name;

            if ("" == ANMName)
                return;

            string FullPathTexFile = ANMDirectory + ANMName + "_icon.tex";
            if(true == File.Exists(FullPathTexFile))
            {
                Debuginfo.Warning("既存のファイルを削除します " + FullPathTexFile, 1);
                File.Delete(FullPathTexFile);
            }
            MaidPoseCreateIcon(ANMName, FullPathTexFile);

            LoadTexFileToPictureBox(Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].iconTexName, MaidPose_Icon_pictureBox);
        }

        private void MaidPose_ExportIcon_Button_Click(object sender, EventArgs e)
        {
            string Category = MaidPose_Category_ComboBox.SelectedItem.ToString();
            string ANMDirectory = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory;
            string ANMName = Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].name;

            if ("" == ANMName)
                return;

            string FullPathTexFile = ANMDirectory + ANMName + "_icon.tex";
            if (true == File.Exists(FullPathTexFile))
            {
#if DEBUG
                Debuginfo.Log("FullPathTexFile =" + FullPathTexFile, 2);
#endif
                string FullPathPngFile = ANMDirectory + ANMName + "_icon.png";
                if (true == File.Exists(FullPathPngFile))
                {
                    Debuginfo.Warning("既存のファイルを削除します " + FullPathPngFile, 1);
                    File.Delete(FullPathPngFile);
                }

                byte[] bytePNG = TexPngTools.TexCM3D2ToPngFromFile(FullPathTexFile);
                File.WriteAllBytes(FullPathPngFile, bytePNG);

                Debuginfo.Log("エクスポートしました " + FullPathPngFile, 1);
            }
            else
            {
                MaidPoseCreateIcon(ANMName, FullPathTexFile);
                LoadTexFileToPictureBox(Super.Pose.Pose_DataList[Category][MaidPose_List_ComboBox.SelectedIndex].iconTexName, MaidPose_Icon_pictureBox);
            }
        }

        private void BackGroundColorPictureBox_Click(object sender, EventArgs e)
        {
            ignoreShowWindow = true;
            BackGroundColorDialog.ShowDialog();
            ignoreShowWindow = false;

            BackGroundColorPictureBox.BackColor = BackGroundColorDialog.Color;
            GameMain.Instance.MainCamera.GetComponent<Camera>().backgroundColor = new UnityEngine.Color((float)BackGroundColorDialog.Color.R / 255f, (float)BackGroundColorDialog.Color.G / 255f, (float)BackGroundColorDialog.Color.B / 255f);
            GameMain.Instance.BgMgr.BgObject.SetActive(!DisableBackGroundCheckBox.Checked);
        }

        private void DisableBackGroundCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            GameMain.Instance.BgMgr.BgObject.SetActive(!DisableBackGroundCheckBox.Checked);
        }

        private void MaidPoseLoopAnimeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(true == MaidPoseLoopAnimeCheckBox.Checked)
            {
                switch(lastPerformedPose)
                {
                    case LastPerformedPose.Official:
                        Super.PerformPose(CurrentSelectedMaid, Super.Pose.Pose_DataList[MaidPose_Category_ComboBox.SelectedItem.ToString()][MaidPose_List_ComboBox.SelectedIndex]);
                        break;
                    case LastPerformedPose.ANM:
                        Super.PerformPose(CurrentSelectedMaid, Super.Pose.Pose_DataList[MaidPose_Category_ComboBox.SelectedItem.ToString()][MaidPose_List_ComboBox.SelectedIndex].direct_file);
                        break;
                    case LastPerformedPose.Yotogi:
#if COM3D25
                        Super.PerformPose(CurrentSelectedMaid, CurrentSelectedMaid.body0.IsCrcBody ? "crc_" : "" + Yotogi_Item_comboBox.SelectedItem.ToString() + ".anm");
#else
                        Super.PerformPose(CurrentSelectedMaid, Yotogi_Item_comboBox.SelectedItem.ToString() + ".anm");
#endif
                        break;
                }
            }
            else
            {
                CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Stop();
            }
        }

        private void PreviousMaidButton_Click(object sender, EventArgs e)
        {
            int MaidCount = CurrentMaidsStockID.Count;
            if (CurrentSelectedMaidIndex > 0)
            {
                CurrentSelectedMaidIndex--;
            }
            else
            {
                CurrentSelectedMaidIndex = MaidCount - 1;
            }
            SetSceneEditMaid(CurrentSelectedMaidIndex, MaidSwitch_KeepCameraPosRot_checkBox.Checked);
            SetupSelectedMaid(CurrentSelectedMaidIndex);
        }

        private void NextMaidButton_Click(object sender, EventArgs e)
        {
            int MaidCount = CurrentMaidsStockID.Count;
            if (CurrentSelectedMaidIndex < MaidCount - 1)
            {
                CurrentSelectedMaidIndex++;
            }
            else
            {
                CurrentSelectedMaidIndex = 0;
            }
            SetSceneEditMaid(CurrentSelectedMaidIndex, MaidSwitch_KeepCameraPosRot_checkBox.Checked);
            SetupSelectedMaid(CurrentSelectedMaidIndex);
        }

        private void CurrentMaidFaceBlendComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string BlendSetName = CurrentMaidFaceBlendComboBox.SelectedItem.ToString();

            if (null != FaceBlendTimer)
            {
#if DEBUG
                Debuginfo.Log("フェイスブレンド同期停止 " + BlendSetName + "  bSyncFaceBlendTrackBar = " + bSyncFaceBlendTrackBar, 2);
#endif
                FaceBlendTimer.Dispose();
                FaceBlendTimer = null;
            }

            if (false == bSyncFaceBlendTrackBar)
            {
                bSyncFaceBlendTrackBar = true;
                CurrentSelectedMaid.FaceAnime(BlendSetName, 1.0f, 0);
                FaceBlendTimer = new System.Threading.Timer((obj) =>
                            {
                                SyneFaceBlendToTrackBar();
                                FaceBlendTimer.Dispose();
                                FaceBlendTimer = null;
                                bSyncFaceBlendTrackBar = false;
                            },
                            null, 1550, System.Threading.Timeout.Infinite);
            }
            else
            {
                CurrentSelectedMaid.FaceAnime(BlendSetName, 0.0f, 0);
                FaceBlendTimer = new System.Threading.Timer((obj) =>
                            {
                                SyneFaceBlendToTrackBar();
                                FaceBlendTimer.Dispose();
                                FaceBlendTimer = null;
                                bSyncFaceBlendTrackBar = false;
                            },
                            null, 100, System.Threading.Timeout.Infinite);
            }
        }

        private void FaceBlend_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Dictionary<string, float> faceBlend = GetFaceBlendValue();
            foreach (KeyValuePair<string, float> fb in faceBlend)
            {
                float v = ((float)fb.Value) / 10;
                CurrentSelectedMaid.body0.Face.morph.SetValueBlendSet("カスタム", fb.Key, v);
            }
            CurrentSelectedMaid.FaceAnime("カスタム", 1, 0);
            //SetSelectFaceBlend("カスタム");
        }

        private void MuneYureCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            CurrentSelectedMaid.body0.jbMuneL.enabled = GetMuneYureCheckBox();
            CurrentSelectedMaid.body0.jbMuneR.enabled = GetMuneYureCheckBox();
        }

        private void DeleteFaceBlendButton_Click(object sender, EventArgs e)
        {
            string BlendSetName = CurrentMaidFaceBlendComboBox.SelectedItem.ToString();

            if(false == BlendSetName.Contains(".cfb"))
            {
                return;
            }

            string FaceBlendDirectory = Directory.GetCurrentDirectory() + @"\Mod\MultipleMaidsPose\";
            string FullName = FaceBlendDirectory + BlendSetName;
            if (true == File.Exists(FullName))
            {
                Debuginfo.Log("CFB削除: " + FullName, 1);
                File.Delete(FullName);

                CurrentSelectedMaid.FaceAnime("通常", 1, 0);
                SetSelectFaceBlend("通常");
                DeleteFaceBlend(BlendSetName);
            }
        }

        private void SaveFaceBlendButton_Click(object sender, EventArgs e)
        {
            string FaceBlendDirectory = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory;
            string FaceBlendName;

            ignoreShowWindow = true;
            SaveFaceBlendButton.Enabled = false;
            string FaceBlendFile = InputBox.ShowDialog("名前を入力してください", "カスタムフェイスブレンド");
            SaveFaceBlendButton.Enabled = true;
            ignoreShowWindow = false;

            if ("" == FaceBlendFile)
                return;
            Debuginfo.Log("FaceBlendFile=" + FaceBlendFile, 1);

            string CustomFaceBlendFile = FaceBlendDirectory + FaceBlendFile + ".cfb";
            FaceBlendName = FaceBlendFile + ".cfb";
            int Index = 1;
            while (true == File.Exists(CustomFaceBlendFile))
            {
                CustomFaceBlendFile = FaceBlendDirectory + FaceBlendFile + "(" + Index + ").cfb";
                FaceBlendName = FaceBlendFile + "(" + Index + ").cfb";
                Index++;
            }

            if (false == CurrentMaidFaceBlendComboBox.Items.Contains(FaceBlendName))
            {
                CurrentMaidFaceBlendComboBox.SelectedIndex = CurrentMaidFaceBlendComboBox.Items.Add(FaceBlendName);
            }
            else
            {
                CurrentMaidFaceBlendComboBox.SelectedIndex = CurrentMaidFaceBlendComboBox.Items.IndexOf(FaceBlendName);
            }

            string ExportFileName = CustomFaceBlendFile;
            Dictionary<string, float> faceBlend = CreateFaceBlend(CurrentSelectedMaid, "カスタム");

            MemoryStream CFB = EMES.SerializeToStream(faceBlend);
            byte[] CFB_Byte = CFB.ToArray();
            FileStream CFB_FS = new FileStream(ExportFileName, FileMode.OpenOrCreate);
            CFB_FS.Write(CFB_Byte, 0, CFB_Byte.Length);
            CFB_FS.Close();

            string[] RealBlendSetNameList = ExportFileName.Split('\\');
            string RealBlendSetName = RealBlendSetNameList[RealBlendSetNameList.Length - 1];
            CurrentSelectedMaid.body0.Face.morph.NewBlendSet(RealBlendSetName);
            foreach (KeyValuePair<string, float> fb in faceBlend)
            {
                CurrentSelectedMaid.body0.Face.morph.SetValueBlendSet(RealBlendSetName, fb.Key, fb.Value * 100);
            }
        }


        private void LookAtMeAllButton_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            for(int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                Super.MaidIK.IK_RemoveGazePoint(CurrentMaidsList[i], false);
            }

            Items_UpdateCurrentHandleCount();
        }    

        private void StopLookAtMeAllButton_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                Super.MaidIK.IK_AddGazePoint(CurrentMaidsList[i]);
            }

            Items_UpdateCurrentHandleCount();
        }

        private void IgnoreMeAllButton_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                Super.MaidIK.IK_RemoveGazePoint(CurrentMaidsList[i], true);
            }

            Items_UpdateCurrentHandleCount();
        }

        private void PanzMizugiZurashiCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            string[] key = new string[]{ "panz", "mizugi" };
            string[] value = new string[] { "パンツずらし", "パンツずらし" };
            SetItemChangeTemp(CurrentSelectedMaid, key, value, PanzMizugiZurashiCheckBox.Checked);
        }

        private void SkirtUpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            else if(true == SkirtBehindFlipCheckBox.Checked)
            {
                SkirtBehindFlipCheckBox.Checked = false;
            }
            string[] key = new string[] { "skirt", "onepiece" };
            string[] value = new string[] { "めくれスカート", "めくれスカート" };
            SetItemChangeTemp(CurrentSelectedMaid, key, value, SkirtUpCheckBox.Checked);
        }

        private void SkirtBehindFlipCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            else if (true == SkirtUpCheckBox.Checked)
            {
                SkirtUpCheckBox.Checked = false;
            }
            string[] key = new string[] { "skirt", "onepiece" };
            string[] value = new string[] { "めくれスカート後ろ", "めくれスカート後ろ" };
            SetItemChangeTemp(CurrentSelectedMaid, key, value, SkirtBehindFlipCheckBox.Checked);
        }

        private void Bip_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), ((CheckBox)sender).Name.Substring(6)), ((CheckBox)sender).Checked);
        }

        private void ResetSelectedBones_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            Super.Pose.Pose_ResetSelected(CurrentSelectedMaid);
        }

        private void BipSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            ToggleFingerToeComboBox(((CheckBox)sender).Name, ((CheckBox)sender).Checked, true);
            Super.MaidIK.IK_SetBoneSetTypeVisible(GetCurrentMaidStockID(), (EMES_MaidIK.BoneSetType)Enum.Parse(typeof(EMES_MaidIK.BoneSetType), ((CheckBox)sender).Name.Substring(6)), ((CheckBox)sender).Checked);
        }

        private void Bip01_PosRot_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            LockBusy();
            CheckBox checkBox = (CheckBox)sender;
            if(checkBox.Name.Contains("Position"))
            {
                if(true == Bip01_RotationCheckbox.Checked && true == checkBox.Checked)
                {
                    Bip01_RotationCheckbox.Checked = false;
                }
                HandleEx handle = Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Root, checkBox.Checked);
                if(null != handle && true == checkBox.Checked)
                {
                    handle.IK_ChangeHandleKunModePosition(true);
                }
            }
            else
            {
                if (true == Bip01_PositionCheckBox.Checked && true == checkBox.Checked)
                {
                    Bip01_PositionCheckBox.Checked = false;
                }
                HandleEx handle = Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Root, checkBox.Checked);
                if (null != handle && true == checkBox.Checked)
                {
                    handle.IK_ChangeHandleKunModePosition(false);
                }
            }
            UnlockBusy();
        }

        private void Offset_Pos_Rot_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            LockBusy();
            CheckBox checkBox = (CheckBox)sender;
            HandleSelectMode hsMode = GetMaidHandleSelectMode();
            int iID = GetCurrentMaidStockID();
            if (checkBox.Name.Contains("Position"))
            {
                if (true == Offset_RotationCheckbox.Checked && true == checkBox.Checked)
                {
                    Offset_RotationCheckbox.Checked = false;
                }
                if (HandleSelectMode.None != hsMode)
                {
                    for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                    {
                        if (false == CheckMaidSelectMode(hsMode, Index, iID))
                            continue;

                        HandleEx handle = Super.MaidIK.IK_SetBoneTypeVisible(CurrentMaidsStockID[Index], EMES_MaidIK.BoneType.Offset, checkBox.Checked);
                        if (null != handle && true == checkBox.Checked)
                        {
                            handle.IK_ChangeHandleKunModePosition(true);
                        }
                    }
                }
            }
            else
            {
                if (true == Offset_PositionCheckbox.Checked && true == checkBox.Checked)
                {
                    Offset_PositionCheckbox.Checked = false;
                }
                if (HandleSelectMode.None != hsMode)
                {
                    for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                    {
                        if (false == CheckMaidSelectMode(hsMode, Index, iID))
                            continue;

                        HandleEx handle = Super.MaidIK.IK_SetBoneTypeVisible(CurrentMaidsStockID[Index], EMES_MaidIK.BoneType.Offset, checkBox.Checked);
                        if (null != handle && true == checkBox.Checked)
                        {
                            handle.IK_ChangeHandleKunModePosition(false);
                        }
                    }
                }
            }
            UnlockBusy();
        }

        private void ExportCurrentFingerPose_Click(object sender, EventArgs e)
        {
            List<HandleEx> handlesLFinger = Super.MaidIK.IK_GetHandleByBoneSetTyoe(GetCurrentMaidStockID(), EMES_MaidIK.BoneSetType.L_Finger);
            List<HandleEx> handlesRFinger = Super.MaidIK.IK_GetHandleByBoneSetTyoe(GetCurrentMaidStockID(), EMES_MaidIK.BoneSetType.R_Finger);

            Debuginfo.Warning("左指をエクスポート", 0);
            string LF = "";
            float x, y, z;
            foreach (HandleEx handle in handlesLFinger)
            {
                FingerPose.Calc_trBone2Param(handle.GetParentBone(), out x, out y, out z);
                LF += x.ToString("0.0000") + "," + y.ToString("0.0000") + "," + z.ToString("0.0000") + ",";
            }
            Debuginfo.Log(LF, 0);

            Debuginfo.Warning("右指ミラーリングをエクスポート（オプショナル）", 0);
            string RF = "";
            foreach (HandleEx handle in handlesRFinger)
            {
                FingerPose.Calc_trBone2Param(handle.GetParentBone(), out x, out y, out z);
                RF += (x*-1).ToString("0.0000") + "," + (y*-1).ToString("0.0000") + "," + z.ToString("0.0000") + ",";
            }
            Debuginfo.Log(RF, 0);

            Debuginfo.Warning("エクスポートが完了しました", 0);
        }

        private void ArmIKCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            string Key = ((CheckBox)sender).Name.Contains('L') ? "L" : "R";
            if ("L" == Key)
            {
                Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKLA = ((CheckBox)sender).Checked;
            }
            else
            {
                Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKRA = ((CheckBox)sender).Checked;
            }
            if (true == ((CheckBox)sender).Checked)
            {
                TogglePoseComboBox(((CheckBox)sender).Name, false, false);

                Super.MaidIK.IK_Attach(GetCurrentMaidStockID(),
                                (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType),"Hand_"+ Key),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key+ "_UpperArm"), 
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Forearm"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Hand"), -1f);    //-1 == 自動

                Super.MaidIK.IK_Attach(GetCurrentMaidStockID(),
                                (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Forearm_" + Key),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_UpperArm"), 
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_UpperArm"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Forearm"), -1f);

                Super.MaidIK.IK_Attach(GetCurrentMaidStockID(),
                                (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Clavicle_" + Key),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Clavicle"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Clavicle"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_UpperArm"), -1f);
            }
            else
            {
                TogglePoseComboBox(((CheckBox)sender).Name, false, true);

                Super.MaidIK.IK_Deatch(GetCurrentMaidStockID(), (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Hand_" + Key));
                Super.MaidIK.IK_Deatch(GetCurrentMaidStockID(), (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Forearm_" + Key));
                Super.MaidIK.IK_Deatch(GetCurrentMaidStockID(), (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Clavicle_" + Key));

                SyncIKtoANM(CurrentSelectedMaid);
            }
        }

        private void LegIKCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            string Key = ((CheckBox)sender).Name.Contains('L') ? "L" : "R"; //誤解の場合 RightLegIKCheckBox =>> RightRegIKCheckBox
            if ("L" == Key)
            {
                Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKLL = ((CheckBox)sender).Checked;
            }
            else
            {
                Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].bIKRL = ((CheckBox)sender).Checked;
            }
            if (true == ((CheckBox)sender).Checked)
            {
                TogglePoseComboBox(((CheckBox)sender).Name, false, false);

                Super.MaidIK.IK_Attach(GetCurrentMaidStockID(), 
                                (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Foot_" + Key),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Thigh"), 
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Calf"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Foot"), 0.45f);

                Super.MaidIK.IK_Attach(GetCurrentMaidStockID(), 
                                (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Calf_" + Key),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), "Hip_" + Key), 
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Thigh"),
                                (EMES_MaidIK.BoneType)Enum.Parse(typeof(EMES_MaidIK.BoneType), Key + "_Calf"), 0.45f);
            }
            else
            {
                TogglePoseComboBox(((CheckBox)sender).Name, false, true);

                Super.MaidIK.IK_Deatch(GetCurrentMaidStockID(), (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Foot_" + Key));
                Super.MaidIK.IK_Deatch(GetCurrentMaidStockID(), (EMES_MaidIK.IKEffectorType)Enum.Parse(typeof(EMES_MaidIK.IKEffectorType), "Calf_" + Key));

                SyncIKtoANM(CurrentSelectedMaid);
            }
        }

        private void HandleSizeSliderTrackBar_Scroll(object sender, EventArgs e)
        {
            if(false == HandleSizeSliderCheckBox.Checked)
            {
                return;
            }

            float value = ((TrackBar)sender).Value / 10000f;
            Super.RequestScaleHandles(value);
        }

        private void HandleSizeSliderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            HandleSizeSliderTrackBar.Enabled = ((CheckBox)sender).Checked;
            Super.RequestScaleHandles(0f);
        }

        private void nosefook_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            CurrentSelectedMaid.boNoseFook = ((CheckBox)sender).Checked;
        }

        private void openMouth_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            CurrentSelectedMaid.OpenMouth(openMouth_checkBox.Checked);
        }

        private void openMouthLookTooth_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            CurrentSelectedMaid.OpenMouthLookTooth(openMouthLookTooth_checkBox.Checked);
        }

        private void HideAllMaidsHandle_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<int, EMES_MaidIK.EMES_IK> ik in Super.MaidIK.MaidsIK)
            {
                Super.MaidIK.IK_DetachAll(ik.Value.maidStockID);
                Dictionary<EMES_MaidIK.BoneType, HandleEx> handles = ik.Value.handleEx;
                foreach (KeyValuePair<EMES_MaidIK.BoneType, HandleEx> handle in handles)
                {
                    handle.Value.Visible = false;
                }
                ik.Value.bInvisible = true;
            }

            SyncAllHandleExCheckBox();
            MaidTails_DisableAutoIK();

            SyncIKtoANM(CurrentSelectedMaid);
        }

        private void SelectBodyBoneSetType_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            bool bToggle = ((Button)sender).Text.Contains("開");
            string senderName = ((Button)sender).Name;
            string Key = senderName.Contains('L') ? "L" : "R";  //誤解の場合 SelectAllRightLeg =>> SelectAllRightReg
            string IKKey = Key.Contains('L') ? "Left" : "Right";
            if (true == senderName.Contains("Arm"))
            {
                if (false == (Controls.Find(IKKey + "ArmIKCheckBox", true)[0] as CheckBox).Checked)
                {
                    (Controls.Find("Bip01_" + Key + "_UpperArm", true)[0] as CheckBox).Checked = bToggle;
                    (Controls.Find("Bip01_" + Key + "_Forearm", true)[0] as CheckBox).Checked = bToggle;
                    (Controls.Find("Bip01_" + Key + "_Hand", true)[0] as CheckBox).Checked = bToggle;
                }
                else
                {
                    Debuginfo.Log("IKモード中はチェックボックスを設定できません", 0);
                }
                (Controls.Find("Bip01_" + Key + "_Clavicle", true)[0] as CheckBox).Checked = bToggle;
                ((Button)sender).Text = bToggle ? "閉" : "開";
            }
            else if (true == senderName.Contains("eg"))
            {
                if (false == (Controls.Find(IKKey + Key + "egIKCheckBox", true)[0] as CheckBox).Checked) //RightRegIKCheckBox
                {
                    (Controls.Find("Bip01_" + Key + "_Thigh", true)[0] as CheckBox).Checked = bToggle;
                    (Controls.Find("Bip01_" + Key + "_Calf", true)[0] as CheckBox).Checked = bToggle;
                    (Controls.Find("Bip01_" + Key + "_Foot", true)[0] as CheckBox).Checked = bToggle;
                    ((Button)sender).Text = bToggle ? "閉" : "開";
                }
                else
                {
                    Debuginfo.Log("IKモード中はチェックボックスを設定できません", 0);
                }
            }
            else if (true == senderName.Contains("Spine"))
            {
                Bip01_Spine1a.Checked = bToggle;
                Bip01_Spine1.Checked = bToggle;
                Bip01_Spine0a.Checked = bToggle;
                Bip01_Spine.Checked = bToggle;
                ((Button)sender).Text = bToggle ? "閉" : "開";
            }
        }

        private void RightHandPoseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            if("無" == RightHandPoseComboBox.SelectedItem.ToString())
            {
                return;
            }

            //CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Stop();
            int data = 45 * RightHandPoseComboBox.SelectedIndex;
            List<HandleEx> handlesRFinger = Super.MaidIK.IK_GetHandleByBoneSetTyoe(GetCurrentMaidStockID(), EMES_MaidIK.BoneSetType.R_Finger);
            foreach (HandleEx handle in handlesRFinger)
            {
                //左手からミラーリング z -x -y
                handle.GetParentBone().transform.localRotation = Quaternion.identity;
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data + 2], Vector3.forward);
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data] * -1, Vector3.right);
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data + 1] * -1, Vector3.up);
                data += 3;
            }
        }

        private void LeftHandPoseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            if ("無" == LeftHandPoseComboBox.SelectedItem.ToString())
            {
                return;
            }

            //CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Stop();
            int data = 45 * LeftHandPoseComboBox.SelectedIndex;
            List<HandleEx> handlesLFinger = Super.MaidIK.IK_GetHandleByBoneSetTyoe(GetCurrentMaidStockID(), EMES_MaidIK.BoneSetType.L_Finger);
            foreach (HandleEx handle in handlesLFinger)
            {
                handle.GetParentBone().transform.localRotation = Quaternion.identity;
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data + 2], Vector3.forward);
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data], Vector3.right);
                handle.GetParentBone().transform.localRotation *= Quaternion.AngleAxis(FingerPose.Left[data + 1], Vector3.up);
                data += 3;
            }
        }

        private void Settings_GUIHotkeyApplyButton_Click(object sender, EventArgs e)
        {
            if (true == FlexKeycode.ExistKey(Settings_GUIHotkey_TextBox.Text.ToLower()))
            {
                Super.settingsXml.ToggleKey = Settings_GUIHotkey_TextBox.Text.ToLower();
            }
            else
            {
                SettingsXMLDefault settingsXmlDefault = new SettingsXMLDefault();
                Settings_GUIHotkey_TextBox.Text = settingsXmlDefault.ToggleKey;
                Super.settingsXml.ToggleKey = Settings_GUIHotkey_TextBox.Text.ToLower();
            }
            Super.SaveConfigurefile();
        }

        private void Settings_GUIHotkeyResetButton_Click(object sender, EventArgs e)
        {
            SettingsXMLDefault settingsXmlDefault = new SettingsXMLDefault();
            Settings_GUIHotkey_TextBox.Text = settingsXmlDefault.ToggleKey;
            Super.settingsXml.ToggleKey = Settings_GUIHotkey_TextBox.Text.ToLower();
            Super.SaveConfigurefile();
        }

        private void Settings_DebugLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Debuginfo.settingLevel = Settings_DebugLevel_ComboBox.SelectedIndex;
            Super.settingsXml.DebugLogLevel = Settings_DebugLevel_ComboBox.SelectedIndex;
            Super.SaveConfigurefile();

            Debuginfo.Warning("デバッグレベル＝"+ Settings_DebugLevel_ComboBox.SelectedIndex.ToString() + Settings_DebugLevel_ComboBox.SelectedItem.ToString(), 0);
#if DEBUG
#else
            if(2 == Super.settingsXml.DebugLogLevel)
                Debuginfo.Warning("これはリリースビルドです、デバッグ情報が正しく表示されない場合があります", 0);
#endif
        }

        private void Settings_Hotkey_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            bool bActive = ((CheckBox)sender).Checked;
            string senderName = ((CheckBox)sender).Name;
            string textboxName = senderName.Replace("_CheckBox", "_TextBox");
            string Key = senderName.Replace("_CheckBox", "").Replace("Settings_Hotkey", "");
            if (true == bActive)
            {
                if(false == ActivedSettingsHotkey.Contains(Key) && false == Key.Contains("Maid") 
                    && false == Key.Contains("Item") && false == Key.Contains("Dance")
                    && false == Key.Contains("Camera"))
                {
                    ActivedSettingsHotkey.Add(Key);
                }
                (Controls.Find(textboxName, true)[0] as TextBox).Enabled = false;
            }
            else
            {
                if (true == ActivedSettingsHotkey.Contains(Key))
                {
                    ActivedSettingsHotkey.Remove(Key);
                }
                (Controls.Find(textboxName, true)[0] as TextBox).Enabled = true;
            }

            Offset_PositionCheckbox.Enabled = !Settings_HotkeyMaidPos_CheckBox.Checked;
            Offset_RotationCheckbox.Enabled = !Settings_HotkeyMaidRot_CheckBox.Checked;

            foreach (string sKey in Enum.GetNames(typeof(Settings_HotkeyMaid)))
            {
                SetFieldValue(Super.settingsXml, "bHotkey" + sKey, (Controls.Find("Settings_Hotkey"+sKey+"_CheckBox", true)[0] as CheckBox).Checked);
            }

            foreach (string sKey in Enum.GetNames(typeof(Settings_HotkeyItem)))
            {
                SetFieldValue(Super.settingsXml, "bHotkey" + sKey, (Controls.Find("Settings_Hotkey" + sKey + "_CheckBox", true)[0] as CheckBox).Checked);
            }

            foreach (string sKey in Enum.GetNames(typeof(Settings_HotkeyDance)))
            {
                SetFieldValue(Super.settingsXml, "bHotkey" + sKey, (Controls.Find("Settings_Hotkey" + sKey + "_CheckBox", true)[0] as CheckBox).Checked);
            }
            Super.SaveConfigurefile();
        }

        private void Settings_HotkeyCameraScreenShotNoUI_CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            
            if (true == IsBusy())
                return;

            Super.settingsXml.bHotkeyCameraScreenShot = Settings_HotkeyCameraScreenShot_CheckBox.Checked;
            Super.settingsXml.bHotkeyCameraScreenShotNoUI = Settings_HotkeyCameraScreenShotNoUI_CheckBox.Checked;

            Settings_HotkeyCameraScreenShot_TextBox.Enabled = !Settings_HotkeyCameraScreenShot_CheckBox.Checked;

            Super.SaveConfigurefile();
        }

        private void Settings_HotkeyApply_Button_Click(object sender, EventArgs e)
        {
            Action<Type> action_ApplyHotkey = delegate(Type type)
            {
                SettingsXMLDefault settingsXmlDefault = new SettingsXMLDefault();
                foreach (string Key in Enum.GetNames(type))
                {
                    string tKey = (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text.ToLower();
                    if (true == FlexKeycode.ExistKey(tKey))
                    {
                        SetFieldValue(Super.settingsXml, "sHotkey" + Key, (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text);
                        (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = tKey;
                    }
                    else
                    {
                        SetFieldValue(Super.settingsXml, "sHotkey" + Key, GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key));
                        (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key);
                    }
                }
            };

            LockBusy();
            if (true == ((Button)sender).Name.Contains("Item"))
            {
                action_ApplyHotkey(typeof(Settings_HotkeyItem));
            }
            else if (true == ((Button)sender).Name.Contains("Dance"))
            {
                action_ApplyHotkey(typeof(Settings_HotkeyDance));
            }
            else if (true == ((Button)sender).Name.Contains("Camera"))
            {
                action_ApplyHotkey(typeof(Settings_HotkeyCamera));
                action_ApplyHotkey(typeof(Settings_HotkeyCameraMovement));
            }
    
            UnlockBusy();
            Super.SaveConfigurefile();
        }

        private void Settings_HotkeyReset_Button_Click(object sender, EventArgs e)
        {
            Action<Type> action_ResetHotkeyWithCheckBox = delegate (Type type)
            {
                SettingsXMLDefault settingsXmlDefault = new SettingsXMLDefault();
                foreach (string Key in Enum.GetNames(type))
                {
                    (Controls.Find("Settings_Hotkey" + Key + "_CheckBox", true)[0] as CheckBox).Checked = false;// GetFieldValue<SettingsXMLDefault, bool>(settingsXmlDefault, "bHotkey" + Key);
                    (Controls.Find("Settings_Hotkey" + Key + "_CheckBox", true)[0] as CheckBox).Enabled = true;
                    (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key);

                    SetFieldValue(Super.settingsXml, "sHotkey" + Key, GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key));
                    SetFieldValue(Super.settingsXml, "bHotkey" + Key, false);
                }
            };

            Action<Type> action_ResetHotkey = delegate (Type type)
            {
                SettingsXMLDefault settingsXmlDefault = new SettingsXMLDefault();
                foreach (string Key in Enum.GetNames(type))
                {
                    (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Text = GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key);

                    SetFieldValue(Super.settingsXml, "sHotkey" + Key, GetFieldValue<SettingsXMLDefault, string>(settingsXmlDefault, "sHotkey" + Key));
                }
            };

            LockBusy();
            if (true == ((Button)sender).Name.Contains("Item"))
            {
                action_ResetHotkeyWithCheckBox(typeof(Settings_HotkeyItem));
            }
            else if (true == ((Button)sender).Name.Contains("Dance"))
            {
                action_ResetHotkeyWithCheckBox(typeof(Settings_HotkeyDance));
            }
            else if (true == ((Button)sender).Name.Contains("Camera"))
            {
                action_ResetHotkeyWithCheckBox(typeof(Settings_HotkeyCamera));
                Settings_HotkeyCameraMovement_checkBox.Checked = false;
                action_ResetHotkey(typeof(Settings_HotkeyCameraMovement));
            }
            else
            {
                ActivedSettingsHotkey.Clear();
            }
            UnlockBusy();
            Super.SaveConfigurefile();
        }

        private void SaveCurrentPoseButton_Click(object sender, EventArgs e)
        {
            string ANMDirectory = Directory.GetCurrentDirectory() + Super.settingsXml.ANMFilesDirectory;
            string ANMName;
            ignoreShowWindow = true;
            SaveCurrentPoseButton.Enabled = false;
            string PoseName = InputBox.ShowDialog("名前を入力してください", "ANMポーズを保存");
            SaveCurrentPoseButton.Enabled = true;
            ignoreShowWindow = false;

            if ("" == PoseName)
                return;
            Debuginfo.Log("ANMPoseName=" + PoseName, 1);

            string FullPathANMFile = ANMDirectory + PoseName + ".anm";
            string FullPathPngFile = ANMDirectory + PoseName + "_icon.tex";
            ANMName = PoseName;
            int Index = 1;
            while (true == File.Exists(FullPathANMFile))
            {
                FullPathANMFile = ANMDirectory + PoseName + "(" + Index + ").anm";
                FullPathPngFile = ANMDirectory + PoseName + "(" + Index + ")_icon.tex";
                ANMName = PoseName + "(" + Index + ")";
                Index++;
            }
            Debuginfo.Log("FullPathANMFile=" + FullPathANMFile, 1);
            
            //anm
            CacheBoneDataArray cacheBoneDataArray = CurrentSelectedMaid.gameObject.AddComponent<CacheBoneDataArray>();
            cacheBoneDataArray.CreateCache(CurrentSelectedMaid.body0.GetBone("Bip01"));
            byte[] anmBinary = cacheBoneDataArray.GetAnmBinary(true, true);
            File.WriteAllBytes(FullPathANMFile, anmBinary);

            //png
            MaidPoseCreateIcon(ANMName, FullPathPngFile);

            LockBusy();
            Super.Pose.Pose_UpdateCustomMAidPoseANM(ANMName);
            MaidPose_List_ComboBox.DataSource = new BindingSource(Super.Pose.Pose_DataList[MaidPose_Category_ComboBox.SelectedItem.ToString()], null);
            MaidPose_List_ComboBox.ValueMember = "Value";
            MaidPose_List_ComboBox.DisplayMember = "ShowName";
            UnlockBusy();
        }

        private void ITems_tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Items_comboBox_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void Items_List_tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Items_list_tabPage1 == Items_List_tabControl.SelectedTab)
            {
                Items_Handle_Remove_Button.Enabled = true;
                Items_TryCreateSubHandle_button.Enabled = true;
            }
            else if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                Items_Handle_Remove_Button.Enabled = false;
                Items_Single_radioButton.Checked = true;
                Items_TryCreateSubHandle_button.Enabled = false;
            }
        }

        private void Prefab_Button_Click(object sender, EventArgs e)
        {
            string senderName = ((Button)sender).Name.Split('_')[1];
            bool bAll = senderName.Contains("All");

            if ("RemoveAll" == senderName) 
            {
                CurrentSelectedMaid.DelPrefabAll();
                return;
            }

            if (true == senderName.Contains("Remove"))
            {
                senderName = senderName.Replace("Remove", "");
                DelPrefabByClass(CurrentSelectedMaid, typeof(EMES_Yotogi.Perfab), senderName, false);
            }
            else if (true == senderName.Contains ("Nyou"))
            {
                Super.Yotogi.Yotogi_StartNyo(bAll, Prefab_Nyou_comboBox.SelectedItem.ToString());
            }
            else if (true == senderName.Contains("Sio"))
            {
                Super.Yotogi.Yotogi_StartSio(bAll, Prefab_Sio_comboBox.SelectedItem.ToString());
            }
            else if (true == senderName.Contains("Seieki"))
            {
                Super.Yotogi.Yotogi_StartSeieki(Prefab_Seieki_comboBox.SelectedItem.ToString());
            }
            else if (true == senderName.Contains("Enema"))
            {
                Super.Yotogi.Yotogi_StartEnema(Prefab_Enema_comboBox.SelectedItem.ToString());
            }
            else if (true == senderName.Contains("Toiki"))
            {
                Super.Yotogi.Yotogi_StartToiki(EMES_Yotogi.Perfab.Toiki[0]);
            }
            else if (true == senderName.Contains("Steam"))
            {
                Super.Yotogi.Yotogi_StartSteam(Prefab_Steam_comboBox.SelectedItem.ToString());
            }
            else   
            {   //StopAll
                if(true == bAll)
                {
                    for(int i = 0; i < CurrentMaidsStockID.Count; i++)
                    {
                        for(int j = 0; j < EMES_Yotogi.Perfab.Nyou.Count; j++)
                        {
                            CurrentMaidsList[i].DelPrefab(EMES_Yotogi.Perfab.Nyou[j]);
                            CurrentMaidsList[i].DelPrefab(EMES_Yotogi.Perfab.Sio[j]);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < EMES_Yotogi.Perfab.Sio.Count; j++)
                    {
                        CurrentSelectedMaid.DelPrefab(EMES_Yotogi.Perfab.Nyou[j]);
                        CurrentSelectedMaid.DelPrefab(EMES_Yotogi.Perfab.Sio[j]);
                    }
                }
            }
        }

        private void Prefab_Category_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Prefab_Items_comboBox.DataSource = new BindingSource(Super.Items.Items_perfabItems.Items[Prefab_Category_comboBox.SelectedItem.ToString()], null);
        }

        private void MyRoomCustomObjects_Category_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            MyRoomCustomObjects_Items_comboBox.DataSource = new BindingSource(Super.Items.myCustomRoomObject.Data[MyRoomCustomObjects_Category_comboBox.SelectedItem.ToString()], null);
        }

        private void DeskObjects_Category_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            DeskObjects_Items_comboBox.DataSource = new BindingSource(Super.Items.Items_DeskItemData[DeskObjects_Category_comboBox.SelectedItem.ToString()], null);
        }

        private void Items_RealtimeLoadThumbIcon_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            Super.settingsXml.bCreatePreviewIcon = Items_RealtimeLoadThumbIcon_checkBox.Checked;
            Super.SaveConfigurefile();
        }

        private void Items_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = Items_Objects_tabControl.SelectedTab.Text;

#if DEBUG
            Debuginfo.Log("senderName " + senderName, 2);
#endif

            if ("SS" == senderName)
            {
                if (true == Items_RealtimeLoadThumbIcon_checkBox.Checked)
                {
                    TryCreateSSIconToPictureBox(SS_pictureBox);
                }
                else
                {
                    SS_pictureBox.Image = SS_pictureBox.InitialImage;
                }
            }
            else if("部屋" == senderName)
            {
                KeyValuePair<int, string> item = (KeyValuePair<int, string>)MyRoomCustomObjects_Items_comboBox.SelectedItem;
                int selectedItemID = item.Key;
                if (-1 == selectedItemID)
                {
                    //無
                }
                else
                {
                    UpdatePictureBox(Super.Items.myCustomRoomObject.basicDatas[item.Key].GetThumbnail(), SS_pictureBox);
                }
            }
            else if("自席" == senderName)
            {
                if (true == Items_RealtimeLoadThumbIcon_checkBox.Checked)
                {
                    TryCreateDeskIconToPictureBox(SS_pictureBox);
                }
                else
                {
                    SS_pictureBox.Image = SS_pictureBox.InitialImage;
                }
            }
        }

        private void Prefab_Bone_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void PrefabItem_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name.ToString().Split('_')[1];
            string senderCategory = Prefab_Category_comboBox.SelectedItem.ToString();
            string prefix = "Prefab/";

            KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu> selectedItem = (KeyValuePair<string, EMES_Items.PhotoBGObjectData_Odogu>)Prefab_Items_comboBox.SelectedItem;
            string selectedItemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].name;
            EMES_Items.PhotoBGObjectData_Odogu selectedItemValue = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key];
            string sShowName = selectedItem.Key;

            if ("無" == selectedItemName)
            {
                return;
            }
#if DEBUG
            Debuginfo.Log("senderCategory = " + senderCategory, 2);
            Debuginfo.Log("selectedItem.Key = " + selectedItem.Key, 2);         
            Debuginfo.Log("Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].name = " + selectedItemName, 2);
#endif
            Vector3 pos = CurrentSelectedMaid.GetPos();
            Vector3 rot = new Vector3(0f, 0f, 0f);
            GameObject goBone = Items_GetCurrentMaidBone();

            pos.z += 1;
            if ("Add" == senderName)
            {
                HandleEx handle = null;
                if ("効果" == senderCategory)
                {
                    prefix = "Prefab/Particle/";
                    rot = new Vector3(-90f, 0f, 0f);
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, pos, rot, null, null, goBone);
                }
                else if ("手持品" == senderCategory)
                {                   
                    prefix = "model/handitem/";
                    rot = new Vector3(-90f, 0f, 0f);
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, pos, rot, null, null, goBone);
                }
                else if("その他２" == senderCategory)
                {
                    rot = pos;
                    if ("Fish" == Prefab_Items_comboBox.SelectedItem.ToString() || true == Prefab_Items_comboBox.SelectedItem.ToString().Contains("Mob"))
                    {
                        rot = new Vector3(-90f, 0f, 0f);
                    }
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, pos, rot, null, null, goBone);
                }
                else if ("ボディー" == senderCategory)
                {
                    prefix = "Prefab/Particle/";
                    pos.y += 1;
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, pos, rot, null, null, goBone);
                }
                else if ("舶来背景" == senderCategory)
                {
                    if (true == PrefabItem_BackgroundXN90_Checkbox.Checked)
                    {
                        rot = new Vector3(-90f, 0f, 0f);
                    }
                    prefix = "";
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, new Vector3(0f, 0f, 0f), rot, selectedItemValue, null, goBone);
                }
                else if("鏡" == senderCategory || "水" == senderCategory)
                {
                    prefix = "";
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItemName, sShowName, senderCategory, false, pos, rot, selectedItemValue, null, goBone);
                }
                else if ("小物" == senderCategory)
                {
                    string itemName = itemName = Super.Items.Items_perfabItems.Items[senderCategory][selectedItem.Key].assert_bg;
                    rot = new Vector3(0f, 0f, 0f);
                    handle = Super.Items.Items_CreatHandle(prefix, itemName, sShowName, senderCategory, false, pos, rot, selectedItemValue, null, goBone);
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
                                rot = new Vector3(-90f, 0f, 0f);
                                break;
                            case "パーティクル":
                                rot = new Vector3(90f, 0f, 0f);
                                pos = CurrentSelectedMaid.GetPos();
                                pos.y += 0.5f;
                                break;
                        }
                        handle = Super.Items.Items_CreatHandle(prefix, itemName, sShowName, senderCategory, false, pos, rot, selectedItemValue, null, goBone);
                    }
                    else
                    {
                        rot = new Vector3(-90f, 0f, 0f);
#if DEBUG
                        Debuginfo.Log(">> senderCategory = " + senderCategory, 2);
                        Debuginfo.Log(">> itemName = " + itemName, 2);
#endif
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
                        handle = Super.Items.Items_CreatHandle(prefix, itemName, sShowName, senderCategory, false, pos, rot, null, null, goBone);
                    }
                }

                SetHandleOptions(handle, true);
                SetHandleOptions(handle, ShadowCastingMode.On);
            }
            else if ("Remove" == senderName)
            {
                Super.Items.Items_RemoveCategory(senderCategory);
            }
            else
            {
                Super.Items.Items_RemoveAll();
            }

            Items_UpdateCurrentHandleCount();
        }

        private void MyRoomCustomObjects_Add_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name.ToString().Split('_')[1];
            string senderCategory = MyRoomCustomObjects_Category_comboBox.SelectedItem.ToString();

            KeyValuePair<int, string> selectedItem = (KeyValuePair<int, string>)MyRoomCustomObjects_Items_comboBox.SelectedItem;
            int selectedItemID = selectedItem.Key;

            if (-1 == selectedItemID)
            {
                return;
            }

#if DEBUG
            Debuginfo.Log("senderCategory = " + senderCategory, 2);
            Debuginfo.Log("selectedItem.Key = " + selectedItem.Key, 2);
            Debuginfo.Log("selectedItem.Value = " + selectedItem.Value, 2);
#endif

            Vector3 pos = CurrentSelectedMaid.GetPos();
            Vector3 rot = new Vector3(-90f, -90f, 0f);
            GameObject goBone = Items_GetCurrentMaidBone();

            pos.y += 1;
            pos.z += 1;
            switch (senderCategory)
            {
                case "扉":
                case "照明":
                    rot = new Vector3(-90f, 0f, 0f);
                    break;
                case "敷物":
                    pos.z -= 1;
                    break;
            }
            
            if ("Add" == senderName)
            {
                HandleEx handle = Super.Items.Items_CreatHandle(selectedItemID, selectedItem.Value, false, pos, Quaternion.Euler(rot), goBone);

                SetHandleOptions(handle, true);
                SetHandleOptions(handle, ShadowCastingMode.On);
            }

            Items_UpdateCurrentHandleCount();
        }

        private void DeskObjects_Add_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name.ToString().Split('_')[1];
            string senderCategory = DeskObjects_Category_comboBox.SelectedItem.ToString();
            string prefix = "Prefab/";

            KeyValuePair<EMES_Items.DeskItemData, string> selectedItem = (KeyValuePair<EMES_Items.DeskItemData, string>)DeskObjects_Items_comboBox.SelectedItem;

            if ("無" == selectedItem.Value)
            {
                return;
            }

#if DEBUG
            Debuginfo.Log("senderCategory = " + senderCategory, 2);
            Debuginfo.Log("selectedItem.Key = " + selectedItem.Key, 2);
            Debuginfo.Log("selectedItem.Value = " + selectedItem.Value, 2);
#endif

            Vector3 pos = CurrentSelectedMaid.GetPos();
            Vector3 rot = new Vector3(0f, -90f, 0f);
            GameObject goBone = Items_GetCurrentMaidBone();

            pos.y += 1;
            pos.z += 1;

            if ("Add" == senderName)
            {
                HandleEx handle;
                if(true == string.IsNullOrEmpty(selectedItem.Key.asset_name))
                {
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItem.Key.prefab_name, selectedItem.Value, senderCategory, false, pos, rot, null, selectedItem.Key, goBone);
                }
                else
                {
                    rot = new Vector3(-90f, -90f, 0f);
                    handle = Super.Items.Items_CreatHandle(prefix, selectedItem.Key.asset_name, selectedItem.Value, senderCategory, false, pos, rot, null, selectedItem.Key, goBone);
                }

                SetHandleOptions(handle, true);
                SetHandleOptions(handle, ShadowCastingMode.On);
            }

            Items_UpdateCurrentHandleCount();
        }

        private void Dance_PlayOfficial_Button_Click(object sender, EventArgs e)
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int CharaNum = EMES_Dance.Dance.Data[DanceName].iCharaNum;

            if (CurrentMaidsStockID.Count < CharaNum)
            {
                Debuginfo.Warning("人手不足　(´・ω・｀)", 0);
                return;
            }
            
            GameMain.Instance.MainCamera.FadeOut(0f, false, null, true, default(UnityEngine.Color));
            ToggleWindow(false);

            if (null == EMES_Dance.Dance.Dancer[0] || null == EMES_Dance.Dance.Dancer[1] || null == EMES_Dance.Dance.Dancer[2] || null == EMES_Dance.Dance.Dancer[3])
            Dance_AutoSelectDancer(CharaNum);

            for(int i = 0; i< CurrentMaidsStockID.Count; i++)
            {
                CurrentMaidsList[i].Visible = false;
            }

            for (int i = 0; i < CharaNum; i++)
            {
                if(null != EMES_Dance.Dance.Dancer[i])
                    EMES_Dance.Dance.Dancer[i].Visible = true;
            }

            int Index = EMES_Dance.Dance.Data[Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1]].iIndex;
            CharacterSelectManager.DefaultMaidList(EMES_Dance.Dance.Dancer.ToList());   
            DanceMain.SelectDanceData = EMES_Dance.Dance.List[Index];
            DanceSetting.Settings.IsSEPlay = true;
            DanceSetting.Settings.IsDepthOfFieldOn = true;
            DanceSetting.Settings.IsblackBGon = false;
            DanceSetting.Settings.IsNoteEffectLight = true;
            DanceSetting.Settings.UndressFaceOn = Dance_UndressFaceOn_CheckBox.Checked;
            DanceSetting.Settings.FPSCamMode = Dance_FPSCamMode_CheckBox.Checked;
            GameMain.Instance.LoadScene(EMES_Dance.Dance.List[Index].scene_name);     // play
        }

        private void Dance_List_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int Index = EMES_Dance.Dance.Data[DanceName].iIndex;
            
            Dance_SubPose_ComboBox.SelectedIndex = 0;
            Dance_SubPose_ComboBox.Select();

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
        }

        private void Dance_SubBGM_Button_clicked(object sender, EventArgs e)
        {
            GameMain.Instance.SoundMgr.PlayDanceBGM(Dance_SubBGM_ComboBox.SelectedItem + ".ogg", 0.5f, false);
        }

        private void Dance_SubPose_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int CharaNum = EMES_Dance.Dance.Data[DanceName].iCharaNum;

            if (Dance_SubPose_ComboBox.SelectedIndex >= CharaNum)
            {
                Dance_SubPose_ComboBox.SelectedIndex = 0;
                Dance_SubPose_ComboBox.Select();
            }
        }

        private void Dance_SetMaid_Button_Click(object sender, EventArgs e)
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            int CharaNum = EMES_Dance.Dance.Data[DanceName].iCharaNum;
            int Index = Dance_SubPose_ComboBox.SelectedIndex;
            string[] DancerPosition = Enum.GetNames(typeof(EMES_Dance.Dance.DancerPosition));

            Dance_SubPose_ComboBox.Items[Index] = Enum.GetNames(typeof(EMES_Dance.Dance.DancerPosition))[Index] + " ： " + CurrentSelectedMaid.status.callName;

            for (int i = 0; i < CharaNum; i++)
            {
                if (EMES_Dance.Dance.Dancer[i] == CurrentSelectedMaid && i != Index)
                {
                    Dance_SubPose_ComboBox.Items[i] = DancerPosition[i];
                    EMES_Dance.Dance.Dancer[i] = null;
                }
            }

            EMES_Dance.Dance.Dancer[Index] = CurrentSelectedMaid;
        }

        private void Dance_Start_Button_Click(object sender, EventArgs e)
        {
            Dance_StartAll();
        }

        private void Dance_SubPoseCurrent_Button_Click(object sender, EventArgs e)
        {
            string DanceName = Dance_List_ComboBox.SelectedItem.ToString().Split('：')[1];
            if (false == Dance_NoBGM_CheckBox.Checked)
            {
                GameMain.Instance.SoundMgr.PlayDanceBGM(Dance_SubBGM_ComboBox.SelectedItem + ".ogg", 0f, false);
            }
#if COM3D25
            if (false == EMES_Dance.Dance.Data[DanceName].bAbsoluteANMName)
            {
                Super.PerformPose(CurrentSelectedMaid, CurrentSelectedMaid.IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + (Dance_SubPose_ComboBox.SelectedIndex + 1).ToString() + ".anm");
            }
            else
            {
                Super.PerformPose(CurrentSelectedMaid, CurrentSelectedMaid.IsCrcBody ? "crc_" : "" + EMES_Dance.Dance.Data[DanceName].sANM + ".anm");
            }
#else
            if (false == EMES_Dance.Dance.Data[DanceName].bAbsoluteANMName)
            {
                Super.PerformPose(CurrentSelectedMaid, EMES_Dance.Dance.Data[DanceName].sANM + (Dance_SubPose_ComboBox.SelectedIndex + 1).ToString() + ".anm");
            }
            else
            {
                Super.PerformPose(CurrentSelectedMaid, EMES_Dance.Dance.Data[DanceName].sANM + ".anm");
            }
#endif
            lastPerformedPose = LastPerformedPose.ANM;
        }


        private void Dance_SubPoseAll_Button_Click(object sender, EventArgs e)
        {
            Dance_StartAllOther();
        }

        private void Dance_BGMPlay_button_Click(object sender, EventArgs e)
        {
            GameMain.Instance.SoundMgr.PlayDanceBGM(Dance_BGM_ComboBox.SelectedItem + ".ogg", 0f, Dance_BGM_CheckBox.Checked);
        }

        private void Dance_BGMRandomPlay_Button_Click(object sender, EventArgs e)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int Index = rnd.Next(Dance_BGM_ComboBox.Items.Count);

            Dance_BGM_ComboBox.SelectedIndex = Index;
            Dance_BGM_ComboBox.Select();
            GameMain.Instance.SoundMgr.PlayDanceBGM(Dance_BGM_ComboBox.SelectedItem + ".ogg", 0f, Dance_BGM_CheckBox.Checked);
        }


        private void Yotogi_Item_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string ANM = Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex] + ".anm";

            if (true == ANM.Contains("_m") && false == Yotogi_AutoANMTranslate_checkBox.Checked)
            {
#if DEBUG
                Debuginfo.Log("自動転写=off 無視する", 1);
#endif
                return;
            }

            if (true == ANM.Contains("無"))
            {
#if DEBUG
                Debuginfo.Log("無　(；ﾟДﾟ)", 1);
#endif

                return;
            }

            Super.Yotogi.Yotogi_Perform(CurrentSelectedMaid, ANM, Yotogi_LoopPlay_checkBox.Checked);
            lastPerformedPose = LastPerformedPose.Yotogi;
        }

        private void Yotogi_Category_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            BindingSource bs = new BindingSource();
            bs.DataSource = Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()];
            Yotogi_Item_comboBox.DataSource = bs;

            bool bRefresh = false;
            if(true == Yotogi_Category_comboBox.SelectedItem.ToString().Contains("奻"))
            {   
                if (EMES_Yotogi.YotogiState.奻 != Super.Yotogi.YotogiLastState)
                {
                    Super.Yotogi.Yotogi_InitPerformer(false, true, false, true, EMES_Yotogi.YotogiState.奻);
                    bRefresh = true;
                }
            }
            else if (true == Yotogi_Category_comboBox.SelectedItem.ToString().Contains("嬲"))
            {   
                if (EMES_Yotogi.YotogiState.嬲 != Super.Yotogi.YotogiLastState)
                {
                    Super.Yotogi.Yotogi_InitPerformer(true, true, true, false, EMES_Yotogi.YotogiState.嬲);
                    bRefresh = true;
                }
            }
            else if(true == Yotogi_Category_comboBox.SelectedItem.ToString().Contains("嫐"))
            {
                if (EMES_Yotogi.YotogiState.嫐 != Super.Yotogi.YotogiLastState)
                {
                    Super.Yotogi.Yotogi_InitPerformer(true, true, false, true, EMES_Yotogi.YotogiState.嫐);
                    bRefresh = true;
                }
            }
            else if ("女男男男" == Yotogi_Category_comboBox.SelectedItem.ToString())
            {
                Super.Yotogi.Yotogi_InitPerformer(true, true, true, true, EMES_Yotogi.YotogiState.女男男男);
                bRefresh = true;
            }
            else
            {
                if (EMES_Yotogi.YotogiState.娚 != Super.Yotogi.YotogiLastState)
                {
                    Super.Yotogi.Yotogi_InitPerformer(true, true, false, false, EMES_Yotogi.YotogiState.娚);
                    bRefresh = true;
                }
            }

            if (true == bRefresh)
            {
                LockBusy();
                Yotogi_Role_comboBox.Items.Clear();
                foreach (KeyValuePair<EMES_Yotogi.YotogiRole, Maid> Key in Super.Yotogi.YotogiRoleList)
                {
                    Yotogi_Role_comboBox.Items.Add(Key.Key.ToString());
                }
                Yotogi_Role_comboBox.SelectedIndex = 0;
                Yotogi_Role_comboBox.Select();
                Yotogi_Category_comboBox.Select();
                UnlockBusy();
            }
        }

        private void Yotogi_AutoANMTranslate_checkBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Yotogi_Role_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Yotogi_Role_button_Click(object sender, EventArgs e)
        {
            string Role = Yotogi_Role_comboBox.SelectedItem.ToString().Split('：')[0];
            int Index = Yotogi_Role_comboBox.SelectedIndex;

            Super.Yotogi.Yotogi_SetRole((EMES_Yotogi.YotogiRole)Enum.Parse(typeof(EMES_Yotogi.YotogiRole), Role), CurrentSelectedMaid);

            Yotogi_Role_comboBox.Items.Clear();
            foreach (KeyValuePair<EMES_Yotogi.YotogiRole, Maid> Key in Super.Yotogi.YotogiRoleList)
            {
                Yotogi_Role_comboBox.Items.Add(Key.Key.ToString() + (Key.Value ? "：" + Key.Value.status.callName : ""));
            }
            Yotogi_Role_comboBox.SelectedIndex = Index;
            Yotogi_Role_comboBox.Select();
        }

        private void Yotogi_Play_button_Click(object sender, EventArgs e)
        {
            if (EMES_Yotogi.YotogiState.娚 == Super.Yotogi.YotogiLastState)
            {
                Super.Yotogi.Yotogi_PerformFM(Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex]+".anm", Yotogi_LoopPlay_checkBox.Checked, Yotogi_SnycPosRot_checkBox.Checked);

            }
            else if (EMES_Yotogi.YotogiState.奻 == Super.Yotogi.YotogiLastState)
            {
                Super.Yotogi.Yotogi_PerformFF(Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex] + ".anm", Yotogi_LoopPlay_checkBox.Checked, Yotogi_SnycPosRot_checkBox.Checked);

            }
            else if (EMES_Yotogi.YotogiState.嬲 == Super.Yotogi.YotogiLastState)
            {
                Super.Yotogi.Yotogi_PerformMFM(Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex] + ".anm", Yotogi_LoopPlay_checkBox.Checked, Yotogi_SnycPosRot_checkBox.Checked);
            }
            else if (EMES_Yotogi.YotogiState.嫐 == Super.Yotogi.YotogiLastState)
            {
                Super.Yotogi.Yotogi_PerformFMF(Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex] + ".anm", Yotogi_LoopPlay_checkBox.Checked, Yotogi_SnycPosRot_checkBox.Checked);
            }
            else if (EMES_Yotogi.YotogiState.女男男男 == Super.Yotogi.YotogiLastState)
            {
                Super.Yotogi.Yotogi_PerformFMMM(Super.Yotogi.Yotogi_data[Yotogi_Category_comboBox.SelectedItem.ToString()][Yotogi_Item_comboBox.SelectedIndex] + ".anm", Yotogi_LoopPlay_checkBox.Checked, Yotogi_SnycPosRot_checkBox.Checked);
            }
        }

        private void Shader_Bloom_SyncData(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_BloomData(Shader_Bloom_screenBlendMode_comboBox.SelectedIndex, Shader_Bloom_BloomQuality_comboBox.SelectedIndex,
                Shader_Bloom_sepBlurSpread_trackBar.Value, Shader_Bloom_bloomBlurIterations_trackBar.Value);
        }

        private void Shader_Bloom_Reset_Button_Click(object sender, EventArgs e)
        {
            Super.exShader.Reset_BloomData();
            CameraPlus_ShaderWindowInit();
        }

        private void Shader_Blur_SyncData(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_BlurData(Shader_Blur_Enable_checkbox.Checked, Shader_Blur_blurSize_trackBar.Value, Shader_Blur_blurIterations_trackBar.Value);
        }

        private void Shader_DOF_SyncData(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_DOFData(Shader_DOF_Enable_checkBox.Checked, Shader_DOF_visualizeFocus_checkBox.Checked,
                Shader_DOF_focalLength_trackBar.Value, Shader_DOF_focalSize_trackBar.Value, Shader_DOF_aperture_trackBar.Value, Shader_DOF_maxBlurSize_trackBar.Value,
                Shader_DOF_blurType_comboBox.SelectedIndex);
        }

        private void Shader_Fog_SyncData(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_GlobalFogData(Shader_Fog_Enable_checkBox.Checked, Shader_Fog_startDistance_trackBar.Value, Shader_Fog_globalDensity_trackBar.Value,
                Shader_Fog_heightScale_trackBar.Value, Shader_Fog_height_trackBar.Value, Shader_Fog_globalFogColor_pictureBox.BackColor);
        }

        private void Shader_Fog_globalFogColor_pictureBox_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            ignoreShowWindow = true;
            BackGroundColorDialog.Color = Shader_Fog_globalFogColor_pictureBox.BackColor;
            BackGroundColorDialog.ShowDialog();
            ignoreShowWindow = false;

            Shader_Fog_globalFogColor_pictureBox.BackColor = BackGroundColorDialog.Color;

            Shader_Fog_SyncData(sender, EventArgs.Empty);
        }

        private void Shader_Fog_Reset_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Reset_GlobalFogData();
            CameraPlus_ShaderWindowInit();
        }

        private void Shader_Sepia_Enable_checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_SepiaData(Shader_Sepia_Enable_checkbox.Checked);
        }

        private void Shader_Vignetting_SyncData(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Set_VignettingData(Shader_Vignette_Enable_checkBox.Checked,
                Shader_Vignetting_intensity_trackBar.Value, Shader_Vignetting_chromaticAberration_trackBar.Value, 
                Shader_Vignetting_blurSpread_trackBar.Value, Shader_Vignetting_blur_trackBar.Value);
        }

        private void Shader_Vignetting_Reset_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Super.exShader.Reset_VignettingData();
            CameraPlus_ShaderWindowInit();
        }

        private void MaidPose_FaceAddGazePoint_button_Click(object sender, EventArgs e)
        {
            Super.MaidIK.IK_AddGazePoint(CurrentSelectedMaid);
            Items_UpdateCurrentHandleCount();
        }

        private void MaidPose_FaceRemoveGazePoint_button_Click(object sender, EventArgs e)
        {
            Super.MaidIK.IK_RemoveGazePoint(CurrentSelectedMaid, false);
            Items_UpdateCurrentHandleCount();
        }

        private void MaidPose_FaceShowGazePoint_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            foreach (KeyValuePair<string, EMES_MaidIK.UniversalPoint> gp in Super.MaidIK.MaidGazePoint.ToList())
            {
                Super.MaidIK.MaidGazePoint[gp.Key].target.SetActive(MaidFace_ShowGazePoint_checkBox.Checked);
            }
        }

        private void MaidPose_FaceLock_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }
            CurrentSelectedMaid.LockHeadAndEye(MaidPose_FaceLock_checkBox.Checked);

            if (true == MaidPose_FaceLock_checkBox.Checked)
            {
                MaidFace_ShowEyesHandleL_checkBox.Checked = false;
                MaidFace_ShowEyesHandleR_checkBox.Checked = false;
            }
        }

        private void MaidFace_ShowEyesHandle_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            string senderName = ((CheckBox)sender).Name;
            if (senderName.Contains("L"))
            {
                Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Eye_L, ((CheckBox)sender).Checked);
            }
            else
            {
                Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Eye_R, ((CheckBox)sender).Checked);
            }

            if (true == ((CheckBox)sender).Checked)
            {
                CurrentSelectedMaid.body0.boLockHeadAndEye = MaidPose_FaceLock_checkBox.Checked;
                LockBusy();
                MaidPose_FaceLock_checkBox.Checked = true;
                UnlockBusy();
            }
        }

        private void MaidFace_ResetEyeRotation_button_Click(object sender, EventArgs e)
        {
            CurrentSelectedMaid.body0.trsEyeL.localRotation = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].Bakcup_Eye_L;
            CurrentSelectedMaid.body0.trsEyeR.localRotation = Super.MaidIK.MaidsIK[GetCurrentMaidStockID()].Bakcup_Eye_R;
        }

        private void Mune_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            string senderName = ((CheckBox)sender).Name;
            if (senderName.Contains("L"))
            {
                Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Mune_L, ((CheckBox)sender).Checked);
            }
            else
            {
                Super.MaidIK.IK_SetBoneTypeVisible(GetCurrentMaidStockID(), EMES_MaidIK.BoneType.Mune_R, ((CheckBox)sender).Checked);
            }

            if (true == ((CheckBox)sender).Checked)
            {
                MuneYureCheckBox.Checked = false;
            }
        }

        private void Scene_Select_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            string selectedName = Scene_Select_ComboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            EMES_SceneManagement.SceneDataNew sd = Super.sceneManagement.MaidSceneData[Scene_Select_ComboBox.SelectedIndex];
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(sd.ScreenShot, 0, Convert.ToInt32(sd.ScreenShot.Length));
                Scene_PictureBox.Image = Image.FromStream(mStream);
            }
        }

        private void Scene_Save_Button_Click(object sender, EventArgs e)
        {
            EMES_SceneManagement.SceneDataNew sd = new EMES_SceneManagement.SceneDataNew();

            int resW = GameMain.Instance.MainCamera.camera.pixelWidth / 8;
            int resH = GameMain.Instance.MainCamera.camera.pixelHeight / 8;

            sd.ScreenShot = Super.sceneManagement.ScreenShot(resW, resH, true);
            sd.ScreenShotName = EMES_SceneManagement.ScreenShotName(resW, resH);
            sd.DateTime = EMES_SceneManagement.ScreenShotTime();

            //Maids
            for (int i = 0; i < CurrentMaidsStockID.Count; i++)
            {
                EMES_SceneManagement.MaidData md = new EMES_SceneManagement.MaidData();
                md.guid = CurrentMaidsList[i].status.guid;
                md.firstName = CurrentMaidsList[i].status.firstName;
                md.lastName = CurrentMaidsList[i].status.lastName;
                Vector3 pos = CurrentMaidsList[i].GetPos();
                Vector3 rot = CurrentMaidsList[i].GetRot();
                float scale = CurrentMaidsList[i].body0.transform.localScale.x;
                md.pos_x = pos.x;
                md.pos_y = pos.y;
                md.pos_z = pos.z;
                md.rot_x = rot.x;
                md.rot_y = rot.y;
                md.rot_z = rot.z;
                md.scale = scale;
                CacheBoneDataArray cacheBoneDataArray = CurrentMaidsList[i].gameObject.AddComponent<CacheBoneDataArray>();
                cacheBoneDataArray.CreateCache(CurrentMaidsList[i].body0.GetBone("Bip01"));
                md.poseData = cacheBoneDataArray.GetAnmBinary(true, true);
                sd.Maids.Add(md);
            }

            //cameraData
            Vector3 cam_pos = GameMain.Instance.MainCamera.GetTargetPos();
            Quaternion cam_rot = GameMain.Instance.MainCamera.camera.transform.rotation;
            sd.cameraData.pos_x = cam_pos.x;
            sd.cameraData.pos_y = cam_pos.y;
            sd.cameraData.pos_z = cam_pos.z;
            sd.cameraData.rot_x = cam_rot.x;
            sd.cameraData.rot_y = cam_rot.y;
            sd.cameraData.rot_z = cam_rot.z;
            sd.cameraData.rot_w = cam_rot.w;
            sd.cameraData.distance = GameMain.Instance.MainCamera.GetDistance();
            sd.cameraData.fov = GameMain.Instance.MainCamera.camera.fieldOfView;

            //cameraDataSlot
#if DEBUG
            Debuginfo.Log("saving cameraDataSlot Super.camPlus.CameraPosDataList.count = " + Super.camPlus.CameraPosDataList.Count, 2);
#endif
            Action<int> actionCameraDataSlot = delegate (int index)
            {
                List<string> sList = Super.camPlus.CameraPosDataList[index].Keys.ToList();
#if DEBUG
                Debuginfo.Log("saving cameraDataSlot index = " + index.ToString(), 2);
#endif
                sd.cameraDataSlot.Add(new EMES_SceneManagement.CameraDataSlot());
                sd.cameraDataSlot[index].pos_x = Super.camPlus.CameraPosDataList[index]["px"];
                sd.cameraDataSlot[index].pos_y = Super.camPlus.CameraPosDataList[index]["py"];
                sd.cameraDataSlot[index].pos_z = Super.camPlus.CameraPosDataList[index]["pz"];
                sd.cameraDataSlot[index].rot_x = Super.camPlus.CameraPosDataList[index]["rx"];
                sd.cameraDataSlot[index].rot_y = Super.camPlus.CameraPosDataList[index]["ry"];
                sd.cameraDataSlot[index].rot_z = Super.camPlus.CameraPosDataList[index]["rz"];
                sd.cameraDataSlot[index].distance = Super.camPlus.CameraPosDataList[index]["d"];
                sd.cameraDataSlot[index].fov = Super.camPlus.CameraPosDataList[index]["fov"];
                foreach(string s in sList)
                {
#if DEBUG
                    Debuginfo.Log("cameraDataSlot s=" + s, 2);
#endif
                    if (true == s.StartsWith("TAG_"))
                        sd.cameraDataSlot[index].tag = s;
                    else
                        sd.cameraDataSlot[index].tag = "TAG_無";
                }                
            };

            for (int index = 0; index < 5; index++)
                actionCameraDataSlot(index);

            //shaderData
            Super.exShader.GetAllData(out sd.shaderData);

            //mainLight
            sd.mainLight.rot_x = Light_Main_X_trackBar.Value;
            sd.mainLight.rot_y = Light_Main_Y_trackBar.Value;
            sd.mainLight.color_r = Light_Main_Color_pictureBox.BackColor.R;
            sd.mainLight.color_g = Light_Main_Color_pictureBox.BackColor.G;
            sd.mainLight.color_b = Light_Main_Color_pictureBox.BackColor.B;
            sd.mainLight.brightness = Light_Main_Brightness_trackBar.Value;
            sd.mainLight.shadow = Light_Main_Shadow_trackBar.Value;

            //subLight
            sd.subLight.Clear();
            if ("無" != SubLight_index_comboBox.SelectedItem.ToString())
            {
                for (int i = 0; i < SubLight_index_comboBox.Items.Count; i++)
                {
                    EMES_SceneManagement.SubLightData sld = new EMES_SceneManagement.SubLightData();
                    bool bEnable;
                    LightType lt;
                    int range;
                    int brightness;
                    int angle;
                    GameObject goSubLight = Super.camPlus.GetSubLight(i, out bEnable, out lt, out range, out brightness, out angle);

                    sld.enable = bEnable;
                    sld.type = (int)lt;
                    sld.range = range;
                    sld.brightness = brightness;
                    sld.spotAngle = angle;

                    HandleEx handle = CamPlus_SubLightHandleList[i];
                    sld.pos_x = handle.GetParentBone().transform.position.x;
                    sld.pos_y = handle.GetParentBone().transform.position.y;
                    sld.pos_z = handle.GetParentBone().transform.position.z;
                    sld.rot_x = handle.GetParentBone().transform.rotation.x;
                    sld.rot_y = handle.GetParentBone().transform.rotation.y;
                    sld.rot_z = handle.GetParentBone().transform.rotation.z;
                    sld.rot_w = handle.GetParentBone().transform.rotation.w;

                    sd.subLight.Add(sld);
                }
            }

            //maidTailsBoneData
            sd.maidTailsBoneData = Super.maidTails.CreateScenceManagementBontTailData();

            //itemsHandleInfos
            foreach(KeyValuePair<HandleEx, string> kvp in Super.Items.Items_ItemHandle)
            {
                if (true == kvp.Key.sCategory.Equals("WildHandle") || true == kvp.Key.sCategory.Equals("MaidPartsHandle") || true == kvp.Key.sCategory.Equals("RTMIHandle"))
                {
                    continue;
                }

                EMES_SceneManagement.ItemsHandleInfo ihi = new EMES_SceneManagement.ItemsHandleInfo();
                ihi.Position_x = kvp.Key.GetParentBone().position.x;
                ihi.Position_y = kvp.Key.GetParentBone().position.y;
                ihi.Position_z = kvp.Key.GetParentBone().position.z;
                ihi.Rotation_x = kvp.Key.GetParentBone().rotation.x;
                ihi.Rotation_y = kvp.Key.GetParentBone().rotation.y;
                ihi.Rotation_z = kvp.Key.GetParentBone().rotation.z;
                ihi.Rotation_w = kvp.Key.GetParentBone().rotation.w;

                ihi.localScale_x = kvp.Key.GetParentBone().localScale.x;
                ihi.localScale_y = kvp.Key.GetParentBone().localScale.y;
                ihi.localScale_z = kvp.Key.GetParentBone().localScale.z;

                ihi.localPosition_x = kvp.Key.GetParentBone().localPosition.x;
                ihi.localPosition_y = kvp.Key.GetParentBone().localPosition.y;
                ihi.localPosition_z = kvp.Key.GetParentBone().localPosition.z;
                ihi.localRotation_x = kvp.Key.GetParentBone().localRotation.x;
                ihi.localRotation_y = kvp.Key.GetParentBone().localRotation.y;
                ihi.localRotation_x = kvp.Key.GetParentBone().localRotation.z;
                ihi.localRotation_w = kvp.Key.GetParentBone().localRotation.w;

                ihi.sCategory = kvp.Key.sCategory;
                ihi.sItemName = kvp.Key.sItemName;
                ihi.sItemFullName = kvp.Key.sItemFullName;
                ihi.sShowName = kvp.Value;             
                ihi.sOptions = kvp.Key.sOptions;
#if DEBUG
                Debuginfo.Log("Add " + ihi.sItemFullName, 2);
#endif
                sd.itemsHandleInfo.Add(ihi);
            }

            Super.sceneManagement.MaidSceneData.Add(sd);
            Super.sceneManagement.SaveMaidSceneData();

            SceneManagement_UpdateWindow(sd, null);
        }

        private void Scene_Sync_Button_Click(object sender, EventArgs e)
        {
            string selectedName = Scene_Select_ComboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            ToggleWindow(false);

            if (true == Scene_RemoveCurrentItems_checkBox.Checked)
            {
                Super.Items.Items_RemoveAll();
            }

            EMES_SceneManagement.SceneDataNew sd = Super.sceneManagement.MaidSceneData[Scene_Select_ComboBox.SelectedIndex];
            List<int> maidDataListOrder = new List<int>();
            List<Maid> maidDataList = new List<Maid>();
            if (true == Scene_GUID_checkBox.Checked)
            {
                Dictionary<string, int> sdMaid = new Dictionary<string, int>();
                Dictionary<string, int> sdMaidFirstPass = new Dictionary<string, int>();
                Dictionary<string, int> currentMaid = new Dictionary<string, int>();
                for (int i = 0; i < sd.Maids.Count; i++)
                {
                    sdMaid.Add(sd.Maids[i].guid, i);
                    sdMaidFirstPass.Add(sd.Maids[i].guid, i);
                }

                for(int i = 0; i < CurrentMaidsStockID.Count; i++)
                {
                    currentMaid.Add(CurrentMaidsList[i].status.guid, i);
                }

                foreach (KeyValuePair<string, int> key in currentMaid.ToList())
                {
                    if (true == sdMaidFirstPass.ContainsKey(key.Key))
                    {
#if DEBUG
                        Debuginfo.Log("Sync Maids with GUID " + CurrentMaidsList[currentMaid[key.Key]].status.callName, 2);
#endif
                        CurrentMaidsList[currentMaid[key.Key]].SetPos(new Vector3(sd.Maids[sdMaid[key.Key]].pos_x, sd.Maids[sdMaid[key.Key]].pos_y, sd.Maids[sdMaid[key.Key]].pos_z));
                        CurrentMaidsList[currentMaid[key.Key]].SetRot(new Vector3(sd.Maids[sdMaid[key.Key]].rot_x, sd.Maids[sdMaid[key.Key]].rot_y, sd.Maids[sdMaid[key.Key]].rot_z));
                        CurrentMaidsList[currentMaid[key.Key]].body0.transform.localScale = new Vector3(sd.Maids[sdMaid[key.Key]].scale, sd.Maids[sdMaid[key.Key]].scale, sd.Maids[sdMaid[key.Key]].scale);
                        Super.Yotogi.Yotogi_LoadAnime(CurrentMaidsList[currentMaid[key.Key]], "scenemanagement", sd.Maids[sdMaid[key.Key]].poseData, false, false);
                        CurrentMaidsList[currentMaid[key.Key]].body0.m_Bones.GetComponent<Animation>().Play("scenemanagement");
#if DEBUG
                        Debuginfo.Log("key.Key = " + key.Key, 2);
                        Debuginfo.Log("sdMaid[key.Key] = " + sdMaid[key.Key].ToString(), 2);
#endif
                        maidDataListOrder.Add(sdMaid[key.Key]);
                        maidDataList.Add(CurrentMaidsList[currentMaid[key.Key]]);
                        currentMaid.Remove(key.Key);
                        sdMaid.Remove(key.Key);
                    }
                }

                if(0 != sdMaid.Count && 0 != currentMaid.Count)
                {
                    int Counter = sdMaid.Count;

                    if(sdMaid.Count >= currentMaid.Count)
                    {
                        Counter = currentMaid.Count;
                    }

                    List<int> sdList = sdMaid.Select(key => key.Value).ToList();
                    List<int> maidList = currentMaid.Select(key => key.Value).ToList();
                    for (int i = 0; i < Counter; i++)
                    {
#if DEBUG
                        Debuginfo.Log("Sync Maids without GUID " + CurrentMaidsList[maidList[i]].status.callName, 2);
#endif
                        if (sd.Maids[sdList[i]].guid != CurrentMaidsList[maidList[i]].status.guid)
                        {
                            Debuginfo.Warning("警告、メイドGUIDが一致しません、続行します", 0);
                            Debuginfo.Log("ストレージ " + sd.Maids[sdList[i]].firstName + " " + sd.Maids[sdList[i]].lastName, 1);
                            Debuginfo.Log("現在 " + CurrentMaidsList[maidList[i]].status.firstName + " " + CurrentMaidsList[maidList[i]].status.lastName, 1);
                        }
                        CurrentMaidsList[maidList[i]].SetPos(new Vector3(sd.Maids[sdList[i]].pos_x, sd.Maids[sdList[i]].pos_y, sd.Maids[sdList[i]].pos_z));
                        CurrentMaidsList[maidList[i]].SetRot(new Vector3(sd.Maids[sdList[i]].rot_x, sd.Maids[sdList[i]].rot_y, sd.Maids[sdList[i]].rot_z));
                        CurrentMaidsList[maidList[i]].body0.transform.localScale = new Vector3(sd.Maids[sdList[i]].scale, sd.Maids[sdList[i]].scale, sd.Maids[sdList[i]].scale);
                        Super.Yotogi.Yotogi_LoadAnime(CurrentMaidsList[maidList[i]], "scenemanagement", sd.Maids[sdList[i]].poseData, false, false);
                        CurrentMaidsList[maidList[i]].body0.m_Bones.GetComponent<Animation>().Play("scenemanagement");
                        maidDataListOrder.Add(sdList[i]);
                        maidDataList.Add(CurrentMaidsList[maidList[i]]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < sd.Maids.Count; i++)
                {
                    if (i >= CurrentMaidsStockID.Count)
                    {
                        break;
                    }

#if DEBUG
                    Debuginfo.Log("Sync Maids " + CurrentMaidsList[i].status.callName, 2);
#endif

                    if (sd.Maids[i].guid != CurrentMaidsList[i].status.guid)
                    {
                        Debuginfo.Warning("警告、メイドGUIDが一致しません、続行します", 0);
                        Debuginfo.Log("ストレージ " + sd.Maids[i].firstName + " " + sd.Maids[i].lastName, 1);
                        Debuginfo.Log("現在 " + CurrentMaidsList[i].status.firstName + " " + CurrentMaidsList[i].status.lastName, 1);
                    }
                    CurrentMaidsList[i].SetPos(new Vector3(sd.Maids[i].pos_x, sd.Maids[i].pos_y, sd.Maids[i].pos_z));
                    CurrentMaidsList[i].SetRot(new Vector3(sd.Maids[i].rot_x, sd.Maids[i].rot_y, sd.Maids[i].rot_z));
                    CurrentMaidsList[i].body0.transform.localScale = new Vector3(sd.Maids[i].scale, sd.Maids[i].scale, sd.Maids[i].scale);
                    Super.Yotogi.Yotogi_LoadAnime(CurrentMaidsList[i], "scenemanagement", sd.Maids[i].poseData, false, false);
                    CurrentMaidsList[i].body0.m_Bones.GetComponent<Animation>().Play("scenemanagement");
                }
            }

#if DEBUG
            Debuginfo.Log("Sync MainCamera ", 2);
#endif
            GameMain.Instance.MainCamera.SetTargetPos(new Vector3(sd.cameraData.pos_x, sd.cameraData.pos_y, sd.cameraData.pos_z), false);
            GameMain.Instance.MainCamera.SetDistance(sd.cameraData.distance, false);
            GameMain.Instance.MainCamera.camera.fieldOfView = sd.cameraData.fov;
            GameMain.Instance.MainCamera.camera.transform.rotation = new Quaternion(sd.cameraData.rot_x, sd.cameraData.rot_y, sd.cameraData.rot_z, sd.cameraData.rot_w);
            CamPlus_UpdateFOV();

            //cameraDataSlot
#if DEBUG
            Debuginfo.Log("Sync cameraDataSlot ", 2);
#endif
            for (int index = 0; index < 5; index++)
            {
                Super.camPlus.CameraDataSlotInit(index, sd.cameraDataSlot[index]);
                TextBox textBox = (Controls.Find("CameraPos_Tag" + (index + 1).ToString() + "_textBox", true)[0] as TextBox);
                textBox.Text = sd.cameraDataSlot[index].tag.Replace("TAG_", "");
            }

            Super.exShader.SetAllData(sd.shaderData);
            CameraPlus_ShaderWindowInit();

#if DEBUG
            Debuginfo.Log("Sync Main Light ", 2);
#endif
            LockBusy();
            SetTrackBarValue(Light_Main_X_trackBar, sd.mainLight.rot_x);
            SetTrackBarValue(Light_Main_Y_trackBar, sd.mainLight.rot_y);
            Light_Main_Color_pictureBox.BackColor = System.Drawing.Color.FromArgb(255, sd.mainLight.color_r, sd.mainLight.color_g, sd.mainLight.color_b);
            SetTrackBarValue(Light_Main_Shadow_trackBar, sd.mainLight.shadow);
            SetTrackBarValue(Light_Main_Brightness_trackBar, sd.mainLight.brightness);
            UnlockBusy();
            Light_Main_trackBar_Scroll(this, EventArgs.Empty);

#if DEBUG
            Debuginfo.Log("Remove current Sub Light ", 2);
#endif
            LockBusy();
            selectedName = SubLight_index_comboBox.SelectedItem.ToString();
            if ("無" == selectedName)
            {
                if (0 == sd.subLight.Count)
                {
                    SubLight_index_comboBox.Items.Add("無");
                }
            }
            else
            {
                for (int i = SubLight_index_comboBox.Items.Count - 1; i >= 0; i--)
                {
#if DEBUG
                    Debuginfo.Log("Remove Sub Light Point " + CamPlus_SubLightHandleList[i], 2);
#endif
                    Super.MaidIK.IK_RemoveSubLightPoint(CamPlus_SubLightHandleList[i]);

#if DEBUG
                    Debuginfo.Log("Remove Sub Light " + i.ToString(), 2);
#endif
                    Super.camPlus.RemoveSubLight(Super.camPlus.SubLightList[i]);
                }
            }
            SubLight_index_comboBox.Items.Clear();

            if (0 != sd.subLight.Count)
            {
#if DEBUG
                Debuginfo.Log("Sync Sub Light total=" + sd.subLight.Count, 2);
#endif
                int Index = 0;
                foreach (EMES_SceneManagement.SubLightData sld in sd.subLight)
                {
                    GameObject subLight;
                    Super.camPlus.CreateSubLight((LightType)sld.type, out subLight, out Index);
#if DEBUG
                    Debuginfo.Log("Sync Sub Light " + Index.ToString(), 2);
#endif
                    HandleEx handle;
                    Super.MaidIK.IK_AddSubLightPoint(subLight, Index, out handle);
                    GameObject goSubLight = Super.camPlus.SetSubLight(Index, sld.enable, (LightType)sld.type, ((float)sld.range) / 100, ((float)sld.brightness) / 100, ((float)sld.spotAngle) / 100);
#if DEBUG
                    Debuginfo.Log("Sync Sub Light sld.range=" + sld.range + " sld.brightness=" + sld.brightness + " sld.spotAngle=" + sld.spotAngle, 2);
#endif

#if DEBUG
                    Debuginfo.Log("Move Sub Light " + goSubLight.name, 2);
#endif
                    //HandleEx handle = Super.MaidIK.IK_GetSubLightHandle(goSubLight.name);
                    handle.Visible = true;
                    handle.GetParentBone().position = new Vector3(sld.pos_x, sld.pos_y, sld.pos_z);
                    handle.GetParentBone().rotation = new Quaternion(sld.rot_x, sld.rot_y, sld.rot_z, sld.rot_w);

                    SubLight_index_comboBox.Items.Add((Index + 1).ToString());
                }
#if DEBUG
                Debuginfo.Log("Sync Sub Light Done", 2);
#endif
            }
            else
            {
                SubLight_index_comboBox.Items.Add("無");
            }
            UnlockBusy();
            SubLight_index_comboBox.SelectedIndex = 0;
            SubLight_index_comboBox.Select();

            if(0 == sd.maidTailsBoneData.Count)
            {
#if DEBUG
                Debuginfo.Log("maidTailsBoneData無視する", 2); 
#endif
            }
            else
            {
#if DEBUG
                Debuginfo.Log("Sync maidTailsBoneData", 2);

                foreach(int i in maidDataListOrder)
                {
                    Debuginfo.Log("Order = " + i.ToString(), 2);
                }
#endif
                MT.RotateScenceManagementBontTailData(sd.maidTailsBoneData, maidDataList, maidDataListOrder);
            }
            MaidTails_Init();

            if (0 == sd.itemsHandleInfo.Count)
            {
#if DEBUG
                Debuginfo.Log("itemsHandleInfo無視する", 2);
#endif
            }
            else
            {
#if DEBUG
                Debuginfo.Log("Sync maidTailsBoneData", 2);
#endif
                LockBusy();
                Super.Items.Items_RemoveAll();

                foreach (EMES_SceneManagement.ItemsHandleInfo ihi in sd.itemsHandleInfo)
                {
#if DEBUG
                    Debuginfo.Log("Load [" + ihi.sCategory + "] =" + ihi.sItemName + " FullName = " + ihi.sItemFullName, 2);
                    Debuginfo.Log("sOptions = " + ihi.sOptions, 2);
#endif
                    Vector3 Pos = new Vector3(ihi.Position_x, ihi.Position_y, ihi.Position_z);
                    Vector3 localPos = new Vector3(ihi.localPosition_x, ihi.localPosition_y, ihi.localPosition_z);
                    Quaternion Rot = new Quaternion(ihi.Rotation_x, ihi.Rotation_y, ihi.Rotation_z, ihi.Rotation_w);
                    Quaternion localRot = new Quaternion(ihi.localRotation_x, ihi.localRotation_y, ihi.localRotation_z, ihi.localRotation_w);

                    HandleEx handle = null;
                    if (true == ihi.sItemFullName.Contains("MyRoomCustomObject"))
                    {
                        int iItemID = int.Parse(ihi.sItemFullName.Split('_')[1]);
                        handle = Super.Items.Items_CreatHandle(iItemID, ihi.sShowName, false, new Vector3(0,0,0), Quaternion.Euler(new Vector3(0, 0, 0)), null);
                    }
                    else if(true == ihi.sItemFullName.Contains('|'))
                    {
                        string[] sDetailedInformation = ihi.sItemFullName.Split('|');
                        if(3 == sDetailedInformation.Length)
                        {
                            if(true == string.Equals("Odogu", sDetailedInformation[0]))
                            {
                                EMES_Items.PhotoBGObjectData_Odogu odoguData = new EMES_Items.PhotoBGObjectData_Odogu();
                                SetFieldValue(odoguData, sDetailedInformation[1], sDetailedInformation[2]);
                                handle = Super.Items.Items_CreateDirectObjectHandle(ihi.sItemName, ihi.sItemFullName, ihi.sCategory, ihi.sShowName, odoguData, null);
                            }
                            else if (true == string.Equals("DeskItem", sDetailedInformation[0]))
                            {
                                EMES_Items.DeskItemData deskItemData = new EMES_Items.DeskItemData();
                                SetFieldValue(deskItemData, sDetailedInformation[1], sDetailedInformation[2]);
                                handle = Super.Items.Items_CreateDirectObjectHandle(ihi.sItemName, ihi.sItemFullName, ihi.sCategory, ihi.sShowName, null, deskItemData);
                            }
                        }
                    }
                    else
                    {
                        if (true == string.Equals("ExternalImage", ihi.sCategory))
                        {
                            string[] options = ihi.sOptions.Split('|');
                            handle = Super.Items.Items_CreateExternalImageHandle(ihi.sItemFullName, ihi.sItemName, options[(int)SceneOptions.Shader], (PrimitiveType)(int.Parse(options[(int)SceneOptions.PrimitiveType])), 
                                                                                 string.Equals("1", options[(int)SceneOptions.ReceiveShadows]), (ShadowCastingMode)(int.Parse(options[(int)SceneOptions.ShadowCastingMode])),
                                                                                 false);
                        }
                        else
                        {
                            handle = Super.Items.Items_CreateDirectObjectHandle(ihi.sItemName, ihi.sItemFullName, ihi.sCategory, ihi.sShowName, null, null);
                        }
                    }

                    if (null != handle)
                    {
#if DEBUG
                        Debuginfo.Log("Success", 2);
#endif

                        Renderer render = handle.parentBone.GetComponent<Renderer>();
                        if(null == render)
                        {
                            render = handle.parentBone.GetComponent<Renderer>();
                            if (null == render)
                            {
                                UnityEngine.Component[] componentsChildren = handle.parentBone.GetComponentsInChildren(typeof(UnityEngine.Component));
                                for (int i = 0; i < componentsChildren.Length; i++)
                                {
                                    render = componentsChildren[i].gameObject.GetComponent<Renderer>();
                                    if (null != render)
                                    {
                                        string[] options = ihi.sOptions.Split('|');
                                        if (((int)SceneOptions.Maxium) == options.Length)
                                        {
                                            SetHandleOptions(handle, options[(int)SceneOptions.Shader]);
                                            SetHandleOptions(handle, string.Equals("1", options[(int)SceneOptions.ReceiveShadows]));
                                            SetHandleOptions(handle, (ShadowCastingMode)(int.Parse(options[(int)SceneOptions.ShadowCastingMode])));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string[] options = ihi.sOptions.Split('|');
                            if (((int)SceneOptions.Maxium) == options.Length)
                            {
                                SetHandleOptions(handle, options[(int)SceneOptions.Shader]);
                                SetHandleOptions(handle, string.Equals("1", options[(int)SceneOptions.ReceiveShadows]));
                                SetHandleOptions(handle, (ShadowCastingMode)(int.Parse(options[(int)SceneOptions.ShadowCastingMode])));
                            }
#if DEBUG
                            else
                            {
                                Debuginfo.Warning("options.Length = " + options.Length.ToString(), 2);
                                for (int i = 0; i < options.Length; i++)
                                {
                                    Debuginfo.Warning("options[" + i.ToString() + "] = " + options[i], 2);
                                }
                            }
#endif
                        }

                        handle.GetParentBone().position = Pos;                        
                        handle.GetParentBone().rotation = Rot;
                        handle.GetParentBone().localPosition = localPos; 
                        //handle.GetParentBone().localRotation = localRot;
                        handle.GetParentBone().localScale = new Vector3(ihi.localScale_x, ihi.localScale_y, ihi.localScale_z);
                    }
                    else
                    {
#if DEBUG
                        Debuginfo.Log("Failed >> " + ihi.sItemFullName + " >> continue", 2);
#endif
                    }
                }
#if DEBUG
                Debuginfo.Log("Sync maidTailsBoneData Done", 2);
#endif
                UnlockBusy();
            }

            Items_UpdateCurrentHandleCount();
            ToggleWindow(true);
            GC.Collect();
        }

        private void Scene_Delete_Button_Click(object sender, EventArgs e)
        {
            string selectedName = Scene_Select_ComboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            LockBusy();
            Super.sceneManagement.MaidSceneData.RemoveAt(Scene_Select_ComboBox.SelectedIndex);

            if (0 == Super.sceneManagement.MaidSceneData.Count)
            {
                Scene_Select_ComboBox.Items.Clear();
                Scene_Select_ComboBox.Items.Add("無");
                Scene_PictureBox.Image = null;
            }
            else
            {
                Scene_Select_ComboBox.Items.RemoveAt(Scene_Select_ComboBox.SelectedIndex);
            }
            UnlockBusy();
            Scene_Select_ComboBox.SelectedIndex = 0;
            Scene_Select_ComboBox.Select();
        }


        private void Scene_Import_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            EMES_openFileDialog.Filter = "XML(*.xml)|*.xml";
            if (DialogResult.OK == EMES_openFileDialog.ShowDialog())
            {
                string sFullPathXML = EMES_openFileDialog.FileName;
                string sPath = Path.GetDirectoryName(sFullPathXML);
                string sNameNoExt = Path.GetFileNameWithoutExtension(sFullPathXML);

                EMES_SceneManagement.SceneDataNew sd = Super.sceneManagement.ImportSceneData(sFullPathXML, sPath, sNameNoExt);
                if(null != sd)
                    SceneManagement_UpdateWindow(sd, sd.ScreenShotName);
            }
        }

        private void Scene_Export_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if ("無" == Scene_Select_ComboBox.SelectedItem.ToString())
                return;

            EMES_saveFileDialog.Filter = "XML(*.xml)|*.xml";
            EMES_saveFileDialog.DefaultExt = "xml";
            EMES_saveFileDialog.InitialDirectory = Super.GetConfigDirectory();

            if (DialogResult.OK == EMES_saveFileDialog.ShowDialog())
            {
                string sFullPathXML = EMES_saveFileDialog.FileName;
                string sPath = Path.GetDirectoryName(sFullPathXML);
                string sNameNoExt = Path.GetFileNameWithoutExtension(sFullPathXML);
                string sFullPathPNG = sPath + "\\" + sNameNoExt + ".png";
                
                EMES_SceneManagement.SceneDataNew sd = Super.sceneManagement.MaidSceneData[Scene_Select_ComboBox.SelectedIndex];

                if(true == System.IO.File.Exists(sFullPathXML))
                {
                    Debuginfo.Log("既存のXMLを削除する =" + sFullPathXML, 1);
                    System.IO.File.Delete(sFullPathXML);
                }

                if (true == System.IO.File.Exists(sFullPathPNG))
                {
                    Debuginfo.Log("既存のPNGを削除する =" + sFullPathPNG, 1);
                    System.IO.File.Delete(sFullPathPNG);
                }

                System.IO.FileStream fsPNG = new System.IO.FileStream(sFullPathPNG, FileMode.OpenOrCreate);
                Scene_PictureBox.Image.Save(fsPNG, System.Drawing.Imaging.ImageFormat.Png);

                Super.sceneManagement.ExportSceneData(sFullPathXML, sPath, sNameNoExt, sd);
            }
        }

        private void Light_Main_trackBar_Scroll(object sender, EventArgs e)
        {
            Super.camPlus.SetMainLight(
                new Vector3(((float)Light_Main_X_trackBar.Value) / 100, ((float)Light_Main_Y_trackBar.Value) / 100, 18),
                new UnityEngine.Color(((float)Light_Main_Color_pictureBox.BackColor.R) / 255, ((float)Light_Main_Color_pictureBox.BackColor.G) / 255, ((float)Light_Main_Color_pictureBox.BackColor.B) / 255),
                ((float)Light_Main_Brightness_trackBar.Value) / 10000,
                ((float)Light_Main_Shadow_trackBar.Value) / 10000
                );
        }

        private void Light_Main_Reset_button_Click(object sender, EventArgs e)
        {
            Super.camPlus.ResetMainLight();

            SetTrackBarValue(Light_Main_X_trackBar, 40 * 100);
            SetTrackBarValue(Light_Main_Y_trackBar, 180 * 100);
            SetTrackBarValue(Light_Main_Brightness_trackBar, (int)(0.95f * 10000));
            SetTrackBarValue(Light_Main_Shadow_trackBar, (int)(0.098f * 10000));
            Light_Main_Color_pictureBox.BackColor = System.Drawing.Color.White;
        }

        private void Light_Main_Color_pictureBox_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            ignoreShowWindow = true;
            BackGroundColorDialog.Color = Light_Main_Color_pictureBox.BackColor;
            BackGroundColorDialog.ShowDialog();
            ignoreShowWindow = false;

            Light_Main_Color_pictureBox.BackColor = BackGroundColorDialog.Color;

            Light_Main_trackBar_Scroll(sender, EventArgs.Empty);
        }

        private void SubLight_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string selectedName = SubLight_index_comboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            Super.camPlus.SetSubLight(
                SubLight_index_comboBox.SelectedIndex, SubLight_enable_checkBox.Checked, (LightType)SubLight_type_comboBox.SelectedIndex,
                ((float)SubLight_range_trackBar.Value) / 100, ((float)SubLight_brightness_trackBar.Value) / 100, ((float)SubLight_angle_trackBar.Value) / 100);

            SubLight_angle_trackBar.Visible = true;

            if (0 != SubLight_type_comboBox.SelectedIndex)
            {
                SubLight_angle_trackBar.Visible = false;
            }
        }

        private void SubLight_add_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string selectedName = SubLight_index_comboBox.SelectedItem.ToString();

            int Index;
            GameObject subLight;
            Super.camPlus.CreateSubLight((LightType)SubLight_type_comboBox.SelectedIndex, out subLight, out Index);

            if ("無" == selectedName)
            {
                SubLight_index_comboBox.Items.Clear();
            }
            SubLight_index_comboBox.Items.Add((Index + 1).ToString());
            SubLight_index_comboBox.SelectedIndex = Index;
            SubLight_index_comboBox.Select();

            bool bEnable;
            LightType lt;
            int range;
            int brightness;
            int angle;

            Super.camPlus.GetSubLight(SubLight_index_comboBox.SelectedIndex, out bEnable, out lt, out range, out brightness, out angle);

            SubLight_enable_checkBox.Checked = bEnable;
            SubLight_type_comboBox.SelectedIndex = (int)lt;
            SubLight_type_comboBox.Select();
            SetTrackBarValue(SubLight_range_trackBar, range);
            SetTrackBarValue(SubLight_brightness_trackBar, brightness);
            SetTrackBarValue(SubLight_angle_trackBar, angle);

            HandleEx handle;
            Super.MaidIK.IK_AddSubLightPoint(subLight, Index, out handle);
            Items_UpdateCurrentHandleCount();
        }

        private void SubLight_remove_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string selectedName = SubLight_index_comboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            Super.MaidIK.IK_RemoveSubLightPoint(CamPlus_SubLightHandleList[SubLight_index_comboBox.SelectedIndex]);
            Super.camPlus.RemoveSubLight(Super.camPlus.SubLightList[SubLight_index_comboBox.SelectedIndex]);
            LockBusy();
            if (0 == Super.camPlus.SubLightList.Count)
            {
                SubLight_index_comboBox.Items.Clear();
                SubLight_index_comboBox.Items.Add("無");
            }
            else
            {
                Refresh_SubLight_list();
            }
            SubLight_index_comboBox.SelectedIndex = 0;
            UnlockBusy();
            SubLight_index_comboBox.Select();
            Items_UpdateCurrentHandleCount();
        }

        private void SubLight_index_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string selectedName = SubLight_index_comboBox.SelectedItem.ToString();

            if ("無" == selectedName)
                return;

            LockBusy();
            bool bEnable;
            LightType lt;
            int range;
            int brightness;
            int angle;
            Super.camPlus.GetSubLight(SubLight_index_comboBox.SelectedIndex, out bEnable, out lt, out range, out brightness, out angle);

            SubLight_enable_checkBox.Checked = bEnable;
            SubLight_type_comboBox.SelectedIndex = (int)lt;
            SubLight_type_comboBox.Select();
            SetTrackBarValue(SubLight_range_trackBar, range);
            SetTrackBarValue(SubLight_brightness_trackBar, brightness);
            SetTrackBarValue(SubLight_angle_trackBar, angle);
            SubLight_index_comboBox.Select();
            UnlockBusy();
        }

        private void SubLight_ShowLightPoint_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            foreach (KeyValuePair<HandleEx, EMES_MaidIK.UniversalPoint> lp in Super.MaidIK.SublightPoint.ToList())
            {
                Super.MaidIK.SublightPoint[lp.Key].target.SetActive(SubLight_ShowLightPoint_checkBox.Checked);
            }
        }

        private void MessageWindow_enable_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            GameObject gameObject = GameObject.Find("SystemUI Root/Manager_SystemUI/MessageWindowMgr");
            MessageWindowMgr component = gameObject.GetComponent<MessageWindowMgr>();
            string name = "";
            if (true == MessageWindow_enable_checkBox.Checked)
            {
                switch (MessageWindow_maidname_comboBox.SelectedIndex)
                {
                    case 0:
                        name = CurrentSelectedMaid.status.callName;
                        break;
                    case 1:
                        name = CurrentSelectedMaid.status.firstName;
                        break;
                    case 2:
                        name = CurrentSelectedMaid.status.lastName;
                        break;
                    case 3:
                        name = MessageWindow_maidname_textBox.Text;
                        break;
                }
                component.SetText(name, MessageWindow_text_textBox.Text, "se005", 0, AudioSourceMgr.Type.Se);
                component.OpenMessageWindowPanel();
            }
            else
            {
                component.ClearText();
                component.CloseMessageWindowPanel();
            }
        }

        private void MaidPose_Copy_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            int Index = int.Parse(MaidPose_Source_comboBox.SelectedItem.ToString().Split(':')[0]);
            Maid src = CurrentMaidsList[Index];

            CacheBoneDataArray cacheBoneDataArray = src.gameObject.AddComponent<CacheBoneDataArray>();
            cacheBoneDataArray.CreateCache(src.body0.GetBone("Bip01"));
            byte[] anmBinary = cacheBoneDataArray.GetAnmBinary(true, true);

            Super.Yotogi.Yotogi_LoadAnime(CurrentSelectedMaid, "posecopy", anmBinary, false, false);
            CurrentSelectedMaid.body0.m_Bones.GetComponent<Animation>().Play("posecopy");

            if (true == MaidPose_CopyPosition_checkBox.Checked)
            {
                CurrentSelectedMaid.SetPos(src.GetPos());
                CurrentSelectedMaid.SetRot(src.GetRot());
            }
        }

        private void MaidTails_Scale_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            //if (e.KeyChar == '-' && (sender as TextBox).Text.Length > 0)
            //e.Handled = true;
        }

        private void MaidTails_Scale_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && MT.GetBoneTails().Count > 0)
            {
                if (true == IsDigitsOnly(MaidTails_Scale_textBox.Text))
                {
                    ((Transform)(MaidTails_listBox.SelectedValue)).localScale = new Vector3(float.Parse(MaidTails_Scale_textBox.Text), float.Parse(MaidTails_Scale_textBox.Text), float.Parse(MaidTails_Scale_textBox.Text));
                }
            }
        }

        private void MaidTails_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (-1 == MaidTails_listBox.SelectedIndex)
                return;

            MaidTails_TryCreateIK_checkBox.Checked = false;

            MaidTails_Cat_groupBox.Visible = true;
            MaidTails_groupBox.Visible = true;

            Transform trBone = (Transform)(MaidTails_listBox.SelectedValue);

#if DEBUG
            Debuginfo.Log("trBone = " + trBone, 2);
#endif

            MaidTails_groupBox.Text = MT.GetBoneShortName(trBone);
            MaidTails_Scale_textBox.Text = trBone.localScale.x.ToString("F4");
            MT.CreateTailHandle(trBone);
        }

        private void MaidTails_UnSelect_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            MaidTails_listBox.SelectedIndex = -1;
            MaidTails_listBox.ClearSelected();
            MT.DestoryTailHandle();

            MaidTails_Cat_groupBox.Visible = false;
            MaidTails_groupBox.Visible = false;
        }

        private void MaidTails_ShowPos_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            Dictionary<string, Transform> boneData = MT.GetBoneTails();
            if (boneData.Count > 0)
            {
                Transform trBone = (Transform)(MaidTails_listBox.SelectedValue);
                MaidTails_groupBox.Text = MT.GetBoneShortName(trBone);
                MaidTails_Scale_textBox.Text = trBone.localScale.x.ToString("F4");
                MT.CreateTailHandle(trBone);

                if (true == MaidTails_ShowPos_checkBox.Checked)
                {
                    MT.SetHandleMode(MaidTailsLite.MaidTailsHandleMethod.Position);
                    MaidTails_ShowRot_checkBox.Checked = false;
                }

                if (false == MaidTails_ShowPos_checkBox.Checked && false == MaidTails_ShowRot_checkBox.Checked)
                {
                    MT.SetHandleMode(MaidTailsLite.MaidTailsHandleMethod.Universal);
                }
            }
        }

        private void MaidTails_ShowRot_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            Dictionary<string, Transform> boneData = MT.GetBoneTails();
            if (boneData.Count > 0)
            {
                Transform trBone = (Transform)(MaidTails_listBox.SelectedValue);
                MaidTails_groupBox.Text = MT.GetBoneShortName(trBone);
                MaidTails_Scale_textBox.Text = trBone.localScale.x.ToString("F4");
                MT.CreateTailHandle(trBone);

                if (true == MaidTails_ShowRot_checkBox.Checked)
                {
                    MT.SetHandleMode(MaidTailsLite.MaidTailsHandleMethod.Rotation);
                    MaidTails_ShowPos_checkBox.Checked = false;
                }

                if (false == MaidTails_ShowPos_checkBox.Checked && false == MaidTails_ShowRot_checkBox.Checked)
                {
                    MT.SetHandleMode(MaidTailsLite.MaidTailsHandleMethod.Universal);
                }
            }
        }

        private void MaidTails_CatCopy_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.CopyBonesInfoAll();
            }
        }

        private void MaidTails_CatPaste_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.PasteBonesInfoAll();
            }
        }

        private void MaidTails_CatReset_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.ResetBoneInfoAll();
            }
        }

        private void MaidTails_Copy_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.CopyBonesInfo((Transform)(MaidTails_listBox.SelectedValue));
            }
        }

        private void MaidTails_Paste_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.PasteBonesInfo((Transform)(MaidTails_listBox.SelectedValue));
            }
        }

        private void MaidTails_Reset_button_Click(object sender, EventArgs e)
        {
            if (MT.GetBoneTails().Count > 0)
            {
                MT.ResetBoneInfo((Transform)(MaidTails_listBox.SelectedValue));
            }
        }

        private void MaidTails_TryCreateIK_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
            {
                return;
            }

            if (true == MaidTails_TryCreateIK_checkBox.Checked && -1 != MaidTails_listBox.SelectedIndex)
            {
                int ChainCount = MaidTails_TryCreateIKChain_comboBox.SelectedIndex + 1;
                Transform root = (Transform)(MaidTails_listBox.SelectedValue) ?? null;

                if(null != root)
                {
#if DEBUG
                    Debuginfo.Log("IK_AutoIK_Attach root =" + root.name, 2);
                    Debuginfo.Log("IK_AutoIK_Attach ChainCount =" + ChainCount.ToString(), 2);
#endif
                    if (0 == Super.MaidIK.IK_AutoIK_Attach(GetCurrentMaidStockID(), root, ChainCount, -1f))
                    {
                        MaidTails_DisableAutoIK();
                    }
                }
            }
            else
            {
                MaidTails_DisableAutoIK();
            }

        }

        private void MaidTails_TryCreateIKChain_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            MaidTails_TryCreateIK_checkBox_CheckedChanged(sender, EventArgs.Empty);
        }

        private void CameraPos_QuickLoad_button_Click(object sender, EventArgs e)
        {
            Super.camPlus.CameraQuickLoad();
        }

        private void CameraPos_QuickSave_button_Click(object sender, EventArgs e)
        {
            Super.camPlus.CameraQuickSave();
        }

        private void Camera_UI_Hide_CheckedChanged(object sender, EventArgs e)
        {
            Super.sceneManagement.HideUI(Camera_UI_Hide.Checked);
        }

        private void Gravity_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            TrackBar tb = ((TrackBar)sender);
            string trackName = tb.Name;
            string textName = trackName.Replace("_trackBar", "_textBox");
            float value = ((float)tb.Value / 1000);

            (Controls.Find(textName, true)[0] as TextBox).Text = value.ToString();

            if (true == Gravity_SyncAll_checkBox.Checked)
            {
                for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                {
                    UpdateMaidGravity(Index);
                }
            }
            else
            {
                UpdateMaidGravity(CurrentSelectedMaidIndex);
            }
        }

        private void Gravity_xyz_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            if (e.KeyChar == '-' && (sender as TextBox).Text.Length > 0)
                e.Handled = true;
        }

        private void Gravity_sy_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            //if (e.KeyChar == '-' && (sender as TextBox).Text.Length > 0)
            //e.Handled = true;
        }

        private void Gravity_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string textName = ((TextBox)sender).Name;
                string trackName = textName.Replace("_textBox", "_trackBar");
                string value = ((TextBox)sender).Text;
                if (true == IsDigitsOnly(value))
                {
                    LockBusy();
                    (Controls.Find(trackName, true)[0] as TrackBar).Value = (int)(float.Parse(value) * 1000);
                    UnlockBusy();

                    if (true == Gravity_SyncAll_checkBox.Checked)
                    {
                        for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                        {
                            UpdateMaidGravity(Index);
                        }
                    }
                    else
                    {
                        UpdateMaidGravity(CurrentSelectedMaidIndex);
                    }
                }
            }
        }

        private void Gravity_SyncAll_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == Gravity_SyncAll_checkBox.Checked)
            {
                for (int Index = 0; Index < CurrentMaidsStockID.Count; Index++)
                {
                    UpdateMaidGravity(Index);
                }
            }
        }


        private void Settings_HotkeyCameraMovement_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            foreach (string Key in Enum.GetNames(typeof(Settings_HotkeyCameraMovement)))
            {
                string sKey = GetFieldValue<SettingsXML, string>(Super.settingsXml, "sHotkey" + Key);
                (Controls.Find("Settings_Hotkey" + Key + "_TextBox", true)[0] as TextBox).Enabled = !Settings_HotkeyCameraMovement_checkBox.Checked;
            }
        }

        private void Items_HandleMethod_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (0 == Super.Items.Items_ItemHandle.Count)
                return;

            string senderName = ((RadioButton)sender).Name;

            if(true == senderName.Contains("All"))
            {
                LockBusy();
                Items_All_radioButton.Checked = true;
                Items_Single_radioButton.Checked = false;
                Items_Multi_radioButton.Checked = false;
                Items_HandledObjects_listBox.SelectionMode = SelectionMode.One;
                Items_SubObjects_listBox.SelectionMode = SelectionMode.One;
                UnlockBusy();
                Items_Settings_tabControl.Enabled = true;
            }
            else if (true == senderName.Contains("Single"))
            {
                LockBusy();
                Items_All_radioButton.Checked = false;
                Items_Single_radioButton.Checked = true;
                Items_Multi_radioButton.Checked = false;
                Items_HandledObjects_listBox.SelectionMode = SelectionMode.One;
                Items_SubObjects_listBox.SelectionMode = SelectionMode.One;
                UnlockBusy();
                Items_Settings_tabControl.Enabled = true;
            }
            else if (true == senderName.Contains("Multi"))
            {
                LockBusy();
                Items_All_radioButton.Checked = false;
                Items_Single_radioButton.Checked = false;
                Items_Multi_radioButton.Checked = true;
                Items_HandledObjects_listBox.SelectionMode = SelectionMode.MultiExtended;
                Items_SubObjects_listBox.SelectionMode = SelectionMode.MultiExtended;
                UnlockBusy();
                Items_Settings_tabControl.Enabled = false;
            }
        }

        private void Items_TryCreateSubHandle_button_Click(object sender, EventArgs e)
        {
            if (true == Items_isItemHandleMethodAllorSingle() && Items_HandledObjects_listBox.SelectedIndex >= 0)
            {
                HandleEx posHandle = (HandleEx)Items_HandledObjects_listBox.SelectedValue;

                if (false == posHandle.CheckParentAlive())
                {
#if DEBUG
                    Debuginfo.Log("Items_TryCreateSubHandle_button_Click() posHandle.goHandleMasterObject == null reinit", 2);
#endif
                    Items_Control_groupBox.Enabled = false;
                    Items_Settings_tabControl.Enabled = false;
                    Items_Handle_Visible_checkBox.Enabled = false;

                    if (true == Super.Parts.Init(CurrentSelectedMaid))
                    {
                        Items_UpdateCurrentHandleCount();
                        Items_Single_radioButton.Checked = true;
                    }

                    return;
                }

                LockBusy();
                GetSlotBodyHit(Items_SubObjects_SlotBodyHit_checkBox);

                subItems_list_tabPage2.Enabled = CheckSubItems(posHandle);
                if (true == subItems_list_tabPage2.Enabled)
                {
                    Items_List_tabControl.SelectedTab = subItems_list_tabPage2;
                    Items_List_tabControl.Select();
                    Items_SubObjects_listBox.ClearSelected();
                }
                Super.Items.Items_Sub_UpdateSelectedHandleExList(Items_SubObjects_listBox.SelectedItems);
                UnlockBusy();
            }
            else
            {
                subItems_list_tabPage2.Enabled = false;
            }
        }

        private void Items_HandledObjects_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Items_UpdateCurrentHandleFunc();
        }

        private void Items_SubObjects_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            Items_UpdateCurrentHandleFunc();
        }

        private void Items_Handle_Button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name;
            GameObject gameObject;
            HandleEx posHandle;

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                {
                    posHandle = (HandleEx)Items_SubObjects_listBox.SelectedValue;
                }
                else
                {
                    return;
                }
            }
            else
            {
                posHandle = (HandleEx)Items_HandledObjects_listBox.SelectedValue;
            }
            gameObject = posHandle.parentBone;

            if (true == senderName.Contains("_ResetRos_"))
            {
                Items_Handle_Scale_textBox.Text = "1.0";

                if (true == Items_Handle_Pos_checkBox.Checked)
                    gameObject.transform.position = gameObject.transform.parent.position;

                if (true == Items_Handle_Rot_checkBox.Checked)
                {
                    gameObject.transform.rotation = gameObject.transform.parent.rotation;
                    //gameObject.transform.localRotation = gameObject.transform.parent.localRotation;
                }

                if (true == Items_Handle_Scale_checkBox.Checked)
                    gameObject.transform.localScale = new Vector3(1, 1, 1);

                Items_UpdateItemHandleScaleInfo(gameObject);
            }
            else if (true == senderName.Contains("_ResetRot_"))
            {
                gameObject.transform.rotation = gameObject.transform.parent.rotation;
                gameObject.transform.localRotation = gameObject.transform.parent.localRotation;
                Items_UpdateItemHandleScaleInfo(gameObject);
            }
            else if (true == senderName.Contains("_Paste_"))
            {
                Super.Items.Items_PasteHandlePosRotScale(gameObject, Items_Handle_Pos_checkBox.Checked, Items_Handle_Rot_checkBox.Checked, Items_Handle_Scale_checkBox.Checked);
                Items_UpdateItemHandleScaleInfo(gameObject);
            }
            else if (true == senderName.Contains("_Copy_"))
            {
                Super.Items.Items_CopyHandlePosRotScale(gameObject);
            }
            else if (true == senderName.Contains("_Remove_"))
            {
                if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                {
                    Debuginfo.Log("代わりに非表示を使用して子オブジェクトを削除", 1);
                    return;
                }
                else
                {
                    if (true == posHandle.sCategory.Equals("MaidPartsHandle"))
                    {
                        Debuginfo.Log("代わりにメイドパーツ編集のチェックを外します", 1);
                        return;
                    }
                }

                if (true == Items_isItemHandleMethodAllorSingle())
                {
                    if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    {
#if DEBUG
                        Debuginfo.Log("Remove " + ((HandleEx)Items_HandledObjects_listBox.SelectedValue).sItemName, 2);
#endif
                        Super.Items.Items_RemoveHandle((HandleEx)Items_HandledObjects_listBox.SelectedValue);
                    }
                }
                else
                {
                    foreach (HandleEx kvpHandle in Super.Items.Items_selectedHandles.ToList())
                    {
#if DEBUG
                        Debuginfo.Log("Remove List " + kvpHandle.sItemName, 2);
#endif
                        Super.Items.Items_RemoveHandle(kvpHandle);
                    }
                }
                Items_ClearSubItems();
                Items_UpdateCurrentHandleCount();
            }
            else if (true == senderName.Contains("_Active_"))
            {

                if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                {
                    return;
                }

                if (true == Items_isItemHandleMethodAllorSingle())
                {
                    if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    {
                        Super.Items.Items_RenewPrefab((HandleEx)Items_HandledObjects_listBox.SelectedValue);
                    }
                }
                else
                {
                    foreach (HandleEx kvpHandle in Super.Items.Items_selectedHandles.ToList())
                    {
                        Super.Items.Items_RenewPrefab(kvpHandle);
                    }
                }
            }
        }

        private void Items_Handle_Scale_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void Items_Handle_Scale_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && 1 == Items_HandledObjects_listBox.SelectedItems.Count)
            {
                if (true == IsDigitsOnly(Items_Handle_Scale_textBox.Text))
                {
                    if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                    {
                        if (Items_SubObjects_listBox.SelectedIndex >= 0)
                        {
                            ((HandleEx)Items_SubObjects_listBox.SelectedValue).GetParentBone().localScale = Vector3.one * float.Parse(Items_Handle_Scale_textBox.Text);
                        }
                    }
                    else
                    {
                        ((HandleEx)Items_HandledObjects_listBox.SelectedValue).GetParentBone().localScale = Vector3.one * float.Parse(Items_Handle_Scale_textBox.Text);
                    }
                }
            }
        }

        private void Items_Handle_ScaleXYZ_textBox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && 1 == Items_HandledObjects_listBox.SelectedItems.Count)
            {
                if (true == string.Equals((sender as TextBox).Text, "0"))
                {
                    (sender as TextBox).Text = "0.1";
                }

                if (true == IsDigitsOnly(Items_Handle_ScaleX_textBox.Text) && true == IsDigitsOnly(Items_Handle_ScaleY_textBox.Text) && true == IsDigitsOnly(Items_Handle_ScaleZ_textBox.Text))
                {
                    if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                    {
                        if (Items_SubObjects_listBox.SelectedIndex >= 0)
                        {
                            ((HandleEx)Items_SubObjects_listBox.SelectedValue).GetParentBone().localScale = new Vector3(float.Parse(Items_Handle_ScaleX_textBox.Text), float.Parse(Items_Handle_ScaleY_textBox.Text), float.Parse(Items_Handle_ScaleZ_textBox.Text));
                        }
                    }
                    else
                    {
                        ((HandleEx)Items_HandledObjects_listBox.SelectedValue).GetParentBone().localScale = new Vector3(float.Parse(Items_Handle_ScaleX_textBox.Text), float.Parse(Items_Handle_ScaleY_textBox.Text), float.Parse(Items_Handle_ScaleZ_textBox.Text));
                    }
                }
            }
        }

        private void Items_Handle_RotateXYZ_textBox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && 1 == Items_HandledObjects_listBox.SelectedItems.Count)
            {
                if (true == IsDigitsOnly(Items_Handle_RotX_textBox.Text) && true == IsDigitsOnly(Items_Handle_RotY_textBox.Text) && true == IsDigitsOnly(Items_Handle_RotZ_textBox.Text))
                {
                    if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                    {
                        if (Items_SubObjects_listBox.SelectedIndex >= 0)
                        {
                            ((HandleEx)Items_SubObjects_listBox.SelectedValue).GetParentBone().rotation = Quaternion.Euler(new Vector3(
                                    float.Parse(Items_Handle_RotX_textBox.Text), float.Parse(Items_Handle_RotY_textBox.Text), float.Parse(Items_Handle_RotZ_textBox.Text)));
                        }
                    }
                    else
                    {
                        ((HandleEx)Items_HandledObjects_listBox.SelectedValue).GetParentBone().rotation = Quaternion.Euler(new Vector3(
                                float.Parse(Items_Handle_RotX_textBox.Text), float.Parse(Items_Handle_RotY_textBox.Text), float.Parse(Items_Handle_RotZ_textBox.Text)));
                    }
                }
            }
        }

        private void Items_Handle_Pos_textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;

            if (e.KeyChar == '-' && (sender as TextBox).Text.Length > 0)
                e.Handled = true;
        }

        private void Items_Handle_PositionXYZ_textBox(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && MT.GetBoneTails().Count > 0)
            {
                if (true == IsDigitsOnly(Items_Handle_PosX_textBox.Text) && true == IsDigitsOnly(Items_Handle_PosY_textBox.Text) && true == IsDigitsOnly(Items_Handle_PosZ_textBox.Text))
                {
                    if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
                    {
                        if (Items_SubObjects_listBox.SelectedIndex >= 0)
                        {
                            ((HandleEx)Items_SubObjects_listBox.SelectedValue).GetParentBone().position = new Vector3(
                                        float.Parse(Items_Handle_PosX_textBox.Text), float.Parse(Items_Handle_PosY_textBox.Text), float.Parse(Items_Handle_PosZ_textBox.Text));
                        }
                    }
                    else
                    {
                        ((HandleEx)Items_HandledObjects_listBox.SelectedValue).GetParentBone().position = new Vector3(
                                float.Parse(Items_Handle_PosX_textBox.Text), float.Parse(Items_Handle_PosY_textBox.Text), float.Parse(Items_Handle_PosZ_textBox.Text));
                    }
                }
            }
        }

        private void Items_Handle_Visible_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                {
                    ((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone.SetActive(Items_Handle_Visible_checkBox.Checked);
                }
                /*
                else
                {
                    foreach (HandleEx kvpHandle in Super.Items.Items_Sub_selectedHandles.ToList())
                    {
                        kvpHandle.parentBone.SetActive(Items_Handle_Visible_checkBox.Checked);
                    }
                }
                */
            }
            else
            {
                if (true == Items_isItemHandleMethodAllorSingle())
                {
                    if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    {
                        ((HandleEx)Items_HandledObjects_listBox.SelectedValue).parentBone.SetActive(Items_Handle_Visible_checkBox.Checked);
                    }
                }
                else
                {
                    foreach (HandleEx kvpHandle in Super.Items.Items_selectedHandles.ToList())
                    {
                        kvpHandle.parentBone.SetActive(Items_Handle_Visible_checkBox.Checked);
                    }
                }
            }
        }

        private void Items_Handle_CRotate_Button_Click(object sender, EventArgs e)
        {
            string senderName = ((Button)sender).Name.Split('_')[2];

            Action<GameObject> action_Rotate = delegate (GameObject gameObject)
            {
                if ("RX" == senderName)
                {
                    gameObject.transform.rotation *= Quaternion.Euler(new Vector3(gameObject.transform.rotation.x + 90, gameObject.transform.rotation.y, gameObject.transform.rotation.z));
                }
                else if ("RY" == senderName)
                {
                    gameObject.transform.rotation *= Quaternion.Euler(new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y + 90, gameObject.transform.rotation.z));
                }
                else if ("RZ" == senderName)
                {
                    gameObject.transform.rotation *= Quaternion.Euler(new Vector3(gameObject.transform.rotation.x, gameObject.transform.rotation.y, gameObject.transform.rotation.z + 90));
                }
            };

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                {
                    action_Rotate(((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone);
                    Items_UpdateItemHandleScaleInfo(((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone);
                }
            }
            else
            {
                action_Rotate(((HandleEx)Items_HandledObjects_listBox.SelectedValue).parentBone);
                Items_UpdateItemHandleScaleInfo(((HandleEx)Items_HandledObjects_listBox.SelectedValue).parentBone);
            }
        }

        private void Items_HandledObjects_listBox_Key(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Items_HandledObjects_listBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CameraPos_FOV_textbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && (!char.IsDigit(e.KeyChar)) && (e.KeyChar != '.') && (e.KeyChar != '-'))
                e.Handled = true;

            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void CameraPos_FOV_textbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (true == IsDigitsOnly(MaidTails_Scale_textBox.Text))
                {
                    GameMain.Instance.MainCamera.camera.fieldOfView = float.Parse(CameraPos_FOV_textbox.Text);
                    SetTrackBarValue(CameraPos_FOV_trackBar, (int)(float.Parse(CameraPos_FOV_textbox.Text) * 10.0f));
                }
            }
        }

        private void CameraPos_FOV_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            GameMain.Instance.MainCamera.camera.fieldOfView = ((float)CameraPos_FOV_trackBar.Value / 10.0f);
            CameraPos_FOV_textbox.Text = GameMain.Instance.MainCamera.camera.fieldOfView.ToString("F1");
        }

        private void Items_LoadImage_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            EMES_openFileDialog.Filter = "PNG(*.png)|*.png";
            if(DialogResult.OK == EMES_openFileDialog.ShowDialog())
            {
                string sFullPath = EMES_openFileDialog.FileName;
                string sFileName = Path.GetFileName(sFullPath);
                string sShaderName = Items_LoadImage_Shader_comboBox.Items[Items_LoadImage_Shader_comboBox.SelectedIndex].ToString();

                Super.Items.Items_CreateExternalImageHandle(sFullPath, sFileName, sShaderName, (PrimitiveType)Items_LoadImage_Type_comboBox.SelectedIndex, 
                                                            Items_LoadImage_Projection_checkBox.Checked, (ShadowCastingMode)Items_LoadImage_Shadow_comboBox.SelectedIndex,
                                                            true);
                Items_UpdateCurrentHandleCount();
            }
        }

        private void Items_LoadImage_Shader_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                    SetSubItemOptions(((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone, Items_LoadImage_Shader_comboBox.Items[Items_LoadImage_Shader_comboBox.SelectedIndex].ToString());
            }
            else
            {
                if (false == Items_isItemHandleMethodAllorSingle())
                    return;

                if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    SetHandleOptions((HandleEx)Items_HandledObjects_listBox.SelectedValue, Items_LoadImage_Shader_comboBox.Items[Items_LoadImage_Shader_comboBox.SelectedIndex].ToString());
            }
        }

        private void Items_LoadImage_Shadow_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                    SetSubItemOptions(((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone, (ShadowCastingMode)Items_LoadImage_Shadow_comboBox.SelectedIndex);
            }
            else
            {
                if (false == Items_isItemHandleMethodAllorSingle())
                    return;

                if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    SetHandleOptions((HandleEx)Items_HandledObjects_listBox.SelectedValue, (ShadowCastingMode)Items_LoadImage_Shadow_comboBox.SelectedIndex);
            }
        }

        private void Items_LoadImage_Projection_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if(Items_SubObjects_listBox.SelectedIndex >= 0)
                    SetSubItemOptions(((HandleEx)Items_SubObjects_listBox.SelectedValue).parentBone, Items_LoadImage_Projection_checkBox.Checked);
            }
            else
            {
                if (false == Items_isItemHandleMethodAllorSingle())
                    return;

                if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    SetHandleOptions((HandleEx)Items_HandledObjects_listBox.SelectedValue, Items_LoadImage_Projection_checkBox.Checked);
            }
        }

        private void MaidOffset_PosX_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && MT.GetBoneTails().Count > 0)
            {
                if (true == IsDigitsOnly(MaidOffset_PosX_textBox.Text) && true == IsDigitsOnly(MaidOffset_PosY_textBox.Text) && true == IsDigitsOnly(MaidOffset_PosZ_textBox.Text))
                {
                    CurrentSelectedMaid.SetPos(new Vector3(float.Parse(MaidOffset_PosX_textBox.Text), float.Parse(MaidOffset_PosY_textBox.Text), float.Parse(MaidOffset_PosZ_textBox.Text)));
                }
            }
        }

        private void MaidOffset_RotX_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && MT.GetBoneTails().Count > 0)
            {
                if (true == IsDigitsOnly(MaidOffset_RotX_textBox.Text) && true == IsDigitsOnly(MaidOffset_RotY_textBox.Text) && true == IsDigitsOnly(MaidOffset_RotZ_textBox.Text))
                {
                    CurrentSelectedMaid.SetRot(new Vector3(float.Parse(MaidOffset_RotX_textBox.Text), float.Parse(MaidOffset_RotY_textBox.Text), float.Parse(MaidOffset_RotZ_textBox.Text)));
                }
            }
        }

        private void MaidOffset_Scale_textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && MT.GetBoneTails().Count > 0)
            {
                if (true == IsDigitsOnly(MaidOffset_Scale_textBox.Text))
                {
                    CurrentSelectedMaid.body0.transform.localScale = (new Vector3(float.Parse(MaidOffset_Scale_textBox.Text), float.Parse(MaidOffset_Scale_textBox.Text), float.Parse(MaidOffset_Scale_textBox.Text)));
                }
            }
        }

        private void MaidOffset_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name;
            if (true == senderName.Contains("_Reset_"))
            {
                if (true == MaidOffset_Pos_checkBox.Checked)
                {
                    CurrentSelectedMaid.SetPos(new Vector3(0, 0, 0));
                }

                if (true == MaidOffset_Rot_checkBox.Checked)
                {
                    CurrentSelectedMaid.SetRot(new Vector3(0, 0, 0));
                }

                if (true == MaidOffset_Scale_checkBox.Checked)
                {
                    CurrentSelectedMaid.body0.transform.localScale = new Vector3(1, 1, 1);
                }

                if (true == MaidOffset_Pose_checkBox.Checked)
                {
                    Super.Pose.Pose_Reset(CurrentSelectedMaid, EMES_Pose.PoseMethod.ALL);
                }

                MaidOffset_UpdateOffsetInfo(CurrentSelectedMaid);
            }
            else if (true == senderName.Contains("_Paste_"))
            {
                MaidOffset_PasteMaidOffset(CurrentSelectedMaid);
                MaidOffset_UpdateOffsetInfo(CurrentSelectedMaid);
            }
            else if (true == senderName.Contains("_Copy_"))
            {
                MaidOffset_CopyMaidOffset(CurrentSelectedMaid);
            }
        }

        private void MaidMirror_All_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name;

            if (true == senderName.Contains("_All_"))
            {
                Super.Pose.Pose_MirrorAll(CurrentSelectedMaid, EMES_Pose.PoseMethod.ALL);
            }
            else if (true == senderName.Contains("_Upper_"))
            {
                Super.Pose.Pose_MirrorAll(CurrentSelectedMaid, EMES_Pose.PoseMethod.Upper);
            }
            else if (true == senderName.Contains("_Lower_"))
            {
                Super.Pose.Pose_MirrorAll(CurrentSelectedMaid, EMES_Pose.PoseMethod.Lower);
            }
            else if (true == senderName.Contains("_Filgers_"))
            {
                Super.Pose.Pose_MirrorAll(CurrentSelectedMaid, EMES_Pose.PoseMethod.Fingers);
            }
            else if (true == senderName.Contains("_Toe_"))
            {
                Super.Pose.Pose_MirrorAll(CurrentSelectedMaid, EMES_Pose.PoseMethod.Toes);
            }

        }

        private void MaidReset_All_button_Click(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((Button)sender).Name;

            if (true == senderName.Contains("_All_"))
            {
                Super.Pose.Pose_Reset(CurrentSelectedMaid, EMES_Pose.PoseMethod.ALL);
            }
            else if (true == senderName.Contains("_Upper_"))
            {
                Super.Pose.Pose_Reset(CurrentSelectedMaid, EMES_Pose.PoseMethod.Upper);
            }
            else if (true == senderName.Contains("_Lower_"))
            {
                Super.Pose.Pose_Reset(CurrentSelectedMaid, EMES_Pose.PoseMethod.Lower);
            }
        }

        private void FingerBlend_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((TrackBar)sender).Name;
            bool bLeft = senderName.Contains("_Left_");
            bool bOpen = senderName.Contains("_Open_");
            bool bSync = false;

            Action ROtoLO = delegate ()
            {
                if (true == FingerBlend_Sync_Open_checkBox.Checked) //RO->LO
                {
                    FingerBlend_Left_Open_trackBar.Value = FingerBlend_Right_Open_trackBar.Value;
                    if (true == FingerBlend_Left_Sync_checkBox.Checked)  //LO->LF
                    {
                        FingerBlend_Left_Fist_trackBar.Value = FingerBlend_Left_Open_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action RFtoLF = delegate ()
            {
                if (true == FingerBlend_Sync_Fist_checkBox.Checked) //RF->LF
                {
                    FingerBlend_Left_Fist_trackBar.Value = FingerBlend_Right_Fist_trackBar.Value;
                    if (true == FingerBlend_Left_Sync_checkBox.Checked) //LF->LO
                    {
                        FingerBlend_Left_Open_trackBar.Value = FingerBlend_Left_Fist_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action LOtoRO = delegate ()
            {
                if (true == FingerBlend_Sync_Open_checkBox.Checked) //LO->RO
                {
                    FingerBlend_Right_Open_trackBar.Value = FingerBlend_Left_Open_trackBar.Value;
                    if (true == FingerBlend_Right_Sync_checkBox.Checked)  //RO->RF
                    {
                        FingerBlend_Right_Fist_trackBar.Value = FingerBlend_Left_Open_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action LFtoRF = delegate ()
            {
                if (true == FingerBlend_Sync_Fist_checkBox.Checked) //LF->RF
                {
                    FingerBlend_Right_Fist_trackBar.Value = FingerBlend_Left_Fist_trackBar.Value;
                    if (true == FingerBlend_Right_Sync_checkBox.Checked) //RF->RO
                    {
                        FingerBlend_Right_Open_trackBar.Value = FingerBlend_Right_Fist_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            LockBusy();
            if (true == bLeft)
            {
                if (true == FingerBlend_Left_Sync_checkBox.Checked)
                {
                    if (true == bOpen) //LO->LF
                    {
                        FingerBlend_Left_Fist_trackBar.Value = FingerBlend_Left_Open_trackBar.Value;
                        LOtoRO();
                    }
                    else //LF->LO
                    {
                        FingerBlend_Left_Open_trackBar.Value = FingerBlend_Left_Fist_trackBar.Value;
                        LFtoRF();
                    }
                }
                else
                {
                    if (true == bOpen)
                    {
                        LOtoRO();
                    }
                    else
                    {
                        LFtoRF();
                    }
                }
                Super.Pose.Pose_FingerOpenAndClose(CurrentSelectedMaid, FingerBlend_Left_Open_trackBar.Value, FingerBlend_Left_Fist_trackBar.Value, false);

                if (true == bSync)
                {
                    Super.Pose.Pose_FingerOpenAndClose(CurrentSelectedMaid, FingerBlend_Right_Open_trackBar.Value, FingerBlend_Right_Fist_trackBar.Value, true);
                }
            }
            else
            {
                if (true == FingerBlend_Right_Sync_checkBox.Checked)
                {
                    if (true == bOpen) //RO->RF
                    {
                        FingerBlend_Right_Fist_trackBar.Value = FingerBlend_Right_Open_trackBar.Value;
                        ROtoLO();
                    }
                    else //RF->RO
                    {
                        FingerBlend_Right_Open_trackBar.Value = FingerBlend_Right_Fist_trackBar.Value;
                        RFtoLF();
                    }
                }
                else
                {
                    if (true == bOpen)
                    {
                        ROtoLO();
                    }
                    else
                    {
                        RFtoLF();
                    }
                }
                Super.Pose.Pose_FingerOpenAndClose(CurrentSelectedMaid, FingerBlend_Right_Open_trackBar.Value, FingerBlend_Right_Fist_trackBar.Value, true);

                if (true == bSync)
                {
                    Super.Pose.Pose_FingerOpenAndClose(CurrentSelectedMaid, FingerBlend_Left_Open_trackBar.Value, FingerBlend_Left_Fist_trackBar.Value, false);
                }
            }

            UnlockBusy();
        }

        private void ToeBlend_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            string senderName = ((TrackBar)sender).Name;
            bool bLeft = senderName.Contains("_Left_");
            bool bOpen = senderName.Contains("_Open_");
            bool bSync = false;

            Action ROtoLO = delegate ()
            {
                if (true == ToeBlend_Sync_Open_checkBox.Checked) //RO->LO
                {
                    ToeBlend_Left_Open_trackBar.Value = ToeBlend_Right_Open_trackBar.Value;
                    if (true == ToeBlend_Left_Sync_checkBox.Checked)  //LO->LF
                    {
                        ToeBlend_Left_Fist_trackBar.Value = ToeBlend_Left_Open_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action RFtoLF = delegate ()
            {
                if (true == ToeBlend_Sync_Fist_checkBox.Checked) //RF->LF
                {
                    ToeBlend_Left_Fist_trackBar.Value = ToeBlend_Right_Fist_trackBar.Value;
                    if (true == ToeBlend_Left_Sync_checkBox.Checked) //LF->LO
                    {
                        ToeBlend_Left_Open_trackBar.Value = ToeBlend_Left_Fist_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action LOtoRO = delegate ()
            {
                if (true == ToeBlend_Sync_Open_checkBox.Checked) //LO->RO
                {
                    ToeBlend_Right_Open_trackBar.Value = ToeBlend_Left_Open_trackBar.Value;
                    if (true == ToeBlend_Right_Sync_checkBox.Checked)  //RO->RF
                    {
                        ToeBlend_Right_Fist_trackBar.Value = ToeBlend_Left_Open_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            Action LFtoRF = delegate ()
            {
                if (true == ToeBlend_Sync_Fist_checkBox.Checked) //LF->RF
                {
                    ToeBlend_Right_Fist_trackBar.Value = ToeBlend_Left_Fist_trackBar.Value;
                    if (true == ToeBlend_Right_Sync_checkBox.Checked) //RF->RO
                    {
                        ToeBlend_Right_Open_trackBar.Value = ToeBlend_Right_Fist_trackBar.Value;
                    }
                    bSync = true;
                }
            };

            LockBusy();
            if (true == bLeft)
            {
                if (true == ToeBlend_Left_Sync_checkBox.Checked)
                {
                    if (true == bOpen) //LO->LF
                    {
                        ToeBlend_Left_Fist_trackBar.Value = ToeBlend_Left_Open_trackBar.Value;
                        LOtoRO();
                    }
                    else //LF->LO
                    {
                        ToeBlend_Left_Open_trackBar.Value = ToeBlend_Left_Fist_trackBar.Value;
                        LFtoRF();
                    }
                }
                else
                {
                    if (true == bOpen)
                    {
                        LOtoRO();
                    }
                    else
                    {
                        LFtoRF();
                    }
                }
                Super.Pose.Pose_ToeOpenAndClose(CurrentSelectedMaid, ToeBlend_Left_Open_trackBar.Value, ToeBlend_Left_Fist_trackBar.Value, false);

                if (true == bSync)
                {
                    Super.Pose.Pose_ToeOpenAndClose(CurrentSelectedMaid, ToeBlend_Right_Open_trackBar.Value, ToeBlend_Right_Fist_trackBar.Value, true);
                }
            }
            else
            {
                if (true == ToeBlend_Right_Sync_checkBox.Checked)
                {
                    if (true == bOpen) //RO->RF
                    {
                        ToeBlend_Right_Fist_trackBar.Value = ToeBlend_Right_Open_trackBar.Value;
                        ROtoLO();
                    }
                    else //RF->RO
                    {
                        ToeBlend_Right_Open_trackBar.Value = ToeBlend_Right_Fist_trackBar.Value;
                        RFtoLF();
                    }
                }
                else
                {
                    if (true == bOpen)
                    {
                        ROtoLO();
                    }
                    else
                    {
                        RFtoLF();
                    }
                }
                Super.Pose.Pose_ToeOpenAndClose(CurrentSelectedMaid, ToeBlend_Right_Open_trackBar.Value, ToeBlend_Right_Fist_trackBar.Value, true);

                if (true == bSync)
                {
                    Super.Pose.Pose_ToeOpenAndClose(CurrentSelectedMaid, ToeBlend_Left_Open_trackBar.Value, ToeBlend_Left_Fist_trackBar.Value, false);
                }
            }

            UnlockBusy();
        }

        private void FingerBlend_Lock_CheckedChanged(object sender, EventArgs e)
        {
            string senderName = ((RadioButton)sender).Name;

            if (true == senderName.Contains("_LockNone_"))
            {
                Blend_LockSelected_radioButton.Checked = false;
                Blend_LockOthers_radioButton.Checked = false;
            }
            else if (true == senderName.Contains("_LockSelected_"))
            {
                Blend_LockNone_radioButton.Checked = false;
                Blend_LockOthers_radioButton.Checked = false;
            }
            else
            {
                Blend_LockNone_radioButton.Checked = false;
                Blend_LockSelected_radioButton.Checked = false;
            }
        }

        private void MaidPartsEdit_Enable_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == MaidPartsEdit_Enable_checkBox.Checked)
            {
                if (true == Super.Parts.Init(CurrentSelectedMaid))
                {
                    Items_UpdateCurrentHandleCount();
                    Items_Single_radioButton.Checked = true;
                }
            }
            else
            {
                Super.Items.Items_RemoveCategory("MaidPartsHandle");
                Items_UpdateCurrentHandleCount();
            }
        }

        private void Items_SubObjects_TempDisableMaidHandle_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if(true == Items_SubObjects_TempDisableMaidHandle_checkBox.Checked)
                MaidHandleSelectModle_None_radioButton.Checked = true;
            else
                MaidHandleSelectModle_All_radioButton.Checked = true;
        }

        private void Items_SubObjects_DynamicBone_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if(true == IsBusy())
            {
                return;
            }

            HandleEx posHandle = (HandleEx)Items_SubObjects_listBox.SelectedValue;
            DynamicBone dynamicBone = posHandle.parentBone.GetComponent<DynamicBone>();
            if (null != dynamicBone)
            {
#if DEBUG
                Debuginfo.Log(posHandle.parentBone.name + ": DynamicBone.enabled = " + Items_SubObjects_DynamicBone_checkBox.Checked, 2);
#endif
                dynamicBone.enabled = Items_SubObjects_DynamicBone_checkBox.Checked;
                if(true == dynamicBone.enabled)
                {
                    dynamicBone.m_Gravity = Vector3.zero;
                }
            }
        }

        private void ReloadMaid_button_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("「はい」をクリックすると、現在のメイドが再読み込みされます", "滅茶苦茶", MessageBoxButtons.YesNo);
            if(DialogResult.Yes == dialogResult)
            {
                int ActiveSlotNo = CurrentSelectedMaid.ActiveSlotNo;
#if DEBUG
                Debuginfo.Warning("Reloading ActiveSlotNo=" + ActiveSlotNo, 0);
                Debuginfo.Warning("CurrentSelectedMaidIndex=" + CurrentSelectedMaidIndex, 0);
#endif
                GameMain.Instance.CharacterMgr.DeactivateMaid(CurrentSelectedMaid);
                GameMain.Instance.CharacterMgr.SetActiveMaid(CurrentSelectedMaid, ActiveSlotNo);
                CurrentMaidsList[CurrentSelectedMaidIndex] = GameMain.Instance.CharacterMgr.CharaVisible(CurrentSelectedMaidIndex, true, false);
                Super.SetReloadingMaid(true, CurrentSelectedMaidIndex);
            }
        }

        private void Items_SubObjects_SlotBodyHit_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            SetSlotBodyHit(Items_SubObjects_SlotBodyHit_checkBox.Checked);
        }

        private void CameraPos_SaveAndLoad_button_Click(object sender, EventArgs e)
        {
            string sSenderName = ((Button)sender).Name.Split('_')[1];
            int iSenderIndex = int.Parse(sSenderName.Remove(0, 4));
            string sTagName = "CameraPos_Tag" + iSenderIndex.ToString() + "_textBox";
            TextBox textBox = (Controls.Find(sTagName, true)[0] as TextBox);

            if (true == sSenderName.Contains("Save"))
            {
                if(true == string.Equals(textBox.Text, "無"))
                    textBox.Text = DateTime.Now.ToString();
                Super.camPlus.CameraDataSlotSave(iSenderIndex - 1, textBox.Text);
            }
            else  // Load
            {
                if (false == string.Equals(textBox.Text, "無"))
                    Super.camPlus.CameraDataSlotLoad(iSenderIndex - 1);
                else
                    Debuginfo.Warning("カメラデータがありません iSenderIndex = "+ iSenderIndex.ToString(), 1);
            }
        }

        private void MaidTails_SpecialMark_Method_ForceEnum_radioButton1_Click(object sender, EventArgs e)
        {
            string sSenderName = ((RadioButton)sender).Name.Split('_')[2];

            if (true == Super.settingsXml.MaidTailsUseDFS)
            {
                Debuginfo.Warning("DFS: 深さ優先モードでは「強制列挙」を使用できません", 0);
                MaidTails_SpecialMarkMethod_ForceEnum_radioButton.Checked = false;
                MaidTails_SpecialMarkMethod_Ignore_radioButton.Checked = true;
                Super.settingsXml.MaidTailsSpecialMarkMethodIgnore = true;
                Super.SaveConfigurefile();
            }
            else
            {
                if (true == sSenderName.Equals("ForceEnum"))
                {
                    MaidTails_SpecialMarkMethod_Ignore_radioButton.Checked = false;
                    Super.settingsXml.MaidTailsSpecialMarkMethodIgnore = false;
                    Super.SaveConfigurefile();
                }
                else
                {
                    MaidTails_SpecialMarkMethod_ForceEnum_radioButton.Checked = false;
                    Super.settingsXml.MaidTailsSpecialMarkMethodIgnore = true;
                    Super.SaveConfigurefile();
                }
            }
        }

        private void MaidTails_BoneEnumMethod_Enum_radioButton_Click(object sender, EventArgs e)
        {
            string sSenderName = ((RadioButton)sender).Name.Split('_')[2];

            if (true == sSenderName.Equals("DFS"))
            {
                MaidTails_BoneEnumMethod_Enum_radioButton.Checked = false;
                Super.settingsXml.MaidTailsUseDFS = true;

                MaidTails_SpecialMarkMethod_Ignore_radioButton.Checked = true;
                MaidTails_SpecialMarkMethod_ForceEnum_radioButton.Checked = false;
                Super.settingsXml.MaidTailsSpecialMarkMethodIgnore = true;
                Settings_MaidTails_SpecialMark_groupBox.Enabled = false;

                Super.SaveConfigurefile();
            }
            else
            {
                MaidTails_BoneEnumMethod_DFS_radioButton.Checked = false;
                Super.settingsXml.MaidTailsUseDFS = false;

                Settings_MaidTails_SpecialMark_groupBox.Enabled = true;

                Super.SaveConfigurefile();
            }
        }

        private void RTME_Export_button_Click(object sender, EventArgs e)
        {
            Hide();
            Super.MaidIK.LockIK();
            LockBusy();

            EMES_RuntimeModelExport RTME = new EMES_RuntimeModelExport();
            RTME.SaveModel(CurrentSelectedMaid, Super.GetConfigDirectory().Replace("\\Config\\", "\\RTME\\"), RTME_NoMTL_checkBox.Checked);

            Super.MaidIK.UnLockIK();
            UnlockBusy();
            Show();
        }

        private void RTMI_Import_button_Click(object sender, EventArgs e)
        {
            RTMI_openFileDialog.Title = "ランタイムモデルのインポート";
            RTMI_openFileDialog.Filter = "OBJ(*.obj)|*.obj";
            RTMI_openFileDialog.InitialDirectory = Super.GetConfigDirectory().Replace("\\Config\\", "");
            if (DialogResult.OK == RTMI_openFileDialog.ShowDialog())
            {
                Hide();
                Super.MaidIK.LockIK();
                LockBusy();

                string sFullPathOBJ = RTMI_openFileDialog.FileName;
                string handleName = Path.GetFileNameWithoutExtension(sFullPathOBJ);
                EMES_RuntimeModelImporter RTMI = new EMES_RuntimeModelImporter();
                GameObject go = RTMI.LoadModel(sFullPathOBJ, Super.Items.goItemMaster);
                if (null != go)
                {
                    go.transform.localPosition = new  Vector3(CurrentSelectedMaid.GetPos().x, CurrentSelectedMaid.GetPos().y, CurrentSelectedMaid.GetPos().z + 0.5f);
                    //Super.Items.Items_RemoveCategory("RTMIHandle");
                    HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, go, false, true);
                    handle.SetupItem(DateTime.Now.ToString("hhmmss") + CurrentSelectedMaid.status.callName + handleName, handleName, "RTMIHandle");
                    Super.Items.Items_ItemHandle.Add(handle, handleName);
                    Items_UpdateCurrentHandleCount();
                }

                Super.MaidIK.UnLockIK();
                UnlockBusy();
                Show();
            }
        }

        private void Items_Material_trackBar_Scroll(object sender, EventArgs e)
        {
            if (true == IsBusy())
                return;

            HandleEx handle = null;
            if (subItems_list_tabPage2 == Items_List_tabControl.SelectedTab)
            {
                if (Items_SubObjects_listBox.SelectedIndex >= 0)
                    handle = (HandleEx)Items_SubObjects_listBox.SelectedValue;
            }
            else
            {
                if (false == Items_isItemHandleMethodAllorSingle())
                    return;

                if (Items_HandledObjects_listBox.SelectedIndex >= 0)
                    handle = (HandleEx)Items_HandledObjects_listBox.SelectedValue;
            }

            if(null == handle)
            {
                return;
            }

            bool inChildren = Items_Material_InChildren_checkBox.Checked;
            SkinnedMeshRenderer[] skinnedMeshRenderers = handle.parentBone.GetComponentsInChildren<SkinnedMeshRenderer>();
            int loop = inChildren ? skinnedMeshRenderers.Length : 1;
            string shaderType = Items_Material_Type_comboBox.SelectedItem.ToString();
#if DEBUG
            Debuginfo.Log("shaderType = " + shaderType, 2);
#endif
            if (skinnedMeshRenderers.Length > 0)
            {
                float value = ((float)Items_Material_Value_trackBar.Value / 1000f);
                Items_Shader_Value_label.Text = value.ToString();

                for (int i = 0; i < loop; i++)
                {
                    foreach(Material material in skinnedMeshRenderers[i].materials)
                    {
                        if (true == shaderType.Equals("Color"))
                        {
                            UnityEngine.Color color = material.GetColor(Items_Material_Name_textBox.Text);
                            if (null == color)
                                color = new UnityEngine.Color(1, 1, 1, 1);

                            color.a = value;
                            material.SetColor(Items_Material_Name_textBox.Text, color);
                        }
                        else if (true == shaderType.Equals("Float"))
                        {
                            material.SetFloat(Items_Material_Name_textBox.Text, value);
                        }
                        else if (true == shaderType.Equals("Int"))
                        {
                            material.SetInt(Items_Material_Name_textBox.Text, (int)value);
                        }
                    }
                }
            }
        }

        #endregion
    }
}


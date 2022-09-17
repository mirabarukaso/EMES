using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class MaidTailsLite
    {
        public enum MaidTailsHandleMethod
        {
            None = 0,
            Position = 1,
            Rotation = 2,
            Universal = 3,
            IK = 4
        };

        private int BoneCounts = 0;
        private Dictionary<string, Transform> BoneTails;
        private int accShippoObjectInstanceID = -1;
        private int accSenakaObjectInstanceID = -1;

        private bool MutexLock = true;

        private string[] DFS_BonesPriorityWords;
        private string DFS_LastKey;
        private class BoneData
        {
            public Transform BoneTransform = null;
            public int Level = 0;
            public int Index = 0;
            public int CurrentIndex = 0;
            public int ChildrenCount = 0;

            public int BoneSliderIndex = 65535;
            public string ShortName = "無";

        }
        private Dictionary<string, Dictionary<Transform, BoneData>> DFS_ParentBones;

        public class BonePosRotScaleInfo
        {
            public Vector3 localPosition;
            public Vector3 localScale;
            public Quaternion localRotation;
            public string name;
            public string ShortName = "無";
        }
        private Dictionary<Transform, BonePosRotScaleInfo> DefaultBoneData;
        private Dictionary<Transform, BonePosRotScaleInfo> CopyAllBoneData;
        private BonePosRotScaleInfo CopyBoneData = null;

        private EMES Super;
        
        public HandleEx tailHandle { get; private set; }

        public bool bIsAutoIK = false;
        public List<EMES_MaidIK.EMES_IK.IK> AutoIK = null;

        private Action action_ProcessHandle = delegate { };

#region public method
        public MaidTailsLite()
        {
            BoneTails = new Dictionary<string, Transform>();
            DFS_ParentBones = new Dictionary<string, Dictionary<Transform, BoneData>>();
            DefaultBoneData = new Dictionary<Transform, BonePosRotScaleInfo>();
            CopyAllBoneData = new Dictionary<Transform, BonePosRotScaleInfo>();
            CopyBoneData = new BonePosRotScaleInfo()
            {
                localPosition = Vector3.zero,
                localRotation = Quaternion.identity,
                localScale = Vector3.one,
                name = "無"
            };
            AutoIK = new List<EMES_MaidIK.EMES_IK.IK>();
            Init();
        }

        public bool Init()
        {
            SetMutexLock(true);

            DestoryTailHandle();
            DestoryTailHandleAutoIK();

            BoneCounts = 0;
            BoneTails.Clear();
            accShippoObjectInstanceID = -1;
            accSenakaObjectInstanceID = -1;

            return true;
        }

        public void Finalized()
        {
#if DEBUG
            Debuginfo.Log("MaidTails finalize...", 2);
#endif
            DestoryTailHandle();
            DestoryTailHandleAutoIK();

            if (DFS_ParentBones.Count > 0)
            {
                foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentBoneList in DFS_ParentBones)
                {
                    if (CurrentBoneList.Value.Count > 0)
                    {
                        DFS_ParentBones[CurrentBoneList.Key].Clear();
                    }
                }
            }

            DFS_ParentBones.Clear();
            DFS_ParentBones = null;
            DefaultBoneData.Clear();
            DefaultBoneData = null;
            CopyAllBoneData.Clear();
            CopyAllBoneData = null;
            CopyBoneData = null;
            BoneTails.Clear();
#if DEBUG
            Debuginfo.Log("MaidTails finalize Done", 2);
#endif
        }

        public void SetupInstance(EMES super_instance)
        {
            Super = super_instance;
        }

        public bool IsMutexLock()
        {
            return MutexLock;
        }

        public void SetMutexLock(bool trigger)
        {
            MutexLock = trigger;
        }

        public int GetBoneCounts()
        {
            return BoneCounts;
        }

        public Dictionary<string, Transform> GetBoneTails()
        {
            return BoneTails;
        }

        public string GetBoneShortName(Transform trBone)
        {
            if (DefaultBoneData.Count > 0)
            {
                if (true == DefaultBoneData.ContainsKey(trBone))
                {
                    return DefaultBoneData[trBone].ShortName;
                }
            }

            return "無";
        }

        public bool DFS_Init()
        {
            DFS_LastKey = System.DateTime.Now.ToString();
            DFS_BonesPriorityWords = Super.settingsXml.BonesPriority.Split(',');
            DFS_ParentBones.Clear();
            for (int Index = 0; Index < DFS_BonesPriorityWords.Count(); Index++)
            {
                if (false == DFS_ParentBones.ContainsKey(DFS_BonesPriorityWords[Index].ToString()))
                {
                    DFS_ParentBones.Add(DFS_BonesPriorityWords[Index].ToString(), new Dictionary<Transform, BoneData>());
                }
            }
            if (false == DFS_ParentBones.ContainsKey(DFS_LastKey))
            {
                DFS_ParentBones.Add(DFS_LastKey, new Dictionary<Transform, BoneData>());
            }

            SetMutexLock(false);
            return true;
        }

        public bool CheckShippoChange(Maid maid)
        {
            if (true == IsMutexLock())
                return false;

            if (null == maid)
                return false;

            List<TBodySkin> objList = new List<TBodySkin>();
            int tmp_accShippoObjectInstanceID = -1;
            int tmp_accSenakaObjectInstanceID = -1;
            bool Change = false;

            objList.Clear();

            for (int Index = 0; Index < maid.body0.goSlot.Count; Index++)
            {
                TBodySkin tbs = maid.body0.goSlot[Index];
                if (null != tbs.obj)
                {
                    if (TBody.SlotID.body == tbs.SlotId)
                    {
                        objList.Add(tbs);
                    }

                    if (TBody.SlotID.accShippo == tbs.SlotId)
                    {
                        if (accShippoObjectInstanceID != tbs.obj.GetInstanceID())
                        {
                            Change = true;
                        }
                        objList.Add(tbs);
                        tmp_accShippoObjectInstanceID = tbs.obj.GetInstanceID();
                    }
                    else if (TBody.SlotID.accSenaka == tbs.SlotId)
                    {
                        if (accSenakaObjectInstanceID != tbs.obj.GetInstanceID())
                        {
                            Change = true;
                        }
                        objList.Add(tbs);
                        tmp_accSenakaObjectInstanceID = tbs.obj.GetInstanceID();
                    }
                }
            }

            if ((accShippoObjectInstanceID != tmp_accShippoObjectInstanceID) || (accSenakaObjectInstanceID != tmp_accSenakaObjectInstanceID))
            {
#if DEBUG
                Debuginfo.Log("ボーンデータを更新", 2);
#endif
                Change = true;
            }

            if (true == Change)
            {
                Init();

                if (tmp_accShippoObjectInstanceID != -1)
                    accShippoObjectInstanceID = tmp_accShippoObjectInstanceID;

                if (tmp_accSenakaObjectInstanceID != -1)
                    accSenakaObjectInstanceID = tmp_accSenakaObjectInstanceID;

                SetMutexLock(true);
                ReadTailbonesData(maid, objList, false);
                CreateBoneTail();
                SetMutexLock(false);
                return true;
            }
            else if (GetBoneCounts() != 0)
            {
                if (accShippoObjectInstanceID != tmp_accShippoObjectInstanceID
                    || accSenakaObjectInstanceID != tmp_accSenakaObjectInstanceID
                    )
                {
                    Init();
                    SetMutexLock(false);
                    return true;
                }
            }

            return false;
        }

#region Bone C/P/R
        public bool CopyBonesInfo(Transform tr)
        {
            if (0 == GetBoneCounts())
            {
                return false;
            }

            CopyBoneData.localPosition = tr.localPosition;
            CopyBoneData.localRotation = tr.localRotation;
            CopyBoneData.localScale = tr.localScale;
            CopyBoneData.name = tr.name;

            return true;
        }
        
        public bool CopyBonesInfoAll()
        {
            if (GetBoneCounts() == 0)
            {
                return false;
            }

            CopyAllBoneData.Clear();
            foreach (KeyValuePair<string, Transform> trBone in BoneTails)
            {
                BonePosRotScaleInfo bi = new BonePosRotScaleInfo()
                {
                    localPosition = trBone.Value.localPosition,
                    localRotation = trBone.Value.localRotation,
                    localScale = trBone.Value.localScale,
                    name = trBone.Value.name
                };

                CopyAllBoneData.Add(trBone.Value, bi);
            }

            return true;
        }
        
        public bool PasteBonesInfo(Transform tr)
        {
            if (0 == GetBoneCounts())
            {
                return false;
            }

            if("無" == CopyBoneData.name)
            {
                return false;
            }

            if (CopyBoneData.name != tr.name)
            {
                Debuginfo.Log("内容の不一致が発生します、継続する >>> " + tr.name + " vs " + CopyBoneData.name, 1);
            }

            tr.localPosition = CopyBoneData.localPosition;
            tr.localRotation = CopyBoneData.localRotation;
            tr.localScale = CopyBoneData.localScale;

            return true;
        }
        

        public bool PasteBonesInfoAll()
        {
            if (0 == GetBoneCounts())
            {
                return false;
            }

            if (0 == CopyAllBoneData.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, Transform> trBone in BoneTails)
            {
                if(true == CopyAllBoneData.ContainsKey(trBone.Value))
                {
                    trBone.Value.localPosition = CopyAllBoneData[trBone.Value].localPosition;
                    trBone.Value.localRotation = CopyAllBoneData[trBone.Value].localRotation;
                    trBone.Value.localScale = CopyAllBoneData[trBone.Value].localScale;
                }
                else
                {
                    Debuginfo.Log("内容の不一致が発生します、継続する >>> " + trBone.Value.name, 1);
                }
            }

            return true;
        }

        public bool ResetBoneInfo(Transform trBone)
        {
            if (0 == GetBoneCounts())
            {
                return false;
            }

            if (0 == DefaultBoneData.Count)
            {
                return false;
            }

            if (true == DefaultBoneData.ContainsKey(trBone))
            {
                trBone.localPosition = DefaultBoneData[trBone].localPosition;
                trBone.localRotation = DefaultBoneData[trBone].localRotation;
                trBone.localScale = DefaultBoneData[trBone].localScale;
                Debuginfo.Log("値をリセット " + trBone.name, 1);

                return true;
            }

            return false;
        }

        public bool ResetBoneInfoAll()
        {
            if (0 == GetBoneCounts())
            {
                return false;
            }

            if (0 == DefaultBoneData.Count)
            {
                return false;
            }

            foreach(KeyValuePair<string, Transform> trBone in BoneTails)
            {
                if(false == ResetBoneInfo(trBone.Value))
                {
                    Debuginfo.Log("内容の不一致が発生します、継続する >>> " + trBone.Value.name, 1);
                }
            }

            return false;
        }
#endregion


        public List<List<EMES_SceneManagement.BonePosRotScaleInfo>> CreateScenceManagementBontTailData()
        {
            List<List<EMES_SceneManagement.BonePosRotScaleInfo>> Data = new List<List<EMES_SceneManagement.BonePosRotScaleInfo>>();

            for (int Index = 0; Index < Super.Window.CurrentMaidsStockID.Count; Index++)
            {
                List<TBodySkin> objList = CreateObjList(Super.Window.CurrentMaidsList[Index]);
                ReadTailbonesData(Super.Window.CurrentMaidsList[Index], objList, true);
#if DEBUG
                Debuginfo.Log("Read maid tails from [" + Index.ToString() + "] " + Super.Window.CurrentMaidsList[Index].status.callName, 2);
#endif
                Data.Add(CreateBonePosRotScaleInfo());
            }

            return Data;
        }

        public void RotateScenceManagementBontTailData(List<List<EMES_SceneManagement.BonePosRotScaleInfo>> data, List<Maid> maid, List<int> order)
        {
            for (int Index = 0; Index < data.Count; Index++)
            {
                if (0 == order.Count)
                {
                    if (Index >= Super.Window.CurrentMaidsStockID.Count)
                    {
                        break;
                    }
#if DEBUG
                    Debuginfo.Log("Sync maid tails for [" + Index.ToString() + "] " + Super.Window.CurrentMaidsList[Index].status.callName, 2);
#endif
                    List<TBodySkin> objList = CreateObjList(Super.Window.CurrentMaidsList[Index]);
                    ReadTailbonesData(Super.Window.CurrentMaidsList[Index], objList, true);
                    RotateBonePosRotScaleInfo(data[Index]);
                }
                else
                {
                    if (Index >= maid.Count)
                    {
                        break;
                    }
#if DEBUG
                    Debuginfo.Log("Sync maid tails with order for [" + order[Index].ToString() + "] " + maid[Index].status.callName, 2);
#endif
                    List<TBodySkin> objList = CreateObjList(maid[Index]);
                    ReadTailbonesData(maid[Index], objList, true);
                    RotateBonePosRotScaleInfo(data[order[Index]]);
                }
            }
        }

#endregion

#region AutoIK
        private void DestoryTailHandleAutoIK()
        {
            if (0 != AutoIK.Count)
            {
                foreach (EMES_MaidIK.EMES_IK.IK ik in AutoIK)
                {
                    ik.handle.Visible = false;
                    ik.handle.Destroy();
                }
            }
            AutoIK.Clear();
        }

        public void CreateTailHandleAutoIK(List<Transform> trBones, float scale)
        {
            DestoryTailHandleAutoIK();

            foreach (Transform trBone in trBones)
            {
                Transform root = trBone;
                Transform mid = root.parent ?? null;
                Transform end = mid.parent ?? null;

#if DEBUG
                Debuginfo.Log("Create root=" + root.name + " mid=" + mid.name + " end=" + end.name, 2);
#endif

                EMES_MaidIK.EMES_IK.IK ik = new EMES_MaidIK.EMES_IK.IK();
                ik.GetIKEffectorType = EMES_MaidIK.IKEffectorType.Body;
                ik.GetBoneType = 0;
                ik.hip = end;
                ik.knee = mid;
                ik.ankle = root;

#if DEBUG
                Debuginfo.Log("Create HandleEx", 2);
#endif
                HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, root.gameObject, false, true);
                handle.IK_ChangeHandleKunModeIK(true);
                handle.Visible = true;
                handle.Scale = scale;
                ik.handle = handle;

#if DEBUG
                Debuginfo.Log("Create CustomIKCMO CurrentSelectedMaid =" + Super.Window.CurrentSelectedMaid.status.callName, 2); ;
#endif
                ik.GetIKCMO = new EMES_MaidIK.CustomIKCMO();
                ik.GetIKCMO.Init(ik.hip, ik.knee, ik.ankle, Super.Window.CurrentSelectedMaid.body0);

#if DEBUG
                Debuginfo.Log("Create AutoIK.Add", 2);
#endif
                AutoIK.Add(ik);
            }
        }
#endregion

#region action method
        public void DestoryTailHandle()
        {
            if (null != tailHandle)
            {
                tailHandle.Visible = false;
                tailHandle.Destroy();
                tailHandle = null;
            }
        }

        public void ProcessHandle()
        {
            action_ProcessHandle();
        }

        public void CreateTailHandle(Transform trBone)
        {
            if (null == tailHandle)
            {
                tailHandle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, trBone.gameObject, false, true);
                tailHandle.SetupItem("tailHandle", "tailHandle", "tailHandle");
                tailHandle.IKmode = HandleEx.IKMODE.UniversalIK;
                SetHandleMode(MaidTailsHandleMethod.Universal);
            }
            else
            {
                if(false == tailHandle.SetParentBone(trBone))
                {
                    Super.Window.ReloadCurrentMaidInfo();
                }
                else
                {
                    SetHandleMode(MaidTailsHandleMethod.Universal);
                }
            }
        }

        public void SetHandleMode(MaidTailsHandleMethod method)
        {
            tailHandle.IK_ChangeHandleKunModeIK(false);
            bIsAutoIK = false;

            switch (method)
            {
                case MaidTailsHandleMethod.IK:
                    tailHandle.IK_ChangeHandleKunModeIK(true);
                    bIsAutoIK = true;
                    action_ProcessHandle = delegate { };
                    tailHandle.Visible = true;
                    break;
                case MaidTailsHandleMethod.Universal:
                    action_ProcessHandle = delegate { Action_Universal(); };
                    tailHandle.Visible = false;
                    tailHandle.Scale = -1f;
                    break;
                case MaidTailsHandleMethod.Position:
                    action_ProcessHandle = delegate { Action_Pos(); };
                    tailHandle.IK_ChangeHandleKunModePosition(true);
                    break;
                case MaidTailsHandleMethod.Rotation:
                    action_ProcessHandle = delegate { Action_Rot(); };
                    tailHandle.IK_ChangeHandleKunModePosition(false);
                    break;
                case MaidTailsHandleMethod.None:
                    action_ProcessHandle = delegate { };
                    tailHandle.Visible = false;
                    break;
            }
        }

        private void Action_Universal()
        {
            if (null == tailHandle)
                return;

            if (false == tailHandle.Visible)
            {
                if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && true == Super.settingsXml.bHotkeyItemPos)
                {
                    if (false == tailHandle.IK_GetHandleKunPosotionMode())
                        tailHandle.IK_ChangeHandleKunModePosition(true);
                    tailHandle.Visible = true;
                }
                else if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemRot) && true == Super.settingsXml.bHotkeyItemRot)
                {
                    if (true == tailHandle.IK_GetHandleKunPosotionMode())
                        tailHandle.IK_ChangeHandleKunModePosition(false);
                    tailHandle.Visible = true;
                }
                else if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize) && true == Super.settingsXml.bHotkeyItemSize)
                {
                    if (false == tailHandle.IK_GetHandleKunIKMode())
                        tailHandle.IK_ChangeHandleKunModeIK(true);
                    tailHandle.Visible = true;
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize))
                {
                    if (true == tailHandle.IK_GetHandleKunIKMode())
                    {
                        if (true == Super.settingsXml.bHotkeyItemSize)
                            tailHandle.IK_ChangeHandleKunModePosition(true);
                    }
                }
            }
            else
            {
                if (true == tailHandle.ControllDragged())
                {
                    if (false == tailHandle.IK_GetHandleKunPosotionMode())
                    {
                        //ボーンを回転させておく
                        tailHandle.GetParentBone().rotation *= tailHandle.DeltaQuaternion();
                    }
                    else
                    {
                        tailHandle.GetParentBone().position += tailHandle.DeltaVector();
                    }

                    if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize) && true == Super.settingsXml.bHotkeyItemSize)
                    {
                        tailHandle.GetParentBone().localScale += new Vector3(tailHandle.DeltaVector().y, tailHandle.DeltaVector().y, tailHandle.DeltaVector().y);
                        Super.Window.SetMaidTailsBoneScale(tailHandle.GetParentBone().localScale.x);
                    }
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemRot)
                        && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize))
                {
                    if (true == Super.settingsXml.bHotkeyItemPos || true == Super.settingsXml.bHotkeyItemRot || true == Super.settingsXml.bHotkeyItemSize)
                        tailHandle.Visible = false;
                }
            }
        }

        private void Action_Pos()
        {
            if (null == tailHandle)
                return;

            if (false == tailHandle.Visible)
            {
                if (false == tailHandle.IK_GetHandleKunPosotionMode())
                    tailHandle.IK_ChangeHandleKunModePosition(true);
                tailHandle.Visible = true;
            }
            else
            {
                if (true == tailHandle.ControllDragged())
                {
                    tailHandle.GetParentBone().position += tailHandle.DeltaVector();
                }
            }
        }

        private void Action_Rot()
        {
            if (null == tailHandle)
                return;

            if (false == tailHandle.Visible)
            {
                if (true == tailHandle.IK_GetHandleKunPosotionMode())
                    tailHandle.IK_ChangeHandleKunModePosition(false);
                tailHandle.Visible = true;
            }
            else
            {
                if (true == tailHandle.ControllDragged())
                {
                    tailHandle.GetParentBone().rotation *= tailHandle.DeltaQuaternion();
                }
            }
        }
#endregion

#region private method
        private bool EnumBones(Transform InBone, int Index, out Transform OutBone)
        {
            if (InBone.transform.childCount <= Index)
            {
                OutBone = null;
                return false;
            }

            OutBone = InBone.transform.GetChild(Index);
            if (null == OutBone)
            {
                return false;
            }

            return true;
        }

        private bool DFS_CheckBonesPriorityName(string Name, out string TargetString)
        {
            string CheckedNameLeft = null;

            if (Name.Contains("BoneTail"))
            {
                CheckedNameLeft = Name.Substring(("BoneTail").Length).Split('_')[0];
            }
            else if (Name.Contains("Bip01 BoneTail"))
            {
                CheckedNameLeft = Name.Substring(("Bip01 BoneTail").Length).Split('_')[0];
            }

            if (DFS_BonesPriorityWords.Contains(CheckedNameLeft))
            {
                TargetString = string.Copy(CheckedNameLeft);
                return true;
            }

            TargetString = DFS_LastKey;
            return false;
        }

        public void ReadTailbonesData(Maid maid, List<TBodySkin> tbsList, bool ScenceManagement)
        {
            if (0 != GetBoneCounts() && false == ScenceManagement)
            {
                return;
            }
            
            string DFS_TargetString;
            int DFS_Count = 1;

            //Body_Bip01
            Transform Body_Bip01 = null;
            Transform Body_Bip01_Pelvis = null;

            //Bip01
            Transform Bip01 = null;
            Transform Bip01_Pelvis = null;

            Stack<Transform> BoneTransformStack = new Stack<Transform>();
            if (DFS_ParentBones.Count > 0)
            {
                foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentBone in DFS_ParentBones)
                {
                    if (CurrentBone.Value.Count > 0)
                    {
                        DFS_ParentBones[CurrentBone.Key].Clear();
                    }
                }
            }

            foreach (TBodySkin tbs in tbsList)
            {
                if (TBody.SlotID.body == tbs.SlotId)
                {
                    Body_Bip01 = tbs.obj.transform.Find("Bip01");
                    Body_Bip01_Pelvis = Body_Bip01.transform.Find("Bip01 Pelvis");
                    continue;
                }

                Transform tmpBone = tbs.obj.transform.Find("Bone_center");
                if (!tmpBone)
                {
                    tmpBone = tbs.obj.transform.Find("Bip01");

                    try
                    {
                        if (!tmpBone)
                        {
                            continue;
                        }
                        else if (tmpBone.transform.childCount < 1)
                        {
                            continue;
                        }
                        else if (!tmpBone.transform.GetChild(0).name.Contains("BoneTail") && !tmpBone.transform.GetChild(0).name.Contains("Bip01"))
                        {
#if DEBUG
                            Debuginfo.Log("\"BoneTail\" マークが見つかりません、無視する。 >>> " + tmpBone.transform.GetChild(0).name, 2);
#endif
                            continue;  
                        }
#if DEBUG
                        for (int i = 0; i < tmpBone.transform.childCount; i++)
                        {
                            Debuginfo.Log("tmpBone.transform.GetChild(" + i + ").name = " + tmpBone.transform.GetChild(i).name, 2);
                        }
#endif
                        Bip01 = tmpBone.transform;
                    }
#if DEBUG
                    catch (Exception Error)
                    {

                        Debuginfo.Warning("エラー（自動無視する）: " + Error, 2);
                        continue;
                    }
#else
                    catch
                    {
                        continue;
                    }
#endif
                }
                else if (tmpBone.transform.childCount < 2)
                {
#if DEBUG
                    Debuginfo.Log("\"Bone_center->Bone_nub\"、無視する。", 2);
#endif
                    continue;
                }

                bool NotMailTail = false;

                try
                {
                    for (int Index = 0; Index < tmpBone.transform.childCount; Index++)
                    {
#if DEBUG
                        //Debuginfo.Log("Try >>> " + tmpBone.transform.GetChild(Index).name, 2);
#endif
                        if ("Bip01 Pelvis" != tmpBone.transform.GetChild(Index).name 
                            && false == tmpBone.transform.GetChild(Index).name.Contains("BoneTail") 
                            && false == tmpBone.transform.GetChild(Index).name.Contains("nub"))
                        {
#if DEBUG
                            Debuginfo.Log("\"BoneTail\" マークが見つかりません、無視する。 Index>>> " + tmpBone.transform.GetChild(Index).name, 2);
#endif
                            NotMailTail = true;
                        }
                        else if ("Bip01 Pelvis" == tmpBone.transform.GetChild(Index).name)
                        {
                            for (int IndexPelvis = 0; IndexPelvis < tmpBone.transform.GetChild(Index).childCount; IndexPelvis++)
                            {
                                if (false == tmpBone.transform.GetChild(Index).GetChild(IndexPelvis).name.Contains("BoneTail") 
                                    && false == tmpBone.transform.GetChild(Index).GetChild(IndexPelvis).name.Contains("nub"))
                                {
#if DEBUG
                                    Debuginfo.Log("\"BoneTail\" マークが見つかりません、無視する。 IndexPelvis>>> " + tmpBone.transform.GetChild(Index).GetChild(IndexPelvis).name, 2);
#endif
                                    NotMailTail = true;
                                }
                            }
                        }
                    }
                }
#if DEBUG
                catch (Exception Error)
                {

                    Debuginfo.Warning("エラー（自動無視する）: " + Error, 2);
                    NotMailTail = true;
                }
#else
                catch
                {
                    NotMailTail = true;
                }
#endif

                if (true == NotMailTail)
                {
#if DEBUG
                    Debuginfo.Log("\"BoneTail\" マークが見つかりません、無視する。", 2);

#endif
                    continue;
                }

                //Bone_center
                Transform ThisBoneTransform;
                int ThisBoneCurrentIndex = 0;
                int ThisBoneCurrentLevel = 1;

                BoneData Bone_center = new BoneData()
                {
                    BoneTransform = tmpBone,
                    Level = 0,
                    Index = 0,
                    CurrentIndex = 0,
                    ChildrenCount = tmpBone.childCount,
                    BoneSliderIndex = 0
                };

                //DFS 深さ優先探索
                BoneTransformStack.Push(tmpBone);
                DFS_CheckBonesPriorityName(tmpBone.name, out DFS_TargetString);
                DFS_ParentBones[DFS_TargetString].Add(Bone_center.BoneTransform, Bone_center);

                while (BoneTransformStack.Count > 0)
                {
                    bool bEnumBones = EnumBones(tmpBone, ThisBoneCurrentIndex, out ThisBoneTransform);
#if DEBUG
                    /*
                    Debuginfo.Log("tmpBone=" + tmpBone.name, 2);
                    Debuginfo.Log("ThisBoneCurrentIndex=" + ThisBoneCurrentIndex.ToString() ,2);
                    if(true == bEnumBones) 
                        Debuginfo.Log("bEnumBones = true ThisBoneTransform =" + ThisBoneTransform.name, 2);
                    else
                        Debuginfo.Log("bEnumBones = false", 2);
                    //*/
#endif
                    if (true == bEnumBones)
                    {
                        DFS_CheckBonesPriorityName(ThisBoneTransform.name, out DFS_TargetString);
                        if (false == DFS_ParentBones[DFS_TargetString].ContainsKey(ThisBoneTransform))
                        {
                            if ((ThisBoneTransform.name.Contains("_nub"))
                                || (ThisBoneTransform.name.Contains("_base"))
                                || (ThisBoneTransform.name.Contains("_yure_")) || (ThisBoneTransform.name.Contains("_Base"))
                               )
                            {
#if DEBUG
                                Debuginfo.Log("DFS: 特別にマークされたボーンを無視する (" + ThisBoneTransform.gameObject + ")", 2);
#endif
                            }
                            else if (false == ThisBoneTransform.name.Contains("BoneTail")
                                    && "Bone_center" != ThisBoneTransform.name
                                    && "Bip01" != ThisBoneTransform.name
                                    && false == ThisBoneTransform.name.Contains("Bip01")
                                    && "Bone_nub" != ThisBoneTransform.name
                                    )
                            {
                                Debuginfo.Warning("DFS: DestroyImmediate ダングリングブジェクト (" + ThisBoneTransform.name + ") Index=" + ThisBoneCurrentIndex + " 心配しないで（多分）", 1);
                                UnityEngine.Object.DestroyImmediate(ThisBoneTransform.gameObject);
                                continue;
                            }

                            BoneData ThisBone = new BoneData()
                            {
                                BoneTransform = ThisBoneTransform,

                                Level = ThisBoneCurrentLevel,
                                Index = ThisBoneCurrentIndex,
                                CurrentIndex = 0,
                                ChildrenCount = ThisBoneTransform.transform.childCount
                            };

                            if ("Bip01 Pelvis" == ThisBone.BoneTransform.name)
                                Bip01_Pelvis = ThisBone.BoneTransform;

                            ThisBone.BoneSliderIndex = 65535;
                            string[] CurrentPrefix = ThisBone.BoneTransform.name.Split('_');
                            if (CurrentPrefix != null)
                            {
                                if (CurrentPrefix.Length == 3)
                                {
                                    try
                                    {
                                        int Index = Int32.Parse(CurrentPrefix[1]);
                                        ThisBone.BoneSliderIndex = Index;
                                    }
                                    catch
                                    {
                                        //無視する
                                    }
                                }
                                //DFS_LastKey
                                if (DFS_LastKey == DFS_TargetString)
                                {
                                    if (CurrentPrefix[CurrentPrefix.Length - 1].Contains("Bip01 BoneTail"))
                                        ThisBone.ShortName = CurrentPrefix[CurrentPrefix.Length - 1].Substring(("Bip01 BoneTail").Length);
                                    else if (CurrentPrefix[CurrentPrefix.Length - 1].Contains("BoneTail"))
                                        ThisBone.ShortName = CurrentPrefix[CurrentPrefix.Length - 1].Substring(("BoneTail").Length);
                                    else
                                        ThisBone.ShortName = CurrentPrefix[CurrentPrefix.Length - 1];
                                }
                                else
                                {
                                    ThisBone.ShortName = DFS_TargetString + "_" + CurrentPrefix[CurrentPrefix.Length - 1];
                                }
                            }
                            else
                            {
                                ThisBone.ShortName = ThisBone.BoneTransform.name;
                            }

                            DFS_ParentBones[DFS_TargetString].Add(ThisBone.BoneTransform, ThisBone);

                            BoneTransformStack.Push(ThisBone.BoneTransform);
                            ThisBoneCurrentLevel++;
                            tmpBone = ThisBone.BoneTransform;
                            ThisBoneCurrentIndex = 0;
                        }
                        else
                        {
                            //すでに存在します、無視する
                            ThisBoneCurrentIndex++;
                        }
                    }
                    else
                    {
                        BoneTransformStack.Pop();
                        ThisBoneCurrentLevel--;
                        if (BoneTransformStack.Count > 0)
                        {
                            tmpBone = BoneTransformStack.Peek();

                            DFS_CheckBonesPriorityName(tmpBone.name, out DFS_TargetString);
                            if (true == DFS_ParentBones[DFS_TargetString].ContainsKey(tmpBone))
                            {
                                ThisBoneCurrentIndex = DFS_ParentBones[DFS_TargetString][tmpBone].CurrentIndex++;
                            }
                            else
                            {
                                if (tmpBone.name.Contains("_nub"))
                                {
                                    //無視する
                                }
                                else if (tmpBone.name.Contains("_base"))
                                {
                                    //無視する
                                }
                                else
                                {
                                    Debuginfo.Warning("DFS: *** 致命的なエラー " + tmpBone.name + " 見つかりません", 1);
                                }
                            }
                        }
                    }
                }

                //整合性チェック
                BoneTransformStack.Clear();
                if (null != Bip01)
                {
                    DFS_CheckBonesPriorityName(Bip01.name, out DFS_TargetString);
                    if (true == DFS_ParentBones[DFS_TargetString].ContainsKey(Bip01))
                    {
                        if (null != Body_Bip01)
                        {
                            Bip01.transform.position = Body_Bip01.transform.position;
                            Bip01.transform.localPosition = Body_Bip01.transform.localPosition;
                            Bip01.transform.rotation = Body_Bip01.transform.rotation;
                            Bip01.transform.localRotation = Body_Bip01.transform.localRotation;
                            Bip01.transform.localScale = Body_Bip01.transform.localScale;
                        }
                        Body_Bip01 = null;
#if DEBUG
                        Debuginfo.Log("DFS: " + Bip01.name + "はリストから削除されました", 2);
#endif
                        DFS_ParentBones[DFS_TargetString].Remove(Bip01);
                    }
                }

                if (null != Bip01_Pelvis)
                {
                    DFS_CheckBonesPriorityName(Bip01_Pelvis.name, out DFS_TargetString);
                    if (true == DFS_ParentBones[DFS_TargetString].ContainsKey(Bip01_Pelvis))
                    {
                        if (null != Body_Bip01_Pelvis)
                        {
                            Bip01_Pelvis.transform.position = Body_Bip01_Pelvis.transform.position;
                            Bip01_Pelvis.transform.localPosition = Body_Bip01_Pelvis.transform.localPosition;
                            Bip01_Pelvis.transform.rotation = Body_Bip01_Pelvis.transform.rotation;
                            Bip01_Pelvis.transform.localRotation = Body_Bip01_Pelvis.transform.localRotation;
                            //Bip01_Pelvis.transform.localScale = Body_Bip01_Pelvis.transform.localScale;
                        }
                        Body_Bip01_Pelvis = null;
#if DEBUG
                        Debuginfo.Log("DFS: " + Bip01_Pelvis.name + "はリストから削除されました", 2);
#endif
                        DFS_ParentBones[DFS_TargetString].Remove(Bip01_Pelvis);
                    }
                }
            }
            BoneTransformStack = null;

            DFS_Count = 0;
            foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentCategory in DFS_ParentBones)
            {
                foreach (KeyValuePair<Transform, BoneData> CurrentBone in CurrentCategory.Value)
                {
#if DEBUG
                    //Debuginfo.Log("DFS: 種別: " + CurrentCategory.Key + " / 名前: " + CurrentBone.Key.name + " / レベル: " + CurrentBone.Value.Level + " / 索引: " + CurrentBone.Value.Index + " / 派生:" + CurrentBone.Value.ChildrenCount + "", 2);
#endif
                    if ((CurrentBone.Key.name.Contains("_nub"))
                                || (CurrentBone.Key.name.Contains("_base"))
                                || (CurrentBone.Key.name.Contains("_yure_")) || (CurrentBone.Key.name.Contains("_Base"))
                                || (CurrentBone.Key.name.Contains("_HIDE_"))
                                )
                    {
                        //無視する
                    }
                    else
                    {
                        DFS_Count++;
                    }
                }
            }
#if DEBUG
            Debuginfo.Log("DFS: ボーンデータ合計 " + DFS_Count, 2);
#endif
        }

        private void CreateBoneTail()
        {
            DefaultBoneData.Clear();
            foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentCategory in DFS_ParentBones)
            {
                var Bones = from pair in CurrentCategory.Value
                            orderby pair.Value.BoneSliderIndex ascending
                            select pair;

                foreach (KeyValuePair<Transform, BoneData> CurrentBone in Bones)
                {
                    if ((CurrentBone.Key.name.Contains("_nub"))
                                || (CurrentBone.Key.name.Contains("_base"))
                                || (CurrentBone.Key.name.Contains("_yure_")) || (CurrentBone.Key.name.Contains("_Base"))
                                || (CurrentBone.Key.name.Contains("_HIDE_"))
                                )
                    {
                        Debuginfo.Log("非表示マークで「" + CurrentBone.Key.name + "」を非表示にします", 1);
                        continue;
                    }

                    if (CurrentBone.Value.Level == 0)
                    {
                        //L0 無視する
                        continue;
                    }
#if DEBUG
                    //Debuginfo.Log("DFS: 種別: " + CurrentCategory.Key + " / 名前: " + CurrentBone.Key.name + " / レベル: " + CurrentBone.Value.Level + " / 索引: " + CurrentBone.Value.Index + " / 派生:" + CurrentBone.Value.ChildrenCount + "", 2);
#endif
                    BonePosRotScaleInfo bi = new BonePosRotScaleInfo()
                    {
                        localPosition = CurrentBone.Value.BoneTransform.localPosition,
                        localRotation = CurrentBone.Value.BoneTransform.localRotation,
                        localScale = CurrentBone.Value.BoneTransform.localScale,
                        name = CurrentBone.Value.BoneTransform.name,
                        ShortName = CurrentBone.Value.ShortName
                    };
                    if(true == string.IsNullOrEmpty(bi.ShortName))
                    {
                        CurrentBone.Value.ShortName = "Bone";
                    }
                    DefaultBoneData.Add(CurrentBone.Value.BoneTransform, bi);

                    string BoneShowName = CurrentBone.Value.ShortName + "(" + "l" + CurrentBone.Value.Level.ToString() + "->i" + CurrentBone.Value.Index.ToString() + ")";
#if DEBUG
                    BoneTails.Add(BoneShowName, CurrentBone.Value.BoneTransform);
#else
                    BoneShowName = CurrentBone.Value.ShortName;
                    int WarpIndex = 1;
                    while (true == BoneTails.ContainsKey(BoneShowName))
                    {
                        BoneShowName = CurrentBone.Value.ShortName + "(" + WarpIndex.ToString() + ")";
                        WarpIndex++;
                    }
                    BoneTails.Add(BoneShowName, CurrentBone.Value.BoneTransform);
#endif

                    BoneCounts++;
                }
            }
        }

        private List<TBodySkin> CreateObjList(Maid maid)
        {
            if (null == maid)
                return null;

            List<TBodySkin> objList = new List<TBodySkin>();

            for (int Index = 0; Index < maid.body0.goSlot.Count; Index++)
            {
                TBodySkin tbs = maid.body0.goSlot[Index];
                if (null != tbs.obj)
                {
                    if (TBody.SlotID.body == tbs.SlotId)
                    {
                        objList.Add(tbs);
                    }

                    if (TBody.SlotID.accShippo == tbs.SlotId)
                    {
                        objList.Add(tbs);
                    }
                    else if (TBody.SlotID.accSenaka == tbs.SlotId)
                    {
                        objList.Add(tbs);
                    }
                }
            }

            return objList;
        }

        private List<EMES_SceneManagement.BonePosRotScaleInfo> CreateBonePosRotScaleInfo()
        {
            List<EMES_SceneManagement.BonePosRotScaleInfo> boneData = new List<EMES_SceneManagement.BonePosRotScaleInfo>();
            foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentCategory in DFS_ParentBones)
            {
                var Bones = from pair in CurrentCategory.Value
                            orderby pair.Value.BoneSliderIndex ascending
                            select pair;

                foreach (KeyValuePair<Transform, BoneData> CurrentBone in Bones)
                {
                    if ((CurrentBone.Key.name.Contains("_nub"))
                                || (CurrentBone.Key.name.Contains("_base"))
                                || (CurrentBone.Key.name.Contains("_yure_")) || (CurrentBone.Key.name.Contains("_Base"))
                                || (CurrentBone.Key.name.Contains("_HIDE_"))
                                )
                    {
                        continue;
                    }
                    else if (false == CurrentBone.Key.name.ToString().Contains("BoneTail")
                        && "Bone_center" != CurrentBone.Key.name.ToString()
                        && "Bip01" != CurrentBone.Key.name.ToString()
                        && false == CurrentBone.Key.name.ToString().Contains("Bip01")
                        )
                    {
                        continue;
                    }

                        if (CurrentBone.Value.Level == 0)
                    {
                        //L0 無視する
                        continue;
                    }

                    EMES_SceneManagement.BonePosRotScaleInfo bi = new EMES_SceneManagement.BonePosRotScaleInfo()
                    {
                        localPosition_x = CurrentBone.Value.BoneTransform.localPosition.x,
                        localPosition_y = CurrentBone.Value.BoneTransform.localPosition.y,
                        localPosition_z = CurrentBone.Value.BoneTransform.localPosition.z,
                        localScale = CurrentBone.Value.BoneTransform.localScale.x,
                        localRotation_x = CurrentBone.Value.BoneTransform.localRotation.x,
                        localRotation_y = CurrentBone.Value.BoneTransform.localRotation.y,
                        localRotation_z = CurrentBone.Value.BoneTransform.localRotation.z,
                        localRotation_w = CurrentBone.Value.BoneTransform.localRotation.w,

                        name = CurrentBone.Value.BoneTransform.name,
                        ShortName = CurrentBone.Value.ShortName
                    };

                    boneData.Add(bi);
                }
            }

            return boneData;
        }

        private void RotateBonePosRotScaleInfo(List<EMES_SceneManagement.BonePosRotScaleInfo> boneData)
        {
            int Index = 0;
            bool bBreak = false;
            foreach (KeyValuePair<string, Dictionary<Transform, BoneData>> CurrentCategory in DFS_ParentBones)
            {
                var Bones = from pair in CurrentCategory.Value
                            orderby pair.Value.BoneSliderIndex ascending
                            select pair;

                foreach (KeyValuePair<Transform, BoneData> CurrentBone in Bones)
                {
                    if ((CurrentBone.Key.name.Contains("_nub"))
                                || (CurrentBone.Key.name.Contains("_base"))
                                || (CurrentBone.Key.name.Contains("_yure_")) || (CurrentBone.Key.name.Contains("_Base"))
                                || (CurrentBone.Key.name.Contains("_HIDE_"))
                                )
                    {
                        continue;
                    }
                    else if (false == CurrentBone.Key.name.ToString().Contains("BoneTail")
                            && "Bone_center" != CurrentBone.Key.name.ToString()
                            && "Bip01" != CurrentBone.Key.name.ToString()
                            && false == CurrentBone.Key.name.ToString().Contains("Bip01")
                            )
                    {
                        continue;
                    }

                    if (CurrentBone.Value.Level == 0)
                    {
                        //L0 無視する
                        continue;
                    }

                    if (Index < boneData.Count)
                    {
                        CurrentBone.Value.BoneTransform.localPosition = new Vector3(boneData[Index].localPosition_x, boneData[Index].localPosition_y, boneData[Index].localPosition_z);
                        CurrentBone.Value.BoneTransform.localScale = new Vector3(boneData[Index].localScale, boneData[Index].localScale, boneData[Index].localScale);
                        CurrentBone.Value.BoneTransform.localRotation = new Quaternion(boneData[Index].localRotation_x, boneData[Index].localRotation_y, boneData[Index].localRotation_z, boneData[Index].localRotation_w);
                        Index++;
                    }
                    else
                    {
                        Debuginfo.Log("内容の不一致が発生します、停止します Index="+ Index.ToString(), 1);
                        bBreak = true;
                        break;
                    }
                }
                if (true == bBreak)
                    break;
            }
        }

#endregion
    }
}

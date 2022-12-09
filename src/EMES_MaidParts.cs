using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using wf;
using Random = System.Random;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class EMES_MaidParts
    {
        public readonly List<string> PartsSlotIgnoreList = new List<string>()
        {
            //"body",
            "moza"
        };

        private EMES Super;

        #region public method
        public EMES_MaidParts(EMES super)
        {
            Super = super;
        }

        public bool Init(Maid maid)
        {
            if(false == maid.IsBusy)
            {
                Super.Items.Items_RemoveCategory("MaidPartsHandle");
                return UpdateMaidParts(maid);
            }

            return false;
        }

        public void Parts_Finalized()
        {

        }

        public void EnumParts(HandleEx handle)
        {
            Component[] com = handle.parentBone.GetComponentsInChildren(typeof(Component));
            for (int i = 0; i < com.Length; i++)
            {
                Type tCom = com[i].GetType();
#if DEBUG
                Debuginfo.Log("Type [" + tCom + "] >>  [" + (i + 1).ToString() + "] >>> " + com[i].name, 2);
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
        }
        #endregion

        #region privage method
        private bool UpdateMaidParts(Maid maid)
        {
            bool bChanged = false;
            foreach(TBody.SlotID slotID in Enum.GetValues(typeof(TBody.SlotID)))
            {
                string sSlotName = Enum.GetName(typeof(TBody.SlotID), slotID);
                if (true == sSlotName.Equals("end"))
                    break;

                if (true == PartsSlotIgnoreList.Contains(sSlotName))
                    continue;

                if (true == maid.body0.GetSlotLoaded(slotID))
                {                   
                    HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, maid.body0.GetSlot((int)slotID).obj, false, true);
                    handle.SetupItem(maid.status.guid + "_" + sSlotName, maid.status.callName + "_" + sSlotName, "MaidPartsHandle");
                    Super.Items.Items_ItemHandle.Add(handle, maid.status.callName + "_" + sSlotName);
                    bChanged = true;
                }
            }

            return bChanged;                    
        }
        #endregion
    }
}



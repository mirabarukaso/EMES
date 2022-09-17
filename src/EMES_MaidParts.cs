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
        private EMES Super;
        private readonly List<string> PartsSlotIgnoreList = new List<string>()
        {
            //"body",
            "moza"
        };

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



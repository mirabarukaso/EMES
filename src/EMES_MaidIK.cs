using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class EMES_MaidIK
    {
        private EMES Super;
        public EMES_MaidIK(EMES super)
        {
            Super = super;
        }

        #region EMES IK manager 
        public enum BoneType
        {
            Offset,
            Root,
            Head,
            Neck,
            Pelvis,
            Spine,
            Spine0a,
            Spine1,
            Spine1a,
            Mune_R,
            R_Clavicle,
            R_UpperArm,
            R_Forearm,
            R_Hand,
            Hip_R,
            R_Thigh,
            R_Calf,
            R_Foot,
            Mune_L,
            L_Clavicle,
            L_UpperArm,
            L_Forearm,
            L_Hand,
            Hip_L,
            L_Thigh,
            L_Calf,
            L_Foot,
            R_Finger0,
            R_Finger01,
            R_Finger02,
            R_Finger1,
            R_Finger11,
            R_Finger12,
            R_Finger2,
            R_Finger21,
            R_Finger22,
            R_Finger3,
            R_Finger31,
            R_Finger32,
            R_Finger4,
            R_Finger41,
            R_Finger42,
            L_Finger0,
            L_Finger01,
            L_Finger02,
            L_Finger1,
            L_Finger11,
            L_Finger12,
            L_Finger2,
            L_Finger21,
            L_Finger22,
            L_Finger3,
            L_Finger31,
            L_Finger32,
            L_Finger4,
            L_Finger41,
            L_Finger42,
            R_Toe0,
            R_Toe01,
            R_Toe1,
            R_Toe11,
            R_Toe2,
            R_Toe21,
            L_Toe0,
            L_Toe01,
            L_Toe1,
            L_Toe11,
            L_Toe2,
            L_Toe21,
            Mouth,
            Mune_L_Sub,
            Mune_R_Sub,
            Nipple_L,
            Nipple_R,
            Eye_L,
            Eye_R
        }

        public enum BoneSetType
        {
            Root,
            Head,
            Body,
            Spine,
            Mune,
            RightArm,
            R_Finger,
            LeftArm,
            L_Finger,
            RightLeg,
            R_Toe,
            LeftLeg,
            L_Toe,
            Face
        }

        public enum IKEffectorType
        {
            Body = 0,
            Hand_R = 1,
            Forearm_R = 2,
            UpperArm_R = 3,
            Hand_L = 4,
            Forearm_L = 5,
            UpperArm_L = 6,
            Foot_R = 7,
            Calf_R = 8,
            Thigh_R = 9,
            Foot_L = 10,
            Calf_L = 11,
            Thigh_L = 12,
            Head = 13,
            Penis = 14,
            Bust_R = 15,
            Bust_L = 16,
            Mouth = 17,
            Finger_R0 = 18,
            Finger_R1 = 19,
            Finger_R2 = 20,
            Finger_R3 = 21,
            Finger_R4 = 22,
            Finger_L0 = 23,
            Finger_L1 = 24,
            Finger_L2 = 25,
            Finger_L3 = 26,
            Finger_L4 = 27,
            Toe_R0,
            Toe_R1,
            Toe_R2,
            Toe_L0,
            Toe_L1,
            Toe_L2
        }

        public enum IKBoneBinding
        {
            無し,
            頭,
            背中,
            左手,
            右手,
            左足,
            右足,
            XXX前,
            XXX後
        }

        public Dictionary<BoneType, KeyValuePair<BoneSetType, GameObject>> CreateBoneDic(Maid maid)
        {
            if (maid == null)
            {
                return null;
            }
            string text = (!maid.boMAN) ? "Bip01" : "ManBip";
            Dictionary<BoneType, KeyValuePair<BoneSetType, GameObject>> dictionary = new Dictionary<BoneType, KeyValuePair<BoneSetType, GameObject>>();
            dictionary.Add(BoneType.Offset, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Root, maid.body0.transform.gameObject));
            dictionary.Add(BoneType.Root, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Root, maid.body0.GetBone(text).gameObject));
            dictionary.Add(BoneType.Head, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Head, maid.body0.GetBone(text + " Head").gameObject));
            dictionary.Add(BoneType.Neck, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Head, maid.body0.GetBone(text + " Neck").gameObject));
            dictionary.Add(BoneType.Pelvis, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Pelvis").gameObject));
            dictionary.Add(BoneType.Spine, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Spine0a, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine0a").gameObject));
                dictionary.Add(BoneType.Spine1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine1").gameObject));
                dictionary.Add(BoneType.Spine1a, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine1a").gameObject));
            }
            else
            {
                dictionary.Add(BoneType.Spine1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine1").gameObject));
                dictionary.Add(BoneType.Spine1a, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Spine, maid.body0.GetBone(text + " Spine2").gameObject));
            }
            //dictionary.Add(BoneType.Mouth, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Hide, maid.body0.GetBone("Mouth").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Mune_R, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Mune_R").gameObject));
                //dictionary.Add(BoneType.Mune_R_Sub, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Mune_R_sub").gameObject));
                //dictionary.Add(BoneType.Nipple_R, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Nipple_R").gameObject));
            }
            dictionary.Add(BoneType.R_Clavicle, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightArm, maid.body0.GetBone(text + " R Clavicle").gameObject));
            dictionary.Add(BoneType.R_UpperArm, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightArm, maid.body0.GetBone(text + " R UpperArm").gameObject));
            dictionary.Add(BoneType.R_Forearm, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightArm, maid.body0.GetBone(text + " R Forearm").gameObject));
            dictionary.Add(BoneType.R_Hand, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightArm, maid.body0.GetBone(text + " R Hand").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Mune_L, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Mune_L").gameObject));
                //dictionary.Add(BoneType.Mune_L_Sub, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Mune_L_sub").gameObject));
                //dictionary.Add(BoneType.Nipple_L, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Mune, maid.body0.GetBone("Nipple_L").gameObject));
            }
            dictionary.Add(BoneType.L_Clavicle, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftArm, maid.body0.GetBone(text + " L Clavicle").gameObject));
            dictionary.Add(BoneType.L_UpperArm, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftArm, maid.body0.GetBone(text + " L UpperArm").gameObject));
            dictionary.Add(BoneType.L_Forearm, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftArm, maid.body0.GetBone(text + " L Forearm").gameObject));
            dictionary.Add(BoneType.L_Hand, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftArm, maid.body0.GetBone(text + " L Hand").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Hip_R, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightLeg, maid.body0.GetBone("Hip_R").gameObject));
            }
            dictionary.Add(BoneType.R_Thigh, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightLeg, maid.body0.GetBone(text + " R Thigh").gameObject));
            dictionary.Add(BoneType.R_Calf, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightLeg, maid.body0.GetBone(text + " R Calf").gameObject));
            dictionary.Add(BoneType.R_Foot, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.RightLeg, maid.body0.GetBone(text + " R Foot").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Hip_L, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftLeg, maid.body0.GetBone("Hip_L").gameObject));
            }
            dictionary.Add(BoneType.L_Thigh, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftLeg, maid.body0.GetBone(text + " L Thigh").gameObject));
            dictionary.Add(BoneType.L_Calf, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftLeg, maid.body0.GetBone(text + " L Calf").gameObject));
            dictionary.Add(BoneType.L_Foot, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.LeftLeg, maid.body0.GetBone(text + " L Foot").gameObject));

            dictionary.Add(BoneType.R_Finger0, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger0").gameObject));
            dictionary.Add(BoneType.R_Finger01, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger01").gameObject));
            dictionary.Add(BoneType.R_Finger02, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger02").gameObject));
            dictionary.Add(BoneType.R_Finger1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger1").gameObject));
            dictionary.Add(BoneType.R_Finger11, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger11").gameObject));
            dictionary.Add(BoneType.R_Finger12, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger12").gameObject));
            dictionary.Add(BoneType.R_Finger2, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger2").gameObject));
            dictionary.Add(BoneType.R_Finger21, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger21").gameObject));
            dictionary.Add(BoneType.R_Finger22, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger22").gameObject));
            dictionary.Add(BoneType.R_Finger3, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger3").gameObject));
            dictionary.Add(BoneType.R_Finger31, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger31").gameObject));
            dictionary.Add(BoneType.R_Finger32, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger32").gameObject));
            dictionary.Add(BoneType.R_Finger4, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger4").gameObject));
            dictionary.Add(BoneType.R_Finger41, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger41").gameObject));
            dictionary.Add(BoneType.R_Finger42, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Finger, maid.body0.GetBone(text + " R Finger42").gameObject));

            dictionary.Add(BoneType.L_Finger0, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger0").gameObject));
            dictionary.Add(BoneType.L_Finger01, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger01").gameObject));
            dictionary.Add(BoneType.L_Finger02, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger02").gameObject));
            dictionary.Add(BoneType.L_Finger1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger1").gameObject));
            dictionary.Add(BoneType.L_Finger11, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger11").gameObject));
            dictionary.Add(BoneType.L_Finger12, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger12").gameObject));
            dictionary.Add(BoneType.L_Finger2, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger2").gameObject));
            dictionary.Add(BoneType.L_Finger21, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger21").gameObject));
            dictionary.Add(BoneType.L_Finger22, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger22").gameObject));
            dictionary.Add(BoneType.L_Finger3, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger3").gameObject));
            dictionary.Add(BoneType.L_Finger31, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger31").gameObject));
            dictionary.Add(BoneType.L_Finger32, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger32").gameObject));
            dictionary.Add(BoneType.L_Finger4, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger4").gameObject));
            dictionary.Add(BoneType.L_Finger41, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger41").gameObject));
            dictionary.Add(BoneType.L_Finger42, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Finger, maid.body0.GetBone(text + " L Finger42").gameObject));

            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.R_Toe2, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe2").gameObject));
                dictionary.Add(BoneType.R_Toe21, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe21").gameObject));
            }
            dictionary.Add(BoneType.R_Toe1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe1").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.R_Toe11, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe11").gameObject));
            }
            dictionary.Add(BoneType.R_Toe0, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe0").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.R_Toe01, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.R_Toe, maid.body0.GetBone(text + " R Toe01").gameObject));
            }
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.L_Toe2, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe2").gameObject));
                dictionary.Add(BoneType.L_Toe21, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe21").gameObject));
            }
            dictionary.Add(BoneType.L_Toe1, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe1").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.L_Toe11, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe11").gameObject));
            }
            dictionary.Add(BoneType.L_Toe0, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe0").gameObject));
            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.L_Toe01, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.L_Toe, maid.body0.GetBone(text + " L Toe01").gameObject));
            }

            if (!maid.boMAN)
            {
                dictionary.Add(BoneType.Eye_L, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Face, maid.body0.trsEyeL.gameObject));
                dictionary.Add(BoneType.Eye_R, new KeyValuePair<BoneSetType, GameObject>(BoneSetType.Face, maid.body0.trsEyeR.gameObject));
            }
            return dictionary;
        }
        
        public class CustomIKCMO : MonoBehaviour
        {
            private TBody body;
            private float defLEN1;
            private float defLEN2;
            private Vector3 knee_old;
            private Quaternion defHipQ;
            private Quaternion defKneeQ;
            private Vector3 vechand;

            public void Init(Transform hip, Transform knee, Transform ankle, TBody b)
            {
                body = b;
                defLEN1 = (hip.position - knee.position).magnitude;
                defLEN2 = (ankle.position - knee.position).magnitude;
                knee_old = knee.position;
                defHipQ = hip.localRotation;
                defKneeQ = knee.localRotation;
                vechand = Vector3.zero;
            }

            public void Porc(Transform hip, Transform knee, Transform ankle, Vector3 tgt, Vector3 vechand_offset)
            {
                tgt += vechand;
                if ((knee.position - knee_old).sqrMagnitude > 1f)
                {
                    knee_old = knee.position;
                }
                knee_old = knee_old * 0.5f + knee.position * 0.5f;
                Vector3 normalized = (knee_old - tgt).normalized;
                knee_old = tgt + normalized * defLEN2;
                Vector3 normalized2 = (knee_old - hip.position).normalized;
                knee_old = hip.position + normalized2 * defLEN1;
                if (Vector3.zero == normalized2)
                {
                    normalized2 = Vector3.one;
                }
                default(Quaternion).SetLookRotation(normalized2);
                hip.localRotation = defHipQ;
                hip.transform.rotation = Quaternion.FromToRotation(knee.transform.position - hip.transform.position, knee_old - hip.transform.position) * hip.transform.rotation;

                knee.localRotation = defKneeQ;
                knee.transform.rotation = Quaternion.FromToRotation(ankle.transform.position - knee.transform.position, tgt - knee.transform.position) * knee.transform.rotation;
                vechand = ankle.rotation * vechand_offset;
            }
        }
        #endregion

        #region Variables
        private bool bIKLocked = false;
        private bool bIKInitCompleted = false;

        public Dictionary<int, EMES_IK> MaidsIK { get; private set; }
        public class EMES_IK
        {
            public Maid maid;
            public int maidStockID;
            public Dictionary<BoneType, KeyValuePair<BoneSetType, GameObject>> BoneDict;
            public Dictionary<BoneType, HandleEx> handleEx = new Dictionary<BoneType, HandleEx>();
            public bool bInvisible = false;
            public List<IK> IKCMO = new List<IK>();

            public bool bIKLA = false;
            public bool bIKRA = false;
            public bool bIKLL = false;
            public bool bIKRL = false;

            public Quaternion Bakcup_Eye_L = Quaternion.identity;
            public Quaternion Bakcup_Eye_R = Quaternion.identity;

            public class IK
            {
                public IKEffectorType GetIKEffectorType;
                public CustomIKCMO GetIKCMO;
                public BoneType GetBoneType;

                public Transform hip;
                public Transform knee;
                public Transform ankle;
                public HandleEx handle;
            }
        };

        public class UniversalPoint
        {
            public GameObject target;
            public HandleEx handle;

            public UniversalPoint(Vector3 pos, Transform parent, PrimitiveType pt)
            {
                target = GameObject.CreatePrimitive(pt);
                BoxCollider colliderR = target.GetComponent<Collider>() as BoxCollider;
                Mesh mesh = target.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Count(); ++i)
                {
                    vertices[i].x *= 0.05f;
                    vertices[i].y *= 0.05f;
                    vertices[i].z *= 0.05f;
                }
                mesh.vertices = vertices;

                Vector2[] uv = mesh.uv;
                for (int i = 0; i < uv.Count(); ++i)
                {
                    uv[i].x = 0.25f * uv[i].x + 0 * 0.25f;
                    uv[i].y = 0.25f * uv[i].y + 2 * 0.25f;
                }
                mesh.uv = uv;
                target.GetComponent<Renderer>().receiveShadows = false;
                target.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                target.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                target.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                target.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                target.transform.parent = parent;
                target.transform.position = pos;
            }

            public void DestoryTarget()
            {
                if (null != target)
                {
                    target.SetActive(false);
                    UnityEngine.Object.Destroy(target);
                }
            }
        }
        public Dictionary<string, UniversalPoint> MaidGazePoint { get; private set; }
        public Dictionary<HandleEx, UniversalPoint> SublightPoint { get; private set; }
        #endregion

        #region public method
        public void IK_Init(string guid, int StockID)
        {
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(guid);
            if (null == maid)
                return;

            if (true == maid.IsBusy)
                return;

#if DEBUG
            Debuginfo.Log("ParentBoneが行方不明です、新しいボディを変更していますか？", 2);
#endif
            IK_Init(maid, StockID);
            Super.Window.TryAddCustomFaceBlend(StockID);
            maid.body0.MoveHeadAndEye();
        }

        public void IK_Init(Maid maid, int maidStockID)
        {
            bIKInitCompleted = false;
            if(null == MaidsIK)
            {
                MaidsIK = new Dictionary<int, EMES_IK>();
            }
            if (true == MaidsIK.ContainsKey(maidStockID))
            {
                Debuginfo.Log("IK更新 " + maid.status.firstName + " " + maid.status.lastName, 1);
                foreach(KeyValuePair<BoneType, HandleEx> handle in MaidsIK[maidStockID].handleEx)
                {
                    handle.Value.Visible = false;
                    handle.Value.Destroy();
                }
                MaidsIK[maidStockID].IKCMO.Clear();
                MaidsIK[maidStockID].handleEx.Clear();
                MaidsIK.Remove(maidStockID);

                IK_RemoveGazePoint(maid, false);
            }
            EMES_IK newIK = new EMES_IK();
            newIK.maid = maid;
            newIK.maidStockID = maidStockID;
            newIK.BoneDict = CreateBoneDic(maid);
            foreach (KeyValuePair<BoneType, KeyValuePair<BoneSetType, GameObject>> bone in newIK.BoneDict)
            {
                HandleEx handle = new HandleEx(bone.Key, bone.Value.Key, bone.Value.Value, false, true);
                handle.IKmode = HandleEx.IKMODE.None;
                newIK.handleEx.Add(bone.Key, handle);
            }
            newIK.bInvisible = true;
            newIK.Bakcup_Eye_L = maid.body0.trsEyeL.localRotation;
            newIK.Bakcup_Eye_R = maid.body0.trsEyeR.localRotation;
            MaidsIK.Add(maidStockID, newIK);

            if (null == MaidGazePoint)
            {
                MaidGazePoint = new Dictionary<string, UniversalPoint>();
            }
            if (null == SublightPoint)
            {
                SublightPoint = new Dictionary<HandleEx, UniversalPoint>();
            }

            bIKInitCompleted = true;
        }

        public void IK_Finalized()
        {
#if DEBUG
            Debuginfo.Log("EMES_IK Finalize ...", 2);
#endif
            bIKInitCompleted = false;

            if (null != MaidGazePoint)
            {
                foreach (KeyValuePair<string, UniversalPoint> gp in MaidGazePoint)
                {
                    gp.Value.handle.Visible = false;
                    gp.Value.handle.Destroy();
                    gp.Value.DestoryTarget();
                }
                MaidGazePoint.Clear();
                MaidGazePoint = null;
            }

            if (null != SublightPoint)
            {
                foreach (KeyValuePair<HandleEx, UniversalPoint> lp in SublightPoint)
                {
                    lp.Value.handle.Visible = false;
                    lp.Value.handle.Destroy();
                    lp.Value.DestoryTarget();
                }
                SublightPoint.Clear();
                SublightPoint = null;
            }

            foreach (KeyValuePair<int, EMES_IK> maidIK in MaidsIK)
            {
                maidIK.Value.IKCMO.Clear();
                maidIK.Value.handleEx.Clear();
            }
            MaidsIK.Clear();
            MaidsIK = null;
#if DEBUG
            Debuginfo.Log("EMES_IK Finalized Done", 2);
#endif
        }

        public void IK_Collect()
        {
            foreach(KeyValuePair<int, EMES_IK> ik in MaidsIK.ToList())
            {
                if(false == ik.Value.maid.Visible)
                {
                    Debuginfo.Log("IK回収 " + ik.Value.maid.status.firstName + " " + ik.Value.maid.status.lastName, 1);
                    foreach (KeyValuePair<BoneType, HandleEx> handle in ik.Value.handleEx)
                    {
                        handle.Value.Visible = false;
                        handle.Value.Destroy();
                    }
                    ik.Value.IKCMO.Clear();
                    ik.Value.handleEx.Clear();
                    MaidsIK.Remove(ik.Key);
                }
            }
        }

        public void IK_Attach(int MaidStockID, IKEffectorType iket, BoneType end, BoneType mid, BoneType root, float scale)
        {
            if (false == MaidsIK.ContainsKey(MaidStockID))
                return;

            HandleEx handleEnd = IK_SetBoneTypeVisible(MaidStockID, end, false);
            HandleEx handleMid = IK_SetBoneTypeVisible(MaidStockID, mid, false);
            HandleEx handleRoot = IK_SetBoneTypeVisible(MaidStockID, root, true);
            handleRoot.IK_ChangeHandleKunModeIK(true);
            handleRoot.IKmode = HandleEx.IKMODE.UniversalIK;
            handleRoot.Scale = scale;

            EMES_IK.IK ik = new EMES_IK.IK();
            ik.GetIKEffectorType = iket;
            ik.GetBoneType = root;
            ik.hip = handleEnd.GetParentBone();
            ik.knee = handleMid.GetParentBone();
            ik.ankle = handleRoot.GetParentBone();
            ik.handle = handleRoot;
            ik.GetIKCMO = new CustomIKCMO();
            ik.GetIKCMO.Init(ik.hip, ik.knee, ik.ankle, MaidsIK[MaidStockID].maid.body0);
            MaidsIK[MaidStockID].IKCMO.Add(ik);
            MaidsIK[MaidStockID].bInvisible = false;
        }

        public void IK_Deatch(int MaidStockID, IKEffectorType iket)
        {
            if(0 < MaidsIK[MaidStockID].IKCMO.Count)
            {
                int Index = 0;
                foreach(EMES_IK.IK ik in MaidsIK[MaidStockID].IKCMO.ToList())
                {
                    if (iket == ik.GetIKEffectorType)
                    {
                        MaidsIK[MaidStockID].IKCMO.RemoveAt(Index);
                        HandleEx handleRoot = IK_SetBoneTypeVisible(MaidStockID, ik.GetBoneType, false);
                        handleRoot.IK_ChangeHandleKunModeIK(false);
                        handleRoot.IKmode = HandleEx.IKMODE.None;
                        handleRoot.Visible = false;
                        handleRoot.Scale = 0f;
                        break;
                    }
                    Index++;
                }
            }
        }

        public bool IK_DetachAll(int MaidStockID)
        {
            bool Deatached = false;
            if (0 < MaidsIK[MaidStockID].IKCMO.Count)
            {
                int Index = 0;
                foreach (EMES_IK.IK ik in MaidsIK[MaidStockID].IKCMO)
                {                   
                    HandleEx handleRoot = IK_SetBoneTypeVisible(MaidStockID, ik.GetBoneType, false);
                    handleRoot.IK_ChangeHandleKunModeIK(false);
                    handleRoot.IKmode = HandleEx.IKMODE.None;
                    handleRoot.Visible = false;
                    handleRoot.Scale = 0f;
                    Index++;
                }
                Deatached = true;
            }
            
            MaidsIK[MaidStockID].IKCMO.Clear();
            MaidsIK[MaidStockID].bIKRA = false;
            MaidsIK[MaidStockID].bIKLA = false;
            MaidsIK[MaidStockID].bIKRL = false;
            MaidsIK[MaidStockID].bIKLL = false;

            return Deatached;
        }

        public int IK_AutoIK_Attach(int MaidStockID, Transform baseRoot, int chainCount, float scale)
        {
            IK_AutoIK_Deatch();

            Transform root = baseRoot;
            List<Transform> trBones = new List<Transform>();
            for (int Index = 0; Index < chainCount; Index++)
            {
                Transform mid = root.parent ?? null;
                Transform end = mid.parent ?? null;

                if(null != end)
                {
                    if (("Bip01 Pelvis" == end.name || "Bip01" == end.name || "Bone_center" == end.name) ||
                        ("Bip01 Pelvis" == mid.name || "Bip01" == mid.name || "Bone_center" == mid.name) ||
                        ("Bip01 Pelvis" == root.name || "Bip01" == root.name || "Bone_center" == root.name) )
                    {
#if DEBUG
                        Debuginfo.Log("〆 root=" + root.name + " mid=" + mid.name + " end=" + end.name, 2);
#endif
                        break;
                    }
                    else
                    {
#if DEBUG
                        Debuginfo.Log("Add " + root.name + " mid=" + mid.name + " end=" + end.name, 2);
#endif
                        trBones.Add(root);
                        root = mid;
                    }
                }
                else
                {
                    break;
                }
            }

            if (0 != trBones.Count)
            {
#if DEBUG
                Debuginfo.Log("CreateTailHandleAutoIK trBones.count=" + trBones.Count, 2);
#endif
                Super.maidTails.CreateTailHandleAutoIK(trBones, scale);
                Super.maidTails.bIsAutoIK = true;
#if DEBUG
                Debuginfo.Log("CreateTailHandleAutoIK Done", 2);
#endif
            }

            return trBones.Count;
        }

        public void IK_AutoIK_Deatch()
        {
            Super.maidTails.bIsAutoIK = false;
            foreach (EMES_IK.IK ik in Super.maidTails.AutoIK)
            {
                ik.handle.Visible = false;
                ik.handle.Destroy();
                ik.GetIKCMO = null;
            }
            Super.maidTails.AutoIK.Clear();
        }

        public void IK_SetAllHandleVisible(int MaidStockID, bool bVisible)
        {
            if (false == MaidsIK.ContainsKey(MaidStockID))
                return;

            if (true == MaidsIK[MaidStockID].bInvisible)
                return;

            foreach(BoneSetType boneSetType in Enum.GetValues(typeof(BoneSetType)))
            {
                IK_SetBoneSetTypeVisible(MaidStockID, boneSetType, bVisible);
            }

            MaidsIK[MaidStockID].bInvisible = !bVisible;
        }

        public HandleEx IK_SetBoneTypeVisible(int MaidStockID, BoneType boneSetType, bool bVisible)
        {
            if (true == MaidsIK.ContainsKey(MaidStockID))
            {
                EMES_IK maidIK = MaidsIK[MaidStockID];
                HandleEx handleEx;
                if (true == maidIK.handleEx.TryGetValue(boneSetType, out handleEx))
                {
                    handleEx.Visible = bVisible;

                    if (true == bVisible)
                        MaidsIK[MaidStockID].bInvisible = false;

                    return handleEx;
                }
            }
            return null;
        }

        public List<HandleEx> IK_SetBoneSetTypeVisible(int MaidStockID, BoneSetType boneSetType, bool bVisible)
        {
            if(true == MaidsIK.ContainsKey(MaidStockID))
            {
                List<HandleEx> handles = new List<HandleEx>();
                EMES_IK maidIK = MaidsIK[MaidStockID];
                foreach (KeyValuePair<BoneType, KeyValuePair<BoneSetType, GameObject>> bone in maidIK.BoneDict)
                {
                    if (boneSetType == maidIK.handleEx[bone.Key].boneSetType)
                    {                       
                        maidIK.handleEx[bone.Key].Visible = bVisible;
                        if (true == bVisible)
                            MaidsIK[MaidStockID].bInvisible = false;
                        handles.Add(maidIK.handleEx[bone.Key]);
                    }
                }
                return handles;
            }

            return null;
        }

        public List<HandleEx> IK_GetHandleByBoneSetTyoe(int MaidStockID, BoneSetType boneSetType)
        {
            if (true == MaidsIK.ContainsKey(MaidStockID))
            {
                List<HandleEx> handles = new List<HandleEx>();
                EMES_IK maidIK = MaidsIK[MaidStockID];
                foreach (KeyValuePair<BoneType, KeyValuePair<BoneSetType, GameObject>> bone in maidIK.BoneDict)
                {
                    if (boneSetType == maidIK.handleEx[bone.Key].boneSetType)
                    {
                        handles.Add(maidIK.handleEx[bone.Key]);
                    }
                }
                return handles;
            }
            return null;
        }

        public void LockIK()
        {
            bIKLocked = true;
        }

        public void UnLockIK()
        {
            bIKLocked = false;
        }

        public bool IsLockIK()
        {
            return bIKLocked;
        }

        public bool IsIKInitCompleted()
        {
            return bIKInitCompleted;
        }

        public void IK_AddGazePoint(Maid maid)
        {
            if(false == MaidGazePoint.ContainsKey(maid.status.guid))
            {
#if DEBUG
                Debuginfo.Log("IK_AddGazePoint at maid = " + maid.status.callName, 2);
#endif
                Vector3 pos = CMT.SearchObjName(maid.body0.m_trBones, "Bip01 Head", true).position;
                pos.z += 1;
                UniversalPoint gp = new UniversalPoint(pos, Super.Items.goItemMaster.transform, PrimitiveType.Sphere);
                //ワイルドハンドル
                HandleEx handle = new HandleEx(BoneType.Root, BoneSetType.Root, gp.target, false, true);
                handle.SetupItem("GazePoint", maid.status.guid, "WildHandle");
                handle.IKmode = HandleEx.IKMODE.UniversalIK;
#if DEBUG
                Debuginfo.Log("ItemHandle.Add at maid = " + maid.status.guid, 2);
#endif
                Super.Items.Items_ItemHandle.Add(handle, "注視点_" + maid.status.callName);
                gp.handle = handle;
                MaidGazePoint.Add(maid.status.guid, gp);
            }

            maid.EyeToTargetObject(MaidGazePoint[maid.status.guid].target.transform, 0.5f);
        }

        public void IK_RemoveGazePoint(Maid maid, bool bSorasu)
        {
            maid.EyeToTargetObject(GameMain.Instance.MainCamera.transform, 0.5f);

            if (true == MaidGazePoint.ContainsKey(maid.status.guid))
            {
                HandleEx handle = MaidGazePoint[maid.status.guid].handle;
#if DEBUG
                Debuginfo.Log("ItemHandle.Remove at maid = " + maid.status.guid, 2);
#endif
                handle.Visible = false;
                Super.Items.Items_ItemHandle.Remove(handle);
                handle.Destroy();

#if DEBUG
                Debuginfo.Log("IK_RemoveGazePoint at maid = " + maid.status.guid, 2);
#endif
                MaidGazePoint[maid.status.guid].DestoryTarget();
                MaidGazePoint.Remove(maid.status.guid);
            }

            if (true == bSorasu)
            {
                maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, 0.8f);
            }
            else
            {
                maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 0.8f);
            }
        }

        public GameObject IK_AddSubLightPoint(GameObject goSubLight, int Index, out HandleEx handle)
        {
            //ワイルドハンドル
            Vector3 pos = Super.Window.CurrentSelectedMaid.GetPos();
            pos.y += 1;
            pos.z += 1;
            goSubLight.transform.position = pos;
            handle = new HandleEx(BoneType.Root, BoneSetType.Root, goSubLight, false, true);
            handle.SetupItem("SubLightPoint_" + Index.ToString(), goSubLight.name, "WildHandle");
            handle.IKmode = HandleEx.IKMODE.UniversalIK;
            handle.Pos = goSubLight.transform.position;
            Super.Items.Items_ItemHandle.Add(handle, "光源_" + Index.ToString());
#if DEBUG
            Debuginfo.Log("IK_AddSubLightPoint at goSubLight.name = " + goSubLight.name, 2);
#endif

            UniversalPoint lp = new UniversalPoint(goSubLight.transform.position, goSubLight.transform, PrimitiveType.Cube);
            lp.handle = handle;
            SublightPoint.Add(handle, lp);
            Super.Window.CamPlus_SubLightHandleList.Add(handle);
            return goSubLight;
        }

        public GameObject IK_RemoveSubLightPoint(HandleEx hSubLightHandle)
        {
#if DEBUG
            Debuginfo.Log("IK_RemoveSubLightPoint at sSubLight = " + hSubLightHandle.sItemName, 2);
#endif
            //ワイルドハンドル           
            hSubLightHandle.Visible = false;
#if DEBUG
            Debuginfo.Log("handle.sItemFullName = " + hSubLightHandle.sItemFullName, 2);
#endif
            GameObject goParent = hSubLightHandle.GetParentBone().gameObject;           
            Super.Items.Items_ItemHandle.Remove(hSubLightHandle);
            SublightPoint[hSubLightHandle].DestoryTarget();
            SublightPoint.Remove(hSubLightHandle);
            Super.Window.CamPlus_SubLightHandleList.Remove(hSubLightHandle);
            hSubLightHandle.Destroy();
            return goParent;
        }

        #region runtime method
        public void IK_Porc(int MaidStockID)
        {
            if (false == MaidsIK.ContainsKey(MaidStockID))
                return;

            if (true == MaidsIK[MaidStockID].bInvisible)
                return;

            if (0 == MaidsIK[MaidStockID].IKCMO.Count)
                return;

            foreach (EMES_IK.IK ik in MaidsIK[MaidStockID].IKCMO)
            {
                Vector3 pos = ik.handle.GetParentBone().position + ik.handle.DeltaVector();
                ik.GetIKCMO.Porc(ik.hip, ik.knee, ik.ankle, pos, default(Vector3));
            }
        }

        public void IK_CheckInvisible(int MaidStockID)
        {
            if (false == MaidsIK.ContainsKey(MaidStockID))
                return;

            if (true == MaidsIK[MaidStockID].bInvisible)
                return;

            MaidsIK[MaidStockID].bInvisible = true;

            foreach (KeyValuePair<BoneType, KeyValuePair<BoneSetType, GameObject>> bone in MaidsIK[MaidStockID].BoneDict)
            {
                if (true == MaidsIK[MaidStockID].handleEx[bone.Key].Visible)
                {
                    MaidsIK[MaidStockID].bInvisible = false;
                    break;
                }
            }
        }
        

        public bool IK_SyncHandle(Maid maid, HandleEx posHandle)
        {
            Transform parentBone = posHandle.GetParentBone();

            if (null == parentBone)
                return false;

#if DEBUG
            //ボーンを強制的に移動させておく
            if (true == FlexKeycode.GetKey("shift"))
            {
                if (false == posHandle.IK_GetHandleKunPosotionMode() && (parentBone.name.Contains("Bip01") || parentBone.name.Contains("Mune")) && "Bip01" != parentBone.name)
                {
                    posHandle.IK_ChangeHandleKunModePosition(true);
                }
            }
            else
            {
                if (true == posHandle.IK_GetHandleKunPosotionMode() && (parentBone.name.Contains("Bip01") || parentBone.name.Contains("Mune")) && "Bip01" != parentBone.name)
                {
                    posHandle.IK_ChangeHandleKunModePosition(false);
                }
            }
#endif
            if (posHandle.ControllDragged() && true == posHandle.Visible)
            {
                LockIK();
                if (BoneType.Offset != posHandle.boneType)
                {
                    if (false == Super.Window.GetYotogiNonStop())
                    {
                        maid.body0.m_Bones.GetComponent<Animation>().Stop();
                    }
                    maid.body0.boHeadToCam = false;
                    maid.body0.boEyeToCam = false;
                }
                string bone = parentBone.name;

                if (false == posHandle.IK_GetHandleKunPosotionMode())
                {
                    //ボーンを回転させておく
                    if (false == parentBone.name.Contains("Eye"))
                    {
                        posHandle.GetParentBone().rotation *= posHandle.DeltaQuaternion();
                    }
                    else
                    {
                        posHandle.GetParentBone().localRotation *= posHandle.DeltaQuaternion();
                    }
                }
                else
                {
                    if (("Bip01" == bone) || (true == bone.Contains("Maid")) || (true == bone.Contains("BoneTail")))
                    {
                        //ボーンを移動させておく
                        posHandle.GetParentBone().position += posHandle.DeltaVector();
                    }
                    else
                    {
                        if (true == FlexKeycode.GetKey("shift"))
                        {
                            //ボーンを強制的に移動させておく
                            posHandle.GetParentBone().position += posHandle.DeltaVector();
                        }
                        else
                        {
                            if (HandleEx.IKMODE.UniversalIK != posHandle.IKmode)
                                Debuginfo.Log("骨の位置ドラッグ EXCEPTION bone=" + bone, 1);
                        }
                    }
                }
                UnLockIK();
            }

            return true;
        }

        public void IK_SyncFromHandle(int MaidStockID)
        {
            if (false == MaidsIK.ContainsKey(MaidStockID))
                return;

            if (true == MaidsIK[MaidStockID].bInvisible)
                return;

            foreach (KeyValuePair<BoneType, HandleEx> handle in MaidsIK[MaidStockID].handleEx)
            {
                IK_SyncHandle(GameMain.Instance.CharacterMgr.GetStockMaid(MaidStockID), handle.Value);
            }
        }
        #endregion

        #endregion

        #region private method
        #endregion
    }
}

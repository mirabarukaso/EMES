using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    //参照：ハンドル君初期化生成処理
    //原作者：夜勤D @YakinKazuya
    public class HandleEx
    {
        #region Variables
        private readonly int BaseRenderQueue = 3500;
        private readonly Dictionary<string, int[]> sColorUV = new Dictionary<string, int[]>()
        {
            { "red",    new int[] {0,0} },
            { "green",  new int[] {1,0} },
            { "blue",   new int[] {2,0} },
            { "yellow", new int[] {0,1} },
            { "cyan",   new int[] {1,1} },
            { "magenta",new int[] {2,1} },
            { "white",  new int[] {0,2} }
        };

        public enum IKMODE
        {
            None = 99,
            LeftLeg = 0,
            RightLeg = 1,
            LeftArm = 2,
            RightArm = 3,
            UniversalIK
        }
        private IKMODE ikmode = IKMODE.None;

        public IKMODE IKmode
        {
            get { return (bInitCompleted) ? ikmode : IKMODE.None; }
            set
            {
                if (bInitCompleted)
                {
                    //Debuginfo.Log("IKmode:" + (int)value , 2);
                    ikmode = value;
                    if (value == IKMODE.None)
                    {
                        //Rot = Quaternion.identity;
                        controllOnMouseC.ikmode = false;
                    }
                    else
                    {
                        //Rot = Quaternion.Euler(-90, 0, 90);
                        controllOnMouseC.ikmode = true;
                    }
                }
            }
        }

        public Transform Transform
        {
            get
            {
                return (bInitCompleted) ? goHandleMasterObject.transform : null;
            }
        }
        public Vector3 Pos
        {
            get { return (bInitCompleted) ? goHandleMasterObject.transform.position : default(Vector3); }
            set { if (bInitCompleted) goHandleMasterObject.transform.position = value; }
        }
        public Quaternion Rot
        {
            get { return (bInitCompleted) ? goHandleMasterObject.transform.rotation : default(Quaternion); }
            set { if (bInitCompleted) goHandleMasterObject.transform.rotation = value; }
        }
        public Quaternion LocalRot
        {
            get { return (bInitCompleted) ? goHandleMasterObject.transform.localRotation : default(Quaternion); }
            set { if (bInitCompleted) goHandleMasterObject.transform.localRotation = value; }
        }

        public float Scale
        {
            get { return (bInitCompleted) ? handleScale : 0; }
            set
            {
                if (bInitCompleted)
                {
                    if (0f == value)
                    {
                        handleScale = handleScaleBackup;
                    }
                    else if (-1f == value)
                    {
                        //自動
                        if (null != GetParentBone())
                        {
                            //ハンドル君の大きさは子ボーンまでの長さに比例させる
                            int childBoneCount = 0;
                            handleScale = 0.1f;
                            for (int i = 0; i < GetParentBone().childCount; ++i)
                            {
                                Transform childBone = GetParentBone().GetChild(i);
                                if (false == childBone.name.Contains("SCL"))
                                {
                                    ++childBoneCount;
                                    handleScale += childBone.localPosition.magnitude;
                                }
                            }
                            if (childBoneCount != 0)
                            {
                                handleScale /= (float)childBoneCount;
                            }
                            if (handleScale < 0.001) handleScale = 0.001f;
                        }
                        else
                        {
                            handleScale = handleScaleBackup;
                        }
                    }
                    else
                    {
                        handleScale = value;
                    }
                    goHandleMasterObject.transform.localScale = Vector3.one * handleScale;
                }
            }

        }
        public bool Visible
        {
            get
            {
                return (bInitCompleted && goHandleMasterObject != null) ? goHandleMasterObject.activeSelf : default(bool);
            }
            set
            {
                if (bInitCompleted && goHandleMasterObject != null) goHandleMasterObject.SetActive(value);
            }
        }

        public bool CheckParentAlive()
        {
            return (null == goHandleMasterObject) ? false : true;
        }

        private GameObject goHandleMasterObject;
        private GameObject goAngleHandle;
        private GameObject goPositionHandle;
        public GameObject goIKBoneTarget { get; private set; }

        private Material mHandleMaterial;
        private bool bHandlePositionMode = false;
        private bool bHandleIKMode = false;
        private bool bInitCompleted = false;

        private ControllOnMouse controllOnMouseX;
        private ControllOnMouse controllOnMouseY;
        private ControllOnMouse controllOnMouseZ;
        private ControllOnMouse controllOnMousePX;
        private ControllOnMouse controllOnMousePY;
        private ControllOnMouse controllOnMousePZ;
        private ControllOnMouse controllOnMouseC;
        private ControllOnMouse controllOnMouseH;
        private ControllOnMouse controllOnMouseIK;

        private Texture2D m_texture_all = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private GameObject redring;
        private GameObject bluering;
        private GameObject greenring;
        private GameObject redvector;
        private GameObject bluevector;
        private GameObject greenvector;
        private GameObject whitecenter;
        private GameObject cyancube;

        private float handleScale = 0.5f;
        private float handleScaleBackup = 0.5f;
        private bool bIKAttached = false;

        private Transform trParentBone = null;
        public EMES_MaidIK.BoneType boneType
        {
            get;
            private set;
        }
        public EMES_MaidIK.BoneSetType boneSetType
        {
            get;
            private set;
        }

        public string sItemFullName { get; private set; }
        public string sItemName { get; private set; }        
        public string sCategory { get; private set; }
        public string sOptions { get; private set; }
        public GameObject parentBone { get; private set; }
        #endregion

        #region Mouse
        private class ControllOnMouse : MonoBehaviour
        {
            public enum WheelType
            {
                Angle,
                Position,
                PosCenter,
                RotHead
            }

            public enum AxisType
            {
                RX,
                RY,
                RZ,
                NONE
            }

            public bool Clicked = false;
            public bool rightClicked = false;
            public bool mouseOver = false;
            private Vector3 objectPoint = Vector3.zero;
            public WheelType wheelType = WheelType.Angle;
            public AxisType axisType = AxisType.RX;
            public bool ShouldReset = false;
            public bool ikmode = false;
            public bool Dragged = false;
            public bool DragFinished = false;
            public Vector3 clickPointVector = Vector3.zero;
            public float oldValue = 0f;
            Vector3 identitytoScreen = Vector3.zero;
            public Quaternion dragQuaternion = Quaternion.identity;
            public Vector3 dragVector = Vector3.zero;

            public void Destroy()
            {
            }

            public void Awake()
            {
            }

            public void OnMouseDown()
            {
                if (Input.GetMouseButton(0))
                {
                    if (Clicked == false)
                    {
                        Clicked = true;
                    }
                }

                //Debuginfo.Log("ControllOnMouse OnMouseDown", 2);
                if (wheelType == WheelType.Angle)
                {
                    //カメラから見たオブジェクトの現在位置を画面位置座標に変換
                    //screenPoint = Camera.main.WorldToScreenPoint(transform.position);

                    //リングが直線状になるとき
                    if (Math.Abs(Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 10f)
                    {
                        if ((Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) - 90f) < 0f)
                            objectPoint = Camera.main.WorldToScreenPoint(transform.position + 0.2f * transform.up);
                        else
                            objectPoint = Camera.main.WorldToScreenPoint(transform.position - 0.2f * transform.up);
                    }
                    else
                    {
                        objectPoint = Camera.main.WorldToScreenPoint(transform.position);
                    }

                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = 0;
                    clickPointVector -= objectPoint;

                    oldValue = 0.0f;
                }
                else if (wheelType == WheelType.Position)
                {
                    clickPointVector = Input.mousePosition;
                    //clickPointVector.z = 0;//Camera.main.WorldToScreenPoint(transform.position).z;

                    oldValue = 0.0f;

                    identitytoScreen = Camera.main.WorldToScreenPoint(transform.up + transform.position) - Camera.main.WorldToScreenPoint(transform.position);
                    //identitytoScreen.z = 0;

                }
                else if (wheelType == WheelType.PosCenter)
                {
                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    oldValue = Camera.main.WorldToScreenPoint(transform.position).z;
                    clickPointVector = Camera.main.ScreenToWorldPoint(clickPointVector);
                }
                else if (wheelType == WheelType.RotHead)
                {
                    //ここから
                    clickPointVector = Input.mousePosition;
                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    oldValue = Camera.main.WorldToScreenPoint(transform.position).z;
                    clickPointVector = Camera.main.ScreenToWorldPoint(clickPointVector);
                }
            }

            public void OnMouseDrag()
            {
                if (Clicked == true)
                {
                    Clicked = false;
                }

                //Debuginfo.Log("ControllOnMouse OnMouseDrag", 2);
                if (wheelType == WheelType.Angle)
                {
                    Vector3 dragPoint = Input.mousePosition;

                    dragPoint.z = 0;
                    dragPoint -= objectPoint;

                    float dragAngle = Vector3.Angle(clickPointVector, dragPoint);

                    if ((clickPointVector.x * dragPoint.y - clickPointVector.y * dragPoint.x) < 0)
                    {
                        dragAngle = -dragAngle;
                    }
                    if (Vector3.Angle(Vector3.forward, Camera.main.worldToCameraMatrix.MultiplyVector(transform.up)) < 90)
                    {
                        dragAngle = -dragAngle;

                    }
                    if (axisType == AxisType.RY)
                    {
                        dragAngle = -dragAngle;
                    }

                    float offsetAngle = dragAngle - oldValue;

                    switch (axisType)
                    {
                        case AxisType.RY:

                            dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.right);
                            break;

                        case AxisType.RZ:

                            dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.up);
                            break;

                        case AxisType.RX:

                            dragQuaternion = Quaternion.AngleAxis(offsetAngle, Vector3.forward);
                            break;

                        default:
                            break;
                    }
                    oldValue = dragAngle;
                }
                else if (wheelType == WheelType.Position)
                {

                    Vector3 dragPointVector = Input.mousePosition;

                    float dragLength = (dragPointVector - clickPointVector).magnitude;

                    Vector3 yajirushi = Camera.main.WorldToScreenPoint(transform.up + transform.position) - Camera.main.WorldToScreenPoint(transform.position);//Camera.main.worldToCameraMatrix.MultiplyVector(transform.up);

                    dragLength = dragLength != 0 ? (yajirushi.x * (dragPointVector - clickPointVector).x + yajirushi.y * (dragPointVector - clickPointVector).y) / (yajirushi.magnitude * dragLength) : 0;

                    clickPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector.z = Camera.main.WorldToScreenPoint(transform.position).z;
                    Vector3 clickPoint = Camera.main.ScreenToWorldPoint(clickPointVector);
                    Vector3 dragPoint = Camera.main.ScreenToWorldPoint(dragPointVector);
                    dragLength = (dragPoint - clickPoint).magnitude * dragLength;

                    float offsetLength = dragLength - oldValue;

                    switch (axisType)
                    {
                        case AxisType.RY:

                            dragVector = offsetLength * transform.up;//(-Vector3.right);
                            break;

                        case AxisType.RZ:

                            dragVector = offsetLength * transform.up;//Vector3.up;
                            break;

                        case AxisType.RX:

                            dragVector = offsetLength * transform.up;// Vector3.forward;
                            break;

                        default:
                            break;
                    }
                    oldValue = dragLength;
                }
                else if (wheelType == WheelType.PosCenter)
                {
                    Vector3 dragPointVector = Input.mousePosition;
                    dragPointVector.z = oldValue;//Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector = Camera.main.ScreenToWorldPoint(dragPointVector);
                    switch (axisType)
                    {
                        case AxisType.NONE:

                            dragVector = dragPointVector - clickPointVector;

                            break;

                        default:
                            break;

                    }
                    clickPointVector = dragPointVector;
                }
                else if (wheelType == WheelType.RotHead)
                {
                    Vector3 dragPointVector = Input.mousePosition;
                    dragPointVector.z = oldValue;//Camera.main.WorldToScreenPoint(transform.position).z;
                    dragPointVector = Camera.main.ScreenToWorldPoint(dragPointVector);

                    switch (axisType)
                    {
                        case AxisType.NONE:

                            //何かすごく回りくどい計算してるかもしれないので
                            //もっと簡潔に計算できる方法見つけたらここ書き換える
                            dragQuaternion = Quaternion.AngleAxis(
                                Vector3.Angle(clickPointVector - transform.parent.position, dragPointVector - transform.parent.position),
                                new Vector3(0,
                                Vector3.Dot(transform.parent.forward, (dragPointVector - clickPointVector)),
                                -Vector3.Dot(transform.parent.up, (dragPointVector - clickPointVector))));

                            break;

                        default:
                            break;

                    }
                    clickPointVector = dragPointVector;
                }
                Dragged = true;
            }
            public void OnMouseUp()
            {
                //Debuginfo.Log("ControllOnMouse OnMouseUp", 2);
                if (Dragged)
                {
                    Dragged = false;
                    DragFinished = true;
                }
            }

            public void OnMouseEnter()
            {
                mouseOver = true;
            }

            public void OnMouseOver()
            {
            }
            public void OnMouseExit()
            {
                mouseOver = false;
            }

            public void Update()
            {
                if (mouseOver)
                {
                    if (Input.GetMouseButton(2)) ShouldReset = true;
                }
            }

            public void OnGui()
            {
                if (DragFinished)
                {
                    DragFinished = false;
                    Clicked = false;
                }
            }
        }
        #endregion

        #region Public method
        public HandleEx(EMES_MaidIK.BoneType boneT, EMES_MaidIK.BoneSetType boneST, GameObject goParentBone, bool bShowHandle, bool bAllFront)
        {
            boneType = boneT;
            boneSetType = boneST;
            CreateHandleKun(false, bAllFront);
            SetupItem("無", "無", "無");
            SetParentBone(goParentBone.transform);
            parentBone = goParentBone;
            Visible = bShowHandle;
            sOptions = "無";
        }

        public void SetupItem(string fullname, string name, string cat)
        {
            if(null != fullname)
                sItemFullName = fullname;
            if (null != name)
                sItemName = name;
            if (null != cat)
                sCategory = cat;
        }

        public int SetOptions(string option)
        {
            sOptions = option;
            return sOptions.Split('|').Length;
        }
        
        //今動かしてるボーンを取得
        public Transform GetParentBone()
        {
            return (bInitCompleted && goHandleMasterObject != null) ? trParentBone : null;
        }

        //動かすボーン設定
        public bool SetParentBone(Transform _trParentBone)
        {
            if (goHandleMasterObject == null)
            {
                Debuginfo.Error("goHandleMasterObject is null!");
                return false;
            }
            if (_trParentBone != null)
            {
                //現在のボーンが渡されてきたら処理をスキップ
                if (_trParentBone != trParentBone)
                {
                    trParentBone = _trParentBone;
                    goHandleMasterObject.transform.parent = _trParentBone;
                    goHandleMasterObject.transform.localPosition = Vector3.zero;
                    goHandleMasterObject.transform.localRotation = Quaternion.identity;
                    bInitCompleted = true;
                    bIKAttached = true;

                    //ハンドル君の大きさ調整
                    if (IKMODE.None == IKmode)
                    {
                        if ("Bip01" == _trParentBone.name)
                        {
                            handleScale = 0.5f;
                        }
                        else if (true == _trParentBone.name.Contains("Maid"))
                        {
                            handleScale = 0.3f;
                        }
                        else if (_trParentBone.name.Contains("Finger") || _trParentBone.name.Contains("Toe"))
                        {
                            handleScale = 0.02f;
                        }
                        else if (_trParentBone.name.Contains("Bip01"))
                        {   //ハンドル君の大きさは子ボーンまでの長さに比例させる
                            int childBoneCount = 0;
                            handleScale = 0.1f;
                            for (int i = 0; i < trParentBone.childCount; ++i)
                            {
                                Transform childBone = _trParentBone.GetChild(i);
                                if (childBone.name.Contains("Bip") && !childBone.name.Contains("SCL"))
                                {
                                    ++childBoneCount;
                                    handleScale += childBone.localPosition.magnitude;
                                }
                            }
                            if (childBoneCount != 0)
                            {
                                handleScale /= (float)childBoneCount;
                            }
                            if (handleScale < 0.1) handleScale = 0.1f;
                        }
                        else
                        {
                            handleScale = 0.2f;
                        }
                    }
                    else
                    {   //ハンドル君がIKモードのときは大きさ固定
                        handleScale = 0.3f;
                    }
                    
                    goHandleMasterObject.transform.localScale = Vector3.one * handleScale;

                    handleScaleBackup = handleScale;
                }
            }
            else
            {
                //nullが来たら非表示にしとく
                goHandleMasterObject.SetActive(false);
                bInitCompleted = false;
                bIKAttached = false;
            }

            return true;
        }


        public void IK_ChangeHandleKunModePosition(bool isPositionMode)
        {
            bHandlePositionMode = isPositionMode;
            bHandleIKMode = false;
            goAngleHandle.SetActive(!isPositionMode);
            goPositionHandle.SetActive(isPositionMode);
            goIKBoneTarget.SetActive(false);
        }

        public void IK_ChangeHandleKunModeIK(bool isIKMode)
        {
            bHandlePositionMode = false;
            bHandleIKMode = isIKMode;
            goAngleHandle.SetActive(!isIKMode);
            goPositionHandle.SetActive(false);
            goIKBoneTarget.SetActive(isIKMode);
        }

        public bool IK_GetHandleKunPosotionMode()
        {
            return bHandlePositionMode;
        }

        public bool IK_GetHandleKunIKMode()
        {
            return bHandleIKMode;
        }

        //どの軸がドラッグされてるのか判別してドラッグ回転を返す
        public Quaternion DeltaQuaternion()
        {
            if (!bInitCompleted) return Quaternion.identity;
            if (controllOnMouseX.Dragged)
            {
                return controllOnMouseX.dragQuaternion;
            }
            else if (controllOnMouseY.Dragged)
            {
                return controllOnMouseY.dragQuaternion;
            }
            else if (controllOnMouseZ.Dragged)
            {
                return controllOnMouseZ.dragQuaternion;
            }

            else if (controllOnMouseH.Dragged)
            {
                return controllOnMouseH.dragQuaternion;
            }
            else
            {
                return Quaternion.identity;
            }

        }

        //どの軸がドラッグされてるのか判別してドラッグ移動量を返す
        public Vector3 DeltaVector()
        {
            if (!bInitCompleted) return Vector3.zero;
            if (controllOnMousePX.Dragged)
            {
                return controllOnMousePX.dragVector;
            }
            else if (controllOnMousePY.Dragged)
            {
                return controllOnMousePY.dragVector;
            }
            else if (controllOnMousePZ.Dragged)
            {
                return controllOnMousePZ.dragVector;
            }
            else if (controllOnMouseC.Dragged)
            {
                return controllOnMouseC.dragVector;
            }
            else if(controllOnMouseIK.Dragged)
            {
                return controllOnMouseIK.dragVector;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public void Destroy()
        {
            if (goHandleMasterObject)
            {
                GameObject.Destroy(goHandleMasterObject);
            }
            bInitCompleted = false;
        }

        public bool ControllClicked()
        {
            if (!bInitCompleted)
            {
                return false;
            }

            return controllOnMouseIK.Clicked;
        }

        public bool ControllDragged()
        {
            if (!bInitCompleted)
            {
                return false;
            }

            if (true == bHandleIKMode)
            {
                if (true == controllOnMouseIK.DragFinished)
                {
                    Visible = false;
                    controllOnMouseIK.DragFinished = false;
                    SetUVColor(bIKAttached ? "white" : "magenta", goIKBoneTarget);
                    Visible = true;
                }

                if (false == controllOnMouseIK.Dragged)
                {
                    if (controllOnMouseIK.mouseOver)
                    {
                        SetUVColor("yellow", goIKBoneTarget);
                    }
                    else
                    {
                        SetUVColor(bIKAttached ? "white" : "magenta", goIKBoneTarget);
                    }
                }
                return (controllOnMouseIK.Dragged);
            }

            if (false == bHandlePositionMode && false == bHandleIKMode)
            {
                if (controllOnMouseX.DragFinished || controllOnMouseY.DragFinished || controllOnMouseZ.DragFinished || controllOnMouseH.DragFinished)
                {
                    Visible = false;
                    controllOnMouseX.DragFinished = false;
                    controllOnMouseY.DragFinished = false;
                    controllOnMouseZ.DragFinished = false;
                    controllOnMouseH.DragFinished = false;

                    SetUVColor("red", redring);
                    SetUVColor("green", greenring);
                    SetUVColor("blue", bluering);
                    SetUVColor("cyan", cyancube);
                    Visible = true;
                }

                if (!controllOnMouseX.Dragged && !controllOnMouseY.Dragged && !controllOnMouseZ.Dragged && !controllOnMouseH.Dragged)
                {
                    if (controllOnMouseX.mouseOver)
                    {
                        SetUVColor("yellow", redring);
                    }
                    else
                    {
                        SetUVColor("red", redring);
                    }

                    if (controllOnMouseY.mouseOver)
                    {
                        SetUVColor("yellow", greenring);
                    }
                    else
                    {
                        SetUVColor("green", greenring);
                    }

                    if (controllOnMouseZ.mouseOver)
                    {
                        SetUVColor("yellow", bluering);
                    }
                    else
                    {
                        SetUVColor("blue", bluering);
                    }

                    if (controllOnMouseH.mouseOver)
                    {
                        SetUVColor("yellow", cyancube);
                    }
                    else
                    {
                        SetUVColor("cyan", cyancube);
                    }
                }
                return (controllOnMouseX.Dragged || controllOnMouseY.Dragged || controllOnMouseZ.Dragged || controllOnMouseH.Dragged);
            }
            else 
            {
                if (controllOnMousePX.DragFinished || controllOnMousePY.DragFinished || controllOnMousePZ.DragFinished || controllOnMouseC.DragFinished)
                {
                    Visible = false;
                    controllOnMousePX.DragFinished = false;
                    controllOnMousePY.DragFinished = false;
                    controllOnMousePZ.DragFinished = false;
                    controllOnMouseC.DragFinished = false;
                    SetUVColor("red", redvector);
                    SetUVColor("green", greenvector);
                    SetUVColor("blue", bluevector);
                    SetUVColor(bIKAttached ? "cyan" : "white", whitecenter);
                    Visible = true;
                }

                if (!controllOnMousePX.Dragged && !controllOnMousePY.Dragged && !controllOnMousePZ.Dragged && !controllOnMouseC.Dragged)
                {
                    if (controllOnMousePX.mouseOver)
                    {
                        SetUVColor("yellow", redvector);
                    }
                    else
                    {
                        SetUVColor("red", redvector);
                    }

                    if (controllOnMousePY.mouseOver)
                    {
                        SetUVColor("yellow", greenvector);
                    }
                    else
                    {
                        SetUVColor("green", greenvector);
                    }

                    if (controllOnMousePZ.mouseOver)
                    {
                        SetUVColor("yellow", bluevector);
                    }
                    else
                    {
                        SetUVColor("blue", bluevector);
                    }

                    if (controllOnMouseC.mouseOver)
                    {
                        SetUVColor("yellow", whitecenter);
                    }
                    else
                    {
                        SetUVColor(bIKAttached ? "cyan" : "white", whitecenter);
                    }
                }
                return (controllOnMousePX.Dragged || controllOnMousePY.Dragged || controllOnMousePZ.Dragged || controllOnMouseC.Dragged);
            }

        }
        #endregion

        #region Private method
        private void SetMaterialAllPixel(string _name, Color _color)
        {
            for (int y = 4 * sColorUV[_name][1]; y < 4 * (sColorUV[_name][1] + 1); y++)
            {
                for (int x = 4 * sColorUV[_name][0]; x < 4 * (sColorUV[_name][0] + 1); x++)
                {
                    m_texture_all.SetPixel(x, y, _color);
                }
            }
        }

        private void SetUVColor(string _name, GameObject _obj)
        {
            try
            {
                Mesh mesh = _obj.GetComponent<MeshFilter>().mesh;
                Vector2[] uv = mesh.uv;
                for (int i = 0; i < uv.Count(); ++i)
                {
                    uv[i].x = 0.25f * uv[i].x + sColorUV[_name][0] * 0.25f;
                    uv[i].y = 0.25f * uv[i].y + sColorUV[_name][1] * 0.25f;
                }
                mesh.uv = uv;
            }
            catch (Exception e)
            {
                Debuginfo.Log("Exception SetUVColor => boneType = " + boneType, 2);
                Debuginfo.Log("Exception SetUVColor => _name = " + _name, 2);
                Debuginfo.Log("Exception SetUVColor => _obj = " + _obj, 2);                
                Debuginfo.Log("Exception SetUVColor =>" + e, 2);
            }
        }

        private GameObject SetHandleObject2(PrimitiveType _type, Vector3 _scale, Vector3 _angle, string _name, GameObject _parent)
        {
            GameObject PartsObject = GameObject.CreatePrimitive(_type);
            Mesh mesh = PartsObject.GetComponent<MeshFilter>().mesh;

            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;

            for (int i = 0; i < vertices.Count(); ++i)
            {
                vertices[i].x *= _scale.x;
                vertices[i].y *= _scale.y;
                vertices[i].z *= _scale.z;
            }

            for (int i = 0; i < uv.Count(); ++i)
            {
                uv[i].x = 0.25f * uv[i].x + sColorUV[_name][0] * 0.25f;
                uv[i].y = 0.25f * uv[i].y + sColorUV[_name][1] * 0.25f;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            switch (_type)
            {
                case PrimitiveType.Sphere:
                    SphereCollider colliderS = PartsObject.GetComponent<Collider>() as SphereCollider;
                    colliderS.radius *= (_scale.x + _scale.y + _scale.z) / 3f;
                    break;
                case PrimitiveType.Cube:
                    BoxCollider colliderR = PartsObject.GetComponent<Collider>() as BoxCollider;
                    Vector3 boxScale = colliderR.size;
                    boxScale.x *= 2 * _scale.x;
                    boxScale.y *= 2 * _scale.y;
                    boxScale.z *= 2 * _scale.z;
                    colliderR.size = boxScale;
                    break;
                default:
                    break;

            }
            PartsObject.GetComponent<Renderer>().receiveShadows = false;
            PartsObject.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            PartsObject.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            PartsObject.GetComponent<Renderer>().material = mHandleMaterial;
            PartsObject.transform.localEulerAngles = _angle;
            PartsObject.transform.parent = _parent.transform;
            return PartsObject;
        }

        private GameObject SetHandleRingObject2(Vector3 handleAngle, string _name)
        {
            #region createPrimitive
            GameObject Ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh mesh = Ring.GetComponent<MeshFilter>().mesh;

            //円筒プリミティブを縁を少し残して円柱の底面を抜いたメッシュに修正
            Vector3[] newMesh = new Vector3[92];
            Vector2[] newUV = new Vector2[92];

            for (int i = 0; i < 92; ++i)
            {
                if (i >= 46)
                {
                    newMesh[i] = newMesh[i - 46];
                    newMesh[i].x *= 0.95f;
                    newMesh[i].z *= 0.95f;

                    newUV[i] = newUV[i - 46];
                    //newUV[i].y = 0.5f;
                    newUV[i].y = 0.125f + sColorUV[_name][1] * 0.25f;

                }
                else if (i >= 40)
                {
                    newMesh[i] = mesh.vertices[i + 2];
                    newMesh[i].x *= 2.0f;
                    newMesh[i].y *= 0.005f;
                    newMesh[i].z *= 2.0f;

                    newUV[i] = mesh.uv[i + 2];
                    newUV[i].x = 0.25f * newUV[i].x + sColorUV[_name][0] * 0.25f;
                    newUV[i].y = 0.25f * newUV[i].y + sColorUV[_name][1] * 0.25f;

                }
                else
                {
                    newMesh[i] = mesh.vertices[i];
                    newMesh[i].x *= 2.0f;
                    newMesh[i].y *= 0.005f;
                    newMesh[i].z *= 2.0f;

                    newUV[i] = mesh.uv[i];
                    newUV[i].x = 0.25f * newUV[i].x + sColorUV[_name][0] * 0.25f;
                    newUV[i].y = 0.25f * newUV[i].y + sColorUV[_name][1] * 0.25f;
                }
            }

            int[] newTri = new int[360];

            for (int i = 0; i < 120; ++i)
            {
                if (mesh.triangles[i] > 40)
                    newTri[i] = mesh.triangles[i] - 2;
                else
                    newTri[i] = mesh.triangles[i];
            }

            for (int i = 0; i < 20; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    if (newTri[6 * i + j] == 41)
                    {
                        newTri[6 * i + 122 - j] = 86;
                    }
                    else if (newTri[6 * i + j] == 43)
                    {
                        newTri[6 * i + 122 - j] = 90;
                    }
                    else if (newTri[6 * i + j] == 45)
                    {
                        newTri[6 * i + 122 - j] = 88;
                    }
                    else if (newTri[6 * i + j] >= 20 && newTri[6 * i + j] < 40)
                    {
                        newTri[6 * i + 122 - j] = newTri[6 * i + j] + 26;
                    }
                    else
                    {
                        newTri[6 * i + 122 - j] = newTri[6 * i + j];

                    }

                    if (newTri[6 * i + j + 3] == 41)
                    {
                        newTri[6 * i + 125 - j] = 86;
                    }
                    else if (newTri[6 * i + j + 3] == 43)
                    {
                        newTri[6 * i + 125 - j] = 90;
                    }
                    else if (newTri[6 * i + j + 3] == 45)
                    {
                        newTri[6 * i + 125 - j] = 88;
                    }
                    else if (newTri[6 * i + j + 3] >= 20 && newTri[6 * i + j + 3] < 40)
                    {
                        newTri[6 * i + 125 - j] = newTri[6 * i + j + 3] + 26;
                    }
                    else
                    {
                        newTri[6 * i + 125 - j] = newTri[6 * i + j + 3];
                    }

                    if (newTri[6 * i + j] == 40)
                    {
                        newTri[6 * i + 242 - j] = 87;
                    }
                    else if (newTri[6 * i + j] == 42)
                    {
                        newTri[6 * i + 242 - j] = 91;
                    }
                    else if (newTri[6 * i + j] == 44)
                    {
                        newTri[6 * i + 242 - j] = 89;
                    }
                    else if (newTri[6 * i + j] < 20)
                    {
                        newTri[6 * i + 242 - j] = newTri[6 * i + j] + 66;
                    }
                    else
                    {
                        newTri[6 * i + 242 - j] = newTri[6 * i + j];
                    }

                    if (newTri[6 * i + j + 3] == 40)
                    {
                        newTri[6 * i + 245 - j] = 87;
                    }
                    else if (newTri[6 * i + j + 3] == 42)
                    {
                        newTri[6 * i + 245 - j] = 91;
                    }
                    else if (newTri[6 * i + j + 3] == 44)
                    {
                        newTri[6 * i + 245 - j] = 89;
                    }
                    else if (newTri[6 * i + j + 3] < 20)
                    {
                        newTri[6 * i + 245 - j] = newTri[6 * i + j + 3] + 66;
                    }
                    else
                    {
                        newTri[6 * i + 245 - j] = newTri[6 * i + j + 3];
                    }
                }
            }

            mesh.Clear();
            mesh.vertices = newMesh;
            mesh.uv = newUV;
            mesh.triangles = newTri;

            Ring.GetComponent<Renderer>().receiveShadows = false;
            Ring.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Ring.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            Ring.GetComponent<Renderer>().material = mHandleMaterial;
            Ring.transform.localEulerAngles = handleAngle;
            UnityEngine.Object.Destroy(Ring.GetComponent<Collider>());
            BoxCollider collider = Ring.AddComponent<BoxCollider>();

            Vector3 box = collider.size;
            box.x = 2.5f;
            box.y = 0.02f;
            box.z = 2.5f;
            collider.size = box;

            Vector3 pos = collider.center;
            pos.x = 0;
            pos.y = 0;
            pos.z = 0;
            collider.center = pos;

            Ring.transform.parent = this.goAngleHandle.transform;
            #endregion
            return Ring;
        }

        private GameObject SetHandleVectorObject2(Vector3 handleAngle, string _name)
        {
            #region createPrimitive
            GameObject Segare = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh mesh = Segare.GetComponent<MeshFilter>().mesh;

            //円筒プリミティブを矢印形に修正
            Vector3[] newMesh = new Vector3[88];
            Vector2[] newUV = new Vector2[88];

            for (int i = 0; i < 88; ++i)
            {
                if (i == 41)
                {
                    newMesh[i] = new Vector3(0f, 1.25f, 0f);
                }
                else if (i >= 68)
                {
                    newMesh[i] = mesh.vertices[i];
                    newMesh[i].x *= 2.0f;
                    newMesh[i].z *= 2.0f;
                    //newMesh[i].y += 1.0f;
                }
                else
                {
                    newMesh[i] = mesh.vertices[i];
                    ///newMesh[i].y += 1.0f;
                }
                newUV[i] = mesh.uv[i];

                newMesh[i].x *= 0.05f;
                newMesh[i].y *= 0.4f;
                newMesh[i].z *= 0.05f;
                newUV[i].x = 0.25f * newUV[i].x + sColorUV[_name][0] * 0.25f;
                newUV[i].y = 0.25f * newUV[i].y + sColorUV[_name][1] * 0.25f;

            }

            int[] newTri = new int[360];
            for (int i = 0; i < 240; ++i)
            {
                newTri[i] = mesh.triangles[i];
            }
            for (int i = 0; i < 19; ++i)
            {
                newTri[6 * i + 240] = 20 + i;
                newTri[6 * i + 241] = 68 + i;
                newTri[6 * i + 242] = 21 + i;
                newTri[6 * i + 243] = 69 + i;
                newTri[6 * i + 244] = 21 + i;
                newTri[6 * i + 245] = 68 + i;
            }
            {
                newTri[354] = 39;
                newTri[355] = 87;
                newTri[356] = 20;
                newTri[357] = 68;
                newTri[358] = 20;
                newTri[359] = 87;
            }
            mesh.Clear();
            mesh.vertices = newMesh;
            mesh.uv = newUV;
            mesh.triangles = newTri;

            Segare.GetComponent<Renderer>().receiveShadows = false;
            Segare.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Segare.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            Segare.GetComponent<Renderer>().material = mHandleMaterial;
            Segare.transform.localEulerAngles = handleAngle;

            CapsuleCollider collider = Segare.GetComponent<Collider>() as CapsuleCollider;
            collider.radius *= 0.05f;
            collider.height *= 0.4f;
            Segare.transform.parent = this.goPositionHandle.transform;
            #endregion
            return Segare;
        }

        private void CreateHandleKun(bool isPositionMode, bool bAllFront)
        {
            SetMaterialAllPixel("red", new Color(1f, 0f, 0f, 0.75f)); //red
            SetMaterialAllPixel("green", new Color(0f, 1f, 0f, 0.75f)); //green
            SetMaterialAllPixel("blue", new Color(0f, 0f, 1f, 0.75f)); //blue
            SetMaterialAllPixel("yellow", new Color(1f, 0.92f, 0.04f, 0.75f));  //yellow
            SetMaterialAllPixel("cyan", new Color(0f, 1f, 1f, 0.75f));  //cyan
            SetMaterialAllPixel("magenta", new Color(1f, 0f, 1f, 0.75f));  //magenta
            SetMaterialAllPixel("white", new Color(1f, 1f, 1f, 0.5f));  //white
            m_texture_all.Apply();
            m_texture_all.name = "all";

            mHandleMaterial = new Material(Shader.Find("Unlit/Transparent"));
            //mHandleMaterial = new Material(Shader.Find("Unlit/Transparent Colored RenderTexture"));
            //mHandleMaterial = new Material(Shader.Find("Unlit/Transparent Colored"));
            //Debuginfo.Log("HandleKun: " + mHandleMaterial, 2);
            mHandleMaterial.mainTexture = m_texture_all;
            mHandleMaterial.renderQueue = BaseRenderQueue;

            goHandleMasterObject = new GameObject();
            goAngleHandle = new GameObject();
            goPositionHandle = new GameObject();

            goAngleHandle.transform.parent = goHandleMasterObject.transform;
            goPositionHandle.transform.parent = goHandleMasterObject.transform;
            goAngleHandle.name = "AngleHandle";
            goPositionHandle.name = "PositionHandle";

            SetHandleObject2(PrimitiveType.Sphere, new Vector3(0.125f, 0.125f, 0.125f), new Vector3(0f, 0f, 0f), "white", goAngleHandle);
            SetHandleObject2(PrimitiveType.Cylinder, new Vector3(0.025f, 1f, 0.025f), new Vector3(0f, 0f, 0f), "blue", goAngleHandle);
            SetHandleObject2(PrimitiveType.Cylinder, new Vector3(0.025f, 1f, 0.025f), new Vector3(90f, 0f, 0f), "red", goAngleHandle);
            SetHandleObject2(PrimitiveType.Cylinder, new Vector3(0.025f, 1f, 0.025f), new Vector3(0f, 0f, 90f), "green", goAngleHandle);

            bluering = SetHandleRingObject2(new Vector3(0f, 0f, 0f), "blue");//Z
            if (true == bAllFront)
                bluering.layer = 23;     //AbsolutFront
            controllOnMouseZ = bluering.AddComponent<ControllOnMouse>();
            controllOnMouseZ.wheelType = ControllOnMouse.WheelType.Angle;
            controllOnMouseZ.axisType = ControllOnMouse.AxisType.RZ;

            redring = SetHandleRingObject2(new Vector3(90f, 0f, 0f), "red");//X
            if (true == bAllFront)
                redring.layer = 23;     //AbsolutFront
            controllOnMouseX = redring.AddComponent<ControllOnMouse>();
            controllOnMouseX.wheelType = ControllOnMouse.WheelType.Angle;
            controllOnMouseX.axisType = ControllOnMouse.AxisType.RX;

            greenring = SetHandleRingObject2(new Vector3(0f, 0f, 90f), "green");//Y
            if (true == bAllFront)
                greenring.layer = 23;     //AbsolutFront
            controllOnMouseY = greenring.AddComponent<ControllOnMouse>();
            controllOnMouseY.wheelType = ControllOnMouse.WheelType.Angle;
            controllOnMouseY.axisType = ControllOnMouse.AxisType.RY;

            bluevector = SetHandleVectorObject2(new Vector3(0f, 0f, 0f), "blue");//Z
            if (true == bAllFront)
                bluevector.layer = 23;     //AbsolutFront
            controllOnMousePZ = bluevector.AddComponent<ControllOnMouse>();
            controllOnMousePZ.wheelType = ControllOnMouse.WheelType.Position;
            controllOnMousePZ.axisType = ControllOnMouse.AxisType.RZ;

            redvector = SetHandleVectorObject2(new Vector3(90f, 0f, 0f), "red");//X
            if (true == bAllFront)
                redvector.layer = 23;     //AbsolutFront
            controllOnMousePX = redvector.AddComponent<ControllOnMouse>();
            controllOnMousePX.wheelType = ControllOnMouse.WheelType.Position;
            controllOnMousePX.axisType = ControllOnMouse.AxisType.RX;

            greenvector = SetHandleVectorObject2(new Vector3(0f, 0f, 90f), "green");//Y
            if (true == bAllFront)
                greenvector.layer = 23;     //AbsolutFront
            controllOnMousePY = greenvector.AddComponent<ControllOnMouse>();
            controllOnMousePY.wheelType = ControllOnMouse.WheelType.Position;
            controllOnMousePY.axisType = ControllOnMouse.AxisType.RY;

            cyancube = SetHandleObject2(PrimitiveType.Cube, new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0f, 0f, 0f), "cyan", goAngleHandle);
            cyancube.layer = 23;    //AbsolutFront
            cyancube.transform.localPosition = new Vector3(-1, 0, 0);
            controllOnMouseH = cyancube.AddComponent<ControllOnMouse>();
            controllOnMouseH.wheelType = ControllOnMouse.WheelType.RotHead;
            controllOnMouseH.axisType = ControllOnMouse.AxisType.NONE;

            whitecenter = SetHandleObject2(PrimitiveType.Sphere, new Vector3(0.3f, 0.3f, 0.3f), new Vector3(0f, 0f, 0f), "white", goPositionHandle);
            whitecenter.layer = 23; //AbsolutFront
            controllOnMouseC = whitecenter.AddComponent<ControllOnMouse>();
            controllOnMouseC.wheelType = ControllOnMouse.WheelType.PosCenter;
            controllOnMouseC.axisType = ControllOnMouse.AxisType.NONE;

            goIKBoneTarget = SetHandleObject2(PrimitiveType.Sphere, new Vector3(0.3f, 0.3f, 0.3f), new Vector3(0f, 0f, 0f), "magenta", goHandleMasterObject);
            goIKBoneTarget.layer = 23;  //AbsolutFront
            controllOnMouseIK = goIKBoneTarget.AddComponent<ControllOnMouse>();
            controllOnMouseIK.wheelType = ControllOnMouse.WheelType.PosCenter;
            controllOnMouseIK.axisType = ControllOnMouse.AxisType.NONE;

            IK_ChangeHandleKunModePosition(isPositionMode);
        }
        #endregion
    }
}



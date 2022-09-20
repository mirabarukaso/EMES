using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    //Extra Shader Lite
    public class ExtraShader : CameraPlus
    {
        //SHADER
        private Bloom sBloom;
        private readonly Bloom sBloom_Backup;
        private Blur sBlur;
        private DepthOfFieldScatter sDepthOfField;
        private GlobalFog sGlobalFog;
        private GlobalFog sGlobalFog_Backup;
        private SepiaToneEffect sSepia;
        private Vignetting sVignetting;
        private Vignetting sVignetting_Backup;

        public ExtraShader()
        {
#if DEBUG
            Debuginfo.Log("Now Loading ExtraShader", 2);
#endif

#if DEBUG
            Debuginfo.Log("sBloom", 2);
#endif
            //Image Effects/Bloom and Glow/Bloom
            sBloom = MainCameraObject.GetComponent<Bloom>();
            sBloom_Backup = new Bloom()
            {
                screenBlendMode = sBloom.screenBlendMode,
                quality = sBloom.quality,
                sepBlurSpread = sBloom.sepBlurSpread,
                bloomBlurIterations = sBloom.bloomBlurIterations,
            };

#if DEBUG
            Debuginfo.Log("sBlur", 2);
#endif
            //Image Effects/Blur/Blur (Optimized)
            sBlur = MainCamera.gameObject.GetComponent<Blur>();
            if (null == sBlur)
            {
#if DEBUG
                Debuginfo.Log("Create new Blur", 2);
#endif
                sBlur = MainCameraObject.GetOrAddComponent<Blur>();
                sBlur.blurShader = Shader.Find("Hidden/FastBlur");
                sBlur.enabled = false;
            }

#if DEBUG
            Debuginfo.Log("sDepthOfField", 2);
#endif
            //Image Effects/Camera/Depth of Field (Lens Blur, Scatter, DX11)
            sDepthOfField = MainCameraObject.GetComponent<DepthOfFieldScatter>();

#if DEBUG
            Debuginfo.Log("sGlobalFog", 2);
#endif
            //Image Effects/Rendering/Global Fog
            sGlobalFog = MainCameraObject.GetComponent<GlobalFog>();
            if (null == sGlobalFog)
            {
                //3.8.0 SSモード後にシェーダーが欠落している
#if DEBUG
                Debuginfo.Log("Create new sGlobalFog", 2);
#endif
                sGlobalFog = MainCameraObject.GetOrAddComponent<GlobalFog>();
                sGlobalFog.fogShader = Shader.Find("Hidden/GlobalFog");
                sGlobalFog.enabled = false;
            }
            sGlobalFog.startDistance = 5;
            sGlobalFog.height = 1;
            sGlobalFog.globalFogColor = new Color(0.95f, 0.95f, 0.95f, 1f);
            sGlobalFog_Backup = new GlobalFog()
            {
                startDistance = sGlobalFog.startDistance,
                globalDensity = sGlobalFog.globalDensity,
                heightScale = sGlobalFog.heightScale,
                height = sGlobalFog.height,
                fogMode = sGlobalFog.fogMode,
                globalFogColor = sGlobalFog.globalFogColor
            };

#if DEBUG
            Debuginfo.Log("sSepia", 2);
#endif
            //Image Effects/Color Adjustments/Sepia Tone
            sSepia = MainCameraObject.GetComponent<SepiaToneEffect>();

#if DEBUG
            Debuginfo.Log("Vignetting", 2);
#endif
            //Image Effects/Camera/Vignette and Chromatic Aberration
            sVignetting = MainCameraObject.GetComponent<Vignetting>();
            sVignetting_Backup = new Vignetting()
            {
                mode = sVignetting.mode,
                intensity = sVignetting.intensity,
                chromaticAberration = sVignetting.chromaticAberration,
                axialAberration = sVignetting.axialAberration,
                blur = sVignetting.blur,
                blurSpread = sVignetting.blurSpread,
                luminanceDependency = sVignetting.luminanceDependency,
                blurDistance = sVignetting.blurDistance
            };

            CameraQuickSave();
#if DEBUG
            Debuginfo.Log("Loading ExtraShader Done", 2);
#endif
        }

        public CameraPlus GetParent()
        {
            return Instance;
        }

        #region public Bloom
        public void Get_BloomData(out int BloomScreenBlendMode, out int quality, out int sepBlurSpread, out int bloomBlurIterations)
        {
            BloomScreenBlendMode = (int)sBloom.screenBlendMode;
            quality = (int)sBloom.quality;
            sepBlurSpread = (int)(sBloom.sepBlurSpread * 100);
            bloomBlurIterations = sBloom.bloomBlurIterations;
        }

        public void Set_BloomData(int BloomScreenBlendMode, int quality, int sepBlurSpread, int bloomBlurIterations)
        {
            sBloom.screenBlendMode = (Bloom.BloomScreenBlendMode)BloomScreenBlendMode;
            sBloom.quality = (Bloom.BloomQuality)quality;
            sBloom.sepBlurSpread = (float)(sepBlurSpread / 100);
            sBloom.bloomBlurIterations = bloomBlurIterations;
        }

        public void Reset_BloomData()
        {
            sBloom.screenBlendMode = sBloom_Backup.screenBlendMode;
            sBloom.quality = sBloom_Backup.quality;
            sBloom.sepBlurSpread = sBloom_Backup.sepBlurSpread;
            sBloom.bloomBlurIterations = sBloom_Backup.bloomBlurIterations;
        }
        #endregion

        #region public Blur
        public void Get_BlurData(out bool bEnable, out int blurSize, out int blurIterations)
        {
            bEnable = sBlur.enabled;
            blurSize = (int)(sBlur.blurSize * 100);
            blurIterations = sBlur.blurIterations;
        }

        public void Set_BlurData(bool bEnable, int blurSize, int blurIterations)
        {
            sBlur.enabled = bEnable;
            //[Range(0f, 10f)]
            sBlur.blurSize = ((float)blurSize) / 100;
            //[Range(1f, 4f)]
            sBlur.blurIterations = blurIterations;
        }
        #endregion

        #region public DepthOfFieldScatter
        public void Get_DOFData(out bool bEnable, out bool visualizeFocus, out int focalLength, out int focalSize, out int aperture, out int maxBlurSize, out int blurType)
        {
            bEnable = sDepthOfField.enabled;
            visualizeFocus = sDepthOfField.visualizeFocus;
            focalLength = (int)(sDepthOfField.focalLength * 1000);
            focalSize = (int)(sDepthOfField.focalSize * 1000);
            aperture = (int)(sDepthOfField.aperture * 1000);
            maxBlurSize = (int)(sDepthOfField.maxBlurSize * 100);
            blurType = (int)sDepthOfField.blurType;
        }

        public void Set_DOFData(bool bEnable, bool visualizeFocus, int focalLength, int focalSize, int aperture, int maxBlurSize, int blurType)
        {
            sDepthOfField.enabled = bEnable;
            sDepthOfField.visualizeFocus = visualizeFocus;
            sDepthOfField.focalLength = ((float)focalLength) / 1000;
            sDepthOfField.focalSize = ((float)focalSize) / 1000;
            sDepthOfField.aperture = ((float)aperture) / 1000;
            sDepthOfField.maxBlurSize = ((float)maxBlurSize) / 100;
            sDepthOfField.blurType = (DepthOfFieldScatter.BlurType)blurType;
        }
        #endregion

        #region public GlobalFog
        public void Get_GlobalFogData(out bool bEnable, out int startDistance, out int globalDensity, out int heightScale, out int height, out System.Drawing.Color globalFogColor)
        {
            bEnable = sGlobalFog.enabled;
            startDistance = (int)(sGlobalFog.startDistance * 1000);
            globalDensity = (int)(sGlobalFog.globalDensity * 1000);
            heightScale = (int)(sGlobalFog.heightScale * 1000);
            height = (int)(sGlobalFog.height * 1000);
            globalFogColor = ConvertSystemARGB(sGlobalFog.globalFogColor);
        }

        public void Set_GlobalFogData(bool bEnable, int startDistance, int globalDensity, int heightScale, int height, System.Drawing.Color globalFogColor)
        {
            sGlobalFog.enabled = bEnable;
            sGlobalFog.startDistance = ((float)startDistance) / 1000;
            sGlobalFog.globalDensity = ((float)globalDensity) / 1000;
            sGlobalFog.heightScale = ((float)heightScale) / 1000;
            sGlobalFog.height = ((float)height) / 1000;
            sGlobalFog.globalFogColor = ConvertUnityRGBA(globalFogColor);
        }

        public void Reset_GlobalFogData()
        {
            sGlobalFog.enabled = false;
            sGlobalFog.startDistance = sGlobalFog_Backup.startDistance;
            sGlobalFog.globalDensity = sGlobalFog_Backup.globalDensity;
            sGlobalFog.heightScale = sGlobalFog_Backup.heightScale;
            sGlobalFog.height = sGlobalFog_Backup.height;
            sGlobalFog.fogMode = sGlobalFog_Backup.fogMode;
            sGlobalFog.globalFogColor = sGlobalFog_Backup.globalFogColor;
        }
        #endregion


        #region public SepiaToneEffect
        public void Set_SepiaData(bool bEnable)
        {
            sSepia.enabled = bEnable;
        }

        public void Get_SepiaData(out bool bEnable)
        {
            bEnable = sSepia.enabled;
        }
        #endregion

        #region public Vignetting
        public void Get_VignettingData(out bool bEnable, out int intensity, out int chromaticAberration, out int blurSpread, out int blur)
        {
            bEnable = sVignetting.enabled;
            intensity = (int)(sVignetting.intensity * 100);
            chromaticAberration = (int)(sVignetting.chromaticAberration * 100);
            blurSpread = (int)(sVignetting.blurSpread * 100);
            blur = (int)(sVignetting.blur * 100);
        }

        public void Set_VignettingData(bool bEnable, int intensity, int chromaticAberration, int blurSpread, int blur)
        {
            sVignetting.enabled = bEnable;
            sVignetting.intensity = ((float)intensity) / 100;
            sVignetting.chromaticAberration = ((float)chromaticAberration) / 100;
            sVignetting.blurSpread = ((float)blurSpread) / 100;
            sVignetting.blur = ((float)blur) / 100;
        }

        public void Reset_VignettingData()
        {
            sVignetting.enabled = false;
            sVignetting.mode = sVignetting_Backup.mode;
            sVignetting.intensity = sVignetting_Backup.intensity;
            sVignetting.chromaticAberration = sVignetting_Backup.chromaticAberration;
            sVignetting.axialAberration = sVignetting_Backup.axialAberration;
            sVignetting.blur = sVignetting_Backup.blur;
            sVignetting.blurSpread = sVignetting_Backup.blurSpread;
            sVignetting.luminanceDependency = sVignetting_Backup.luminanceDependency;
            sVignetting.blurDistance = sVignetting_Backup.blurDistance;
        }
        #endregion

        #region Scene Data
        public void GetAllData(out EMES_SceneManagement.ShaderDataNew shaderData)
        {
            shaderData = new EMES_SceneManagement.ShaderDataNew();

            shaderData.sBloom.enabled = sBloom.enabled;
            shaderData.sBloom.screenBlendMode = (int)sBloom.screenBlendMode;
            shaderData.sBloom.quality = (int)sBloom.quality;
            shaderData.sBloom.sepBlurSpread = sBloom.sepBlurSpread;
            shaderData.sBloom.bloomBlurIterations = sBloom.bloomBlurIterations;

            shaderData.sBlur.enabled = sBlur.enabled;
            shaderData.sBlur.blurSize = sBlur.blurSize;
            shaderData.sBlur.blurIterations = sBlur.blurIterations;

            shaderData.sDepthOfField.enabled = sDepthOfField.enabled;
            shaderData.sDepthOfField.visualizeFocus = sDepthOfField.visualizeFocus;
            shaderData.sDepthOfField.focalLength = sDepthOfField.focalLength;
            shaderData.sDepthOfField.focalSize = sDepthOfField.focalSize;
            shaderData.sDepthOfField.aperture = sDepthOfField.aperture;
            shaderData.sDepthOfField.maxBlurSize = sDepthOfField.maxBlurSize;
            shaderData.sDepthOfField.blurType = (int)sDepthOfField.blurType;

            shaderData.sGlobalFog.enabled = sGlobalFog.enabled;
            shaderData.sGlobalFog.startDistance = sGlobalFog.startDistance;
            shaderData.sGlobalFog.globalDensity = sGlobalFog.globalDensity;
            shaderData.sGlobalFog.heightScale = sGlobalFog.heightScale;
            shaderData.sGlobalFog.height = sGlobalFog.height;
            shaderData.sGlobalFog.r = sGlobalFog.globalFogColor.r;
            shaderData.sGlobalFog.g = sGlobalFog.globalFogColor.g;
            shaderData.sGlobalFog.b = sGlobalFog.globalFogColor.b;
            shaderData.sGlobalFog.a = sGlobalFog.globalFogColor.a;

            shaderData.sSepia_enabled = sSepia.enabled;

            shaderData.sVignetting.enabled = sVignetting.enabled;
            shaderData.sVignetting.intensity = sVignetting.intensity;
            shaderData.sVignetting.chromaticAberration = sVignetting.chromaticAberration;
            shaderData.sVignetting.blur = sVignetting.blur;
            shaderData.sVignetting.blurSpread = sVignetting.blurSpread;
        }

        public void SetAllData(EMES_SceneManagement.ShaderDataNew shaderData)
        {
            sBloom.enabled = shaderData.sBloom.enabled;
            sBloom.screenBlendMode = (Bloom.BloomScreenBlendMode)shaderData.sBloom.screenBlendMode;
            sBloom.quality = (Bloom.BloomQuality)shaderData.sBloom.quality;
            sBloom.sepBlurSpread = shaderData.sBloom.sepBlurSpread;
            sBloom.bloomBlurIterations = shaderData.sBloom.bloomBlurIterations;

            sBlur.enabled = shaderData.sBlur.enabled;
            sBlur.blurSize = shaderData.sBlur.blurSize;
            sBlur.blurIterations = shaderData.sBlur.blurIterations;

            sDepthOfField.enabled = shaderData.sDepthOfField.enabled;
            sDepthOfField.visualizeFocus = shaderData.sDepthOfField.visualizeFocus;
            sDepthOfField.focalLength = shaderData.sDepthOfField.focalLength;
            sDepthOfField.focalSize = shaderData.sDepthOfField.focalSize;
            sDepthOfField.aperture = shaderData.sDepthOfField.aperture;
            sDepthOfField.maxBlurSize = shaderData.sDepthOfField.maxBlurSize;
            sDepthOfField.blurType = (DepthOfFieldScatter.BlurType)shaderData.sDepthOfField.blurType;

            sGlobalFog.enabled = shaderData.sGlobalFog.enabled;
            sGlobalFog.startDistance = shaderData.sGlobalFog.startDistance;
            sGlobalFog.globalDensity = shaderData.sGlobalFog.globalDensity;
            sGlobalFog.heightScale = shaderData.sGlobalFog.heightScale;
            sGlobalFog.height = shaderData.sGlobalFog.height;
            sGlobalFog.globalFogColor = new Color(shaderData.sGlobalFog.a, shaderData.sGlobalFog.r, shaderData.sGlobalFog.g, shaderData.sGlobalFog.b);

            sSepia.enabled = shaderData.sSepia_enabled;

            sVignetting.enabled = shaderData.sVignetting.enabled;
            sVignetting.intensity = shaderData.sVignetting.intensity;
            sVignetting.chromaticAberration = shaderData.sVignetting.chromaticAberration;
            sVignetting.blur = shaderData.sVignetting.blur;
            sVignetting.blurSpread = shaderData.sVignetting.blurSpread;
        }
        #endregion
    }

    //Camera Plus
    public class CameraPlus
    {
        public const string PluginName = "CameraPlus Lite";
        public const string Version = "1.1.0.0";

        private EMES Super;

        protected bool init = false;

        //カメラ
        protected CameraMain MainCamera;
        protected GameObject MainCameraObject;

        //カメラ位置
        private Dictionary<string, float> CameraPosData;
        private float CameraSpeed = 0.05f;
        public List<Dictionary<string, float>> CameraPosDataList { get; private set; }

        //サブライト
        private GameObject goSubLightMaster;
        public List<GameObject> SubLightList;

        //public
        public CameraPlus Instance
        {
            get
            {
                return m_objInstance;
            }
        }
        private CameraPlus m_objInstance;

        public void SetupInstance(EMES super_instance)
        {
            Super = super_instance;
        }

        public CameraPlus()
        {
#if DEBUG
            Debuginfo.Log("Now Loading CameraPlus", 2);
#endif
            m_objInstance = this;
            CameraPosData = new Dictionary<string, float>();
            CameraPosDataList = new List<Dictionary<string, float>>();
            for(int index = 0; index <5; index++)
            {
                CameraPosDataList.Add(new Dictionary<string, float>());
                CameraPosDataList[index].Add("px", 0);
                CameraPosDataList[index].Add("py", 0);
                CameraPosDataList[index].Add("pz", 0);
                CameraPosDataList[index].Add("rx", 0);
                CameraPosDataList[index].Add("ry", 0);
                CameraPosDataList[index].Add("rz", 1.5f);
                CameraPosDataList[index].Add("d", 2f);
                CameraPosDataList[index].Add("fov", 35f);
                CameraPosDataList[index].Add("TAG_無", index);
            }
            MainCamera = GameMain.Instance.MainCamera;
            MainCameraObject = GameMain.Instance.MainCamera.camera.gameObject;

            goSubLightMaster = InitSubLight();
            SubLightList = new List<GameObject>();
#if DEBUG
            Debuginfo.Log("Loading CameraPlus Done!", 2);
#endif
        }

        public void CameraPlus_Finalized()
        {
#if DEBUG
            Debuginfo.Log("CameraPlus Finalize ...", 2);
#endif
            if(null != CameraPosData)
            {
                CameraPosData.Clear();
                CameraPosData = null;
                CameraPosDataList.Clear();
                CameraPosDataList = null;
            }

            if (null != SubLightList)
            {
                SubLightList.Clear();
                SubLightList = null;
            }

            UnityEngine.Object.Destroy(goSubLightMaster);
            goSubLightMaster = null;
#if DEBUG
            Debuginfo.Log("CameraPlus Finalized Done", 2);
#endif
        }

        #region camera pos
        //カメラ位置
        public void CameraDataSlotInit(int iIndex, EMES_SceneManagement.CameraDataSlot cameraDataSlot)
        {
            if (iIndex >= CameraPosDataList.Count)
                return;

            CameraPosDataList[iIndex].Clear();
            CameraPosDataList[iIndex].Add("px", cameraDataSlot.pos_x);
            CameraPosDataList[iIndex].Add("py", cameraDataSlot.pos_y);
            CameraPosDataList[iIndex].Add("pz", cameraDataSlot.pos_z);
            CameraPosDataList[iIndex].Add("rx", cameraDataSlot.rot_x);
            CameraPosDataList[iIndex].Add("ry", cameraDataSlot.rot_y);
            CameraPosDataList[iIndex].Add("rz", cameraDataSlot.rot_z);
            CameraPosDataList[iIndex].Add("d", cameraDataSlot.distance);
            CameraPosDataList[iIndex].Add("fov", cameraDataSlot.fov);
            CameraPosDataList[iIndex].Add(cameraDataSlot.tag, iIndex);
        }

        public void CameraDataSlotSave(int iIndex, string sTag)
        {
            if (iIndex >= CameraPosDataList.Count)
                return;

            Vector3 CameraPos = MainCamera.GetTargetPos();
            Vector3 CameraRotation = MainCamera.camera.transform.rotation.eulerAngles;

            CameraPosDataList[iIndex].Clear();
            CameraPosDataList[iIndex].Add("px", CameraPos.x);
            CameraPosDataList[iIndex].Add("py", CameraPos.y);
            CameraPosDataList[iIndex].Add("pz", CameraPos.z);
            CameraPosDataList[iIndex].Add("rx", CameraRotation.x);
            CameraPosDataList[iIndex].Add("ry", CameraRotation.y);
            CameraPosDataList[iIndex].Add("rz", CameraRotation.z);
            CameraPosDataList[iIndex].Add("d", MainCamera.GetDistance());
            CameraPosDataList[iIndex].Add("fov", MainCamera.camera.fieldOfView);
            CameraPosDataList[iIndex].Add("TAG_"+sTag, iIndex);
        }

        public void CameraDataSlotLoad(int iIndex)
        {
            if (iIndex >= CameraPosDataList.Count)
                return;

            if (CameraPosDataList[iIndex].Count < 1)
                return;

            MainCamera.SetTargetPos(new Vector3(CameraPosDataList[iIndex]["px"], CameraPosDataList[iIndex]["py"], CameraPosDataList[iIndex]["pz"]), false);
            MainCamera.SetDistance(CameraPosDataList[iIndex]["d"], false);
            MainCamera.camera.fieldOfView = CameraPosDataList[iIndex]["fov"];
            MainCamera.SetRotation(new Vector3(CameraPosDataList[iIndex]["rx"], CameraPosDataList[iIndex]["ry"], CameraPosDataList[iIndex]["rz"]));

            Super.Window.UpdateCameraFOVinfo();
        }

        public void CameraQuickSave()
        {
            Vector3 CameraPos = MainCamera.GetTargetPos();
            Vector3 CameraRotation = MainCamera.camera.transform.rotation.eulerAngles;

            CameraPosData.Clear();
            CameraPosData.Add("px", CameraPos.x);
            CameraPosData.Add("py", CameraPos.y);
            CameraPosData.Add("pz", CameraPos.z);
            CameraPosData.Add("ry", CameraRotation.y);
            CameraPosData.Add("rx", CameraRotation.x);
            CameraPosData.Add("rz", CameraRotation.z);
            CameraPosData.Add("d", MainCamera.GetDistance());
            CameraPosData.Add("fov", MainCamera.camera.fieldOfView);
        }

        public void CameraQuickLoad()
        {
            if (CameraPosData.Count < 1)
                return;

            MainCamera.SetTargetPos(new Vector3(CameraPosData["px"], CameraPosData["py"], CameraPosData["pz"]), false);
            MainCamera.SetDistance(CameraPosData["d"], false);
            MainCamera.camera.fieldOfView = CameraPosData["fov"];
            MainCamera.SetRotation(new Vector3(CameraPosData["rx"], CameraPosData["ry"], CameraPosData["rz"]));

            Super.Window.UpdateCameraFOVinfo();
        }

        public void CameraKeyProcess(SettingsXML settingsXml, Maid maid, Transform Target)
        {
            Transform CameraPos = GameMain.Instance.MainCamera.camera.transform;
            Vector3 CameraRotation = GameMain.Instance.MainCamera.camera.transform.rotation.eulerAngles;
            float CameraDistance = GameMain.Instance.MainCamera.GetDistance();
            float CameraFieldOfView = GameMain.Instance.MainCamera.camera.fieldOfView;

            //カメラシフト
            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveForward))   //"c+↑"
            {
                Vector3 Position = new Vector3(CameraPos.forward.x * CameraSpeed, CameraPos.forward.y * CameraSpeed, CameraPos.forward.z * CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveBackward)) //"c+↓"
            {
                Vector3 Position = new Vector3(CameraPos.forward.x * -CameraSpeed, CameraPos.forward.y * -CameraSpeed, CameraPos.forward.z * -CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }

            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveLeft))  //"c+←"
            {
                Vector3 Position = new Vector3(CameraPos.right.x * -CameraSpeed, CameraPos.right.y * -CameraSpeed, CameraPos.right.z * -CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveRight))    //"c+→"
            {
                Vector3 Position = new Vector3(CameraPos.right.x * CameraSpeed, CameraPos.right.y * CameraSpeed, CameraPos.right.z * CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }

            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveUp))    //"c+home"
            {
                Vector3 Position = new Vector3(CameraPos.up.x * CameraSpeed, CameraPos.up.y * CameraSpeed, CameraPos.up.z * CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraMoveDown)) //"c+end"
            {
                Vector3 Position = new Vector3(CameraPos.up.x * -CameraSpeed, CameraPos.up.y * -CameraSpeed, CameraPos.up.z * -CameraSpeed);
                GameMain.Instance.MainCamera.SetTargetPos(GameMain.Instance.MainCamera.GetTargetPos() + Position, false);
            }

            //カメラチルト
            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateVerticalUp))    //"a+↑"
            {
                float rx = CameraRotation.x + CameraSpeed * 25;
                if (rx > 360) rx -= 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(rx, CameraRotation.y, CameraRotation.z));
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateVerticalDown))  //"a+↓"
            {
                float rx = CameraRotation.x - CameraSpeed * 25;
                if (rx < 0) rx += 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(rx, CameraRotation.y, CameraRotation.z));
            }

            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateHorizontalLeft))  //"a+←"
            {
                float ry = CameraRotation.y + CameraSpeed * 25;
                if (ry > 360) ry -= 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(CameraRotation.x, ry, CameraRotation.z));
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateHorizontalRight))    //"a+→"
            {
                float ry = CameraRotation.y - CameraSpeed * 25;
                if (ry < 0) ry += 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(CameraRotation.x, ry, CameraRotation.z));
            }

            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateLeft))  //"a+home"
            {
                float rz = CameraRotation.z - CameraSpeed * 25;
                if (rz < 0) rz += 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(CameraRotation.x, CameraRotation.y, rz));
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraRotateRight))    //"a+end"
            {
                float rz = CameraRotation.z + CameraSpeed * 25;
                if (rz > 360) rz -= 360;
                GameMain.Instance.MainCamera.SetRotation(new Vector3(CameraRotation.x, CameraRotation.y, rz));
            }

            //Reset
            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraResetToMaid))    //"a+delete"
            {
                GameMain.Instance.MainCamera.camera.fieldOfView = 35;
                GameMain.Instance.MainCamera.SetDistance(3, true);
                SceneEdit componentSceneEdit = GameObject.Find("__SceneEdit__").GetComponent<SceneEdit>();
                componentSceneEdit.PartsTypeCamera(MPN.stkg);
                Super.Window.CamPlus_UpdateFOV();
            }

            //カメラの距離
            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraDistanceClose))   //"s+↑"
            {
                CameraDistance -= CameraSpeed;
                if (CameraDistance < 0) CameraDistance = 0.0001f;
                GameMain.Instance.MainCamera.SetDistance(CameraDistance, false);
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraDistanceFar)) //"s+↓"
            {
                CameraDistance += CameraSpeed;
                if (CameraDistance > 30) CameraDistance = 30;
                GameMain.Instance.MainCamera.SetDistance(CameraDistance, false);
            }

            //カメラの視野
            if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraFieldOfViewWider))    //"home"
            {
                CameraFieldOfView -= CameraSpeed * 5;
                if (CameraFieldOfView < 0) CameraFieldOfView = 0.0001f;
                GameMain.Instance.MainCamera.camera.fieldOfView = CameraFieldOfView;
                Super.Window.CamPlus_UpdateFOV();
            }
            else if (FlexKeycode.GetMultipleKey(settingsXml.sHotkeyCameraFieldOfViewNarrower)) //"end"
            {
                CameraFieldOfView += CameraSpeed * 5;
                if (CameraFieldOfView > 180) CameraFieldOfView = 180;
                GameMain.Instance.MainCamera.camera.fieldOfView = CameraFieldOfView;
                Super.Window.CamPlus_UpdateFOV();
            }

            //移動速度
            if (FlexKeycode.GetMultipleKeyDown(settingsXml.sHotkeyCameraMovementSlower))    //"-"
            {
                CameraSpeed -= 0.005f;
                if (CameraSpeed < 0.005f) CameraSpeed = 0.005f;
#if DEBUG
                Debuginfo.Log("CameraSpeed = " + CameraSpeed, 0);
#endif
            }
            else if (FlexKeycode.GetMultipleKeyDown(settingsXml.sHotkeyCameraMovementFaster))  //"="
            {
                CameraSpeed += 0.005f;
                if (CameraSpeed > 0.5f) CameraSpeed = 0.5f;
#if DEBUG
                Debuginfo.Log("CameraSpeed = " + CameraSpeed, 0);
#endif
            }

            //メイドを視界に移動
            if (FlexKeycode.GetMultipleKeyDown(settingsXml.sHotkeyCameraMoveMaidToView))  //"a+insert"
            {
                Vector3 Position = GameMain.Instance.MainCamera.GetTargetPos();
                float DistanceY = Vector3.Distance(new Vector3(0, Position.y, 0), new Vector3(0, Target.position.y, 0));
                float OverrideY = Position.y - DistanceY;

                maid.SetPos(new Vector3(Position.x, OverrideY, Position.z));
            }
        }
        #endregion

        public void SetMainLight(Vector3 rot, Color color, float brightness, float shadow)
        {
            GameMain.Instance.MainLight.SetRotation(rot);
            GameMain.Instance.MainLight.SetColor(color);
            GameMain.Instance.MainLight.SetIntensity(brightness);
            GameMain.Instance.MainLight.SetShadowStrength(shadow);
        }

        public void ResetMainLight()
        {
            GameMain.Instance.MainLight.Reset();
        }

        public void CreateSubLight(LightType lt, out GameObject subLight, out int Index)
        {
            GameObject gameObject3 = new GameObject();
            gameObject3.name = "SubLight_" + Guid.NewGuid().ToString();
            Light light = gameObject3.AddComponent<Light>();
            light.type = lt;
            light.range = 5f;
            light.enabled = true;
            light.intensity = 5f;
            light.spotAngle = 30f;
            gameObject3.transform.SetParent(goSubLightMaster.transform, false);
            gameObject3.SetActive(true);
            SubLightList.Add(gameObject3);
            Index = SubLightList.Count - 1;
            subLight = gameObject3;
        }

        public void RemoveSubLight(GameObject goLight)
        {
            if (null == goLight)
                return;

            SubLightList.Remove(goLight);
            goLight.SetActive(false);
            UnityEngine.Object.DestroyImmediate(goLight);
        }

        public GameObject SetSubLight(int Index, bool bEnable, LightType lt, float range, float intensity, float spotAngle)
        {
            GameObject goLight = SubLightList[Index];
            goLight.SetActive(bEnable);
            Light light = goLight.GetComponent<Light>();
            light.enabled = bEnable;
            light.type = lt;
            light.range = range;
            light.intensity = intensity;
            light.spotAngle = spotAngle;

            return goLight;
        }

        public GameObject GetSubLight(int Index, out bool bEnable, out LightType lt, out int  range, out int  intensity, out int spotAngle)
        {
            GameObject goLight = SubLightList[Index];

            Light light = goLight.GetComponent<Light>();
            bEnable = light.enabled;
            lt = light.type;
            range = (int)(light.range * 100);
            intensity = (int)(light.intensity * 100);
            spotAngle = (int)(light.spotAngle * 100);

            return goLight;
        }

        private GameObject InitSubLight()
        {
            Transform transform = GameMain.Instance.BgMgr.bg_parent_object.transform.Find("LightObject");
            if (transform == null)
            {
                GameObject gameObject = new GameObject("LightObject");
                gameObject.transform.SetParent(GameMain.Instance.BgMgr.bg_parent_object.transform, false);
                transform = gameObject.transform;
            }

            GameObject gameObject2 = new GameObject();
            gameObject2.name = Guid.NewGuid().ToString();           
            gameObject2.transform.SetParent(transform, false);
            gameObject2.transform.localPosition = new Vector3(0f, 1f, 0f);

            return gameObject2;
        }

        protected static Color ConvertUnityRGBA(System.Drawing.Color color)
        {
            return new Color(((float)color.R / 255), ((float)color.G / 255), ((float)color.B / 255), ((float)color.A / 255));
        }

        protected static System.Drawing.Color ConvertSystemARGB(Color color)
        {
            return System.Drawing.Color.FromArgb(255, (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255));
        }

        #region Utility methods
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

        //ResetProp
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
            var goArray = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
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
        #endregion
    }
}



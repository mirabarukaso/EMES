using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class EMES_SceneManagement //: PluginBase
    {
		public class MaidData
        {
			public string guid;
			public string firstName;
			public string lastName;
			public float pos_x;
			public float pos_y;
			public float pos_z;
			public float rot_x;
			public float rot_y;
			public float rot_z;
			public float scale;
			public byte[] poseData;
		}

		public class CameraData
        {
			public float pos_x;
			public float pos_y;
			public float pos_z;
			public float rot_x;
			public float rot_y;
			public float rot_z;
			public float rot_w;
			public float distance;
			public float fov;
		}

		public class CameraDataSlot
		{
			public float pos_x;
			public float pos_y;
			public float pos_z;
			public float rot_x;
			public float rot_y;
			public float rot_z;
			public float distance;
			public float fov;
			public string tag;
		}

		//v1.2
		public class ShaderData
        {
			public bool sBloom_enabled;
            public int sBloom_screenBlendMode;
			public int sBloom_quality;
			public float sBloom_sepBlurSpread;
			public int sBloom_bloomBlurIterations;

			public bool sBlur_enabled;
			public float sBlur_blurSize;
			public int sBlur_blurIterations;

			public bool sDepthOfField_enabled;
			public bool sDepthOfField_visualizeFocus;
			public float sDepthOfField_focalLength;
			public float sDepthOfField_focalSize;
			public float sDepthOfField_aperture;
			public float sDepthOfField_maxBlurSize;
			public int sDepthOfField_blurType;

			public bool sGlobalFog_enabled;
			public float sGlobalFog_startDistance;
			public float sGlobalFog_globalDensity;
			public float sGlobalFog_heightScale;
			public float sGlobalFog_height;
			public float sGlobalFog_globalFogColor_r;
			public float sGlobalFog_globalFogColor_g;
			public float sGlobalFog_globalFogColor_b;
			public float sGlobalFog_globalFogColor_a;

			public bool sSepia_enabled;
		}

		//v1.3
		public class Shader_Bloom
        {
			public bool enabled;
			public int screenBlendMode;
			public int quality;
			public float sepBlurSpread;
			public int bloomBlurIterations;
		}

		public class Shader_Blur
        {
			public bool enabled;
			public float blurSize;
			public int blurIterations;
		}

		public class Shader_DepthOfField
		{
			public bool enabled;
			public bool visualizeFocus;
			public float focalLength;
			public float focalSize;
			public float aperture;
			public float maxBlurSize;
			public int blurType;
		}

		public class Shader_GlobalFog
		{

			public bool enabled;
			public float startDistance;
			public float globalDensity;
			public float heightScale;
			public float height;
			public float r;
			public float g;
			public float b;
			public float a;
		}

		public class Shader_Vignetting
		{
			public bool enabled;
			public float intensity;
			public float chromaticAberration;
			public float blurSpread;
			public float blur;
		}

		public class ShaderDataNew
        {
			public Shader_Bloom sBloom = new Shader_Bloom();
			public Shader_Blur sBlur = new Shader_Blur();
			public Shader_DepthOfField sDepthOfField = new Shader_DepthOfField();
			public Shader_GlobalFog sGlobalFog = new Shader_GlobalFog();
			public Shader_Vignetting sVignetting = new Shader_Vignetting();
			public bool sSepia_enabled;
		}

		public class MainLightData
		{
			public int rot_x;
			public int rot_y;
			public int color_r;
			public int color_g;
			public int color_b;
			public int brightness;
			public int shadow;
		}

		public class SubLightData
		{
			public bool enable;
			public int range;
			public int brightness;
			public int spotAngle;
			public int type;

			public float pos_x;
			public float pos_y;
			public float pos_z;
			public float rot_x;
			public float rot_y;
			public float rot_z;
			public float rot_w;
		}

		public class BonePosRotScaleInfo
		{
			public float localPosition_x;
			public float localPosition_y;
			public float localPosition_z;
			public float localScale;
			public float localRotation_x;
			public float localRotation_y;
			public float localRotation_z;
			public float localRotation_w;
			public string name;
			public string ShortName = "無";
		}

		public class ItemsHandleInfo
        {
			public float Position_x;
			public float Position_y;
			public float Position_z;
			public float Rotation_x;
			public float Rotation_y;
			public float Rotation_z;
			public float Rotation_w;

			public float localScale_x;
			public float localScale_y;
			public float localScale_z;

			public float localPosition_x;
			public float localPosition_y;
			public float localPosition_z;
			public float localRotation_x;
			public float localRotation_y;
			public float localRotation_z;
			public float localRotation_w;

			public string sCategory;
			public string sItemName;
			public string sItemFullName;
			public string sShowName;

			public string sOptions;
		}

		//1.1.0.0 v1.3
		public class SceneDataNew
		{
			public string XmlDataVersion = "1.3";

			public byte[] ScreenShot;
			public string ScreenShotName;
			public string DateTime;
			public string BackGround;

			public CameraData cameraData = new CameraData();
			public List<CameraDataSlot> cameraDataSlot = new List<CameraDataSlot>();
			public ShaderDataNew shaderData = new ShaderDataNew();

			public MainLightData mainLight = new MainLightData();
			public List<SubLightData> subLight = new List<SubLightData>();

			public List<MaidData> Maids = new List<MaidData>();
			public List<List<BonePosRotScaleInfo>> maidTailsBoneData = new List<List<BonePosRotScaleInfo>>();

			public List<ItemsHandleInfo> itemsHandleInfo = new List<ItemsHandleInfo>();
		}
		public List<SceneDataNew> MaidSceneData;

		//1.0.0.0 v1.2
		public class SceneData
        {
			public string XmlDataVersion = "1.2";

			public byte[] ScreenShot;
			public string ScreenShotName;
			public string DateTime;
			public string BackGround;

			public CameraData cameraData = new CameraData();
			public ShaderData shaderData;

			public MainLightData mainLight = new MainLightData();
			public List<SubLightData> subLight = new List<SubLightData>();

			public List<MaidData> Maids = new List<MaidData>();
			public List<List<BonePosRotScaleInfo>> maidTailsBoneData = new List<List<BonePosRotScaleInfo>>();

			public List<ItemsHandleInfo> itemsHandleInfo = new List<ItemsHandleInfo>();
		}
		public List<SceneData> MaidSceneDataOld;

		/*
		//0.7.5.0  v1.1
		public class SceneDataNew
		{
			public string XmlDataVersion = "1.1";

			public byte[] ScreenShot;
			public string ScreenShotName;
			public string DateTime;
			public string BackGround;

			public CameraData cameraData = new CameraData();
			public ShaderData shaderData;

			public MainLightData mainLight = new MainLightData();
			public List<SubLightData> subLight = new List<SubLightData>();

			public List<MaidData> Maids = new List<MaidData>();
			public List<List<BonePosRotScaleInfo>> maidTailsBoneData = new List<List<BonePosRotScaleInfo>>();
		}
		public List<SceneDataNew> MaidSceneDataOld;

		//0.7.2.0 v1.0
		public class SceneData
		{
			public byte[] ScreenShot;
			public string ScreenShotName;
			public string DateTime;
			public string BackGround;

			public CameraData cameraData = new CameraData();
			public ShaderData shaderData;

			public MainLightData mainLight = new MainLightData();
			public List<SubLightData> subLight = new List<SubLightData>();

			public List<MaidData> Maids = new List<MaidData>();
		}
		//*/

		private List<UICamera> ui_cam_hide_list_ = new List<UICamera>();
		private List<GameObject> ui_hide_object_list_ = new List<GameObject>();
		private bool hide_ui_;
		private string sceneDataFileName;

		private readonly string xmlVersion = "1.3";

		public EMES_SceneManagement(string configDir)
        {
			Init(configDir);
		}

		public void Finalized()
		{
#if DEBUG
			Debuginfo.Log("EMES_SceneManagement Finalize ...", 2);
#endif
			HideUI(false);

			MaidSceneData.Clear();
			if (null != MaidSceneDataOld)
				MaidSceneDataOld.Clear();

			MaidSceneData = null;
			MaidSceneDataOld = null;
#if DEBUG
			Debuginfo.Log("EMES_SceneManagement Finalized Done", 2);
#endif
		}

		public static string ScreenShotName(int width, int height)
        {
			return string.Format("EMES_screen_{0}x{1}.png", width, height);
		}

		public static string ScreenShotTime()
        {
			return string.Format(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}

		public byte[] ScreenShot(int resWidth, int resHeight, bool hideUI)
        {
			if(true == hideUI)
				UIHide();
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
			GameMain.Instance.MainCamera.camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			GameMain.Instance.MainCamera.camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			GameMain.Instance.MainCamera.camera.targetTexture = null;
            RenderTexture.active = null; 
            UnityEngine.Object.Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();

			if (true == hideUI) 
				UIResume();

			return bytes;
		}

		public void SaveMaidSceneData()
        {
			if (null == MaidSceneData)
				return;

#if DEBUG
			Debuginfo.Log("Save XML FileName=" + sceneDataFileName, 2);
#endif
			XmlSerializer xs = new XmlSerializer(typeof(List<SceneDataNew>));
			TextWriter tw = new StreamWriter(sceneDataFileName);
			xs.Serialize(tw, MaidSceneData);
			tw.Close();

		}

		public SceneDataNew ImportSceneData(string sXMLFileName, string sXMLFilePath, string sNameNoExt)
        {
#if DEBUG
			Debuginfo.Log("Import XML=" + sXMLFileName, 2);
#endif

			bool oldXML = false;
			using (var xml = new StreamReader(sXMLFileName))
			{
				for (int i = 0; i < 10; i++)
				{
					string Line = xml.ReadLine();

					if (true == Line.Contains("XmlDataVersion"))
					{
						if (true == Line.Contains("1.2"))
						{
							oldXML = true;
							break;
						}
						else if (true == Line.Contains("1.3"))
						{
							oldXML = false;
							break;
						}
						else
                        {
							Debuginfo.Warning("不明なシーンデータバージョン、インポートに失敗しました " + sXMLFileName, 0);
							Debuginfo.Warning("["+ Line+"]", 0);
							return null;
						}
					}
				}
			}

			SceneDataNew sceneDataNew = null;
			if (oldXML)
			{
				SceneData sd = null;
				XmlSerializer xs = new XmlSerializer(typeof(SceneData));
				try
				{
					using (var sr = new StreamReader(sXMLFileName))
					{
						sd = (SceneData)xs.Deserialize(sr);
					}
				}
				catch (Exception e)
				{
					Debuginfo.Warning("不正なXMLシーンファイル、無視する", 0);
					Debuginfo.Warning("Exception: [" + e + "]", 2);
					return null;
				}

				sceneDataNew = UpgradeSceneData(sd);
			}
			else
            {
				XmlSerializer xs = new XmlSerializer(typeof(SceneDataNew));
				try
				{
					using (var sr = new StreamReader(sXMLFileName))
					{
						sceneDataNew = (SceneDataNew)xs.Deserialize(sr);
					}
				}
				catch (Exception e)
				{
					Debuginfo.Warning("不正なXMLシーンファイル、無視する", 0);
					Debuginfo.Warning("Exception: [" + e + "]", 2);
					return null;
				}
			}

			if (null != sceneDataNew)
			{
				foreach (ItemsHandleInfo ihi in sceneDataNew.itemsHandleInfo)
				{
					if (true == string.Equals("ExternalImage", ihi.sCategory))
					{
						string sFullPath = sXMLFilePath + "\\" + ihi.sItemFullName.Split('|')[1];

						ihi.sItemFullName = sFullPath;
						Debuginfo.Log("外部PNG [" + ihi.sItemFullName + "]", 0);
					}
				}

				MaidSceneData.Add(sceneDataNew);
				SaveMaidSceneData();
#if DEBUG
				Debuginfo.Log("Done", 2);
#endif
			}
			else
            {
#if DEBUG
				Debuginfo.Log("Failed", 2);
#endif
			}

			return sceneDataNew;
		}

		public void ExportSceneData(string sXMLFileName, string sXMLFilePath, string sNameNoExt, EMES_SceneManagement.SceneDataNew sdo)
		{
#if DEBUG
			Debuginfo.Log("Export XML=" + sXMLFileName, 2);
#endif
			SceneDataNew sd = new SceneDataNew()
			{
				ScreenShot = sdo.ScreenShot,
				ScreenShotName = sNameNoExt,
				DateTime = sdo.DateTime,
				BackGround = sdo.BackGround,
				cameraData = sdo.cameraData,
				cameraDataSlot = sdo.cameraDataSlot,
				shaderData = sdo.shaderData,
				mainLight = sdo.mainLight,
				subLight = sdo.subLight,
				Maids = sdo.Maids,
				maidTailsBoneData = sdo.maidTailsBoneData,
				itemsHandleInfo = sdo.itemsHandleInfo
			};

			int Index = 0;
			foreach(ItemsHandleInfo ihi in sd.itemsHandleInfo.ToArray())
            {
				if (true == string.Equals("ExternalImage", ihi.sCategory))
                {
					string sFullPath = ihi.sItemFullName;
					string sFileName = sNameNoExt + "_" + sd.DateTime + "_" + Index.ToString() + ".png";
#if DEBUG
					Debuginfo.Log("copy sFileName=" + sFileName, 2);
					Debuginfo.Log("copy sFileName=" + sXMLFilePath + "\\" + sFileName, 2);
#endif
					System.IO.File.Copy(ihi.sItemFullName, sXMLFilePath + "\\" + sFileName, true);
					ihi.sItemFullName = "EXTERNAL|" + sFileName;
					Debuginfo.Log("外部PNG [" + sFullPath + "] >> [" + ihi.sItemFullName + "]", 0);					
					Index++;
				}
			}

			XmlSerializer xs = new XmlSerializer(typeof(SceneDataNew));
			TextWriter tw = new StreamWriter(sXMLFileName);
			xs.Serialize(tw, sd);
			tw.Close();

#if DEBUG
			Debuginfo.Log("Done", 2);
#endif
		}

		public void HideUI(bool bHide)
        {
			if (true == bHide)
				UIHide();
			else
				UIResume();
        }

		#region private method
		private void Init(string configDir)
        {
#if DEBUG
			Debuginfo.Log("sceneManager Init", 2);
#endif

			sceneDataFileName = configDir + "EMES_MaidsceneData.xml";
			if (!File.Exists(sceneDataFileName))
			{
				MaidSceneData = new List<SceneDataNew>();
			}
			else
			{
#if DEBUG
				Debuginfo.Log("Loading XML=" + sceneDataFileName, 2);
#endif
				bool oldXML = true;
				using (var xml = new StreamReader(sceneDataFileName))
				{
					for (int i = 0; i < 10; i++)
					{
						string Line = xml.ReadLine();

						if(true == Line.Contains("XmlDataVersion") && true == Line.Contains(xmlVersion))
                        {
							oldXML = false;
							break;
                        }
					}
				}

				if (true == oldXML)
				{
#if DEBUG
					Debuginfo.Warning("アップグレード EMES_MaidsceneData.xml", 2);
#endif
					MaidSceneDataOld = new List<SceneData>();
					XmlSerializer xs = new XmlSerializer(typeof(List<SceneData>));
					using (var sr = new StreamReader(sceneDataFileName))
					{
						MaidSceneDataOld = (List<SceneData>)xs.Deserialize(sr);
					}

					MaidSceneData = new List<SceneDataNew>();
					UpgradeXML();

					string sBackupname = sceneDataFileName + "_12_backup";
					if(true == File.Exists(sBackupname))
                    {
						Debuginfo.Log("削除: " + sBackupname, 0);
						File.Delete(sBackupname);
                    }
					Debuginfo.Warning("バックアップして名前を変更" + sBackupname, 0);
					File.Move(sceneDataFileName, sBackupname);
					SaveMaidSceneData();
#if DEBUG
					Debuginfo.Warning("アップグレード完了", 2);
#endif
				}
				else
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<SceneDataNew>));
					using (var sr = new StreamReader(sceneDataFileName))
					{
						MaidSceneData = (List<SceneDataNew>)xs.Deserialize(sr);
					}
				}
			}

#if DEBUG
			Debuginfo.Log("sceneManager Init Done", 2);
#endif
		}

		//v1.2 to v1.3
		private SceneDataNew UpgradeSceneData(SceneData sdo)
        {
#if DEBUG
			Debuginfo.Log("アップグレード " + sdo.DateTime, 2);
#endif
			SceneDataNew sceneDataNew = new SceneDataNew()
			{
				ScreenShot = sdo.ScreenShot,
				ScreenShotName = sdo.ScreenShotName,
				DateTime = sdo.DateTime,
				BackGround = sdo.BackGround,
				cameraData = sdo.cameraData,
				mainLight = sdo.mainLight,
				subLight = sdo.subLight,
				Maids = sdo.Maids,
				maidTailsBoneData = sdo.maidTailsBoneData
			};

			Action<int> actionCameraDataSlot = delegate (int index)
			{
				sceneDataNew.cameraDataSlot[index].pos_x = 0;
                sceneDataNew.cameraDataSlot[index].pos_y = 0f;
				sceneDataNew.cameraDataSlot[index].pos_z = 1.5f;
				sceneDataNew.cameraDataSlot[index].rot_x = 0;
				sceneDataNew.cameraDataSlot[index].rot_y = 0;
				sceneDataNew.cameraDataSlot[index].rot_z = 0;
				sceneDataNew.cameraDataSlot[index].distance = 2f;
				sceneDataNew.cameraDataSlot[index].fov = 35f;
				sceneDataNew.cameraDataSlot[index].tag = "TAG_無";
			};

			for (int index = 0; index < 5; index++)
			{
				sceneDataNew.cameraDataSlot.Add(new CameraDataSlot());
				actionCameraDataSlot(index);
			}

			sceneDataNew.shaderData.sBloom.enabled = sdo.shaderData.sBloom_enabled;		
			sceneDataNew.shaderData.sBloom.screenBlendMode = sdo.shaderData.sBloom_screenBlendMode;
			sceneDataNew.shaderData.sBloom.quality = sdo.shaderData.sBloom_quality;
			sceneDataNew.shaderData.sBloom.sepBlurSpread = sdo.shaderData.sBloom_sepBlurSpread;
			sceneDataNew.shaderData.sBloom.bloomBlurIterations = sdo.shaderData.sBloom_bloomBlurIterations;

			sceneDataNew.shaderData.sBlur.enabled = sdo.shaderData.sBlur_enabled;
			sceneDataNew.shaderData.sBlur.blurSize = sdo.shaderData.sBlur_blurSize;
			sceneDataNew.shaderData.sBlur.blurIterations = sdo.shaderData.sBlur_blurIterations;

			sceneDataNew.shaderData.sDepthOfField.enabled = sdo.shaderData.sDepthOfField_enabled;
			sceneDataNew.shaderData.sDepthOfField.visualizeFocus = sdo.shaderData.sDepthOfField_visualizeFocus;
			sceneDataNew.shaderData.sDepthOfField.focalLength = sdo.shaderData.sDepthOfField_focalLength;
			sceneDataNew.shaderData.sDepthOfField.focalSize = sdo.shaderData.sDepthOfField_focalSize;
			sceneDataNew.shaderData.sDepthOfField.aperture = sdo.shaderData.sDepthOfField_aperture;
			sceneDataNew.shaderData.sDepthOfField.maxBlurSize = sdo.shaderData.sDepthOfField_maxBlurSize;
			sceneDataNew.shaderData.sDepthOfField.blurType = sdo.shaderData.sDepthOfField_blurType;

			sceneDataNew.shaderData.sGlobalFog.enabled = sdo.shaderData.sGlobalFog_enabled;
			sceneDataNew.shaderData.sGlobalFog.startDistance = sdo.shaderData.sGlobalFog_startDistance;
			sceneDataNew.shaderData.sGlobalFog.globalDensity = sdo.shaderData.sGlobalFog_globalDensity;
			sceneDataNew.shaderData.sGlobalFog.heightScale = sdo.shaderData.sGlobalFog_heightScale;
			sceneDataNew.shaderData.sGlobalFog.height = sdo.shaderData.sGlobalFog_height;
			sceneDataNew.shaderData.sGlobalFog.r = sdo.shaderData.sGlobalFog_globalFogColor_r;
			sceneDataNew.shaderData.sGlobalFog.g = sdo.shaderData.sGlobalFog_globalFogColor_g;
			sceneDataNew.shaderData.sGlobalFog.b = sdo.shaderData.sGlobalFog_globalFogColor_b;
			sceneDataNew.shaderData.sGlobalFog.a = sdo.shaderData.sGlobalFog_globalFogColor_a;

			sceneDataNew.shaderData.sVignetting.enabled = false;
			sceneDataNew.shaderData.sVignetting.intensity = -3.98f;
			sceneDataNew.shaderData.sVignetting.chromaticAberration = 2f;
			sceneDataNew.shaderData.sVignetting.blur = 0;
			sceneDataNew.shaderData.sVignetting.blurSpread = 0.82f;

			sceneDataNew.shaderData.sSepia_enabled = sdo.shaderData.sSepia_enabled;

			return sceneDataNew;
		}

		private void UpgradeXML()
        {
			foreach (SceneData sdo in MaidSceneDataOld)
            {
				SceneDataNew sceneDataNew = UpgradeSceneData(sdo);
				if(null != sceneDataNew)
					MaidSceneData.Add(sceneDataNew);
			}
		}

		private void UIHide()
		{
			if (true == hide_ui_)
				return;

			ui_cam_hide_list_.Clear();
			UICamera[] array = NGUITools.FindActive<UICamera>();
			foreach (UICamera uicamera in array)
			{
				Camera component = uicamera.GetComponent<Camera>();
				if (component.enabled)
				{
					UIRoot uiroot = NGUITools.FindInParents<UIRoot>(uicamera.gameObject);
					if (!(uiroot == null))
					{
						UIPanel component2 = uiroot.GetComponent<UIPanel>();
						if (!(component2 == null))
						{
							if (0 > component2.name.ToLower().IndexOf("fix") && 0f < component2.alpha)
							{
								component2.alpha = 0f;
								ui_cam_hide_list_.Add(uicamera);
							}
						}
					}
				}
			}
			ui_hide_object_list_.Clear();
			GameObject gameObject = GameObject.Find("WorldTransformAxisParent");
			if (gameObject != null)
			{
				Transform transform = gameObject.transform;
				for (int j = 0; j < transform.childCount; j++)
				{
					if (transform.gameObject.activeSelf)
					{
						transform.gameObject.SetActive(true);
						ui_hide_object_list_.Add(transform.gameObject);
					}
				}
				gameObject.SetActive(false);
			}
			GizmoRender.UIVisible = false;
			hide_ui_ = true;
		}

		private void UIResume()
		{
			if (false == hide_ui_)
				return;

			foreach (UICamera uicamera in ui_cam_hide_list_)
			{
				Camera component = uicamera.GetComponent<Camera>();
				UIRoot uiroot = NGUITools.FindInParents<UIRoot>(uicamera.gameObject);
				if (!(uiroot == null))
				{
					UIPanel component2 = uiroot.GetComponent<UIPanel>();
					if (component2 != null)
					{
						component2.alpha = 1f;
					}
				}
			}
			ui_cam_hide_list_.Clear();
			for (int i = 0; i < ui_hide_object_list_.Count; i++)
			{
				ui_hide_object_list_[i].SetActive(true);
			}
			ui_hide_object_list_.Clear();
			GizmoRender.UIVisible = true;
			hide_ui_ = false;
		}
		#endregion
	}
}


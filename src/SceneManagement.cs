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

		//1.0.0.0
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
		public List<SceneData> MaidSceneData;

		//0.7.5.0
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
		/*
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

		private readonly string xmlVersion = "1.2";

		public EMES_SceneManagement(string configDir)
        {
			Init(configDir);
		}

		public void Finalized()
		{
#if DEBUG
			Debuginfo.Log("EMES_SceneManagement Finalize ...", 2);
#endif
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

			XmlSerializer xs = new XmlSerializer(typeof(List<SceneData>));
			TextWriter tw = new StreamWriter(sceneDataFileName);
			xs.Serialize(tw, MaidSceneData);
			tw.Close();
#if DEBUG
			Debuginfo.Log("Save XML FileName=" + sceneDataFileName, 2);
#endif
		}

		public SceneData ImportSceneData(string sXMLFileName, string sXMLFilePath, string sNameNoExt)
        {
#if DEBUG
			Debuginfo.Log("Import XML=" + sXMLFileName, 2);
#endif
			SceneData sd = null;
			XmlSerializer xs = new XmlSerializer(typeof(SceneData));
			try
			{
				using (var sr = new StreamReader(sXMLFileName))
				{
					sd = (SceneData)xs.Deserialize(sr);
				}
			}
			catch(Exception e)
            {
				Debuginfo.Warning("不正なXMLシーンファイル、無視する", 0);
				Debuginfo.Warning("Exception: [" + e + "]", 2);
				return null;
			}

			foreach (ItemsHandleInfo ihi in sd.itemsHandleInfo)
			{
				if (true == string.Equals("ExternalImage", ihi.sCategory))
				{
					string sFullPath = sXMLFilePath + "\\" + ihi.sItemFullName.Split('|')[1];

					ihi.sItemFullName = sFullPath;
					Debuginfo.Log("外部PNG [" + ihi.sItemFullName + "]", 0);
				}
			}

			MaidSceneData.Add(sd);
			SaveMaidSceneData();
#if DEBUG
			Debuginfo.Log("Done", 2);
#endif
			return sd;
		}

		public void ExportSceneData(string sXMLFileName, string sXMLFilePath, string sNameNoExt, EMES_SceneManagement.SceneData sdo)
		{
#if DEBUG
			Debuginfo.Log("Export XML=" + sXMLFileName, 2);
#endif
			SceneData sd = new SceneData()
			{
				ScreenShot = sdo.ScreenShot,
				ScreenShotName = sNameNoExt,
				DateTime = sdo.DateTime,
				BackGround = sdo.BackGround,
				cameraData = sdo.cameraData,
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

			XmlSerializer xs = new XmlSerializer(typeof(SceneData));
			TextWriter tw = new StreamWriter(sXMLFileName);
			xs.Serialize(tw, sd);
			tw.Close();

#if DEBUG
			Debuginfo.Log("Done", 2);
#endif
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
				MaidSceneData = new List<SceneData>();
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
					MaidSceneDataOld = new List<SceneDataNew>();
					XmlSerializer xs = new XmlSerializer(typeof(List<SceneDataNew>));
					using (var sr = new StreamReader(sceneDataFileName))
					{
						MaidSceneDataOld = (List<SceneDataNew>)xs.Deserialize(sr);
					}

					MaidSceneData = new List<SceneData>();
					UpgradeXML();

					Debuginfo.Warning("バックアップして名前を変更" + sceneDataFileName + "_11_backup", 0);

					File.Move(sceneDataFileName, sceneDataFileName+"_11_backup");
					SaveMaidSceneData();
#if DEBUG
					Debuginfo.Warning("アップグレード完了", 2);
#endif
				}
				else
				{
					XmlSerializer xs = new XmlSerializer(typeof(List<SceneData>));
					using (var sr = new StreamReader(sceneDataFileName))
					{
						MaidSceneData = (List<SceneData>)xs.Deserialize(sr);
					}
				}
			}

#if DEBUG
			Debuginfo.Log("sceneManager Init Done", 2);
#endif
		}


		private void UpgradeXML()
        {
			foreach (SceneDataNew sdo in MaidSceneDataOld)
            {
#if DEBUG
				Debuginfo.Log("アップグレード " + sdo.DateTime, 2);
#endif
				SceneData sd = new SceneData()
				{
					ScreenShot = sdo.ScreenShot,
					ScreenShotName = sdo.ScreenShotName,
					DateTime = sdo.DateTime,
					BackGround = sdo.BackGround,
					cameraData = sdo.cameraData,
					shaderData = sdo.shaderData,
					mainLight = sdo.mainLight,
					subLight = sdo.subLight,
					Maids = sdo.Maids,
					maidTailsBoneData = sdo.maidTailsBoneData
				};
				MaidSceneData.Add(sd);
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


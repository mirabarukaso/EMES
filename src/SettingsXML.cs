using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    //ini設定用
    static class ConfigureFileHelper
    {
        [DllImport("KERNEL32.DLL")]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        public static extern uint WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        public static T Read<T>(string section, string filepath)
        {
            T ret = (T)Activator.CreateInstance(typeof(T));

            foreach (var n in typeof(T).GetFields())
            {
                if (n.FieldType == typeof(int))
                {
                    n.SetValue(ret, (int)GetPrivateProfileInt(section, n.Name, 0, Path.GetFullPath(filepath)));
                }
                else if (n.FieldType == typeof(uint))
                {
                    n.SetValue(ret, GetPrivateProfileInt(section, n.Name, 0, Path.GetFullPath(filepath)));
                }
                else if (n.FieldType == typeof(bool))
                {
                    var sb = new StringBuilder(1024);
                    GetPrivateProfileString(section, n.Name, "False", sb, (uint)sb.Capacity, Path.GetFullPath(filepath));
                    n.SetValue(ret, "True" == sb.ToString());
                }
                else
                {
                    var sb = new StringBuilder(1024);
                    GetPrivateProfileString(section, n.Name, "error", sb, (uint)sb.Capacity, Path.GetFullPath(filepath));
                    n.SetValue(ret, sb.ToString());
                }
            };

            return ret;
        }

        public static void Write<T>(string secion, T data, string filepath)
        {
            foreach (var n in typeof(T).GetFields())
            {
                WritePrivateProfileString(secion, n.Name, n.GetValue(data).ToString(), Path.GetFullPath(filepath));
            };
        }

        public static SettingsXML LoadSettings(string sFileName, bool bForceDefault)
        {
            SettingsXML settingsXml = new SettingsXML();

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(sFileName);
            if (xmldoc == null)
            {
                Debuginfo.Log("読み込みに失敗しました、新しいファイルを作成する", 0);
            }
            else
            {
                XmlNode config = xmldoc.GetElementsByTagName(EMES.PluginName)[0];
                XmlNode config_root = xmldoc.SelectSingleNode("//" + EMES.PluginName);
                if (config != null)
                {
                    FieldInfo[] fields = settingsXml.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    XmlNode add = null;
                    foreach (FieldInfo key in fields)
                    {
                        XmlNode node = config.SelectSingleNode("//" + EMES.PluginName + "//" + key.Name);
                        if (null == node)
                        {
                            Debuginfo.Warning(key.Name + "の設定がありません、デフォルト値を使用する " + key.GetValue(settingsXml).ToString(), 0);
                            add = xmldoc.CreateNode(XmlNodeType.Element, key.Name, null);
                            add.InnerText = key.GetValue(settingsXml).ToString();
                            config_root.AppendChild(add);
                        }
                        else
                        {
#if DEBUG
                            Debuginfo.Log("設定を読み込む " + key.Name + "[" + node.InnerText + "]", 0);
#endif
                            if ("VERSION" == key.Name)
                            {
                                if (false == string.Equals(EMES.PluginVersion, node.InnerText))
                                {
                                    Debuginfo.Warning("現在のプログラムバージョンと一致していません", 0);

                                    if (true == bForceDefault)
                                    {
                                        Debuginfo.Warning("自動的に初期設定に戻す", 0);                                        
                                        return new SettingsXML();
                                    }
                                    else
                                    {
                                        Debuginfo.Warning("最新版にアップグレードする [" + EMES.PluginVersion + "]", 0);
                                        key.SetValue(settingsXml, EMES.PluginVersion);
                                    }
                                }                               
                                continue;
                            }

                            if (typeof(bool) == key.FieldType)
                            {
                                if ("true" == node.InnerText.ToLower())
                                    key.SetValue(settingsXml, true);
                                else
                                    key.SetValue(settingsXml, false);
                            }
                            else if(typeof(int) == key.FieldType)
                            {
                                key.SetValue(settingsXml, int.Parse(node.InnerText));
                            }
                            else if (typeof(float) == key.FieldType)
                            {
                                key.SetValue(settingsXml, float.Parse(node.InnerText));
                            }
                            else
                            {
                                key.SetValue(settingsXml, node.InnerText.ToLower());
                            }
                        }                    
                    }

                    if (add != null)
                        xmldoc.Save(sFileName);
                }
            }

            return settingsXml;
        }

        public static void SaveSettings(SettingsXML settingsXml, string sFileName)
        {
            if (null == settingsXml || null == sFileName)
                return;

            XmlDocument xmldoc = new XmlDocument();
            XmlDeclaration declaration = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmldoc.AppendChild(declaration);

            XmlElement config = xmldoc.CreateElement(EMES.PluginName);
            xmldoc.AppendChild(config);

            XmlNode config_root = xmldoc.SelectSingleNode("//" + EMES.PluginName);
            XmlNode add;
            FieldInfo[] fields = settingsXml.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo key in fields)
            {
#if DEBUG
                //Debuginfo.Log("設定を書き込む " + key.Name + ":" + key.GetValue(settingsXml).ToString(), 0);
#endif
                add = xmldoc.CreateNode(XmlNodeType.Element, key.Name, null);
                add.InnerText = key.GetValue(settingsXml).ToString();
                config_root.AppendChild(add);
            }
            
            xmldoc.Save(sFileName);

#if DEBUG
            Debuginfo.Log("Save settingXML =" + sFileName, 2);
#endif
        }

        public static void FixSettings(SettingsXML settingsXml)
        {
            //iniファイルがある場合は設定されてない項目を補完
            settingsXml.ToggleKey = settingsXml.ToggleKey.ToLower();
            settingsXml.sHotkeyMaidPos = settingsXml.sHotkeyMaidPos.ToLower();
            settingsXml.sHotkeyMaidRot = settingsXml.sHotkeyMaidRot.ToLower();
            settingsXml.sHotkeyHead = settingsXml.sHotkeyHead.ToLower();
            settingsXml.sHotkeyNeck = settingsXml.sHotkeyNeck.ToLower();
            settingsXml.sHotkeyFingerX = settingsXml.sHotkeyFingerX.ToLower();
            settingsXml.sHotkeyFingerX1 = settingsXml.sHotkeyFingerX1.ToLower();
            settingsXml.sHotkeyFingerX2 = settingsXml.sHotkeyFingerX2.ToLower();
            settingsXml.sHotkeyArmIK = settingsXml.sHotkeyArmIK.ToLower();
            settingsXml.sHotkeyLegIK = settingsXml.sHotkeyLegIK.ToLower();
            settingsXml.sHotkeyRClavicle = settingsXml.sHotkeyRClavicle.ToLower();
            settingsXml.sHotkeyLClavicle = settingsXml.sHotkeyLClavicle.ToLower();
            settingsXml.sHotkeyHide = settingsXml.sHotkeyHide.ToLower();
            settingsXml.sHotkeyItemPos = settingsXml.sHotkeyItemPos.ToLower();
            settingsXml.sHotkeyItemRot = settingsXml.sHotkeyItemRot.ToLower();
            settingsXml.sHotkeyItemSize = settingsXml.sHotkeyItemSize.ToLower();
            settingsXml.sHotkeyItemDelete = settingsXml.sHotkeyItemDelete.ToLower();
            settingsXml.sHotkeyItemReloadParticle = settingsXml.sHotkeyItemReloadParticle.ToLower();
            settingsXml.sHotkeyItemReset = settingsXml.sHotkeyItemReset.ToLower();
            settingsXml.sHotkeyDanceStart = settingsXml.sHotkeyDanceStart.ToLower();
            settingsXml.sHotkeyDanceAllOtherStart = settingsXml.sHotkeyDanceAllOtherStart.ToLower();
            settingsXml.sHotkeyCameraQuickLoad = settingsXml.sHotkeyCameraQuickLoad.ToLower();
            settingsXml.sHotkeyCameraQuickSave = settingsXml.sHotkeyCameraQuickSave.ToLower();
            settingsXml.sHotkeyCameraScreenShot = settingsXml.sHotkeyCameraScreenShot.ToLower();

            settingsXml.sHotkeyCameraMoveLeft = settingsXml.sHotkeyCameraMoveLeft.ToLower();
            settingsXml.sHotkeyCameraMoveRight = settingsXml.sHotkeyCameraMoveRight.ToLower();
            settingsXml.sHotkeyCameraMoveForward = settingsXml.sHotkeyCameraMoveForward.ToLower();
            settingsXml.sHotkeyCameraMoveBackward = settingsXml.sHotkeyCameraMoveBackward.ToLower();
            settingsXml.sHotkeyCameraMoveUp = settingsXml.sHotkeyCameraMoveUp.ToLower();
            settingsXml.sHotkeyCameraMoveDown = settingsXml.sHotkeyCameraMoveDown.ToLower();

            settingsXml.sHotkeyCameraRotateHorizontalLeft = settingsXml.sHotkeyCameraRotateHorizontalLeft.ToLower();
            settingsXml.sHotkeyCameraRotateHorizontalRight = settingsXml.sHotkeyCameraRotateHorizontalRight.ToLower();
            settingsXml.sHotkeyCameraRotateVerticalUp = settingsXml.sHotkeyCameraRotateVerticalUp.ToLower();
            settingsXml.sHotkeyCameraRotateVerticalDown = settingsXml.sHotkeyCameraRotateVerticalDown.ToLower();
            settingsXml.sHotkeyCameraRotateLeft = settingsXml.sHotkeyCameraRotateLeft.ToLower();
            settingsXml.sHotkeyCameraRotateRight = settingsXml.sHotkeyCameraRotateRight.ToLower();
            settingsXml.sHotkeyCameraResetToMaid = settingsXml.sHotkeyCameraResetToMaid.ToLower();

            settingsXml.sHotkeyCameraDistanceClose = settingsXml.sHotkeyCameraDistanceClose.ToLower();
            settingsXml.sHotkeyCameraDistanceFar = settingsXml.sHotkeyCameraDistanceFar.ToLower();
            settingsXml.sHotkeyCameraFieldOfViewWider = settingsXml.sHotkeyCameraFieldOfViewWider.ToLower();
            settingsXml.sHotkeyCameraFieldOfViewNarrower = settingsXml.sHotkeyCameraFieldOfViewNarrower.ToLower();
            settingsXml.sHotkeyCameraMovementFaster = settingsXml.sHotkeyCameraMovementFaster.ToLower();
            settingsXml.sHotkeyCameraMovementSlower = settingsXml.sHotkeyCameraMovementSlower.ToLower();
            settingsXml.sHotkeyCameraMoveMaidToView = settingsXml.sHotkeyCameraMoveMaidToView.ToLower();
        }
    }

    public class SettingsXML
    {
        public string VERSION = EMES.PluginVersion;
        public string ToggleKey = "f7";
#if DEBUG
        public int DebugLogLevel = 2;
#else
        public int DebugLogLevel = 0;
#endif
        public string ANMFilesDirectory = @"\Mod\MultipleMaidsPose\";
        public string MirrorDirectory = @"\Mod\Mirror_props\";
        public string WaterbedsDirectory = @"\Mod\waterbeds\";
        public string OthersDirectory = @"\Mod\AssertBG\";
        public string BackgroundsDirectory = @"\Mod\AssertBG\Backgrounds\";

        public bool bYotogiRefreshCache = true;
        public bool bCreatePreviewIcon = true;

        public bool bHotkeyMaidPos = true;
        public bool bHotkeyMaidRot = true;
        public bool bHotkeyHead = true;
        public bool bHotkeyNeck = true;
        public bool bHotkeyFingerX = true;
        public bool bHotkeyFingerX1 = true;
        public bool bHotkeyFingerX2 = true;
        public bool bHotkeyArmIK = true;
        public bool bHotkeyLegIK = true;
        public bool bHotkeyRClavicle = true;
        public bool bHotkeyLClavicle = true;
        public bool bHotkeyHide = true;

        public string sHotkeyMaidPos = "z";      
        public string sHotkeyMaidRot = "x";
        public string sHotkeyHead = "h_shift+q";
        public string sHotkeyNeck = "h_shift+w";
        public string sHotkeyFingerX = "h_shift+e";
        public string sHotkeyFingerX1 = "h_shift+r";
        public string sHotkeyFingerX2 = "h_shift+t";
        public string sHotkeyArmIK = "h_ctrl+q";
        public string sHotkeyLegIK = "h_ctrl+w";
        public string sHotkeyRClavicle = "h_alt+q";
        public string sHotkeyLClavicle = "h_alt+w";
        public string sHotkeyHide = "space";

        public bool bHotkeyItemPos = true;
        public bool bHotkeyItemRot = true;
        public bool bHotkeyItemSize = true;
        public bool bHotkeyItemDelete = true;
        public bool bHotkeyItemReloadParticle = true;
        public bool bHotkeyItemReset = true;

        public string sHotkeyItemPos = "z";
        public string sHotkeyItemRot = "x";
        public string sHotkeyItemSize = "c";
        public string sHotkeyItemDelete = "d";
        public string sHotkeyItemReloadParticle = "h_ctrl+a";
        public string sHotkeyItemReset = "h_ctrl+r";

        public string fNyodouY = "-0.012";
        public string fChitsuY = "-0.0";
        public string fKetsuY = "0.03";
        public string fToikiY = "-0.03";
        public string fToikiZ = "0.07f";

        //Dance
        public string sHotkeyDanceStart = "h_ctrl+d";
        public string sHotkeyDanceAllOtherStart = "h_ctrl+f";

        public bool bHotkeyDanceStart = true;
        public bool bHotkeyDanceAllOtherStart = true;

        //Camera
        public string sHotkeyCameraQuickSave = "f8";
        public string sHotkeyCameraQuickLoad = "f9";

        //ScreenShot
        public string sHotkeyCameraScreenShot = "s";
        public bool bHotkeyCameraScreenShot = false;
        public bool bHotkeyCameraScreenShotNoUI = true;

        //カメラ移動
        public string sHotkeyCameraMoveLeft = "h_ctrl+left";
        public string sHotkeyCameraMoveRight = "h_ctrl+right";
        public string sHotkeyCameraMoveForward = "h_ctrl+up";
        public string sHotkeyCameraMoveBackward = "h_ctrl+down";
        public string sHotkeyCameraMoveUp = "h_ctrl+home";
        public string sHotkeyCameraMoveDown = "h_ctrl+end";

        public string sHotkeyCameraRotateHorizontalLeft = "h_alt+left";
        public string sHotkeyCameraRotateHorizontalRight = "h_alt+right";
        public string sHotkeyCameraRotateVerticalUp = "h_alt+up";
        public string sHotkeyCameraRotateVerticalDown = "h_alt+down";
        public string sHotkeyCameraRotateLeft = "h_alt+home";
        public string sHotkeyCameraRotateRight = "h_alt+end";
        public string sHotkeyCameraResetToMaid = "h_alt+delete";

        public string sHotkeyCameraDistanceClose = "h_shift+up";
        public string sHotkeyCameraDistanceFar = "h_shift+down";
        public string sHotkeyCameraFieldOfViewWider = "h_shift+home";
        public string sHotkeyCameraFieldOfViewNarrower = "h_shift+end";
        public string sHotkeyCameraMovementFaster = "h_ctrl+=";
        public string sHotkeyCameraMovementSlower = "h_ctrl+-";
        public string sHotkeyCameraMoveMaidToView = "h_alt+insert";

        public bool bHotkeyCameraQuickSave = true;
        public bool bHotkeyCameraQuickLoad = true;
        public bool bHotkeyCameraMovement = true;

        //尻尾
        public string BonesPriority = "Head,Body,FL,FR,RL,RR,XXX,Shippo,Buki";
    }

    public class SettingsXMLDefault
    {
        public readonly string VERSION = EMES.PluginVersion;
        public readonly string ToggleKey = "f7";
#if DEBUG
        public readonly int DebugLogLevel = 2;
#else
        public readonly int DebugLogLevel = 0;
#endif
        public readonly string sHotkeyMaidPos = "z";
        public readonly string sHotkeyMaidRot = "x";
        public readonly string sHotkeyHead = "h_shift+q";
        public readonly string sHotkeyNeck = "h_shift+w";
        public readonly string sHotkeyFingerX = "h_shift+e";
        public readonly string sHotkeyFingerX1 = "h_shift+r";
        public readonly string sHotkeyFingerX2 = "h_shift+t";
        public readonly string sHotkeyArmIK = "h_ctrl+q";
        public readonly string sHotkeyLegIK = "h_ctrl+w";
        public readonly string sHotkeyRClavicle = "h_alt+q";
        public readonly string sHotkeyLClavicle = "h_alt+w";
        public readonly string sHotkeyHide = "space";

        public readonly string sHotkeyItemPos = "z";
        public readonly string sHotkeyItemRot = "x";
        public readonly string sHotkeyItemSize = "c";
        public readonly string sHotkeyItemDelete = "d";
        public readonly string sHotkeyItemReloadParticle = "h_ctrl+a";
        public readonly string sHotkeyItemReset = "h_ctrl+r";

        public readonly string sHotkeyDanceStart = "h_ctrl+d";
        public readonly string sHotkeyDanceAllOtherStart = "h_ctrl+f";

        public readonly string sHotkeyCameraQuickSave = "f8";
        public readonly string sHotkeyCameraQuickLoad = "f9";

        public readonly string sHotkeyCameraMoveLeft = "h_ctrl+left";
        public readonly string sHotkeyCameraMoveRight = "h_ctrl+right";
        public readonly string sHotkeyCameraMoveForward = "h_ctrl+up";
        public readonly string sHotkeyCameraMoveBackward = "h_ctrl+down";
        public readonly string sHotkeyCameraMoveUp = "h_ctrl+home";
        public readonly string sHotkeyCameraMoveDown = "h_ctrl+end";

        public readonly string sHotkeyCameraRotateHorizontalLeft = "h_alt+left";
        public readonly string sHotkeyCameraRotateHorizontalRight = "h_alt+right";
        public readonly string sHotkeyCameraRotateVerticalUp = "h_alt+up";
        public readonly string sHotkeyCameraRotateVerticalDown = "h_alt+down";
        public readonly string sHotkeyCameraRotateLeft = "h_alt+home";
        public readonly string sHotkeyCameraRotateRight = "h_alt+end";
        public readonly string sHotkeyCameraResetToMaid = "h_alt+delete";

        public readonly string sHotkeyCameraDistanceClose = "h_shift+up";
        public readonly string sHotkeyCameraDistanceFar = "h_shift+down";
        public readonly string sHotkeyCameraFieldOfViewWider = "h_shift+home";
        public readonly string sHotkeyCameraFieldOfViewNarrower = "h_shift+end";
        public readonly string sHotkeyCameraMovementFaster = "h_ctrl+=";
        public readonly string sHotkeyCameraMovementSlower = "h_ctrl+-";
        public readonly string sHotkeyCameraMoveMaidToView = "h_alt+insert";
    }        
}



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
    public class EMES_Items
    {
        private EMES Super;
        public EMES_Items(EMES super)
        {
            Super = super;
        }

        #region MyCustomRoomObject
        public class MyCustomRoomObjectData
        {
            public MyCustomRoomObjectData()
            {
#if DEBUG
                Debuginfo.Log("MyCustomRoomObjectData Init ...", 2);
#endif
                categoryIdManager = new CsvCommonIdManager("my_room_placement_obj_category", "EMES.自室カスタム.配置物カテゴリ", CsvCommonIdManager.Type.IdAndUniqueName, (int id) => true);
                basicDatas = new Dictionary<int, MyRoomCustom.PlacementData.Data>();
                enabledIDList = new HashSet<int>();
                Data = new Dictionary<string, Dictionary<int, string>>();
                bInit = false;
            }

            ~MyCustomRoomObjectData()
            {
#if DEBUG
                Debuginfo.Log("MyCustomRoomObjectData Finalize ...", 2);
#endif
                bInit = false;

                enabledIDList.Clear();
                enabledIDList = null;

                basicDatas.Clear();
                basicDatas = null;

                categoryIdManager = null;

                Data.Clear();
                Data = null;
            }

            public int Count
            {
                get
                {
                    if (true == bInit)
                        return enabledIDList.Count;

                    Debuginfo.Error("EMES.自室カスタム.配置物カテゴリ は存在しません");
                    return -1;
                }
            }

            public bool Contains(int id)
            {
                if (true == bInit)
                    return enabledIDList.Contains(id);

                Debuginfo.Error("EMES.自室カスタム.配置物カテゴリ ID[" + id + "]は存在しません");
                return false;
            }

            public MyRoomCustom.PlacementData.Data GetData(int id)
            {
                if (true == bInit)
                {
                    if (true == basicDatas.ContainsKey(id))
                    {
                        return basicDatas[id];
                    }
                }

                Debuginfo.Error("EMES.自室カスタム.配置物カテゴリ ID[" + id + "]のデータは存在しません");
                return null;
            }

            public List<MyRoomCustom.PlacementData.Data> GetAllDatas(bool onlyEnabled)
            {
                if (true == bInit)
                {
                    List<MyRoomCustom.PlacementData.Data> list = new List<MyRoomCustom.PlacementData.Data>();
                    foreach (KeyValuePair<int, MyRoomCustom.PlacementData.Data> keyValuePair in basicDatas)
                    {
                        if (!onlyEnabled || enabledIDList.Contains(keyValuePair.Key))
                        {
                            list.Add(keyValuePair.Value);
                        }
                    }
                    return list;
                }

                Debuginfo.Error("EMES.自室カスタム.配置物カテゴリ のデータは存在しません");
                return null;
            }

            public List<int> CategoryIDList
            {
                get
                {
                    if (true == bInit)
                        return new List<int>(categoryIdManager.idMap.Keys);

                    Debuginfo.Error("EMES.自室カスタム.配置物カテゴリ は存在しません");
                    return null;
                }
            }

            public string GetCategoryName(int categoryID)
            {
                if (true == bInit)
                {
                    if (true == categoryIdManager.idMap.ContainsKey(categoryID))
                    {
                        return categoryIdManager.idMap[categoryID].Key;
                    }
                }

                Debuginfo.Error("カテゴリID[" + categoryID.ToString() + "]の情報が存在しません");
                return "無";
            }

            public int GetCategoryID(string categoryName)
            {
                if (true == bInit)
                {
                    return categoryIdManager.nameMap[categoryName];
                }

                Debuginfo.Error(categoryIdManager.nameMap.ContainsKey(categoryName) + "カテゴリ名[" + categoryName + "]の情報が存在しません");
                return -1;
            }

            public CsvCommonIdManager categoryIdManager;
            public HashSet<int> enabledIDList;
            public Dictionary<int, MyRoomCustom.PlacementData.Data> basicDatas;

            public bool bInit = false;
            public Dictionary<string, Dictionary<int, string>> Data;
        };
        #endregion
        public MyCustomRoomObjectData myCustomRoomObject { get; private set; }


        public class PhotoBGData
        {
            public string id;
            public string icon;
            public string category;
            public string name;
            public string create_prefab_name;
        };
        public List<PhotoBGData> Items_BGDataList { get; private set; }

        public class PhotoBGObjectData_Odogu
        {
            public long id = 0;
            public string category = string.Empty;
            public string name = string.Empty;
            public string create_prefab_name = string.Empty;
            public string create_asset_bundle_name = string.Empty;
            public string direct_file = string.Empty;
            public string assert_bg = string.Empty;
        };

        #region DeskItemData
        public class DeskItemData
        {
            public DeskItemData()
            {

            }

            public DeskItemData(CsvParser csv, int csv_y, Dictionary<int, string> categoryDict)
            {
                int num = 0;
                id = csv.GetCellAsInteger(num++, csv_y);
                name = csv.GetCellAsString(num++, csv_y);
                category_id = csv.GetCellAsInteger(num++, csv_y);
                prefab_name = csv.GetCellAsString(num++, csv_y);
                asset_name = csv.GetCellAsString(num++, csv_y);
                possession_flag = csv.GetCellAsString(num++, csv_y);
                string cellAsString = csv.GetCellAsString(num++, csv_y);
                seasonal_ = !string.IsNullOrEmpty(cellAsString);
                if (seasonal_)
                {
                    string[] array = cellAsString.Split(new char[]
                    {
                    ','
                    });
                    seasonal_month = new int[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        seasonal_month[i] = int.Parse(array[i]);
                    }
                }
                else
                {
                    seasonal_month = new int[0];
                }
                init_visible = (csv.GetCellAsString(num++, csv_y) == "○");
                init_position = csv.GetCellAsVector3(num++, csv_y, ',');
                init_rotation = csv.GetCellAsVector3(num++, csv_y, ',');
                init_scale = csv.GetCellAsVector3(num++, csv_y, ',');
                init_ui_position_scale = csv.GetCellAsReal(num++, csv_y);
                init_ui_rotation_scale = csv.GetCellAsReal(num++, csv_y);
                category = categoryDict[category_id];
            }

            public readonly int id;
            public readonly string name;
            public readonly int category_id;
            public readonly string category;
            public readonly string prefab_name;
            public readonly string asset_name;
            private readonly bool seasonal_;
            public readonly string possession_flag;
            public readonly int[] seasonal_month;
            public readonly bool init_visible;
            public readonly Vector3 init_position;
            public readonly Vector3 init_rotation;
            public readonly Vector3 init_scale;
            public readonly float init_ui_position_scale;
            public readonly float init_ui_rotation_scale;
        }
        #endregion
        public Dictionary<string, Dictionary<DeskItemData, string>> Items_DeskItemData { get; private set; }
        public Dictionary<string, PhotoBGObjectData_Odogu> Items_HandItem { get; private set; }

        private struct AssetBundleObj
        {
            public AssetBundle ab;
            public UnityEngine.Object obj;
        }

        public GameObject goItemMaster{get; private set;}

        public List<HandleEx> Items_selectedHandles { get; private set; }
        public List<HandleEx> Items_Sub_selectedHandles { get; private set; }
        public Dictionary<HandleEx, string> Items_ItemHandle { get; private set; }
        public Dictionary<HandleEx, string> Items_Sub_ItemHandle { get; private set; }
        private class ItemHandlePosRotScale
        {
            public Vector3 position;
            public Quaternion rotation;
            public Quaternion localRotation;
            public Vector3 localScale;
        };
        private ItemHandlePosRotScale itemHandlePosRotScale;

        public int AssertBgStartIndex {get; private set;}
        public bool bAssertBgLoaded = false;

        public PerfabItems Items_perfabItems { get; private set; }


        #region perfab/particle
        public class PerfabItems
        {
            public enum Category
            {
                効果,
                手持品,
                ボディー,
                その他２,
                家具,
                道具,
                文房具,
                グルメ,
                ドリンク,
                その他,
                カジノアイテム,
                プレイアイテム,
                パーティクル,
                小物,
                鏡,
                水,
                舶来背景,
                不明
            };

            public Dictionary<string, Dictionary<string, PhotoBGObjectData_Odogu>> Items = new Dictionary<string, Dictionary<string, PhotoBGObjectData_Odogu>>();

            public readonly Dictionary<string, PhotoBGObjectData_Odogu> Effects = new Dictionary<string, PhotoBGObjectData_Odogu>()
            {
                { "飛行機雲", new PhotoBGObjectData_Odogu{category = "効果", name = "pContrail" } },
                { "クラッカーL", new PhotoBGObjectData_Odogu{category = "効果", name = "pCrackerL" } },
                { "クラッカーS", new PhotoBGObjectData_Odogu{category = "効果", name = "pCrackerS" } },
                { "クラッカーシャワー", new PhotoBGObjectData_Odogu{category = "効果", name = "pCShower" } },
                { "花火", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi" } },
                { "花火　ボール（ブルー）", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi_Botan_Blue" } },
                { "花火　ボール（パープル）", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi_Botan_Purple" } },
                { "花火　ボール（イェロー）", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi_Botan_Yellow" } },
                { "花火　菊（グリーン）", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi_Kiku_Green" } },
                { "花火　菊（オレンジ）", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabi_Kiku_Orange" } },
                { "花火S", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabiS" } },
                { "花火T", new PhotoBGObjectData_Odogu{category = "効果", name = "pHanabiT" } },
                { "ハート", new PhotoBGObjectData_Odogu{category = "効果", name = "pHeart01" } },
                { "ハート１", new PhotoBGObjectData_Odogu{category = "効果", name = "pHeartOne01" } },
                { "ハート２", new PhotoBGObjectData_Odogu{category = "効果", name = "pHeartOne02" } },
                { "ハート３", new PhotoBGObjectData_Odogu{category = "効果", name = "pHeartOne03" } },
                { "線", new PhotoBGObjectData_Odogu{category = "効果", name = "pLine_act2" } },
                { "線１", new PhotoBGObjectData_Odogu{category = "効果", name = "pLineP01" } },
                { "線２", new PhotoBGObjectData_Odogu{category = "効果", name = "pLineP02" } },
                { "線Y", new PhotoBGObjectData_Odogu{category = "効果", name = "pLineY" } },
                { "隕星", new PhotoBGObjectData_Odogu{category = "効果", name = "pMeteor" } },
                { "ピストンEasy", new PhotoBGObjectData_Odogu{category = "効果", name = "pPistonEasy_cm3D2" } },
                { "ピストンHard", new PhotoBGObjectData_Odogu{category = "効果", name = "pPistonHard_cm3D2" } },
                { "ピストンNormal", new PhotoBGObjectData_Odogu{category = "効果", name = "pPistonNormal_cm3D2" } },
                { "吹雪", new PhotoBGObjectData_Odogu{category = "効果", name = "Prame" } },
                { "煙", new PhotoBGObjectData_Odogu{category = "効果", name = "pSmoke" } },
                { "星", new PhotoBGObjectData_Odogu{category = "効果", name = "pstarY_act2" } },
                { "湯気", new PhotoBGObjectData_Odogu{category = "効果", name = "PYuge_LargeBathRoom" } },
                { "リップル", new PhotoBGObjectData_Odogu{category = "効果", name = "Ripple" } },
                { "スモークダンスソファ", new PhotoBGObjectData_Odogu{category = "効果", name = "Smoke_DanceSofa" } },
                { "スモークハンド", new PhotoBGObjectData_Odogu{category = "効果", name = "Smoke_Hand" } },
                { "スモークハンドさくら", new PhotoBGObjectData_Odogu{category = "効果", name = "Smoke_Hand_Sakura" } }
            };

            public readonly Dictionary<string, PhotoBGObjectData_Odogu> Body = new Dictionary<string, PhotoBGObjectData_Odogu>()
            {
                { "尿", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pNyou_cm3D2" } },
                { "尿２", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pNyouE_com3D2" } },
                { "潮", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSio_cm3D2" } },
                { "潮２", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSio2_cm3D2" } },
                { "精液", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSeieki" } },
                { "精液２", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSeieki2" } },
                { "精液（垂れ流し）", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSeieki_tare" } },
                { "精液（なか）", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSeieki_naka" } },
                { "浣腸バースト", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pEnemaBurst_com3D2" } },
                { "浣腸バースト２", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pEnemaBurst02_com3D2" } },
                { "浣腸漏れ", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pEnemaLeak_com3D2" } },
                { "吐息", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pToiki" } },
                { "蒸気", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSteam001_cm3D2" } },
                { "蒸気２", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSteam002_cm3D2" } },
                { "蒸気（黒）", new PhotoBGObjectData_Odogu{category = "ボディー", name = "pSteamBlack" } }
            };

            public readonly Dictionary<string, PhotoBGObjectData_Odogu> EtcItem = new Dictionary<string, PhotoBGObjectData_Odogu>()
            {
                { "PlayAreaOut", new PhotoBGObjectData_Odogu{category = "その他２", name = "PlayAreaOut"} },
                { "魚", new PhotoBGObjectData_Odogu{category = "その他２", name = "Fish" }},
                { "スペバブル", new PhotoBGObjectData_Odogu{category = "その他２", name = "speBubble" }},
                { "湯気鍋", new PhotoBGObjectData_Odogu{category = "その他２", name = "YugeNabe" } },
                { "VVライト" , new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_VVLight_photo_ver"}},
                { "OXカメラ" , new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_OXCamera_photo_ver" }},
                { "レトロカメラ" , new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_HandCameraVV_photo_ver" }},
                { "PC" , new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_PC_photo_ver" }},
                { "モニター" , new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_PC_Monitor_photo_ver" }},
                { "キーボード", new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_PC_Keyboard_photo_ver"}},
                { "マウス", new PhotoBGObjectData_Odogu{category = "その他２", name = "Odogu_PC_Mouse_photo_ver"}},
                { "モブ女　座り１", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Sit001" }},
                { "モブ女　座り２", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Sit002" }},
                { "モブ女　座り３", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Sit003" }},
                { "モブ女　立ち１", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Stand001" }},
                { "モブ女　立ち２", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Stand002" }},
                { "モブ女　立ち３", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Girl_Stand003" }},
                { "モブ男　座り１", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Sit001" }},
                { "モブ男　座り２", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Sit002" }},
                { "モブ男　座り３", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Sit003" }},
                { "モブ男　立ち１", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Stand001" }},
                { "モブ男　立ち２", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Stand002" }},
                { "モブ男　立ち３", new PhotoBGObjectData_Odogu{category = "その他２", name = "Mob_Man_Stand003" }}
            };
        }
        #endregion

        #region public
        public void Items_PreInit()
        {
            Items_InitBackground();
            Items_InitPhotoBGObjectData();
            Items_InitDeskItemData();
            Items_InitMyCustomRoomObjectData();
        }

        public void Items_Init()
        {
#if DEBUG
            Debuginfo.Log("Items_Init goItemMaster", 2);
#endif
            goItemMaster = new GameObject();
            goItemMaster.name = "EMES_Item_Root";
            goItemMaster.transform.position = Vector3.zero;
            goItemMaster.transform.localPosition = Vector3.zero;
            goItemMaster.transform.rotation = Quaternion.identity;
            goItemMaster.transform.localRotation = Quaternion.identity;
            goItemMaster.transform.eulerAngles = Vector3.zero;
            goItemMaster.transform.localEulerAngles = Vector3.zero;

            Items_selectedHandles = new List<HandleEx>();
            Items_selectedHandles.Clear();

            Items_ItemHandle = new Dictionary<HandleEx, string>();
            Items_ItemHandle.Clear();

            Items_Sub_selectedHandles = new List<HandleEx>();
            Items_Sub_selectedHandles.Clear();

            Items_Sub_ItemHandle = new Dictionary<HandleEx, string>();
            Items_Sub_ItemHandle.Clear();

            itemHandlePosRotScale = new ItemHandlePosRotScale();
            itemHandlePosRotScale.position = goItemMaster.transform.position;
            itemHandlePosRotScale.localScale = goItemMaster.transform.localScale;
            itemHandlePosRotScale.rotation = goItemMaster.transform.rotation;
            itemHandlePosRotScale.localRotation = goItemMaster.transform.localRotation;
#if DEBUG
            Debuginfo.Log("Items_Init goItemMaster Done", 2);
#endif
        }

        public void Items_Finalized()
        {
#if DEBUG
            Debuginfo.Log("EMES_Items Finalize ...", 2);
#endif
            foreach (KeyValuePair<HandleEx, string> handle in Items_ItemHandle)
            {
                handle.Key.Visible = false;
                handle.Key.Destroy();
            }

            itemHandlePosRotScale = null;

            Items_selectedHandles.Clear();
            Items_selectedHandles = null;

            Items_ItemHandle.Clear();
            Items_ItemHandle = null;

            Items_Sub_selectedHandles.Clear();
            Items_Sub_selectedHandles = null;

            Items_Sub_ItemHandle.Clear();
            Items_Sub_ItemHandle = null;

            Items_HandItem.Clear();
            Items_HandItem = null;

            goItemMaster = null;

            Items_perfabItems.Items.Clear();
            Items_perfabItems = null;

            Items_BGDataList.Clear();
            Items_BGDataList = null;

            Items_DeskItemData.Clear();
            Items_DeskItemData = null;

            myCustomRoomObject = null;
#if DEBUG
            Debuginfo.Log("EMES_Items Finalized Done", 2);
#endif
        }

        public HandleEx Items_CreatHandle(int iD, string sShowName, bool bShowHandle, Vector3 pos, Quaternion rot, GameObject goParentBone)
        {
            HandleEx handle = null;
            GameObject gameObject = goItemMaster;

            if (null != goParentBone)
            {
                gameObject = goParentBone;
            }

            GameObject goItem = null;
            GameObject newGoItem = null;

            try
            {
                goItem = myCustomRoomObject.basicDatas[iD].GetPrefab();
            }
            catch
            {
                Debuginfo.Warning("myCustomRoomObjectが見つかりません ID=" + iD.ToString(), 0);
                goItem = null;
            }

            if (null != goItem)
            {
#if DEBUG
                Debuginfo.Log("goItem.name" + goItem.name, 2);
#endif
                if (true == Items_AddExistPrefab(goItem, gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), out newGoItem))
                {
                    handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, newGoItem, bShowHandle, true);
                    handle.SetupItem("MyRoomCustomObject_" + iD.ToString(), newGoItem.name, "MyRoomCustomObject");
                    Items_ItemHandle.Add(handle, sShowName);
#if DEBUG
                    Debuginfo.Log("Items_CreatHandle " + handle.GetParentBone().ToString(), 2);
                    Debuginfo.Log("handle.sItemFullName=" + handle.sItemFullName, 2);
                    Debuginfo.Log("handle.sItemName=" + handle.sItemName, 2);
                    Debuginfo.Log("handle.sCategory=" + handle.sCategory, 2);
#endif

                    if (null != goParentBone)
                    {
                        handle.GetParentBone().position = goParentBone.transform.position;
                    }
                    else
                    {
                        handle.GetParentBone().position = pos;
                    }
                    handle.GetParentBone().rotation = rot;

                    return handle;
                }
            }

            return null;
        }

        public HandleEx Items_CreatHandle(string sPrefix, string sName, string sShowName, string sCategory, bool bShowHandle, Vector3 pos, Vector3 rot, PhotoBGObjectData_Odogu odogu, DeskItemData deskItem, GameObject goBone)
        {
            HandleEx handle = null;
            GameObject gameObject = goItemMaster;

            if (null != goBone)
            {
                gameObject = goBone;
            }

            bool ret = false;
            GameObject newGoItem = null;
            string sDetailedInformation = "";
            if (null != odogu)
            {
                ret = Items_AddPrefabOdogu(sName, gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), odogu, out newGoItem, out sDetailedInformation);
            }
            else if (null != deskItem)
            {
                ret = Items_AddPrefabDeskItem(sName, gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), deskItem, out newGoItem, out sDetailedInformation);
            }
            else
            {
                ret = Items_AddPrefab(sPrefix + sName, sName, gameObject, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), out newGoItem);
                sDetailedInformation = sPrefix;
            }

            if (true == ret && null != newGoItem)
            {
                handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, newGoItem, bShowHandle, true);
                handle.SetupItem(sDetailedInformation + sName, newGoItem.name, sCategory);
                Items_ItemHandle.Add(handle, sShowName);
#if DEBUG
                Debuginfo.Log("Items_CreatHandle " + handle.GetParentBone().ToString(), 2);
                Debuginfo.Log("handle.sCategory=" + handle.sCategory, 2);
                Debuginfo.Log("handle.sItemName=" + handle.sItemName, 2);
                Debuginfo.Log("handle.sItemFullName=" + handle.sItemFullName, 2);
                Debuginfo.Log("handle.parentBone=" + handle.parentBone.name, 2);
#endif
                if (null != goBone)
                {
                    handle.GetParentBone().position = goBone.transform.position;
                }
                else
                {
                    handle.GetParentBone().position = pos;
                }
                handle.GetParentBone().rotation = Quaternion.Euler(rot);

                return handle;
            }
            return null;
        }

        public HandleEx Items_CreatHandle(string sFullName, string sName, string sCategory, GameObject gameObject)
        {
            HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, gameObject, false, true);
            handle.SetupItem(sFullName, sName, sCategory);
            Items_ItemHandle.Add(handle, sName);
            return handle;
        }

        public HandleEx Items_CreateExternalImageHandle(string sFullPath, string sFileName, string sShaderName, PrimitiveType primitiveType, bool receiveShadows, 
                                                        ShadowCastingMode shadowCastingMode, bool bPosRot)
        {
#if DEBUG
            Debuginfo.Log("Load Png " + sFullPath, 2);
            Debuginfo.Log("sFileName " + sFileName, 2);
            Debuginfo.Log("sShaderName " + sShaderName, 2);
            Debuginfo.Log("primitiveType " + primitiveType, 2);
            Debuginfo.Log("receiveShadows " + receiveShadows, 2);
            Debuginfo.Log("shadowCastingMode " + shadowCastingMode, 2);
#endif
            if (true == File.Exists(sFullPath))
            {
                byte[] bImage = System.IO.File.ReadAllBytes(sFullPath);
                Texture2D texImage = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                texImage.LoadImage(bImage);

                GameObject emptyGO = GameObject.CreatePrimitive(primitiveType);
                emptyGO.transform.SetParent(goItemMaster.transform, false);

                Mesh mesh = emptyGO.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Count(); ++i)
                {
                    vertices[i].x *= 0.5f;
                    vertices[i].y *= 0.5f;
                    vertices[i].z *= 0.5f;
                }
                mesh.vertices = vertices;

                Shader shader = Shader.Find(sShaderName);
                if(null == shader)
                {
                    Debuginfo.Warning("シェーダーが見つかりません " + sShaderName, 1);
                    shader = Shader.Find("Standard");
                    sShaderName = "Standard";
                }

                Renderer render = emptyGO.GetOrAddComponent<Renderer>();
                render.material = new Material(shader);
                render.material.mainTexture = texImage;
                render.receiveShadows = receiveShadows;
                render.shadowCastingMode = shadowCastingMode;
                render.lightProbeUsage = LightProbeUsage.BlendProbes;
                emptyGO.SetActive(true);

                HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, emptyGO, false, true);
                handle.SetupItem(sFullPath, sFileName, "ExternalImage");

                if (true == bPosRot)
                {
                    if (PrimitiveType.Plane == primitiveType)
                    {
                        handle.GetParentBone().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        handle.GetParentBone().position = new Vector3(Super.Window.CurrentSelectedMaid.body0.transform.position.x, Super.Window.CurrentSelectedMaid.body0.transform.position.y, Super.Window.CurrentSelectedMaid.body0.transform.position.z + 0.5f);
                    }
                    else if (PrimitiveType.Cube == primitiveType)
                    {
                        handle.GetParentBone().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        handle.GetParentBone().position = new Vector3(Super.Window.CurrentSelectedMaid.body0.transform.position.x, Super.Window.CurrentSelectedMaid.body0.transform.position.y + 1f, Super.Window.CurrentSelectedMaid.body0.transform.position.z + 1f);
                    }
                    else
                    {
                        handle.GetParentBone().rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                        handle.GetParentBone().position = new Vector3(Super.Window.CurrentSelectedMaid.body0.transform.position.x, Super.Window.CurrentSelectedMaid.body0.transform.position.y + 1f, Super.Window.CurrentSelectedMaid.body0.transform.position.z + 1f);
                    }
                }
                else
                {
                    handle.GetParentBone().rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    handle.GetParentBone().position = new Vector3(0, 0, 0);
                }

                handle.SetOptions(((int)primitiveType).ToString() + "|" + (render.receiveShadows ? "1" : "0") + "|" + 
                                  ((int)render.shadowCastingMode).ToString() + "|" + ((int)render.reflectionProbeUsage).ToString() + "|" + 
                                  sShaderName);

                Super.Items.Items_ItemHandle.Add(handle, sFileName);                
#if DEBUG
                Debuginfo.Log("Png Loaded", 2);
#endif
                return handle;
            }

#if DEBUG
            Debuginfo.Warning("Png Failed", 2);
#endif
            return null;
        }

        public GameObject Items_CreatShadow(string sPrefix, string sName, Vector3 pos, Vector3 rot, PhotoBGObjectData_Odogu odogu, DeskItemData deskItem)
        {
            GameObject newGoItem;
            string sDetailedInformation = "";
            if (null != odogu)
            {
                Items_AddPrefabOdogu(sName, goItemMaster, pos, rot, odogu, out newGoItem, out sDetailedInformation);
            }
            else if (null != deskItem)
            {
                Items_AddPrefabDeskItem(sName, goItemMaster, pos, rot, deskItem, out newGoItem, out sDetailedInformation);
            }
            else
            {
                Items_AddPrefab(sPrefix + sName, sName, goItemMaster, pos, rot, out newGoItem);
            }
#if DEBUG
            Debuginfo.Log("sDetailedInformation = [" + sDetailedInformation + "]", 2);
#endif
            return newGoItem;
        }

        public HandleEx Items_CreateDirectObjectHandle(string sItemName, string sItemFullName, string sCategory, string sShowName, PhotoBGObjectData_Odogu odogu, DeskItemData deskItem)
        {
            HandleEx handle = null;
            GameObject goItem = null;

            string sDetailedInformation = "";
            if (null != odogu)
            {
                Items_AddPrefabOdogu(sItemName, goItemMaster, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), odogu, out goItem, out sDetailedInformation);
            }
            else if (null != deskItem)
            {
                Items_AddPrefabDeskItem(sItemName, goItemMaster, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), deskItem, out goItem, out sDetailedInformation);
            }
            else
            {
                Items_AddPrefab(sItemFullName, sItemName, goItemMaster, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), out goItem);
                /*
                UnityEngine.Object @object = Resources.Load(sItemFullName);
                if (@object != null)
                {
                    goItem = UnityEngine.Object.Instantiate(@object) as GameObject;
                    Transform transform = goItemMaster.transform;
                    goItem.AddComponent<AttachPrefab>();
                    goItem.name = goItem.name.Replace("(Clone)", string.Empty);
                    if (!string.IsNullOrEmpty(sItemName))
                    {
                        goItem.name = sItemName;
                    }
                    AttachPrefab[] componentsInChildren = goItemMaster.transform.GetComponentsInChildren<AttachPrefab>(true);
                    AttachPrefab[] array = (from a in componentsInChildren
                                            where a.name == goItem.name
                                            select a).ToArray<AttachPrefab>();
                    for (int i = 0; i < array.Length; i++)
                    {
                        UnityEngine.Object.DestroyImmediate(array[i].gameObject);
                    }
                    Random rnd = new Random(DateTime.Now.Millisecond);
                    goItem.name += ("_" + DateTime.UtcNow.ToString() + "_" + rnd.Next(10000));
                    goItem.transform.SetParent(transform, false);
                }
                else
                {
                    Debug.LogError("人物にプレハブ追加で、プレハブが見つかりません。" + sItemFullName);
                }
                //*/
            }

            if (null != goItem)
            {
                handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, goItem, false, true);
                handle.SetupItem(sItemFullName, sItemName, "Direct_" + sCategory);
                Super.Items.Items_ItemHandle.Add(handle, sShowName);
            }
            return handle;
        }

        public void Items_RemoveHandle(HandleEx hHandle)
        {
            hHandle.Visible = false;

            Action<HandleEx> actionDestoryHandle = delegate (HandleEx handle)
            {
                Items_ItemHandle.Remove(handle);
                handle.Destroy();
            };

            if (true == hHandle.sCategory.Equals("WildHandle"))
            {
                if (true == hHandle.sItemFullName.Contains("SubLightPoint_"))
                {
                    Super.camPlus.RemoveSubLight(Super.MaidIK.IK_RemoveSubLightPoint(hHandle));
                    Super.Window.Refresh_SubLight_list();
                }
                else if ("GazePoint" == hHandle.sItemFullName)
                {
                    Super.MaidIK.IK_RemoveGazePoint(GameMain.Instance.CharacterMgr.GetMaid(hHandle.sItemName), false);
                }               
            }
            else if (true == hHandle.sCategory.Equals("MyRoomCustomObject") || true == hHandle.sCategory.Equals("ExternalImage") || true == hHandle.sCategory.Equals("RTMIHandle")
                || true == hHandle.sCategory.Contains("Direct_") )
            {
                UnityEngine.Object.DestroyImmediate(hHandle.parentBone);
                actionDestoryHandle(hHandle);
            }
            else if (true == hHandle.sCategory.Equals("SubItemHandle") || true == hHandle.sCategory.Equals("RoomAsItem"))
            {
                Items_Sub_ItemHandle.Remove(hHandle);
                actionDestoryHandle(hHandle);
            }
            else if (true == hHandle.sCategory.Equals("MaidPartsHandle"))
            {
                actionDestoryHandle(hHandle);
            }
            else
            {
                Items_DelPrefab(hHandle.sItemName, hHandle.parentBone);
                actionDestoryHandle(hHandle);
            }            
        }

        public void Items_RemoveCategory(string sCategory)
        {
            if (0 == GetItemHandleCount())
                return;

            if (true == string.Equals("WildHandle", sCategory))
            {
#if DEBUG
                Debuginfo.Log("これは注視点ハンドルカテゴリハンドルです、代わりにIK_RemoveGazePoint", 2);
#endif
                return;
            }

            foreach (KeyValuePair<HandleEx, string> handle in Items_ItemHandle.ToList())
            {
                if(sCategory == handle.Key.sCategory)
                    Items_RemoveHandle(handle.Key);
            }
        }

        public void Items_RemoveAll()
        {
            if (0 == GetItemHandleCount())
                return;

            foreach(KeyValuePair<HandleEx, string> handle in Items_ItemHandle.ToList())
            {
                if (true == string.Equals("WildHandle", handle.Key.sCategory))
                {
#if DEBUG
                    Debuginfo.Log("ワイルドハンドルを無視する："+ handle.Key.sItemName, 2);
#endif
                    continue;
                }

                Items_RemoveHandle(handle.Key);
            }
        }

        public int GetItemHandleCount()
        {
            return Items_ItemHandle.Count;
        }

        public HandleEx Items_Sub_CreateHandle(string sFullName, string sName, string sCategory, GameObject gameObject)
        {
#if DEBUG
            Debuginfo.Log("Items_Sub_CreateHandle() sName = " + sName, 2);
#endif
            HandleEx handle = new HandleEx(EMES_MaidIK.BoneType.Root, EMES_MaidIK.BoneSetType.Root, gameObject, false, true);
            handle.SetupItem(sFullName, sName, sCategory);
            Items_Sub_ItemHandle.Add(handle, sName);
            return handle;
        }

        public void Items_Sub_RemoveAll()
        {
            if (0 == GetItemHandleCount())
                return;

            foreach (KeyValuePair<HandleEx, string> handle in Items_Sub_ItemHandle.ToList())
            {
                Items_RemoveHandle(handle.Key);
            }

            Items_Sub_ItemHandle.Clear();
        }

        public void Items_UpdateSelectedHandleExList(System.Windows.Forms.ListBox.SelectedObjectCollection selectedHandles)
        {
            Items_selectedHandles.Clear();
            foreach (KeyValuePair<HandleEx, string> handle in selectedHandles)
            {
                Items_selectedHandles.Add(handle.Key);
                handle.Key.Visible = false;
            }
        }

        public void Items_Sub_UpdateSelectedHandleExList(System.Windows.Forms.ListBox.SelectedObjectCollection selectedHandles)
        {
            if (null == selectedHandles)
            {
#if DEBUG
                Debuginfo.Log("Items_Sub_UpdateSelectedHandleExList() selectedHandles == NULL" + DateTime.Now, 0);
#endif
                return;
            }

            Items_Sub_selectedHandles.Clear();
            foreach (KeyValuePair<HandleEx, string> handle in selectedHandles)
            {
                Items_Sub_selectedHandles.Add(handle.Key);
                handle.Key.Visible = false;
            }
        }

        public void Items_CopyHandlePosRotScale(GameObject gameObject)
        {
            itemHandlePosRotScale.position = gameObject.transform.position;
            itemHandlePosRotScale.rotation = gameObject.transform.rotation;
            itemHandlePosRotScale.localRotation = gameObject.transform.localRotation;
            itemHandlePosRotScale.localScale = gameObject.transform.localScale;
        }

        public void Items_PasteHandlePosRotScale(GameObject gameObject, bool bPos, bool bRot, bool bScale)
        {
            if (true == bPos)
                gameObject.transform.position = itemHandlePosRotScale.position;

            if (true == bRot)
            {
                gameObject.transform.rotation = itemHandlePosRotScale.rotation;
                gameObject.transform.localRotation = itemHandlePosRotScale.localRotation;
            }

            if (true == bScale)
                gameObject.transform.localScale = itemHandlePosRotScale.localScale;
        }

        public void Items_RenewPrefab(HandleEx handle)
        {
            handle.parentBone.SetActive(false);
#if DEBUG
            Debug.Log("プレハブ更新 " + handle.parentBone.name);
#endif
            handle.parentBone.SetActive(true);
        }
        #endregion

        #region private
        private void Items_InitBackground()
        {
#if DEBUG
            Debuginfo.Log("InitBackground()", 2);
#endif
            Items_BGDataList = new List<PhotoBGData>();
            AssertBgStartIndex = 0;

            HashSet<int> hashSet = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Normal, true, "phot_bg_enabled_list", ref hashSet);
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("phot_bg_list.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("phot_bg_list.nei open failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (csvParser.IsCellToExistData(0, i) && hashSet.Contains(csvParser.GetCellAsInteger(0, i)))
                            {
                                int num = 0;

                                PhotoBGData bg = new PhotoBGData()
                                {
                                    id = csvParser.GetCellAsInteger(num++, i).ToString(),
                                    category = csvParser.GetCellAsString(num++, i),
                                    name = csvParser.GetCellAsString(num++, i),
                                    create_prefab_name = csvParser.GetCellAsString(num++, i)
                                };
#if DEBUG
                                Debuginfo.Log("id=" + bg.id + " category=" + bg.category + " name=" + bg.name + " create_prefab_name:" + bg.create_prefab_name, 2);
#endif
                                Items_BGDataList.Add(bg);
                                AssertBgStartIndex++;
                            }
                        }
                    }
                }
            }


            Dictionary<string, string> saveDataDic = MyRoomCustom.CreativeRoomManager.GetSaveDataDic();
            if (saveDataDic != null)
            {
                foreach (KeyValuePair<string, string> keyValuePair in saveDataDic)
                {
                    PhotoBGData bg = new PhotoBGData()
                    {
                        id = keyValuePair.Key,
                        category = "マイルーム",
                        name = keyValuePair.Value,
                        create_prefab_name = keyValuePair.Value
                    };
#if DEBUG
                    Debuginfo.Log("guid=" + bg.id + " category=" + bg.category + " name=" + bg.name, 2);
#endif
                    Items_BGDataList.Add(bg);
                    AssertBgStartIndex++;
                }
            }

            HashSet<int> hashSet2 = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Normal, true, "edit_bg_enabled_list", ref hashSet2);
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("edit_bg.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("edit_bg.nei open failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (csvParser.IsCellToExistData(0, i) && hashSet2.Contains(csvParser.GetCellAsInteger(0, i)))
                            {
                                int num = 0;

                                PhotoBGData bg = new PhotoBGData()
                                {
                                    id = csvParser.GetCellAsInteger(num++, i).ToString(),
                                    icon = Path.ChangeExtension(csvParser.GetCellAsString(num++, i).ToString(), "tex"),
                                    name = csvParser.GetCellAsString(num, i),
                                    create_prefab_name = csvParser.GetCellAsString(num, i)
                                };
                                bg.category = "編";  //編集モード
#if DEBUG
                                Debuginfo.Log("id=" + bg.id + " category=" + bg.category + " name=" + bg.name + " create_prefab_name:" + bg.create_prefab_name, 2);
#endif
                                Items_BGDataList.Add(bg);
                                AssertBgStartIndex++;
                            }
                        }
                    }
                }
            }

            Action<string> action = delegate (string sPath)
            {
#if DEBUG
                Debuginfo.Log(">> Directory.GetCurrentDirectory() + sPath=" + Directory.GetCurrentDirectory() + sPath, 2);
#endif
                if (true == Directory.Exists(Directory.GetCurrentDirectory() + sPath))
                {
                    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + sPath);
                    int Index = 9000;
                    foreach (string file in files)
                    {
                        PhotoBGData bg = new PhotoBGData()
                        {
                            id = Index.ToString(),
                            category = "舶来背景",
                            name = file.Replace(".asset_bg", "").Replace(Directory.GetCurrentDirectory() + sPath, ""),
                            create_prefab_name = file.Replace(".asset_bg", "")
                        };
                        if (false == file.ToLower().Contains("_shader") && false == file.ToLower().Contains("_hit"))
                        {
#if DEBUG
                            Debuginfo.Log("id=" + bg.id + " category=" + bg.category + " name=" + bg.name + " create_prefab_name:" + bg.create_prefab_name, 2);
#endif
                            Items_BGDataList.Add(bg);
                            Index++;
                        }
                    }
                }
            };
            action(Super.settingsXml.BackgroundsDirectory);
        }

        private void Items_InitPhotoBGObjectData()
        {
#if DEBUG
            Debuginfo.Log("Items_InitPhotoBGObjectData()", 2);
#endif
            Items_perfabItems = new PerfabItems();
            Items_perfabItems.Items.Add("効果", Items_perfabItems.Effects);
            Items_ReadHandItems();
            Items_perfabItems.Items.Add("手持品", Items_HandItem);
            Items_perfabItems.Items.Add("ボディー", Items_perfabItems.Body);
            Items_perfabItems.Items.Add("その他２", Items_perfabItems.EtcItem);
            foreach (string cat in Enum.GetNames(typeof(PerfabItems.Category)))
            {
                if (false == Items_perfabItems.Items.ContainsKey(cat))
                {
                    Items_perfabItems.Items.Add(cat, new Dictionary<string, PhotoBGObjectData_Odogu>());
                }
            }

            HashSet<int> hashSet = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Normal, true, "phot_bg_object_enabled_list", ref hashSet);
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("phot_bg_object_list.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("phot_maid_item_list.nei open failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (csvParser.IsCellToExistData(0, i) && hashSet.Contains(csvParser.GetCellAsInteger(0, i)))
                            {
                                int num = 0;
                                PhotoBGObjectData_Odogu photoBGObjectData = new PhotoBGObjectData_Odogu()
                                {
                                    id = csvParser.GetCellAsInteger(num++, i),
                                    category = csvParser.GetCellAsString(num++, i),
                                    name = csvParser.GetCellAsString(num++, i),
                                    create_prefab_name = csvParser.GetCellAsString(num++, i),
                                    create_asset_bundle_name = csvParser.GetCellAsString(num++, i),
                                    direct_file = string.Empty,
                                    assert_bg = string.Empty
                                };
                                string cellAsString = csvParser.GetCellAsString(num++, i);
                                if (string.IsNullOrEmpty(cellAsString) || PluginData.IsEnabled(cellAsString))
                                {
                                    if (true == Items_perfabItems.Items.ContainsKey(photoBGObjectData.category))
                                    {
#if DEBUG
                                        Debuginfo.Log(">> name=" + photoBGObjectData.name + " category=" + photoBGObjectData.category + " create_prefab_name=" + photoBGObjectData.create_prefab_name + " create_asset_bundle_name=" + photoBGObjectData.create_asset_bundle_name, 2);
#endif
                                        Items_perfabItems.Items[photoBGObjectData.category].Add(photoBGObjectData.name, photoBGObjectData);
                                    }
                                    else
                                    {
#if DEBUG
                                        Debuginfo.Warning("不明>> name=" + photoBGObjectData.name + " category=" + photoBGObjectData.category + " create_prefab_name=" + photoBGObjectData.create_prefab_name + " create_asset_bundle_name=" + photoBGObjectData.create_asset_bundle_name, 2);
#endif
                                        Items_perfabItems.Items["不明"].Add(photoBGObjectData.name, photoBGObjectData);
                                    }
                                }
#if DEBUG
                                else
                                {
                                    Debuginfo.Warning("NE>> name=" + photoBGObjectData.name + " category=" + photoBGObjectData.category + " create_prefab_name=" + photoBGObjectData.create_prefab_name + " create_asset_bundle_name=" + photoBGObjectData.create_asset_bundle_name, 2);
                                }
#endif
                            }
                        }
                    }
                }
            }

            Action<string, string> action = delegate (string sCategory, string sPath)
            {
#if DEBUG
                Debuginfo.Log(">> Directory.GetCurrentDirectory() + sPath=" + Directory.GetCurrentDirectory() + sPath, 2);
#endif
                if (true == Directory.Exists(Directory.GetCurrentDirectory() + sPath))
                {
                    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + sPath);
                    foreach (string file in files)
                    {
                        PhotoBGObjectData_Odogu photoBGObjectData = new PhotoBGObjectData_Odogu()
                        {
                            id = 0,
                            category = sCategory,
                            name = file.Replace(".asset_bg", "").Replace(Directory.GetCurrentDirectory() + sPath, ""),
                            create_prefab_name = string.Empty,
                            create_asset_bundle_name = string.Empty,
                            direct_file = string.Empty,
                            assert_bg = file.Replace(".asset_bg", "")
                        };
                        if (false == file.ToLower().Contains("_shader") && false == file.ToLower().Contains("_hit"))
                        {
#if DEBUG
                            Debuginfo.Log(">> name=" + photoBGObjectData.name + " category=" + photoBGObjectData.category + " create_prefab_name=" + photoBGObjectData.create_prefab_name + " assert_bg=" + photoBGObjectData.assert_bg, 2);
#endif
                            Items_perfabItems.Items[sCategory].Add(photoBGObjectData.name, photoBGObjectData);
                        }
                    }
                }

                if (0 == Items_perfabItems.Items[sCategory].Count)
                {
                    Items_perfabItems.Items[sCategory].Add("無", new PhotoBGObjectData_Odogu { category = sCategory, name = "無" });
                }
            };
            action("鏡", Super.settingsXml.MirrorDirectory);
            action("水", Super.settingsXml.WaterbedsDirectory);
            action("小物", Super.settingsXml.OthersDirectory);
            action("舶来背景", Super.settingsXml.BackgroundsDirectory);

            Action<string> action2 = delegate (string sCategory)
            {
                foreach (KeyValuePair<string, AFileSystemBase> kvpFile in GameUty.BgFiles.ToList())
                {
                    PhotoBGObjectData_Odogu photoBGObjectData = new PhotoBGObjectData_Odogu()
                    {
                        id = 0,
                        category = sCategory,
                        name = kvpFile.Key.Replace(".asset_bg", ""),
                        create_prefab_name = string.Empty,
                        create_asset_bundle_name = string.Empty,
                        direct_file = string.Empty,
                        assert_bg = kvpFile.Key.Replace(".asset_bg", "")
                    };

                    if (false == Items_perfabItems.Items[sCategory].ContainsKey(photoBGObjectData.name))
                    {
                        if (false == photoBGObjectData.name.ToLower().Contains("_shader") && false == photoBGObjectData.name.ToLower().Contains("_hit")
                         && false == photoBGObjectData.name.ToLower().Contains("mrc_")
                         && false == Items_perfabItems.Items["舶来背景"].ContainsKey(photoBGObjectData.name.ToLower())
                         && false == Items_perfabItems.Items["小物"].ContainsKey(photoBGObjectData.name.ToLower())
                        )
                        {
                            bool bContinue = false;
                            foreach (PhotoBGData photoBGData in Items_BGDataList.ToArray())
                            {
                                if (photoBGObjectData.assert_bg == photoBGData.create_prefab_name)
                                {
                                    bContinue = true;
                                    break;
                                }
                            }
                            if (true == bContinue)
                                continue;
#if DEBUG
                            Debuginfo.Log(">> category=" + photoBGObjectData.category + " assert_bg=" + photoBGObjectData.assert_bg, 2);
#endif
                            Items_perfabItems.Items[sCategory].Add(photoBGObjectData.name, photoBGObjectData);
                        }
                    }
                }
            };
            action2("不明");

            if (0 == Items_perfabItems.Items["不明"].Count)
            {
                Items_perfabItems.Items["不明"].Add("無", new PhotoBGObjectData_Odogu { category = "不明", name = "無" });
            }
#if DEBUG
            Debuginfo.Log("Items_Init Loading PhotoBGObjectData_Background Done", 2);
#endif
        }

        private void Items_InitDeskItemData()
        {
#if DEBUG
            Debuginfo.Log("Items_InitDeskItemData()", 2);
#endif
            Items_DeskItemData = new Dictionary<string, Dictionary<DeskItemData, string>>();

            Dictionary<int, string> item_category_data_dic_ = new Dictionary<int, string>();
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("desk_item_category.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("desk_item_category.nei\nopen failed.");
                    }
                    else
                    {
                        for (int i = 1; i < csvParser.max_cell_y; i++)
                        {
                            if (!csvParser.IsCellToExistData(0, i))
                            {
                                break;
                            }
                            int cellAsInteger = csvParser.GetCellAsInteger(0, i);
                            string cellAsString = csvParser.GetCellAsString(1, i);
                            if (false == item_category_data_dic_.ContainsKey(cellAsInteger))
                            {
                                item_category_data_dic_.Add(cellAsInteger, cellAsString);
                                Items_DeskItemData.Add(cellAsString, new Dictionary<DeskItemData, string>());
#if DEBUG
                                Debuginfo.Log("Load Desk category [" + cellAsInteger.ToString() + "] [" + cellAsString + "]", 2);
#endif
                            }
                        }
                    }
                }
            }
            HashSet<int> hashSet = new HashSet<int>();
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Normal, true, "desk_item_enabled_id", ref hashSet);
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Old, true, "desk_item_enabled_id", ref hashSet);
            using (AFileBase afileBase2 = GameUty.FileSystem.FileOpen("desk_item_detail.nei"))
            {
                using (CsvParser csvParser2 = new CsvParser())
                {
                    bool condition2 = csvParser2.Open(afileBase2);
                    if (false == condition2)
                    {
                        Debuginfo.Error("desk_item_detail.nei open failed.");
                    }
                    else
                    {
                        for (int j = 1; j < csvParser2.max_cell_y; j++)
                        {
                            if (csvParser2.IsCellToExistData(0, j))
                            {
                                int cellAsInteger2 = csvParser2.GetCellAsInteger(0, j);

                                if (hashSet.Contains(cellAsInteger2))
                                {
                                    DeskItemData itemData = new DeskItemData(csvParser2, j, item_category_data_dic_);
                                    Items_DeskItemData[itemData.category].Add(itemData, itemData.name);
#if DEBUG
                                    Debuginfo.Log("Load Desk [" + itemData.category + "] [" + itemData.name + "] assertname=" + itemData.asset_name + " prefabname=" + itemData.prefab_name, 2);
#endif
                                }
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, Dictionary<DeskItemData, string>> kvp in Items_DeskItemData.ToList())
            {
                if (0 == kvp.Value.Count)
                {
                    Items_DeskItemData[kvp.Key].Add(new DeskItemData(), "無");
                }
            }
#if DEBUG
            Debuginfo.Log("Items_InitDeskItemData() done!", 2);
#endif
        }

        private void Items_InitMyCustomRoomObjectData()
        {
#if DEBUG
            Debuginfo.Log("Items_InitMyCustomRoomObjectData()", 2);
#endif
            myCustomRoomObject = new MyCustomRoomObjectData();
#if DEBUG
            Debuginfo.Log("Read CSV Start!", 2);
#endif
            CsvCommonIdManager.ReadEnabledIdList(CsvCommonIdManager.FileSystemType.Normal, true, "my_room_placement_obj_enabled_list", ref myCustomRoomObject.enabledIDList);
            using (AFileBase afileBase = GameUty.FileSystem.FileOpen("my_room_placement_obj_list.nei"))
            {
                using (CsvParser csvParser = new CsvParser())
                {
                    bool condition = csvParser.Open(afileBase);
                    if (false == condition)
                    {
                        Debuginfo.Error("my_room_placement_obj_list open failed.");
                    }
                    else
                    {
                        foreach (int num in myCustomRoomObject.enabledIDList)
                        {
                            MyRoomCustom.PlacementData.Data value = new MyRoomCustom.PlacementData.Data(num, csvParser);
                            myCustomRoomObject.basicDatas.Add(num, value);
                        }
                        myCustomRoomObject.bInit = true;
                    }
                }
            }
#if DEBUG
            Debuginfo.Log("Read CSV Done!", 2);
#endif
            if (true == myCustomRoomObject.bInit)
            {
                List<int> cID = myCustomRoomObject.CategoryIDList;
                Dictionary<int, string> cString = new Dictionary<int, string>();

                foreach (int id in cID)
                {
                    myCustomRoomObject.Data.Add(myCustomRoomObject.GetCategoryName(id), new Dictionary<int, string>());
                    cString.Add(id, myCustomRoomObject.GetCategoryName(id));
                }

                foreach (KeyValuePair<int, MyRoomCustom.PlacementData.Data> item in myCustomRoomObject.basicDatas)
                {
#if DEBUG
                    Debuginfo.Log("Add " + item.Key.ToString() + " to [" + cString[item.Value.categoryID] + "] [" + item.Value.drawName + "]", 2);
#endif
                    myCustomRoomObject.Data[cString[item.Value.categoryID]].Add(item.Key, item.Value.drawName);
                }
                //セーフチェック
                foreach(string sCategory in myCustomRoomObject.Data.Keys.ToList())
                {
                    if(0 == myCustomRoomObject.Data[sCategory].Count)
                    {
                        myCustomRoomObject.Data[sCategory].Add(-1, "無");
                    }
                }
            }
            else
            {
                myCustomRoomObject.Data.Add("無", new Dictionary<int, string>());
                myCustomRoomObject.Data["無"].Add(-1, "無");
            }

#if DEBUG
            Debuginfo.Log("Items_InitMyCustomRoomObjectData() Done", 2);
#endif
        }

        private void Items_ReadHandItems()
        {
#if DEBUG
            Debuginfo.Log("Items_ReadHandItems()", 2);
#endif
            string[] list = GameUty.FileSystem.GetList(@"model\handitem\", AFileSystemBase.ListType.TopFile);
            Items_HandItem = new Dictionary<string, PhotoBGObjectData_Odogu>();

            foreach (string s in list)
            {
                UnityEngine.Object @object = Resources.Load(s.Replace(".model", ""));
                if (@object != null)
                {
#if DEBUG
                    Debuginfo.Log("Add Item >> " + s, 2);
#endif
                    Items_HandItem.Add(s.Replace(".model", "").Replace("model\\handitem\\handitem", ""), new PhotoBGObjectData_Odogu { category = "手持品", name = s.Replace(".model", "").Replace("model\\handitem\\", "") });

                    GameObject goItem = UnityEngine.Object.Instantiate(@object) as GameObject;
                    UnityEngine.Object.DestroyImmediate(goItem);
                }
                else
                {
#if DEBUG
                    Debuginfo.Log("Item << " + s, 2);
#endif
                }
            }

#if DEBUG
            Debuginfo.Log("Items_ReadHandItems() Done", 2);
#endif
        }

        public void Items_SyncPosRotFromHandle()
        {
            if (0 == GetItemHandleCount())
                return;

            if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyItemReset) && true == Super.settingsXml.bHotkeyItemReset)
            {
                Items_RemoveAll();
                return;
            }

            #region Actions
            Action<HandleEx> aRenewPrefab = delegate (HandleEx posHandle)
            {
                if (true == FlexKeycode.GetMultipleKeyUp(Super.settingsXml.sHotkeyItemReloadParticle) && ("効果" == posHandle.sCategory || "ボディー" == posHandle.sCategory) && true == Super.settingsXml.bHotkeyItemReloadParticle)
                {
                    Items_RenewPrefab(posHandle);
                }
            };

            Action<HandleEx, bool> aProcessHandle_Invisible = delegate (HandleEx posHandle, bool bNoDelete)
            {
                if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && true == Super.settingsXml.bHotkeyItemPos)
                {
                    if (false == posHandle.IK_GetHandleKunPosotionMode())
                        posHandle.IK_ChangeHandleKunModePosition(true);
                    posHandle.Visible = true;
                }
                else if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemRot) && true == Super.settingsXml.bHotkeyItemRot)
                {
                    if (true == posHandle.IK_GetHandleKunPosotionMode())
                        posHandle.IK_ChangeHandleKunModePosition(false);
                    posHandle.Visible = true;
                }
                else if ((true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemDelete) && false == bNoDelete && true == Super.settingsXml.bHotkeyItemDelete)
                      || (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize) && true == Super.settingsXml.bHotkeyItemSize))
                {
                    if (false == posHandle.IK_GetHandleKunIKMode())
                        posHandle.IK_ChangeHandleKunModeIK(true);
                    posHandle.Visible = true;
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemDelete) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize))
                {
                    if (true == posHandle.IK_GetHandleKunIKMode())
                    {
                        if (true == Super.settingsXml.bHotkeyItemDelete || true == Super.settingsXml.bHotkeyItemSize)
                            posHandle.IK_ChangeHandleKunModePosition(true);
                    }
                }
            };

            Action<HandleEx> aProcessHandle_Visible = delegate (HandleEx posHandle)
            {
                if (true == posHandle.ControllDragged())
                {
                    Vector3 deltaVector = posHandle.DeltaVector();
                    Quaternion deltaQuaternion = posHandle.DeltaQuaternion();

                    if (false == posHandle.IK_GetHandleKunPosotionMode())
                    {
                        //ボーンを回転させておく
                        posHandle.GetParentBone().rotation *= deltaQuaternion;
                    }
                    else
                    {
                        posHandle.GetParentBone().position += deltaVector;
                    }

                    if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize) && true == Super.settingsXml.bHotkeyItemSize)
                    {
                        /*
                        XYZ 0
                        X   1
                        Y   2
                        Z   3
                        XY  4
                        XZ  5
                        YZ  6
                         */
                        if (0 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(deltaVector.y, deltaVector.y, deltaVector.y);
                        }
                        else if (1 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(deltaVector.y, 0, 0);
                        }
                        else if (2 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(0, deltaVector.y, 0);
                        }
                        else if (3 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(0, 0, deltaVector.y);
                        }
                        else if (4 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(deltaVector.y, deltaVector.y, 0);
                        }
                        else if (5 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(deltaVector.y, 0, deltaVector.y);
                        }
                        else if (6 == Super.Window.GetItemsHandleScaleFactor())
                        {
                            posHandle.GetParentBone().localScale += new Vector3(0, deltaVector.y, deltaVector.y);
                        }

                        if (true == Super.Window.Items_isItemHandleMethodAllorSingle())
                        {
                            Super.Window.Items_UpdateItemHandleScaleInfo(posHandle.parentBone);
                        }
                    }
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemRot)
                    && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemDelete) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize))
                {
                    if (true == Super.settingsXml.bHotkeyItemPos || true == Super.settingsXml.bHotkeyItemRot || true == Super.settingsXml.bHotkeyItemDelete || true == Super.settingsXml.bHotkeyItemSize)
                        posHandle.Visible = false;
                }
            };

            Action<HandleEx> aProcessHandle_Invisible_Gravity = delegate (HandleEx posHandle)
            {
                if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && true == Super.settingsXml.bHotkeyItemPos)
                {
                    if (false == posHandle.IK_GetHandleKunIKMode())
                        posHandle.IK_ChangeHandleKunModeIK(true);
                    posHandle.Visible = true;
                }
            };

            Action<HandleEx> aProcessHandle_Visible_Gravity = delegate (HandleEx posHandle)
            {
                if (true == posHandle.ControllDragged())
                {
                    DynamicBone component2 = posHandle.parentBone.GetComponent<DynamicBone>();
                    if (component2 != null && component2.enabled)
                    {
                        Vector3 softG = component2.m_Gravity;
                        softG += (posHandle.DeltaVector() * 0.1f);
                        component2.m_Gravity = new Vector3(softG.x, softG.y, softG.z);
                    }
                    if (true == Super.Window.Items_isItemHandleMethodAllorSingle())
                    {
                        Super.Window.Items_UpdateItemHandleScaleInfo(posHandle.parentBone);
                    }
                }
                else if (false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemPos) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemRot)
                    && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemDelete) && false == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemSize))
                {
                    if (true == Super.settingsXml.bHotkeyItemPos || true == Super.settingsXml.bHotkeyItemRot || true == Super.settingsXml.bHotkeyItemDelete || true == Super.settingsXml.bHotkeyItemSize)
                        posHandle.Visible = false;
                }
            };
            #endregion

            List<HandleEx> ItemHandleList = new List<HandleEx>();
            if (true == Super.Window.Items_isItemHandleMethodAll())
            {
                ItemHandleList = Items_ItemHandle.Select(kvp => kvp.Key).ToList();
            }
            else
            {
                ItemHandleList = Items_selectedHandles.ToList();
            }

            if (true == Super.Window.isItems_list_tabPage1())
            {
                foreach (HandleEx posHandle in ItemHandleList)
                {
                    if(false == posHandle.CheckParentAlive())
                    {
#if DEBUG
                        Debuginfo.Log("Items_SyncPosRotFromHandle() posHandle.goHandleMasterObject == null", 2);
                        continue;
#endif
                    }
                    aRenewPrefab(posHandle);

                    if (false == posHandle.Visible)
                    {
                        aProcessHandle_Invisible(posHandle, posHandle.sCategory.Equals("MaidPartsHandle"));
                    }
                    else
                    {
                        aProcessHandle_Visible(posHandle);

                        if (true == posHandle.ControllClicked() && true == Super.settingsXml.bHotkeyItemDelete)
                        {
                            if (true == FlexKeycode.GetKey(Super.settingsXml.sHotkeyItemDelete))
                            {
                                Super.Window.Items_ClearSubItems();
                                Items_RemoveHandle(posHandle);
                                Super.Window.Items_UpdateCurrentHandleCount();
                                break;
                            }
                        }
                    }
                }
            }
            else if (true == Super.Window.issubItems_list_tabPage2())
            {
                ItemHandleList = new List<HandleEx>();
                if (true == Super.Window.Items_isItemHandleMethodAll())
                {
                    ItemHandleList = Items_Sub_ItemHandle.Select(kvp => kvp.Key).ToList();
                }
                else
                {
                    if (Items_Sub_selectedHandles.Count > 0)
                        ItemHandleList = Items_Sub_selectedHandles.ToList();
                    else
                        ItemHandleList.Clear();
                }

                if (0 != ItemHandleList.Count)
                {
                    foreach (HandleEx posHandle in ItemHandleList)
                    {
                        if (true == posHandle.sItemName.Contains("S>"))
                        {
                            posHandle.Visible = false;
                            continue;
                        }

                        if (false == posHandle.Visible)
                        {
                            if (true == posHandle.sItemName.Contains("D>"))
                            {
                                aProcessHandle_Invisible_Gravity(posHandle);
                            }
                            else
                            {
                                aProcessHandle_Invisible(posHandle, true);
                            }
                        }
                        else
                        {
                            if (true == posHandle.sItemName.Contains("D>"))
                            {
                                aProcessHandle_Visible_Gravity(posHandle);
                            }
                            else
                            {
                                aProcessHandle_Visible(posHandle);
                            }
                        }
                    }
                }
            }
        }

        private bool Items_AddExistPrefab(GameObject oldGoItem, GameObject goParentBone, Vector3 f_vOffsetLocalPos, Vector3 f_vOffsetLocalRot, out GameObject newGoItem)
        {
            Transform transform = goParentBone.transform;

            if (transform != null)
            {
                GameObject goItem = UnityEngine.Object.Instantiate(oldGoItem) as GameObject;
                goItem.name = goItem.name.Replace("(Clone)", string.Empty);
                if (!string.IsNullOrEmpty(oldGoItem.name))
                {
                    goItem.name = oldGoItem.name;
                }               
                AttachPrefab[] componentsInChildren = goItemMaster.transform.GetComponentsInChildren<AttachPrefab>(true);
                AttachPrefab[] array = (from a in componentsInChildren
                                        where a.name == goItem.name
                                        select a).ToArray<AttachPrefab>();
                for (int i = 0; i < array.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(array[i].gameObject);
                }
                Random rnd = new Random(DateTime.Now.Millisecond);
                goItem.name += ("_" + DateTime.UtcNow.ToString() + "_" + rnd.Next(10000));
                goItem.transform.SetParent(transform, false);
                goItem.transform.localPosition = f_vOffsetLocalPos;
                goItem.transform.localRotation = Quaternion.Euler(f_vOffsetLocalRot);

                newGoItem = goItem;
                return true;
            }
            else
            {
                Debug.LogError("人物にプレハブ追加で、ボーン名が見つかりません " + goParentBone.name);
            }

            newGoItem = null;
            return false;
        }
        
        private bool Items_AddPrefab(string f_strPrefab, string sName, GameObject goParentBone, Vector3 f_vOffsetLocalPos, Vector3 f_vOffsetLocalRot, out GameObject newGoItem)
        {
            UnityEngine.Object @object = Resources.Load(f_strPrefab);
            if (@object != null)
            {
                GameObject goItem = UnityEngine.Object.Instantiate(@object) as GameObject;
                return Items_CreateItem(sName, goParentBone, f_vOffsetLocalPos, f_vOffsetLocalRot, goItem, out newGoItem);
            }
            else
            {
                Debug.LogError("人物にプレハブ追加で、プレハブが見つかりません。" + f_strPrefab);
            }
            newGoItem = null;
            return false;
        }

        private bool Items_AddPrefabOdogu(string sName, GameObject goParentBone, Vector3 f_vOffsetLocalPos, Vector3 f_vOffsetLocalRot, PhotoBGObjectData_Odogu odogu, out GameObject newGoItem, out string sDetailedInformation)
        {
            GameObject goItem = Items_Odogu_Instantiate(sName, odogu, out sDetailedInformation);
            if (goItem != null)
            {
                return Items_CreateItem(sName, goParentBone, f_vOffsetLocalPos, f_vOffsetLocalRot, goItem, out newGoItem);
            }
            else
            {
                Debug.LogError("人物にプレハブ追加で、プレハブが見つかりません。" + sName);
            }
            newGoItem = null;
            return false;
        }

        private bool Items_AddPrefabDeskItem(string sName, GameObject goParentBone, Vector3 f_vOffsetLocalPos, Vector3 f_vOffsetLocalRot, DeskItemData deskItem, out GameObject newGoItem, out string sDetailedInformation)
        {
            GameObject goItem = Items_DeskItem_Instantiate(sName, deskItem, out sDetailedInformation);
            if (goItem != null)
            {
                return Items_CreateItem(sName, goParentBone, f_vOffsetLocalPos, f_vOffsetLocalRot, goItem, out newGoItem);
            }
            else
            {
                Debug.LogError("人物にプレハブ追加で、プレハブが見つかりません。" + sName);
            }
            newGoItem = null;
            return false;
        }

        private bool Items_CreateItem(string sName, GameObject goParentBone, Vector3 f_vOffsetLocalPos, Vector3 f_vOffsetLocalRot, GameObject goItem, out GameObject newGoItem)
        {
            Transform transform = goParentBone.transform;
            if (transform != null)
            {
                goItem.AddComponent<AttachPrefab>();
                goItem.name = goItem.name.Replace("(Clone)", string.Empty);
                if (!string.IsNullOrEmpty(sName))
                {
                    goItem.name = sName;
                }
                AttachPrefab[] componentsInChildren = goItemMaster.transform.GetComponentsInChildren<AttachPrefab>(true);
                AttachPrefab[] array = (from a in componentsInChildren
                                        where a.name == goItem.name
                                        select a).ToArray<AttachPrefab>();
                for (int i = 0; i < array.Length; i++)
                {
                    UnityEngine.Object.DestroyImmediate(array[i].gameObject);
                }
                Random rnd = new Random(DateTime.Now.Millisecond);
                goItem.name += ("_" + DateTime.UtcNow.ToString() + "_" + rnd.Next(10000));
                goItem.transform.SetParent(transform, false);
                goItem.transform.localPosition = f_vOffsetLocalPos;
                goItem.transform.localRotation = Quaternion.Euler(f_vOffsetLocalRot);

                newGoItem = goItem;
                return true;
            }

            Debug.LogError("人物にプレハブ追加で、ボーン名が見つかりません" + goParentBone.name);
            newGoItem = null;
            return false;
        }

        public GameObject CreateAssetBundle(string fileName)
        {
            string bgFileName = fileName.ToLower() + ".asset_bg";
            if (false == File.Exists(bgFileName))
            {
#if DEBUG
                Debuginfo.Log("bgFileName not found "+bgFileName, 2);
#endif
                return null;
            }
            long fileSize = new FileInfo(bgFileName).Length;
            byte[] fileByte = new byte[fileSize + 1];
            FileStream FS = new FileStream(bgFileName, FileMode.Open);
            long readSize = FS.Read(fileByte, 0, fileByte.Length);
            FS.Close();
#if DEBUG
            Debuginfo.Log("readSize=" + readSize + "  fileSize=" + fileSize, 2);
#endif

            UnityEngine.Object @object = null;
            if (readSize == fileSize)
            {
                AssetBundle assetBundle = AssetBundle.LoadFromMemory(fileByte);
                AssetBundleObj value = default(AssetBundleObj);
                if (assetBundle.mainAsset != null)
                {
                    value.obj = assetBundle.mainAsset;
                }
                else
                {
                    value.obj = assetBundle.LoadAllAssets<GameObject>()[0];
                }
                value.ab = assetBundle;
                @object = value.obj;

                string shaderFileName = fileName.ToLower() + ".asset_bg_shader";
                if (true == File.Exists(shaderFileName))
                {
                    long shaderfileSize = new FileInfo(shaderFileName).Length;
                    byte[] shaderfileByte = new byte[shaderfileSize + 1];
                    FileStream shaderFS = new FileStream(shaderFileName, FileMode.Open);
                    long shaderreadSize = shaderFS.Read(shaderfileByte, 0, shaderfileByte.Length);
                    shaderFS.Close();

                    if (shaderreadSize == shaderfileSize)
                    {
                        LoadAssetBgShaderData(@object as GameObject, shaderfileByte);
                    }
                }
            }
            return (!(@object == null)) ? (@object as GameObject) : null;
        }

        private void LoadAssetBgShaderData(GameObject bgObject, byte[] binary)
        {
            if (binary == null || binary.Length <= 0)
            {
                return;
            }
            using (MemoryStream memoryStream = new MemoryStream(binary))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int num = binaryReader.ReadInt32();
                    List<Shader> list = new List<Shader>();
                    Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
                    int num2 = (int)binaryReader.ReadInt16();
                    for (int i = 0; i < num2; i++)
                    {
                        string text = binaryReader.ReadString();
                        Shader shader = Shader.Find(text);
                        if (shader == null)
                        {
                            Debug.LogError("shader名[" + text + "]からshaderを特定できませんでした");
                        }
                        else
                        {
                            list.Add(shader);
                        }
                    }
                    num2 = (int)binaryReader.ReadInt16();
                    for (int j = 0; j < num2; j++)
                    {
                        string key = binaryReader.ReadString();
                        List<int> list2 = new List<int>();
                        int num3 = (int)binaryReader.ReadInt16();
                        for (int k = 0; k < num3; k++)
                        {
                            list2.Add(binaryReader.ReadInt32());
                        }
                        dictionary.Add(key, list2);
                    }
                    Renderer[] componentsInChildren = bgObject.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer renderer in componentsInChildren)
                    {
                        List<int> list3 = dictionary[renderer.gameObject.name];
                        int num4 = 0;
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (!(material == null))
                            {
                                int index = list3[num4];
                                material.shader = list[index];
                                num4++;
                            }
                        }
                    }
                }
            }
        }

        private GameObject Items_Odogu_Instantiate(string sName, PhotoBGObjectData_Odogu odogu, out string sDetailedInformation)
        {
            Transform transform = GameMain.Instance.BgMgr.bg_parent_object.transform.Find("PhotoPrefab");
            if (transform == null)
            {
                GameObject gameObject = new GameObject("PhotoPrefab");
                gameObject.transform.SetParent(GameMain.Instance.BgMgr.bg_parent_object.transform, false);
                transform = gameObject.transform;
            }
            GameObject gameObject2 = null;
            sDetailedInformation = "Odogu|";
            if (!string.IsNullOrEmpty(odogu.create_prefab_name))
            {
                sDetailedInformation += "create_prefab_name|";
                UnityEngine.Object @object = Resources.Load("Prefab/" + odogu.create_prefab_name);
                if (@object == null)
                {
                    return null;
                }
                gameObject2 = (UnityEngine.Object.Instantiate(@object) as GameObject);
            }
            else if (!string.IsNullOrEmpty(odogu.create_asset_bundle_name))
            {
                sDetailedInformation += "create_asset_bundle_name|";
                GameObject gameObject3 = GameMain.Instance.BgMgr.CreateAssetBundle(odogu.create_asset_bundle_name);
                if (gameObject3 == null)
                {
                    return null;
                }
                gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject3);
            }
            else if (!string.IsNullOrEmpty(odogu.direct_file))
            {
                sDetailedInformation += "direct_file|";
                BasePhotoCustomObject basePhotoCustomObject = BasePhotoCustomObject.InstantiateFromFile(transform.gameObject, odogu.direct_file);
                gameObject2 = basePhotoCustomObject.gameObject;
            }
            else if (!string.IsNullOrEmpty(odogu.assert_bg))
            {
                sDetailedInformation += "assert_bg|";
                UnityEngine.Object @object = null;
                string sAssertBgName = Path.GetFileName(odogu.assert_bg.ToLower());
                if (true == GameUty.BgFiles.ContainsKey(sAssertBgName + ".asset_bg"))
                {
#if DEBUG
                    Debuginfo.Warning("@object found in GameUty.BgFiles", 2);
#endif
                    @object = GameMain.Instance.BgMgr.CreateAssetBundle(sAssertBgName);
                }
                else
                {
#if DEBUG
                    Debuginfo.Warning("@object load from file", 2);
#endif
                    @object = CreateAssetBundle(odogu.assert_bg);
                }
                if (@object == null)
                {
                    return null;
                }
                gameObject2 = (UnityEngine.Object.Instantiate(@object) as GameObject);
            }
            if (gameObject2 == null)
            {
                sDetailedInformation += "null|";
                return null;
            }
            if (gameObject2.GetComponentInChildren<BoxCollider>() == null)
            {
                MeshRenderer componentInChildren = gameObject2.GetComponentInChildren<MeshRenderer>(true);
                if (componentInChildren != null)
                {
                    componentInChildren.gameObject.AddComponent<BoxCollider>();
#if DEBUG
                    Debuginfo.Log("AddComponent<BoxCollider>() gameObject.name " + componentInChildren.gameObject.name , 2);
#endif
                }
            }
            if (!string.IsNullOrEmpty(sName))
            {
                gameObject2.name = sName;
            }
            gameObject2.name = gameObject2.name.Replace("(Clone)", "");
            gameObject2.transform.SetParent(transform);
            return gameObject2;
        }


        private GameObject Items_DeskItem_Instantiate(string sName, DeskItemData deskItem, out string sDetailedInformation)
        {
            Transform transform = GameMain.Instance.BgMgr.bg_parent_object.transform.Find("PhotoPrefab");
            if (transform == null)
            {
                GameObject gameObject = new GameObject("PhotoPrefab");
                gameObject.transform.SetParent(GameMain.Instance.BgMgr.bg_parent_object.transform, false);
                transform = gameObject.transform;
            }
            GameObject gameObject2 = null;
            sDetailedInformation = "DeskItem|";
            if (!string.IsNullOrEmpty(deskItem.prefab_name))
            {
                sDetailedInformation += "prefab_name|";
                UnityEngine.Object @object = Resources.Load("Prefab/" + deskItem.prefab_name);
                if (@object == null)
                {
                    return null;
                }
                gameObject2 = (UnityEngine.Object.Instantiate(@object) as GameObject);
            }
            else if (!string.IsNullOrEmpty(deskItem.asset_name))
            {
                sDetailedInformation += "asset_name|";
                GameObject gameObject3 = GameMain.Instance.BgMgr.CreateAssetBundle(deskItem.asset_name);
                if (gameObject3 == null)
                {
                    return null;
                }
                gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject3);
            }
            
            if (gameObject2 == null)
            {
                sDetailedInformation += "null|";
                return null;
            }
            if (gameObject2.GetComponentInChildren<BoxCollider>() == null)
            {
                MeshRenderer componentInChildren = gameObject2.GetComponentInChildren<MeshRenderer>(true);
                if (componentInChildren != null)
                {
                    componentInChildren.gameObject.AddComponent<BoxCollider>();
#if DEBUG
                    Debuginfo.Log("AddComponent<BoxCollider>() gameObject.name " + componentInChildren.gameObject.name, 2);
#endif
                }
            }
            if (!string.IsNullOrEmpty(sName))
            {
                gameObject2.name = sName;
            }
            gameObject2.name = gameObject2.name.Replace("(Clone)", "");
            gameObject2.transform.SetParent(transform);
            return gameObject2;
        }

        private void Items_DelPrefab(string sName, GameObject goMaster)
        {
            AttachPrefab[] componentsInChildren = goMaster.transform.GetComponentsInChildren<AttachPrefab>(true);
            AttachPrefab[] array = (from a in componentsInChildren
                                    where a.name == sName
                                    select a).ToArray<AttachPrefab>();
            for (int i = 0; i < array.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(array[i].gameObject);
            }
        }
        #endregion
    }
}



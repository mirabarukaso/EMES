using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    //ランタイムモデルエクスポート
    public class EMES_RuntimeModelExport
    {
        //参照
        //https://en.wikipedia.org/wiki/Wavefront_.obj_file
        //https://answers.unity.com/questions/603081/realtime-vertex-position-of-a-skinnedmesh.html

        public const string PluginVersion = "1.0.0.0";

        private int currentVerticesIndex = 0;
        private int materialCount = 0;
        private int mtlMaterialCount = 0;
        private int materialIndex = 0;

        static string TrimGUID(string guid)
        {
            return guid.Substring(11, 5).Replace("-", "").ToUpper();
        }

        ~EMES_RuntimeModelExport()
        {
        }

        #region public method
        static public Texture2D SetTransparencyTexture(Texture2D texture, bool noScale = false)
        {
            Texture2D dst = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, true);
            Color firstColor = texture.GetPixel(0, 0);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    if (firstColor == texture.GetPixel(x, y))
                    {
                        dst.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        dst.SetPixel(x, y, texture.GetPixel(y, x));
                    }
                }
            }
            dst.Apply();

            if (false == noScale)
                TextureScale.Bilinear(dst, dst.width * 2, dst.height * 2);

            return dst;
        }


        public EMES_RuntimeModelExport()
        {
        }

        public void SaveModel(Maid maid, string directoryPath, bool noPNG)
        {
            TBody body = maid.body0;
            string timeStamp = DateTime.Now.ToString("MMddhhmmss");
            string maidName = TrimGUID(maid.status.guid);

            if (false == Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string objFileName = directoryPath + timeStamp + "_" + maidName + ".obj";
            string mtlFileName = directoryPath + timeStamp + "_" + maidName + ".mtl";

            FileStream objFile = new FileStream(objFileName, FileMode.Create, FileAccess.Write, FileShare.Write);
            StreamWriter objWriter = new StreamWriter(objFile, Encoding.UTF8);

            FileStream mtlFile = null;
            StreamWriter mtlWriter = null;

            List<SkinnedMeshRenderer> skinnedMeshRenderer = new List<SkinnedMeshRenderer>();
            Dictionary<Material, string> materialDict = new Dictionary<Material, string>();
            List<string> materialPngNameList = new List<string>();
            List<string> tattooPngNameList = new List<string>();

            string sSlotName;
            string targetTexture = "_MainTex";

            objWriter.WriteLine("#EMESによる自動生成\n#Enhanced Maid Edit Scene v" + EMES_RuntimeModelExport.PluginVersion);
            objWriter.WriteLine("#" + maid.status.firstName + maid.status.lastName);
            objWriter.WriteLine("#" + maid.status.guid);            
            objWriter.WriteLine("mtllib " + Path.GetFileName(mtlFileName));

            if (false == noPNG)
            {
                mtlFile = new FileStream(mtlFileName, FileMode.Create, FileAccess.Write, FileShare.Write);
                mtlWriter = new StreamWriter(mtlFile, Encoding.UTF8);
                mtlWriter.WriteLine("#EMESによる自動生成\n#Enhanced Maid Edit Scene v" + EMES_RuntimeModelExport.PluginVersion);
            }

            //タトゥー　✕　ほくる
            sSlotName = "Tattoo&Hokuru";
            if (false == noPNG)
            {
                Dictionary<string, TBody.TexLay.Mat> m_dicLaySlot = EMES.GetFieldValue<TBody, Dictionary<string, TBody.TexLay.Mat>>(body, "m_dicLaySlot");
                foreach (KeyValuePair<string, TBody.TexLay.Mat> mat in m_dicLaySlot)
                {
#if DEBUG
                    Debuginfo.Log(string.Format("m_dicLaySlot {0} Key={1} Value={2}", sSlotName, mat.Key, mat.Value), 2);
#endif
                    foreach (KeyValuePair<int, TBody.TexLay.Prop> keyValuePair in mat.Value.dicPropInMat)
                    {
#if DEBUG
                        Debuginfo.Log(string.Format("dicPropInMat Key={0} Value={1}", keyValuePair.Key, keyValuePair.Value), 2);
#endif
                        foreach (KeyValuePair<string, TBody.TexLay.Lay> keyValuePair2 in keyValuePair.Value.dicLayInProp)
                        {
#if DEBUG
                            Debuginfo.Log(string.Format("dicLayInProp string={0} TBody.TexLay.Lay={1}", keyValuePair2.Key, keyValuePair2.Value), 2);
#endif
                            if (true == keyValuePair2.Key.Equals(targetTexture)) 
                            {
                                foreach (TBody.TexLay.OrderTex orderTex in keyValuePair2.Value.listLayer)
                                {
                                    foreach (TBody.TexLay.LaySet laySet in orderTex.listLaySet)
                                    {
                                        Texture2D mainTexture = laySet.tex as Texture2D;
                                        if (null != mainTexture)
                                        {
                                            //body | head
                                            string pngName = string.Format("{0}_{1}_{2}_{3}_{4}{5}.png", objFileName.Replace(".obj", ""), sSlotName, mat.Key, orderTex.nLayerNo, tattooPngNameList.Count, keyValuePair2.Key);
                                            writePNG(pngName, mainTexture);
                                            tattooPngNameList.Add(Path.GetFileName(pngName));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Model
            foreach (TBody.SlotID slotID in Enum.GetValues(typeof(TBody.SlotID)))
            {
                sSlotName = Enum.GetName(typeof(TBody.SlotID), slotID);
                if (true == sSlotName.Equals("end"))
                    break;

                if (true == sSlotName.Contains("moza"))
                    continue;

                if (true == body.GetSlotLoaded(slotID))
                {
                    string currentMaterialName;
                    TBodySkin tbodySkin = body.GetSlot((int)slotID);
#if DEBUG
                    Debuginfo.Log("sSlotName = " + sSlotName, 2);
#endif
                    //OBJ
                    SkinnedMeshRenderer smr = tbodySkin.obj.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (null == smr)
                    {
                        //非表示のマチエールをスキップ
                        continue;
                    }

                    //PNG
                    if (false == noPNG)
                    {
#if COM3D25
                        Material[] materials = EMES.GetFieldValue<MaterialMgr, Material[]>(tbodySkin.MaterialMgr, "m_materials");
#else
                        Renderer[] list = tbodySkin.obj.transform.GetComponentsInChildren<Renderer>();
                        List<Material> materials = new List<Material>();
                        for (int i = 0; i < list.Length; i++)
                        {
                            Renderer renderer = list[i];
                            if (null != renderer)
                            {
                                if (null != renderer.materials)
                                {
                                    foreach (Material material in renderer.materials)
                                    {
                                        materials.Add(material);
                                    }
                                }
                            }
                        }
#endif
                        foreach (Material material in materials)
                        {
                            currentMaterialName = material.name.Replace("\\", "-").Replace("/", "-").Replace(" (Instance)", "");
                            Texture2D mainTexture = material.mainTexture as Texture2D;
                            if (null != mainTexture)
                            {
                                string pngName = string.Format("{0}_{1}_{2}_{3}.png", objFileName.Replace(".obj", ""), sSlotName, materialIndex, currentMaterialName);
                                string materialName = string.Format("{0}_{1}", currentMaterialName, materialIndex);
                                SavePNG(pngName, mainTexture, materialName, material, materialDict, materialPngNameList);
                            }
                            else
                            {
                                Dictionary<int, Dictionary<string, InfinityColorTextureCache.TextureSet>> tex_dic_ =
                                    EMES.GetFieldValue<InfinityColorTextureCache, Dictionary<int, Dictionary<string, InfinityColorTextureCache.TextureSet>>>(tbodySkin.TextureCache, "tex_dic_");

                                bool ret = false;
                                foreach (KeyValuePair<int, Dictionary<string, InfinityColorTextureCache.TextureSet>> dic in tex_dic_)
                                {
                                    foreach (KeyValuePair<string, InfinityColorTextureCache.TextureSet> tex in dic.Value)
                                    {
#if DEBUG
                                        Debuginfo.Log(string.Format("Index:{0} Key={1} Value={2}", dic.Key, tex.Key, tex.Value), 2);
#endif
                                        string materialName = string.Format("{0}_{1}-{2}", currentMaterialName, materialIndex, dic.Key);
                                        if (true == tex.Key.Equals(targetTexture))   
                                        {
                                            Texture2D _MainTex = ConvertRenderTextureToTexture2D(dic.Value[tex.Key].modified_tex);
                                            if (null != _MainTex)
                                            {
                                                string pngName = string.Format("{0}_{1}_{2}_{3}{4}-{5}.png", objFileName.Replace(".obj", ""), sSlotName, materialIndex, currentMaterialName, tex.Key, dic.Key);
                                                ret = SavePNG(pngName, _MainTex, materialName, material, materialDict, materialPngNameList);
                                                if (true == ret)
                                                    break;
                                            }
                                            else
                                            {
                                                _MainTex = dic.Value[tex.Key].base_tex as Texture2D;
                                                if (null != _MainTex)
                                                {
                                                    string pngName = string.Format("{0}_{1}_{2}_{3}_bt-{4}.png", objFileName.Replace(".obj", ""), sSlotName, materialIndex, currentMaterialName, dic.Key);
                                                    ret = SavePNG(pngName, _MainTex, materialName, material, materialDict, materialPngNameList);
                                                    if (true == ret)
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    if (true == ret)
                                        break;
                                }
                            }
                        }
                    }

                    //OBJ 2
                    objWriter.WriteLine("o " + smr.name);   //o = object     g = group
                    objWriter.Write(MeshToString(smr, materialDict.Values.ToList(), noPNG, maid.GetPos()));
                    objWriter.WriteLine("");
                }
            }
            objWriter.Close();
            objFile.Close();

            //MTL
            if (false == noPNG)
            {
                mtlWriter.Write(MaterialToString(materialDict, materialPngNameList, tattooPngNameList));
                mtlWriter.Close();
                mtlFile.Close();
            }

            Debuginfo.Log("[RTME]保存しました " + objFileName, 1);
            Debuginfo.Log("materialCount = " + materialCount, 2);
            Debuginfo.Log("materialIndex = " + materialIndex, 2);
            Debuginfo.Log("tattoo&hokuru = " + tattooPngNameList.Count, 2);
        }
#endregion

#region private method
        private static Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
        {
            if (null == renderTexture)
            {
                return null;
            }

            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            RenderTexture.active = old;

            return tex;
        }

        private string MeshToString(SkinnedMeshRenderer skinnedMeshRenderer, List<string> materialNameList, bool noMaterial, Vector3 maidPos)
        {
            int meshIndex = currentVerticesIndex;
            int biggest = 0;
            StringBuilder sb = new StringBuilder();

            Mesh mesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            skinnedMeshRenderer.BakeMesh(mesh);
            mesh.GetVertices(meshVertices);
            
            foreach (Vector3 worldPosVertex in meshVertices)
            {
                Vector3 v = skinnedMeshRenderer.transform.localToWorldMatrix.MultiplyPoint3x4(worldPosVertex);
                sb.Append(string.Format("v {0} {1} {2}\n", ((v.x - maidPos.x) * -1), (v.y - maidPos.y), (v.z - maidPos.z)));
            }
            foreach (Vector3 v in mesh.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            foreach (Vector3 v in mesh.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < mesh.subMeshCount; material++)
            {
                if (false == noMaterial)
                {
#if DEBUG
                    Debuginfo.Log(string.Format("materialNameList ={0}", materialNameList.Count), 2);
                    Debuginfo.Log(string.Format("[{0}] skinnedMeshRenderer {1} [{2}]", materialCount, skinnedMeshRenderer.name, materialNameList[materialCount]), 2);
#endif
                    sb.Append(string.Format("usemtl {0}\n", materialNameList[materialCount]));
                }
                else
                {
                    sb.Append(string.Format("usemtl Material_{0}\n", materialCount));
                }
                sb.Append("s 1\n");

                int[] triangles = mesh.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i] + 1 + meshIndex, triangles[i + 1] + 1 + meshIndex, triangles[i + 2] + 1 + meshIndex));
                    biggest = (biggest > (triangles[i + 0] + 1 + meshIndex)) ? biggest : (triangles[i + 0] + 1 + meshIndex);
                    biggest = (biggest > (triangles[i + 1] + 1 + meshIndex)) ? biggest : (triangles[i + 1] + 1 + meshIndex);
                    biggest = (biggest > (triangles[i + 2] + 1 + meshIndex)) ? biggest : (triangles[i + 2] + 1 + meshIndex);
                }

                materialCount++;
            }

            currentVerticesIndex = biggest;

            return sb.ToString();
        }

        private string MaterialToString(Dictionary<Material, string> materialNameDict, List<string> materialPngNameList, IEnumerable<string> tattooPngNameList)
        {
            List<string> materialNameList = materialNameDict.Values.ToList();
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<Material, string> material in materialNameDict)
            {
#if DEBUG
                Debuginfo.Log(string.Format("[{0}]{1}", mtlMaterialCount, materialNameList[mtlMaterialCount]), 2);
#endif
                sb.Append(string.Format("newmtl {0}\n", materialNameList[mtlMaterialCount]));
                sb.Append(string.Format("Ns 10.000\n"));
                sb.Append(string.Format("Ka {0} {1} {2}\n", material.Key.color.r, material.Key.color.g, material.Key.color.b));
                sb.Append(string.Format("Kd 1.000 1.000 1.000\n"));
                sb.Append(string.Format("Ks 0.000 0.000 0.000\n"));
                sb.Append(string.Format("Ni 1.000\n"));
                if (
                    true == materialNameList[mtlMaterialCount].Contains("_SkinAlpha_") ||
                    true == materialNameList[mtlMaterialCount].Contains("_SkinHiL_") ||
                    true == materialNameList[mtlMaterialCount].Contains("_SkinHiR_") ||
                    true == materialNameList[mtlMaterialCount].Contains("_SkinHi_") ||
                    true == materialNameList[mtlMaterialCount].Contains("_Mayu_") ||
                    true == materialNameList[mtlMaterialCount].Contains("nip_")
                    )
                {
                    sb.Append(string.Format("d 0\n"));
                }
                else
                {
                    sb.Append(string.Format("d {0}\n", material.Key.color.a));
                }
                sb.Append(string.Format("illum 2\n"));
                sb.Append(string.Format("map_Kd {0}\n", materialPngNameList[mtlMaterialCount]));
                sb.Append(string.Format("map_Ka {0}\n", materialPngNameList[mtlMaterialCount]));
                sb.Append(string.Format("map_d {0}\n", materialPngNameList[mtlMaterialCount]));
                if (true == materialNameList[mtlMaterialCount].StartsWith("skin_"))
                {
                    foreach (string pngName in tattooPngNameList)
                    {
                        if (true == pngName.Contains("body"))
                            sb.Append(string.Format("map_Kd {0}\n", pngName));
                    }
                }
                else if (true == materialNameList[mtlMaterialCount].Contains("_Skin_"))
                {
                    foreach (string pngName in tattooPngNameList)
                    {
                        if (true == pngName.Contains("head"))
                            sb.Append(string.Format("map_Kd {0}\n", pngName));
                    }
                }
                sb.Append("\n");
                mtlMaterialCount++;
            }

            return sb.ToString();
        }

        private void writePNG(string pngName, Texture2D texture)
        {
            byte[] png = texture.EncodeToPNG();
#if DEBUG
            Debuginfo.Log("writePNG pngName=" + pngName, 2);
#endif
            using (var file = new FileStream(pngName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                file.Write(png, 0, png.Length);
            }
        }

        private bool SavePNG(string pngName, Texture2D texture, string materialName, Material material, Dictionary<Material, string> materialDict, List<string> materialPngNameList)
        {
            Texture2D dst = texture;
            if (true == materialName.Contains("EyeL") || true == materialName.Contains("EyeR"))
            {
                if (false == pngName.EndsWith("-0.png"))
                {
                    return false;     //スキップ
                }
                dst = SetTransparencyTexture(texture);
            }
            else if (true == materialName.Contains("Mayu"))
            {
                if (false == pngName.EndsWith("-3.png"))
                {
                    return false;     //スキップ
                }
                dst = SetTransparencyTexture(texture);
            }
            else if (true == materialName.Contains("EyeWhite"))
            {
                if (false == pngName.EndsWith("-8.png"))
                {
                    return false;     //スキップ
                }
            }
            else if (true == materialName.Contains("EyeLashesTop"))
            {
                if (false == pngName.EndsWith("-10.png"))
                {
                    return false;     //スキップ
                }
            }

            if (false == materialDict.ContainsKey(material))
            {
                writePNG(pngName, dst);
                materialDict.Add(material, materialName);
                materialPngNameList.Add(Path.GetFileName(pngName));
                materialIndex++;
                return true;
            }

            return false;
        }
#endregion
    }

    //ランタイムモデルのインポート
    //RTMI
    public class EMES_RuntimeModelImporter
    {
        public const string PluginVersion = EMES_RuntimeModelExport.PluginVersion;

        public class ObjFile
        {
            public string mtllib;
            public List<Group> group = new List<Group>();
            public bool mirror_x = false;

            public class Group
            {
                public string g;
                public List<Vector3> v = new List<Vector3>();
                public List<Vector3> vn = new List<Vector3>();
                public List<Vector2> vt = new List<Vector2>();
                
                public Dictionary<string, int[]> usemtl = new Dictionary<string, int[]>();
            }            
        }

        public class MtlFile
        {
            public Dictionary<string, MtlGroup> newmtl = new Dictionary<string, MtlGroup>();

            public class MtlGroup
            {
                public float d = 1;
                public List<string> mapKa = new List<string>();
                public List<string> mapKd = new List<string>();
                public List<string> mapD = new List<string>();
            }
        }

        ~EMES_RuntimeModelImporter()
        {

        }

#region public method
        public EMES_RuntimeModelImporter()
        { 
        }

        public GameObject LoadModel(string objFileName, GameObject masterObject)
        {
            ObjFile obj = ReadOBJ(objFileName);
            if (null == obj)
            {
                return null;
            }

            string mtlFileName = Path.GetDirectoryName(objFileName) + "\\" + obj.mtllib;
            MtlFile mtl = ReadMTL(mtlFileName);
            if(null == mtl)
            {
                return null;
            }

            if(false == MAterialsCheck(obj,mtl))
            {
                return null;
            }

#if DEBUG
            Debuginfo.Log(string.Format("obj {0} {1}", obj.mtllib,obj.group.Count), 2);
            Debuginfo.Log(string.Format("mtl {0} ", mtl.newmtl.Count), 2);
#endif
            GameObject newModelObject = new GameObject();            
            System.Random rnd = new System.Random(DateTime.Now.Millisecond);
            newModelObject.name = "ImportedModel_" + UnityEngine.Random.Range(100,200).ToString();
            newModelObject.transform.SetParent(masterObject.transform);
            newModelObject.transform.position = masterObject.transform.position;
            newModelObject.transform.rotation = masterObject.transform.rotation;
            newModelObject.transform.localPosition = masterObject.transform.localPosition;
            newModelObject.transform.localRotation = masterObject.transform.localRotation;
            Create(obj, mtl, Path.GetDirectoryName(objFileName), newModelObject);
            return newModelObject;
        }
#endregion

#region private method        
        private bool MAterialsCheck(ObjFile obj, MtlFile mtl)
        {
            bool bPass = false;

            foreach (ObjFile.Group group in obj.group)
            {
                List<string> usemtlList = group.usemtl.Keys.ToList();
                foreach(string usemtl in usemtlList)
                {
#if DEBUG
                    Debuginfo.Log(string.Format("Checking Material {0} Kd.Count = {1}", usemtl, mtl.newmtl[usemtl].mapKd.Count), 2);
#endif
                    bPass = mtl.newmtl.ContainsKey(usemtl);
                    if (false == bPass)
                    {
                        Debuginfo.Warning(string.Format("Missing Material {0}", usemtl), 1);
                        break;
                    }

                    bPass = (mtl.newmtl[usemtl].mapKd.Count > 0) ? true : false;
                    if (false == bPass)
                    {
                        Debuginfo.Warning(string.Format("Missing map_Kd in Material {0}", usemtl), 1);
                        break;
                    }
                }

                if (false == bPass)
                    break;
            }
            
            return bPass;
        }

        private void Create(ObjFile obj, MtlFile mtl, string directoryPath, GameObject masterObject)
        {
            int index;
            int materialIndex;
            int biggest = 0;

            foreach (ObjFile.Group group in obj.group)
            {
#if DEBUG
                Debuginfo.Log("group.usemtl.Count " + group.usemtl.Count, 2);
#endif
                if (0 == group.usemtl.Count)
                {
#if DEBUG
                    Debuginfo.Log("空グループ無視する " + group.usemtl.Count, 2);
#endif
                    continue;
                }

                Material[] materials = new Material[group.usemtl.Count];
                index = 0;
                foreach (KeyValuePair<string, int[]> usemtl in group.usemtl)
                {
#if DEBUG
                    Debuginfo.Log("usemtl.Key " + usemtl.Key, 2);
#endif
                    Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    texture.LoadImage(File.ReadAllBytes(directoryPath + "\\" + mtl.newmtl[usemtl.Key].mapKd[0]));
#if DEBUG
                    Debuginfo.Log("mtl.newmtl[usemtl.Key].mapKd[0] " + mtl.newmtl[usemtl.Key].mapKd[0], 2);
                    Debuginfo.Log("mtl.newmtl[usemtl.Key].mapKd.Count " + mtl.newmtl[usemtl.Key].mapKd.Count, 2);
#endif
                    if (mtl.newmtl[usemtl.Key].mapKd.Count > 1)
                    {
                        for (int i = 1; i < mtl.newmtl[usemtl.Key].mapKd.Count; i++)
                        {
#if DEBUG
                            Debuginfo.Log("混じる mapKd[" + i +"] " + mtl.newmtl[usemtl.Key].mapKd[i], 2);
#endif
                            Texture2D texture_new = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                            texture_new.LoadImage(File.ReadAllBytes(directoryPath + "\\" + mtl.newmtl[usemtl.Key].mapKd[i]));

                            Color[] colors = texture.GetPixels(0, 0, texture.width, texture.height);
                            Color[] colors_new = texture_new.GetPixels(0, 0, texture_new.width, texture_new.height);
                            Color[] colors_merge = new Color[colors.Length];
                            for (int j = 0; j < colors.Length; j++)
                            {
                                colors_merge[j] = Color.Lerp(colors[j] , colors_new[j], colors_new[j].a);
                            }
                            texture.SetPixels(colors_merge);
                            texture.Apply();
                        }
                    }
                    /*
                     * MaterialProperty.type
                       Properties
                        0   Color	Color property.
                        1   Vector	Vector property.
                        2   Float	Float property.
                        3   Range	Ranged float (with min/max values) property.
                        4   Texture	Texture property.
                        5   Int	Int property.
                     */
                    materials[index] = new Material(Shader.Find("CM3D2/Lighted_Cutout_AtC"));
                    materials[index].name = usemtl.Key;                   
                    materials[index].SetColor("_Color", new Color(1,1,1,1f));
                    materials[index].SetColor("_SpecColor", new Color(0.5f, 0.5f, 0.5f, 1));
                    materials[index].SetColor("_Emission", new Color(1, 1, 1, 1));
                    materials[index].SetFloat("_Shininess", 0.7f);
                    materials[index].SetTexture("_MainTex", texture);
                    materials[index].SetFloat("_Cutoff", 0.01f);
                    index++;
                }


#if DEBUG
                Debuginfo.Log("vertices group.v.Count " + group.v.Count, 2);
                Debuginfo.Log("normals group.vn.Count " + group.vn.Count, 2);
                Debuginfo.Log("uvs group.vt.Count " + group.vt.Count, 2);
#endif
                Vector3[] vertices = group.v.ToArray();
                Vector3[] normals;
                Vector2[] uvs = group.vt.ToArray();

                if(0 == group.vn.Count)
                {
                    normals = null;
                }
                else
                {
                    normals = group.vn.ToArray();
                }

                if (true == obj.mirror_x)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i].x *= -1;
                    }
                }

#if DEBUG
                Debuginfo.Log("Create Mesh ", 2);
#endif
                Mesh mesh = new Mesh();
                mesh.name = group.g + "_mesh";
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uvs;
                mesh.subMeshCount = group.usemtl.Count;
#if DEBUG
                Debuginfo.Log("mesh.name " + mesh.name, 2);
                Debuginfo.Log("group.usemtl " + group.usemtl.Count, 2);
#endif
                index = 0;
                materialIndex = biggest;
                foreach (KeyValuePair<string, int[]> kvp in group.usemtl)
                {
#if DEBUG
                    Debuginfo.Log("kvp name " + kvp.Key, 2);
                    Debuginfo.Log("kvp value " + kvp.Value.Length, 2);
#endif
                    int[] triangles = new int[kvp.Value.Length];
                    for (int j = 0; j < triangles.Length; j++)
                    {
                        triangles[j] = kvp.Value[j] - 1 - materialIndex;
                        if (biggest < kvp.Value[j])
                            biggest = kvp.Value[j];
                    }
#if DEBUG
                    Debuginfo.Log("SetTriangles " + index, 2);
#endif
                    mesh.SetTriangles(triangles, index);
                    index++;
                }
                mesh.RecalculateBounds();

#if DEBUG
                Debuginfo.Log("Create GameObject ", 2);
#endif
                GameObject gameObject = new GameObject();
                gameObject.name = group.g + "_obj";
                gameObject.layer = masterObject.layer;
                gameObject.transform.SetParent(masterObject.transform, false);
#if DEBUG
                Debuginfo.Log("Create SkinnedMeshRenderer ", 2);
#endif
                SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetOrAddComponent<SkinnedMeshRenderer>();
                skinnedMeshRenderer.sharedMesh = mesh;
                skinnedMeshRenderer.sharedMaterials = materials;
                skinnedMeshRenderer.receiveShadows = true;
                skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.On;
                skinnedMeshRenderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            }

#if DEBUG
            Debuginfo.Log("Done", 2);
#endif
        }

        private bool CheckMagicKey(string fileName, string sender)
        {
            using (var reader = new StreamReader(fileName))
            {
                string magic = reader.ReadLine();
                if (true == magic.StartsWith("#EMES"))
                {
                    return true;
                }
                else
                {
                    Debuginfo.Warning(sender + " 指定されたマジックキーが見つかりません", 1);
                }
            }
            return false;
        }

        private ObjFile ReadOBJ(string objFileName)
        {
#if DEBUG
            Debuginfo.Log(string.Format("obj = {0}", objFileName), 2);
#endif
            if (false == File.Exists(objFileName))
            {
                Debuginfo.Warning("OBJファイルが存在しません", 1);
                return null;
            }

            ObjFile obj = new ObjFile();
            obj.mirror_x = CheckMagicKey(objFileName, "OBJ");

            string[] lines = File.ReadAllLines(objFileName);
            string[] data;
            string lastMTL = null;
            ObjFile.Group g = null;
            List<int> fList = new List<int>();
            foreach (string line in lines)
            {
                if ("" == line || true == line.StartsWith("#"))
                    continue;
//#if DEBUG
                //Debuginfo.Warning(line, 2);
//#endif
                data = line.Split(' ');
                if (true == data[0].Equals("mtllib"))
                {
                    obj.mtllib = data[1];
                }
                else if (true == data[0].Equals("o") || true == data[0].Equals("g"))
                {
                    if (null != lastMTL)
                    {
                        if (false == g.usemtl.ContainsKey(lastMTL))
                        {
                            g.usemtl.Add(lastMTL, fList.ToArray());
                            fList = new List<int>();
                        }
                    }
                    lastMTL = null;
                    g = new ObjFile.Group();
                    g.g = data[1];
                    obj.group.Add(g);
                }
                else if (true == data[0].Equals("v"))
                {
                    g.v.Add(new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])));
                }
                else if (true == data[0].Equals("vn"))
                {
                    g.vn.Add(new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])));
                }
                else if (true == data[0].Equals("vt"))
                {
                    g.vt.Add(new Vector3(float.Parse(data[1]), float.Parse(data[2])));
                }
                else if (true == data[0].Equals("usemtl"))
                {
                    if (null != lastMTL)
                    {
                        if (false == g.usemtl.ContainsKey(lastMTL))
                        {
                            g.usemtl.Add(lastMTL, fList.ToArray());
                            fList = new List<int>();
                        }
                    }
                    lastMTL = data[1].Split('.')[0];
                }
                else if (true == data[0].Equals("f"))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        string[] f = data[i].Split('/');
                        fList.Add(int.Parse(f[0]));
                    }
                }
                else if (true == data[0].Equals("s"))
                {
                    continue;
                }
            }
            if (null != lastMTL)
            {
                if (false == g.usemtl.ContainsKey(lastMTL))
                    g.usemtl.Add(lastMTL, fList.ToArray());
            }

            return obj;
        }

        private MtlFile ReadMTL(string mtlFileName)
        {
#if DEBUG
            Debuginfo.Log(string.Format("mtl = {0}", mtlFileName), 2);
#endif
            if (false == File.Exists(mtlFileName))
            {
                Debuginfo.Warning("MTLファイルが存在しません", 1);
                return null;
            }

            CheckMagicKey(mtlFileName, "MTL");

            MtlFile mtl = new MtlFile();
            string[] lines = File.ReadAllLines(mtlFileName);
            string[] data;
            string lastMtllib = null;
            MtlFile.MtlGroup newmtl = null;
            foreach (string line in lines)
            {
                if ("" == line || true == line.StartsWith("#"))
                    continue;

                data = line.Split(' ');
                if (true == data[0].Equals("newmtl"))
                {
                    newmtl = new MtlFile.MtlGroup();
                    mtl.newmtl.Add(data[1].Split('.')[0], newmtl);
                    lastMtllib = data[1].Split('.')[0];
                }
                else if (true == data[0].Equals("d"))
                {
                    mtl.newmtl[lastMtllib].d = float.Parse(data[1]);
                }
                else if (true == data[0].Equals("map_Kd"))
                {
                    mtl.newmtl[lastMtllib].mapKd.Add(data[1]);
                }
                else if (true == data[0].Equals("map_Ka"))
                {
                    mtl.newmtl[lastMtllib].mapKa.Add(data[1]);
                }
                else if (true == data[0].Equals("map_d"))
                {
                    mtl.newmtl[lastMtllib].mapD.Add(data[1]);
                }
            }

            if (null != lastMtllib)
            {
                if (false == mtl.newmtl.ContainsKey(lastMtllib))
                    mtl.newmtl.Add(lastMtllib, newmtl);
            }

            return mtl;
        }
#endregion
    }
}

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public static class TexPngTools
    {
        public static byte[] PngToTexCM3D2(byte[] pngBuffer)
        {
            Image image;
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(pngBuffer, 0, Convert.ToInt32(pngBuffer.Length));
                image = Image.FromStream(mStream);
            }

            byte[] outByte;
            using (MemoryStream mStreamPNG = new MemoryStream())
            {
                image.Save(mStreamPNG, ImageFormat.Png);
                using (MemoryStream msTex = new MemoryStream())
                {
                    using (BinaryWriter bwTex = new BinaryWriter(msTex))
                    {
                        bwTex.Write("CM3D2_TEX");
                        bwTex.Write(1010);
                        bwTex.Write(string.Empty);

                        bwTex.Write(image.Width);
                        bwTex.Write(image.Height);
                        bwTex.Write((int)TextureFormat.ARGB32);
                        bwTex.Write(mStreamPNG.ToArray().Length);
                        bwTex.Write(mStreamPNG.ToArray());
                    }
                    msTex.Flush();
                    outByte = (byte[])msTex.GetBuffer().Clone();
                }
            }

            return outByte;
        }

        public static byte[] TexCM3D2ToPngFromFile(string texFileName)
        {
            byte[] texBuffer = File.ReadAllBytes(texFileName);
            return TexCM3D2ToPng(texBuffer);
        }

        public static byte[] TexCM3D2ToPng(byte[] texBuffer)
        {
            BinaryReader binaryReader = new BinaryReader(new MemoryStream(texBuffer), Encoding.UTF8);
            string tagCM3D2 = binaryReader.ReadString();

            byte[] bStreamIcon;
            if (tagCM3D2 != "CM3D2_TEX")
            {
                Debuginfo.Error("ProcScriptBin 例外 : ヘッダーファイルが不正です。" + tagCM3D2);
                return null;
            }
            else
            {
                int num = binaryReader.ReadInt32();
                string text2 = binaryReader.ReadString();
                int width = 0;
                int height = 0;
                TextureFormat textureFormat = TextureFormat.ARGB32;
                Rect[] array = null;
                if (1010 <= num)
                {
                    if (1011 <= num)
                    {
                        int num2 = binaryReader.ReadInt32();
                        if (0 < num2)
                        {
                            array = new Rect[num2];
                            for (int i = 0; i < num2; i++)
                            {
                                float x = binaryReader.ReadSingle();
                                float y = binaryReader.ReadSingle();
                                float width2 = binaryReader.ReadSingle();
                                float height2 = binaryReader.ReadSingle();
                                array[i] = new Rect(x, y, width2, height2);
                            }
                        }
                    }
                    width = binaryReader.ReadInt32();
                    height = binaryReader.ReadInt32();
                    textureFormat = (TextureFormat)binaryReader.ReadInt32();
                }
                int num3 = binaryReader.ReadInt32();
                byte[] array2;
                array2 = new byte[num3];
                binaryReader.Read(array2, 0, num3);
                if (num == 1000)
                {
                    width = ((int)array2[16] << 24 | (int)array2[17] << 16 | (int)array2[18] << 8 | (int)array2[19]);
                    height = ((int)array2[20] << 24 | (int)array2[21] << 16 | (int)array2[22] << 8 | (int)array2[23]);
                }
                binaryReader.Close();

                TextureResource textureResource = new TextureResource(width, height, textureFormat, array, array2);
                Texture2D tIcon = textureResource.CreateTexture2D();

                bStreamIcon = tIcon.EncodeToPNG();
            }

            return bStreamIcon;
        }
    }
}


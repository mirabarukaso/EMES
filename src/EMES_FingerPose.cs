using System.IO;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    public class FingerPose
    {
        const int maxDataCount = 1 * 1024 * 1024; //1MB
        private static int Index = 0;

        public static float[] Left { get { return _Left; } private set { _Left = value; } }
        private static float[] _Left = new float[]
        {
            26.8565f, -56.0957f, 22.7366f, 0.0000f, 0.0000f, 11.5098f, 0.0000f, 0.0000f, 2.1397f, -2.3634f, -16.7405f, 8.7994f, 0.0000f, 0.0000f, -1.8603f, 0.0000f, 0.0000f, 2.1397f, -0.0127f, -0.3442f, -5.4917f, 0.0000f, 0.0000f, 2.0211f, 0.0000f, 0.0000f, 0.1397f, -5.9387f, 10.3549f, -22.6833f, 0.0000f, 0.0000f, 0.2581f, 0.0000f, 0.0000f, 1.1425f, -20.5529f, 18.6498f, -39.7336f, 0.0000f, 0.0000f, -1.8603f, 0.0000f, 0.0000f, 4.0325f,
            32.0514f, 180.3895f, 138.1139f, 0.0000f, 0.0000f, -5.6545f, 0.0000f, 0.0000f, 4.0000f, 6.2530f, 2.7673f, -61.8130f, 0.0000f, 0.0000f, -27.0245f, 0.0000f, 0.0000f, -2.0000f, 0.2953f, -0.1774f, -66.6182f, 0.0000f, 0.0000f, -23.1431f, 0.0000f, 0.0000f, -2.0000f, -9.6486f, -4.3892f, -71.4632f, 0.0000f, 0.0000f, -24.9062f, 0.0000f, 0.0000f, -0.9972f, -17.4266f, -6.8840f, -74.5232f, 0.0000f, 0.0000f, -32.0241f, 0.0000f, 0.0000f, -3.1072f,
            0   //削除しないで
        };

        //夜勤さんの引用：
        //”ボーンの本体側の数値(クォータニオン)をプラグイン側の数値（オイラー角）に反映させる関数”
        //”回転順序を変えてるのでunity標準関数が使えなくて自力で変換させた結果がこれだよ！”
        public static void Calc_trBone2Param(Transform bone, out float x, out float y, out float z)
        {
            if (bone.name == "Bip01 L Hand" || bone.name == "Bip01 R Hand")
            {
                // m00:1-2y^2-2z^2 m01:2xy+2wz     m02:2xz-2wy
                // m10:2xy-2wz     m11:1-2x^2-2z^2 m12:2yz+2wx
                // m20:2xz+2wy     m21:2yz-2wx     m22:1-2x^2-2y^2
                // X->Y->Z

                float qx = bone.localRotation.x;
                float qy = bone.localRotation.y;
                float qz = bone.localRotation.z;
                float qw = bone.localRotation.w;

                float m02 = 2 * (qx * qz - qw * qy);
                if (m02 > 1.0f) m02 = 1.0f;
                if (m02 < -1.0f) m02 = -1.0f;

                y = Mathf.Asin(-m02) * Mathf.Rad2Deg;

                if (System.Math.Floor(Mathf.Cos(y * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
                {
                    float m00 = 1 - 2 * (qy * qy + qz * qz);
                    float m01 = 2 * (qx * qy + qw * qz);
                    float m12 = 2 * (qy * qz + qw * qx);
                    float m22 = 1 - 2 * (qx * qx + qy * qy);

                    if (m00 > 1.0f) m00 = 1.0f;
                    if (m00 < -1.0f) m00 = -1.0f;
                    if (m01 > 1.0f) m01 = 1.0f;
                    if (m01 < -1.0f) m01 = -1.0f;

                    if (m12 > 1.0f) m12 = 1.0f;
                    if (m12 < -1.0f) m12 = -1.0f;
                    if (m22 > 1.0f) m22 = 1.0f;
                    if (m22 < -1.0f) m22 = -1.0f;

                    z = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg;
                    float before = m12 / Mathf.Cos(y * Mathf.Deg2Rad);
                    if (before > 1.0f) before = 1.0f;
                    if (before < -1.0f) before = -1.0f;
                    x = Mathf.Asin(before) * Mathf.Rad2Deg;
                    if (m22 < 0)
                    {
                        x = 180 - x;
                    }
                }
                else
                {
                    float m10 = 2 * (qx * qy - qw * qz);
                    float m11 = 1 - 2 * (qx * qx + qz * qz);

                    if (m10 > 1.0f) m10 = 1.0f;
                    if (m10 < -1.0f) m10 = -1.0f;
                    if (m11 > 1.0f) m11 = 1.0f;
                    if (m11 < -1.0f) m11 = -1.0f;

                    x = 0f;
                    z = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg;
                }
            }
            else
            {
                // m00:1-2y^2-2z^2 m01:2xy+2wz     m02:2xz-2wy
                // m10:2xy-2wz     m11:1-2x^2-2z^2 m12:2yz+2wx
                // m20:2xz+2wy     m21:2yz-2wx     m22:1-2x^2-2y^2
                // Y->X->Z

                float qx = bone.localRotation.x;
                float qy = bone.localRotation.y;
                float qz = bone.localRotation.z;
                float qw = bone.localRotation.w;

                float m02 = 2 * (qx * qz - qw * qy);
                float m10 = 2 * (qx * qy - qw * qz);
                float m11 = 1 - 2 * (qx * qx + qz * qz);
                float m12 = 2 * (qy * qz + qw * qx);
                float m20 = 2 * (qx * qz + qw * qy);
                float m21 = 2 * (qy * qz - qw * qx);
                float m22 = 1 - 2 * (qx * qx + qy * qy);

                if (m12 > 1.0f) m12 = 1.0f;
                if (m12 < -1.0f) m12 = -1.0f;
                if (m11 > 1.0f) m11 = 1.0f;
                if (m11 < -1.0f) m11 = -1.0f;
                if (m10 > 1.0f) m10 = 1.0f;
                if (m10 < -1.0f) m10 = -1.0f;
                if (m22 > 1.0f) m22 = 1.0f;
                if (m22 < -1.0f) m22 = -1.0f;

                x = Mathf.Asin(m12) * Mathf.Rad2Deg;
                if (System.Math.Floor(Mathf.Cos(x * Mathf.Deg2Rad) * 10000) / 10000 != 0f)
                {

                    z = Mathf.Atan2(-m10, m11) * Mathf.Rad2Deg;
                    float before = -m02 / Mathf.Cos(x * Mathf.Deg2Rad);
                    if (before > 1.0f) before = 1.0f;
                    if (before < -1.0f) before = -1.0f;

                    y = Mathf.Asin(before) * Mathf.Rad2Deg;
                    if (m22 < 0)
                    {
                        y = 180 - y;
                    }

                }
                else
                {
                    float m00 = 1 - 2 * (qy * qy + qz * qz);
                    float m01 = 2 * (qx * qy + qw * qz);

                    if (m01 > 1.0f) m01 = 1.0f;
                    if (m01 < -1.0f) m01 = -1.0f;
                    if (m00 > 1.0f) m00 = 1.0f;
                    if (m00 < -1.0f) m00 = -1.0f;

                    y = 0f;
                    z = Mathf.Atan2(m01, m00) * Mathf.Rad2Deg;

                }
            }

        }

        public static int GetDataCount()
        {
            return Index / 45;
        }

        public static void LoadFingerPose(string configDir)
        {
            string txtFileName = configDir + "EMES_FingerPose.txt";
            //ファイルがない場合、デフォルトのデータを使用する
            if (!File.Exists(txtFileName))
            {
                Debuginfo.Warning("ファイルが見つかりません、デフォルトのデータを使用する" + txtFileName, 1);
            }
            else
            {
                Left = new float[maxDataCount];
                Index = 0;
                using (var reader = new StreamReader(txtFileName))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] data = line.Split(',');
                        for(int j=0; j < 45; j++)
                        {
                            Left[Index] = float.Parse(data[j]);
                            Index++;

                            if(Index >= maxDataCount)
                            {
                                Debuginfo.Warning("データが1MBを超えている、ブレーク", 0);
                                break;
                            }
                        }
                    }
                    Left[Index + 1] = 0f;
                }

                Debuginfo.Log(txtFileName + "ロードが完了しました、有効なデータ 「" + (Index/45).ToString() + "」", 1);
            }
        }
    }
}
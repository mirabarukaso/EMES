using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.EnhancedMaidEditScene.Plugin
{
    //キー入力でキー名がややこしいやつの対策
    public static class FlexKeycode
    {

        static Dictionary<string, KeyCode> dicKey = new Dictionary<string, KeyCode>()
        {
            {"0", KeyCode.Keypad0},
            {"1", KeyCode.Keypad1},
            {"2", KeyCode.Keypad2},
            {"3", KeyCode.Keypad3},
            {"4", KeyCode.Keypad4},
            {"5", KeyCode.Keypad5},
            {"6", KeyCode.Keypad6},
            {"7", KeyCode.Keypad7},
            {"8", KeyCode.Keypad8},
            {"9", KeyCode.Keypad9},
            {"a", KeyCode.A},
            {"b", KeyCode.B},
            {"c", KeyCode.C},
            {"d", KeyCode.D},
            {"e", KeyCode.E},
            {"f", KeyCode.F},
            {"g", KeyCode.G},
            {"h", KeyCode.H},
            {"i", KeyCode.I},
            {"j", KeyCode.J},
            {"k", KeyCode.K},
            {"l", KeyCode.L},
            {"m", KeyCode.M},
            {"n", KeyCode.N},
            {"o", KeyCode.O},
            {"p", KeyCode.P},
            {"q", KeyCode.Q},
            {"r", KeyCode.R},
            {"s", KeyCode.S},
            {"t", KeyCode.T},
            {"u", KeyCode.U},
            {"v", KeyCode.V},
            {"w", KeyCode.W},
            {"x", KeyCode.X},
            {"y", KeyCode.Y},
            {"z", KeyCode.Z},
            {"f1",KeyCode.F1},
            {"f2",KeyCode.F2},
            {"f3",KeyCode.F3},
            {"f4",KeyCode.F4},
            {"f5",KeyCode.F5},
            {"f6",KeyCode.F6},
            {"f7",KeyCode.F7},
            {"f8",KeyCode.F8},
            {"f9",KeyCode.F9},
            {"f10",KeyCode.F10},
            {"f11",KeyCode.F11},
            {"f12",KeyCode.F12},
            {"capslock",KeyCode.CapsLock},
            {"caps lock",KeyCode.CapsLock},
            {"backspace",KeyCode.Backspace },
            {"back space",KeyCode.Backspace },
            {"↓",KeyCode.DownArrow },
            {"down",KeyCode.DownArrow },
            {"downarrow",KeyCode.DownArrow },
            {"down arrow",KeyCode.DownArrow },
            {"↑",KeyCode.UpArrow },
            {"up",KeyCode.UpArrow },
            {"uparrow",KeyCode.UpArrow },
            {"up arrow",KeyCode.UpArrow },
            {"←",KeyCode.LeftArrow },
            {"left",KeyCode.LeftArrow },
            {"leftarrow",KeyCode.LeftArrow },
            {"left arrow",KeyCode.LeftArrow },
            {"→",KeyCode.RightArrow },
            {"right",KeyCode.RightArrow },
            {"rightarrow",KeyCode.RightArrow },
            {"right arrow",KeyCode.RightArrow },
            {"alt",KeyCode.LeftAlt },
            {"leftalt",KeyCode.LeftAlt },
            {"left alt",KeyCode.LeftAlt },
            {"rightalt",KeyCode.RightAlt },
            {"right alt",KeyCode.RightAlt },
            {"shift",KeyCode.LeftShift },
            {"leftshift",KeyCode.LeftShift },
            {"left shift",KeyCode.LeftShift },
            {"rightshift",KeyCode.RightShift },
            {"right shift",KeyCode.RightShift },
            {"control",KeyCode.LeftControl },
            {"leftcontrol",KeyCode.LeftControl },
            {"left control",KeyCode.LeftControl },
            {"rightcontrol",KeyCode.RightControl },
            {"right control",KeyCode.RightControl },
            {"ctrl",KeyCode.LeftControl },
            {"left ctrl",KeyCode.LeftControl },
            {"rightctrl",KeyCode.RightControl },
            {"right ctrl",KeyCode.RightControl },
            {"numlock", KeyCode.Numlock },
            {"num lock", KeyCode.Numlock },
            {"pageup", KeyCode.PageUp },
            {"page up", KeyCode.PageUp },
            {"pagedown", KeyCode.PageDown },
            {"page down", KeyCode.PageDown },
            {"escape",KeyCode.Escape},
            {"esc",KeyCode.Escape},
            {"home",KeyCode.Home},
            {"end",KeyCode.End},
            {"insert",KeyCode.Insert},
            {"ins",KeyCode.Insert},
            {"delete",KeyCode.Delete},
            {"del",KeyCode.Delete},
            {"-",KeyCode.Minus},
            {"=",KeyCode.Equals},
            {"tab", KeyCode.Tab},
            {"space", KeyCode.Space}
        };

        public static bool GetKeyDown(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKeyDown(dicKey[key]) : Input.GetKeyDown(key);
        }
        public static bool GetKeyUp(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKeyUp(dicKey[key]) : Input.GetKeyUp(key);
        }
        public static bool GetKey(string key)
        {
            return dicKey.ContainsKey(key) ? Input.GetKey(dicKey[key]) : Input.GetKey(key);
        }

        public static bool GetMultipleKeyUp(string key)
        {
            if (false == key.Contains("+"))
            {
                return GetKeyUp(key);
            }

            string[] keys = key.Split('+');
            bool ret = true;
            foreach (string k in keys)
            {
                if (true == k.Contains("h_"))
                {
                    ret &= GetKey(k.Split('_')[1]);
                }
                else
                {
                    ret &= GetKeyUp(k);
                }
            }
            return ret;
        }

        public static bool GetMultipleKeyDown(string key)
        {
            if (false == key.Contains("+"))
                return GetKey(key);

            string[] keys = key.Split('+');
            bool ret = true;
            foreach(string k in keys)
            {
                if (true == k.Contains("h_"))
                {
                    ret &= GetKey(k.Split('_')[1]);
                }
                else
                {
                    ret &= GetKeyDown(k);
                }
            }
            return ret;
        }

        public static bool GetMultipleKey(string key)
        {
            if (false == key.Contains("+"))
                return GetKey(key);

            string[] keys = key.Split('+');
            bool ret = true;
            foreach (string k in keys)
            {
                if (true == k.Contains("h_"))
                {
                    ret &= GetKey(k.Split('_')[1]);
                }
                else
                {
                    ret &= GetKey(k);
                }
            }
            return ret;
        }

        public static bool ExistKey(string key)
        {
            if (false == key.Contains("+"))
                return dicKey.ContainsKey(key);

            if(true == key.Contains("alt") && true == key.Contains("f4"))
            {
                Debuginfo.Warning("(=ﾟωﾟ)ﾉ　Alt+F4 ", 0);
                return false;
            }

            if (true == key.Contains("ctrl") && true == key.Contains("alt") && true == key.Contains("del"))
            {
                Debuginfo.Warning("(∩´∀｀)∩　Ctrl+Alt+Delete", 0);
                return false;
            }

            string[] keys = key.Split('+');
            bool ret = true;
            foreach (string k in keys)
            {
#if DEBUG
                Debuginfo.Log("Flexkey Checking " + k, 2);
#endif
                if (true == k.Contains("h_"))
                {
                    ret &= dicKey.ContainsKey(k.Split('_')[1]);
                }
                else
                {
                    ret &= dicKey.ContainsKey(k);
                }
#if DEBUG
                Debuginfo.Log("Flexkey ret = " + ret, 2);
#endif
            }
            return ret;
        }
    }
}


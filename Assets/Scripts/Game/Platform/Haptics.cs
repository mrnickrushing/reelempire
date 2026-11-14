using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace ReelEmpire.Game
{
    /// <summary>The phone's taptic engine: a light tap, a firmer one, and the success pattern. Settings can turn it off.</summary>
    public static class Haptics
    {
        const string OffKey = "re_haptics_off";
        public static bool On
        {
            get { return PlayerPrefs.GetInt(OffKey, 0) == 0; }
            set { PlayerPrefs.SetInt(OffKey, value ? 0 : 1); PlayerPrefs.Save(); }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void REHapticImpact(int style);
        [DllImport("__Internal")] static extern void REHapticSuccess();
        public static void Light() { if (On) REHapticImpact(0); }
        public static void Medium() { if (On) REHapticImpact(1); }
        public static void Success() { if (On) REHapticSuccess(); }
#else
        public static void Light() { }
        public static void Medium() { }
        public static void Success() { }
#endif
    }
}

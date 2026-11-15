using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>Shares what is on screen (a record catch's card) through the iPhone's share sheet. Elsewhere it saves
    /// the picture to the game's folder and says where.</summary>
    public static class Share
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void REShareImage(string path, string text);
#endif

        /// <summary>Waits for the frame to be drawn, saves it, then opens the share sheet with <paramref name="text"/>.</summary>
        public static IEnumerator Screen(string text, System.Action<string> done = null)
        {
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            string path = Path.Combine(Application.temporaryCachePath, "reel-empire-catch.png");
            File.WriteAllBytes(path, shot.EncodeToPNG());
            Object.Destroy(shot);
#if UNITY_IOS && !UNITY_EDITOR
            REShareImage(path, text);
            done?.Invoke(null);
#else
            done?.Invoke(path);
#endif
        }
    }
}

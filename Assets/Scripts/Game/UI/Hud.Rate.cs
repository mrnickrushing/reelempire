using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// Asking for an App Store rating, once: after a milestone level (12 stars and up, nothing else on screen) a small
    /// card asks "Enjoying Reel Empire?". RATE IT brings up Apple's own rating sheet; either answer means it never
    /// asks again on this device. Settings keeps a Rate row that opens the App Store page.
    /// </summary>
    public sealed partial class Hud
    {
        const string RateAskedPref = "re_rate_asked";
        const int RateFromStars = 12;
        public const string ReviewUrl = "https://apps.apple.com/app/id0000000000?action=write-review";

        float _rateIn = -1;
        RectTransform _rateCard;

        static bool RateAsked { get { return PlayerPrefs.GetInt(RateAskedPref, 0) == 1; } }

        static void MarkRateAsked() { PlayerPrefs.SetInt(RateAskedPref, 1); PlayerPrefs.Save(); }

        /// <summary>A good moment just happened (a milestone level): ask once the screen is clear again.</summary>
        void RateSoon()
        {
            if (RateAsked || S.Stars < RateFromStars) return;
            _rateIn = 2.5f;
        }

        void UpdateRate()
        {
            if (_rateIn < 0) return;
            if (ModalOpen || SheetOpen || ScreenOpen) return;      // wait for cards and the sheet to be out of the way
            if ((_rateIn -= Time.unscaledDeltaTime) > 0) return;
            _rateIn = -1;
            if (RateAsked) return;
            MarkRateAsked();
            ShowRateCard();
        }

        void ShowRateCard()
        {
            _rateCard = Ui.Img("Rate", _root, new Color(0.05f, 0.15f, 0.25f, 0.6f)).rectTransform;
            _rateCard.Fill(); _rateCard.GetComponent<Image>().raycastTarget = true;
            var box = Ui.ArtImg("Box", _rateCard, "panel", Color.white, 60).rectTransform;
            box.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 520));
            var star = Ui.ArtImg("Star", box, "star", Color.white, 0); star.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(150, 150));
            Ui.Label("T", box, "ENJOYING REEL EMPIRE?", 58, Ui.Ink).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -96), new Vector2(840, 80));
            Ui.Label("B", box, "If you like running your harbour, a quick rating on the App Store helps other players find it.", 36, Ui.Dim, TextAnchor.UpperCenter, false)
                .rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -190), new Vector2(800, 160));
            var yes = Ui.Button("Yes", box, "RATE IT", Ui.Green, () => { CloseRateCard(); RequestRating(); }, 46);
            ((RectTransform)yes.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 50), new Vector2(400, 120));
            var no = Ui.Button("No", box, "NO THANKS", Ui.Off, CloseRateCard, 46);
            ((RectTransform)no.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 50), new Vector2(400, 120));
            Sfx.Play("open");
        }

        void CloseRateCard()
        {
            if (_rateCard != null) Destroy(_rateCard.gameObject);
            _rateCard = null;
        }

        /// <summary>Apple's in-app rating sheet (the App Store page where that isn't available).</summary>
        static void RequestRating()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (UnityEngine.iOS.Device.RequestStoreReview()) return;
#endif
            Application.OpenURL(ReviewUrl);
        }

        /// <summary>The Settings row: straight to the App Store's review page.</summary>
        static void OpenStoreReview()
        {
            MarkRateAsked();
            Application.OpenURL(ReviewUrl);
        }
    }
}

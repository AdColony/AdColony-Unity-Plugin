using System;
using System.Collections;
using UnityEngine;

namespace AdColony
{
    public class AdColonyAdView
    {
        public string ZoneId;
        public AdPosition adPosition;

        // ---------------------------------------------------------------------------

#region Internal Methods - do not call these
        public string Id;

        public AdColonyAdView(Hashtable values)
        {
            UpdateValues(values);
        }

        public void UpdateValues(Hashtable values)
        {
            if (values != null)
            {
                if (values.ContainsKey("zone_id"))
                {
                    ZoneId = values["zone_id"] as string;
                }
                if (values.ContainsKey("id"))
                {
                    Id = values["id"] as string;
                }
                if (values.ContainsKey("position"))
                {
                    int adpositionTemp = Int32.Parse(values["position"] as string);
                    adPosition = getAdPosition(adpositionTemp);
                }
            }
        }

        private AdPosition getAdPosition(int position)
        {
            switch (position) {

                case 0:
                    return AdPosition.Top;
                case 1:
                    return AdPosition.Bottom;
                case 2:
                    return AdPosition.TopLeft;
                case 3:
                    return AdPosition.TopRight;
                case 4:
                    return AdPosition.BottomLeft;
                case 5:
                    return AdPosition.BottomRight;
                case 6:
                    return AdPosition.Center;
                default:
                    return AdPosition.Bottom;
            }
        }

        ~AdColonyAdView()
        {
            if (IsValid())
            {
                Ads.SharedGameObject.EnqueueAction(() => { Ads.DestroyAd(Id); });
            }
        }

        public void DestroyAdView() {
            Debug.Log("AdColony.AdColonyAdView.DestroyAdView called.");
            Ads.DestroyAdView(Id);
        }

        public void ShowAdView()
        {
            Debug.Log("AdColony.AdColonyAdView.show called.");
            Ads.ShowAdView(Id);
        }

        public void HideAdView()
        {
            Debug.Log("AdColony.AdColonyAdView.hide called.");
            Ads.HideAdView(Id);
        }

        private bool IsValid()
        {
            return !System.String.IsNullOrEmpty(Id);
        }

#endregion
    }
}
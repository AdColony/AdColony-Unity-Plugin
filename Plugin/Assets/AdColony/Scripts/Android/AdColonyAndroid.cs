using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AdColony {
#if UNITY_ANDROID
    public class AdsAndroid : IAds {

        private AndroidJavaClass _plugin;
        private AndroidJavaClass _pluginWrapper;

        public AdsAndroid(string managerName) {
            // This is the raw AdColony.jar
            _plugin = new AndroidJavaClass("com.adcolony.sdk.AdColony");

            // This is a separate wrapper to manage type conversions and callbacks
            _pluginWrapper = new AndroidJavaClass("com.adcolony.unityplugin.UnityADCAds");
            _pluginWrapper.CallStatic("setManagerName", managerName);
        }

        public void Configure(string appId, AppOptions appOptions, params string[] zoneIds) {
            Hashtable values = new Hashtable();
            values["app_id"] = appId;
            values["zone_ids"] = new ArrayList(zoneIds);
            if (appOptions == null) {
                appOptions = new AppOptions();
            }
            appOptions.SetOption("plugin_version", AdColony.Constants.AdapterVersion);
            values["app_options"] = appOptions.ToHashtable();

            string json = AdColonyJson.Encode(values);
            _pluginWrapper.CallStatic("configure", json);
        }

        public string GetSDKVersion() {
            return _plugin.CallStatic<string>("getSDKVersion");
        }

        public void RequestInterstitialAd(string zoneId, AdOptions adOptions) {
            Hashtable values = new Hashtable();
            values["zone_id"] = zoneId;
            if (adOptions != null) {
                values["ad_options"] = adOptions.ToHashtable();
            }

            string json = AdColonyJson.Encode(values);
            _pluginWrapper.CallStatic("requestInterstitialAd", json);
        }

        public void RequestAdView(string zoneId,AdSize adSize, AdPosition adPosition, AdOptions adOptions)
        {
            Hashtable values = new Hashtable();
            values["zone_id"] = zoneId;
            values["ad_size"] = (Int32)adSize;
            values["ad_position"] = (Int32)adPosition;
            if (adOptions != null)
            {
                values["ad_options"] = adOptions.ToHashtable();
            }

            string json = AdColonyJson.Encode(values);
            _pluginWrapper.CallStatic("requestAdView", json);
        }

        public Zone GetZone(string zoneId) {
            string zoneJson = _pluginWrapper.CallStatic<string>("getZone", zoneId);
            Hashtable zoneValues = (AdColonyJson.Decode(zoneJson) as Hashtable);
            return new Zone(zoneValues);
        }

        public string GetUserID() {
            AppOptions appOptions = GetAppOptions();
            if (appOptions != null) {
                return appOptions.UserId;
            }
            return null;
        }

        public void SetAppOptions(AppOptions appOptions) {
            string json = null;
            if (appOptions != null) {
                json = appOptions.ToJsonString();
            }
            _pluginWrapper.CallStatic("setAppOptions", json);
        }

        public AppOptions GetAppOptions() {
            string appOptionsJson = _pluginWrapper.CallStatic<string>("getAppOptions");
            Hashtable appOptionsValues = new Hashtable();
            if (appOptionsJson != null) {
                appOptionsValues = (AdColonyJson.Decode(appOptionsJson) as Hashtable);
            }
            return new AppOptions(appOptionsValues);
        }

        public void SendCustomMessage(string type, string content) {
            _pluginWrapper.CallStatic("sendCustomMessage", type, content);
        }

        public void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode) {
            _plugin.CallStatic<bool>("notifyIAPComplete", productId, transactionId, currencyCode, (double)purchasePriceMicro / 1000000.0);
        }

        public void ShowAd(InterstitialAd ad) {
            _pluginWrapper.CallStatic("showAd", ad.Id);
        }

        public void CancelAd(InterstitialAd ad) {
            _pluginWrapper.CallStatic("cancelAd", ad.Id);
        }

        public void DestroyAd(string id) {
            _pluginWrapper.CallStatic("destroyAd", id);
        }

        public void DestroyAdView(string id)
        {
            Debug.Log("AdColony.Android.DestroyAdView called.");
            _pluginWrapper.CallStatic("destroyAdView", id);
        }

        public void ShowAdView(string id)
        {
            Debug.Log("AdColony.Android.show called.");
            _pluginWrapper.CallStatic("show", id);
        }

        public void HideAdView(string id)
        {
            Debug.Log("AdColony.Android.hide called.");
            _pluginWrapper.CallStatic("hide", id);
        }

        public void RegisterForCustomMessage(string type) {
            _pluginWrapper.CallStatic("registerForCustomMessage", type);
        }

        public void UnregisterForCustomMessage(string type) {
            _pluginWrapper.CallStatic("unregisterForCustomMessage", type);
        }
    }
#endif
}

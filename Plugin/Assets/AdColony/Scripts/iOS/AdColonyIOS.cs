using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AdColony {
#if UNITY_IOS
    public class AdsIOS : IAds {
        [DllImport ("__Internal")] private static extern void _AdcSetManagerNameAds(string managerName, string adapterVersion);
        [DllImport ("__Internal")] private static extern void _AdcConfigure(string appId, string appOptionsJson, int zoneIdsCount, string[] zoneIds);
        [DllImport ("__Internal")] private static extern string _AdcGetSDKVersion();
        [DllImport ("__Internal")] private static extern void _AdcRequestInterstitialAd(string zoneId, string adOptionsJson);
        [DllImport ("__Internal")] private static extern string _AdcGetZone(string zoneId);
        [DllImport ("__Internal")] private static extern string _AdcGetUserID();
        [DllImport ("__Internal")] private static extern void _AdcSetAppOptions(string appOptionsJson);
        [DllImport ("__Internal")] private static extern string _AdcGetAppOptions();
        [DllImport ("__Internal")] private static extern void _AdcSendCustomMessage(string type, string content);
        [DllImport ("__Internal")] private static extern void _AdcLogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode);
        [DllImport ("__Internal")] private static extern void _AdcShowInterstitialAd(string id);
        [DllImport ("__Internal")] private static extern void _AdcCancelInterstitialAd(string id);
        [DllImport ("__Internal")] private static extern void _AdcDestroyInterstitialAd(string id); 
        [DllImport("__Internal")] private static extern void _AdcRequestAdView(string zoneId, AdSize adSize, AdPosition adPosition, string adOptions);
        [DllImport ("__Internal")] private static extern void _AdcDestroyAdView(string id); 
        [DllImport("__Internal")] private static extern void _AdcShowAdView(string id);
        [DllImport("__Internal")] private static extern void _AdcHideAdView(string id);

        public AdsIOS(string managerName) {
            _AdcSetManagerNameAds(managerName, Constants.AdapterVersion);
        }

        public void Configure(string appId, AppOptions options, params string[] zoneIds) {
            string optionsJson = null;
            if (options != null) {
                optionsJson = options.ToJsonString();
            }

            int length = 0;
            if (zoneIds != null) {
                length = zoneIds.Length;
            }

            _AdcConfigure(appId, optionsJson, length, zoneIds);
        }

        public string GetSDKVersion() {
            return _AdcGetSDKVersion();
        }

        public void RequestInterstitialAd(string zoneId, AdOptions options) {
            string optionsJson = null;
            if (options != null) {
                optionsJson = options.ToJsonString();
            }

            _AdcRequestInterstitialAd(zoneId, optionsJson);
        }

        public void RequestAdView(string zoneId,AdSize adSize, AdPosition adPosition, AdOptions adOptions)
        {
            string optionsJson = null;
            if (adOptions != null) {
                optionsJson = adOptions.ToJsonString();
            }

            _AdcRequestAdView(zoneId, adSize, adPosition, optionsJson);
        }

        public Zone GetZone(string zoneId) {
            string zoneJson = _AdcGetZone(zoneId);
            Hashtable zoneValues = (AdColonyJson.Decode(zoneJson) as Hashtable);
            return new Zone(zoneValues);
        }

        public string GetUserID() {
            return _AdcGetUserID();
        }

        public void SetAppOptions(AppOptions options) {
            string optionsJson = null;
            if (options != null) {
                optionsJson = options.ToJsonString();
            }

            _AdcSetAppOptions(optionsJson);
        }

        public AppOptions GetAppOptions() {
            string optionsJson = _AdcGetAppOptions();
            Hashtable optionsValues = new Hashtable();
            if (optionsJson != null) {
                optionsValues = (AdColonyJson.Decode(optionsJson) as Hashtable);
            }
            return new AppOptions(optionsValues);
        }

        public void SendCustomMessage(string type, string content) {
            _AdcSendCustomMessage(type, content);
        }

        public void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode) {
            _AdcLogInAppPurchase(transactionId, productId, purchasePriceMicro, currencyCode);
        }

        public void ShowAd(InterstitialAd ad) {
            _AdcShowInterstitialAd(ad.Id);
        }

        public void CancelAd(InterstitialAd ad) {
            _AdcCancelInterstitialAd(ad.Id);
        }

        public void DestroyAd(string id) {
            _AdcDestroyInterstitialAd(id);
        }

        public void DestroyAdView(string id)
        {
            Debug.Log("AdColony.AdColonyIOS.DestroyAdView called.");
            _AdcDestroyAdView(id);
        }

        public void ShowAdView(string id)
        {
            Debug.Log("AdColony.AdColonyIOS.show called.");
            _AdcShowAdView(id);
        }
        public void HideAdView(string id)
        {
            Debug.Log("AdColony.AdColonyIOS.hide called.");
            _AdcHideAdView(id);
        }
    }
#endif
}

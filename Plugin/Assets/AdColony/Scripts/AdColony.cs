using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AdColony
{

    /// <summary>
    /// Main class for Ads within AdColony SDK
    /// </summary>
    /// <example>
    /// <code>
    ///    AdColony.InterstitialAd _ad = null;
    ///    bool _configured = false;
    ///
    ///    void ConfigureAds() {
    ///        AdColony.Ads.Configure("your_app_id", null, "your_zone_1", "your_zone_2");
    ///
    ///        AdColony.Ads.OnConfigurationCompleted += (List<AdColony.Zone> zones) => {
    ////            if (zones == null || zones.Count <= 0) {
    ////                _configured = false;
    ///            } else {
    ///                _configured = true;
    ///            }
    ///        };
    ///
    ///        AdColony.Ads.OnRequestInterstitial += (AdColony.InterstitialAd ad) => {
    ///            _ad = ad;
    ///        };
    ///    }
    ///
    ///    void RequestAd() {
    ///        if (_configured) {
    ////            AdColony.Ads.RequestInterstitialAd("your_zone_1", null);
    ////        }
    ///    }
    ////
    ///    void PlayAd() {
    ////        if (_ad != null) {
    ///            AdColony.Ads.ShowAd(_ad);
    ///        }
    ///    }
    /// </code>
    /// </example>
    /// 
    public class Ads : MonoBehaviour
    {
        // ------------------------------------Common API--------------------------------------

        /// <summary>
        /// Configures AdColony specifically for your app; required for usage of the rest of the API.
        /// </summary>
        /// This method returns immediately; any long-running work such as network connections are performed in the background.
        /// AdColony does not begin preparing ads for display or performing reporting until after it is configured by your app.
        /// The required appId and zoneIds parameters for this method can be created and retrieved at the [Control Panel](http://clients.adcolony.com).
        /// If either of these are `null`, app will be unable to play ads and AdColony will only provide limited reporting and install-tracking functionality.
        /// You should not start requesting ads until `OnConfigurationCompleted` event has fired.
        /// If there is a configuration error, the set of zones passed to the completion handler will be null.
        /// <param name="appId">The AdColony app ID for your app.</param>
        /// <param name="zoneIds">An array of at least one AdColony zone ID string.</param>
        /// <param name="options">(optional) Configuration options for your app.</param>
        /// <see cref="AppOptions" />
        /// <see cref="Zone" />
        /// <see cref="OnConfigurationCompleted" />
        public static void Configure(string appId, AppOptions options, params string[] zoneIds)
        {
            // Using SharedInstance to make sure the MonoBehaviour is instantiated
            if (SharedInstance == null)
            {
                Debug.LogWarning(Constants.AdsMessageSDKUnavailable);
                return;
            }
            SharedInstance.Configure(appId, options, zoneIds);
            _initialized = true;
        }

        /// <summary>
        /// Requests an AdColonyInterstitial.
        /// </summary>
        /// This method returns immediately, before the ad request completes.
        /// If the request is successful, an AdColonyInterstitial object will be passed to the success block.
        /// If the request is unsuccessful, the failure block will be called and an AdColonyAdRequestError will be passed to the handler.
        /// <param name="zoneId">The zone identifier string indicating which zone the ad request is for.</param>
        /// <param name="adOptions">An AdOptions object used to set configurable aspects of the ad request.</param>
        /// <see cref="AdOptions" />
        /// <see cref="InterstitialAd" />
        /// <see cref="OnRequestInterstitial" />
        /// <see cref="OnRequestInterstitialFailedWithZone" />
        public static void RequestInterstitialAd(string zoneId, AdOptions adOptions)
        {
            if (IsInitialized())
            {
                SharedInstance.RequestInterstitialAd(zoneId, adOptions);
            }
        }

        public static void RequestAdView(string zoneId, AdSize adSize, AdPosition adPosition, AdOptions adOptions)
        {
            if (Ads.IsInitialized())
            {
                SharedInstance.RequestAdView(zoneId, adSize, adPosition, adOptions);
            }
        }

        /// <summary>
        /// Shows an interstitial ad.
        /// </summary>
        /// <param name="ad">The interstitial ad returned from RequestInterstitialAd.</param>
        /// <see cref="OnOpened" />
        /// <see cref="OnClosed" />
        /// <see cref="OnExpiring" />
        /// <see cref="OnAudioStarted" />
        /// <see cref="OnAudioStopped" />
        public static void ShowAd(InterstitialAd ad)
        {
            if (IsInitialized())
            {
                if (ad != null)
                {
                    SharedInstance.ShowAd(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorNullAd);
                }
            }
        }

        /// <summary>
        /// Sets the current, global set of AppOptions.
        /// </summary>
        /// Use the object's option-setting methods to configure currently-supported options.
        /// <param name="options">The AppOptions object to be used for configuring global options such as a custom user identifier.</param>
        /// <see cref="AppOptions" />
        public static void SetAppOptions(AppOptions options)
        {
            if (IsInitialized())
            {
                SharedInstance.SetAppOptions(options);
            }
        }

        /// <summary>
        /// Returns the current, global set of AppOptions.
        /// </summary>
        /// Use this method to obtain the current set of app options used to configure SDK behavior.
        /// If no options object has been set, this method will return `null`.
        /// <returns>The current AppOptions object being used by the SDK.</returns>
        /// <see cref="AppOptions" />
        public static AppOptions GetAppOptions()
        {
            if (IsInitialized())
            {
                return SharedInstance.GetAppOptions();
            }
            return null;
        }

        /// <summary>
        /// Retrieve a string-based representation of the SDK version.
        /// </summary>
        /// The returned string will be in the form of "<Major Version>.<Minor Version>.<External Revision>.<Internal Revision>"
        /// <returns>The current AdColony SDK version string.</returns>
        public static string GetSDKVersion()
        {
            if (IsInitialized())
            {
                return SharedInstance.GetSDKVersion();
            }
            return null;
        }

        // Asynchronously request an Interstitial Ad
        // see OnRequestInterstitial, OnRequestInterstitialFailed

        /// <summary>
        /// Retrieves a Zone object.
        /// </summary>
        /// AdColonyZone objects aggregate informative data about unique AdColony zones such as their identifiers, whether or not they are configured for rewards, etc.
        /// AdColony zone IDs can be created and retrieved at the [Control Panel](http://clients.adcolony.com).
        /// <param name="zoneId">The AdColony zone identifier string indicating which zone to return.</param>
        /// <returns>A Zone object. Returns `null` if an invalid zone ID is passed.</returns>
        /// <see cref="Zone" />
        public static Zone GetZone(string zoneId)
        {
            if (IsInitialized())
            {
                return SharedInstance.GetZone(zoneId);
            }
            return null;
        }

        /// <summary>
        /// Retrieves a custom identifier for the current user if it has been set.
        /// </summary>
        /// This is an arbitrary, application-specific identifier string for the current user.
        /// To configure this identifier, use the `AppOptions.UserId` property of the object passed to `Configure()`.
        /// Note that if this method is called before `Configure()`, it will return an empty string.
        /// <returns>The identifier for the current user.</returns>
        /// <see cref="AppOptions" />
        public static string GetUserID()
        {
            if (IsInitialized())
            {
                return SharedInstance.GetUserID();
            }
            return null;
        }

        /// <summary>
        /// Sends a custom message to the AdColony SDK.
        /// </summary>
        /// Use this method to send custom messages to the AdColony SDK.
        /// <param name="type">The type of the custom message. Must be 128 characters or less.</param>
        /// <param name="content">The content of the custom message. Must be 1024 characters or less.</param>
        /// <param name="reply">A block of code to be executed when a reply is sent to the custom message.</param>
        public static void SendCustomMessage(string type, string content)
        {
            if (IsInitialized())
            {
                SharedInstance.SendCustomMessage(type, content);
            }
        }

        /// <summary>
        /// Reports IAPs within your application.
        /// </summary>
        /// Note that this API can be used to report standard IAPs as well as those triggered by AdColony’s IAP Promo (IAPP) advertisements.
        /// Leveraging this API will improve overall ad targeting for your application.
        /// <param name="transactionId">An string representing the unique SKPaymentTransaction identifier for the IAP. Must be 128 chars or less.</param>
        /// <param name="productId">An string identifying the purchased product. Must be 128 chars or less.</param>
        /// <param name="purchasePriceMicro">(optional) Total price of the items purchased in micro-cents, $0.99 = 990000</param>
        /// <param name="currencyCode">(optional) An string indicating the real-world, three-letter ISO 4217 (e.g. USD) currency code of the transaction.</param>
        public static void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode)
        {
            if (IsInitialized())
            {
                SharedInstance.LogInAppPurchase(transactionId, productId, purchasePriceMicro, currencyCode);
            }
        }

        /// <summary>
        /// Cancels the interstitial and returns control back to the application.
        /// </summary>
        /// Call this method to cancel the interstitial. Canceling interstitials before they finish will diminish publisher revenue.
        /// Note this has no affect on Android.
        /// <param name="ad">The interstitial ad returned from RequestInterstitialAd.</param>
        public static void CancelAd(InterstitialAd ad)
        {
            if (IsInitialized())
            {
                if (ad != null)
                {
                    SharedInstance.CancelAd(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorNullAd);
                }
            }
        }

        /// <summary>
        /// Interface for PIE (Post-Install Events)
        /// </summary>
        public static IEventTracker GetEventTracker()
        {
            IEventTracker ret = null;
            if (SharedGameObject != null)
            {
                ret = SharedGameObject._eventTracker;
            }
            if (ret == null)
            {
                Debug.LogError(Constants.AdsMessageErrorInvalidImplementation);
            }
            return ret;
        }

        #region Internal Methods - do not call these

        public static Ads SharedGameObject
        {
            get
            {
                if (!_sharedGameObject)
                {
                    _sharedGameObject = (Ads)FindObjectOfType(typeof(Ads));
                }

                if (!_sharedGameObject)
                {
                    GameObject singleton = new GameObject();
                    _sharedGameObject = singleton.AddComponent<Ads>();
                    singleton.name = Constants.AdsManagerName;
                    DontDestroyOnLoad(singleton);

                    if (_sharedGameObject._sharedInstance != null)
                    {
                        Debug.LogWarning(Constants.AdsMessageAlreadyInitialized);
                    }
                    else
                    {
                        _sharedGameObject._sharedInstance = null;
        #if UNITY_EDITOR

        #elif UNITY_ANDROID
                        _sharedGameObject._sharedInstance = new AdsAndroid(singleton.name);
                        _sharedGameObject._eventTracker = new EventTrackerAndroid();
        #elif UNITY_IOS
                        _sharedGameObject._sharedInstance = new AdsIOS(singleton.name);
                        _sharedGameObject._eventTracker = new EventTrackerIOS();
        #elif UNITY_WP8

        #elif UNITY_METRO

        #endif
                    }
                }

                return _sharedGameObject;
            }
        }

        protected static IAds SharedInstance
        {
            get
            {
                IAds ret = null;
                Ads gameObject = SharedGameObject;
                if (gameObject != null)
                {
                    ret = gameObject._sharedInstance;
                }
                if (ret == null)
                {
                    Debug.LogError(Constants.AdsMessageErrorInvalidImplementation);
                }
                return ret;
            }
        }

        private static bool IsSupportedOnCurrentPlatform()
        {
            // Using SharedInstance to make sure the MonoBehaviour is instantiated
            if (SharedInstance == null)
            {
                return false;
            }
            return true;
        }

        protected static bool IsInitialized()
        {
            if (!IsSupportedOnCurrentPlatform())
            {
                return false;
            }
            else if (!_initialized)
            {
                Debug.LogError(Constants.AdsMessageNotInitialized);
                return false;
            }
            return true;
        }

        void Awake()
        {
            if (gameObject == SharedGameObject.gameObject)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (_updateOnMainThreadActions.Count > 0)
            {
                System.Action action;
                do
                {
                    action = null;
                    lock (_updateOnMainThreadActionsLock)
                    {
                        if (_updateOnMainThreadActions.Count > 0)
                        {
                            action = _updateOnMainThreadActions.Dequeue();
                        }
                    }
                    if (action != null)
                    {
                        action.Invoke();
                    }
                } while (action != null);
            }
        }

        public void EnqueueAction(System.Action action)
        {
            lock (_updateOnMainThreadActionsLock)
            {
                _updateOnMainThreadActions.Enqueue(action);
            }
        }

        public static void DestroyAd(String id)
        {
            if (IsInitialized())
            {
                SharedInstance.DestroyAd(id);
            }
        }

        //------------------------------Start Native Intertital-------------------------------------------------------------------------

        /// <summary>
        /// Event that is triggered after a call to Configure has completed.
        /// </summary>
        /// On Android, the Zone objects aren't fully populated by the time this is called, use GetZone() after a delay if required.
        /// If the configuration is not successful, the list of zones will be empty
        public static event Action<List<Zone>> OnConfigurationCompleted;

        /// <summary>
        /// Event that is triggered after a call to RequestInterstitialAd has completed successfully. The InterstitialAd object returned can be used to show an interstitial ad when ready.
        /// </summary>
        public static event Action<InterstitialAd> OnRequestInterstitial;

        /// <summary>
        /// Event triggered after a call to RequestInterstitialAd has failed.
        /// </summary>
        /// <remarks>
        /// DEPRECATED: use OnRequestInterstitialFailedWithZone instead
        /// </remarks>
        public static event Action OnRequestInterstitialFailed;

        /// <summary>
        /// Event triggered after a call to RequestInterstitialAd has failed.
        /// Parameter 1: zoneId
        /// </summary>
        public static event Action<string> OnRequestInterstitialFailedWithZone;

        /// <summary>
        /// Event triggered after a custom message is received by the SDK.
        /// Parameter 1: type
        /// Parameter 2: message
        /// </summary>
        public static event Action<string, string> OnCustomMessageReceived;

        /// <summary>
        /// Event triggered after an InterstitialAd is opened.
        /// </summary>
        public static event Action<InterstitialAd> OnOpened;

        /// <summary>
        /// Event triggered after an InterstitialAd is closed.
        /// It's recommended to request a new ad within this callback.
        /// </summary>
        public static event Action<InterstitialAd> OnClosed;

        /// <summary>
        /// Event triggered after an InterstitialAd expires and is no longer valid for playback.
        /// This does not get triggered when the expired flag is set because it has been viewed.
        /// It's recommended to request a new ad within this callback.
        /// </summary>
        public static event Action<InterstitialAd> OnExpiring;

        /// <summary>
        /// Event triggered after an InterstitialAd's audio has started.
        /// </summary>
        public static event Action<InterstitialAd> OnAudioStarted;

        /// <summary>
        /// Event triggered after an InterstitialAd's audio has stopped.
        /// </summary>
        public static event Action<InterstitialAd> OnAudioStopped;

        /// <summary>
        /// Event triggered if action with ad caused the application to background.
        /// </summary>
        public static event Action<InterstitialAd> OnLeftApplication;

        /// <summary>
        /// Event triggered after an InterstitialAd was clicked.
        /// </summary>
        public static event Action<InterstitialAd> OnClicked;

        /// <summary>
        /// Event triggered after the ad triggers an IAP opportunity.
        /// Parameter 1: ad
        /// Parameter 2: IAP product ID
        /// Parameter 3: engagement type
        /// </summary>
        public static event Action<InterstitialAd, string, AdsIAPEngagementType> OnIAPOpportunity;

        // Params: zoneId, success, name, amount
        /// <summary>
        /// Event triggered after V4VC ad has been completed.
        /// Client-side reward implementations should consider incrementing the user's currency balance in this method.
        /// Server-side reward implementations should consider the success parameter and then contact the game server to determine the current total balance for the virtual currency.
        /// Parameter 1: zone ID
        /// Parameter 2: success
        /// Parameter 3: name of reward type
        /// Parameter 4: reward quantity
        /// </summary>
        public static event Action<string, bool, string, int> OnRewardGranted;


        //below methods called from native code for Interstital Ad.
        public void _OnConfigure(string paramJson)
        {
            List<Zone> zoneList = new List<Zone>();
            ArrayList zoneJsonList = (AdColonyJson.Decode(paramJson) as ArrayList);
            if (zoneJsonList == null)
            {
                Debug.LogError("Unable to parse parameters in _OnConfigure, " + (paramJson ?? "null"));
                return;
            }

            foreach (string zoneJson in zoneJsonList)
            {
                Hashtable zoneValues = (AdColonyJson.Decode(zoneJson) as Hashtable);
                zoneList.Add(new Zone(zoneValues));
            }

            if (Ads.OnConfigurationCompleted != null)
            {
                Ads.OnConfigurationCompleted(zoneList);
            }
        }

        public void _OnRequestInterstitial(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnRequestInterstitial, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnRequestInterstitial, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnRequestInterstitial != null)
            {
                if (ad != null)
                {
                    Ads.OnRequestInterstitial(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnRequestInterstitialFailed(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnRequestInterstitialFailed, " + (paramJson ?? "null"));
                return;
            }

            string zoneId = "";
            if (values != null && values.ContainsKey("zone_id"))
            {
                zoneId = values["zone_id"] as string;
            }

            if (Ads.OnRequestInterstitialFailed != null)
            {
                Ads.OnRequestInterstitialFailed();
            }
            if (Ads.OnRequestInterstitialFailedWithZone != null)
            {
                Ads.OnRequestInterstitialFailedWithZone(zoneId);
            }
        }

        public void _OnOpened(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnOpened, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnOpened, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnOpened != null)
            {
                if (ad != null)
                {
                    Ads.OnOpened(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnClosed(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnClosed, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnClosed, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnClosed != null)
            {
                if (ad != null)
                {
                    Ads.OnClosed(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }

            _ads.Remove(ad.Id);
        }

        public void _OnExpiring(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnExpiring, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnExpiring, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnExpiring != null)
            {
                if (ad != null)
                {
                    Ads.OnExpiring(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }

            _ads.Remove(ad.Id);
        }

        public void _OnAudioStarted(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAudioStarted, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnAudioStarted, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAudioStarted != null)
            {
                if (ad != null)
                {
                    Ads.OnAudioStarted(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnAudioStopped(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAudioStopped, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnAudioStopped, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAudioStopped != null)
            {
                if (ad != null)
                {
                    Ads.OnAudioStopped(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnLeftApplication(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnLeftApplication, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnLeftApplication, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnLeftApplication != null)
            {
                if (ad != null)
                {
                    Ads.OnLeftApplication(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnClicked(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnClicked, " + (paramJson ?? "null"));
                return;
            }

            InterstitialAd ad = GetAdFromHashtable(values);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnClicked, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnClicked != null)
            {
                if (ad != null)
                {
                    Ads.OnClicked(ad);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnIAPOpportunity(string paramJson)
        {
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnIAPOpportunity, " + (paramJson ?? "null"));
                return;
            }

            Hashtable valuesAd = null;
            string iapProductId = null;
            AdsIAPEngagementType engagement = AdsIAPEngagementType.AdColonyIAPEngagementEndCard;

            if (values.ContainsKey(Constants.OnIAPOpportunityAdKey))
            {
                valuesAd = (AdColonyJson.Decode(values[Constants.OnIAPOpportunityAdKey] as String)) as Hashtable;
            }
            if (values.ContainsKey(Constants.OnIAPOpportunityEngagementKey))
            {
                engagement = (AdsIAPEngagementType)Convert.ToInt32(values[Constants.OnIAPOpportunityEngagementKey]);
            }
            if (values.ContainsKey(Constants.OnIAPOpportunityIapProductIdKey))
            {
                iapProductId = values[Constants.OnIAPOpportunityIapProductIdKey] as string;
            }

            InterstitialAd ad = GetAdFromHashtable(valuesAd);
            if (ad == null)
            {
                Debug.LogError("Unable to create ad within _OnIAPOpportunity, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnIAPOpportunity != null)
            {
                Ads.OnIAPOpportunity(ad, iapProductId, engagement);
            }
        }

        public void _OnRewardGranted(string paramJson)
        {
            string zoneId = null;
            bool success = false;
            string productId = null;
            int amount = 0;

            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnRewardGranted, " + (paramJson ?? "null"));
                return;
            }

            if (values != null)
            {
                if (values.ContainsKey(Constants.OnRewardGrantedZoneIdKey))
                {
                    zoneId = values[Constants.OnRewardGrantedZoneIdKey] as string;
                }
                if (values.ContainsKey(Constants.OnRewardGrantedSuccessKey))
                {
                    success = Convert.ToBoolean(Convert.ToInt32(values[Constants.OnRewardGrantedSuccessKey]));
                }
                if (values.ContainsKey(Constants.OnRewardGrantedNameKey))
                {
                    productId = values[Constants.OnRewardGrantedNameKey] as string;
                }
                if (values.ContainsKey(Constants.OnRewardGrantedAmountKey))
                {
                    amount = Convert.ToInt32(values[Constants.OnRewardGrantedAmountKey]);
                }
            }

            if (Ads.OnRewardGranted != null)
            {
                Ads.OnRewardGranted(zoneId, success, productId, amount);
            }
        }

        public void _OnCustomMessageReceived(string paramJson)
        {
            string type = null;
            string message = null;

            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnCustomMessageReceived, " + (paramJson ?? "null"));
                return;
            }

            if (values != null)
            {
                if (values.ContainsKey(Constants.OnCustomMessageReceivedTypeKey))
                {
                    type = values[Constants.OnCustomMessageReceivedTypeKey] as string;
                }
                if (values.ContainsKey(Constants.OnCustomMessageReceivedMessageKey))
                {
                    message = values[Constants.OnCustomMessageReceivedMessageKey] as string;
                }
            }

            if (Ads.OnCustomMessageReceived != null)
            {
                Ads.OnCustomMessageReceived(type, message);
            }
        }

        //End Native Interstital Callbacks.

        //Start internal private methods of Native Interstital Callbacks.
        private InterstitialAd GetAdFromHashtable(Hashtable values)
        {
            string id = null;
            if (values != null && values.ContainsKey("id"))
            {
                id = values["id"] as string;
            }

            InterstitialAd ad = null;
            if (id != null)
            {
                if (_ads.ContainsKey(id))
                {
                    ad = _ads[id];
                    ad.UpdateValues(values);
                }
                else
                {
                    ad = new InterstitialAd(values);
                    _ads[id] = ad;
                }
            }
            return ad;
        }

        private Dictionary<string, InterstitialAd> _ads = new Dictionary<string, InterstitialAd>();
        private static Ads _sharedGameObject;
        private static bool _initialized;
        private IAds _sharedInstance;
        private IEventTracker _eventTracker;
        private readonly System.Object _updateOnMainThreadActionsLock = new System.Object();
        private readonly Queue<System.Action> _updateOnMainThreadActions = new Queue<System.Action>();

        //End internal private methods of Native Interstital Callbacks.

        //------------------------------End Interstital-------------------------------------------------------------------------

        //------------------------------Start Banner-------------------------------------------------------------------------

        /// <summary>
        /// Event triggered after an banner ad is available.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewLoaded;

        /// <summary>
        /// Event triggered after an Banner request is failed.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewFailedToLoad;

        /// <summary>
        /// Event triggered after an Banner full screen expanded.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewOpened;

        /// <summary>
        /// Event triggered after an Banner fullscreen closed.
        /// It's recommended to request a new ad within this callback.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewClosed;

        /// <summary>
        /// Event triggered if action with ad caused the application to background.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewLeftApplication;

        /// <summary>
        /// Event triggered after an Banner was clicked.
        /// </summary>
        public static event Action<AdColonyAdView> OnAdViewClicked;



        //Start internal private methods of Native Banner Callbacks.
        private AdColonyAdView GetAdColonyAdViewFromHashtable(Hashtable values)
        {
            string id = null;
            if (values != null && values.ContainsKey("id"))
            {
                id = values["id"] as string;
            }

            AdColonyAdView ad = null;
            if (id != null)
            {
                if (adcolonyAdViewDictionary.ContainsKey(id))
                {
                    ad = adcolonyAdViewDictionary[id];
                    ad.UpdateValues(values);
                }
                else
                {
                    ad = new AdColonyAdView(values);
                    adcolonyAdViewDictionary[id] = ad;
                }
            }
            return ad;
        }

        private Dictionary<string, AdColonyAdView> adcolonyAdViewDictionary = new Dictionary<string, AdColonyAdView>();

        //Start Native Banner Callbacks.
        //below methods called from native code for Banner Ad.    
        public void _OnAdColonyAdViewLoaded(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewLoaded called.");
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewLoaded, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewLoaded, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAdViewLoaded != null)
            {
                if (adColonyAdView != null)
                {
                    Ads.OnAdViewLoaded(adColonyAdView);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnAdColonyAdViewFailedToLoad(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewFailedToLoad called.");
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewFailedToLoad, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewLoaded, " + (paramJson ?? "null"));
                return;
            }

            //string zoneId = "";
            //if (values != null && values.ContainsKey("zone_id"))
            //{
            //    zoneId = values["zone_id"] as string;
            //}

            if (Ads.OnAdViewFailedToLoad != null)
            {
                Ads.OnAdViewFailedToLoad(adColonyAdView);
            }
            else
            {
                Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
            }
        }

        public void _OnAdColonyAdViewOpened(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewOpened called.");
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewOpened, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewOpened, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAdViewOpened != null)
            {
                if (adColonyAdView != null)
                {
                    Ads.OnAdViewOpened(adColonyAdView);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnAdColonyAdViewClosed(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewClosed called.");
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewClosed, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewClosed, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAdViewClosed != null)
            {
                if (adColonyAdView != null)
                {
                    Ads.OnAdViewClosed(adColonyAdView);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnAdColonyAdViewLeftApplication(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewLeftApplication called.");     
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewLeftApplication, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewLeftApplication, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAdViewLeftApplication != null)
            {
                if (adColonyAdView != null)
                {
                    Ads.OnAdViewLeftApplication(adColonyAdView);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        public void _OnAdColonyAdViewClicked(string paramJson)
        {
            Debug.Log("AdColony.Wrapper._OnAdColonyAdViewClicked called.");
            Hashtable values = (AdColonyJson.Decode(paramJson) as Hashtable);
            if (values == null)
            {
                Debug.LogError("Unable to parse parameters in _OnAdColonyAdViewClicked, " + (paramJson ?? "null"));
                return;
            }

            AdColonyAdView adColonyAdView = GetAdColonyAdViewFromHashtable(values);
            if (adColonyAdView == null)
            {
                Debug.LogError("Unable to create ad within _OnAdColonyAdViewClicked, " + (paramJson ?? "null"));
                return;
            }

            if (Ads.OnAdViewClicked != null)
            {
                if (adColonyAdView != null)
                {
                    Ads.OnAdViewClicked(adColonyAdView);
                }
                else
                {
                    Debug.LogError(Constants.AdsMessageErrorUnableToRebuildAd);
                }
            }
        }

        internal static void  DestroyAdView(string id)
        {
            Debug.Log("AdColony.Wrapper.DestroyAdView called.");
            SharedInstance.DestroyAdView(id);
        }

        internal static void ShowAdView(string id)
        {
            Debug.Log("AdColony.Wrapper.show called.");
            SharedInstance.ShowAdView(id);
        }

        internal static void HideAdView(string id)
        {
            Debug.Log("AdColony.Wrapper.hide called.");
            SharedInstance.HideAdView(id);
        }

        //------------------------------End Banner-------------------------------------------------------------------------
        #endregion

    }

    public enum AdSize
    {
        Banner = 1,
        MediumRectangle = 2,
        Leaderboard = 3,
        SKYSCRAPER = 4
    }

    public enum AdPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }

    public interface IAds
    {
        void Configure(string appId, AppOptions options, params string[] zoneIds);
        string GetSDKVersion();
        void RequestInterstitialAd(string zoneId, AdOptions adOptions);
        void RequestAdView(string zoneId, AdSize ad, AdPosition adPosition, AdOptions adOptions);
        Zone GetZone(string zoneId);
        string GetUserID();
        void SetAppOptions(AppOptions options);
        AppOptions GetAppOptions();
        void SendCustomMessage(string type, string content);
        void LogInAppPurchase(string transactionId, string productId, int purchasePriceMicro, string currencyCode);
        void ShowAd(InterstitialAd ad);
        void CancelAd(InterstitialAd ad);
        void DestroyAd(string id);
        void DestroyAdView(string id);
        void ShowAdView(string id);
        void HideAdView(string id);
    }
}

package com.adcolony.unityplugin;

import java.util.*;
import java.util.concurrent.Semaphore;
import android.location.Location;
import android.util.Log;
import android.content.Context;
import android.app.Activity;
import android.view.Gravity;
import android.view.View;

import com.unity3d.player.UnityPlayer;
import com.adcolony.sdk.*;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

//
// Useful for converting data-types between the two languages and enforcing synchronous
// operations within an asynchronous framework (which we don't do anymore).
//
public class UnityADCAds {
    private static Map<String, UnityADCAdContainer> _adContainers = new HashMap<String, UnityADCAdContainer>();
    private static Map<String, UnityADCAdViewContainer> _adViewContainers = new HashMap<String, UnityADCAdViewContainer>();
    private static UnityADCAds _sharedInstance;
    private Map<String, Object> _cachedAppOptions = null;
    private String _managerName = "";
    private UnityADCAdColonyCustomMessageListener _customMessageListener = new UnityADCAdColonyCustomMessageListener();
    private Set<String> _customMessageListenerTypes = new HashSet<String>();
    private static final String TAG = "UnityADCAds";
    private static final String ADC_UNITY_ON_CONFIGURATION_COMPLETED = "AdColony.on_configuration_completed";

    static synchronized UnityADCAds getSharedInstance() {
        if (_sharedInstance == null) {
            _sharedInstance = new UnityADCAds();
        }
        return _sharedInstance;
    }

    public static void sendUnityMessage(String method, String param) {
        UnityPlayer.UnitySendMessage(getSharedInstance()._managerName, method, param);
    }

    public static void setManagerName(String managerName) {
        getSharedInstance()._managerName = managerName;
    }

    AdColonyAppOptions appOptionsFromMap(Map<String, Object> map) {
        Map<String, Object> mapCopy = new HashMap<String, Object>(map);
        AdColonyAppOptions options = new AdColonyAppOptions();

        if (mapCopy.containsKey("metadata")) {
            Map<String, Object> metadata = (Map<String, Object>)mapCopy.get("metadata");
            if (metadata != null) {
                options.setUserMetadata(metadataFromMap(metadata));
                mapCopy.remove("metadata");
            }
        }

        if (mapCopy.containsKey("gdpr_required")) {
            options.setGDPRRequired((Boolean)mapCopy.get("gdpr_required"));
            mapCopy.remove("gdpr_required");
        }
        if (mapCopy.containsKey("consent_string")) {
            options.setGDPRConsentString((String)mapCopy.get("consent_string"));
            mapCopy.remove("consent_string");
        }
        if (mapCopy.containsKey("user_id")) {
            options.setUserID((String)mapCopy.get("user_id"));
            mapCopy.remove("user_id");
        }
        if (mapCopy.containsKey("origin_store")) {
            options.setOriginStore((String)mapCopy.get("origin_store"));
            mapCopy.remove("origin_store");
        }
        if (mapCopy.containsKey("orientation")) {
            options.setRequestedAdOrientation((Integer)mapCopy.get("orientation"));
            mapCopy.remove("orientation");
        }
        if (mapCopy.containsKey("multi_window_enabled")) {
            options.setMultiWindowEnabled((Boolean)mapCopy.get("multi_window_enabled"));
            mapCopy.remove("multi_window_enabled");
        }
        if (mapCopy.containsKey("logging")) {
            // TODO: disable logging isn't available on Android
            mapCopy.remove("logging");
        }

        // NOTE: setAppVersion isn't exposed on iOS
        if (mapCopy.containsKey("app_version")) {
            // options.setAppVersion((String)mapCopy.get("app_version"));
            mapCopy.remove("app_version");
        }
        if (mapCopy.containsKey("plugin_version")) {
            options.setPlugin(AdColonyAppOptions.UNITY, (String)mapCopy.get("plugin_version"));
            mapCopy.remove("plugin_version");
        }

        // Add all the remaining keys
        for (Map.Entry<String, Object> entry : mapCopy.entrySet()) {
            Object o = entry.getValue();
            if (o instanceof Integer) {
                options.setOption(entry.getKey(), ((Integer)o).doubleValue());
            } else if (o instanceof String) {
                options.setOption(entry.getKey(), (String)o);
            } else if (o instanceof Double) {
                options.setOption(entry.getKey(), (Double)o);
            } else if (o instanceof Boolean) {
                options.setOption(entry.getKey(), (Boolean)o);
            }
        }

        return options;
    }

    AdColonyAdOptions adOptionsFromMap(Map<String, Object> map) {
        Map<String, Object> mapCopy = new HashMap<String, Object>(map);
        AdColonyAdOptions options = new AdColonyAdOptions();

        if (mapCopy.containsKey("metadata")) {
            Map<String, Object> metadata = (Map<String, Object>)mapCopy.get("metadata");
            if (metadata != null) {
                options.setUserMetadata(metadataFromMap(metadata));
                mapCopy.remove("metadata");
            }
        }
        if (mapCopy.containsKey("pre_popup")) {
            options.enableConfirmationDialog((Boolean)mapCopy.get("pre_popup"));
            mapCopy.remove("pre_popup");
        }
        if (mapCopy.containsKey("post_popup")) {
            options.enableResultsDialog((Boolean)mapCopy.get("post_popup"));
            mapCopy.remove("post_popup");
        }

        // Add all the remaining keys
        for (Map.Entry<String, Object> entry : mapCopy.entrySet()) {
            Object o = entry.getValue();
            if (o instanceof Integer) {
                options.setOption(entry.getKey(), ((Integer)o).doubleValue());
            } else if (o instanceof String) {
                options.setOption(entry.getKey(), (String)o);
            } else if (o instanceof Double) {
                options.setOption(entry.getKey(), (Double)o);
            } else if (o instanceof Boolean) {
                options.setOption(entry.getKey(), (Boolean)o);
            }
        }

        return options;
    }

    AdColonyUserMetadata metadataFromMap(Map<String, Object> map) {
        Map<String, Object> mapCopy = new HashMap<String, Object>(map);
        AdColonyUserMetadata meta = new AdColonyUserMetadata();

        if (mapCopy.containsKey("age")) {
            meta.setUserAge((Integer)mapCopy.get("age"));
            mapCopy.remove("age");
        }
        if (mapCopy.containsKey("interests")) {
            List<String> interests = (List<String>)mapCopy.get("interests");
            for (String interest : interests) {
                meta.addUserInterest(interest);
            }
            mapCopy.remove("interests");
        }
        if (mapCopy.containsKey("gender")) {
            meta.setUserGender((String)mapCopy.get("gender"));
            mapCopy.remove("gender");
        }
        if (mapCopy.containsKey("latitude") && mapCopy.containsKey("longitude")) {
            Location location = new Location("");
            location.setLatitude(Double.valueOf((Double)mapCopy.get("latitude")));
            location.setLongitude(Double.valueOf((Double)mapCopy.get("longitude")));
        }
        if (mapCopy.containsKey("zipcode")) {
            meta.setUserZipCode((String)mapCopy.get("zipcode"));
            mapCopy.remove("zipcode");
        }
        if (mapCopy.containsKey("income")) {
            meta.setUserAnnualHouseholdIncome((Integer)mapCopy.get("income"));
            mapCopy.remove("income");
        }
        if (mapCopy.containsKey("marital_status")) {
            meta.setUserMaritalStatus((String)mapCopy.get("marital_status"));
            mapCopy.remove("marital_status");
        }
        if (mapCopy.containsKey("edu_level")) {
            meta.setUserEducation((String)mapCopy.get("edu_level"));
            mapCopy.remove("edu_level");
        }

        // Add all the remaining keys
        for (Map.Entry<String, Object> entry : mapCopy.entrySet()) {
            Object o = entry.getValue();
            if (o instanceof Integer) {
                meta.setMetadata(entry.getKey(), ((Integer)o).doubleValue());
            } else if (o instanceof String) {
                meta.setMetadata(entry.getKey(), (String)o);
            } else if (o instanceof Double) {
                meta.setMetadata(entry.getKey(), (Double)o);
            } else if (o instanceof Boolean) {
                meta.setMetadata(entry.getKey(), (Boolean)o);
            }
        }

        return meta;
    }

    public static void configure(String json) {
        Activity activity = UnityPlayer.currentActivity;
        Map<String, Object> params = UnityADCUtils.jsonToMap(json);

        AdColonyAppOptions appOptions = null;
        Map<String, Object> appOptionsMap = (Map<String, Object>)params.get("app_options");
        if (appOptionsMap != null) {
            appOptions = getSharedInstance().appOptionsFromMap(appOptionsMap);
            getSharedInstance()._cachedAppOptions = new HashMap<String, Object>(appOptionsMap);
        }

        String appId = (String)params.get("app_id");

        List<String> zoneList = (List<String>)params.get("zone_ids");
        String[] zones = new String[zoneList.size()];
        int index = 0;
        for (String zone : zoneList) {
            zones[index++] = zone;
        }

        AdColony.configure(activity, appOptions, appId, zones);
        AdColony.removeRewardListener();
        AdColony.setRewardListener(new UnityADCAdColonyRewardListener());

        // Requires Android native changes for SDK-679 for the custom onConfigureCompleted message
        AdColony.addCustomMessageListener(new AdColonyCustomMessageListener() {
            @Override
            public void onAdColonyCustomMessage(AdColonyCustomMessage customMessage) {
                AdColony.removeCustomMessageListener(ADC_UNITY_ON_CONFIGURATION_COMPLETED);
                try {
                    JSONObject message = new JSONObject(customMessage.getMessage());
                    JSONArray zoneIds = message.getJSONArray("zone_ids");
                    List lZoneIds = UnityADCUtils.toList(message.getJSONArray("zone_ids"));
                    String zoneListJson = UnityADCUtils.toJsonFromStringList(lZoneIds);
                    sendUnityMessage("_OnConfigure", zoneListJson);
                } catch (JSONException e) {
                    Log.d(TAG, "Error Parsing Configuration Completed JSON");
                    e.printStackTrace();
                }
            }
        }, ADC_UNITY_ON_CONFIGURATION_COMPLETED);
    }

    public static void requestInterstitialAd(String json) {
        Map<String, Object> params = UnityADCUtils.jsonToMap(json);

        AdColonyAdOptions adOptions = null;
        Map<String, Object> adOptionsMap = (Map<String, Object>)params.get("ad_options");
        if (adOptionsMap != null) {
            adOptions = getSharedInstance().adOptionsFromMap(adOptionsMap);
        }

        String zoneId = (String)params.get("zone_id");

        UnityADCAdContainer adContainer = new UnityADCAdContainer();
        getSharedInstance()._adContainers.put(adContainer.id, adContainer);
        AdColony.requestInterstitial(zoneId, adContainer, adOptions);
    }

    public static void setAppOptions(String json) {
        AdColonyAppOptions appOptions = null;

        if (json != null) {
            Map<String, Object> appOptionsMap = UnityADCUtils.jsonToMap(json);
            if (appOptionsMap != null) {
                appOptions = getSharedInstance().appOptionsFromMap(appOptionsMap);
                getSharedInstance()._cachedAppOptions = new HashMap<String, Object>(appOptionsMap);
            }
        }

        AdColony.setAppOptions(appOptions);
    }

    public static String getAppOptions() {
        String json = null;
        Map<String, Object> appOptionsMap = getSharedInstance()._cachedAppOptions;
        if (appOptionsMap != null) {
            json = UnityADCUtils.toJson(appOptionsMap);
        }
        return json;
    }

    public static void showAd(String id) {
        UnityADCAdContainer unityAd = getSharedInstance()._adContainers.get(id);
        if (unityAd != null) {
            unityAd.ad.show();
        } else {
            Log.d(TAG, "Unable to find existing ad");
        }
    }

    public static void cancelAd(String id) {
        UnityADCAdContainer unityAd = getSharedInstance()._adContainers.get(id);
        if (unityAd != null) {
            unityAd.ad.cancel();
        } else {
            Log.d(TAG, "Unable to find existing ad");
        }
    }

    public static void destroyAd(String id) {
        getSharedInstance()._adContainers.remove(id);
    }

    public static String getZone(String zoneID) {
        AdColonyZone zone = AdColony.getZone(zoneID);
        if (zone == null) {
            return null;
        }
        return UnityADCUtils.zoneToJson(zone);
    }

    public static void sendCustomMessage(String type, String message) {
        if (!getSharedInstance()._customMessageListenerTypes.contains(type)) {
            AdColony.addCustomMessageListener(getSharedInstance()._customMessageListener, type);
            getSharedInstance()._customMessageListenerTypes.add(type);
        }
        new AdColonyCustomMessage(type, message).send();
    }

    public static void logEvent(String eventName, String json) {
        if (json != null) {
            Map<String, Object> map = UnityADCUtils.jsonToMap(json);
            if (map != null) {
                HashMap<String, String> hashMap = new HashMap<String, String>();

                for (Map.Entry<String, Object> entry : map.entrySet()) {
                    if (entry.getValue() != null && !entry.getValue().equals("null") && entry.getValue().getClass().equals(String.class)) {
                        hashMap.put(entry.getKey(), (String)entry.getValue());
                    }
                }

                AdColonyEventTracker.logEvent(eventName, hashMap);
            }
        }
    }

    // Need this extra conversion step because the Java interface uses objects for Integer/Double
    public static void logTransactionWithID(String itemID, int quantity, double price, String currencyCode, String receipt, String store, String description) {
        AdColonyEventTracker.logTransaction(itemID, (Integer)quantity, (Double)price, currencyCode, receipt, store, description);
    }

    // Need this extra conversion step because the Java interface uses objects for Integer/Double
    public static void logCreditsSpentWithName(String name, int quantity, double val, String currencyCode) {
        AdColonyEventTracker.logCreditsSpent(name, (Integer)quantity, (double)val, currencyCode);
    }

    // Need this extra conversion step because the Java interface uses objects for Integer
    public static void logLevelAchieved(int level) {
        AdColonyEventTracker.logLevelAchieved((Integer)level);
    }

    // START Banner AD

    public static void requestAdView(String json) {
        Map<String, Object> params = UnityADCUtils.jsonToMap(json);

        AdColonyAdOptions adOptions = null;
        Map<String, Object> adOptionsMap = params.containsKey("ad_options")?(Map<String, Object>)params.get("ad_options"):null;
        if (adOptionsMap != null) {
            adOptions = getSharedInstance().adOptionsFromMap(adOptionsMap);
        }

        String zoneId = (String)params.get("zone_id");
        int adSize = (int)params.get("ad_size");
        int adPosition = (int)params.get("ad_position");

        UnityADCAdViewContainer adContainer = new UnityADCAdViewContainer(adPosition);
        getSharedInstance()._adViewContainers.put(adContainer.id, adContainer);
        AdColony.requestAdView(zoneId, adContainer, getAdSize(adSize), adOptions);
    }

    public static void show(final String id) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                UnityADCAdViewContainer adcAdViewContainer = getSharedInstance()._adViewContainers.get(id);
                if (adcAdViewContainer != null && adcAdViewContainer.ad != null &&
                        adcAdViewContainer.ad.get_ad()!= null) {
                    adcAdViewContainer.ad.get_ad().setVisibility(View.VISIBLE);;
                }
            }
        });
    }

    public static void hide(final String id) {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                UnityADCAdViewContainer adcAdViewContainer = getSharedInstance()._adViewContainers.get(id);
                if (adcAdViewContainer != null && adcAdViewContainer.ad != null &&
                        adcAdViewContainer.ad.get_ad()!= null) {
                    adcAdViewContainer.ad.get_ad().setVisibility(View.GONE);;
                }
            }
        });
    }

    public static void destroyAdView(String id) {
        destroyNativeAdView(id);
    }

    private static void destroyNativeAdView(String id) {
        UnityADCAdViewContainer adcAdViewContainer = getSharedInstance()._adViewContainers.get(id);
        if (adcAdViewContainer != null && adcAdViewContainer.ad != null &&
                adcAdViewContainer.ad.get_ad() != null) {
            adcAdViewContainer.ad.get_ad().destroy();
            getSharedInstance()._adViewContainers.remove(id);
        }
    }

    private static AdColonyAdSize getAdSize(int adSize) {
        switch (adSize) {
            case 2:
                return AdColonyAdSize.MEDIUM_RECTANGLE;
            case 3:
                return AdColonyAdSize.LEADERBOARD;
            case 4:
                return AdColonyAdSize.SKYSCRAPER;
             default:
                return AdColonyAdSize.BANNER;
        }
    }
    // END Banner AD
}

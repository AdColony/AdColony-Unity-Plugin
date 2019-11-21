package com.adcolony.unityplugin;

import com.adcolony.sdk.AdColonyAdSize;
import com.adcolony.sdk.AdColonyAdView;
import com.adcolony.sdk.AdColonyInterstitial;

import java.util.HashMap;
import java.util.Map;


public class UnityADCAdViewAds {
    public String id;

    private AdColonyAdSize adColonyAdSize;
    private int adPosition;
    private AdColonyAdView _ad;

    UnityADCAdViewAds(AdColonyAdView ad, String id, int adPosition) {
        _ad = ad;
        this.id = id;
        adColonyAdSize = ad.getAdSize();
        this.adPosition = adPosition;
    }

    public String toJson() {
        Map<String, Object> data = new HashMap<String, Object>();
        if (_ad != null) {
            data.put("zone_id", _ad.getZoneId());
            data.put("id", id);
            data.put("position", Integer.toString(adPosition));
        }
        String json = UnityADCUtils.toJson(data);
        return json;
    }

    public AdColonyAdSize getAdColonyAdSize() {
        return adColonyAdSize;
    }

    public int getAdPosition() {
        return adPosition;
    }

    public AdColonyAdView get_ad() {
        return _ad;
    }
}

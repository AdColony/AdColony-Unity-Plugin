#import <AdColony/AdColony.h>
#import "UnityAppController.h"
#import "PluginBase/AppDelegateListener.h"
#import "AdColonyUnityConstants.h"
#import "AdColonyUnityJson.h"

#pragma mark - Unity Helpers

// Converts NSString to C style string by way of copy (Mono will free it)
#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]


// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil

#pragma mark - Sanity Check Macros

#define ADC_IS_NSSTRING(str)                                    (str != nil && [str isKindOfClass:NSString.class])
#define ADC_IS_NSNUMBER(num)                                    (num != nil && [num isKindOfClass:NSNumber.class])
#define ADC_IS_NSARRAY(arr)                                     (arr != nil && [arr isKindOfClass:NSArray.class])
#define ADC_IS_NSDICTIONARY(dict)                               (dict != nil && [dict isKindOfClass:NSDictionary.class])

#define ADC_NSSTRING_EXISTS(str)                                (ADC_IS_NSSTRING(str) && str.length > 0)

#define ADC_NSSTRING_OR_EMPTY(str)                              (ADC_IS_NSSTRING(str) ? str : @"")
#define ADC_NSNUMBER_OR_NEGONE(num)                             (ADC_IS_NSNUMBER(num) ? num : @-1)
#define ADC_NSARRAY_OR_EMPTY(arr)                               (ADC_IS_NSARRAY(arr) ? arr : @[])
#define ADC_NSDICTIONARY_OR_EMPTY(dict)                         (ADC_IS_NSDICTIONARY(dict) ? dict : @{})

#define ADC_NSDICTIONARYKEY_NSSTRING_OR_EMPTY(dict, key)        (ADC_IS_NSDICTIONARY(dict) && ADC_IS_NSSTRING(dict[key]) ? dict[key] : @"")
#define ADC_NSDICTIONARYKEY_NSNUMBER_OR_NEGONE(dict, key)       (ADC_IS_NSDICTIONARY(dict) && ADC_IS_NSNUMBER(dict[key]) ? dict[key] : @-1)
#define ADC_NSDICTIONARYKEY_NSARRAY_OR_EMPTY(dict, key)         (ADC_IS_NSDICTIONARY(dict) && ADC_IS_NSARRAY(dict[key]) != nil ? dict[key] : @[])
#define ADC_NSDICTIONARYKEY_NSDICTIONARY_OR_EMPTY(dict, key)    (ADC_IS_NSDICTIONARY(dict) && ADC_IS_NSDICTIONARY(dict[key]) ? dict[key] : @{})
#define ADC_NSDICTIONARYKEY_OBJ_EXISTS(dict, key)               (ADC_IS_NSDICTIONARY(dict) && dict[key] != nil)

#pragma mark - Weak Self Helpers

#define weakify(var) __weak __typeof(var) ADCWeak_##var = var

#define strongify(var) \
_Pragma("clang diagnostic push") \
_Pragma("clang diagnostic ignored \"-Wshadow\"") \
__strong __typeof(var) var = ADCWeak_##var; \
_Pragma("clang diagnostic pop")

void UnitySendMessage(const char *className, const char *methodName, const char *param);

void SafeUnitySendMessage(const char *className, const char *methodName, const char *param) {
    if (className == NULL) {
        NSLog(@"Invalid className for UnitySendMessage, make sure ManagerName is set in platform object constructor.");
    }
    if (methodName == NULL) {
        methodName = "";
    }
    if (param == NULL) {
        param = "";
    }
    UnitySendMessage(className, methodName, param);
}

NSString *getGUID() {
    CFUUIDRef newUniqueId = CFUUIDCreate(kCFAllocatorDefault);
    NSString *uuidString = (__bridge_transfer NSString *)CFUUIDCreateString(kCFAllocatorDefault, newUniqueId);
    CFRelease(newUniqueId);
    return uuidString;
}


@interface AdColonyInterstitial (ADCUnityWrapper)

@end

@implementation AdColonyInterstitial (ADCUnityWrapper)

- (NSDictionary *)dictionaryForUnity {
    return @{@"zone_id": ADC_NSSTRING_OR_EMPTY(self.zoneID),
             @"expired": @(self.expired),
             @"audio_enabled": @(self.audioEnabled),
             @"iap_enabled": @(self.iapEnabled)};
    
}

- (NSString *)serializeForUnityWithId:(NSString *)adId {
    NSMutableDictionary *dict = [[self dictionaryForUnity] mutableCopy];
    if (ADC_NSSTRING_EXISTS(adId)) {
        [dict setObject:adId forKey:@"id"];
        return [dict toJsonString];
    }
    return nil;
}

@end


@interface AdcAdsUnityController : NSObject

@property (nonatomic, copy) NSString *managerName;
@property (nonatomic, copy) NSString *adapterVersion;
@property (nonatomic, strong) NSMutableDictionary *interstitialAds;
@property (nonatomic, strong) NSMutableDictionary *bannerAds;
@property (nonatomic, copy) NSString *appOptionsJson;

@end

@implementation AdcAdsUnityController

+ (AdcAdsUnityController *)sharedInstance {
    static AdcAdsUnityController * instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{ instance = [[AdcAdsUnityController alloc] init]; });
    return instance;
}

+ (void)sendUnityMessage:(NSString *)message toMethod:(const char *)method {
    SafeUnitySendMessage(MakeStringCopy([AdcAdsUnityController sharedInstance].managerName), method, MakeStringCopy(message));
}

- (id)init {
    if (self = [super init]) {
        self.interstitialAds = @{}.mutableCopy;
        self.bannerAds = @{}.mutableCopy;
    }
    return self;
}
@end


@interface AdcBannerAdManager : NSObject<AdColonyAdViewDelegate>

// Properties
@property (nonatomic, strong) NSString *adID;
@property (nonatomic) NSInteger adViewLocation;
@property (nonatomic, strong) NSString *zoneID;
@property (nonatomic, strong) AdColonyAdView *adView;

// Methods
- (void)requestAdViewInZone:(NSString *)zoneId withSize:(NSInteger)adSize location:(NSInteger)adLocation andOptions:(NSString *)adOptionsJson;
- (void)destroyAdView;
- (void)showAdView;
- (void)hideAdView;

@end

typedef enum : NSInteger {
    AdViewLocationTop = 0,
    AdViewLocationBottom,
    AdViewLocationTopLeft,
    AdViewLocationTopRight,
    AdViewLocationBottomLeft,
    AdViewLocationBottomRight,
    AdViewLocationCenter
} AdViewLocation;

@implementation AdcBannerAdManager

- (id)init {
    self = [super init];
    if (self) {
        self.adID = getGUID();
    }
    return self;
}


- (void)requestAdViewInZone:(NSString *)zoneId withSize:(NSInteger)adSize location:(NSInteger)adLocation andOptions:(NSString *)adOptionsJson {
    self.zoneID = zoneId;
    self.adViewLocation = adLocation;
    
    AdColonyAdOptions *adOptions = nil;
    if (adOptionsJson.length) {
        adOptions = [[AdColonyAdOptions alloc] init];
        [adOptions setupWithJson:adOptionsJson];
    }
    
    AdColonyAdSize size;
    switch (adSize) {
        case 1:
            size = kAdColonyAdSizeBanner;
            break;
        case 2:
            size = kAdColonyAdSizeMediumRectangle;
            break;
        case 3:
            size = kAdColonyAdSizeLeaderboard;
            break;
        case 4:
            size = kAdColonyAdSizeSkyscraper;
            break;
        default:
            size = kAdColonyAdSizeBanner;
            break;
    }
    
   UIViewController *viewController = GetAppController().rootViewController;
   [AdColony requestAdViewInZone:zoneId withSize:size viewController:viewController andDelegate:self];
}

- (void)destroyAdView {
    if (self.adView) {
        [self.adView destroy];
        self.adView = nil;
        [[AdcAdsUnityController sharedInstance].bannerAds removeObjectForKey:self.adID];
    }
}

- (void)showAdView {
    if (self.adView) {
        [self.adView setHidden:false];
    }
}

- (void)hideAdView {
    if (self.adView) {
        [self.adView setHidden:true];
    }
}

- (NSString *)getMessageString {
    NSString *message = [NSString stringWithFormat:@"{\"id\" : \"%@\", \"zone_id\" : \"%@\", \"position\" : \"%ld\"}", self.adID, self.zoneID, long(self.adViewLocation)];
    return message;
}


#pragma mark --
#pragma mark  AdViewBannerDelegate

- (void)adColonyAdViewDidLoad:(AdColonyAdView *)adView {
    adView.frame = [self getFrameForAdView:adView];
    UnityAppController* unityAppController = GetAppController();
    [unityAppController.rootView addSubview:adView];
    self.adView = adView;
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewLoaded"];
}

- (void)adColonyAdViewDidFailToLoad:(AdColonyAdRequestError *)error {
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewFailedToLoad"];
}

- (void)adColonyAdViewWillLeaveApplication:(AdColonyAdView *)adView {
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewLeftApplication"];
}

- (void)adColonyAdViewWillOpen:(AdColonyAdView *)adView {
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewOpened"];
}

- (void)adColonyAdViewDidClose:(AdColonyAdView *)adView {
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewClosed"];
}

- (void)adColonyAdViewDidReceiveClick:(AdColonyAdView *)adView  {
    [AdcAdsUnityController sendUnityMessage:[self getMessageString] toMethod:"_OnAdColonyAdViewClicked"];
}

-  (CGRect)getFrameForAdView:(AdColonyAdView *)adView {
    CGRect frame;
    UnityAppController* unityAppController = GetAppController();
    CGFloat centeredX = (unityAppController.rootView.bounds.size.width - adView.bounds.size.width) / 2;
    CGFloat centeredY = (unityAppController.rootView.bounds.size.height - adView.bounds.size.height) / 2;
    
    switch (self.adViewLocation) {
        case AdViewLocationTop:
            frame = CGRectMake(centeredX, 0, adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationTopLeft:
            frame = CGRectMake(0, 0, adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationTopRight:
            frame = CGRectMake(unityAppController.rootView.bounds.size.width - adView.bounds.size.width, 0,
                               adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationBottom:
            frame = CGRectMake(centeredX, unityAppController.rootView.bounds.size.height - adView.bounds.size.height,
                               adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationBottomLeft:
            frame = CGRectMake(0, unityAppController.rootView.bounds.size.height - adView.bounds.size.height,
                               adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationBottomRight:
            frame = CGRectMake(unityAppController.rootView.bounds.size.width - adView.bounds.size.width,
                               unityAppController.rootView.bounds.size.height - adView.bounds.size.height,
                               adView.bounds.size.width, adView.bounds.size.height);
            break;
        case AdViewLocationCenter:
            frame = CGRectMake(centeredX, centeredY, adView.bounds.size.width, adView.bounds.size.height);
            break;
        default:
            break;
    }
    
    return frame;
}

@end

extern "C" {
    
    void _AdcSetManagerNameAds(const char *managerName, const char *adapterVersion) {
        [AdcAdsUnityController sharedInstance].managerName = GetStringParam(managerName);
        [AdcAdsUnityController sharedInstance].adapterVersion = GetStringParam(adapterVersion);
    }
    
#pragma mark --
#pragma mark  AdConfigure
    
    
    void _AdcConfigure(const char *appId_, const char *appOptionsJson_, int zoneIdsCount_, const char *zoneIds_[]) {
        NSString *appId = GetStringParamOrNil(appId_);
        
        NSMutableArray *zoneIds = @[].mutableCopy;
        for (int i = 0; i < zoneIdsCount_; ++i) {
            [zoneIds addObject:GetStringParamOrNil(zoneIds_[i])];
        }
        
        NSString *appOptionsJson = GetStringParamOrNil(appOptionsJson_);
        [AdcAdsUnityController sharedInstance].appOptionsJson = appOptionsJson;
        
        // SDK-40 appOptions can no longer be nil all the time; the metadata
        // regarding the plugin must be set now
        AdColonyAppOptions *appOptions = [[AdColonyAppOptions alloc] init];
        if (appOptionsJson) {
            [appOptions setupWithJson:appOptionsJson];
        }
        [appOptions setPlugin:ADCUnity];
        [appOptions setPluginVersion:[AdcAdsUnityController sharedInstance].adapterVersion];
      
        
        
        [AdColony configureWithAppID:appId zoneIDs:zoneIds options:appOptions completion:^(NSArray<AdColonyZone *> *zones) {
            NSMutableArray *zoneJsonArray = [NSMutableArray array];
            for (AdColonyZone *zone in zones) {
                [zoneJsonArray addObject:[zone toJsonString]];
                
                if (zone.rewarded) {
                    // For each zone returned that is also a rewarded zone, register a callback that will then call _OnRewardGranted.
                    NSString *zoneID = zone.identifier;
                    [zone setReward:^(BOOL success, NSString * _Nonnull name, int amount) {
                        NSDictionary *dict = @{ADC_ON_REWARD_GRANTED_ZONEID_KEY  : ADC_NSSTRING_OR_EMPTY(zoneID),
                                               ADC_ON_REWARD_GRANTED_SUCCESS_KEY : @(success),
                                               ADC_ON_REWARD_GRANTED_NAME_KEY    : ADC_NSSTRING_OR_EMPTY(name),
                                               ADC_ON_REWARD_GRANTED_AMOUNT_KEY  : [NSString stringWithFormat:@"%d", amount]};
                        [AdcAdsUnityController sendUnityMessage:[dict toJsonString] toMethod:"_OnRewardGranted"];
                    }];
                }
            }
            
            [AdcAdsUnityController sendUnityMessage:[zoneJsonArray toJsonString] toMethod:"_OnConfigure"];
        }];
    }
    
    const char *_AdcGetSDKVersion() {
        return MakeStringCopy([AdColony getSDKVersion]);
    }
    
    void _AdcShowInterstitialAd(const char *id) {
        NSString *adId = GetStringParamOrNil(id);
        if (adId) {
            AdColonyInterstitial *ad = [[AdcAdsUnityController sharedInstance].interstitialAds objectForKey:adId];
            if (ad) {
                UnityAppController* unityAppController = GetAppController();
                [ad showWithPresentingViewController:unityAppController.rootViewController];
                return;
            }
        }
    }
    
    void _AdcCancelInterstitialAd(const char *id) {
        NSString *adId = GetStringParamOrNil(id);
        if (adId) {
            AdColonyInterstitial *ad = [[AdcAdsUnityController sharedInstance].interstitialAds objectForKey:adId];
            if (ad) {
                [ad cancel];
                return;
            }
        }
    }
    
    void _AdcDestroyInterstitialAd(const char *id) {
        NSString *adId = GetStringParamOrNil(id);
        [[AdcAdsUnityController sharedInstance].interstitialAds removeObjectForKey:adId];
    }
    
    void _AdcRequestInterstitialAd(const char *zoneId, const char *adOptionsJson) {
        NSString *myAdOptionsJson = GetStringParamOrNil(adOptionsJson);
        AdColonyAdOptions *adOptions = nil;
        if (myAdOptionsJson) {
            adOptions = [[AdColonyAdOptions alloc] init];
            [adOptions setupWithJson:myAdOptionsJson];
        }
        
        NSString *zoneIds = GetStringParam(zoneId);
        
        [AdColony requestInterstitialInZone:zoneIds
                                    options:adOptions
                                    success:^(AdColonyInterstitial *ad) {
                                        NSString *adId = getGUID();
                                        [AdcAdsUnityController sharedInstance].interstitialAds[adId] = ad;
                                        
                                        weakify(ad);
                                        [ad setOpen:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnOpened"];
                                        }];
                                        [ad setClose:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnClosed"];
                                        }];
                                        [ad setAudioStop:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnAudioStopped"];
                                        }];
                                        [ad setAudioStart:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnAudioStarted"];
                                        }];
                                        [ad setExpire:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnExpiring"];
                                        }];
                                        [ad setLeftApplication:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnLeftApplication"];
                                        }];
                                        [ad setClick:^{
                                            strongify(ad);
                                            [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnClicked"];
                                        }];
                                        [ad setIapOpportunity:^(NSString * _Nonnull iapProductID, AdColonyIAPEngagement engagement) {
                                            strongify(ad);
                                            NSMutableDictionary *mutableDict = [NSMutableDictionary dictionary];
                                            [mutableDict setObject:[ad serializeForUnityWithId:adId] forKey:ADC_ON_IAP_OPPORTUNITY_AD_KEY];
                                            [mutableDict setObject:ADC_NSSTRING_OR_EMPTY(iapProductID) forKey:ADC_ON_IAP_OPPORTUNITY_IAP_PRODUCT_ID_KEY];
                                            [mutableDict setObject:@((int)engagement) forKey:ADC_ON_IAP_OPPORTUNITY_ENGAGEMENT_KEY];
                                            [AdcAdsUnityController sendUnityMessage:[mutableDict toJsonString] toMethod:"_OnIAPOpportunity"];
                                        }];
                                        
                                        [AdcAdsUnityController sendUnityMessage:[ad serializeForUnityWithId:adId] toMethod:"_OnRequestInterstitial"];
                                    }
                                    failure:^(AdColonyAdRequestError *error) {
                                        NSDictionary *dict = @{@"error_code": @(error.code),
                                                               @"zone_id": ADC_NSSTRING_OR_EMPTY(zoneIds)};
                                        [AdcAdsUnityController sendUnityMessage:[dict toJsonString] toMethod:"_OnRequestInterstitialFailed"];
                                    }];
    }
    
    
#pragma mark --
#pragma mark  AdView
    
    void _AdcRequestAdView(const char *zoneId, const int adSize, const int adPosition, const char *optionsJson) {
        AdcBannerAdManager *bannerAdManager = [[AdcBannerAdManager alloc] init];
        [bannerAdManager requestAdViewInZone:GetStringParam(zoneId) withSize:adSize location:adPosition andOptions:GetStringParamOrNil(optionsJson)];
        [AdcAdsUnityController sharedInstance].bannerAds[bannerAdManager.adID] = bannerAdManager;
    }
    
    void _AdcDestroyAdView(const char *adID) {
        AdcBannerAdManager *bannerAdManager = [AdcAdsUnityController sharedInstance].bannerAds[GetStringParamOrNil(adID)];
        if (bannerAdManager) {
            [bannerAdManager destroyAdView];
        }
    }

    void _AdcShowAdView(const char *adID) {
        AdcBannerAdManager *bannerAdManager = [AdcAdsUnityController sharedInstance].bannerAds[GetStringParamOrNil(adID)];
        if (bannerAdManager) {
            [bannerAdManager showAdView];
        }
    }

    void _AdcHideAdView(const char *adID) {
        AdcBannerAdManager *bannerAdManager = [AdcAdsUnityController sharedInstance].bannerAds[GetStringParamOrNil(adID)];
        if (bannerAdManager) {
            [bannerAdManager hideAdView];
        }
    }
    
    
    // should return JSON
    const char *_AdcGetZone(const char *zoneId) {
        NSString *zoneString = GetStringParamOrNil(zoneId);
        if (zoneString == nil) {
            return nil;
        }
        
        AdColonyZone *zone = [AdColony zoneForID:zoneString];
        if (zone == nil) {
            return nil;
        }
        
        return MakeStringCopy([zone toJsonString]);
    }
    
    const char *_AdcGetUserID() {
        return MakeStringCopy([AdColony getUserID]);
    }
    
    void _AdcSetAppOptions(const char *appOptionsJson) {
        [AdcAdsUnityController sharedInstance].appOptionsJson = GetStringParam(appOptionsJson);
        
        AdColonyAppOptions *appOptions = [[AdColonyAppOptions alloc] init];
        [appOptions setupWithJson:[AdcAdsUnityController sharedInstance].appOptionsJson];
        [AdColony setAppOptions:appOptions];
    }
    
    const char *_AdcGetAppOptions() {
        return MakeStringCopy([AdcAdsUnityController sharedInstance].appOptionsJson);
    }
    
    void _AdcSendCustomMessage(const char *type, const char *content) {
        NSString *typeString = GetStringParamOrNil(type);
        if (typeString != nil) {
            [AdColony sendCustomMessageOfType:typeString
                                  withContent:GetStringParamOrNil(content)
                                        reply:^(id _Nullable obj) {
                                            if ([obj isKindOfClass:[NSString class]]) {
                                                NSDictionary *dict = @{ADC_ON_CUSTOM_MESSAGE_RECEIVED_TYPE_KEY    : ADC_NSSTRING_OR_EMPTY(typeString),
                                                                       ADC_ON_CUSTOM_MESSAGE_RECEIVED_MESSAGE_KEY : GetStringParam(content)};
                                                [AdcAdsUnityController sendUnityMessage:[dict toJsonString] toMethod:"_OnCustomMessageRecieved"];
                                            }
                                        }];
        }
    }
    
    void _AdcLogInAppPurchase(const char *transactionId, const char *productId, int purchasePriceMicro, const char *currencyCode) {
        [AdColony iapCompleteWithTransactionID:GetStringParam(transactionId)
                                     productID:GetStringParam(productId)
                                         price:[NSNumber numberWithInt:purchasePriceMicro]
                                  currencyCode:GetStringParamOrNil(currencyCode)];
    }
    
    void _AdcLogTransactionWithID(const char *itemID, int quantity, double price, const char *currencyCode, const char *receipt, const char *store, const char *description) {
        [AdColonyEventTracker logTransactionWithID:GetStringParam(itemID)
                                          quantity:(NSInteger)quantity
                                             price:[NSNumber numberWithDouble:price]
                                      currencyCode:GetStringParam(currencyCode)
                                           receipt:GetStringParam(receipt)
                                             store:GetStringParam(store)
                                       description:GetStringParam(description)];
    }
    
    void _AdcLogCreditsSpentWithName(const char *name, int quantity, double value, const char *currencyCode) {
        [AdColonyEventTracker logCreditsSpentWithName:GetStringParam(name)
                                             quantity:(NSInteger)quantity
                                                value:[NSNumber numberWithDouble:value]
                                         currencyCode:GetStringParam(currencyCode)];
    }
    
    void _AdcLogPaymentInfoAdded() {
        [AdColonyEventTracker logPaymentInfoAdded];
    }
    
    void _AdcLogAchievementUnlocked(const char *description) {
        [AdColonyEventTracker logAchievementUnlocked:GetStringParam(description)];
    }
    
    void _AdcLogLevelAchieved(int level) {
        [AdColonyEventTracker logLevelAchieved:(NSInteger)level];
    }
    
    void _AdcLogAppRated() {
        [AdColonyEventTracker logAppRated];
    }
    
    void _AdcLogActivated() {
        [AdColonyEventTracker logActivated];
    }
    
    void _AdcLogTutorialCompleted() {
        [AdColonyEventTracker logTutorialCompleted];
    }
    
    void _AdcLogSocialSharingEventWithNetwork(const char *network, const char *description) {
        [AdColonyEventTracker logSocialSharingEventWithNetwork:GetStringParam(network) description:GetStringParam(description)];
    }
    
    void _AdcLogRegistrationCompletedWithMethod(const char *method, const char *description) {
        [AdColonyEventTracker logRegistrationCompletedWithMethod:GetStringParam(method) description:GetStringParam(description)];
    }
    
    void _AdcLogCustomEvent(const char *event, const char *description) {
        [AdColonyEventTracker logCustomEvent:GetStringParam(event) description:GetStringParam(description)];
    }
    
    void _AdcLogAddToCartWithID(const char *itemID) {
        [AdColonyEventTracker logAddToCartWithID:GetStringParam(itemID)];
    }
    
    void _AdcLogAddToWishlistWithID(const char *itemID) {
        [AdColonyEventTracker logAddToWishlistWithID:GetStringParam(itemID)];
    }
    
    void _AdcLogCheckoutInitiated() {
        [AdColonyEventTracker logCheckoutInitiated];
    }
    
    void _AdcLogContentViewWithID(const char *contentID, const char *contentType) {
        [AdColonyEventTracker logContentViewWithID:GetStringParam(contentID) contentType:GetStringParam(contentType)];
    }
    
    void _AdcLogInvite() {
        [AdColonyEventTracker logInvite];
    }
    
    void _AdcLogLoginWithMethod(const char *method) {
        [AdColonyEventTracker logLoginWithMethod:GetStringParam(method)];
    }
    
    void _AdcLogReservation() {
        [AdColonyEventTracker logReservation];
    }
    
    void _AdcLogSearchWithQuery(const char *queryString) {
        [AdColonyEventTracker logSearchWithQuery:GetStringParam(queryString)];
    }
    
    void _AdcLogEvent(const char *name, const char *dataAsJson) {
        NSString *dataAsJsonStr = GetStringParamOrNil(dataAsJson);
        NSDictionary *dict = [dataAsJsonStr jsonStringToDictionary];
        if (dict) {
            [AdColonyEventTracker logEvent:GetStringParamOrNil(name) withDictionary:[dict mutableCopy]];
        }
    }
}

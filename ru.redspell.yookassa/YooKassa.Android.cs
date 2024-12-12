using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

// ReSharper disable InconsistentNaming

namespace YooKassa
{
    
 #if UNITY_ANDROID

    public partial class YooKassa
    {
        private const string PLUGIN_CLASS = "com.burninglab.yookassaunityplugin.YooKassaUnityPluginActivity";
        private const string OBJECT_NAME = "YooKassaBridge";

        
        public (bool,string) UserEmail { get; private set; }

        private static YooKassa _instance;

        public static YooKassa Instance()
        {
            if (_instance is not null) return _instance;
            var t = typeof(YooKassa);
            _instance = new GameObject(OBJECT_NAME, t).GetComponent<YooKassa>();
            return _instance;
        }

        void Awake()
        {
            gameObject.name = OBJECT_NAME;
            DontDestroyOnLoad(gameObject);
        }

        public class TokenizationCompleteData
        {
            public string productId;
            public string paymentToken;
            public string paymentType;
            public Bundle bundle;
        }

        private Action<TokenizationCompleteData> OnTokenizationComplete;

    

        private Action<ErrorInfo> OnTokenizationError;

        public class Bundle
        {
            public struct AmountData
            {
                public decimal amount;
                public string currencyCode;
            }

            public string id;
            public string title;
            public string description = String.Empty;
            public AmountData amountData;
        }


        private class TokenizationResponse
        {
            public bool status;

            public class TokenizationResult
            {
                public string token;
                public string paymentMethodType;
            }

            public TokenizationResult result;

            public Bundle bundle;

            public ErrorInfo error;
        }

        public void OnTokenizationCompleteEventHandler(string data)
        {
            Debug.Log("OnTokenizationCompleteEventHandler");
            var result = JsonConvert.DeserializeObject<TokenizationResponse>(data);
            var callback = OnTokenizationComplete;
            var errorCallback = OnTokenizationError;
            OnTokenizationComplete = null;
            OnTokenizationError = null;
            if (!result.status)
            {
                // Need to be called in MainUIThread?
                errorCallback?.Invoke(result.error);
                return;
            }

            var successData = new TokenizationCompleteData()
            {
                productId = result.bundle.id,
                paymentToken = result.result.token,
                paymentType = result.result.paymentMethodType,
                bundle = result.bundle
            };
            callback?.Invoke(successData);
        }

        private struct AuthData
        {
            public string shopId;
            public string appKey;
            public string clientId;
        }

        private struct ResponseConfig
        {
            public string callbackObjectName;
            public string callbackMethodName;
        }

        private const string shopId = "470036"; // string 

        private class Request
        {
            public AuthData authData;
            public Bundle bundle;
            public ResponseConfig responseConfig;

            public Request(Bundle bundle, bool isSandbox)
            {
                if (isSandbox)
                {
                    authData.shopId = "471480Э";
                    authData.appKey = "test_NDcxNDgwD8QSpZ9l_N7aWns5wlvK_3fknr0letzkcYI";
                    authData.clientId = "a3f3sdd8h811opqdn2cfjmrird2256k0";
                }
                else
                {
                    authData.shopId = "470036";
                    authData.appKey = "live_NDcwMDM2P7UCoYZ5IbgIIyKX5aVv7vhi5mj1R9l9XPg";
                    authData.clientId = "a3f3sdd8h811opqdn2cfjmrird2256k0"; // what about this iD? it need be different for different games? or not?
                }

                this.bundle = bundle;
            }
        }

        private class TokenizationRequest : Request
        {

            public struct Options
            {
                public string[] paymentMethods;

                public string savePaymentMethod;

            }

            public Options options;


            public TokenizationRequest(Bundle bundle, bool isSandbox) : base(bundle,isSandbox)
            {
                responseConfig.callbackObjectName = OBJECT_NAME;
                responseConfig.callbackMethodName = "OnTokenizationCompleteEventHandler";

                //options.paymentMethods = new[] { "BANK_CARD", "YOO_MONEY", "SBP" };
                options.paymentMethods = new[] { "BANK_CARD", "YOO_MONEY", "SBERBANK", "SBP" };
                options.savePaymentMethod = "USER_SELECTS";
            }
        }


        


        private void _RunTokenization(ProductDescription product,Action<TokenizationCompleteData> callback,Action<ErrorInfo> errorCallback)
        {
            var bundle = new Bundle()
            {
                id = product.storeSpecificId,
                title = product.metadata.localizedTitle,
                description = product.metadata.localizedDescription ?? "",
                amountData = new Bundle.AmountData()
                {
                    amount = product.metadata.localizedPrice,
                    currencyCode = product.metadata.isoCurrencyCode
                }
            };
            var request = new TokenizationRequest(bundle,IsSandbox);

            var tokenizationRequest = JsonConvert.SerializeObject(request);
            //Debug.Log($"YooKassa tokenize request: {tokenizationRequest}");
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            OnTokenizationComplete = callback;
            OnTokenizationError = errorCallback;
            // Call tokenization process from ui thread.
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using AndroidJavaObject yooKassaUnityPluginActivity = new AndroidJavaObject(PLUGIN_CLASS);
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // Call start tokenization plugin method.
                yooKassaUnityPluginActivity.Call("startTokenization", currentActivity, tokenizationRequest);
            }));
        }

        public void RunTokenization(ProductDescription product,Action<TokenizationCompleteData> callback,Action<ErrorInfo> errorCallback)
        {
            ShowEmailForm(email =>
                {
                    UserEmail = email;
                    _RunTokenization(product, callback, errorCallback);
                },
                errorCallback
            );
        }

        private class ConfirmationRequest : Request
        {

            public string paymentId;
            public string paymentMethodType;
            public string confirmationUrl;

            public ConfirmationRequest(
                Bundle bundle,
                bool isSundbox,
                string paymentId,
                string paymentMethodType,
                string confirmationUrl 
                ) : base(bundle,isSundbox)
            {
                responseConfig.callbackObjectName = OBJECT_NAME;
                responseConfig.callbackMethodName = "OnConfirmationCompleteEventHandler";
                this.paymentId = paymentId;
                this.confirmationUrl = confirmationUrl;
                this.paymentMethodType = paymentMethodType;
            }
        }

        public void RunConfirmation(
            string paymentId, string confirmationUrl, TokenizationCompleteData tokenization,
            Action<ConfirmationResponse> callback, Action<ErrorInfo> errorCallback
        )
        {
            var request = new ConfirmationRequest(tokenization.bundle,IsSandbox,paymentId,tokenization.paymentType, confirmationUrl);
            var confirmationRequest = JsonConvert.SerializeObject(request);
            //Debug.Log("YooKassa confirmation request: " + confirmationRequest);
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            OnConfirmationComplete = callback;
            OnConfirmationError = errorCallback;

            // Call tokenization process from ui thread.
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaObject yooKassaUnityPluginActivity = new AndroidJavaObject(PLUGIN_CLASS))
                {
                    AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    // Call start tokenization plugin method.
                    yooKassaUnityPluginActivity.Call("startConfirmation", currentActivity, confirmationRequest);
                }
            }));
        }

        public class ConfirmationResponse
        {
            public bool status;
            public string paymentId;
            public Bundle bundle;
            public ErrorInfo error;
        }

        
        public void OnConfirmationCompleteEventHandler(string data)
        {
            Debug.Log("OnConfirmationCompleteEventHandler");

            var confirmationResponse = JsonConvert.DeserializeObject<ConfirmationResponse>(data);
            var callback = OnConfirmationComplete;
            OnConfirmationComplete = null;
            var errorCallback = OnConfirmationError;
            OnConfirmationError = null;
            if (!confirmationResponse.status)
            {
                // Need to be called in MainUIThread?
                errorCallback?.Invoke(confirmationResponse.error);
                
                return;
            }
            // TODO: Call callback on success
            callback?.Invoke(confirmationResponse);
        }

        private Action<ErrorInfo> OnConfirmationError;
        private Action<ConfirmationResponse> OnConfirmationComplete;
    }
#endif
}
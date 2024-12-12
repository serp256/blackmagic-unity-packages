using System;
using System.Collections.Generic;
using UnityEngine;
using YandexPay.Enums;

namespace YandexPay
{
    public class YandexPayAndroid
    {
	    private AndroidJavaClass _yandexPayBridge;

	    public event Action<SuccessPaymentResponse> PaymentCompleted;
	    public event Action<FailurePaymentResponse> PaymentFailed;

	    public void Initialize(string metadata, YPayApiEnvironment environment)
	    {
		    JavaMessageBus messageBus = SetupMessageBus();

		    _yandexPayBridge = new AndroidJavaClass(Constants.PluginName);
		    _yandexPayBridge.CallStatic(Constants.InitializeMethodName, metadata, (int) environment, messageBus);
	    }

	    public void Pay(string productId, string metadata = null)
	    {
		    _yandexPayBridge.CallStatic(Constants.PayMethodName, productId, metadata);
	    }

        private JavaMessageBus SetupMessageBus()
        {
	        var messageFunctionMap = new Dictionary<string, Action<object>>
	        {
		        { "PaymentComplete", OnPaymentComplete },
		        { "PaymentFailed", OnPaymentFailed }
	        };

	        var messageBus = new JavaMessageBus((message, args) =>
	        {
		        messageFunctionMap.TryGetValue(message, out var function);
		        function?.Invoke(args);
	        });

	        return messageBus;
        }

        private void OnPaymentComplete(object args)
        {
	        if (args is not AndroidJavaObject androidJavaObject)
		        return;
	        
	        var successPaymentResponse = new SuccessPaymentResponse
	        {
		        Status = (PaymentStatus)androidJavaObject.Get<int>(Constants.StatusFieldName),
		        OrderId = androidJavaObject.Get<string>(Constants.OrderIdFieldName),
				ProductId = androidJavaObject.Get<string>(Constants.MetadataFieldName)
	        };

	        PaymentCompleted?.Invoke(successPaymentResponse);
        }

        private void OnPaymentFailed(object args)
        {
	        if (args is not AndroidJavaObject androidJavaObject)
		        return;
	        
	        var failurePaymentResponse = new FailurePaymentResponse
	        {
		        Status = (PaymentStatus)androidJavaObject.Get<int>(Constants.StatusFieldName),
		        ErrorMessage = androidJavaObject.Get<string>(Constants.ErrorMessageFieldName),
		        ProductId = androidJavaObject.Get<string>(Constants.MetadataFieldName)
	        };
	        
	        PaymentFailed?.Invoke(failurePaymentResponse);
        }
    }
}
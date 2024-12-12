using System;
using UnityEngine;

namespace YandexPay
{
    public class JavaMessageBus : AndroidJavaProxy
    {
        private Action<string, object> _callback;

        public JavaMessageBus(in Action<string, object> callback) : base(Constants.PluginName + Constants.MessageBusClassName)
        {
            _callback = callback;
        }
			
        public void Execute(string message, object args)
        {
            _callback(message, args);
        }
    }
}
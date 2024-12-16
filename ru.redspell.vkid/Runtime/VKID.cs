using System;
using UnityEngine;

    public class VKID
    {
        //private const string _javaClassName = "games.redspell.vkid_unity_bridge.VKUnityBridge";
        //private const string _proxyObject = "MessageBus";


        public bool Authenticated { get; private set; }

        public string UserId { get; private set; }
        public string UserName { get; private set; }

        private Action<bool, string> _onAuth;

        private class JavaMessageBus : AndroidJavaProxy
        {
            private readonly VKID _parent;

            public JavaMessageBus(VKID parent) : base("games.redspell.vkid_unity_bridge.MessageBus")
            {
                _parent = parent;
            }

            public void Success(long userId, string name)
            {
                Debug.Log($"VKID unit message bus OnSuccess: {userId}:{name}");
                _parent.Authenticated = true;
                _parent.UserId = userId.ToString();
                _parent.UserName = name;
                _parent._onAuth?.Invoke(true, null);
                _parent._onAuth = null;
            }

            public void Failure(int code, string message)
            {
                Debug.Log($"VKID unit message bus OnFailure: {code}:{message}");
                _parent._onAuth?.Invoke(false, message);
            }
        }


        private const string PluginClass = "games.redspell.vkid_unity_bridge.VKIDUnityBridge";


        private static VKID _instance;
        public static VKID Instance => _instance;

        public static void Initialize()
        {
            if (_instance is not null) return;
            _instance = new VKID();
        }

        private VKID()
        {
            var messageBus = new JavaMessageBus(this);
            using AndroidJavaObject pluginCls = new AndroidJavaClass(PluginClass);
            pluginCls.CallStatic("init", messageBus);
        }

        public void Auth(Action<bool, string> callback)
        {
            Debug.Log("VKID call auth");
            _onAuth += callback;
            using AndroidJavaObject pluginCls = new AndroidJavaClass(PluginClass);
            pluginCls.CallStatic<bool>("auth");
        }

        public void LogOut()
        {
            using AndroidJavaObject pluginCls = new AndroidJavaClass(PluginClass);
            pluginCls.CallStatic("logout");
        }

        public static bool IsGooglePlayAvailable()
        {
            Debug.Log("IsGooglePlay Available called!");
            using AndroidJavaObject pluginCls = new AndroidJavaClass(PluginClass);
            return pluginCls.CallStatic<bool>("checkIsGoogleAvailable");
        }
    }
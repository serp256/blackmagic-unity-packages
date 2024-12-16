using UnityEditor;
using UnityEngine;



[CreateAssetMenu(fileName = "VKIDSettings", menuName = "RedSpell/VKIDSettings")]
// only compile time resources needed
[System.Serializable]
class VKIDSettings : ScriptableObject
{


    internal const string FileName = "Assets/VKIDSettings.asset";

    public string VKIDClientID; //  "52722655",
    public string VKIDClientSecret; // "zeQKWssdPcoAThnk0edu",
    
    private static VKIDSettings _instance;
    internal static VKIDSettings Instance {
        get {
            if (_instance != null) return _instance;
            _instance = AssetDatabase.LoadAssetAtPath<VKIDSettings>(FileName);
            if (_instance == null)
            {
                _instance = CreateInstance<VKIDSettings>();
                AssetDatabase.CreateAsset(_instance, FileName);
            }
            return _instance;
        }
    }

}

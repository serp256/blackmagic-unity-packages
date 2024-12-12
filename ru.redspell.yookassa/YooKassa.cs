using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace YooKassa
{
    public partial class YooKassa : MonoBehaviour
    {
        
        public static bool IsSandbox { get; set; }
        
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
        public class ErrorInfo
        {
            public string errorCode;
            public string errorMessage;
        }
        
        public static void ShowEmailForm(Action<(bool,string)> onContinue, Action<ErrorInfo> onCancelled)
        {
            // Assuming the prefab is located at "Assets/Resources/EmailForm.prefab"
            GameObject emailFormPrefab = Resources.Load<GameObject>("YooKassaEmailForm");

            if (emailFormPrefab != null)
            {
                GameObject emailFormInstance = Instantiate(emailFormPrefab);
                var form = emailFormInstance.AddComponent<EmailForm>();
                form.OnContinue = email =>
                {
                    Destroy(emailFormInstance);
                    if (email == null)
                    {
                        var error = new ErrorInfo
                        {
                            errorCode = "CANCELED_BY_USER",
                            errorMessage = "User cancelled purchase"
                        };
                        onCancelled(error);
                        return;
                    };
                    onContinue(email.Value);
                };
                    
                // You can also set the parent of the instantiated object if needed
                // emailFormInstance.transform.SetParent(gameObject.transform);
            }
            else
            {
                Debug.LogError("EmailForm prefab not found in Resources");
            }
        }
    }
}
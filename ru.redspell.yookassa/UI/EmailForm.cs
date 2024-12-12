using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;


namespace YooKassa
{
    [Preserve]
    public class EmailForm : MonoBehaviour
    {


        private Button _button;

        private TextField _inputField;

        private Label _error_label;

        public Action<(bool,string)?> OnContinue { get; set; }

        // Start is called before the first frame update
        private void OnEnable()
        {
            // Get it from Prefs

            var saved_email = PlayerPrefs.GetString("user_email");
            // The UXML is already instantiated by the UIDocument component
            var uiDocument = GetComponent<UIDocument>();

            _button = uiDocument.rootVisualElement.Q("submit") as Button;
            
            var cancel = uiDocument.rootVisualElement.Q("cancel") as Button;
            cancel.RegisterCallback<ClickEvent>(evt =>
            {
                OnContinue.Invoke(null);
            });

            _button.RegisterCallback<ClickEvent>(Submit);

            _inputField = uiDocument.rootVisualElement.Q("email") as TextField; 
            _inputField.value = saved_email ?? "";
            _inputField.RegisterCallback<ChangeEvent<string>>(OnInputChange);
            
        }

        private void OnInputChange(ChangeEvent<string> evt)
        {
            if (_error_label is null) return;
            _error_label.style.visibility = Visibility.Hidden;
            _error_label.text = "";
        }

        
        
        
        private static readonly string _phoneBasicPattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

        // International format: +1-123-456-7890 or +1 (123) 456-7890
        private static readonly string _phopneInternationalPattern = @"^\+?([0-9]{1,3})?[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";


        private static readonly string _emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";


        private static bool CheckIsPhone(string input)
        {
            return Regex.IsMatch(input, _phoneBasicPattern) || Regex.IsMatch(input, _phopneInternationalPattern);
        }
        private void Submit(ClickEvent evt)
        {
            Debug.Log("email is: " + _inputField.value);
            var email = _inputField.value.Trim();
            var isError = string.IsNullOrWhiteSpace(email);
            var isPhone = false;
            if (!isError)
            {
                var isEmail = System.Text.RegularExpressions.Regex.IsMatch(email, _emailPattern);
                if (!isEmail)
                {
                    isPhone = CheckIsPhone(email);
                    if (!isPhone) isError = true;
                }
            }

            if (isError)
            {
                _error_label ??= _inputField.parent.Q<Label>("error");
                _error_label.text = "не корректный email или номер!";
                _error_label.style.visibility = Visibility.Visible;
                return;
            }
            PlayerPrefs.SetString("user_email", email);
            OnContinue.Invoke((isPhone,email));
            // continue
        }
    }
}
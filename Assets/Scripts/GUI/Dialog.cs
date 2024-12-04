using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    /// <summary>
    /// Custom Dialog Class
    /// </summary>
    public class Dialog : MonoBehaviour
    {
        private static Dialog _currentDialog;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text txtTitle;
        [SerializeField] private Text txtDesc;
        [SerializeField] private Button btnBack;
        [SerializeField] private InputField inputText;

        [ReadOnly] public DialogResult dialogResult = DialogResult.None;
        private string _resultText;
        private bool _requireText = true;
        private bool _showInput;

        /// <summary>
        /// Sets the dialog result field of this Dialog instance.
        /// Note: Dialog result will not be set if inputfield is enabled, and empty, and text is required.
        /// </summary>
        /// <param name="result">The result of the Dialog.</param>
        public void SetResult(int result)
        {
            var res = (DialogResult)result;
            if (res == DialogResult.Cancel)
            {
                dialogResult = res;
                return;
            }
        
            if (inputText.text.Trim().Length == 0 && _requireText && _showInput)
                return;
        
            dialogResult = res;
        }

        private void CreateDialog(string title, string description, bool showInputField, bool requireTextInput = true, bool okOnly = false)
        {
            dialogResult = DialogResult.None;

            txtTitle.text = title;
            txtDesc.text = description;
            inputText.gameObject.SetActive(showInputField);
            _showInput = showInputField;

            if (showInputField)
                inputText.onValueChanged.AddListener(value => _resultText = value);
            else
                panel.GetComponent<RectTransform>().sizeDelta = new(900, 200);
            
            if (okOnly) btnBack.gameObject.SetActive(false);

            _requireText = requireTextInput;
            ToggleDialog(true);
        }
    
        /// Toggles visibility of the dialog.
        private void ToggleDialog(bool state) => gameObject.SetActive(state);

        /// <summary>
        /// Initialises, and displays an input dialog to the user.
        /// </summary>
        /// <param name="dialog">The dialog prefab to instantiate.</param>
        /// <param name="resultsAction">The callback to retrieve input value.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="description">The subtitle, or descriptive text of the dialog.</param>
        /// <param name="requireTextInput">If showInputField is true, set this to true if text response is required.</param>
        /// <param name="okOnly">Determine whether to hide the "cancel" button.</param>
        public static IEnumerator ShowInputDialog(Dialog dialog, Action<string> resultsAction, string title,
            string description, bool requireTextInput = true, bool okOnly = false)
        {
            yield return new WaitUntil(() => !_currentDialog);
            _currentDialog = dialog;
            var d = Instantiate(dialog);
            d.CreateDialog(title, description, true, requireTextInput, okOnly);
            yield return new WaitWhile(() => d.dialogResult == DialogResult.None);
            if (d.dialogResult == DialogResult.Confirm)
            {
                resultsAction(d._resultText);
            }
            _currentDialog = null;
            Destroy(d.gameObject);
        }

        /// <summary>
        /// Initialises, and displays a message only dialog to the user.
        /// </summary>
        /// <param name="dialog">The dialog prefab to instantiate.</param>
        /// <param name="resultsAction">The callback to retrieve input value.</param>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="description">The subtitle, or descriptive text of the dialog.</param>
        public static IEnumerator ShowMessageDialog(Dialog dialog, Action<DialogResult> resultsAction, string title, string description, bool okOnly = true)
        {
            yield return new WaitUntil(() => !_currentDialog);
            _currentDialog = dialog;
        
            var d = Instantiate(dialog);
            d.CreateDialog(title, description, false, false, okOnly);
            yield return new WaitWhile(() => d.dialogResult == DialogResult.None);
            resultsAction(d.dialogResult);
        
            _currentDialog = null;
            Destroy(d.gameObject);
        }
    }
}

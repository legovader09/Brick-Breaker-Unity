using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    /// <summary>
    /// Custom Dialog Class
    /// </summary>
    public class Dialog : MonoBehaviour
    {
        public Rect position;
        public Text txtTitle;
        public Text txtDesc;
        public Button btnGo;
        public Button btnBack;
        public InputField inputText;

        internal DialogResult DialogResult = DialogResult.None;
        public Text ResultText { get; set; }
        private bool _requireText = true;
        private bool _showInput;

        /// <summary>
        /// Sets the dialog result field of this Dialog instance.
        /// Note: Dialog result will not be set if inputfield is enabled, and empty, and text is required.
        /// </summary>
        /// <param name="result">The numeric result of the DialogResult enumerator.</param>
        public void SetResult(int result)
        {
            if (DialogResult == DialogResult.None)
            {
                if (result == 2)
                {
                    DialogResult = (DialogResult)result;
                }
                else if (result == 1)
                {
                    if (ResultText.text.Length == 0)
                    {
                        if (_requireText && _showInput)
                            return;
                    }
                    else DialogResult = (DialogResult)result;
                }
            }
        }

        // Start is called before the first frame update
        private void Start() => ToggleDialog(true);

        /// <summary>
        /// Initialises, and displays the Dialog to the user.
        /// </summary>
        /// <param name="title">THe title of the dialog.</param>
        /// <param name="description">The subtitle, or descriptive text of the dialog.</param>
        /// <param name="showInputField">If true, will show a text input field.</param>
        /// <param name="requireTextInput">If showInputField is true, set this to true if text response is required.</param>
        /// <param name="okOnly">Show the "Confirm" button only.</param>
        public void CreateDialog(string title, string description, bool showInputField, bool requireTextInput = true, bool okOnly = false)
        {
            DialogResult = DialogResult.None;

            txtTitle.text = title;
            txtDesc.text = description;
            inputText.gameObject.SetActive(showInputField);
            _showInput = showInputField;

            if (showInputField)
                ResultText = inputText.GetComponentInChildren<Text>();

            if (okOnly)
                btnBack.gameObject.SetActive(false);

            _requireText = requireTextInput;
            gameObject.SetActive(true);
        }

        private void ToggleDialog(bool state) => gameObject.SetActive(state); //Toggles visibility of the dialog.

    }
}

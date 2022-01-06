using UnityEngine;
using UnityEngine.UI;

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

    internal DialogResult dialogResult = DialogResult.None;
    public Text ResultText { get; set; }
    private bool requireText = true;
    private bool showInput = false;

    /// <summary>
    /// Sets the dialog result field of this Dialog instance.
    /// Note: Dialog result will not be set if inputfield is enabled, and empty, and text is required.
    /// </summary>
    /// <param name="result">The numeric result of the DialogResult enumerator.</param>
    public void SetResult(int result)
    {
        if (dialogResult == DialogResult.None)
        {
            if (result == 2)
            {
                dialogResult = (DialogResult)result;
            }
            else if (result == 1)
            {
                if (ResultText.text.Length == 0)
                {
                    if (requireText && showInput)
                        return;
                }
                else dialogResult = (DialogResult)result;
            }
        }
    }

    // Start is called before the first frame update
    void Start() => ToggleDialog(true);

    /// <summary>
    /// Initialises, and displays the Dialog to the user.
    /// </summary>
    /// <param name="title">THe title of the dialog.</param>
    /// <param name="description">The subtitle, or descriptive text of the dialog.</param>
    /// <param name="showInputField">If true, will show a text input field.</param>
    /// <param name="requireTextInput">If showInputField is true, set this to true if text response is required.</param>
    /// <param name="OkOnly">Show the "Confirm" button only.</param>
    public void CreateDialog(string title, string description, bool showInputField, bool requireTextInput = true, bool OkOnly = false)
    {
        dialogResult = DialogResult.None;

        txtTitle.text = title;
        txtDesc.text = description;
        inputText.gameObject.SetActive(showInputField);
        showInput = showInputField;

        if (showInputField)
            ResultText = inputText.GetComponentInChildren<Text>();

        if (OkOnly)
            btnBack.gameObject.SetActive(false);

        requireText = requireTextInput;
        gameObject.SetActive(true);
    }

    void ToggleDialog(bool state) => gameObject.SetActive(state); //Toggles visibility of the dialog.

}

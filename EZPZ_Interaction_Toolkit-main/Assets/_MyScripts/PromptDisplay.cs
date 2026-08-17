using UnityEngine;
using UnityEngine.UI;

public class PromptDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI GameObject to show/hide (e.g. ShowInfo)")]
    public GameObject targetUI;

    [Tooltip("The Text component to update")]
    public Text targetText;

    [Header("Prompt Text")]
    [Tooltip("Text to display when Show() is called")]
    public string message;

    public void Show()
    {
        if (targetText != null)
            targetText.text = message;
        if (targetUI != null)
            ScreenMessageGate.Arm(targetUI);
        messageToken = ScreenMessageGate.Begin();
    }

    public void Hide()
    {
        if (targetUI != null && ScreenMessageGate.CanHide(messageToken))
            targetUI.SetActive(false);
    }

    private uint messageToken;
}

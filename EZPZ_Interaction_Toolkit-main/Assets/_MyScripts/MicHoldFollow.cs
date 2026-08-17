using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MicHoldFollow : MonoBehaviour
{
    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message = "I don\u2019t know if I can finish it tonight.";

    private InteractableGeneral interactable;
    private bool triggered = false;

    void Start()
    {
        interactable = GetComponent<InteractableGeneral>();

        if (interactable != null)
        {
            interactable.onPrimaryInteract.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (triggered) return;
        triggered = true;

        AudioManager.Instance.PlaySound("Mic", OnMicFinished);
    }

    private void OnMicFinished()
    {
        TimedFlickerEvent.NotifyObjectExplored("mic");

        if (showInfoText != null)
            showInfoText.text = message;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);
    }
}

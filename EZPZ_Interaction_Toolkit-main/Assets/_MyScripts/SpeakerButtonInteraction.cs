using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeakerButtonInteraction : MonoBehaviour
{
    [Header("ShowInfo UI")]
    public GameObject showInfo;
    public string message = "Now, step onto the stage. The song is still waiting for you to finish it.";

    [Header("Stage References")]
    public Light stageSpotlight;
    public MonoBehaviour stageTrigger;

    private bool hasBeenPressed = false;

    void Start()
    {
        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
        {
            interactable.onPrimaryInteract.AddListener(OnButtonPressed);
        }
    }

    void OnButtonPressed()
    {
        if (hasBeenPressed) return;
        hasBeenPressed = true;

        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
            interactable.hoverText = "";

        if (showInfo != null)
        {
            showInfo.SetActive(true);
            var text = showInfo.GetComponent<Text>();
            if (text == null)
                text = showInfo.GetComponentInChildren<Text>();
            if (text != null)
                text.text = message;
        }

        // Play Guitar, Piano, Vocal simultaneously via AudioManager
        AudioManager.Instance.PlaySimultaneous(OnAllAudioFinished, "Guitar", "Piano", "Vocal");
    }

    void OnAllAudioFinished()
    {
        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(true);

        if (stageTrigger != null)
            stageTrigger.gameObject.SetActive(true);
    }
}

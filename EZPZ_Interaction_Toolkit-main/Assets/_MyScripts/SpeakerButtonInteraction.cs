using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeakerButtonInteraction : MonoBehaviour
{
    [Header("ShowInfo UI")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message = "Now, step onto the stage. The song is still waiting for you to finish it.";

    [Header("Stage References")]
    public Light stageSpotlight;
    public MonoBehaviour stageTrigger;

    [Header("Post Audio Sequence")]
    public GameObject doorCloseTriggerZone;
    public GameObject selectImage;
    public Animator doorAnimator;
    public string doorOpenTrigger = "open";
    public string endMessage = "Go back to the corridor.";
    public float endMessageDuration = 5f;

    private bool audioPressed = false;
    private bool q3Pressed = false;
    private MonoBehaviour starterInputs;

    void Start()
    {
        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
        {
            interactable.onPrimaryInteract.AddListener(OnButtonPressed);
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            starterInputs = player.GetComponent("StarterAssetsInputs") as MonoBehaviour;
    }

    // Called by Q3 button onClick — goes straight to EndSequence
    public void OnQ3Pressed()
    {
        if (q3Pressed) return;
        q3Pressed = true;
        StartCoroutine(EndSequence());
    }

    // Called by ButtonCap InteractableGeneral — plays audio first, then EndSequence
    public void OnButtonPressed()
    {
        if (audioPressed) return;
        audioPressed = true;

        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
            interactable.hoverText = "";

        if (showInfoText != null)
            showInfoText.text = message;
        if (showInfo != null)
            showInfo.SetActive(true);

        AudioManager.Instance.PlaySimultaneous(OnAllAudioFinished, "Guitar", "Piano", "Vocal");
    }

    void OnAllAudioFinished()
    {
        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(true);

        if (stageTrigger != null)
            stageTrigger.gameObject.SetActive(true);

        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        if (doorCloseTriggerZone != null)
            doorCloseTriggerZone.SetActive(true);

        if (selectImage != null)
            selectImage.SetActive(false);

        LockCursor();

        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        if (showInfoText != null)
            showInfoText.text = endMessage;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(endMessageDuration);

        if (showInfo != null)
            showInfo.SetActive(false);
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (starterInputs != null)
        {
            starterInputs.GetType().GetField("cursorLocked").SetValue(starterInputs, true);
            starterInputs.GetType().GetField("cursorInputForLook").SetValue(starterInputs, true);
        }
    }
}

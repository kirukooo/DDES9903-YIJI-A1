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

    [Header("Q1 Flow")]
    public GameObject textTMP4;
    public GameObject recordTriggerZone;
    public string q1Message = "Now, please head to the stage.";
    public float q1MessageDuration = 5f;

    private bool audioPressed = false;
    private bool q3Pressed = false;
    private bool q1Pressed = false;
    private MonoBehaviour starterInputs;
    private uint messageToken;

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

    // Called by Q1 button onClick
    public void OnQ1Pressed()
    {
        if (q1Pressed) return;
        q1Pressed = true;
        PowerBoxController.EndingTracker.Select(PowerBoxController.EndingChoice.A);
        StartCoroutine(Q1Sequence());
    }

    private IEnumerator Q1Sequence()
    {
        if (selectImage != null)
            selectImage.SetActive(false);

        LockCursor();

        if (showInfoText != null)
            showInfoText.text = q1Message;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);
        messageToken = ScreenMessageGate.Begin();

        yield return new WaitForSeconds(q1MessageDuration);

        if (showInfo != null && ScreenMessageGate.CanHide(messageToken))
            showInfo.SetActive(false);

        if (textTMP4 != null)
            textTMP4.SetActive(true);

        if (recordTriggerZone != null)
            recordTriggerZone.SetActive(true);
    }

    // Called by Q3 button onClick — goes straight to EndSequence
    public void OnQ3Pressed()
    {
        if (q3Pressed) return;
        q3Pressed = true;
        PowerBoxController.EndingTracker.Select(PowerBoxController.EndingChoice.C);
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

        if (q1Pressed)
        {
            if (stageTrigger is WuTaiTrigger wtt)
                wtt.q1Flow = true;
            if (stageTrigger != null)
                stageTrigger.gameObject.SetActive(true);
        }
        else
        {
            AudioManager.Instance.PlaySimultaneous(OnAllAudioFinished, "Guitar", "Piano", "Vocal");
        }
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
            ScreenMessageGate.Arm(showInfo);
        messageToken = ScreenMessageGate.Begin();

        yield return new WaitForSeconds(endMessageDuration);

        if (showInfo != null && ScreenMessageGate.CanHide(messageToken))
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

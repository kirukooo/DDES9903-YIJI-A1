using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PaperDropAudio : MonoBehaviour
{
    [Header("Door")]
    public Animator doorAnimator;

    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message = "The melody returned, but the final bars are still empty.";

    public void PlayPiano()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger("close");

        AudioManager.Instance.PlaySound("Piano", OnPianoFinished);
    }

    private void OnPianoFinished()
    {
        if (showInfoText != null)
            showInfoText.text = message;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);

        StartCoroutine(DelayedNotify());
    }

    private IEnumerator DelayedNotify()
    {
        yield return new WaitForSeconds(5f);
        TimedFlickerEvent.NotifyObjectExplored("paper");
    }
}

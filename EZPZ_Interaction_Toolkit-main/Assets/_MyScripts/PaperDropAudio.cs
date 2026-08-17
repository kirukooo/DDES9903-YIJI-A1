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
    public float messageDuration = 5f;

    public void PlayPiano()
    {
        if (doorAnimator != null)
            doorAnimator.SetTrigger("close");

        AudioManager.Instance.PlaySound("Piano", OnPianoFinished);
    }

    private void OnPianoFinished()
    {
        TimedFlickerEvent.NotifyObjectExplored("paper");

        if (showInfoText != null)
            showInfoText.text = message;
        if (showInfo != null)
        {
            showInfo.SetActive(true);
            StartCoroutine(HideAfterSeconds());
        }
    }

    private IEnumerator HideAfterSeconds()
    {
        yield return new WaitForSeconds(messageDuration);
        if (showInfo != null)
            showInfo.SetActive(false);
    }
}

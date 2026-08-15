using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimedFlickerEvent : MonoBehaviour
{
    [Header("Timing")]
    public float flickerDelay = 240f; // 4 minutes
    public float flickerDuration = 2f;
    public float message1Duration = 5f;
    public float message2Duration = 2f;
    public float message3Duration = 5f;
    public float message4Duration = 5f;

    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message1 = "Final rehearsal detected. Playing unsaved session.";
    public string message2 = "We have started again too many times.";
    public string message3 = "The final rehearsal is playing back.";
    public string message4 = "Stop.\nIt will never become the old version again.";

    [Header("Audio")]
    public string audio1 = "6";
    public string audio2 = "7";

    private void Start()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        yield return new WaitForSeconds(flickerDelay);

        // === Phase 1: Lights flicker ===
        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var l in lights)
            l.enabled = false;

        yield return new WaitForSeconds(flickerDuration);

        foreach (var l in lights)
            l.enabled = true;

        // === Phase 2: Message 1 → Audio 6 ===
        ShowMessage(message1);
        yield return new WaitForSeconds(message1Duration);
        HideShowInfo();

        bool audio1Done = false;
        AudioManager.Instance.PlaySound(audio1, () => audio1Done = true);
        yield return new WaitUntil(() => audio1Done);

        // === Phase 3: Message 2 (2s) → Audio 7 ===
        ShowMessage(message2);
        yield return new WaitForSeconds(message2Duration);
        HideShowInfo();

        // Play audio 7, show message 3 simultaneously
        bool audio2Done = false;
        AudioManager.Instance.PlaySound(audio2, () => audio2Done = true);

        ShowMessage(message3);
        yield return new WaitForSeconds(message3Duration);
        HideShowInfo();

        // Wait for audio 7 to finish
        yield return new WaitUntil(() => audio2Done);

        // === Phase 4: Lights off → Message 4 ===
        foreach (var l in lights)
            l.enabled = false;

        ShowMessage(message4);
        yield return new WaitForSeconds(message4Duration);
        HideShowInfo();
    }

    private void ShowMessage(string msg)
    {
        if (showInfoText != null)
            showInfoText.text = msg;
        if (showInfo != null)
            showInfo.SetActive(true);
    }

    private void HideShowInfo()
    {
        if (showInfo != null)
            showInfo.SetActive(false);
    }
}

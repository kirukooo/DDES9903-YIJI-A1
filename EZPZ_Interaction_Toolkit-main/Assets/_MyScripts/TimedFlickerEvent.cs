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

    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message1 = "Final rehearsal detected. Playing unsaved session.";
    public string message2 = "We have started again too many times.";

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

        // Get all lights in scene
        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        // Turn off all lights
        foreach (var l in lights)
            l.enabled = false;

        yield return new WaitForSeconds(flickerDuration);

        // Turn lights back on
        foreach (var l in lights)
            l.enabled = true;

        // Show message 1
        if (showInfoText != null)
            showInfoText.text = message1;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(message1Duration);

        if (showInfo != null)
            showInfo.SetActive(false);

        // Play audio 1
        bool audio1Done = false;
        AudioManager.Instance.PlaySound(audio1, () => audio1Done = true);
        yield return new WaitUntil(() => audio1Done);

        // Show message 2
        if (showInfoText != null)
            showInfoText.text = message2;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(message2Duration);

        if (showInfo != null)
            showInfo.SetActive(false);

        // Play audio 2
        AudioManager.Instance.PlaySound(audio2);
    }
}

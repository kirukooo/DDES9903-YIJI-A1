using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WuTaiTrigger : MonoBehaviour
{
    public Light stageSpotlight;
    public GameObject stageLightCircle;
    public GameObject showInfo;
    public Text showInfoText;
    public string message = "Now, please step onto the stage.\nThis song is still waiting for you to finish it.\nPlease walk into the spotlight.";

    [Header("Q1 Flow")]
    public GameObject juGuangDeng;
    public Animator doorAnimator;
    public string doorOpenTrigger = "open";
    public GameObject doorCloseTriggerZone;
    public string q1EndMessage1 = "You made one that could exist now.\nThis time, the stage belonged to you.";
    public string q1EndMessage2 = "Leave the room.";
    public float q1MessageDuration = 5f;
    public float lightFadeDuration = 1f;
    public float finalLightIntensity = 0.3f;
    public float juGuangDengFinalIntensity = 1f;

    [HideInInspector] public bool q1Flow = false;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggered) return;
        triggered = true;

        if (q1Flow)
        {
            StartCoroutine(Q1FlowSequence());
        }
        else
        {
            if (stageSpotlight != null)
                stageSpotlight.gameObject.SetActive(true);

            if (stageLightCircle != null)
                stageLightCircle.SetActive(true);

            if (showInfo != null)
                showInfo.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (q1Flow) return;

        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(false);

        if (stageLightCircle != null)
            stageLightCircle.SetActive(false);
    }

    private IEnumerator Q1FlowSequence()
    {
        if (juGuangDeng != null)
            juGuangDeng.SetActive(true);

        bool audioDone = false;
        AudioManager.Instance.PlaySound("dian", () => audioDone = true);
        yield return new WaitUntil(() => audioDone);

        if (juGuangDeng != null)
        {
            var jgdLight = juGuangDeng.GetComponent<Light>();
            if (jgdLight != null)
                DOTween.To(() => jgdLight.intensity, v => jgdLight.intensity = v, juGuangDengFinalIntensity, lightFadeDuration);
        }

        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (juGuangDeng != null && l.gameObject == juGuangDeng) continue;
            DOTween.To(() => l.intensity, v => l.intensity = v, finalLightIntensity, lightFadeDuration);
        }

        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        if (doorCloseTriggerZone != null)
            doorCloseTriggerZone.SetActive(true);

        if (showInfoText != null)
            showInfoText.text = q1EndMessage1;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(q1MessageDuration);

        if (showInfoText != null)
            showInfoText.text = q1EndMessage2;

        yield return new WaitForSeconds(q1MessageDuration);

        if (showInfo != null)
            showInfo.SetActive(false);
    }
}

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
    public string fullSongAudioName = "Full";

    [HideInInspector] public bool q1Flow = false;

    private bool triggered = false;
    private uint messageToken;

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
        // 聚光灯亮起
        if (juGuangDeng != null)
        {
            juGuangDeng.SetActive(true);
            var jgdLight = juGuangDeng.GetComponent<Light>();
            if (jgdLight != null)
                DOTween.To(() => jgdLight.intensity, v => jgdLight.intensity = v, juGuangDengFinalIntensity, lightFadeDuration);
        }

        // 录音提示音
        bool dianDone = false;
        AudioManager.Instance.PlaySound("dian", () => dianDone = true);
        yield return new WaitUntil(() => dianDone);

        // 播放完整歌曲(8),玩家视角不锁定,可自由走动
        bool fullDone = false;
        AudioManager.Instance.PlaySound(fullSongAudioName, () => fullDone = true);
        yield return new WaitUntil(() => fullDone);

        // 歌曲播完:聚光灯暗
        if (juGuangDeng != null)
        {
            var jgdLight = juGuangDeng.GetComponent<Light>();
            if (jgdLight != null)
                DOTween.To(() => jgdLight.intensity, v => jgdLight.intensity = v, 0f, lightFadeDuration);
        }

        // 全屋变暗但不要全黑
        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (juGuangDeng != null && l.gameObject == juGuangDeng) continue;
            DOTween.To(() => l.intensity, v => l.intensity = v, finalLightIntensity, lightFadeDuration);
        }

        // 出口门打开
        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        if (doorCloseTriggerZone != null)
            doorCloseTriggerZone.SetActive(true);

        if (showInfoText != null)
            showInfoText.text = q1EndMessage1;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);
        messageToken = ScreenMessageGate.Begin();

        yield return new WaitForSeconds(q1MessageDuration);

        if (showInfoText != null)
            showInfoText.text = q1EndMessage2;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);
        messageToken = ScreenMessageGate.Begin();

        yield return new WaitForSeconds(q1MessageDuration);

        if (showInfo != null && ScreenMessageGate.CanHide(messageToken))
            showInfo.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("Select Image")]
    public GameObject selectImage;
    public float selectImageDelay = 2f;

    [Header("Stage Spotlight (Audio 7)")]
    public GameObject stageSpotlight;
    public float roomBrightenMultiplier = 1.6f;
    public float spotlightFadeDuration = 1f;

    private MonoBehaviour starterInputs;

    private Light spotlightLight;
    private readonly List<Light> roomLights = new List<Light>();
    private readonly List<float> roomLightIntensities = new List<float>();

    private static TimedFlickerEvent instance;
    private static readonly HashSet<string> exploredIds = new HashSet<string>();
    private bool conflictTriggered = false;
    private Coroutine timerRoutine;

    public const int RequiredExploreCount = 4;

    void Awake()
    {
        instance = this;

        // 未手动指定时自动查找场景根对象 StageSpotlight(默认未激活,GameObject.Find 找不到)
        if (stageSpotlight == null)
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                if (root.name == "StageSpotlight")
                {
                    stageSpotlight = root;
                    break;
                }
            }
        }

        if (stageSpotlight != null)
            spotlightLight = stageSpotlight.GetComponentInChildren<Light>(true);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    /// <summary>
    /// 四个物件(纸/吉他箱/录音机/麦克风)完成时调用;集齐即提前触发冲突事件。
    /// </summary>
    public static void NotifyObjectExplored(string objectId)
    {
        if (string.IsNullOrEmpty(objectId) || !exploredIds.Add(objectId)) return;

        Debug.Log($"[TimedFlickerEvent] Object explored: {objectId} ({exploredIds.Count}/{RequiredExploreCount})");

        if (exploredIds.Count >= RequiredExploreCount)
            instance?.TryTriggerConflict();
    }

    /// <summary>
    /// 触发冲突事件(全物件探索完成或4分钟计时,二选一,只触发一次)。
    /// </summary>
    public void TryTriggerConflict()
    {
        if (conflictTriggered) return;
        conflictTriggered = true;

        if (timerRoutine != null)
        {
            StopCoroutine(timerRoutine);
            timerRoutine = null;
        }

        StartCoroutine(FlickerRoutine());
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            starterInputs = player.GetComponent("StarterAssetsInputs") as MonoBehaviour;

        timerRoutine = StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(flickerDelay);
        TryTriggerConflict();
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (starterInputs != null)
        {
            starterInputs.GetType().GetField("cursorLocked").SetValue(starterInputs, false);
            starterInputs.GetType().GetField("cursorInputForLook").SetValue(starterInputs, false);
        }
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

    private IEnumerator FlickerRoutine()
    {
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

        // === 舞台聚光灯亮,同时房间整体变亮 ===
        if (stageSpotlight != null)
            stageSpotlight.SetActive(true);

        roomLights.Clear();
        roomLightIntensities.Clear();
        foreach (var l in lights)
        {
            if (stageSpotlight != null && l.transform.IsChildOf(stageSpotlight.transform)) continue;

            roomLights.Add(l);
            roomLightIntensities.Add(l.intensity);
            var lightRef = l;
            DOTween.To(() => lightRef.intensity, v => lightRef.intensity = v,
                lightRef.intensity * roomBrightenMultiplier, spotlightFadeDuration);
        }

        ShowMessage(message3);
        yield return new WaitForSeconds(message3Duration);
        HideShowInfo();

        // Wait for audio 7 to finish
        yield return new WaitUntil(() => audio2Done);

        // === Phase 4: 聚光灯暗,房间灯光恢复原亮度 ===
        if (spotlightLight != null)
            DOTween.To(() => spotlightLight.intensity, v => spotlightLight.intensity = v, 0f, spotlightFadeDuration)
                .OnComplete(() =>
                {
                    if (stageSpotlight != null)
                        stageSpotlight.SetActive(false);
                });

        for (int i = 0; i < roomLights.Count; i++)
        {
            var lightRef = roomLights[i];
            var target = roomLightIntensities[i];
            DOTween.To(() => lightRef.intensity, v => lightRef.intensity = v, target, spotlightFadeDuration);
        }

        ShowMessage(message4);
        yield return new WaitForSeconds(message4Duration);
        HideShowInfo();

        // === Phase 5: Select Image ===
        yield return new WaitForSeconds(selectImageDelay);
        if (selectImage != null)
            selectImage.SetActive(true);

        UnlockCursor();
    }

    private uint messageToken;

    private void ShowMessage(string msg)
    {
        if (showInfoText != null)
            showInfoText.text = msg;
        if (showInfo != null)
            ScreenMessageGate.Arm(showInfo);
        messageToken = ScreenMessageGate.Begin();
    }

    private void HideShowInfo()
    {
        if (showInfo != null && ScreenMessageGate.CanHide(messageToken))
            showInfo.SetActive(false);
    }
}

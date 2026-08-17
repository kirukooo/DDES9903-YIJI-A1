using System.Collections;
using System.Collections.Generic;
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

    [Header("Select Image")]
    public GameObject selectImage;
    public float selectImageDelay = 2f;

    private MonoBehaviour starterInputs;

    private static TimedFlickerEvent instance;
    private static readonly HashSet<string> exploredIds = new HashSet<string>();
    private bool conflictTriggered = false;
    private Coroutine timerRoutine;

    public const int RequiredExploreCount = 4;

    void Awake()
    {
        instance = this;
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

        // === Phase 5: Select Image ===
        yield return new WaitForSeconds(selectImageDelay);
        if (selectImage != null)
            selectImage.SetActive(true);

        UnlockCursor();
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

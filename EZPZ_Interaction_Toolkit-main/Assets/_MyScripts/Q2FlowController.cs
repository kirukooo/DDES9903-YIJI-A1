using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Q2FlowController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject showInfo;
    public Text showInfoText;
    public GameObject selectImage;

    [Header("Trigger Zones")]
    public GameObject recTriggerZone;

    [Header("Cassette & Archive")]
    public Renderer cassetteRenderer;
    public Holdable cassetteHoldable;
    public Renderer[] archBoxRenderers;
    public Transform archDoor;
    public Vector3 archDoorClosedRotation = Vector3.zero;
    public float archDoorRotateDuration = 0.5f;

    [Header("Door")]
    public Animator doorAnimator;
    public string doorOpenTrigger = "open";

    [Header("Messages")]
    public string introMessage = "It's still in the tape recorder.";
    public float introMessageDuration = 5f;
    public string placeMessage = "Place the cassette in the archive box.";
    public string closeLidMessage = "Close the lid.";
    public string endMessage = "The song remained unfinished.\nSome memories survive because they are not repaired.\nLeave the room.";

    [Header("Highlight")]
    public Color outlineColor = new Color(1f, 0.85f, 0.3f, 1f);
    public float outlineWidth = 0.02f;

    private bool q2Pressed = false;
    private bool cassetteTriggered = false;
    private bool cassettePlaced = false;

    private Material outlineMat;
    private List<Material[]> archBoxOriginalMats = new List<Material[]>();

    private MonoBehaviour starterInputs;

    private Collider cassetteCollider;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            starterInputs = player.GetComponent("StarterAssetsInputs") as MonoBehaviour;

        CacheArchBoxMaterials();
        CreateOutlineMaterial();

        if (cassetteHoldable != null)
        {
            cassetteHoldable.enabled = false;
            cassetteCollider = cassetteHoldable.GetComponent<Collider>();
            if (cassetteCollider != null)
                cassetteCollider.enabled = false;
        }
    }

    public void OnQ2Pressed()
    {
        if (q2Pressed) return;
        q2Pressed = true;
        PowerBoxController.EndingTracker.Select(PowerBoxController.EndingChoice.B);
        StartCoroutine(Q2Sequence());
    }

    private IEnumerator Q2Sequence()
    {
        if (selectImage != null)
            selectImage.SetActive(false);

        LockCursor();

        if (showInfoText != null)
            showInfoText.text = introMessage;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(introMessageDuration);

        if (showInfo != null)
            showInfo.SetActive(false);

        if (recTriggerZone != null)
            recTriggerZone.SetActive(true);

        if (cassetteHoldable != null)
        {
            cassetteHoldable.enabled = true;
            if (cassetteCollider == null)
                cassetteCollider = cassetteHoldable.GetComponent<Collider>();
            if (cassetteCollider != null)
                cassetteCollider.enabled = true;
        }
    }

    public void OnCassetteTriggered()
    {
        if (cassetteTriggered) return;
        cassetteTriggered = true;

        if (showInfoText != null)
            showInfoText.text = placeMessage;
        if (showInfo != null)
            showInfo.SetActive(true);

        HighlightArchBox(true);
    }

    public void OnCassettePlaced()
    {
        if (cassettePlaced) return;
        cassettePlaced = true;

        StartCoroutine(CassettePlacedSequence());
    }

    private IEnumerator CassettePlacedSequence()
    {
        if (showInfoText != null)
            showInfoText.text = closeLidMessage;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (archDoor != null)
            archDoor.DOLocalRotate(archDoorClosedRotation, archDoorRotateDuration);

        yield return new WaitForSeconds(archDoorRotateDuration);

        HighlightArchBox(false);

        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        yield return new WaitForSeconds(0.5f);

        if (showInfoText != null)
            showInfoText.text = endMessage;
        if (showInfo != null)
            showInfo.SetActive(true);
    }

    private void CacheArchBoxMaterials()
    {
        for (int i = 0; i < archBoxRenderers.Length; i++)
        {
            if (archBoxRenderers[i] == null) continue;
            archBoxOriginalMats.Add(archBoxRenderers[i].sharedMaterials);
        }
    }

    private void CreateOutlineMaterial()
    {
        var shader = Shader.Find("Custom/OutlineHighlight");
        if (shader == null)
        {
            Debug.LogError("[Q2FlowController] Custom/OutlineHighlight shader not found!");
            return;
        }
        outlineMat = new Material(shader);
        outlineMat.SetColor("_OutlineColor", outlineColor);
        outlineMat.SetFloat("_OutlineWidth", outlineWidth);
    }

    private void HighlightArchBox(bool on)
    {
        if (outlineMat == null) return;

        for (int i = 0; i < archBoxRenderers.Length; i++)
        {
            if (archBoxRenderers[i] == null) continue;

            if (on)
            {
                var mats = archBoxRenderers[i].materials;
                var newMats = new Material[mats.Length + 1];
                for (int j = 0; j < mats.Length; j++)
                    newMats[j] = mats[j];
                newMats[mats.Length] = outlineMat;
                archBoxRenderers[i].materials = newMats;
            }
            else
            {
                if (i < archBoxOriginalMats.Count)
                    archBoxRenderers[i].materials = archBoxOriginalMats[i];
            }
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
}

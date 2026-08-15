using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PowerBoxController : MonoBehaviour
{
    [Header("Indicator Light")]
    public Renderer indicatorLightRenderer;
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
    public Color offColor = new Color(1f, 0.1f, 0.1f);
    public Color onColor = new Color(0.1f, 1f, 0.15f);
    public float emissionIntensity = 3f;

    [Header("Lever")]
    public Transform leverPivot;
    public float leverAngleOff = -30f;
    public float leverAngleOn = 30f;
    public float leverAnimDuration = 0.4f;
    public InteractableGeneral leverInteractable;

    [Header("Labels")]
    public TextMeshProUGUI offLabelText;
    public TextMeshProUGUI onLabelText;
    public Color labelActiveColor = Color.white;
    public Color labelDimColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("Startup Delay")]
    public float startupDelay = 20f;

    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string endMessage = "The room no longer had to remember it. The room became a room again. Some works are completed, some are kept, and some simply stop.";
    public float endMessageDuration = 10f;
    public float lightFadeDuration = 1f;

    [Header("State")]
    public bool isOn = false;

    [Header("Events")]
    public UnityEvent onPowerOn;

    private Material indicatorMat;

    private void Start()
    {
        if (indicatorLightRenderer != null)
            indicatorMat = indicatorLightRenderer.material;

        if (leverPivot != null)
            leverPivot.localRotation = Quaternion.Euler(leverAngleOff, 0, 0);

        SetDisabledState();
        StartCoroutine(StartupDelayRoutine());
    }

    private IEnumerator StartupDelayRoutine()
    {
        yield return new WaitForSeconds(startupDelay);
        ApplyVisualState();
        if (leverInteractable != null)
            leverInteractable.enabled = true;
    }

    private void SetDisabledState()
    {
        if (indicatorMat != null)
        {
            indicatorMat.SetColor("_EmissionColor", Color.black);
            indicatorMat.SetColor("_BaseColor", disabledColor);
            indicatorMat.DisableKeyword("_EMISSION");
        }

        if (offLabelText != null)
            offLabelText.color = labelDimColor;
        if (onLabelText != null)
            onLabelText.color = labelDimColor;

        if (leverInteractable != null)
            leverInteractable.enabled = false;
    }

    public void TogglePower()
    {
        if (isOn)
        {
            // 第二次点击：关闭电源
            isOn = false;
            ApplyVisualState();

            if (leverPivot != null)
                leverPivot.DOLocalRotate(Vector3.zero, leverAnimDuration);

            StartCoroutine(EndSequence());
        }
        else
        {
            // 第一次点击：开启电源
            isOn = true;
            ApplyVisualState();

            if (leverPivot != null)
                leverPivot.DOLocalRotate(new Vector3(leverAngleOn, 0, 0), leverAnimDuration);

            AudioManager.Instance.PlaySound("OpenDoor");

            onPowerOn?.Invoke();
        }
    }

    private IEnumerator EndSequence()
    {
        if (showInfoText != null)
            showInfoText.text = endMessage;
        if (showInfo != null)
            showInfo.SetActive(true);

        yield return new WaitForSeconds(endMessageDuration);

        if (showInfo != null)
            showInfo.SetActive(false);

        var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            DOTween.To(() => l.intensity, v => l.intensity = v, 0f, lightFadeDuration);
        }
    }

    private void ApplyVisualState()
    {
        if (indicatorMat != null)
        {
            Color c = isOn ? onColor : offColor;
            indicatorMat.SetColor("_EmissionColor", c * emissionIntensity);
            indicatorMat.SetColor("_BaseColor", c);
            indicatorMat.EnableKeyword("_EMISSION");
        }

        if (offLabelText != null)
            offLabelText.color = isOn ? labelDimColor : Color.red;

        if (onLabelText != null)
            onLabelText.color = isOn ? Color.green : labelDimColor;
    }
}

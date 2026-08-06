using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class PowerBoxController : MonoBehaviour
{
    [Header("Indicator Light")]
    public Renderer indicatorLightRenderer;
    public Color offColor = new Color(1f, 0.1f, 0.1f);
    public Color onColor = new Color(0.1f, 1f, 0.15f);
    public float emissionIntensity = 3f;

    [Header("Lever")]
    public Transform leverPivot;
    public float leverAngleOff = -30f;
    public float leverAngleOn = 30f;
    public float leverAnimDuration = 0.4f;

    [Header("Labels")]
    public TextMeshProUGUI offLabelText;
    public TextMeshProUGUI onLabelText;
    public Color labelActiveColor = Color.white;
    public Color labelDimColor = new Color(0.3f, 0.3f, 0.3f);

    [Header("State")]
    public bool isOn = false;

    [Header("Events")]
    public UnityEvent onPowerOn;

    private Material indicatorMat;

    private void Start()
    {
        if (indicatorLightRenderer != null)
            indicatorMat = indicatorLightRenderer.material;

        ApplyVisualState();

        if (leverPivot != null)
            leverPivot.localRotation = Quaternion.Euler(leverAngleOff, 0, 0);
    }

    public void TogglePower()
    {
        if (isOn) return;
        isOn = true;
        ApplyVisualState();

        if (leverPivot != null)
            leverPivot.DOLocalRotate(new Vector3(leverAngleOn, 0, 0), leverAnimDuration);

        onPowerOn?.Invoke();
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

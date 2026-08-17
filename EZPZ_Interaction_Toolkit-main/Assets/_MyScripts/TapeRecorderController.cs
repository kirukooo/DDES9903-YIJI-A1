using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class TapeRecorderController : MonoBehaviour
{
    [Header("Tape Reels")]
    public Transform leftReel;
    public Transform rightReel;
    public float reelSpeed = 360f;
    public float reelFadeDuration = 0.5f;

    [Header("Buttons")]
    public Renderer playButtonRenderer;
    public Color playButtonIdleColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color playButtonActiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    [Header("Audio")]
    public string audioName = "Redio";

    [Header("Show Info")]
    public GameObject showInfo;
    public Text showInfoText;
    public string message = "The recording stopped on that day.";

    [Header("State")]
    public bool isPlaying = false;

    [Header("Events")]
    public UnityEvent onPlay;

    private Tween leftReelTween;
    private Tween rightReelTween;
    private Material playButtonMat;

    private void Start()
    {
        if (playButtonRenderer != null)
            playButtonMat = playButtonRenderer.material;

        ApplyButtonState();
    }

    public void TogglePlay()
    {
        if (isPlaying)
        {
            StopPlayback();
        }
        else
        {
            StartPlayback();
        }
    }

    public void StartPlayback()
    {
        if (isPlaying) return;
        isPlaying = true;
        ApplyButtonState();

        if (leftReel != null)
        {
            leftReelTween = leftReel.DOLocalRotate(
                new Vector3(0, 0, 360), 1f / (reelSpeed / 360f), RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        if (rightReel != null)
        {
            rightReelTween = rightReel.DOLocalRotate(
                new Vector3(0, 0, 360), 1f / (reelSpeed / 360f), RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        onPlay?.Invoke();

        AudioManager.Instance.PlaySound(audioName, OnAudioFinished);
    }

    private void OnAudioFinished()
    {
        StopPlayback();

        TimedFlickerEvent.NotifyObjectExplored("recorder");

        if (showInfoText != null)
            showInfoText.text = message;
        if (showInfo != null)
            showInfo.SetActive(true);
    }

    public void StopPlayback()
    {
        if (!isPlaying) return;
        isPlaying = false;
        ApplyButtonState();

        leftReelTween?.Kill();
        rightReelTween?.Kill();

        if (leftReel != null)
            leftReel.DOLocalRotate(leftReel.localEulerAngles, reelFadeDuration);
        if (rightReel != null)
            rightReel.DOLocalRotate(rightReel.localEulerAngles, reelFadeDuration);
    }

    private void ApplyButtonState()
    {
        if (playButtonMat != null)
        {
            playButtonMat.SetColor("_BaseColor", isPlaying ? playButtonActiveColor : playButtonIdleColor);
            if (isPlaying)
            {
                playButtonMat.EnableKeyword("_EMISSION");
                playButtonMat.SetColor("_EmissionColor", playButtonActiveColor * 0.5f);
            }
            else
            {
                playButtonMat.DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnDestroy()
    {
        leftReelTween?.Kill();
        rightReelTween?.Kill();
    }
}

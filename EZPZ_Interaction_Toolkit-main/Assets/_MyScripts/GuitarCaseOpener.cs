using DG.Tweening;
using UnityEngine;

public class GuitarCaseOpener : MonoBehaviour
{
    [Header("Lid Settings")]
    public Transform lidPivot;
    public float openAngle = -120f;
    public float duration = 1f;

    private bool isOpened = false;
    private bool isAnimating = false;
    private InteractableGeneral interactable;
    private AudioSource audioSource;

    void Start()
    {
        interactable = GetComponent<InteractableGeneral>();
        audioSource = GetComponent<AudioSource>();

        if (interactable != null)
        {
            interactable.onPrimaryInteract.AddListener(Open);
        }
    }

    public void Open()
    {
        if (isOpened || isAnimating) return;

        isAnimating = true;

        lidPivot.DOLocalRotate(new Vector3(openAngle, 0, 0), duration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isAnimating = false;
                isOpened = true;

                if (audioSource != null && audioSource.clip != null)
                    audioSource.Play();
            });
    }
}

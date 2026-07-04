using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeakerButtonInteraction : MonoBehaviour
{
    [Header("ShowInfo UI")]
    public GameObject showInfo;
    public string message = "Now, step onto the stage. The song is still waiting for you to finish it.";

    [Header("Stage References")]
    public Light stageSpotlight;
    public MonoBehaviour stageTrigger;

    private bool hasBeenPressed = false;
    private AudioSource[] audioSources = new AudioSource[3];

    void Start()
    {
        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
        {
            interactable.onPrimaryInteract.AddListener(OnButtonPressed);
            interactable.hoverText = "点击播放音乐";
        }

        for (int i = 0; i < 3; i++)
            audioSources[i] = gameObject.AddComponent<AudioSource>();
    }

    void OnButtonPressed()
    {
        if (hasBeenPressed) return;
        hasBeenPressed = true;

        var interactable = GetComponent<InteractableGeneral>();
        if (interactable != null)
            interactable.hoverText = "";

        audioSources[0].clip = Resources.Load<AudioClip>("AudioClips/Guitar");
        audioSources[1].clip = Resources.Load<AudioClip>("AudioClips/Piano");
        audioSources[2].clip = Resources.Load<AudioClip>("AudioClips/Vocal");

        foreach (var src in audioSources)
            src.Play();

        StartCoroutine(WaitForAudioFinish());
    }

    IEnumerator WaitForAudioFinish()
    {
        bool allDone = false;
        while (!allDone)
        {
            allDone = true;
            foreach (var src in audioSources)
            {
                if (src.isPlaying) { allDone = false; break; }
            }
            yield return null;
        }

        if (showInfo != null)
        {
            showInfo.SetActive(true);
            var text = showInfo.GetComponent<Text>();
            if (text == null)
                text = showInfo.GetComponentInChildren<Text>();
            if (text != null)
                text.text = message;
        }

        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(true);

        if (stageTrigger != null)
            stageTrigger.gameObject.SetActive(true);
    }
}

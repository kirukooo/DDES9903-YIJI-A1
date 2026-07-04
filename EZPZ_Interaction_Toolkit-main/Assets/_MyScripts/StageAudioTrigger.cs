using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageAudioTrigger : MonoBehaviour
{
    public GameObject showInfo;
    public string message = "You found your voice again. The song is finally complete. But your voice will keep echoing.";

    private bool played = false;

    private void OnTriggerEnter(Collider other)
    {
        if (played) return;
        if (!other.CompareTag("Player")) return;

        played = true;
        StartCoroutine(PlayFullAudio());
    }

    IEnumerator PlayFullAudio()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.clip = Resources.Load<AudioClip>("AudioClips/Full");
        src.Play();

        yield return new WaitWhile(() => src.isPlaying);

        if (showInfo != null)
        {
            showInfo.SetActive(true);
            var text = showInfo.GetComponent<Text>();
            if (text == null)
                text = showInfo.GetComponentInChildren<Text>();
            if (text != null)
                text.text = message;
        }

        DimAllLights();
    }

    void DimAllLights()
    {
        var lights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.gameObject == gameObject) continue;
            StartCoroutine(DimLight(l, 0.05f));
        }
    }

    IEnumerator DimLight(Light light, float target)
    {
        float start = light.intensity;
        float t = 0f;
        float duration = 2f;
        while (t < duration)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        light.intensity = target;
    }
}

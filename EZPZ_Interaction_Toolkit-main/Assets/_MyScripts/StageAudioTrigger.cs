using UnityEngine;

public class StageAudioTrigger : MonoBehaviour
{
    private bool played = false;

    private void OnTriggerEnter(Collider other)
    {
        if (played) return;
        if (!other.CompareTag("Player")) return;

        played = true;
        var src = gameObject.AddComponent<AudioSource>();
        src.clip = Resources.Load<AudioClip>("AudioClips/Full");
        src.Play();
    }
}

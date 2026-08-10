using UnityEngine;

public class PaperDropAudio : MonoBehaviour
{
    public void PlayPiano()
    {
        AudioManager.Instance.PlaySound("Piano");
    }
}
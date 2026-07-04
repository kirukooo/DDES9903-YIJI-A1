using UnityEngine;
using UnityEngine.UI;

public class WuTaiTrigger : MonoBehaviour
{
    public Light stageSpotlight;
    public GameObject stageLightCircle;
    public GameObject showInfo;
    public string message = "Now, please step onto the stage.\nThis song is still waiting for you to finish it.\nPlease walk into the spotlight.";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(true);

        if (stageLightCircle != null)
            stageLightCircle.SetActive(true);

        if (showInfo != null)
            showInfo.SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (stageSpotlight != null)
            stageSpotlight.gameObject.SetActive(false);

        if (stageLightCircle != null)
            stageLightCircle.SetActive(false);

    
    }
}

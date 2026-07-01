using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Ligthtwinkle : MonoBehaviour
{

    private Light pointLight;
    private float baseIntensity;
    public float flashIntensity = 8f;
    public float flashInterval = 0.15f;

    void Awake()
    {
        pointLight = GetComponent<Light>();
        baseIntensity = pointLight.intensity;
    }

    public void Twinkle()
    {
        StopAllCoroutines();
        StartCoroutine(FlashTwoTimes());
    }


    IEnumerator FlashTwoTimes()
    {
        pointLight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashInterval);
        pointLight.intensity = baseIntensity;
        yield return new WaitForSeconds(flashInterval);
        pointLight.intensity = flashIntensity;
        yield return new WaitForSeconds(flashInterval);
        pointLight.intensity = baseIntensity;
    }
}

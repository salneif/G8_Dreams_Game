using UnityEngine;

public class light_lamp : MonoBehaviour
{
    public Light lamp;

    public float minTime = 0.05f;
    public float maxTime = 0.3f;

    void Start()
    {
        StartCoroutine(Flicker());
    }

    System.Collections.IEnumerator Flicker()
    {
        while (true)
        {
            lamp.enabled = !lamp.enabled;

            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}


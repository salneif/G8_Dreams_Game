using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    public Material daySky;
    public Material nightSky;

    public Light sun;

    bool isNight = false;

    void Update()
    {
        // إذا صار ليل
        if (sun.transform.eulerAngles.x > 170 && !isNight)
        {
            RenderSettings.skybox = nightSky;
            DynamicGI.UpdateEnvironment();

            isNight = true;
        }

        // إذا صار نهار
        if (sun.transform.eulerAngles.x < 170 && isNight)
        {
            RenderSettings.skybox = daySky;
            DynamicGI.UpdateEnvironment();

            isNight = false;
        }
    }
}
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;

    public float speed = 10f;

    void Update()
    {
        sun.transform.Rotate(Vector3.right * speed * Time.deltaTime);
    }
}

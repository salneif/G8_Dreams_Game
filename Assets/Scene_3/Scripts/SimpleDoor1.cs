using System.Collections;
using UnityEngine;

public class SimpleDoor1 : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float swingSpeed = 2f;
    [SerializeField] private SceneFader sceneFader;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen && !isMoving)
        {
            StartCoroutine(swingDoor(openRotation));
            isOpen = true;
            sceneFader.startFadeAndLoad();
        } 
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isOpen && !isMoving)
        {
            StartCoroutine(swingDoor(closedRotation));
            isOpen = false;
        }
    }

    IEnumerator swingDoor(Quaternion target)
    {
        isMoving = true;

        while (Quaternion.Angle(transform.localRotation, target) > 0.1f)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                target,
                Time.deltaTime * swingSpeed
            );
            yield return null;
        }

        transform.localRotation = target;
        isMoving = false;
    }
}
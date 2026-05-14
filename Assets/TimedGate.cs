using System.Collections;
using UnityEngine;

public class TimedGate : MonoBehaviour
{
    [Header("Gate")]
    [Tooltip("The gate panel that rotates open")]
    [SerializeField] private Transform gateTransform;
    [Tooltip("How many degrees the gate swings open")]
    [SerializeField] private float openAngle = 90f;
    [Tooltip("How fast the gate swings")]
    [SerializeField] private float swingSpeed = 2f;

    [Header("Timing")]
    [Tooltip("Seconds after scene start before gate begins opening")]
    [SerializeField] private float delayBeforeOpen = 5f;

    [Header("References")]
    [SerializeField] private CharController2 playerController;

    private void Start()
    {
        StartCoroutine(OpenAfterDelay());
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeOpen);

        // Freeze player while gate swings
        if (playerController != null)
            playerController.SetFrozen(true);

        Quaternion closedRotation = gateTransform.localRotation;
        Quaternion openRotation   = closedRotation * Quaternion.Euler(0f, -openAngle, 0f);

        while (Quaternion.Angle(gateTransform.localRotation, openRotation) > 0.1f)
        {
            gateTransform.localRotation = Quaternion.Lerp(
                gateTransform.localRotation,
                openRotation,
                Time.deltaTime * swingSpeed
            );
            yield return null;
        }

        gateTransform.localRotation = openRotation;

        // Unfreeze player once gate is fully open
        if (playerController != null)
            playerController.SetFrozen(false);
    }
}
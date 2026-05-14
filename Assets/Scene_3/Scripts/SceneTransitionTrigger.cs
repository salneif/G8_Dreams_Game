using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private SceneFader sceneFader;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (sceneFader != null)
            sceneFader.startFadeAndLoad();
    }
}
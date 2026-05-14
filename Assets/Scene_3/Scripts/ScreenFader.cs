using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private string targetSceneName;
    [SerializeField] private float fadeDuration = 1f;

    public void startFadeAndLoad()
    {
        StartCoroutine(fadeAndLoad());
    }

    IEnumerator fadeAndLoad()
    {
        yield return new WaitForSeconds(1f);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        SceneManager.LoadScene(targetSceneName);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup startPanelGroup;
    [SerializeField][TextArea(1, 2)] private string titleString = "أم القبيس";
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField][TextArea(1, 3)] private string subtitleString = "Your subtitle here...";
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private GameObject parchmentObject;

    private void Start()
    {
        if (titleText != null)
            titleText.text = titleString;

        if (subtitleText != null)
            subtitleText.text = subtitleString;

        if (startPanelGroup != null)
        {
            startPanelGroup.alpha          = 1f;
            startPanelGroup.interactable   = true;
            startPanelGroup.blocksRaycasts = true;
        }

        if (parchmentObject != null)
            parchmentObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void OnStartButtonClicked()
    {
        StartCoroutine(FadeOutAndHide());
    }

    private IEnumerator FadeOutAndHide()
    {
        if (startPanelGroup != null)
        {
            startPanelGroup.interactable   = false;
            startPanelGroup.blocksRaycasts = false;
        }

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (startPanelGroup != null)
                startPanelGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
            yield return null;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        if (parchmentObject != null)
            parchmentObject.SetActive(true);

        gameObject.SetActive(false);
    }
}
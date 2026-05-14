using UnityEngine;
using TMPro;

public class LetterInteractable : MonoBehaviour
{
    [TextArea(5, 15)]
    [SerializeField] private string letterText = "Letter text...";
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private TextMeshProUGUI letterBodyText;
    [SerializeField] private GameObject interactPrompt;
    [SerializeField] private GameObject parchmentMesh;
    [SerializeField] private CharController2 playerController;

    private bool _playerInRange  = false;
    private bool _isOpen         = false;
    private bool _hasBeenRead    = false;

    private void Update()
    {
        if (_isOpen && Input.GetKeyDown(KeyCode.E))
        {
            CloseLetter();
            return;
        }

        if (_playerInRange && !_hasBeenRead && !_isOpen && Input.GetKeyDown(KeyCode.E))
        {
            OpenLetter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _hasBeenRead)
            return;

        _playerInRange = true;
        ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInRange = false;
        ShowPrompt(false);
    }

    private void OpenLetter()
    {
        _isOpen = true;

        ShowPrompt(false);

        if (letterBodyText != null)
            letterBodyText.text = letterText;

        if (letterPanel != null)
            letterPanel.SetActive(true);

        MeshRenderer[] renderers = parchmentMesh != null
            ? parchmentMesh.GetComponentsInChildren<MeshRenderer>()
            : GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in renderers)
            mr.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void CloseLetter()
    {
        _isOpen      = false;
        _hasBeenRead = true;

        if (letterPanel != null)
            letterPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

            if (playerController != null)
                playerController.SetFrozen(false);
    }

    private void ShowPrompt(bool show)
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(show);
    }
}
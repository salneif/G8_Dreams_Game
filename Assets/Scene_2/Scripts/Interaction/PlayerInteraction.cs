using UnityEngine;

namespace OmmAlQubays
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Camera interactionCamera;

        private PlayerStats _playerStats;
        private Collectible _currentTarget;
        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            if (interactionCamera == null)
                interactionCamera = Camera.main;
        }

        private void Update()
        {
            ScanForInteractable();
            HandleInteractInput();
        }

        private void ScanForInteractable()
        {
            if (interactionCamera == null) return;

            Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
            {
                if (hit.collider.TryGetComponent(out Collectible collectible))
                {
                    if (_currentTarget != collectible)
                    {
                        _currentTarget = collectible;
                        uiManager?.ShowInteractPrompt(collectible.GetPromptText());
                    }
                }
                else
                {
                    ClearTarget();
                }
            }
            else
            {
                ClearTarget();
            }
        }

        private void HandleInteractInput()
        {
            if (_currentTarget == null) return;
            if (!Input.GetKeyDown(KeyCode.E)) return;

            _currentTarget.Collect(_playerStats);
            ClearTarget();
        }

        private void ClearTarget()
        {
            if (_currentTarget == null)
                return;

            _currentTarget = null;
            uiManager?.HideInteractPrompt();
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OmmAlQubays
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Image thirstFill;
        [SerializeField] private Color thirstColorNormal = Color.white;
        [SerializeField] private Color thirstColorLow = new Color(1f, 0.55f, 0.1f, 0.9f);
        [SerializeField] private Color thirstColorCritical = new Color(0.9f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Image oilFill;
        [SerializeField] private Color oilColorNormal = new Color(1f, 0.74f, 0.2f, 0.86f);
        [SerializeField] private Color oilColorLow = new Color(1f, 0.3f, 0.1f, 0.86f);
        [SerializeField] private TextMeshProUGUI interactPromptText;
        [SerializeField] private float promptFadeDuration = 0.25f;

        private bool _thirstIsLow;
        private bool _thirstIsCritical;
        private bool _oilIsLow;
        private float _promptTargetAlpha = 0f;

        private void Start()
        {
            SubscribeToPlayerStats();
            InitialisePrompt();
        }

        private void OnDestroy()
        {
            UnsubscribeFromPlayerStats();
        }

        private void Update()
        {
            if (playerStats == null)
                return;

            UpdateThirstMeter();
            UpdateOilMeter();
            UpdatePromptFade();
        }

        private void SubscribeToPlayerStats()
        {
            if (playerStats == null)
            {
                return;
            }

            playerStats.OnThirstLow      += HandleThirstLow;
            playerStats.OnThirstCritical += HandleThirstCritical;
            playerStats.OnOilLow         += HandleOilLow;
        }

        private void UnsubscribeFromPlayerStats()
        {
            if (playerStats == null)
                return;

            playerStats.OnThirstLow      -= HandleThirstLow;
            playerStats.OnThirstCritical -= HandleThirstCritical;
            playerStats.OnOilLow         -= HandleOilLow;
        }

        private void InitialisePrompt()
        {
            if (interactPromptText != null)
            {
                interactPromptText.alpha = 0f;
            }
        }


        private void UpdateThirstMeter()
        {
            if (thirstFill == null)
                return;
            float normalized = playerStats.ThirstNormalized();
            thirstFill.fillAmount = normalized;

            if (_thirstIsCritical)
            {
                thirstFill.color = thirstColorCritical;
            }
            else if (_thirstIsLow)
            {
                float t = Mathf.InverseLerp(0.30f, 0.10f, normalized);
                thirstFill.color = Color.Lerp(thirstColorLow, thirstColorCritical, t);
            }
            else
            {
                thirstFill.color = thirstColorNormal;
            }
        }

        private void UpdateOilMeter()
        {
            if (oilFill == null)
                return;

            oilFill.fillAmount = playerStats.OilNormalized();
            oilFill.color = _oilIsLow ? oilColorLow : oilColorNormal;
        }

        private void HandleThirstLow()
        {
            _thirstIsLow = true;
        }

        private void HandleThirstCritical()
        {
            _thirstIsCritical = true;
        }

        private void HandleOilLow()
        {
            _oilIsLow = true;
        }

        private void UpdatePromptFade()
        {
            if (interactPromptText == null)
                return;
            interactPromptText.alpha = Mathf.MoveTowards(interactPromptText.alpha, _promptTargetAlpha, Time.deltaTime / promptFadeDuration);
        }

        public void ShowInteractPrompt(string text)
        {
            if (interactPromptText == null)
                return;
            interactPromptText.text = text;
            _promptTargetAlpha = 1f;
        }

        public void HideInteractPrompt()
        {
            if (interactPromptText == null)
                return;
            _promptTargetAlpha = 0f;
        }
    }
}
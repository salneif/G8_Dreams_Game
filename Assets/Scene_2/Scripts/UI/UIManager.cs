using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OmmAlQubays
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Image thirstFill;
        [SerializeField] private Color thirstColorNormal   = Color.white;
        [SerializeField] private Color thirstColorLow      = new Color(1f, 0.55f, 0.1f, 0.9f);
        [SerializeField] private Color thirstColorCritical = new Color(0.9f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Image oilFill;
        [SerializeField] private Color oilColorNormal = new Color(1f, 0.74f, 0.2f, 0.86f);
        [SerializeField] private Color oilColorLow    = new Color(1f, 0.3f, 0.1f, 0.86f);
        [SerializeField] private Image healthFill;
        [SerializeField] private Color healthColorNormal = new Color(0.78f, 0.16f, 0.16f, 0.86f);
        [SerializeField] private Color healthColorLow    = new Color(1f, 0f, 0f, 1f);
        [SerializeField] private TextMeshProUGUI interactPromptText;
        [SerializeField] private float promptFadeDuration = 0.25f;
        [SerializeField] private TextMeshProUGUI jinnWarningText;
        [SerializeField] private CanvasGroup jinnVignetteGroup;
        [SerializeField] private string warningMessage = "Warning: Leave Jinn Area!\n\nBarrage of Rocks Incoming";
        [SerializeField] private float pulseSpeed    = 1.2f;
        [SerializeField] private float pulseAlphaMin = 0.2f;
        [SerializeField] private float pulseAlphaMax = 1f;
        [SerializeField] private float fadeOutSpeed  = 3f;
        [SerializeField] private float meterFlickerThreshold = 0.25f;
        [SerializeField] private float meterFlickerSpeed    = 3f;
        [SerializeField] private float meterFlickerAlphaMin = 0.15f;
        [SerializeField] private float meterFlickerAlphaMax = 1f;
        [SerializeField] private AudioSource pantingAudioSource;
        [SerializeField] private float pantingThreshold = 0.25f;

        private bool  _thirstIsLow;
        private bool  _thirstIsCritical;
        private bool  _oilIsLow;
        private bool  _healthIsLow;
        private float _promptTargetAlpha;
        private bool  _jinnAttackActive;
        private float _pulseTime;
        private float _meterFlickerTime;
        private bool  _isPanting;

        private void Start()
        {
            SubscribeToPlayerStats();

            if (interactPromptText != null)
                interactPromptText.alpha = 0f;

            if (jinnWarningText != null)
                jinnWarningText.gameObject.SetActive(false);

            if (jinnVignetteGroup != null)
            {
                jinnVignetteGroup.alpha          = 0f;
                jinnVignetteGroup.interactable   = false;
                jinnVignetteGroup.blocksRaycasts = false;
            }

            if (pantingAudioSource != null)
                pantingAudioSource.Stop();
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
            UpdateHealthBar();
            UpdatePromptFade();
            UpdateJinnEffects();
            UpdatePanting();
        }

        private void SubscribeToPlayerStats()
        {
            if (playerStats == null)
                return;

            playerStats.OnThirstLow      += HandleThirstLow;
            playerStats.OnThirstCritical += HandleThirstCritical;
            playerStats.OnOilLow         += HandleOilLow;
            playerStats.OnHealthLow      += HandleHealthLow;
        }

        private void UnsubscribeFromPlayerStats()
        {
            if (playerStats == null)
                return;

            playerStats.OnThirstLow      -= HandleThirstLow;
            playerStats.OnThirstCritical -= HandleThirstCritical;
            playerStats.OnOilLow         -= HandleOilLow;
            playerStats.OnHealthLow      -= HandleHealthLow;
        }

        private void UpdateThirstMeter()
        {
            if (thirstFill == null)
                return;

            float normalized = playerStats.ThirstNormalized();
            thirstFill.fillAmount = normalized;

            Color baseColor;
            if (normalized <= 0.10f)
                baseColor = thirstColorCritical;
            else if (normalized <= 0.30f)
                baseColor = Color.Lerp(thirstColorLow, thirstColorCritical,
                    Mathf.InverseLerp(0.30f, 0.10f, normalized));
            else
                baseColor = thirstColorNormal;

            baseColor.a = normalized <= meterFlickerThreshold ? FlickerAlpha() : 1f;
            thirstFill.color = baseColor;
        }

        private void UpdateOilMeter()
        {
            if (oilFill == null)
                return;

            float normalized = playerStats.OilNormalized();
            oilFill.fillAmount = normalized;

            Color baseColor = normalized <= 0.20f ? oilColorLow : oilColorNormal;

            baseColor.a = normalized <= meterFlickerThreshold ? FlickerAlpha() : 1f;
            oilFill.color = baseColor;
        }
        private float FlickerAlpha()
        {
            _meterFlickerTime += Time.deltaTime * meterFlickerSpeed;
            float sine = (Mathf.Sin(_meterFlickerTime * Mathf.PI * 2f) + 1f) * 0.5f;
            return Mathf.Lerp(meterFlickerAlphaMin, meterFlickerAlphaMax, sine);
        }

        private void UpdateHealthBar()
        {
            if (healthFill == null)
                return;

            healthFill.fillAmount = playerStats.HealthNormalized();
            healthFill.color      = _healthIsLow ? healthColorLow : healthColorNormal;
        }

        private void UpdatePromptFade()
        {
            if (interactPromptText == null)
                return;

            interactPromptText.alpha = Mathf.MoveTowards(
                interactPromptText.alpha, _promptTargetAlpha, Time.deltaTime / promptFadeDuration);
        }

        private void UpdateJinnEffects()
        {
            if (_jinnAttackActive)
            {
                _pulseTime += Time.deltaTime * pulseSpeed;
                float sine  = (Mathf.Sin(_pulseTime * Mathf.PI * 2f) + 1f) * 0.5f;
                float alpha = Mathf.Lerp(pulseAlphaMin, pulseAlphaMax, sine);

                if (jinnVignetteGroup != null)
                    jinnVignetteGroup.alpha = alpha;
                if (jinnWarningText != null)
                    jinnWarningText.alpha = alpha;
            }
            else
            {
                if (jinnVignetteGroup != null)
                    jinnVignetteGroup.alpha = Mathf.MoveTowards(
                        jinnVignetteGroup.alpha, 0f, Time.deltaTime * fadeOutSpeed);

                if (jinnWarningText != null)
                {
                    jinnWarningText.alpha = Mathf.MoveTowards(
                        jinnWarningText.alpha, 0f, Time.deltaTime * fadeOutSpeed);

                    if (jinnWarningText.alpha <= 0f)
                        jinnWarningText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdatePanting()
        {
            if (pantingAudioSource == null)
                return;

            float thirstNormalized = playerStats.ThirstNormalized();
            bool shouldPant = thirstNormalized <= pantingThreshold;

            if (shouldPant && !_isPanting)
            {
                _isPanting = true;
                pantingAudioSource.Play();
            }

            else if (!shouldPant && _isPanting)
            {
                _isPanting = false;
                pantingAudioSource.Stop();
            }
        }

        private void HandleThirstLow()      => _thirstIsLow = true;
        private void HandleThirstCritical() => _thirstIsCritical = true;
        private void HandleOilLow()         => _oilIsLow = true;
        private void HandleHealthLow()      => _healthIsLow = true;

        public void ShowJinnAttackWarning()
        {
            _jinnAttackActive = true;
            _pulseTime        = 0f;

            if (jinnWarningText != null)
            {
                jinnWarningText.text = warningMessage;
                jinnWarningText.gameObject.SetActive(true);
            }
        }

        public void HideJinnAttackWarning()
        {
            _jinnAttackActive = false;
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
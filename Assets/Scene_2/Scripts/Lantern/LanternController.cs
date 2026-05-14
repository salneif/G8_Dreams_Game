using UnityEngine;

namespace OmmAlQubays
{
    public class LanternController : MonoBehaviour
    {
        [SerializeField] private Light lanternLight;
        [SerializeField] private ParticleSystem flameParticles;
        [SerializeField] private AudioSource toggleAudioSource;
        [SerializeField] private AudioSource flameAudioSource;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private float normalIntensity = 2.5f;
        [SerializeField] private float lowOilIntensity = 1.2f;
        [SerializeField] private float lowOilThreshold = 0.2f;
        [SerializeField] private float swayAmount = 0.02f;
        [SerializeField] private float swaySmoothing = 6f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F;
        [SerializeField] private float flickerDuration = 2.0f;
        [SerializeField] private float flickerSpeed = 25f;
        [SerializeField] private float flickerMinIntensity = 0.2f;
        [SerializeField] private float flickerMaxIntensity = 3.5f;
        [SerializeField] private float flickerPitchRange = 0.3f;


        private bool _isOn = false;
        private bool _isFlickering = false;
        private float _flickerElapsed = 0f;
        private float _flickerDuration = 0f;
        private float _flickerIntensity = 0f;
        private bool _jinnFlickerActive = false;
        private float _jinnFlickerIntensity = 0f;

        private Vector3 _restPosition;
        private Vector3 _swayCurrentPosition;


        private void Awake()
        {
            _restPosition = transform.localPosition;
            _swayCurrentPosition = _restPosition;
            SetLanternState(false, playSound: false);
        }

        private void Start()
        {
            if (playerStats != null)
                playerStats.OnOilDepleted += HandleOilDepleted;
        }

        private void OnDestroy()
        {
            if (playerStats != null)
                playerStats.OnOilDepleted -= HandleOilDepleted;
        }

        private void Update()
        {
            HandleToggleInput();
            ApplySway();

            if (_isOn)
                UpdateLightIntensityForOil();

            UpdateFlicker();
        }

        private void HandleToggleInput()
        {
            if (!Input.GetKeyDown(toggleKey))
                return;

            if (!_isOn && playerStats != null && playerStats.OilNormalized() <= 0f)
            {
                PlayToggleSound();
                return;
            }

            SetLanternState(!_isOn, playSound: true);
        }

        private void HandleOilDepleted()
        {
            SetLanternState(false, playSound: false);
        }

        private void SetLanternState(bool on, bool playSound)
        {
            _isOn = on;

            if (lanternLight != null)
            {
                lanternLight.enabled = on;
                lanternLight.intensity = normalIntensity;
            }

            if (flameParticles != null)
            {
                flameParticles.gameObject.SetActive(on);
                if (on)
                    flameParticles.Play();
                else
                    flameParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (flameAudioSource != null)
            {
                if (on)
                    flameAudioSource.Play();
                else
                    flameAudioSource.Stop();
            }

            if (playSound)
                PlayToggleSound();

            if (!on && _isFlickering)
                StopOneShotFlicker();
        }

        private void ApplySway()
        {
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            Vector3 targetSway = _restPosition + new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0f);
            _swayCurrentPosition = Vector3.Lerp(_swayCurrentPosition, targetSway, Time.deltaTime * swaySmoothing);
            transform.localPosition = _swayCurrentPosition;
        }

        private void UpdateLightIntensityForOil()
        {
            if (lanternLight == null || _isFlickering || _jinnFlickerActive || playerStats == null)
                return;

            float oil = playerStats.OilNormalized();

            if (oil <= lowOilThreshold)
            {
                float t = oil / lowOilThreshold;
                lanternLight.intensity = Mathf.Lerp(flickerMinIntensity, lowOilIntensity, t);
            }
            else
            {
                lanternLight.intensity = normalIntensity;
            }
        }

        private void UpdateFlicker()
        {
            bool oneShotRunning = _isFlickering;
            bool jinnRunning    = _jinnFlickerActive && _isOn;

            if (!oneShotRunning && !jinnRunning)
                return;

            float effectiveIntensity = oneShotRunning ? _flickerIntensity : _jinnFlickerIntensity;
            float timeInput = oneShotRunning ? _flickerElapsed : Time.time;

            if (oneShotRunning)
            {
                _flickerElapsed += Time.deltaTime;
                if (_flickerElapsed >= _flickerDuration)
                {
                    StopOneShotFlicker();
                    return;
                }
            }

            float noiseValue = Mathf.PerlinNoise(timeInput * flickerSpeed, 0f);

            float minI = Mathf.Lerp(normalIntensity * 0.8f, flickerMinIntensity, effectiveIntensity);
            float maxI = Mathf.Lerp(normalIntensity * 1.1f, flickerMaxIntensity, effectiveIntensity);

            if (lanternLight != null)
                lanternLight.intensity = Mathf.Lerp(minI, maxI, noiseValue);

            if (flameAudioSource != null && flameAudioSource.isPlaying)
            {
                float pitchNoise = Mathf.PerlinNoise(timeInput * flickerSpeed * 0.5f, 100f);
                flameAudioSource.pitch = 1f +
                    Mathf.Lerp(-flickerPitchRange, flickerPitchRange, pitchNoise) * effectiveIntensity;
            }
        }

        public void TriggerFlicker(float intensity = 0.5f)
        {
            if (!_isOn || _isFlickering)
                return;

            _isFlickering    = true;
            _flickerElapsed  = 0f;
            _flickerIntensity = intensity;
            _flickerDuration = flickerDuration * (0.5f + intensity * 0.5f);
        }

        public void SetJinnFlicker(bool active, float intensity = 0.5f)
        {
            _jinnFlickerActive   = active;
            _jinnFlickerIntensity = intensity;

            if (!active && !_isFlickering)
            {
                if (lanternLight != null && _isOn)
                    lanternLight.intensity = normalIntensity;
                if (flameAudioSource != null)
                    flameAudioSource.pitch = 1f;
            }
        }

        private void StopOneShotFlicker()
        {
            _isFlickering = false;
            _flickerElapsed = 0f;

            if (!_jinnFlickerActive)
            {
                if (lanternLight != null && _isOn)
                    lanternLight.intensity = normalIntensity;
                if (flameAudioSource != null)
                    flameAudioSource.pitch = 1f;
            }
        }

        public void StopFlicker()
        {
            StopOneShotFlicker();
        }

        private void PlayToggleSound()
        {
            if (toggleAudioSource != null && toggleAudioSource.clip != null)
                toggleAudioSource.PlayOneShot(toggleAudioSource.clip);
        }

        public void RestoreOil(float amount)
        {
            if (playerStats != null)
                playerStats.RestoreOil(amount);
        }

        public float OilNormalized()
        {
            return playerStats != null ? playerStats.OilNormalized() : 1f;
        }

        public bool IsOn()
        {
            return _isOn;
        }

        public bool IsFlickering()
        {
            return _isFlickering || _jinnFlickerActive;
        }

        public void TurnOn()
        {
            SetLanternState(true, playSound: false);
        }
    }
}
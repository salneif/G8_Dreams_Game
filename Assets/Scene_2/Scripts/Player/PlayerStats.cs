using System;
using UnityEngine;

namespace OmmAlQubays
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float baseDrainRate = 1f;
        [SerializeField] private float startingThirst = 50f;
        [SerializeField] private float thirstLowThreshold = 0.3f;
        [SerializeField] private float thirstCriticalThreshold = 0.1f;
        [SerializeField] private float speedModifierLow = 0.85f;
        [SerializeField] private float speedModifierCritical = 0.70f;
        [SerializeField] private float jinnProximityDrainBonus = 0.5f;

        [SerializeField] private float maxOil = 100f;
        [SerializeField] private float oilDrainRate = 0.5f;
        [SerializeField] private float startingOil = 100f;

        [SerializeField] private float oilLowThreshold = 0.2f;
        [SerializeField] private CharController playerMovement;
        [SerializeField] private LanternController lanternController;

        public event Action OnThirstLow;
        public event Action OnThirstCritical;
        public event Action OnThirstDepleted;
        public event Action OnOilLow;
        public event Action OnOilDepleted;

        [SerializeField] private float _currentThirst;
        [SerializeField] private float _currentOil;
        private bool _thirstLowFired;
        private bool _thirstCriticalFired;
        private bool _thirstDepletedFired;
        private bool _oilLowFired;
        private bool _oilDepletedFired;

        private bool _hasCollapsed;
        private bool _jinnProximityActive;
        private float _drainRateMultiplier = 1f;

        private void Awake()
        {
            _currentThirst = startingThirst;
            _currentOil = startingOil;
        }

        private void Update()
        {
            if (_hasCollapsed)
                return;

            DrainThirst();
            DrainOil();
        }

        private void DrainThirst()
        {
            if (_currentThirst <= 0f)
                return;

            float drainThisFrame = baseDrainRate * _drainRateMultiplier * Time.deltaTime;

            if (_jinnProximityActive)
                drainThisFrame += jinnProximityDrainBonus * Time.deltaTime;

            _currentThirst = Mathf.Max(0f, _currentThirst - drainThisFrame);

            EvaluateThirstThresholds();
        }

        private void EvaluateThirstThresholds()
        {
            float normalized = ThirstNormalized();

            if (!_thirstCriticalFired && normalized <= thirstCriticalThreshold)
            {
                _thirstCriticalFired = true;
                OnThirstCritical?.Invoke();
            }
            else if (!_thirstLowFired && normalized <= thirstLowThreshold)
            {
                _thirstLowFired = true;
                OnThirstLow?.Invoke();
            }

            UpdateSpeedModifier(normalized);

            if (!_thirstDepletedFired && _currentThirst <= 0f)
            {
                _thirstDepletedFired = true;
                _hasCollapsed = true;
                OnThirstDepleted?.Invoke();
                TriggerCollapse();
            }
        }

        private void UpdateSpeedModifier(float normalized)
        {
            if (playerMovement == null)
                return;

            if (normalized <= thirstCriticalThreshold)
                playerMovement.SetThirstSpeedModifier(speedModifierCritical);
            else if (normalized <= thirstLowThreshold)
                playerMovement.SetThirstSpeedModifier(speedModifierLow);
            else
                playerMovement.SetThirstSpeedModifier(1f);
        }

        private void DrainOil()
        {
            if (lanternController == null || !lanternController.IsOn())
                return;
            if (_currentOil <= 0f)
                return;

            _currentOil = Mathf.Max(0f, _currentOil - oilDrainRate * Time.deltaTime);

            EvaluateOilThresholds();
        }

        private void EvaluateOilThresholds()
        {
            float normalized = OilNormalized();

            if (!_oilLowFired && normalized <= oilLowThreshold)
            {
                _oilLowFired = true;
                OnOilLow?.Invoke();
            }

            if (!_oilDepletedFired && _currentOil <= 0f)
            {
                _oilDepletedFired = true;
                OnOilDepleted?.Invoke();
            }
        }

        private void TriggerCollapse()
        {
                if (playerMovement != null)
                playerMovement.SetThirstSpeedModifier(0f);
        }

        public void RestoreThirst(float amount)
        {
            _currentThirst = Mathf.Min(maxThirst, _currentThirst + amount);
            float normalized = ThirstNormalized();
            if (normalized > thirstCriticalThreshold)
                _thirstCriticalFired = false;
            if (normalized > thirstLowThreshold)
                _thirstLowFired = false;
        }

        public void RestoreOil(float amount)
        {
            _currentOil = Mathf.Min(maxOil, _currentOil + amount);

            if (OilNormalized() > oilLowThreshold)
                _oilLowFired = false;
        }

        public void SetJinnProximityPenalty(bool active)
        {
            _jinnProximityActive = active;
        }

        public void SetDrainRateMultiplier(float multiplier)
        {
            _drainRateMultiplier = Mathf.Max(0f, multiplier);
        }

        public void IncreasePermanentDrainRate(float amount)
        {
            baseDrainRate += amount;
        }

        public float ThirstNormalized()
        {
            return _currentThirst / maxThirst;
        }

        public float OilNormalized()
        {
            return _currentOil / maxOil;
        }

        public float CurrentThirst()
        {
            return _currentThirst;
        }

        public float currentOil()
        {
            return _currentOil;
        }

        public bool HasCollapsed()
        {
            return _hasCollapsed;
        }
    }
}
using System;
using System.Collections;
using UnityEngine;

namespace OmmAlQubays
{
    public enum JinnState
    {
        Idle,
        Luring,
        Retreating
    }

    public class JinnLightController : MonoBehaviour
    {
        [SerializeField] private Light jinnPointLight;
        [SerializeField] private ParticleSystem fireParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioReverbFilter reverbFilter;
        [SerializeField] private LanternController lanternController;
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float luringDistance = 30f;
        [SerializeField] private float closeDistance = 15f;
        [SerializeField] private float retreatTriggerDistance = 10f;
        [SerializeField] private float baseIntensity = 3f;
        [SerializeField] private float pulseSpeed = 1.8f;
        [SerializeField] private float pulseAmount = 0.35f;
        [SerializeField] private bool doesNotRetreat = false;
        [SerializeField] private float retreatMoveDistance = 40f;
        [SerializeField] private float retreatDimDuration = 0.6f;
        [SerializeField] private float retreatReappearDuration = 1.2f;
        [SerializeField] private float audioNormalPitch = 1f;
        [SerializeField] private float audioDistortedPitch = 0.88f;
        [SerializeField] private float reverbRoomFar = -10000f;
        [SerializeField] private float reverbRoomClose = -500f;
        [SerializeField] private float flickerIntensity = 0.75f;
        [SerializeField] private bool startDisabled = false;
        public event Action OnPlayerEnterCloseRange;
        public event Action OnPlayerExitCloseRange;
        private static int s_proximityCount = 0;

        private JinnState _currentState = JinnState.Idle;
        private bool _isRetreating = false;
        private bool _isInCloseRange = false;
        private float _pulseTime = 0f;
        private float _activeFlickerIntensity;
        private bool _followPlayer = false;
        private Vector3 _homePosition;

        private void Awake()
        {
            _homePosition = transform.position;
            _activeFlickerIntensity = flickerIntensity;

            if (startDisabled)
                gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _currentState = JinnState.Idle;
            _isRetreating = false;
            _isInCloseRange = false;
            SetAudioDistortion(0f);

            if (jinnPointLight != null)
                jinnPointLight.intensity = baseIntensity;
        }

        private void Update()
        {
            if (playerTransform == null || _isRetreating)
                return;

            if (_followPlayer)
                PursuePlayerAtSafeDistance();

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            EvaluateState(dist);
            UpdateLightPulse();
        }

        private void OnDisable()
        {
            if (_currentState == JinnState.Luring || _currentState == JinnState.Retreating)
                DecrementProximity();

            lanternController?.SetJinnFlicker(false);
            _currentState = JinnState.Idle;
            _isRetreating = false;
        }

        private void EvaluateState(float distance)
        {
            JinnState desired = DesiredState(distance);

            if (desired == _currentState)
            {
                ApplyContinuousEffects(distance);
                return;
            }

            JinnState previous = _currentState;
            HandleStateExit(previous, desired);
            _currentState = desired;
            HandleStateEnter(desired, previous, distance);
        }

        private JinnState DesiredState(float distance)
        {
            if (distance <= retreatTriggerDistance && !doesNotRetreat)
                return JinnState.Retreating;

            if (distance <= luringDistance)
                return JinnState.Luring;

            return JinnState.Idle;
        }

        private void HandleStateEnter(JinnState entering, JinnState from, float distance)
        {
            switch (entering)
            {
                case JinnState.Idle:
                    break;

                case JinnState.Luring:
                    IncrementProximity();
                    lanternController?.SetJinnFlicker(true, _activeFlickerIntensity);
                    break;

                case JinnState.Retreating:
                    if (from == JinnState.Idle)
                    {
                        IncrementProximity();
                        lanternController?.SetJinnFlicker(true, _activeFlickerIntensity);
                    }
                    StartCoroutine(RetreatCoroutine());
                    break;
            }
        }

        private void HandleStateExit(JinnState leaving, JinnState goingTo)
        {
            switch (leaving)
            {
                case JinnState.Idle:
                    break;

                case JinnState.Luring:
                    if (goingTo == JinnState.Idle)
                    {
                        lanternController?.SetJinnFlicker(false);
                        DecrementProximity();
                    }

                    if (_isInCloseRange)
                    {
                        _isInCloseRange = false;
                        OnPlayerExitCloseRange?.Invoke();
                    }
                    break;

                case JinnState.Retreating:
                    break;
            }
        }

        private void ApplyContinuousEffects(float distance)
        {
            switch (_currentState)
            {
                case JinnState.Idle:
                    SetAudioDistortion(0f);
                    break;

                case JinnState.Luring:
                    float t = 1f - Mathf.Clamp01(
                        (distance - retreatTriggerDistance) /
                        (luringDistance - retreatTriggerDistance));
                    SetAudioDistortion(t);
                    UpdateCloseRangeEvents(distance);
                    break;
            }
        }

        private void UpdateCloseRangeEvents(float distance)
        {
            bool nowClose = distance <= closeDistance;

            if (nowClose && !_isInCloseRange)
            {
                _isInCloseRange = true;
                OnPlayerEnterCloseRange?.Invoke();
            }
            else if (!nowClose && _isInCloseRange)
            {
                _isInCloseRange = false;
                OnPlayerExitCloseRange?.Invoke();
            }
        }

        private void UpdateLightPulse()
        {
            if (jinnPointLight == null)
                return;

            _pulseTime += Time.deltaTime;
            float pulse = Mathf.Sin(_pulseTime * pulseSpeed * Mathf.PI * 2f);
            jinnPointLight.intensity = baseIntensity + pulse * pulseAmount;
        }

        private IEnumerator RetreatCoroutine()
        {
            _isRetreating = true;
            float startIntensity = jinnPointLight != null ? jinnPointLight.intensity : baseIntensity;
            yield return StartCoroutine(LerpLightIntensity(startIntensity, 0f, retreatDimDuration));

            if (fireParticles != null)
                fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (audioSource != null)
                audioSource.Stop();

            Vector3 awayDir = (transform.position - playerTransform.position).normalized;
            awayDir.y = 0f;
            float randomAngle = UnityEngine.Random.Range(-40f, 40f);
            awayDir = Quaternion.Euler(0f, randomAngle, 0f) * awayDir;

            transform.position += awayDir * retreatMoveDistance;
            _homePosition = transform.position;

            yield return new WaitForSeconds(0.25f);

            if (fireParticles != null)
                fireParticles.Play();

            if (audioSource != null)
            {
                audioSource.Play();
                SetAudioDistortion(0f);
            }

            yield return StartCoroutine(LerpLightIntensity(0f, baseIntensity, retreatReappearDuration));

            lanternController?.SetJinnFlicker(false);
            DecrementProximity();

            _currentState = JinnState.Idle;
            _isRetreating = false;
        }

        private IEnumerator LerpLightIntensity(float from, float to, float duration)
        {
            if (jinnPointLight == null)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                jinnPointLight.intensity = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            jinnPointLight.intensity = to;
        }

        private void SetAudioDistortion(float t)
        {
            if (audioSource != null)
                audioSource.pitch = Mathf.Lerp(audioNormalPitch, audioDistortedPitch, t);

            if (reverbFilter != null)
                reverbFilter.room = Mathf.Lerp(reverbRoomFar, reverbRoomClose, t);
        }

        private void PursuePlayerAtSafeDistance()
        {
            const float trailDistance = 35f;
            const float approachSpeed = 2.5f;

            float currentDist = Vector3.Distance(transform.position, playerTransform.position);

            if (currentDist > trailDistance + 8f)
            {
                Vector3 targetPos = playerTransform.position +
                    (transform.position - playerTransform.position).normalized * trailDistance;

                transform.position = Vector3.MoveTowards(
                    transform.position, targetPos, approachSpeed * Time.deltaTime);
            }
        }

        private void IncrementProximity()
        {
            s_proximityCount++;
            playerStats?.SetJinnProximityPenalty(true);
        }

        private void DecrementProximity()
        {
            s_proximityCount = Mathf.Max(0, s_proximityCount - 1);
            if (s_proximityCount <= 0)
                playerStats?.SetJinnProximityPenalty(false);
        }

        public void SetFlickerIntensity(float intensity)
        {
            _activeFlickerIntensity = Mathf.Clamp01(intensity);

            if (_currentState == JinnState.Luring)
                lanternController?.SetJinnFlicker(true, _activeFlickerIntensity);
        }

        public void SetFollowPlayer(bool follow)
        {
            _followPlayer = follow;
        }

        public JinnState CurrentState()
        {
            return _currentState;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = new Color(1f, 0.6f, 0f, 0.12f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, luringDistance);

            UnityEditor.Handles.color = new Color(1f, 0.3f, 0f, 0.22f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, closeDistance);

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.35f);
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, retreatTriggerDistance);
        }
#endif
    }
}
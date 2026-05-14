using System;
using UnityEngine;

namespace OmmAlQubays
{
    public class RealCampController : MonoBehaviour
    {
        [SerializeField] private Light campLight;
        [SerializeField] private float baseIntensity = 3f;
        [SerializeField] private float flickerAmount = 0.4f;
        [SerializeField] private float flickerLerpSpeed = 12f;
        [SerializeField] private AudioSource fireAudioSource;
        [SerializeField] private AudioSource camelAudioSource;

        public static event Action OnPlayerWin;

        private float _flickerTimer;
        private float _targetIntensity;
        private bool _winTriggered;

        private void Start()
        {
            _targetIntensity = baseIntensity;
        }

        private void Update()
        {
            FlickerLight();
        }

        private void FlickerLight()
        {
            if (campLight == null)
                return;

            _flickerTimer -= Time.deltaTime;

            if (_flickerTimer <= 0f)
            {
                _targetIntensity = baseIntensity + UnityEngine.Random.Range(-flickerAmount, flickerAmount);
                _flickerTimer = UnityEngine.Random.Range(0.06f, 0.25f);
            }

            campLight.intensity = Mathf.Lerp(campLight.intensity, _targetIntensity, Time.deltaTime * flickerLerpSpeed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_winTriggered)
                return;

            if (!other.CompareTag("Player"))
                return;

            _winTriggered = true;
            OnPlayerWin?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null)
                Gizmos.DrawWireSphere(transform.position, sc.radius);
        }
    }
}
using UnityEngine;

namespace OmmAlQubays
{
    public class CompassUI : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private RectTransform compassNeedle;
        [SerializeField] private bool jitterEnabled = false;
        [SerializeField] private float jitterAmplitude = 25f;
        [SerializeField] private float jitterSpeed = 2.5f;
        [SerializeField] private float jitterSeed = 7.3f;

        private float _jitterTime;

        private void Update()
        {
            if (playerTransform == null || compassNeedle == null)
                return;

            RotateNeedle();
        }

        private void RotateNeedle()
        {
            float playerYaw = playerTransform.eulerAngles.y;
            float needleAngle = -playerYaw;

            if (jitterEnabled)
            {
                _jitterTime += Time.deltaTime * jitterSpeed;
                float noise1 = (Mathf.PerlinNoise(_jitterTime, jitterSeed) - 0.5f) * 2f;
                float noise2 = (Mathf.PerlinNoise(jitterSeed, _jitterTime * 0.7f) - 0.5f) * 2f;
                float jitter = ((noise1 + noise2) * 0.5f) * jitterAmplitude;

                needleAngle += jitter;
            }
            compassNeedle.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
        }

        public void EnableJitter()
        {
            jitterEnabled = true;
            _jitterTime   = Random.Range(0f, 100f);
        }

        public void DisableJitter()
        {
            jitterEnabled = false;
        }
    }
}
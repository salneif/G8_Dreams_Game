using System.Collections;
using UnityEngine;

namespace OmmAlQubays
{
    public class JinnSilhouette : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] silhouetteMeshes;
        [SerializeField] private ParticleSystem dissolveParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float appearDistance = 15f;
        [SerializeField] private float dissolveDistance = 10f;
        [SerializeField] private float appearDuration = 0.6f;
        [SerializeField] private float dissolveHoldDuration = 0.7f;
        [SerializeField] private float dissolveDuration = 0.35f;
        [SerializeField] private float beckonSpeed = 0.9f;
        [SerializeField] private float beckonLeanAngle = 12f;
        [SerializeField] private AudioClip appearClip;
        [SerializeField] private AudioClip dissolveClip;


        private enum SilhouetteState
        {
            Hidden,
            Appearing,
            Beckoning,
            Dissolving,
            Gone       
        }

        private SilhouetteState _state = SilhouetteState.Hidden;
        private float _beckonTimer = 0f;
        private Camera _mainCamera;
        private Material[] _materialInstances;

        private void Awake()
        {
            _mainCamera = Camera.main;
            CreateMaterialInstances();
            SetAllAlpha(0f);
            SetMeshesVisible(false);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
                return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            switch (_state)
            {
                case SilhouetteState.Hidden:
                    if (distance <= appearDistance)
                        StartCoroutine(AppearSequence());
                    break;

                case SilhouetteState.Beckoning:
                    UpdateBeckonLean();
                    if (distance <= dissolveDistance)
                        StartCoroutine(DissolveSequence());
                    break;
            }
        }

        private void LateUpdate()
        {
            if (_state == SilhouetteState.Hidden || _state == SilhouetteState.Gone)
                return;
            if (_mainCamera == null)
                return;

            Vector3 directionToCamera = _mainCamera.transform.position - transform.position;
            directionToCamera.y = 0f;

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                Quaternion billboardRotation = Quaternion.LookRotation(directionToCamera);
                Quaternion leanOffset = Quaternion.Euler(transform.localEulerAngles.x, 0f, 0f);
                transform.rotation = billboardRotation * leanOffset;
            }
        }

        private void UpdateBeckonLean()
        {
            _beckonTimer += Time.deltaTime;
            float leanAngle = Mathf.Sin(_beckonTimer * beckonSpeed * Mathf.PI * 2f) * beckonLeanAngle;
            transform.localRotation = Quaternion.Euler(leanAngle, 0f, 0f);
        }

        private IEnumerator AppearSequence()
        {
            _state = SilhouetteState.Appearing;
            SetMeshesVisible(true);
            SetAllAlpha(0f);

            if (audioSource != null && appearClip != null)
                audioSource.PlayOneShot(appearClip, 0.6f);

            float elapsed = 0f;
            while (elapsed < appearDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0f, 1f, elapsed / appearDuration);
                SetAllAlpha(alpha);
                yield return null;
            }

            SetAllAlpha(1f);
            _state = SilhouetteState.Beckoning;
        }

        private IEnumerator DissolveSequence()
        {
            _state = SilhouetteState.Dissolving;

            transform.localRotation = Quaternion.identity;
            yield return new WaitForSeconds(dissolveHoldDuration);

            if (dissolveParticles != null)
                dissolveParticles.Play();

            if (audioSource != null && dissolveClip != null)
                audioSource.PlayOneShot(dissolveClip, 0.8f);

            float elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dissolveDuration);
                float alpha = (1f - t) * (1f - t);
                SetAllAlpha(alpha);
                yield return null;
            }

            SetAllAlpha(0f);
            SetMeshesVisible(false);
            _state = SilhouetteState.Gone;

            JinnLightController controller = GetComponentInParent<JinnLightController>();
            if (controller != null)
                controller.OnSilhouetteDissolved();
        }

        public void ResetSilhouette()
        {
            StopAllCoroutines();
            _state = SilhouetteState.Hidden;
            _beckonTimer = 0f;
            transform.localRotation = Quaternion.identity;
            SetAllAlpha(0f);
            SetMeshesVisible(false);
        }

        private void CreateMaterialInstances()
        {
            if (silhouetteMeshes == null || silhouetteMeshes.Length == 0)
                return;

            _materialInstances = new Material[silhouetteMeshes.Length];
            for (int i = 0; i < silhouetteMeshes.Length; i++)
            {
                if (silhouetteMeshes[i] != null)
                    _materialInstances[i] = silhouetteMeshes[i].material;
            }
        }

        private void SetAllAlpha(float alpha)
        {
            if (_materialInstances == null)
                return;

            foreach (Material mat in _materialInstances)
            {
                if (mat == null) continue;
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
        }

        private void SetMeshesVisible(bool visible)
        {
            if (silhouetteMeshes == null)
                return;
            foreach (MeshRenderer mesh in silhouetteMeshes)
                if (mesh != null)
                    mesh.enabled = visible;
        }
    }
}
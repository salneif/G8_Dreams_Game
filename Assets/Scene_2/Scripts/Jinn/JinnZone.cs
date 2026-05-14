using UnityEngine;

namespace OmmAlQubays
{
    public class JinnZone : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private LanternController lanternController;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private float delayBeforeAttack = 5f;
        [SerializeField] private float burstIntervalLanternOff = 3f;
        [SerializeField] private float burstIntervalLanternOn = 6f;
        [SerializeField] private int rocksPerBurstLanternOff = 8;
        [SerializeField] private int rocksPerBurstLanternOn = 4;
        [SerializeField] private float damagePerRock = 4f;
        [SerializeField] private float rockSpawnRadius = 12f;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private AudioSource warningAudioSource;
        [SerializeField] private AudioClip exitSound;
        [SerializeField] private MinimapController minimapController;

        private bool _playerInZone;
        private float _timeInZone;
        private float _burstTimer;
        private bool _attackStarted;
        private bool _hasBeenMapped = false;

        private void Update()
        {
            if (!_playerInZone)
                return;

            _timeInZone += Time.deltaTime;

            if (_timeInZone < delayBeforeAttack)
                return;

            if (!_attackStarted)
            {
                _attackStarted = true;
                uiManager?.ShowJinnAttackWarning();

                if (warningAudioSource != null)
                    warningAudioSource.Play();
            }

            _burstTimer -= Time.deltaTime;

            if (_burstTimer <= 0f)
            {
                ThrowBurst();

                bool lanternOn = lanternController != null && lanternController.IsOn();
                _burstTimer = lanternOn ? burstIntervalLanternOn : burstIntervalLanternOff;
            }
        }

        private void ThrowBurst()
        {
            if (rockPrefab == null || playerTransform == null || playerStats == null)
                return;

            bool lanternOn = lanternController != null && lanternController.IsOn();
            int count = lanternOn ? rocksPerBurstLanternOn : rocksPerBurstLanternOff;

            for (int i = 0; i < count; i++)
                SpawnOneRock();
        }

        private void SpawnOneRock()
        {
            Vector2 circle = Random.insideUnitCircle.normalized * rockSpawnRadius;
            Vector3 spawnPos = playerTransform.position + new Vector3(circle.x, Random.Range(3f, 9f), circle.y);

            GameObject rock = Instantiate(rockPrefab, spawnPos, Random.rotation);
            JinnRock rockScript = rock.GetComponent<JinnRock>();
            if (rockScript != null)
                rockScript.Init(playerTransform, damagePerRock, playerStats);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInZone  = true;
            _timeInZone    = 0f;
            _burstTimer    = 0f;
            _attackStarted = false;

            lanternController?.SetJinnFlicker(true, 0.8f);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            _playerInZone  = false;
            _timeInZone    = 0f;
            _burstTimer    = 0f;
            _attackStarted = false;

            lanternController?.SetJinnFlicker(false);

            uiManager?.HideJinnAttackWarning();

            if (warningAudioSource != null)
                warningAudioSource.Stop();

            if (exitSound != null)
                AudioSource.PlayClipAtPoint(exitSound, transform.position);

            if (!_hasBeenMapped)
            {
                _hasBeenMapped = true;
                SphereCollider sc = GetComponent<SphereCollider>();
                if (minimapController != null && sc != null)
                    minimapController.RegisterJinnZone(transform.position, sc.radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.15f, 0.15f, 0.2f);
            SphereCollider sc = GetComponent<SphereCollider>();
            if (sc != null)
                Gizmos.DrawWireSphere(transform.position, sc.radius);
        }
    }
}
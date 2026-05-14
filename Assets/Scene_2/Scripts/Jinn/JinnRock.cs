using UnityEngine;

namespace OmmAlQubays
{
    public class JinnRock : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;

        private Transform _target;
        private float _damage;
        private PlayerStats _playerStats;
        private bool _hasHit;

        public void Init(Transform target, float damage, PlayerStats stats)
        {
            _target      = target;
            _damage      = damage;
            _playerStats = stats;
        }

        private void Update()
        {
            if (_hasHit || _target == null)
                return;

            Vector3 aimPoint = _target.position + Vector3.up * 0.25f;

            transform.position = Vector3.MoveTowards(transform.position, aimPoint, moveSpeed * Time.deltaTime);
            transform.Rotate(Vector3.right * 200f * Time.deltaTime, Space.Self);

            if (Vector3.Distance(transform.position, aimPoint) < 0.4f)
                Hit();
        }

        private void Hit()
        {
            _hasHit = true;
            _playerStats?.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
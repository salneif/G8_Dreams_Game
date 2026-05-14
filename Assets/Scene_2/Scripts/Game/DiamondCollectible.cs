using UnityEngine;
using System;

namespace OmmAlQubays
{
    public class DiamondCollectible : Collectible
    {
        [SerializeField] private AudioClip horrorClip;
        [SerializeField] private float escapeDelay = 0.5f;
        [SerializeField] private float rotationSpeed = 30f;

        public static event Action OnDiamondCollected;

        private bool _diamondCollected = false;

        private void Awake()
        {

        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

        public override void Collect(PlayerStats playerStats)
        {
            if (_diamondCollected)
                return;

            _diamondCollected = true;

            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                mr.enabled = false;

            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
                col.enabled = false;

            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && horrorClip != null)
                audio.PlayOneShot(horrorClip);
            
            Invoke(nameof(TriggerEscape), escapeDelay);
        }

        private void TriggerEscape()
        {
            OnDiamondCollected?.Invoke();
        }
    }
}
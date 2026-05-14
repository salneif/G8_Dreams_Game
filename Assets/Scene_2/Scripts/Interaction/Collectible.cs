using System.Collections;
using UnityEngine;

namespace OmmAlQubays
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    public class Collectible : MonoBehaviour
    {
        public enum CollectibleType
        {
            Water,
            Oil,
            FalseWater
        }

        [SerializeField] private CollectibleType collectibleType = CollectibleType.Water;
        [SerializeField] private string interactPromptText = "Pick Up [E]";
        [SerializeField] private float thirstRestoreAmount = 30f;
        [SerializeField] private float oilRestoreAmount = 40f;
        [SerializeField] private float falseWaterRestoreAmount = 10f;
        [SerializeField] private float falseDrainPenalty = 0.2f;
        [SerializeField] private Light glowLight;
        [SerializeField] public AudioClip pickupClip;
        [SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.8f;
        private AudioSource _audioSource;
        private bool _hasBeenCollected = false;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f;
        }

        public string GetPromptText() => interactPromptText;

        public virtual void Collect(PlayerStats playerStats)
        {
            if (_hasBeenCollected) return;
            _hasBeenCollected = true;

            ApplyStatChange(playerStats);
            StartCoroutine(PlaySoundThenDestroy());
        }

        private void ApplyStatChange(PlayerStats playerStats)
        {
            switch (collectibleType)
            {
                case CollectibleType.Water:
                    playerStats.RestoreThirst(thirstRestoreAmount);
                    break;

                case CollectibleType.Oil:
                    playerStats.RestoreOil(oilRestoreAmount);
                    break;

                case CollectibleType.FalseWater:
                    playerStats.RestoreThirst(falseWaterRestoreAmount);
                    playerStats.IncreasePermanentDrainRate(falseDrainPenalty);
                    break;
            }
        }

        private IEnumerator PlaySoundThenDestroy()
        {
            if (glowLight != null)
                glowLight.enabled = false;

            foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
                mr.enabled = false;

            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
                col.enabled = false;

            if (pickupClip != null)
            {
                _audioSource.PlayOneShot(pickupClip, pickupVolume);
                yield return new WaitForSeconds(pickupClip.length);
            }
            else
            {
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OmmAlQubays
{
    [RequireComponent(typeof(RectTransform))]
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private RectTransform playerArrow;
        [SerializeField] private float mapCenterX = 0f;
        [SerializeField] private float mapCenterZ = 0f;
        [SerializeField] private float mapSize = 1500f;
        [SerializeField] private float minimapDisplaySize = 180f;
        [SerializeField] private Color jinnMarkerColor = new Color(0.3f, 0.7f, 1f, 0.35f);
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseMinAlpha = 0.1f;
        [SerializeField] private float pulseMaxAlpha = 0.5f;
        [SerializeField] private float markerMinSize = 10f;

        private void LateUpdate()
        {
            if (playerTransform == null || playerArrow == null)
                return;

            PositionArrow();
            RotateArrow();
        }

        private void PositionArrow()
        {
            float normalizedX = (playerTransform.position.x - (mapCenterX - mapSize * 0.5f)) / mapSize;
            float normalizedZ = (playerTransform.position.z - (mapCenterZ - mapSize * 0.5f)) / mapSize;

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);

            float localX = (normalizedX - 0.5f) * minimapDisplaySize;
            float localY = (normalizedZ - 0.5f) * minimapDisplaySize;

            playerArrow.anchoredPosition = new Vector2(localX, localY);
        }

        private void RotateArrow()
        {
            float worldYRotation = playerTransform.eulerAngles.y;
            playerArrow.localRotation = Quaternion.Euler(0f, 0f, -worldYRotation);
        }

        public void RegisterJinnZone(Vector3 worldPosition, float worldRadius)
        {
            float normalizedX = (worldPosition.x - (mapCenterX - mapSize * 0.5f)) / mapSize;
            float normalizedZ = (worldPosition.z - (mapCenterZ - mapSize * 0.5f)) / mapSize;

            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedZ = Mathf.Clamp01(normalizedZ);

            float localX = (normalizedX - 0.5f) * minimapDisplaySize;
            float localY = (normalizedZ - 0.5f) * minimapDisplaySize;

            float markerDiameter = (worldRadius * 2f / mapSize) * minimapDisplaySize;
            markerDiameter = Mathf.Max(markerDiameter, markerMinSize);

            GameObject outerGO = new GameObject("JinnZone_Outer");
            outerGO.transform.SetParent(transform, false);
            outerGO.transform.SetAsFirstSibling();

            RectTransform outerRect = outerGO.AddComponent<RectTransform>();
            outerRect.anchorMin = new Vector2(0.5f, 0.5f);
            outerRect.anchorMax = new Vector2(0.5f, 0.5f);
            outerRect.pivot    = new Vector2(0.5f, 0.5f);
            outerRect.anchoredPosition = new Vector2(localX, localY);
            outerRect.sizeDelta = new Vector2(markerDiameter, markerDiameter);

            Image outerImage = outerGO.AddComponent<Image>();
            outerImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            outerImage.color  = jinnMarkerColor;
            outerImage.raycastTarget = false;

            GameObject innerGO = new GameObject("JinnZone_Inner");
            innerGO.transform.SetParent(outerGO.transform, false);

            RectTransform innerRect = innerGO.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.5f, 0.5f);
            innerRect.anchorMax = new Vector2(0.5f, 0.5f);
            innerRect.pivot     = new Vector2(0.5f, 0.5f);
            innerRect.anchoredPosition = Vector2.zero;
            innerRect.sizeDelta = new Vector2(markerDiameter - 10f, markerDiameter - 10f);

            Image innerImage = innerGO.AddComponent<Image>();
            innerImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            innerImage.color = new Color(0.02f, 0.02f, 0.05f, 0.85f);
            innerImage.raycastTarget = false;

            StartCoroutine(PulseMarker(outerImage));
        }

        private IEnumerator PulseMarker(Image image)
        {
            while (image != null)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                Color c = image.color;
                c.a = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, t);
                image.color = c;
                yield return null;
            }
        }

        public void SetMinimapVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
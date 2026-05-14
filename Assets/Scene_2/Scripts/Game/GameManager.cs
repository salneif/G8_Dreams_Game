using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace OmmAlQubays
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private CharController playerMovement;
        [SerializeField] private LanternController lanternController;
        [SerializeField] private CanvasGroup loreScreenGroup;
        [SerializeField] private TextMeshProUGUI loreTitleText;
        [SerializeField][TextArea(1, 3)] private string loreTitleString = "أم قبيس";
        [SerializeField] private float titleFadeDuration = 1.5f;
        [SerializeField] private TextMeshProUGUI loreBodyText;
        [SerializeField][TextArea(3, 10)] private string loreBodyString = "Lore text";
        [SerializeField] private float lineFadeDuration = 0.8f;
        [SerializeField] private float linePauseDuration = 0.4f;
        [SerializeField] private float loreDisplayDuration = 3f;
        [SerializeField] private TextMeshProUGUI pressEnterText;
        [SerializeField] private float loreScreenFadeOutDuration = 1f;
        [SerializeField] private CanvasGroup introPromptGroup;
        [SerializeField] private TextMeshProUGUI introPromptText;
        [SerializeField] private float introFadeDuration = 1f;
        [SerializeField] private CanvasGroup winScreenGroup;
        [SerializeField] private Image winScreenBackground;
        [SerializeField] private Color winScreenColor = new Color(0.55f, 0.3f, 0.05f, 1f);
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private CanvasGroup loseScreenGroup;
        [SerializeField] private TextMeshProUGUI loseText;
        [SerializeField] private float fadeDuration = 2.5f;
        [SerializeField] private CanvasGroup diamondFadeGroup;
        [SerializeField] private Image diamondFadeImage;
        [SerializeField] private float diamondShakeDuration = 2f;
        [SerializeField] private float diamondFadeDuration = 3f;
        [SerializeField] private float diamondShakeIntensity = 0.3f;
        [SerializeField] private int diamondEscapeSceneIndex = 2;
        [SerializeField] private GameObject lanternHintText;

        private enum GameState { LoreScreen, WaitingForEnter, Intro, Playing, WinFade, LoseFade, DiamondEscape, Done }
        private GameState _state = GameState.LoreScreen;
        private float _fadeTimer;
        private float _shakeTimer;
        private Vector3 _originalCameraLocalPosition;
        private Transform _cameraTransform;

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _originalCameraLocalPosition = _cameraTransform.localPosition;
            }

            HideAllScreens();

            if (winScreenBackground != null)
                winScreenBackground.color = winScreenColor;

            if (playerMovement != null)
                playerMovement.SetFrozen(true);

            RealCampController.OnPlayerWin += Win;

            if (playerStats != null)
            {
                playerStats.OnThirstDepleted += Lose;
                playerStats.OnHealthDepleted += Lose;
            }

            DiamondCollectible.OnDiamondCollected += StartDiamondEscape;

            StartCoroutine(RunLoreScreen());
        }

        private void OnDestroy()
        {
            RealCampController.OnPlayerWin -= Win;

            if (playerStats != null)
            {
                playerStats.OnThirstDepleted -= Lose;
                playerStats.OnHealthDepleted -= Lose;
            }

            DiamondCollectible.OnDiamondCollected -= StartDiamondEscape;
        }

        private void Update()
        {
            switch (_state)
            {
                case GameState.WaitingForEnter:
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                        StartCoroutine(TransitionLoreToIntro());
                    break;
                case GameState.Intro:
                    UpdateIntro();
                    break;
                case GameState.WinFade:
                    StepFade(winScreenGroup);
                    break;
                case GameState.LoseFade:
                    StepFade(loseScreenGroup);
                    break;
                case GameState.DiamondEscape:
                    UpdateDiamondEscape();
                    break;
                case GameState.Done:
                    if (Input.anyKeyDown)
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
            }
        }

        private IEnumerator RunLoreScreen()
        {
            if (loreScreenGroup != null)
            {
                loreScreenGroup.gameObject.SetActive(true);
                loreScreenGroup.alpha = 1f;
            }

            if (loreTitleText != null)
            {
                loreTitleText.text  = loreTitleString;
                loreTitleText.alpha = 0f;
                loreTitleText.gameObject.SetActive(true);
            }

            if (loreBodyText != null)
            {
                loreBodyText.text = "";
                loreBodyText.gameObject.SetActive(true);
            }

            if (pressEnterText != null)
                pressEnterText.gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(0.5f);

            yield return StartCoroutine(FadeInText(loreTitleText, titleFadeDuration));

            yield return new WaitForSecondsRealtime(0.6f);

            yield return StartCoroutine(RevealBodyTextLineByLine());

            yield return new WaitForSecondsRealtime(loreDisplayDuration);

            if (pressEnterText != null)
            {
                pressEnterText.gameObject.SetActive(true);
                pressEnterText.alpha = 0f;
                yield return StartCoroutine(FadeInText(pressEnterText, 0.8f));
            }

            _state = GameState.WaitingForEnter;
        }

        private IEnumerator RevealBodyTextLineByLine()
        {
            if (loreBodyText == null) yield break;

            string[] lines    = loreBodyString.Split('\n');
            string   revealed = "";

            foreach (string line in lines)
            {
                float elapsed = 0f;
                while (elapsed < lineFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t        = Mathf.Clamp01(elapsed / lineFadeDuration);
                    int   alphaInt = Mathf.RoundToInt(t * 255);
                    string hex     = alphaInt.ToString("X2");

                    loreBodyText.text = revealed + $"<alpha=#{hex}>{line}";
                    yield return null;
                }

                revealed          += line + "\n";
                loreBodyText.text  = revealed.TrimEnd('\n');

                yield return new WaitForSecondsRealtime(linePauseDuration);
            }
        }

        private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
        {
            if (text == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed    += Time.unscaledDeltaTime;
                text.alpha  = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            text.alpha = 1f;
        }

        private IEnumerator TransitionLoreToIntro()
        {
            _state = GameState.LoreScreen;

            if (loreScreenGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < loreScreenFadeOutDuration)
                {
                    elapsed            += Time.unscaledDeltaTime;
                    loreScreenGroup.alpha = 1f - Mathf.Clamp01(elapsed / loreScreenFadeOutDuration);
                    yield return null;
                }
                loreScreenGroup.alpha = 0f;
                loreScreenGroup.gameObject.SetActive(false);
            }

            if (introPromptGroup != null)
            {
                introPromptGroup.gameObject.SetActive(true);
                introPromptGroup.alpha = 1f;
            }

            _state = GameState.Intro;
        }

        private void UpdateIntro()
        {
            if (Input.GetKeyDown(KeyCode.F))
                StartCoroutine(StartGameSequence());
        }

        private IEnumerator StartGameSequence()
        {
            if (lanternController != null)
                lanternController.TurnOn();

            if (lanternHintText != null)
                lanternHintText.SetActive(true);

            if (introPromptGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < introFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    introPromptGroup.alpha = 1f - (elapsed / introFadeDuration);
                    yield return null;
                }
                introPromptGroup.alpha = 0f;
                introPromptGroup.gameObject.SetActive(false);
            }

            if (playerMovement != null)
                playerMovement.SetFrozen(false);

            if (playerStats != null)
                playerStats.StartThirstDrain();

            _state = GameState.Playing;
        }

        private void UpdateDiamondEscape()
        {
            if (_shakeTimer < diamondShakeDuration)
            {
                _shakeTimer += Time.unscaledDeltaTime;
                ShakeCamera();
            }
            else
            {
                ShakeCamera();
                _fadeTimer += Time.unscaledDeltaTime;

                if (diamondFadeGroup != null)
                {
                    diamondFadeGroup.alpha = Mathf.Clamp01(_fadeTimer / diamondFadeDuration);

                    if (diamondFadeGroup.alpha >= 1f)
                    {
                        SceneManager.LoadScene(diamondEscapeSceneIndex);
                    }
                }
            }
        }

        private void ShakeCamera()
        {
            if (_cameraTransform == null)
                return;

            float x = Random.Range(-diamondShakeIntensity, diamondShakeIntensity);
            float y = Random.Range(-diamondShakeIntensity, diamondShakeIntensity);
            _cameraTransform.localPosition = _originalCameraLocalPosition + new Vector3(x, y, 0f);
        }

        private void StepFade(CanvasGroup screen)
        {
            _fadeTimer += Time.unscaledDeltaTime;
            screen.alpha = Mathf.Clamp01(_fadeTimer / fadeDuration);

            if (screen.alpha >= 1f)
                _state = GameState.Done;
        }

        public void Win()
        {
            if (_state != GameState.Playing)
                return;

            _state     = GameState.WinFade;
            _fadeTimer = 0f;

            FreezePlayer();
            ShowScreen(winScreenGroup);
            UnlockCursor();
        }

        public void Lose()
        {
            if (_state != GameState.Playing)
                return;

            _state     = GameState.LoseFade;
            _fadeTimer = 0f;

            FreezePlayer();
            ShowScreen(loseScreenGroup);
            UnlockCursor();
        }

        private void StartDiamondEscape()
        {
            if (_state != GameState.Playing)
                return;

            _state      = GameState.DiamondEscape;
            _shakeTimer = 0f;
            _fadeTimer  = 0f;

            FreezePlayer();
            ShowScreen(diamondFadeGroup);
            UnlockCursor();
        }

        private void FreezePlayer()
        {
            if (playerMovement != null)
                playerMovement.SetFrozen(true);
        }

        private void ShowScreen(CanvasGroup screen)
        {
            if (screen == null)
                return;

            screen.gameObject.SetActive(true);
            screen.alpha = 0f;
        }

        private void HideAllScreens()
        {
            if (loreScreenGroup != null)
            {
                loreScreenGroup.gameObject.SetActive(true);
                loreScreenGroup.alpha = 1f;
            }

            if (introPromptGroup != null)
            {
                introPromptGroup.alpha = 0f;
                introPromptGroup.gameObject.SetActive(false);
            }

            if (winScreenGroup != null)
            {
                winScreenGroup.alpha = 0f;
                winScreenGroup.gameObject.SetActive(false);
            }

            if (loseScreenGroup != null)
            {
                loseScreenGroup.alpha = 0f;
                loseScreenGroup.gameObject.SetActive(false);
            }

            if (diamondFadeGroup != null)
            {
                diamondFadeGroup.alpha = 0f;
                diamondFadeGroup.gameObject.SetActive(false);
            }
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }
}
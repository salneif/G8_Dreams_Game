using UnityEngine;

public class CharController : MonoBehaviour
{
    // Object references
    public CharacterController characterController;
    public Transform cameraTransform;
    public AudioSource audioSource;

    // Movement
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;
    public float sandPenalty = 0.85f;

    // Stamina
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRecoverRate = 12f;
    public float staminaRecoverThreshold = 30f;

    // Settings
    public float mouseSensitivity = 200f;
    public float verticalLookMin = -90f;
    public float verticalLookMax = 90f;
    public float gravity = -20f;
    public float groundStick = -2f;
    public AudioClip[] footstepClips;
    public float footstepIntervalWalk = 0.55f;
    public float footstepIntervalRun = 0.35f;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float currentStamina;
    private bool isExhausted;
    private bool isRunning;
    private float footstepTimer;
    private float thirstSpeedModifier = 1f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        cameraRot();
        move();
        footsteps();
    }

    void move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = transform.forward * vertical + transform.right * horizontal;
        bool isMoving = direction.magnitude > 0.1f;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && isMoving;
        updateStamina(wantsToRun);
        isRunning = wantsToRun && !isExhausted;

        float speed = isRunning ? runSpeed : walkSpeed;
        speed *= sandPenalty;
        speed *= thirstSpeedModifier;

        if (characterController.isGrounded)
            velocity.y = groundStick;
        else
            velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = direction * speed + Vector3.up * velocity.y;
        characterController.Move(finalMove * Time.deltaTime);
    }

    void updateStamina(bool wantsToRun)
    {
        if (wantsToRun && !isExhausted)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
            }
        }
        else
        {
            currentStamina += staminaRecoverRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            if (isExhausted && currentStamina >= staminaRecoverThreshold)
                isExhausted = false;
        }
    }

    void cameraRot()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, verticalLookMin, verticalLookMax);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void footsteps()
    {
        bool isMoving = characterController.velocity.magnitude > 0.1f && characterController.isGrounded;

        if (!isMoving)
        {
            audioSource.Stop();
            return;
        }

        if (footstepClips == null || footstepClips.Length == 0) 
            return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            footstepTimer = isRunning ? footstepIntervalRun : footstepIntervalWalk;
            audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
        }
    }

    public void SetThirstSpeedModifier(float modifier)
    {
        thirstSpeedModifier = Mathf.Clamp01(modifier);
    }

    public float StaminaNormalized()
    {
        return currentStamina / maxStamina;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}
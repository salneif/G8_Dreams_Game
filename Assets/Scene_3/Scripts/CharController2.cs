using UnityEngine;

public class CharController2 : MonoBehaviour
{
    //Object references
    public CharacterController characterController;
    public Transform cameraTransform;
    public Vector3 velocity;
    public AudioSource audioSource;

    //Settings
    public float speed = 10f;
    private float gravity = -9.81f;
    public float jumpForce = 1.5f;
    public float mouseSensitivity = 200f;

    //Persistent attributes
    private int jumpCount = 0;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool _frozen = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); 
    }

    void Update()
    {
        if (_frozen)
            return;
        cameraRot();
        jump();
    }
    
    void jump()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * Vertical + transform.right * Horizontal;

        if (Input.GetButtonDown("Jump") && jumpCount < 2)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpCount++;
            audioSource.PlayOneShot(audioSource.clip);
        }

        velocity.y += gravity * Time.deltaTime;
        Vector3 finalMove = move * speed + velocity;
        characterController.Move(finalMove * Time.deltaTime);
        isGrounded = characterController.isGrounded;

        if (isGrounded)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }
    }

    void cameraRot()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void SetFrozen(bool frozen)
    {
        _frozen = frozen;
    }
}
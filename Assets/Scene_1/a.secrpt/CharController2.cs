using UnityEngine;
using Unity.Cinemachine;

public class CharController2 : MonoBehaviour
{
    public CharacterController characterController;

    public float speed = 5f;
    public Vector3 velocity;

    public float gravity = -9.8f;
    public float jumpHeight = 3f;

    //public Transform camera;

    public Transform respawnPoint;
    public Transform target;
    public Transform camera1;

    public float mouseSensitivity = 180f;
    public float xRotation;
    public float yRotation;
    public float maxY = 90f;
    public float minY = -90f;
    bool grounded;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            transform.position = respawnPoint.position;
            velocity = Vector3.zero; // يمنع الطيران بعد الرجوع
        }
    }

    void Update()
    {
        cameraRotation();
        grounded = characterController.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }
    void cameraRotation()
    {
        float xMouse = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float yMouse = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= yMouse;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        camera1.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        target.Rotate(Vector3.up * xMouse);
    }
}
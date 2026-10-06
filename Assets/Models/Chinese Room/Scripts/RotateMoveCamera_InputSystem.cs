using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class RotateMoveCamera_InputSystem : MonoBehaviour
{
    [Header("Rotación")]
    public float minX = -360.0f;
    public float maxX = 360.0f;
    public float minY = -89.0f;
    public float maxY = 89.0f;
    public float sensitivityX = 100.0f;
    public float sensitivityY = 100.0f;

    [Header("Movimiento")]
    public float moveSpeed = 5.0f;

    private float rotationY = 0.0f;
    private float rotationX = 0.0f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Camera cam;

    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction lookAction;

    void Awake()
    {
        cam = GetComponent<Camera>();

        // Obtiene las acciones del asset
        moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        lookAction = inputActions.FindActionMap("Player").FindAction("Look");

        // Suscribirse a eventos (opcional)
        moveAction.Enable();
        lookAction.Enable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Rotate();
        Move();
    }

    void Rotate()
    {
        lookInput = lookAction.ReadValue<Vector2>();

        rotationX += lookInput.x * sensitivityX * Time.deltaTime;
        rotationY += lookInput.y * sensitivityY * Time.deltaTime;

        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }

    void Move()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        Vector3 movement = transform.forward * moveInput.y + transform.right * moveInput.x;
        movement *= moveSpeed * Time.deltaTime;

        transform.position += movement;
    }
}
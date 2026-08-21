using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkingSpeed = 5.0f;
    public float lookSpeed = 0.5f;
    public float lookXLimit = 85.0f;

    public InputActionAsset inputAsset;
    private InputAction moveAction;
    private InputAction lookAction;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;
    private bool cursorLocked = true;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

#if UNITY_EDITOR
        if (inputAsset == null)
        {
            inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        }
#endif
        if (inputAsset != null)
        {
            var playerMap = inputAsset.FindActionMap("Player");
            if (playerMap != null)
            {
                moveAction = playerMap.FindAction("Move");
                lookAction = playerMap.FindAction("Look");
                moveAction?.Enable();
                lookAction?.Enable();
            }
        }
    }

    void OnDestroy()
    {
        moveAction?.Disable();
        lookAction?.Disable();
    }

    void Start()
    {
        SetCursorLock(true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLock(!cursorLocked);
        }

        Vector2 moveInput = Vector2.zero;
        Vector2 lookInput = Vector2.zero;

        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        }

        if (lookAction != null && cursorLocked)
        {
            lookInput = lookAction.ReadValue<Vector2>();
        }
        else if (Mouse.current != null && cursorLocked)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float curSpeedX = canMove ? walkingSpeed * moveInput.y : 0;
        float curSpeedY = canMove ? walkingSpeed * moveInput.x : 0;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        
        if (!characterController.isGrounded)
        {
            moveDirection.y -= 9.8f * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove && Camera.main != null && cursorLocked)
        {
            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed, 0);
        }
    }

    private void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}

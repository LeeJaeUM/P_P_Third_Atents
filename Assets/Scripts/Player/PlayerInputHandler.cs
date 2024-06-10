using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 InputDirection { get; private set; }
    public event System.Action OnMove;
    public event System.Action OnStop;
    public event System.Action OnJumpPressed;
    public event System.Action OnAttackPressed;
    public event System.Action OnRollPressed;
    public event System.Action OnBlockPressed;
    public event System.Action OnBlockReleased;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnStopPerformed;
        inputActions.Player.Jump.performed += context => OnJumpPressed?.Invoke();
        inputActions.Player.Attack.performed += context => OnAttackPressed?.Invoke();
        inputActions.Player.Roll.performed += context => OnRollPressed?.Invoke();
        inputActions.Player.Block.performed += context => OnBlockPressed?.Invoke();
        inputActions.Player.Block.canceled += context => OnBlockReleased?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Player.Block.canceled -= context => OnBlockReleased?.Invoke();
        inputActions.Player.Block.performed -= context => OnBlockPressed?.Invoke();
        inputActions.Player.Roll.performed -= context => OnRollPressed?.Invoke();
        inputActions.Player.Attack.performed -= context => OnAttackPressed?.Invoke();
        inputActions.Player.Jump.performed -= context => OnJumpPressed?.Invoke();
        inputActions.Player.Move.canceled -= OnStopPerformed;
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        InputDirection = context.ReadValue<Vector2>();
        OnMove?.Invoke();
    }

    private void OnStopPerformed(InputAction.CallbackContext context)
    {
        InputDirection = Vector2.zero;
        OnStop?.Invoke();
    }
}

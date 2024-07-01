using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 InputDirection { get; private set; }

    public event System.Action OnMove;
    public event System.Action OnJumpPressed;
    public event System.Action OnAttackPressed;
    public event System.Action OnRollPressed;
    public event System.Action OnBlockPressed;
    public event System.Action OnBlockReleased;
    public event System.Action OnSkillPressed;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerCan;
        inputActions.Player.Move.canceled += OnMovePerCan;
        inputActions.Player.Jump.performed += context => OnJumpPressed?.Invoke();
        inputActions.Player.Attack.performed += context => OnAttackPressed?.Invoke();
        inputActions.Player.Roll.performed += context => OnRollPressed?.Invoke();
        inputActions.Player.Block.performed += context => OnBlockPressed?.Invoke();
        inputActions.Player.Block.canceled += context => OnBlockReleased?.Invoke();
        inputActions.Player.Skill.performed += context => OnSkillPressed?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Player.Skill.performed -= context => OnSkillPressed?.Invoke();
        inputActions.Player.Block.canceled -= context => OnBlockReleased?.Invoke();
        inputActions.Player.Block.performed -= context => OnBlockPressed?.Invoke();
        inputActions.Player.Roll.performed -= context => OnRollPressed?.Invoke();
        inputActions.Player.Attack.performed -= context => OnAttackPressed?.Invoke();
        inputActions.Player.Jump.performed -= context => OnJumpPressed?.Invoke();
        inputActions.Player.Move.canceled -= OnMovePerCan;
        inputActions.Player.Move.performed -= OnMovePerCan;
        inputActions.Player.Disable();
    }

    private void OnMovePerCan(InputAction.CallbackContext context)
    {
        InputDirection = context.ReadValue<Vector2>();
        OnMove?.Invoke();
    }
}

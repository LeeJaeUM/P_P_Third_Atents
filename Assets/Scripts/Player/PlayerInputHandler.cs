using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 InputDirection { get; private set; }

    //----------PlayerInput----------
    public event System.Action OnMove;
    public event System.Action OnJumpPressed;
    public event System.Action OnAttackPressed;
    public event System.Action OnRollPressed;
    public event System.Action OnBlockPressed;
    public event System.Action OnBlockReleased;
    public event System.Action OnSkillPressed;

    //---------------UI-----------
    public event System.Action OnWASDPressed;
    public event System.Action<bool> OnESCMenuPressed;
    public event System.Action OnInteractPressed;
    public event System.Action OnNewSkillPressed;

    private PlayerInputActions inputActions;

    private bool isOpen = false;

    #region UnituEvent

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        PlayerInputEnable();
        UIInputEnable();
    }

    private void OnDisable()
    {
        PlayerInputDisable();
        UIInputDisable();
    }

    #endregion

    private void PlayerInputEnable()
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

    private void PlayerInputDisable()
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

    private void UIInputEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.WASD.performed += OnWASDPerformed;
        inputActions.UI.ESCMenu.performed += (context) => 
        {
            isOpen = !isOpen;
            OnESCMenuPressed?.Invoke(isOpen);
        };
        inputActions.UI.Interact.performed += context => OnInteractPressed?.Invoke();
        inputActions.UI.Skill.performed += context => OnNewSkillPressed?.Invoke();
    }
    private void UIInputDisable()
    {
        inputActions.UI.Skill.performed -= context => OnNewSkillPressed?.Invoke();
        inputActions.UI.Interact.performed -= context => OnInteractPressed?.Invoke();
        inputActions.UI.ESCMenu.performed -= context => OnESCMenuPressed?.Invoke(isOpen);
        inputActions.UI.WASD.performed -= OnWASDPerformed;
        inputActions.UI.Disable();
    }
    private void OnWASDPerformed(InputAction.CallbackContext context)
    {
        InputDirection = context.ReadValue<Vector2>();
        OnWASDPressed?.Invoke();
    }
}

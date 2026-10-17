using UnityEngine;
using UnityEngine.InputSystem;
using InputMap;
public class InputController : MonoBehaviour, InputMap.PlayerInput.IGameplayActions
{
    InputMap.PlayerInput input;
    private void Awake()
    {
        input = new InputMap.PlayerInput();
        input.Gameplay.AddCallbacks(this);
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private void OnDestroy()
    {
        input.Gameplay.RemoveCallbacks(this);
        input.Dispose();
    }

    public void OnBreak(InputAction.CallbackContext context)
    {
        Debug.Log("Break");
    }

    public void OnGas(InputAction.CallbackContext context)
    {
        Debug.Log("Gas");
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        Debug.Log("Steer");
    }
}

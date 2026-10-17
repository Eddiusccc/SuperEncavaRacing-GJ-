using UnityEngine;
using UnityEngine.InputSystem;
using InputMap;
public class InputController : MonoBehaviour, InputMap.PlayerInput.IGameplayActions
{
    InputMap.PlayerInput input;
    CarController carController;
    private void Awake()
    {
        input = new InputMap.PlayerInput();
        input.Gameplay.AddCallbacks(this);
        carController = FindAnyObjectByType<CarController>();
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
        carController.SetBrakeInput(context.ReadValue<float>());
    }

    public void OnGas(InputAction.CallbackContext context)
    {
        carController.SetGasInput(context.ReadValue<float>());
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        carController.SetSteerInput(context.ReadValue<float>());
    }
}

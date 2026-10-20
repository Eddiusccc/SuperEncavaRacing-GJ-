using Framework.Input;
using UnityEngine;

public class InputDebug : MonoBehaviour
{
    [SerializeField] InputReader input;
    void OnEnable()
    {
        input.JumpPressed += OnJump;
        input.InteractPressed += OnInteract;
        input.PausePressed += OnPause;
    }

    void OnDisable()
    {
        input.JumpPressed -= OnJump;
        input.InteractPressed -= OnInteract;
        input.PausePressed -= OnPause;
    }
    void OnJump()       => Debug.Log("Salto");
    void OnInteract()   => Debug.Log("Interactuar");
    void OnPause()      => Debug.Log("Pausa");
    void Update()
    {
        // Debug de Move
        if (input.Move != Vector2.zero)
        {
            //Debug.Log($"Move: {input.Move}");
        }
    }
}
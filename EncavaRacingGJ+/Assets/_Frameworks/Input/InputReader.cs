using System;
using Framework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Framework.Input
{
    /// <summary>
    /// Wrapper que encapsula el sistema de entrada personalizado (sin usar Unity Input System).
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Framework/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.ICommonActions
    {
        // Clase generada que implementa IGameplayActions y ICommonActions.
        [SerializeField] GameInput controls;

        // Valores que se pueden consultar en cualquier momento (polling)
        public Vector2 Move {get; private set; }
        public Vector2 Look {get; private set; }
        public bool LookFromGamepad {get; private set; }
        public bool JumpHeld {get; private set; }

        // Eventos de "acabo de pasar" (pusar/saltar)
        public event Action JumpPressed;
        public event Action JumpReleased;
        public event Action DashPressed;
        public event Action InteractPressed;
        public event Action PausePressed;

        // Eventos NO Implementados
        public event Action AttackPressed;
        public event Action CrouchPresed;
        public event Action PreviousPressed;
        public event Action NextPressed;
        public bool GameplayEnable => controls != null && controls.Gameplay.enabled;

        void OnEnable()
        {
            if (controls == null)
            {
                controls = new GameInput();
                controls.Gameplay.SetCallbacks(this);
                controls.Common.SetCallbacks(this);
            }
            controls.Common.Enable();
            controls.Gameplay.Enable();
        }

        void OnDisable()
        {
            controls?.Gameplay.Disable();
            controls?.Common.Disable();
        }

        // ----- Control del modo de input ------
        /// <summary>
        /// Habilita el sistema de entrada.
        /// </summary>
        public void EnableGameplay()
        {
            controls.Gameplay.Enable();
            EventBus.Publish(new GameplayInputChanged(true));
        }
        /// <summary>
        /// Deshabilita el sistema
        /// </summary>
        public void DisableGameplay()
        {
            controls.Gameplay.Disable();
            Move = Vector2.zero;
            Look = Vector2.zero;
            EventBus.Publish(new GameplayInputChanged(false));
        }

        // ----- Callbacks de GameInput.IGameplayActions -----
        /// <summary>
        /// Procesa movimiento
        /// </summary>
        /// <param name="ctx"></param>
        public void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();

        /// <summary>
        /// Procesa mirada
        /// </summary>
        /// <param name="ctx"></param>
        public void OnLook(InputAction.CallbackContext ctx)
        {
            Look = ctx.ReadValue<Vector2>();
            if (ctx.control != null) LookFromGamepad = ctx.control.device is Gamepad;
        }

        /// <summary>
        /// Procesa salto
        /// </summary>
        /// <param name="ctx"></param>
        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                JumpHeld = true;
                JumpPressed?.Invoke();
            }
            else if (ctx.canceled)
            {
                JumpHeld = false;
                JumpReleased?.Invoke();
            }
        }

        /// <summary>
        /// Procesa Dash
        /// </summary>
        /// <param name="ctx"></param>
        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (ctx.started) DashPressed?.Invoke();
        }

        /// <summary>
        /// Procesa interaccion
        /// </summary>
        /// <param name="ctx"></param>
        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.started) InteractPressed?.Invoke();
        }

        /// <summary>
        /// Procesa Ataque
        /// </summary>
        /// <param name="ctx"></param>
        public void OnAttack(InputAction.CallbackContext ctx)
        {
            // Funcion de Ataque
            if (ctx.started) AttackPressed?.Invoke();
        }
        public void OnCrouch(InputAction.CallbackContext ctx){
            // Funcion de Agachado
            if (ctx.started) CrouchPresed?.Invoke();
        }
        public void OnPrevious(InputAction.CallbackContext ctx){
            // Funcion de Previo (ni idea que)
            if (ctx.started) PreviousPressed?.Invoke();
        }
        public void OnNext(InputAction.CallbackContext ctx){
            // Funcion de Siguiente (ni idea que)
            if (ctx.started) NextPressed?.Invoke();
        }

        // ----- Callback de GameInput.ICommonActions -----
        /// <summary>
        /// Procesa pausa en mapa de acciones diferente
        /// </summary>
        /// <param name="ctx"></param>
        public void OnPause(InputAction.CallbackContext ctx)
        {
            if (ctx.started) PausePressed?.Invoke();
        }
    }
}
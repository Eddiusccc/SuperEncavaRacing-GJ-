using Framework.Input;
using UnityEngine;

namespace Framework.CameraRig
{
    public class LookRotator : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] Transform yawTarget; // gira en horizontal
        [SerializeField] Transform pitchTarget; // gira en vertical
        [SerializeField] float mouseSensitivity = 0.12f; // grados por unidad de delta
        [SerializeField] float stickSensitivity = 150f; // grados por segundo
        [SerializeField] float minPitch = -80f;
        [SerializeField] float maxPitch = 80f;

        float yaw;
        float pitch;

        void Start()
        {
            yaw = yawTarget.eulerAngles.y;
            pitch = Mathf.DeltaAngle(0f, pitchTarget.eulerAngles.x);
        }

        void LateUpdate()
        {
            Vector2 look = input.Look;
            float scale = input.LookFromGamepad ? stickSensitivity * Time.deltaTime : mouseSensitivity;

            yaw += look.x * scale;
            pitch = Mathf.Clamp(pitch - look.y * scale, minPitch, maxPitch);

            if (yawTarget == pitchTarget)
            {
                // 3ra persona: un solo objeto orbita en los dos ejes
                yawTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else
            {
                // 1ra persona: el cuerpo gira en horizontal, la cabeza en vertical
                yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
                pitchTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }
}
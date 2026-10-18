using Unity.Cinemachine;
using UnityEngine;

namespace Framework.CameraRig
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson,
        Follow2D
    }

    public class CameraRigSwitcher : MonoBehaviour
    {
        [Header("Cámaras virtuales")]
        [SerializeField] CinemachineCamera firstPerson;
        [SerializeField] CinemachineCamera thirdPerson;
        [SerializeField] CinemachineCamera follow2D;

        [Header("Opcional")]
        [SerializeField] LookRotator firstPersonLook;
        [SerializeField] LookRotator thirdPersonLook;
        [SerializeField] GameObject playerVisual;   // se oculta en 1ª persona

        [SerializeField] CameraMode startMode = CameraMode.ThirdPerson;

        const int ActivePriority = 20;
        const int InactivePriority = 0;

        public CameraMode Current { get; private set; }

        void Start() => Switch(startMode);

        public void Switch(CameraMode mode)
        {
            SetPriority(firstPerson, mode == CameraMode.FirstPerson);
            SetPriority(thirdPerson, mode == CameraMode.ThirdPerson);
            SetPriority(follow2D,    mode == CameraMode.Follow2D);

            if (firstPersonLook != null && startMode.Equals(firstPerson)) firstPersonLook.enabled = mode == CameraMode.FirstPerson;
            if (thirdPersonLook != null && startMode.Equals(thirdPerson)) thirdPersonLook.enabled = mode == CameraMode.ThirdPerson;
            // if (playerVisual != null)    playerVisual.SetActive(mode != CameraMode.FirstPerson);

            Current = mode;
        }

        static void SetPriority(CinemachineCamera cam, bool active)
        {
            if (cam == null) return;
            cam.Priority = new PrioritySettings
            {
                Enabled = true,
                Value = active ? ActivePriority : InactivePriority
            };
        }

        [ContextMenu("Test: 1ª persona")] void TestFirst() => Switch(CameraMode.FirstPerson);
        [ContextMenu("Test: 3ª persona")] void TestThird() => Switch(CameraMode.ThirdPerson);
        [ContextMenu("Test: 2D")]         void TestTwoD()  => Switch(CameraMode.Follow2D);
    }
}

using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow freeLookCam;
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private Camera cam;

    private void OnEnable()
    {
        PlayerMovement.OnDriftBoost += SpeedBoostCamera;
    }

    private void OnDisable()
    {
        PlayerMovement.OnDriftBoost -= SpeedBoostCamera;
    }

    private void SpeedBoostCamera()
    {
        Debug.Log("Recentering cameras");
        inputAxisController.enabled = false;
        freeLookCam.VerticalAxis.TriggerRecentering();
        Invoke("ReturnCameraToNormal", 2);
    }

    private void ReturnCameraToNormal()
    {
        inputAxisController.enabled = true;

    }
}

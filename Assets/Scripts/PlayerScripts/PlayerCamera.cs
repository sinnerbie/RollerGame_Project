using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow freeLookCam;
    [SerializeField] private CinemachineInputAxisController inputAxisController;
    [SerializeField] private CinemachineCamera cam;

    private void OnEnable()
    {
        PlayerMovement.OnDriftBoost += SpeedBoostCamera;
        PlayerGrind.OnGrindStart += GrindRailCamera;
        PlayerGrind.OnGrindEnd += EndGrindCam;
    }

    private void OnDisable()
    {
        PlayerMovement.OnDriftBoost -= SpeedBoostCamera;
        PlayerGrind.OnGrindStart -= GrindRailCamera;
        PlayerGrind.OnGrindEnd -= EndGrindCam;
    }

    private void SpeedBoostCamera()
    {
        inputAxisController.enabled = false;
        var recentering = freeLookCam.HorizontalAxis.Recentering;
        recentering.Enabled = true;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
        recentering = freeLookCam.VerticalAxis.Recentering;
        recentering.Enabled = true;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
        Invoke("EndSpeedCam", 2);
    }

    private void EndSpeedCam()
    {
        inputAxisController.enabled = true;
        var recentering = freeLookCam.HorizontalAxis.Recentering;
        recentering.Enabled = false;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
        recentering = freeLookCam.VerticalAxis.Recentering;
        recentering.Enabled = false;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
    }

    private void GrindRailCamera()
    {
        inputAxisController.enabled = false;
        var recentering = freeLookCam.HorizontalAxis.Recentering;
        recentering.Enabled = true;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
        recentering = freeLookCam.VerticalAxis.Recentering;
        recentering.Enabled = true;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
    }

    private void EndGrindCam()
    {
        inputAxisController.enabled = true;
        var recentering = freeLookCam.HorizontalAxis.Recentering;
        recentering.Enabled = false;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
        recentering = freeLookCam.VerticalAxis.Recentering;
        recentering.Enabled = false;
        recentering.Wait = 0;
        recentering.Time = 0.15f;
        freeLookCam.HorizontalAxis.Recentering = recentering;
    }
}

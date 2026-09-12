using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SpeedometerDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text velText;
    [SerializeField] private Text storedVelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image velGauge;
    [SerializeField] private RectTransform maxVelDisplay;
    [SerializeField] private Vector2 originalSizeDelta;
    [SerializeField] private Image driftInputIcon;
    [SerializeField] private Image chargeGlow;

    private void OnEnable()
    {
        PlayerMovement.OnChangeVelocity += DisplayCurrentVelocity;
        PlayerMovement.OnChangeMaxVel += UpdateMaxVelocity;
        PlayerMovement.OnStoreVelocity += StoreVelocity;
        PlayerMovement.OnReleaseStoredVelocity += ReleaseVelocity;
    }

    private void OnDisable()
    {
        PlayerMovement.OnChangeVelocity -= DisplayCurrentVelocity;
        PlayerMovement.OnChangeMaxVel -= UpdateMaxVelocity;
        PlayerMovement.OnStoreVelocity -= StoreVelocity;
        PlayerMovement.OnReleaseStoredVelocity -= ReleaseVelocity;
    }

    void Start()
    {
        originalSizeDelta = maxVelDisplay.sizeDelta;
    }

    private void DisplayCurrentVelocity(float newVel, float maxVel)
    {
        newVel = Mathf.Round(newVel * 10) * 0.1f;
        velText.text = newVel.ToString() + "m/s";
        velGauge.fillAmount = newVel / maxVel;
    }

    private void UpdateMaxVelocity(float oldMax, float newMax)
    {
        float disparity = newMax / oldMax;
        maxVelDisplay.DOSizeDelta(new Vector2(originalSizeDelta.x, originalSizeDelta.y * disparity), 0.15f);
    }

    private void StoreVelocity(float vel)
    {
        driftInputIcon.enabled = false;
        storedVelText.text = vel.ToString() + "m/s";
        chargeGlow.DOFade(1, 0.15f);
    }

    private void ReleaseVelocity()
    {
        driftInputIcon.enabled = true;
        storedVelText.text = "";
        chargeGlow.DOFade(0, 0.15f);
    }
}

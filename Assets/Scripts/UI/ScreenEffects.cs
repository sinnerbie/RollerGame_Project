using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class ScreenEffects : MonoBehaviour
{
    [SerializeField] private Image blackScreen;

    private void OnEnable()
    {
        HealthManager.OnKillPlayer += FadeInScreen;
        HealthManager.OnPlayerRespawn += FadeOutScreen;
    }

    private void OnDisable()
    {
        HealthManager.OnKillPlayer -= FadeInScreen;
        HealthManager.OnPlayerRespawn -= FadeOutScreen;
    }

    private void Start()
    {
        blackScreen.DOFade(0, 0.15f);
    }

    private void FadeOutScreen()
    {
        blackScreen.DOFade(0, 0.5f);
    }

    private void FadeInScreen()
    {
        blackScreen.DOFade(1, 0.5f);
    }
}

using System;
using UnityEngine;

public class PlayerRailDetection : MonoBehaviour
{
    public static Action<Collider> OnHitRail;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Rail")
            OnHitRail?.Invoke(other);
    }
}

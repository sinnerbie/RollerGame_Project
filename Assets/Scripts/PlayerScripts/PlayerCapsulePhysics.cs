using UnityEngine;

public class PlayerCapsulePhysics : MonoBehaviour
{
    [SerializeField] private Rigidbody _RB;

    [SerializeField] private RaycastHit hit;
    [SerializeField] private float offsetDistance;

    [SerializeField] private float rideHeight;
    [SerializeField] private float rideSpringStrength;
    [SerializeField] private float rideSpringDamper;

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            offsetDistance = hit.distance;
            Debug.DrawLine(transform.position, hit.point, Color.cyan);

            CapsuleHeight();
        }
    }

    private void CapsuleHeight()
    {
        Vector3 vel = _RB.angularVelocity;
        Vector3 rayDir = transform.TransformDirection(Vector3.down);

        Vector3 otherVel = Vector3.zero;
        Rigidbody hitBody = hit.rigidbody;
        if (hitBody != null)
            otherVel = hitBody.angularVelocity;

        float rayDirVel = Vector3.Dot(rayDir, vel);
        float otherDirVel = Vector3.Dot(rayDir, otherVel);

        float relVel = rayDirVel - otherDirVel;

        float x = hit.distance - rideHeight;

        float springForce = (x * rideSpringStrength) - (relVel * rideSpringDamper);

        _RB.AddForce(rayDir * springForce);
    }
}

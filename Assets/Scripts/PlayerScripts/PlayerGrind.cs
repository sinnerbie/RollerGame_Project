using System;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class PlayerGrind : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private bool jump;
    [SerializeField] private Vector3 input;

    [Header("Variables")]
    public bool onRail;
    public float grindSpeed;
    [SerializeField] private float heightOffset;
    private float timeForFullSpline;
    private float elapsedTime;
    [SerializeField] private float lerpSpeed = 10;
    public bool justGrinded = false;

    [Header("References")]
    [SerializeField] GrindRail currentGrindRail;
    [SerializeField] Rigidbody _RB;
    [SerializeField] PlayerMovement movement;

    public static Action OnGrindStart;
    public static Action OnGrindEnd;

    private void OnEnable()
    {
        PlayerRailDetection.OnHitRail += StartGrinding;
    }

    private void OnDisable()
    {
        PlayerRailDetection.OnHitRail -= StartGrinding;
    }

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }

    public void HandleJump(InputAction.CallbackContext context)
    {
        jump = Convert.ToBoolean(context.ReadValue<float>());
    }

    public void HandleMovement(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        input.x = rawInput.x;
        input.z = rawInput.y;
    }

    void FixedUpdate()
    {
        if (onRail)
            MovePlayerAlongRail();
    }

    void MovePlayerAlongRail()
    {
        if (currentGrindRail != null && onRail)
        {
            float progress = elapsedTime / timeForFullSpline;

            if (progress < 0 || progress > 1)
            {
                ThrowOffRail();
                transform.position = transform.position + (transform.forward * 1.25f);
                _RB.AddForce(transform.forward * grindSpeed, ForceMode.VelocityChange);
                movement.currentVelocity = grindSpeed;
                return;
            }

            float nextTimeNormalized;
            if (currentGrindRail.normalDirection)
                nextTimeNormalized = (elapsedTime + Time.deltaTime) / timeForFullSpline;
            else
                nextTimeNormalized = (elapsedTime - Time.deltaTime) / timeForFullSpline;

            float3 pos, tangent, up;
            float3 nextPosFloat, nextTan, nextUp;
            SplineUtility.Evaluate(currentGrindRail.railSpline.Spline, progress, out pos, out tangent, out up);
            SplineUtility.Evaluate(currentGrindRail.railSpline.Spline, nextTimeNormalized, out nextPosFloat, out nextTan, out nextUp);

            Vector3 worldPos = currentGrindRail.LocalToWorldConversion(pos);
            Vector3 nextPos = currentGrindRail.LocalToWorldConversion(nextPosFloat);

            Vector3 setToPos = worldPos + (transform.up * heightOffset);
            transform.position = setToPos;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(nextPos - worldPos), lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.FromToRotation(transform.up, up) * transform.rotation, lerpSpeed * Time.deltaTime);

            if (currentGrindRail.normalDirection)
                elapsedTime += Time.deltaTime;
            else
                elapsedTime -= Time.deltaTime;
        }
    }

    void StartGrinding(Collider hit)
    {
        grindSpeed = movement.currentVelocity;
        onRail = true;
        _RB.useGravity = false;
        currentGrindRail = hit.gameObject.GetComponent<GrindRail>();
        CalculateAndSetRailPosition();
        OnGrindStart?.Invoke();
    }

    void CalculateAndSetRailPosition()
    {
        timeForFullSpline = currentGrindRail.totalSplineLength / grindSpeed;
        Vector3 splinePoint;
        float normalizedTime = currentGrindRail.CalculateTargetRailPoint(transform.position, out splinePoint);
        elapsedTime = timeForFullSpline * normalizedTime;
        float3 pos, forward, up;
        SplineUtility.Evaluate(currentGrindRail.railSpline.Spline, normalizedTime, out pos, out forward, out up);
        currentGrindRail.CalculateDirection(forward, transform.forward);
        transform.position = splinePoint + (transform.up * heightOffset);
    }

    public void ThrowOffRail()
    {
        justGrinded = true;
        onRail = false;
        currentGrindRail = null;
        _RB.useGravity = true;
        OnGrindEnd?.Invoke();
    }
}

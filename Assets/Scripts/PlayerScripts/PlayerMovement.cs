using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _RB;
    [SerializeField] private Transform mainCam;
    [SerializeField] private PlayerGrind railGrinding;
    [SerializeField] private PlayerInput playerInput;
    private InputAction drift;

    [Header("Locomotion")]
    [SerializeField] private Vector3 inputDirection;
    [SerializeField] private Vector3 speed;
    public float currentVelocity;
    [SerializeField] private float[] varyingMaxSpeeds = { 8, 12, 16, 20};
    [SerializeField] private int speedID = 0;
    public int maxSpeedID
    {
        get { return speedID; }
        set
        {
            speedID = value;
            if (speedID < 0 )
                speedID = 0;
            if (speedID >= varyingMaxSpeeds.Length)
                speedID = varyingMaxSpeeds.Length - 1;

            maxSpeed = varyingMaxSpeeds[speedID];
        }
    }

    public float maxSpeed = 8;
    [SerializeField] private float acceleration = 200;
    [SerializeField] private float MaxAccelForce = 150;
    [SerializeField] private float rotSpeed = 150;

    [Header("Drifting")]
    [SerializeField] private float storedVelocity;
    [SerializeField] private bool driftHold = false;
    [SerializeField] private bool driftOnCooldown = false;
    [SerializeField] private float driftCooldownDuration = 2;
    [SerializeField] private float driftMultiplier = 1.5f;

    [Header("Jumping")]
    [SerializeField] private bool isAirborne = false;
    [SerializeField] private bool goingUp = false;
    [SerializeField] private float jumpForce = 2;
    [SerializeField] private Vector3 jumpMomentum;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpMultiplier;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private Vector3 checkOffset;
    [SerializeField] private float sphereRadius;
    [SerializeField] private float normalThreshold;
    private float groundAngle;
    private Vector3 spherePosition;

    [Header("Active Inputs")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isDrifting = false;
    [SerializeField] private bool justJumped = false;
    [SerializeField] private bool holdJump = false;

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
        railGrinding = GetComponent<PlayerGrind>();
    }

    void Start()
    {
        drift = playerInput.actions["Drift"];
        drift.performed += DriftHold;
        drift.canceled += DriftUp;
        maxSpeed = varyingMaxSpeeds[speedID];
    }

    void Update()
    {
        if (isMoving || isDrifting)
        {
            Quaternion toRotation = Quaternion.LookRotation(inputDirection * 10, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotSpeed * Time.deltaTime);
        }

        GroundCheck();

        if (isAirborne)
        {
            if (_RB.linearVelocity.y < 0)
            {
                _RB.linearVelocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.deltaTime;
                goingUp = false;
            }
            else if (_RB.linearVelocity.y > 0 && !holdJump)
            {
                _RB.linearVelocity += Vector3.up * Physics.gravity.y * lowJumpMultiplier * Time.deltaTime;
                goingUp = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving && !isDrifting && !railGrinding.onRail)
        {
            speed = Vector3.MoveTowards(speed, inputDirection * maxSpeed, acceleration * Time.deltaTime);
            speed = Vector3.ClampMagnitude(speed, MaxAccelForce);
            _RB.AddForce(new Vector3(speed.x, 0, speed.z), ForceMode.Force);

            Vector3 linVel = new Vector3(_RB.linearVelocity.x, 0, _RB.linearVelocity.z);
            Vector3 clampedVelocity = Vector3.ClampMagnitude(linVel, maxSpeed);
            _RB.linearVelocity = new Vector3(clampedVelocity.x, _RB.linearVelocity.y, clampedVelocity.z);
            Vector3 curVel = new Vector3(_RB.linearVelocity.x, 0, _RB.linearVelocity.z);
            currentVelocity = curVel.magnitude;
        }



        if (currentVelocity < maxSpeed * 0.15 && !isDrifting && !railGrinding.onRail && !railGrinding.justGrinded)
        {
            speedID--;
            if (speedID < 0) speedID = 0;
            maxSpeed = varyingMaxSpeeds[speedID];
        }

        if (justJumped)
        {
            jumpMomentum = _RB.linearVelocity;
            _RB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            goingUp = true;
            if (railGrinding.onRail)
            {
                _RB.linearVelocity = inputDirection * railGrinding.grindSpeed;
                jumpMomentum = _RB.linearVelocity;
                railGrinding.ThrowOffRail();
            }
            justJumped = false;
            isAirborne = true;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputDirection = Vector3.zero;

        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        inputDirection = camForward * context.ReadValue<Vector2>().y + camRight * context.ReadValue<Vector2>().x;

        if (inputDirection != Vector3.zero)
            isMoving = true;
        else
            isMoving = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        holdJump = context.ReadValueAsButton();
        if (!isAirborne && context.ReadValueAsButton())
            justJumped = true;
    }

    void GroundCheck()
    {
        if (goingUp) return;

        Collider[] colliders;
        spherePosition = transform.position + checkOffset;

        colliders = Physics.OverlapSphere(spherePosition, sphereRadius, groundLayers);
        if (colliders.Length == 0)
            isAirborne = true;
        if (colliders.Length > 0)
        {
            foreach (Collider col  in colliders)
            {
                Vector3 colliderPos = col.gameObject.transform.position;
                groundAngle = Vector3.Angle(spherePosition, colliderPos);

                if (groundAngle > 180 + normalThreshold || groundAngle < 180 - normalThreshold)
                {
                    if (jumpMomentum != Vector3.zero)
                    {
                        _RB.linearVelocity = jumpMomentum;
                        jumpMomentum = Vector3.zero;
                    }
                    isAirborne = false;
                }
            }
        }
    }

    public void DriftHold(InputAction.CallbackContext context)
    {
        if (!driftOnCooldown && currentVelocity > 0 && !railGrinding.onRail)
        {
            driftHold = true;
            storedVelocity = currentVelocity;
            isDrifting = true;
            _RB.linearVelocity = Vector3.zero;
        }
    }

    public static Action OnDriftBoost;
    public void DriftUp(InputAction.CallbackContext context)
    {
        if (inputDirection != Vector3.zero && driftHold)
        {
            _RB.AddForce(inputDirection * (storedVelocity * driftMultiplier), ForceMode.VelocityChange);

            storedVelocity = 0;
            driftOnCooldown = true;
            OnDriftBoost?.Invoke();
            isDrifting = false;
            Invoke("EndDriftCooldown", driftCooldownDuration);
        }
        driftHold = false;
        storedVelocity = 0;
    }

    private void EndDriftCooldown()
    {
        driftOnCooldown = false;
    }
}

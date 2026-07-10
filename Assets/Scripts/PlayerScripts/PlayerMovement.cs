using UnityEngine.InputSystem;
using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _RB;
    [SerializeField] private Transform mainCam;
    [SerializeField] private PlayerInput playerInput;
    private InputAction drift;

    [Header("Locomotion")]
    [SerializeField] private Vector3 inputDirection;
    [SerializeField] private Vector3 speed;
    [SerializeField] private float currentVelocity;
    [SerializeField] private float maxSpeed = 3.5f;
    [SerializeField] private float acceleration = 200;
    [SerializeField] private float MaxAccelForce = 150;
    [SerializeField] private float rotSpeed = 150;

    [Header("Drifting")]
    [SerializeField] private float storedVelocity;
    [SerializeField] private bool driftOnCooldown = false;
    [SerializeField] private float driftCooldownDuration = 2;
    [SerializeField] private float driftMultiplier = 1.5f;

    [Header("Jumping")]
    [SerializeField] private bool isAirborne = false;
    [SerializeField] private float jumpForce = 2;
    [SerializeField] private RaycastHit downHit;
    [SerializeField] private float groundOffset;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpMultiplier;

    [Header("Active Inputs")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isDrifting = false;
    [SerializeField] private bool justJumped = false;
    [SerializeField] private bool holdJump = false;

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
    }

    void Start()
    {
        drift = playerInput.actions["Drift"];
        drift.performed += DriftHold;
        drift.canceled += DriftUp;
    }

    void Update()
    {
        if (isMoving || isDrifting)
        {
            Quaternion toRotation = Quaternion.LookRotation(inputDirection * 10, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotSpeed * Time.deltaTime);
        }

        if (isAirborne)
        {
            if (_RB.linearVelocity.y < 0)
                _RB.linearVelocity += Vector3.up * Physics.gravity.y * fallMultiplier * Time.deltaTime;
            else if (_RB.linearVelocity.y > 0 && !holdJump)
                _RB.linearVelocity += Vector3.up * Physics.gravity.y * lowJumpMultiplier * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        currentVelocity = 0;

        if (isMoving && !isDrifting)
        {
            speed = Vector3.MoveTowards(speed, inputDirection * maxSpeed, acceleration * Time.deltaTime);
            speed = Vector3.ClampMagnitude(speed, MaxAccelForce);
            _RB.AddForce(new Vector3(speed.x, 0, speed.z), ForceMode.Force);

            _RB.linearVelocity = Vector3.ClampMagnitude(_RB.linearVelocity, maxSpeed);
            Vector3 linVel = new Vector3(_RB.linearVelocity.x, 0, _RB.linearVelocity.z);
            currentVelocity = linVel.magnitude;
        }

        if (justJumped)
        {
            _RB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            justJumped = false;
            isAirborne = true;
        }

        // Check if airborn
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0), Vector3.down, out downHit))
        {
            Debug.DrawLine(transform.position + new Vector3(0, 0.1f, 0), downHit.point, Color.red);
            groundOffset = downHit.distance;
            if (groundOffset > 0.2f)
                isAirborne = true;
            else
                isAirborne = false;
        }
        else
            isAirborne = true;
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

    public void DriftHold(InputAction.CallbackContext context)
    {
        if (!driftOnCooldown && currentVelocity > 0)
        {
            storedVelocity = currentVelocity;
            isDrifting = true;
            _RB.linearVelocity = Vector3.zero;
        }
    }

    public static Action OnDriftBoost;

    public void DriftUp(InputAction.CallbackContext context)
    {
        if (inputDirection != Vector3.zero)
        {
            _RB.AddForce(inputDirection * (storedVelocity * driftMultiplier), ForceMode.Impulse);

            storedVelocity = 0;
            driftOnCooldown = true;
            OnDriftBoost?.Invoke();
            Invoke("EndDriftAcceleration", 1.5f);
            Invoke("EndDriftCooldown", driftCooldownDuration);
        }

        storedVelocity = 0;
    }

    private void EndDriftAcceleration()
    {
        isDrifting = false;
    }

    private void EndDriftCooldown()
    {
        driftOnCooldown = false;
    }
}

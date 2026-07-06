using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, Controls.IMovementActions
{
    [SerializeField] private Rigidbody _RB;

    [SerializeField] private Vector3 input;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float maxSpeed = 3.5f;
    [SerializeField] private float acceleration = 200;
    [SerializeField] private float MaxAccelForce = 150;
    [SerializeField] private float drag = 0.09f;

    [SerializeField] private bool isMoving = false;

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            velocity = Vector3.MoveTowards(velocity, input * maxSpeed, acceleration * Time.deltaTime);
            _RB.AddForce(velocity);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        input = Vector3.zero;
        input = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);

        if (input != Vector3.zero)
            isMoving = true;
        else
            isMoving = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {

    }
}

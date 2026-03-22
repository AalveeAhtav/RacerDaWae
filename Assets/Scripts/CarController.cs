using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 1500f;   // How strongly the car accelerates
    public float maxSpeed = 50f;         // Maximum allowed speed
    public float steering = 120f;         // Steering force
    public float drag = 0.1f;            // Custom drag (since velocity is now linearVelocity in Unity 6)

    private Rigidbody rb;                 // Reference to the car's Rigidbody
    private float inputVertical;          // W/S input (forward/backward)
    private float inputHorizontal;        // A/D input (left/right)

    void Awake()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Lower center of mass = more stable car
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        // Read player input every frame
        // W/S or Up/Down = Vertical axis
        inputVertical = Input.GetAxis("Vertical");

        // A/D or Left/Right = Horizontal axis
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        // Current speed using Unity 6 physics
        float speed = rb.linearVelocity.magnitude;

        // ------------------------------
        // 1. ACCELERATION
        // ------------------------------
        if (speed < maxSpeed)
        {
            // Create a forward force based on input
            Vector3 force = transform.forward * inputVertical * acceleration;

            // Apply acceleration force
            rb.AddForce(force * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        // ------------------------------
        // 2. STEERING
        // ------------------------------
        // At high speeds, reduce steering to avoid instant 180° turns
        float speedFactor = Mathf.Clamp01(speed / maxSpeed);

        // How much we steer this frame
        float steerAmount = inputHorizontal * steering * speedFactor;

        // Apply rotation smoothly
        Quaternion turnOffset = Quaternion.Euler(0f, steerAmount * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnOffset);

        // ------------------------------
        // 3. CUSTOM DRAG (linearVelocity version)
        // ------------------------------
        // Unity 6 removed rb.velocity; now we must fully reassign rb.linearVelocity
        Vector3 slowed = rb.linearVelocity * (1f - drag * Time.fixedDeltaTime);

        // Apply the slowed velocity
        rb.linearVelocity = slowed;
    }
}

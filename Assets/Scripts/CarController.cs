using UnityEngine;

// Makes sure this GameObject always has a Rigidbody attached
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float acceleration = 35f;          // Forward acceleration strength
    public float reverseAcceleration = 18f;   // Reverse acceleration strength
    public float maxSpeed = 30f;              // Maximum speed the car can reach
    public float steering = 80f;              // Base steering sensitivity

    [Header("Grip / Handling")]
    public float forwardDrag = 0.995f;        // Small slowdown when coasting forward
    public float lateralGrip = 0.85f;         // Controls how much sideways sliding is removed
    public float angularGrip = 0.92f;         // Reduces spinning/sliding during turns
    public float downforce = 25f;             // Pushes the car down at higher speed for stability

    [Header("Braking")]
    public float brakeStrength = 0.9f;        // Stronger slowdown when braking against movement

    private Rigidbody rb;                     // Reference to the car's Rigidbody
    private float inputVertical;              // W/S or Up/Down input
    private float inputHorizontal;            // A/D or Left/Right input

    void Awake()
    {
        // Get the Rigidbody component attached to this car
        rb = GetComponent<Rigidbody>();

        // Lower the center of mass so the car feels more stable and less likely to tip/slide
        rb.centerOfMass = new Vector3(0f, -0.6f, 0f);

        // Smooths physics-based movement visually
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Helps with collision accuracy at higher speeds
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Keep Unity's built-in damping low because we are controlling movement manually
        rb.linearDamping = 0f;
        rb.angularDamping = 0.5f;
    }

    void Update()
    {
        // Read player movement input every frame
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        // Get current speed from the Rigidbody
        float speed = rb.linearVelocity.magnitude;

        // Handle forward/reverse movement
        ApplyAcceleration(speed);

        // Handle turning
        ApplySteering(speed);

        // Reduce sliding and spinning
        ApplyGrip();

        // Push the car down more as it goes faster
        ApplyDownforce(speed);
    }

    void ApplyAcceleration(float speed)
    {
        // If player is pressing forward and we are below max speed, accelerate forward
        if (inputVertical > 0f && speed < maxSpeed)
        {
            rb.AddForce(transform.forward * inputVertical * acceleration, ForceMode.Acceleration);
        }
        // If player is pressing backward, apply reverse force
        else if (inputVertical < 0f)
        {
            rb.AddForce(transform.forward * inputVertical * reverseAcceleration, ForceMode.Acceleration);
        }
        else
        {
            // If no throttle is being pressed, apply a little rolling resistance
            // Convert world velocity into local car space
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

            // Slightly reduce forward/backward speed
            localVel.z *= forwardDrag;

            // Convert it back to world space and apply it
            rb.linearVelocity = transform.TransformDirection(localVel);
        }

        // If player is pressing backward while still moving forward,
        // apply extra braking to make stopping feel more natural
        if (inputVertical < 0f && Vector3.Dot(rb.linearVelocity, transform.forward) > 0f)
        {
            rb.linearVelocity *= brakeStrength;
        }
    }

    void ApplySteering(float speed)
    {
        // Calculate speed percentage relative to max speed
        float speedPercent = Mathf.Clamp01(speed / maxSpeed);

        // Reduce steering at higher speeds to prevent oversteering/sliding
        float steerStrength = Mathf.Lerp(1f, 0.35f, speedPercent);

        // Only allow proper steering if the car is actually moving a little
        if (speed > 0.5f)
        {
            // Calculate turn amount for this physics step
            float turnAmount = inputHorizontal * steering * steerStrength * Time.fixedDeltaTime;

            // Create a small rotation around the Y axis
            Quaternion turnOffset = Quaternion.Euler(0f, turnAmount, 0f);

            // Apply the rotation smoothly using the Rigidbody
            rb.MoveRotation(rb.rotation * turnOffset);
        }
    }

    void ApplyGrip()
    {
        // Convert the car's velocity into local space
        // localVel.x = sideways velocity
        // localVel.z = forward velocity
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        // Reduce sideways sliding to make the car feel more planted
        localVel.x *= lateralGrip;

        // Convert velocity back to world space and apply it
        rb.linearVelocity = transform.TransformDirection(localVel);

        // Reduce uncontrolled spinning around the Y axis
        rb.angularVelocity = new Vector3(
            rb.angularVelocity.x,
            rb.angularVelocity.y * angularGrip,
            rb.angularVelocity.z
        );
    }

    void ApplyDownforce(float speed)
    {
        // Push the car downward based on current speed
        // This helps the car stay grounded and feel more stable at higher speeds
        rb.AddForce(-transform.up * downforce * speed, ForceMode.Acceleration);
    }
}
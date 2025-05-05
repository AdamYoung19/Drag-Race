using UnityEngine;

// This script automatically accelerates a Rigidbody2D horizontally (using its local Y-axis as 'forward')
// for a set duration, then applies brakes until it stops.
// It now waits for the 'StartDriving' method to be called before beginning acceleration.
// Attach this to a GameObject with a Rigidbody2D and a Collider2D.
// IMPORTANT: Ensure your car sprite is oriented so that its 'up' direction is the driving direction.
[RequireComponent(typeof(Rigidbody2D))]
public class AutoDrive2D : MonoBehaviour
{
    // --- Public Variables (Adjustable in Inspector) ---
    [Header("Driving Parameters")]
    [Tooltip("The magnitude of the force applied during acceleration (along local Y-axis).")]
    public float accelerationForce = 75f;
    [Tooltip("The magnitude of the force applied during braking (along local Y-axis).")]
    public float brakingForce = 100f;
    [Tooltip("How long the car should accelerate before braking (in seconds).")]
    public float accelerationDuration = 5.0f;

    // --- Public Properties ---
    public bool IsFinished { get; private set; } = false; // Flag to check if this car has finished braking

    // --- Private Variables ---
    private Rigidbody2D rb2d;
    private float timer = 0f;
    private bool isAccelerating = false; // Start as false, wait for signal
    private bool isBraking = false;     // State for braking phase
    private bool canDrive = false;      // Flag controlled by RaceManager

    // --- Unity Methods ---
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            Debug.LogError("AutoDrive2D script requires a Rigidbody2D component.", this);
            enabled = false;
            return;
        }
        // Optional: Freeze rotation if needed
        // rb2d.freezeRotation = true;
        // Optional: Set gravity scale (e.g., 0 for top-down)
        // rb2d.gravityScale = 0f;
    }

    void Start()
    {
        // Reset state at the start (important for potential race restarts)
        ResetState();
    }

    void FixedUpdate()
    {
        // Only perform physics updates if allowed to drive and not yet finished
        if (!canDrive || IsFinished || rb2d == null)
        {
            return;
        }

        // --- State Machine: Accelerate or Brake ---
        if (isAccelerating)
        {
            timer += Time.fixedDeltaTime;
            rb2d.AddForce(transform.up * accelerationForce, ForceMode2D.Force);

            if (timer >= accelerationDuration)
            {
                isAccelerating = false;
                isBraking = true; // Switch to braking state
                Debug.Log($"{gameObject.name}: Acceleration finished. Braking.");
            }
        }
        else if (isBraking)
        {
            // Check if still moving significantly in the 'forward' (local Y) direction
            // We use Vector2.Dot to check velocity component along transform.up
            float forwardSpeed = Vector2.Dot(rb2d.linearVelocity, transform.up);

            if (forwardSpeed > 0.1f) // Check if speed along local up is greater than threshold
            {
                // Apply braking force opposite to the local up direction
                rb2d.AddForce(-transform.up * brakingForce, ForceMode2D.Force);
            }
            else
            {
                // Stop Condition
                rb2d.linearVelocity = Vector2.zero; // Stop all movement
                rb2d.angularVelocity = 0f;
                isBraking = false;
                IsFinished = true; // Mark this car as finished braking
                canDrive = false; // Stop driving logic for this car
                Debug.Log($"{gameObject.name}: Stopped.");
            }
        }
    }

    // --- Public Methods ---

    // Called by RaceManager to allow the car to start accelerating
    public void StartDriving()
    {
        if (rb2d != null && !IsFinished) // Only start if not already finished
        {
            canDrive = true;
            isAccelerating = true; // Begin acceleration phase
            isBraking = false;
            timer = 0f; // Reset timer when starting
            Debug.Log($"{gameObject.name}: Starting to drive.");
        }
    }

    // Resets the car's state for a new race
    public void ResetState()
    {
        canDrive = false;
        isAccelerating = false;
        isBraking = false;
        IsFinished = false;
        timer = 0f;
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
        // Consider resetting position/rotation here if needed
        // transform.position = initialPosition;
        // transform.rotation = initialRotation;
    }


    // --- Gizmos for Visualization (Optional) ---
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1);

        if (rb2d != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rb2d.linearVelocity);
        }
    }
}

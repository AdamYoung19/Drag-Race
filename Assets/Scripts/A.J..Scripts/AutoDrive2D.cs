using UnityEngine;

// This script automatically accelerates a Rigidbody2D using stats defined in a CarData asset.
// It accelerates along its local Y-axis ('forward') until 'CrossedFinishLine' is called, then brakes.
// The CarData must be assigned externally (e.g., by a spawner/manager).
[RequireComponent(typeof(Rigidbody2D))]
public class AutoDrive2D : MonoBehaviour
{
    // --- Public Properties ---
    public bool IsFinished { get; private set; } = false; // Flag to check if this car has finished braking
    public CarData AssignedCarData { get; private set; } // Property to hold the assigned car data

    // --- Private Variables ---
    private Rigidbody2D rb2d;
    private bool isAccelerating = false;
    private bool isBraking = false;
    private bool canDrive = false;

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
        rb2d.gravityScale = 0f; // Ensure gravity is off
    }

    void Start()
    {
        // Reset state, but CarData should be assigned before StartDriving is called.
        ResetState();
        if (AssignedCarData == null)
        {
            // Warn if data hasn't been assigned by the time Start runs.
            // It MUST be assigned before StartDriving() is called.
            Debug.LogWarning($"CarData has not been assigned to {gameObject.name} yet!", this);
        }
    }

    void FixedUpdate()
    {
        // Only perform physics updates if allowed to drive, not finished, and CarData is assigned
        if (!canDrive || IsFinished || rb2d == null || AssignedCarData == null)
        {
            return;
        }

        // --- State Machine: Accelerate or Brake ---
        if (isAccelerating)
        {
            // Use acceleration force from the assigned CarData
            rb2d.AddForce(transform.up * AssignedCarData.accelerationForce, ForceMode2D.Force);
        }
        else if (isBraking)
        {
            float forwardSpeed = Vector2.Dot(rb2d.linearVelocity, transform.up);
            if (forwardSpeed > 0.1f)
            {
                // Use braking force from the assigned CarData
                rb2d.AddForce(-transform.up * AssignedCarData.brakingForce, ForceMode2D.Force);
            }
            else
            {
                // Stop Condition
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                isBraking = false;
                IsFinished = true;
                canDrive = false;
                Debug.Log($"{gameObject.name}: Stopped after braking.");
            }
        }
    }

    // --- Public Methods ---

    // Method to assign the CarData to this script instance
    public void Initialize(CarData data)
    {
        if (data == null)
        {
            Debug.LogError($"Attempted to initialize {gameObject.name} with null CarData!", this);
            enabled = false;
            return;
        }
        AssignedCarData = data;
        Debug.Log($"{gameObject.name} initialized with CarData: {data.carName}");

        // Optional: Apply sprite from CarData if this prefab is generic
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && data.carSprite != null)
        {
            spriteRenderer.sprite = data.carSprite;
        }
        else if (spriteRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on {gameObject.name} to apply sprite from CarData.", this);
        }
        else if (data.carSprite == null)
        {
            Debug.LogWarning($"CarData '{data.carName}' does not have a sprite assigned.", data);
        }
    }


    // Called by RaceManager to allow the car to start accelerating
    public void StartDriving()
    {
        // Ensure CarData is assigned before allowing driving
        if (AssignedCarData == null)
        {
            Debug.LogError($"Cannot start driving {gameObject.name}, CarData is not assigned!", this);
            return;
        }

        if (rb2d != null && !IsFinished)
        {
            canDrive = true;
            isAccelerating = true;
            isBraking = false;
            Debug.Log($"{gameObject.name} ({AssignedCarData.carName}): Starting to drive.");
        }
    }

    // Called by FinishLine script when this car crosses it
    public void CrossedFinishLine()
    {
        if (isAccelerating)
        {
            isAccelerating = false;
            isBraking = true;
            Debug.Log($"{gameObject.name} ({AssignedCarData.carName}): Crossed finish line. Braking.");
        }
    }


    // Resets the car's state for a new race
    public void ResetState()
    {
        canDrive = false;
        isAccelerating = false;
        isBraking = false;
        IsFinished = false;
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
        // Note: CarData is NOT reset here, it persists once assigned.
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

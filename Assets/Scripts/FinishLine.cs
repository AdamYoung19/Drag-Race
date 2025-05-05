using UnityEngine;

// Attach this script to the Finish Line GameObject.
// The Finish Line GameObject must have a Collider2D component with 'Is Trigger' enabled.
[RequireComponent(typeof(Collider2D))]
public class FinishLine : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the RaceManager GameObject here.")]
    public RaceManager raceManager; // Assign your RaceManager object

    [Header("Settings")]
    [Tooltip("The tag assigned to the car GameObjects that should trigger the finish line.")]
    public string carTag = "PlayerCar"; // IMPORTANT: Make sure your cars have this tag!

    private bool triggered = false; // Prevent multiple triggers from the same race

    void Start()
    {
        // Basic checks
        if (raceManager == null)
        {
            Debug.LogError("RaceManager is not assigned in the FinishLine script.", this);
            enabled = false;
            return;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("Collider2D on FinishLine should have 'Is Trigger' enabled.", this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the race hasn't already been finished and if the colliding object has the correct tag
        if (!triggered && other.CompareTag(carTag))
        {
            // Notify the RaceManager that a car has finished
            if (raceManager != null)
            {
                triggered = true; // Mark as triggered for this race
                raceManager.CarFinished(other.gameObject); // Pass the winning car's GameObject
            }
            else
            {
                Debug.LogError("FinishLine triggered, but RaceManager reference is missing!", this);
            }
        }
    }

    // Optional: Add a method to reset the trigger if you plan race restarts
    public void ResetTrigger()
    {
        triggered = false;
    }

    // Draw a gizmo line for easy visualization in the Scene view
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            // Draw a line across the bounds of the collider
            Gizmos.DrawLine(new Vector2(col.bounds.min.x, transform.position.y), new Vector2(col.bounds.max.x, transform.position.y));
        }
    }
}

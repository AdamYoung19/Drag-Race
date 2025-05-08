using UnityEngine;
using System.Collections.Generic; // Required for List

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

    private List<GameObject> finishedCars = new List<GameObject>(); // Keep track of cars that already finished to prevent multiple triggers per car

    void Start()
    {
        // Basic checks
        if (raceManager == null)
        {
            // Log error if RaceManager isn't assigned IN THE INSPECTOR
            Debug.LogError("FinishLine Start: RaceManager is not assigned in the Inspector!", this);
            // We don't disable here, as it might be assigned later, but it's a problem.
        }
        else
        {
            Debug.Log("FinishLine Start: RaceManager reference is assigned.", this);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("FinishLine Start: FinishLine script requires a Collider2D component.", this);
            enabled = false;
            return;
        }
        if (!col.isTrigger)
        {
            // Log warning if the collider isn't set to trigger
            Debug.LogWarning("FinishLine Start: Collider2D on FinishLine should have 'Is Trigger' enabled.", this);
        }
        else
        {
            Debug.Log("FinishLine Start: Collider2D is correctly set as a Trigger.", this);
        }

        // Clear list on start (for potential restarts)
        ResetTrigger(); // Call ResetTrigger which clears the list
        Debug.Log("FinishLine Start: Trigger list cleared.", this);
    }

    // --- OnTriggerEnter2D ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // --- DETAILED DEBUG LOGS ---
        Debug.Log($"FinishLine OnTriggerEnter2D: Triggered by GameObject '{other.gameObject.name}' with tag '{other.tag}'.");

        // 1. Check if RaceManager reference is valid *at the time of trigger*
        if (raceManager == null)
        {
            Debug.LogError("FinishLine OnTriggerEnter2D: RaceManager reference is NULL! Cannot report finish.", this);
            return; // Stop processing if manager is missing
        }

        // 2. Check the tag comparison
        bool tagMatch = other.CompareTag(carTag);
        Debug.Log($"FinishLine OnTriggerEnter2D: Comparing tag '{other.tag}' with expected tag '{carTag}'. Match: {tagMatch}");

        if (!tagMatch)
        {
            Debug.LogWarning($"FinishLine OnTriggerEnter2D: Tag mismatch for '{other.gameObject.name}'. Ignoring.", this);
            return; // Stop if tag doesn't match
        }

        // 3. Check if the car is already in the finished list
        bool alreadyFinished = finishedCars.Contains(other.gameObject);
        Debug.Log($"FinishLine OnTriggerEnter2D: Checking if '{other.gameObject.name}' is already in finishedCars list. Already finished: {alreadyFinished}");

        if (alreadyFinished)
        {
            Debug.Log($"FinishLine OnTriggerEnter2D: Car '{other.gameObject.name}' has already finished. Ignoring duplicate trigger.", this);
            return; // Stop if already processed this car
        }

        // --- If all checks pass, proceed ---
        Debug.Log($"FinishLine OnTriggerEnter2D: All checks passed for '{other.gameObject.name}'. Processing finish...");

        GameObject carObject = other.gameObject;
        finishedCars.Add(carObject); // Add car to the finished list
        Debug.Log($"FinishLine OnTriggerEnter2D: Added '{carObject.name}' to finishedCars list. List count: {finishedCars.Count}");


        // --- Tell the RaceManager (only if it's the FIRST car to trigger THIS SCRIPT INSTANCE) ---
        // Note: RaceManager now handles determining the actual winner based on time.
        // We just need to notify it that *a* car finished.
        Debug.Log($"FinishLine OnTriggerEnter2D: Attempting to call raceManager.CarFinished for '{carObject.name}'...");
        raceManager.CarFinished(carObject); // Pass the finishing car's GameObject
        Debug.Log($"FinishLine OnTriggerEnter2D: Called raceManager.CarFinished for '{carObject.name}'.");


        // --- Tell the specific car to start braking ---
        AutoDrive2D carScript = carObject.GetComponent<AutoDrive2D>();
        if (carScript != null)
        {
            Debug.Log($"FinishLine OnTriggerEnter2D: Attempting to call carScript.CrossedFinishLine for '{carObject.name}'...");
            carScript.CrossedFinishLine(); // Call the method to initiate braking
            Debug.Log($"FinishLine OnTriggerEnter2D: Called carScript.CrossedFinishLine for '{carObject.name}'.");
        }
        else
        {
            // Log warning if the car script is missing
            Debug.LogWarning($"FinishLine OnTriggerEnter2D: Car '{carObject.name}' crossed finish line but doesn't have AutoDrive2D script?", carObject);
        }
    }

    // Optional: Add a method to reset the trigger list if you plan race restarts
    // Call this from RaceManager if you add a Reset Race button/functionality
    public void ResetTrigger()
    {
        finishedCars.Clear(); // Clear the list of finished cars
    }

    // Draw a gizmo line for easy visualization in the Scene view
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            // Draw a line across the bounds of the collider
            // Use collider bounds for better accuracy if the finish line isn't centered
            Gizmos.DrawLine(new Vector2(col.bounds.min.x, col.bounds.center.y),
                            new Vector2(col.bounds.max.x, col.bounds.center.y));
        }
    }
}

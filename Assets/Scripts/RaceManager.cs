using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Required for TextMeshPro

public class RaceManager : MonoBehaviour
{
    [Header("Race Setup")]
    [Tooltip("List of all cars participating in the race.")]
    public List<AutoDrive2D> cars; // Assign your car GameObjects (with AutoDrive2D script) here

    [Tooltip("Time in seconds for the countdown before the race starts.")]
    public float countdownTime = 3.0f;

    [Header("UI")]
    [Tooltip("Assign the TextMeshPro UI element for displaying countdown and results.")]
    public TextMeshProUGUI statusText; // Assign your TextMeshProUGUI component here

    [Header("Finish Line")]
    [Tooltip("Assign the Finish Line GameObject here.")]
    public GameObject finishLine; // Assign the finish line object

    // --- Private Variables ---
    private bool raceStarted = false;
    private bool raceFinished = false;
    private string winnerName = "";

    // --- Unity Methods ---
    void Start()
    {
        // Initial setup
        if (statusText == null)
        {
            Debug.LogError("Status Text (TextMeshProUGUI) is not assigned in the RaceManager.", this);
            enabled = false;
            return;
        }
        if (cars == null || cars.Count == 0)
        {
            Debug.LogError("No cars assigned in the RaceManager.", this);
            enabled = false;
            return;
        }
        if (finishLine == null)
        {
            Debug.LogError("Finish Line GameObject is not assigned in the RaceManager.", this);
            enabled = false;
            return;
        }

        // Ensure finish line trigger is active (optional check)
        Collider2D finishCollider = finishLine.GetComponent<Collider2D>();
        if (finishCollider == null || !finishCollider.isTrigger)
        {
            Debug.LogWarning("Finish Line GameObject should have a Collider2D component with 'Is Trigger' enabled.", finishLine);
        }

        // Reset cars and start the countdown
        ResetRace();
        StartCoroutine(CountdownCoroutine());
    }

    void Update()
    {
        // You could add logic here to check if all cars have finished braking after the race, etc.
    }

    // --- Coroutines ---

    private IEnumerator CountdownCoroutine()
    {
        // Countdown Phase
        float timer = countdownTime;
        while (timer > 0)
        {
            statusText.text = Mathf.CeilToInt(timer).ToString(); // Display whole numbers
            yield return new WaitForSeconds(1.0f);
            timer -= 1.0f;
        }

        // Start Race Phase
        statusText.text = "GO!";
        raceStarted = true;
        StartRace();

        // Keep "GO!" message for a short duration
        yield return new WaitForSeconds(1.0f);

        // Clear status text or display race time, etc.
        if (!raceFinished) // Only clear if race isn't already over
        {
            statusText.text = ""; // Or maybe "Race in Progress..."
        }
    }

    // --- Public Methods ---

    // Called by the FinishLine script when a car crosses
    public void CarFinished(GameObject car)
    {
        if (!raceFinished && raceStarted) // Only declare winner once and after race starts
        {
            raceFinished = true;
            winnerName = car.name; // Get the name of the winning car GameObject
            statusText.text = $"{winnerName} Wins!";
            Debug.Log($"{winnerName} crossed the finish line!");

            // Optional: Stop other cars immediately
            // StopAllCars();
        }
    }

    // --- Private Methods ---

    private void StartRace()
    {
        foreach (AutoDrive2D car in cars)
        {
            if (car != null)
            {
                car.StartDriving();
            }
        }
    }

    private void StopAllCars()
    {
        foreach (AutoDrive2D car in cars)
        {
            if (car != null)
            {
                // You might want a specific Stop method in AutoDrive2D
                // or just disable its driving capability
                car.ResetState(); // Using ResetState will stop it
            }
        }
    }

    private void ResetRace()
    {
        raceStarted = false;
        raceFinished = false;
        winnerName = "";
        statusText.text = "Ready..."; // Initial message

        foreach (AutoDrive2D car in cars)
        {
            if (car != null)
            {
                car.ResetState();
            }
        }
    }
}

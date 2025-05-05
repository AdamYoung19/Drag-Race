using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ queries (like FirstOrDefault)
using TMPro; // Required for TextMeshPro
using Unity.Cinemachine; // Required for CinemachineVirtualCamera

public class RaceManager : MonoBehaviour
{
    [Header("Race Setup")]
    [Tooltip("List of ALL possible CarData assets that can be selected.")]
    public List<CarData> allPossibleCars; // Assign ALL your CarData assets here

    [Tooltip("Transform defining where the player's car should spawn.")]
    public Transform playerSpawnPoint;
    [Tooltip("Transform defining where the opponent's car should spawn.")]
    public Transform opponentSpawnPoint; // Add more if needed

    // Removed opponentCarData field - it will be loaded from PlayerPrefs now

    [Tooltip("Time in seconds for the countdown before the race starts.")]
    public float countdownTime = 3.0f;

    [Header("Camera")] // NEW SECTION
    [Tooltip("Assign the Cinemachine Virtual Camera that should follow the player.")]
    public CinemachineCamera playerVirtualCamera; // Assign your player's VCcam here

    [Header("UI")]
    [Tooltip("Assign the TextMeshPro UI element for displaying countdown, timer, and results.")]
    public TextMeshProUGUI statusText; // Assign your TextMeshProUGUI component here

    [Header("Finish Line")]
    [Tooltip("Assign the Finish Line GameObject here.")]
    public GameObject finishLine; // Assign the finish line object

    // --- Private Variables ---
    private List<AutoDrive2D> raceParticipants = new List<AutoDrive2D>(); // List to hold the spawned cars
    private bool raceStarted = false;
    private bool raceFinished = false;
    private float raceStartTime;
    private Dictionary<string, float> finishTimes = new Dictionary<string, float>();
    private int carsFinishedCount = 0;

    // --- Constants for PlayerPrefs Keys (Defined here for clarity) ---
    // These strings MUST match the ones used in CarSelectionManager
    private const string PlayerCarPrefKey = "SelectedPlayerCarName";
    private const string OpponentCarPrefKey = "SelectedOpponentCarName";


    // --- Unity Methods ---

    void Awake() // Changed from Start to ensure spawning happens early
    {
        // Initial checks
        if (statusText == null) { Debug.LogError("Status Text not assigned.", this); enabled = false; return; }
        if (finishLine == null) { Debug.LogError("Finish Line not assigned.", this); enabled = false; return; }
        if (playerSpawnPoint == null) { Debug.LogError("Player Spawn Point not assigned.", this); enabled = false; return; }
        if (opponentSpawnPoint == null) { Debug.LogError("Opponent Spawn Point not assigned.", this); enabled = false; return; }
        // Removed opponentCarData check - loaded dynamically now
        if (allPossibleCars == null || allPossibleCars.Count == 0) { Debug.LogError("All Possible Cars list is empty.", this); enabled = false; return; }
        if (playerVirtualCamera == null) { Debug.LogError("Player Virtual Camera is not assigned in the RaceManager!", this); enabled = false; return; } // Check for camera

        // Spawn cars before doing anything else
        SpawnCars();
    }

    void Start()
    {
        // Get the FinishLine script component to potentially reset it
        FinishLine finishLineScript = finishLine.GetComponent<FinishLine>();
        if (finishLineScript == null)
        {
            Debug.LogError("Finish Line GameObject is missing the FinishLine script.", this);
            enabled = false;
            return;
        }

        // Ensure finish line trigger is active (optional check)
        Collider2D finishCollider = finishLine.GetComponent<Collider2D>();
        if (finishCollider == null || !finishCollider.isTrigger)
        {
            Debug.LogWarning("Finish Line GameObject should have a Collider2D component with 'Is Trigger' enabled.", finishLine);
        }

        // Reset race state and start the countdown
        ResetRace(); // ResetRace now uses raceParticipants list
        StartCoroutine(CountdownCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        // Rolling Timer Logic
        if (raceStarted && !raceFinished)
        {
            float elapsedTime = Time.time - raceStartTime;
            statusText.text = $"Time: {elapsedTime:F2}s";
        }
    }

    // --- Spawning Logic ---
    private void SpawnCars()
    {
        raceParticipants.Clear(); // Clear any previous participants

        // --- Determine Player Car Data ---
        // Use the locally defined constant string for the PlayerPrefs key
        string selectedPlayerCarDataName = PlayerPrefs.GetString(PlayerCarPrefKey, "");
        CarData playerCarData = FindCarDataByName(selectedPlayerCarDataName, "Player");

        // --- Determine Opponent Car Data ---
        // Use the locally defined constant string for the PlayerPrefs key
        string selectedOpponentCarDataName = PlayerPrefs.GetString(OpponentCarPrefKey, ""); // Use opponent key string
        CarData opponentCarData = FindCarDataByName(selectedOpponentCarDataName, "Opponent"); // Find opponent data

        // --- Spawn Player Car ---
        if (playerCarData != null && playerCarData.carPrefab != null)
        {
            GameObject playerCarInstance = Instantiate(playerCarData.carPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerCarInstance.name = "PlayerCar_" + playerCarData.carName;
            AutoDrive2D playerDriver = playerCarInstance.GetComponent<AutoDrive2D>();
            if (playerDriver != null)
            {
                playerDriver.Initialize(playerCarData);
                raceParticipants.Add(playerDriver);
                Debug.Log($"Spawned Player Car: {playerCarInstance.name} using Data: {playerCarData.name}");

                // *** ASSIGN CAMERA TARGET ***
                if (playerVirtualCamera != null)
                {
                    playerVirtualCamera.Follow = playerCarInstance.transform;
                    // Optional: Also set LookAt if your camera setup requires it
                    // playerVirtualCamera.LookAt = playerCarInstance.transform;
                    Debug.Log($"Assigned {playerCarInstance.name} to Player Virtual Camera Follow target.");
                }
                else
                {
                    Debug.LogError("Cannot assign camera target - Player Virtual Camera reference is missing on RaceManager!");
                }
            }
            else { Debug.LogError($"Spawned player prefab '{playerCarData.carPrefab.name}' is missing AutoDrive2D script!", playerCarData.carPrefab); }
        }
        else { Debug.LogError($"Could not spawn player car. Data: {playerCarData?.name}, Prefab Missing: {playerCarData?.carPrefab == null}", playerCarData); }


        // --- Spawn Opponent Car ---
        if (opponentCarData != null && opponentCarData.carPrefab != null) // Check opponent data too
        {
            GameObject opponentCarInstance = Instantiate(opponentCarData.carPrefab, opponentSpawnPoint.position, opponentSpawnPoint.rotation);
            opponentCarInstance.name = "OpponentCar_" + opponentCarData.carName;
            AutoDrive2D opponentDriver = opponentCarInstance.GetComponent<AutoDrive2D>();
            if (opponentDriver != null)
            {
                opponentDriver.Initialize(opponentCarData);
                raceParticipants.Add(opponentDriver);
                Debug.Log($"Spawned Opponent Car: {opponentCarInstance.name} using Data: {opponentCarData.name}");
            }
            else { Debug.LogError($"Spawned opponent prefab '{opponentCarData.carPrefab.name}' is missing AutoDrive2D script!", opponentCarData.carPrefab); }
        }
        else { Debug.LogError($"Could not spawn opponent car. Data: {opponentCarData?.name}, Prefab Missing: {opponentCarData?.carPrefab == null}", opponentCarData); }

        Debug.Log($"Total race participants spawned: {raceParticipants.Count}");
    }

    // Helper method to find CarData, reducing code duplication
    private CarData FindCarDataByName(string dataName, string carType) // carType is for logging (e.g., "Player", "Opponent")
    {
        CarData foundData = null;
        if (!string.IsNullOrEmpty(dataName))
        {
            foundData = allPossibleCars.FirstOrDefault(car => car.name == dataName);
        }

        if (foundData == null)
        {
            Debug.LogWarning($"Selected {carType} car data '{dataName}' not found or not selected. Using first car as default.");
            if (allPossibleCars.Count > 0)
            {
                foundData = allPossibleCars[0]; // Fallback
            }
            else
            {
                Debug.LogError($"Cannot find fallback {carType} car data - AllPossibleCars list is empty!");
            }
        }
        return foundData;
    }


    // --- Coroutines ---
    private IEnumerator CountdownCoroutine()
    {
        // Countdown Phase
        float timer = countdownTime;
        while (timer > 0)
        {
            statusText.text = Mathf.CeilToInt(timer).ToString();
            yield return new WaitForSeconds(1.0f);
            timer -= 1.0f;
        }

        // Start Race Phase
        statusText.text = "GO!";
        StartRace();

        yield return new WaitForSeconds(0.25f);
    }

    // --- Public Methods ---
    public void CarFinished(GameObject carObject)
    {
        if (!raceStarted || raceFinished) return;

        AutoDrive2D finishedDriver = carObject.GetComponent<AutoDrive2D>();
        string carName = carObject.name; // Use GameObject name which includes Player/Opponent prefix

        if (finishTimes.ContainsKey(carName)) return;

        float time = Time.time - raceStartTime;
        finishTimes.Add(carName, time);
        carsFinishedCount++;

        Debug.Log($"Car '{carName}' finished! Time: {time:F2}. Total Finished: {carsFinishedCount}/{raceParticipants.Count}");

        UpdateResultsUI();

        if (carsFinishedCount >= raceParticipants.Count)
        {
            Debug.Log($"Race finished check: Condition met! Count: {carsFinishedCount}, Total Participants: {raceParticipants.Count}");
            raceFinished = true;
            DetermineWinner();
        }
    }

    // --- Private Methods ---
    private void StartRace()
    {
        if (raceParticipants.Count == 0)
        {
            Debug.LogError("Cannot start race - no participants were spawned or added!");
            return;
        }

        raceStarted = true;
        raceStartTime = Time.time;
        finishTimes.Clear();
        carsFinishedCount = 0;

        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null)
            {
                car.StartDriving();
            }
        }
        Debug.Log("Race Started!");
    }

    private void UpdateResultsUI()
    {
        string results = "Results:\n";
        foreach (var pair in finishTimes.OrderBy(p => p.Value))
        {
            results += $"{pair.Key}: {pair.Value:F2}s\n";
        }
        if (!raceFinished)
        {
            results += "\nRace in progress...";
        }
        statusText.text = results;
    }

    private void DetermineWinner()
    {
        if (finishTimes.Count == 0) return;

        KeyValuePair<string, float> winner = finishTimes.OrderBy(pair => pair.Value).First();
        Debug.Log($"Winner determined: {winner.Key} with time {winner.Value:F2}");

        string finalResults = "Final Results:\n";
        foreach (var pair in finishTimes.OrderBy(p => p.Value))
        {
            finalResults += $"{pair.Key}: {pair.Value:F2}s\n";
        }
        finalResults += $"\n{winner.Key} Wins!";
        statusText.text = finalResults;
    }

    private void StopAllCars()
    {
        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null && !car.IsFinished)
            {
                car.ResetState();
            }
        }
    }

    private void ResetRace()
    {
        raceStarted = false;
        raceFinished = false;
        finishTimes.Clear();
        carsFinishedCount = 0;
        statusText.text = "Ready...";

        // Reset states of spawned cars
        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null)
            {
                // Reset position/rotation to spawn points AND internal state
                if (car.name.StartsWith("PlayerCar"))
                {
                    car.transform.position = playerSpawnPoint.position;
                    car.transform.rotation = playerSpawnPoint.rotation;
                }
                else if (car.name.StartsWith("OpponentCar"))
                {
                    car.transform.position = opponentSpawnPoint.position;
                    car.transform.rotation = opponentSpawnPoint.rotation;
                }
                car.ResetState();
            }
        }

        // Reset the finish line trigger
        FinishLine finishLineScript = finishLine.GetComponent<FinishLine>();
        if (finishLineScript != null)
        {
            finishLineScript.ResetTrigger();
        }
        else
        {
            Debug.LogError("Could not find FinishLine script on Finish Line object to reset trigger.", finishLine);
        }
        Debug.Log("Race Reset.");
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for LINQ queries (like FirstOrDefault)
using UnityEngine.SceneManagement; // Required for loading scenes
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

    [Tooltip("Time in seconds for the countdown before the race starts.")]
    public float countdownTime = 3.0f;

    [Header("Camera")]
    [Tooltip("Assign the Cinemachine Virtual Camera that should follow the player.")]
    public CinemachineCamera playerVirtualCamera; // Assign your player's VCcam here

    [Header("UI")]
    [Tooltip("Assign the TextMeshPro UI element for displaying countdown, timer, and results.")]
    public TextMeshProUGUI statusText; // Assign your TextMeshProUGUI component here
    [Tooltip("Assign the Button GameObject that returns to Character Selection.")]
    public GameObject returnToSelectionButton; // Renamed for clarity
    [Tooltip("Assign the Button GameObject that restarts the current race.")]
    public GameObject restartRaceButton;
    [Tooltip("Assign the TextMeshPro UI element used for the Player's Skill Check prompts.")]
    public TextMeshProUGUI playerSkillCheckUI; // Reference for Skill Check UI


    [Header("Finish Line")]
    [Tooltip("Assign the Finish Line GameObject here.")]
    public GameObject finishLine; // Assign the finish line object

    [Header("Scene Management")]
    [Tooltip("The name of the character selection scene to load.")]
    public string characterSelectionSceneName = "CarSelection"; // Make sure this matches your selection scene name


    // --- Private Variables ---
    private List<AutoDrive2D> raceParticipants = new List<AutoDrive2D>(); // List to hold the spawned cars
    private bool raceStarted = false;
    private bool raceFinished = false;
    private float raceStartTime;
    private Dictionary<string, float> finishTimes = new Dictionary<string, float>();
    private int carsFinishedCount = 0;
    private Coroutine countdownCoroutine; // To store the active countdown coroutine

    // --- Constants for PlayerPrefs Keys (Defined here for clarity) ---
    // These strings MUST match the ones used in CarSelectionManager
    private const string PlayerCarPrefKey = "SelectedPlayerCarName";
    private const string OpponentCarPrefKey = "SelectedOpponentCarName";


    // --- Unity Methods ---

    void Awake() // Changed from Start to ensure spawning happens early
    {
        // Initial checks for assigned references
        if (statusText == null) { Debug.LogError("Status Text not assigned.", this); enabled = false; return; }
        if (finishLine == null) { Debug.LogError("Finish Line not assigned.", this); enabled = false; return; }
        if (playerSpawnPoint == null) { Debug.LogError("Player Spawn Point not assigned.", this); enabled = false; return; }
        if (opponentSpawnPoint == null) { Debug.LogError("Opponent Spawn Point not assigned.", this); enabled = false; return; }
        if (allPossibleCars == null || allPossibleCars.Count == 0) { Debug.LogError("All Possible Cars list is empty.", this); enabled = false; return; }
        if (playerVirtualCamera == null) { Debug.LogError("Player Virtual Camera is not assigned!", this); enabled = false; return; }
        if (returnToSelectionButton == null) { Debug.LogError("Return To Selection Button is not assigned!", this); enabled = false; return; }
        if (restartRaceButton == null) { Debug.LogError("Restart Race Button is not assigned!", this); enabled = false; return; }
        if (playerSkillCheckUI == null) { Debug.LogError("Player Skill Check UI is not assigned!", this); enabled = false; return; }
        if (string.IsNullOrEmpty(characterSelectionSceneName)) { Debug.LogError("Character Selection Scene Name is not set!", this); enabled = false; return; }


        // Spawn cars before doing anything else
        SpawnCars();
    }

    void Start()
    {
        // Hide buttons and skill check UI initially
        if (returnToSelectionButton != null) returnToSelectionButton.SetActive(false);
        if (restartRaceButton != null) restartRaceButton.SetActive(false);
        if (playerSkillCheckUI != null) playerSkillCheckUI.gameObject.SetActive(false); // Hide skill check UI

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
        StartRaceCountdown(); // Call the new method to start the process
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
        // --- Clean up existing cars ---
        foreach (AutoDrive2D participant in raceParticipants) { if (participant != null) Destroy(participant.gameObject); }
        raceParticipants.Clear();

        // --- Determine Car Data ---
        string selectedPlayerCarDataName = PlayerPrefs.GetString(PlayerCarPrefKey, "");
        CarData playerCarData = FindCarDataByName(selectedPlayerCarDataName, "Player");
        string selectedOpponentCarDataName = PlayerPrefs.GetString(OpponentCarPrefKey, "");
        CarData opponentCarData = FindCarDataByName(selectedOpponentCarDataName, "Opponent");

        // --- Spawn Player Car ---
        if (playerCarData != null && playerCarData.carPrefab != null)
        {
            GameObject playerCarInstance = Instantiate(playerCarData.carPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerCarInstance.name = "Player"; // Set simple name
            AutoDrive2D playerDriver = playerCarInstance.GetComponent<AutoDrive2D>();
            if (playerDriver != null)
            {
                // Pass TRUE for isPlayer and the UI reference
                playerDriver.Initialize(playerCarData, true, playerSkillCheckUI);
                raceParticipants.Add(playerDriver);
                Debug.Log($"Spawned Player Car: {playerCarInstance.name} using Data: {playerCarData.name}");

                // Assign camera target
                if (playerVirtualCamera != null) { playerVirtualCamera.Follow = playerCarInstance.transform; }
                else { Debug.LogError("Cannot assign camera target - Player Virtual Camera reference missing!"); }
            }
            else { Debug.LogError($"Player prefab '{playerCarData.carPrefab.name}' missing AutoDrive2D script!", playerCarData.carPrefab); }
        }
        else { Debug.LogError($"Could not spawn player car. Data: {playerCarData?.name}, Prefab Missing: {playerCarData?.carPrefab == null}", playerCarData); }


        // --- Spawn Opponent Car ---
        if (opponentCarData != null && opponentCarData.carPrefab != null)
        {
            GameObject opponentCarInstance = Instantiate(opponentCarData.carPrefab, opponentSpawnPoint.position, opponentSpawnPoint.rotation);
            opponentCarInstance.name = "Computer"; // Set simple name
            AutoDrive2D opponentDriver = opponentCarInstance.GetComponent<AutoDrive2D>();
            if (opponentDriver != null)
            {
                // Pass FALSE for isPlayer and NULL for UI
                opponentDriver.Initialize(opponentCarData, false, null);
                raceParticipants.Add(opponentDriver);
                Debug.Log($"Spawned Opponent Car: {opponentCarInstance.name} using Data: {opponentCarData.name}");
            }
            else { Debug.LogError($"Opponent prefab '{opponentCarData.carPrefab.name}' missing AutoDrive2D script!", opponentCarData.carPrefab); }
        }
        else { Debug.LogError($"Could not spawn opponent car. Data: {opponentCarData?.name}, Prefab Missing: {opponentCarData?.carPrefab == null}", opponentCarData); }

        Debug.Log($"Total race participants spawned: {raceParticipants.Count}");
    }

    // Helper method to find CarData, reducing code duplication
    private CarData FindCarDataByName(string dataName, string carType)
    {
        CarData foundData = null;
        if (!string.IsNullOrEmpty(dataName))
        {
            foundData = allPossibleCars.FirstOrDefault(car => car.name == dataName);
        }

        if (foundData == null)
        {
            Debug.LogWarning($"Selected {carType} car data '{dataName}' not found or not selected. Using first car as default.");
            if (allPossibleCars.Count > 0) { foundData = allPossibleCars[0]; }
            else { Debug.LogError($"Cannot find fallback {carType} car data - AllPossibleCars list is empty!"); }
        }
        return foundData;
    }


    // --- Coroutines ---
    private IEnumerator CountdownCoroutine()
    {
        // Hide buttons during countdown/race
        if (returnToSelectionButton != null) returnToSelectionButton.SetActive(false);
        if (restartRaceButton != null) restartRaceButton.SetActive(false);
        if (playerSkillCheckUI != null) playerSkillCheckUI.gameObject.SetActive(false); // Ensure hidden

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
        countdownCoroutine = null; // Mark coroutine as finished
    }

    // --- Public Methods ---
    public void CarFinished(GameObject carObject)
    {
        if (!raceStarted || raceFinished) return;

        string carName = carObject.name;
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
            DetermineWinner(); // DetermineWinner will now show the buttons
        }
    }

    // Method called by the "Return to Selection" Button's OnClick event
    public void ReturnToSelection()
    {
        Debug.Log("ReturnToSelection called. Loading scene: " + characterSelectionSceneName);
        // Add any cleanup if needed before loading
        SceneManager.LoadScene(characterSelectionSceneName);
    }

    // Method called by the "Restart Race" Button's OnClick event
    public void RestartCurrentRace()
    {
        Debug.Log("RestartCurrentRace called.");
        // Reset the race state (positions, variables, etc.)
        ResetRace();
        // Start the countdown again
        StartRaceCountdown();
    }


    // --- Private Methods ---
    private void StartRaceCountdown()
    {
        // Stop previous countdown if it was somehow still running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private void StartRace()
    {
        if (raceParticipants.Count == 0) { Debug.LogError("Cannot start race - no participants!"); return; }

        raceStarted = true;
        raceStartTime = Time.time;
        finishTimes.Clear();
        carsFinishedCount = 0;

        // Ensure buttons are hidden at race start
        if (returnToSelectionButton != null) returnToSelectionButton.SetActive(false);
        if (restartRaceButton != null) restartRaceButton.SetActive(false);
        if (playerSkillCheckUI != null) playerSkillCheckUI.gameObject.SetActive(false); // Ensure hidden


        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null) car.StartDriving();
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

        // Determine the winner and loser
        KeyValuePair<string, float> winner = finishTimes.OrderBy(pair => pair.Value).First();
        KeyValuePair<string, float> loser = finishTimes.OrderBy(pair => pair.Value).Last();

        Debug.Log($"Winner determined: {winner.Key} with time {winner.Value:F2}");

        // Save the winner and loser names
        PlayerPrefs.SetString("PodiumWinner", winner.Key);
        PlayerPrefs.SetString("PodiumLoser", loser.Key);

        // Save the sprites of the player and computer
        foreach (AutoDrive2D car in raceParticipants)
        {
            SpriteRenderer spriteRenderer = car.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (car.name == "Player")
                {
                    PlayerPrefs.SetString("PlayerSprite", spriteRenderer.sprite.name);
                    Debug.Log($"Saved Player Sprite: {spriteRenderer.sprite.name}");
                }
                else if (car.name == "Computer")
                {
                    PlayerPrefs.SetString("ComputerSprite", spriteRenderer.sprite.name);
                    Debug.Log($"Saved Computer Sprite: {spriteRenderer.sprite.name}");
                }
            }
            else
            {
                Debug.LogError($"Car '{car.name}' does not have a SpriteRenderer!");
            }
        }

        PlayerPrefs.Save();

        // Load the podium scene
        SceneManager.LoadScene("PodiumScene");
    }

    private void StopAllCars() // Optional: Called if needed
    {
        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null && !car.IsFinished) car.ResetState();
        }
    }

    private void ResetRace()
    {
        raceStarted = false;
        raceFinished = false;
        finishTimes.Clear();
        carsFinishedCount = 0;
        statusText.text = "Ready...";

        // Hide buttons and skill check UI during reset
        if (returnToSelectionButton != null) returnToSelectionButton.SetActive(false);
        if (restartRaceButton != null) restartRaceButton.SetActive(false);
        if (playerSkillCheckUI != null) playerSkillCheckUI.gameObject.SetActive(false);


        // Reset states of spawned cars
        foreach (AutoDrive2D car in raceParticipants)
        {
            if (car != null)
            {
                // Reset position/rotation based on the exact names
                if (car.name == "Player")
                {
                    car.transform.position = playerSpawnPoint.position;
                    car.transform.rotation = playerSpawnPoint.rotation;
                }
                else if (car.name == "Computer")
                {
                    car.transform.position = opponentSpawnPoint.position;
                    car.transform.rotation = opponentSpawnPoint.rotation;
                }
                car.ResetState(); // Reset internal driving state
            }
            // Note: We don't destroy/respawn cars here, just reset their state & position
        }

        // Reset the finish line trigger
        FinishLine finishLineScript = finishLine.GetComponent<FinishLine>();
        if (finishLineScript != null) finishLineScript.ResetTrigger();
        else { Debug.LogError("Could not find FinishLine script on Finish Line object to reset trigger.", finishLine); }

        Debug.Log("Race Reset.");
    }
}

using UnityEngine;
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for List
using TMPro; // Required for TextMeshPro

// This script automatically accelerates a Rigidbody2D using stats defined in a CarData asset.
// It accelerates along its local Y-axis ('forward') until 'CrossedFinishLine' is called, then brakes.
// Includes an optional skill check mechanic for the player car.
[RequireComponent(typeof(Rigidbody2D))]
public class AutoDrive2D : MonoBehaviour
{
    // --- Public Properties ---
    public bool IsFinished { get; private set; } = false; // Flag to check if this car has finished braking
    public CarData AssignedCarData { get; private set; } // Property to hold the assigned car data
    public bool IsPlayerControlled { get; private set; } = false; // Flag set by RaceManager

    // --- Private Variables ---
    private Rigidbody2D rb2d;
    private bool isAccelerating = false;
    private bool isBraking = false;
    private bool canDrive = false;

    // --- Skill Check Variables ---
    [Header("Skill Check (Player Only)")]
    [SerializeField] private float skillCheckIntervalMin = 0.5f; // Minimum time between skill checks
    [SerializeField] private float skillCheckIntervalMax = 2.0f; // Maximum time between skill checks
    [SerializeField] private float skillCheckWindowDuration = 0.25f; // How long the player has to press the key
    [SerializeField] private float boostMultiplier = 1.75f; // Force multiplier on success
    [SerializeField] private float penaltyMultiplier = 0.6f; // Force multiplier on failure
    [SerializeField] private float effectDuration = 1.0f; // How long boost/penalty lasts
    [SerializeField]
    private List<KeyCode> possibleKeys = new List<KeyCode>() { // Keys that can appear
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.D
    };

    private TextMeshProUGUI skillCheckUI; // Reference assigned by RaceManager
    private bool skillCheckActive = false;
    private KeyCode currentTargetKey;
    private float skillCheckTimer = 0f;
    private float currentEffectTimer = 0f;
    private float currentForceMultiplier = 1.0f;
    private float nextSkillCheckTimer = 0f;
    private Coroutine activeSkillCheckCoroutine = null;


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
        ResetState();
        if (IsPlayerControlled)
        {
            // Schedule the first skill check if this is the player
            ScheduleNextSkillCheck();
            if (skillCheckUI == null)
            {
                Debug.LogError($"Skill Check UI not assigned to player car '{gameObject.name}'! Skill checks disabled.", this);
            }
            else
            {
                skillCheckUI.text = ""; // Ensure UI is clear initially
                skillCheckUI.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // --- Skill Check Triggering (Player Only) ---
        if (IsPlayerControlled && canDrive && !skillCheckActive && skillCheckUI != null)
        {
            nextSkillCheckTimer -= Time.deltaTime;
            if (nextSkillCheckTimer <= 0)
            {
                // Only start a new one if the previous isn't running
                if (activeSkillCheckCoroutine == null)
                {
                    activeSkillCheckCoroutine = StartCoroutine(SkillCheckSequence());
                }
            }
        }

        // --- Skill Check Input Detection (Player Only) ---
        if (IsPlayerControlled && skillCheckActive && skillCheckUI != null)
        {
            skillCheckTimer -= Time.deltaTime; // Countdown the reaction timer

            // Check if the correct key was pressed
            if (Input.GetKeyDown(currentTargetKey))
            {
                Debug.Log("Skill Check Success!");
                ApplyBoost();
                EndSkillCheck(true); // End successfully
            }
            // Optional: Detect *any* key press as failure (more strict)
            // else if (Input.anyKeyDown) {
            //     // Check if the pressed key is one of our possible keys but NOT the target
            //     foreach(KeyCode key in possibleKeys) {
            //         if(key != currentTargetKey && Input.GetKeyDown(key)) {
            //              Debug.Log("Skill Check Failed (Wrong Key)!");
            //              ApplyPenalty();
            //              EndSkillCheck(false); // End with failure
            //              break;
            //         }
            //     }
            // }

            // Check if time ran out
            if (skillCheckTimer <= 0)
            {
                Debug.Log("Skill Check Failed (Timeout)!");
                ApplyPenalty();
                EndSkillCheck(false); // End with failure (timeout)
            }
        }

        // --- Effect Timer ---
        if (currentEffectTimer > 0)
        {
            currentEffectTimer -= Time.deltaTime;
            if (currentEffectTimer <= 0)
            {
                currentForceMultiplier = 1.0f; // Reset multiplier when effect wears off
                Debug.Log("Speed effect wore off.");
            }
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
            // Apply force using acceleration from CarData AND the current multiplier
            float forceToApply = AssignedCarData.accelerationForce * currentForceMultiplier;
            rb2d.AddForce(transform.up * forceToApply, ForceMode2D.Force);
        }
        else if (isBraking)
        {
            float forwardSpeed = Vector2.Dot(rb2d.linearVelocity, transform.up);
            if (forwardSpeed > 0.1f)
            {
                // Braking force is usually not affected by boost/penalty, but could be if desired
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
                currentForceMultiplier = 1.0f; // Reset multiplier on stop
                Debug.Log($"{gameObject.name}: Stopped after braking.");
            }
        }
    }

    // --- Public Methods ---

    // Method to assign the CarData AND identify player/UI
    public void Initialize(CarData data, bool isPlayer, TextMeshProUGUI skillUI = null)
    {
        if (data == null) { Debug.LogError($"Attempted to initialize {gameObject.name} with null CarData!", this); enabled = false; return; }

        AssignedCarData = data;
        IsPlayerControlled = isPlayer;
        if (IsPlayerControlled)
        {
            skillCheckUI = skillUI; // Assign UI reference only for player
        }

        Debug.Log($"{gameObject.name} initialized with CarData: {data.carName}. IsPlayer: {IsPlayerControlled}");

        // Apply sprite
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && data.carSprite != null) spriteRenderer.sprite = data.carSprite;
        else if (spriteRenderer == null) Debug.LogWarning($"No SpriteRenderer found on {gameObject.name}.", this);
        else if (data.carSprite == null) Debug.LogWarning($"CarData '{data.carName}' has no sprite.", data);
    }


    // Called by RaceManager to allow the car to start accelerating
    public void StartDriving()
    {
        if (AssignedCarData == null) { Debug.LogError($"Cannot start driving {gameObject.name}, CarData is not assigned!", this); return; }
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
            currentForceMultiplier = 1.0f; // Reset boost/penalty on finish
            if (activeSkillCheckCoroutine != null) EndSkillCheck(false); // Cancel active check on finish
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
        currentForceMultiplier = 1.0f;
        currentEffectTimer = 0f;
        skillCheckActive = false;
        nextSkillCheckTimer = 0f; // Will be rescheduled in Start if player
        if (activeSkillCheckCoroutine != null)
        {
            StopCoroutine(activeSkillCheckCoroutine);
            activeSkillCheckCoroutine = null;
        }
        if (skillCheckUI != null)
        {
            skillCheckUI.text = "";
            skillCheckUI.gameObject.SetActive(false);
        }

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    // --- Skill Check Logic Methods (Player Only) ---

    private void ScheduleNextSkillCheck()
    {
        nextSkillCheckTimer = Random.Range(skillCheckIntervalMin, skillCheckIntervalMax);
        Debug.Log($"Next skill check scheduled in {nextSkillCheckTimer:F1} seconds.");
    }

    private IEnumerator SkillCheckSequence()
    {
        if (!IsPlayerControlled || skillCheckUI == null) yield break; // Should not happen if called correctly

        skillCheckActive = true;
        skillCheckTimer = skillCheckWindowDuration;

        // Select and display random key
        int randomIndex = Random.Range(0, possibleKeys.Count);
        currentTargetKey = possibleKeys[randomIndex];
        skillCheckUI.text = $"Press [{currentTargetKey.ToString()}]!"; // Display the key name
        skillCheckUI.gameObject.SetActive(true);
        Debug.Log($"Skill Check Started! Press [{currentTargetKey.ToString()}] within {skillCheckWindowDuration}s.");

        // The actual check happens in Update(), this coroutine just waits
        // Wait until the check is no longer active (either success, failure, or timeout handled in Update)
        while (skillCheckActive)
        {
            yield return null; // Wait for the next frame
        }

        // Coroutine ends after EndSkillCheck is called from Update() or elsewhere
        activeSkillCheckCoroutine = null; // Allow a new one to be scheduled
    }

    // Called internally on success/failure/timeout
    private void EndSkillCheck(bool success)
    {
        if (!skillCheckActive) return; // Avoid double calls

        skillCheckActive = false;
        if (skillCheckUI != null)
        {
            skillCheckUI.text = success ? "Boost!" : "Penalty!";
            // Optionally keep the result message for a short time
            StartCoroutine(HideSkillCheckUIAfterDelay(0.75f));
        }

        // Stop the waiting coroutine if it's still running (e.g., success/wrong key before timeout)
        if (activeSkillCheckCoroutine != null)
        {
            StopCoroutine(activeSkillCheckCoroutine);
            activeSkillCheckCoroutine = null;
        }

        // Schedule the next check regardless of success/failure
        ScheduleNextSkillCheck();
    }

    private IEnumerator HideSkillCheckUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skillCheckUI != null && !skillCheckActive)
        { // Check active again in case a new one started fast
            skillCheckUI.text = "";
            skillCheckUI.gameObject.SetActive(false);
        }
    }

    private void ApplyBoost()
    {
        currentForceMultiplier = boostMultiplier;
        currentEffectTimer = effectDuration;
        Debug.Log($"Applying boost! Multiplier: {currentForceMultiplier}, Duration: {currentEffectTimer}");
        // Add visual/sound effect here if desired
    }

    private void ApplyPenalty()
    {
        currentForceMultiplier = penaltyMultiplier;
        currentEffectTimer = effectDuration;
        Debug.Log($"Applying penalty! Multiplier: {currentForceMultiplier}, Duration: {currentEffectTimer}");
        // Add visual/sound effect here if desired
    }

    // --- Gizmos ---
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

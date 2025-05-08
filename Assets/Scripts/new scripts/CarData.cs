using UnityEngine;

// This allows you to create Car Data assets directly in the Project window.
// Right-click in Project window -> Create -> RacingGame/Car Data
[CreateAssetMenu(fileName = "NewCarData", menuName = "RacingGame/Car Data")]
public class CarData : ScriptableObject
{
    [Header("Info")]
    public string carName = "Default Car";
    public Sprite carSprite; // Assign the visual representation here
    // Add other info like description if needed

    [Header("Performance Stats")]
    [Tooltip("How quickly the car accelerates.")]
    public float accelerationForce = 75f;
    [Tooltip("How quickly the car brakes.")]
    public float brakingForce = 100f;
    // Add other stats like top speed, handling, etc. if desired

    [Header("Prefab")]
    [Tooltip("The actual car prefab to spawn for this data.")]
    public GameObject carPrefab; // Assign the corresponding car prefab
}

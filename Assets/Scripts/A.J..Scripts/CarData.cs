using UnityEngine;

[CreateAssetMenu(fileName = "CarData", menuName = "RacingGame/Car Data")]
public class CarData : ScriptableObject
{
    public string carName;
    public Sprite carSprite;
    public GameObject carPrefab;

    [Header("Movement")]
    public float accelerationForce = 50f; // Your existing acceleration
    public float maxSpeed = 100f;         // New: Maximum normal speed for the car
    public float brakingForce = 30f;

    [Header("Skill Check")]
    public float skillCheckSpeedBoost = 20f; // New: Specific speed increase from a successful skill check
    public float skillCheckBoostDuration = 1.5f; // New: How long the skill check boost lasts
}
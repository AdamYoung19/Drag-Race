using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Button
using UnityEngine.SceneManagement; // Required for loading scenes
using System.Collections.Generic; // Required for List
using TMPro; // Optional: If using TextMeshPro for labels

public class CarSelectionManager : MonoBehaviour
{
    [Header("Car Data")]
    [Tooltip("List of all available car data assets.")]
    public List<CarData> availableCars;

    [Header("UI Elements (Optional)")]
    [Tooltip("Assign an Image UI element to display the selected car's sprite.")]
    public Image carDisplayImage;
    [Tooltip("Assign a TextMeshProUGUI element to display the selected car's name.")]
    public TextMeshProUGUI carNameText;
    [Tooltip("Assign a TextMeshProUGUI element to display the selected car's stats.")]
    public TextMeshProUGUI carStatsText;

    [Header("Scene Management")]
    [Tooltip("The name of the scene to load after selection.")]
    public string raceSceneName = "RaceScene"; // Make sure this matches your race scene's name

    // --- Private Variables ---
    private int currentCarIndex = 0;
    public const string SelectedCarPrefKey = "SelectedPlayerCarName"; // Key for PlayerPrefs

    // --- Unity Methods ---

    void Start()
    {
        // Basic checks
        if (availableCars == null || availableCars.Count == 0)
        {
            Debug.LogError("No available cars assigned in CarSelectionManager!", this);
            // Optionally disable buttons or show an error message
            return;
        }

        // Initialize display with the first car
        SelectCar(0); // Select the first car by default
    }

    // --- Public Methods (Called by UI Buttons) ---

    public void NextCar()
    {
        currentCarIndex++;
        if (currentCarIndex >= availableCars.Count)
        {
            currentCarIndex = 0; // Wrap around to the first car
        }
        SelectCar(currentCarIndex);
    }

    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = availableCars.Count - 1; // Wrap around to the last car
        }
        SelectCar(currentCarIndex);
    }

    public void ConfirmSelection()
    {
        if (currentCarIndex >= 0 && currentCarIndex < availableCars.Count)
        {
            // Get the selected CarData asset
            CarData selectedData = availableCars[currentCarIndex];

            // IMPORTANT: Save the NAME of the CarData asset (or prefab name) to PlayerPrefs.
            // We use the asset name as a unique identifier. Ensure your CarData assets have unique names!
            PlayerPrefs.SetString(SelectedCarPrefKey, selectedData.name);
            PlayerPrefs.Save(); // Ensure data is written

            Debug.Log($"Selected car '{selectedData.carName}' (Asset: {selectedData.name}) saved.");

            // Load the race scene
            if (!string.IsNullOrEmpty(raceSceneName))
            {
                SceneManager.LoadScene(raceSceneName);
            }
            else
            {
                Debug.LogError("Race Scene Name is not set in CarSelectionManager!", this);
            }
        }
        else
        {
            Debug.LogError("Invalid car index selected!", this);
        }
    }


    // --- Private Methods ---

    private void SelectCar(int index)
    {
        if (index < 0 || index >= availableCars.Count)
        {
            Debug.LogError($"Invalid car index: {index}", this);
            return;
        }

        currentCarIndex = index;
        CarData data = availableCars[currentCarIndex];

        // Update UI elements if they are assigned
        if (carDisplayImage != null && data.carSprite != null)
        {
            carDisplayImage.sprite = data.carSprite;
        }
        if (carNameText != null)
        {
            carNameText.text = data.carName;
        }
        if (carStatsText != null)
        {
            // Format stats display as desired
            carStatsText.text = $"Accel: {data.accelerationForce}\nBrake: {data.brakingForce}";
        }

        Debug.Log($"Displaying car: {data.carName}");
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedControllerUI : MonoBehaviour
{
    // Simple ui controller for speed modifier

    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    public Mech MechController;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSpeedSliderChanged()
    {
        // Update the speed text based on the slider value
        float speedValue = speedSlider.value;
        speedText.text = $"Speed Modifier: {speedValue:F2}x ";
        MechController.SetSpeed(speedValue);
    }
}

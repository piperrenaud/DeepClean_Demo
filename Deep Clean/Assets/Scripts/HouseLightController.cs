using UnityEngine;

public class HouseLightController : MonoBehaviour
{
    [SerializeField] private TimeController timeController;
    [SerializeField] private Light[] houseLights;

    private bool lightsOn = false;
    
    void Update()
    {
        if (timeController == null) return;

        System.DateTime currentTime = timeController.GetCurrentTime();

        bool shouldTurnOn = (currentTime.TimeOfDay >= timeController.GetSunsetTime() ||
                            currentTime.TimeOfDay < timeController.GetSunriseTime());

        if (shouldTurnOn && !lightsOn)
        {
            SetLights(true);
        }
        else if (!shouldTurnOn && lightsOn)
        {
            SetLights(false);
        }
    }

    private void SetLights(bool state)
    {
        foreach (var light in houseLights)
        {
            if (light != null) light.enabled = state;
        }

        lightsOn = true;
    }
}

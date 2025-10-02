using UnityEngine;
using TMPro;
using System;

public class TimeController : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float timeMultiplier;
    [SerializeField] private float startHour;

    [Header("Sun and Moon")]
    [SerializeField] private Light sunLight;
    [SerializeField] private float sunriseHour;
    [SerializeField] private float sunsetHour;
    [SerializeField] private float maxSunlightIntensity;
    [SerializeField] private Light moonLight;
    [SerializeField]private float maxMoonLightIntensity;

    [Header("Lighting Colors")]
    [SerializeField] private Color dayAmbientLight;
    [SerializeField] private Color nightAmbientLight;
    [SerializeField] private AnimationCurve lightChangeCurve;

    [Header("Skybox Control")]
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private string blendPropertyName = "BlendValue";

    private TimeSpan sunriseTime;
    private TimeSpan sunsetTime;
    private DateTime currentTime;

    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);
        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
    }

    void Update()
    {
        UpdateTimeOfDay();
        RotateSun();
        UpdateLightSettings();
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);
    }

    private void RotateSun()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;
            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;
            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);
        }

        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        float evaluated = lightChangeCurve.Evaluate(dotProduct);

        sunLight.intensity = Mathf.Lerp(0, maxSunlightIntensity, evaluated);
        moonLight.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, evaluated);
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, evaluated);

        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetFloat(blendPropertyName, evaluated);
        }
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
        {
            difference += TimeSpan.FromHours(24);
        }

        return difference;
    }

    public DateTime GetCurrentTime() { return currentTime; }
    public TimeSpan GetSunriseTime() { return sunriseTime; }
    public TimeSpan GetSunsetTime() { return sunsetTime; }
}

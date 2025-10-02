using UnityEngine;
using UnityEngine.UI;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private TimeController timeController;

    [Header("Clock Hands")]
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform minuteHand;

    void Update()
    {
        if (timeController == null) return;

        System.DateTime currentTime = timeController.GetCurrentTime();

        float hours = currentTime.Hour % 12 + currentTime.Minute / 60f;
        float minutes = currentTime.Minute + currentTime.Second / 60f;

        float hourRotation = -hours * 30f;
        float minuteRotation = -minutes * 6f;

        if (hourHand != null) hourHand.localRotation = Quaternion.Euler(0, 0, hourRotation);
        if (minuteHand != null) minuteHand.localRotation = Quaternion.Euler(0, 0, minuteRotation);
    }
}

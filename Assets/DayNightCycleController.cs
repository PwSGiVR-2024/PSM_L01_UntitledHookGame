using UnityEngine;
using System.Net;
using Newtonsoft.Json.Linq;
using System;
using static UnityEngine.Rendering.DebugUI.Table;

public class DayNightCycleController : MonoBehaviour
{
    private string apiKey = "2687504016135b9b607a3a085505f5b1";
    private float latitude= 53.4256493f;
    private float longitude = 14.565149f;
    private int timeZone, timeUTC, currentTime;
    private DateTime currentDate;

    private float minTemperature = 2000f; // sunrise/sunset
    private float maxTemperature = 6500f; // midday

    private float minIntensity = 0.1f; // night
    private float maxIntensity = 1.2f; // noon

    [SerializeField] Light sceneLight;

    private string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

    void Start()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is not set. Please set it in the Inspector.");
            return;
        }

        StartCoroutine(GetTimeOfDay());
        ChangeDaylight();

    }

    System.Collections.IEnumerator GetTimeOfDay()
    {
        string url = $"{apiUrl}?lat={latitude}&lon={longitude}&appid={apiKey}";

        using (WebClient client = new WebClient())
        {
            client.DownloadStringAsync(new System.Uri(url));
            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    Debug.LogError("Error fetching time data: " + e.Error.Message);
                    return;
                }

                string jsonResponse = e.Result;
                ParseTimeData(jsonResponse);
            };
        }
        yield return null; // Wait for the request to complete
    }

    void ChangeDaylight() {
        if (currentDate != null) {
            float timeNormalized = (currentDate.Hour * 3600f + currentDate.Minute * 60f + currentDate.Second) / 86400f;

            float angle = timeNormalized * 2f * Mathf.PI;

            float lightFactor = Mathf.Clamp01(Mathf.Sin(angle));

            float intensity = Mathf.Lerp(minIntensity, maxIntensity, lightFactor);
            float temperature = Mathf.Lerp(minTemperature, maxTemperature, lightFactor);

            sceneLight.useColorTemperature = true;
            sceneLight.intensity = intensity;
            sceneLight.colorTemperature = temperature;
        }
    }
    void ParseTimeData(string jsonString)
    {
        try
        {
            JObject json = JObject.Parse(jsonString);
            JToken timeUTCToken = json["dt"];
            JToken timezoneToken = json["timezone"];

            if (timezoneToken != null && timeUTCToken != null)
            {
                timeUTC = timeUTCToken.ToObject<int>();
                timeZone = timezoneToken.ToObject<int>();
                currentTime = timeUTC + timeZone;
                Debug.Log("Current Time: " + currentTime);
                currentDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                currentDate = currentDate.AddSeconds(currentTime);
                //currentDate = currentDate.AddSeconds(3600*8); for testing
                Debug.Log("Current date: " + currentDate.ToString("dddd, dd MMM yyyy HH:mm"));
            }
            else
            {
                Debug.LogError("Time data not found in the response.");
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing JSON: " + e.Message);
        }
    }
}



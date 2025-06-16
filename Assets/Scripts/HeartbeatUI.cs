using UnityEngine;
using UnityEngine.UIElements;

public class HeartbeatUI : MonoBehaviour
{
    public UIDocument uiDocument;
    public float animationDuration = 2f;

    private VisualElement pulse;
    private float elapsedTime;

    void OnEnable()
    {
        pulse = uiDocument.rootVisualElement.Q<VisualElement>("heartbeat");
    }

    void Update()
    {
        if (pulse == null)
            return;

        elapsedTime += Time.deltaTime;
        float t = (elapsedTime % animationDuration) / animationDuration;

        // Animate width as a simulation of heartbeat
        float width = Mathf.Lerp(0f, 200f, HeartbeatEase(t));
        pulse.style.width = width;
    }

    float HeartbeatEase(float t)
    {
        // A rough heartbeat shape
        if (t < 0.1f) return t * 10f;
        if (t < 0.2f) return 1f - (t - 0.1f) * 10f;
        if (t < 0.3f) return 0.2f;
        if (t < 0.4f) return 1f;
        if (t < 0.5f) return 0.2f;
        return 0f;
    }
}
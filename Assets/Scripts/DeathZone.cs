using UnityEngine;
using UnityEngine.Events;

public class DeathZone : MonoBehaviour
{
    private UnityAction<float> cachedListener;
    private int playerLayer;

    void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        var player = GameObject.FindGameObjectWithTag("Player");
        var receiver = player?.GetComponent<TimerHealth>();
        if (receiver != null)
        {
            cachedListener = receiver.ModifyTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == playerLayer && cachedListener != null)
        {
            Debug.Log("Player entered DEATH ZONE.");
            cachedListener.Invoke(-9999f);
        }
    }
}

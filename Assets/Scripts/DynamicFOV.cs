using Fragsurf.Movement;
using UnityEngine;

public class DynamicFOV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera playerCamera;
    [SerializeField] SurfCharacter mvmt;

    [Header("FOV Settings")]
    [SerializeField] float baseFOV = 90f;
    [SerializeField] float maxFOV = 110f;
    [SerializeField] float maxSpeed = 50f;
    [SerializeField] float fovLerpSpeed = 5f;

    private void Reset()
    {
        if (!playerCamera) playerCamera = Camera.main;
    }

    private void Update()
    {
        float speed = mvmt.moveData.velocity.magnitude;
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speed / maxSpeed);
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }

}

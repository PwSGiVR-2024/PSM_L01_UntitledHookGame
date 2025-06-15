using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Movement;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;

public class GrapplingHook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SurfCharacter mvmt;
    [SerializeField] Transform player;
    [SerializeField] Transform cam;
    [SerializeField] Transform gunTip;
    [SerializeField] LayerMask grappleable;
    [SerializeField] LineRenderer lr;

    [Header("Grappling")]
    [SerializeField] float maxGrappleDistance;
    [SerializeField] float overshootY;
    [SerializeField] float grappleDelay;
    [SerializeField] float groundedGrappleForce = 25f;

    [Header("Swinging")]
    [SerializeField] float maxSwingDistance;
    private Vector3 swingPoint;
    private SpringJoint jointG, jointS;
    [SerializeField] float jointMaxDistance = 0.9f;
    [SerializeField] float jointMinDistance = 0.1f;
    [SerializeField] float jointSpring = 15f;
    [SerializeField] float jointDamper = 5f;
    [SerializeField] float jointMassScale = 1f; 
    private Vector3 currentGrapplePosition;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private Transform groundCheckPoint;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse2;
    public KeyCode swingKey = KeyCode.Mouse1;

    private bool grappling;
    private bool swinging;
    private Rigidbody playerRb;
    private bool wasKinematic;
    private bool wasUsingGravity;
    private Vector3 preGrappleVelocity;

    void Start()
    {

    }

    // Helper method to safely get the rigidbody
    private Rigidbody GetPlayerRigidbody()
    {
        if (playerRb == null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
        return playerRb;
    }

    void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }
        if (grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.deltaTime;
        }
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
        }
    }

    void LateUpdate()
    {
        if (grappling && lr.positionCount > 1)
        {
            lr.SetPosition(0, gunTip.position);
        }
        DrawRope();
    }

    private void StartSwing()
    {
        if (swinging) return; // Prevent multiple swings

        // Safely get the rigidbody
        Rigidbody rb = GetPlayerRigidbody();
        if (rb == null)
        {
            Debug.LogError("Cannot swing: No Rigidbody found on player!");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, grappleable))
        {
            swinging = true;

            // Store original rigidbody state
            wasKinematic = rb.isKinematic;
            wasUsingGravity = rb.useGravity;

            // Store current Fragsurf velocity before switching to physics
            Vector3 fragsurfVelocity = mvmt.moveData.velocity;

            // Enable physics for swinging
            rb.isKinematic = false;
            rb.useGravity = true;

            // Apply the Fragsurf velocity to the rigidbody BEFORE disabling movement
            rb.linearVelocity = fragsurfVelocity;

            // Disable Fragsurf movement
            mvmt.grappling = true;
            mvmt.enabled = false;

            swingPoint = hit.point;
            jointS = player.gameObject.AddComponent<SpringJoint>();
            jointS.autoConfigureConnectedAnchor = false;
            jointS.connectedAnchor = swingPoint;
            jointS.enablePreprocessing = false;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // More reasonable joint settings
            jointS.maxDistance = distanceFromPoint * jointMaxDistance;
            jointS.minDistance = distanceFromPoint * jointMinDistance;
            jointS.spring = jointSpring;
            jointS.damper = jointDamper;
            jointS.massScale = jointMassScale;

            // Setup line renderer
            lr.enabled = true;
            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;

            Debug.Log("Starting swing with velocity: " + fragsurfVelocity + " magnitude: " + fragsurfVelocity.magnitude);
        }
    }

    void DrawRope()
    {
        if (!swinging || lr.positionCount < 2) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
    }

    private void StopSwing()
    {
        if (!swinging) return;

        Rigidbody rb = GetPlayerRigidbody();
        if (rb == null) return;

        Vector3 exitVelocity = rb.linearVelocity;

        // Clean up spring joint
        if (jointS != null)
        {
            Destroy(jointS);
            jointS = null;
        }

        // Clean up line renderer
        lr.positionCount = 0;
        lr.enabled = false;

        swinging = false;

        Debug.Log("Swing exit velocity: " + exitVelocity + " magnitude: " + exitVelocity.magnitude);

        // Transition back to Fragsurf movement
        StartCoroutine(ReenableFragsurfAfterSwing(exitVelocity));
    }

    IEnumerator ReenableFragsurfAfterSwing(Vector3 carryOverVelocity)
    {
        // Wait a physics frame to ensure clean transition
        yield return new WaitForFixedUpdate();

        Rigidbody rb = GetPlayerRigidbody();
        if (rb != null)
        {
            // Restore rigidbody state
            rb.isKinematic = wasKinematic;
            rb.useGravity = wasUsingGravity;
        }

        // Re-enable Fragsurf movement
        mvmt.enabled = true;
        mvmt.grappling = false;

        // Wait one more frame for Fragsurf to initialize
        yield return null;

        // Apply carried velocity with some damping to prevent extreme speeds
        Vector3 dampedVelocity = carryOverVelocity;

        // Limit horizontal velocity to prevent uncontrollable sliding
        Vector3 horizontalVel = new Vector3(dampedVelocity.x, 0, dampedVelocity.z);
        if (horizontalVel.magnitude > 25f) // Adjust this limit as needed
        {
            horizontalVel = horizontalVel.normalized * 25f;
            dampedVelocity = new Vector3(horizontalVel.x, dampedVelocity.y, horizontalVel.z);
        }

        mvmt.moveData.velocity = dampedVelocity;
        Debug.Log("Final velocity applied: " + mvmt.moveData.velocity);
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0)
        {
            return;
        }

        // Don't start grapple if already swinging
        if (swinging)
        {
            return;
        }

        // Store current velocity before any modifications
        preGrappleVelocity = mvmt.moveData.velocity;

        grappling = true;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            grapplePoint = hit.point;

            // Check if player is grounded - if so, don't freeze them completely
            bool isGrounded = mvmt.moveData.velocity.y > -0.1f && mvmt.moveData.velocity.y < 0.1f;

            if (!isGrounded)
            {
                // For air grapples, freeze movement for windup
                mvmt.frozen = true;
            }
            else
            {
                // For grounded grapples, just reduce movement but don't freeze
                mvmt.moveData.velocity *= 0.5f; // Reduce momentum but don't eliminate it
            }

            // Setup line renderer for grapple
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);

            Invoke(nameof(ExecuteGrapple), grappleDelay);
        }
        else
        {
            // No valid target found
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            // Setup line renderer for failed grapple
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, grapplePoint);

            Invoke(nameof(StopGrapple), grappleDelay);
        }
    }

    private void ExecuteGrapple()
    {
        mvmt.frozen = false;

        Vector3 playerPos = transform.position;
        float grapplePointRelativeYPos = grapplePoint.y - playerPos.y;

        // Check if this is a grounded grapple (horizontal or slightly upward)
        // Use a more reliable detection based on the actual Y difference
        bool isGroundedGrapple = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer); // Increased threshold

        Debug.Log($"Grapple point Y difference: {grapplePointRelativeYPos}, IsGrounded: {isGroundedGrapple}");
        Debug.Log($"Pre-grapple velocity: {preGrappleVelocity}");

        if (isGroundedGrapple)
        {
            Debug.Log("Executing GROUNDED grapple");

            // For grounded grapples, manually apply momentum instead of using JumpToPosition
            Vector3 directionToGrapple = (grapplePoint - playerPos).normalized;
            Vector3 horizontalDirection = new Vector3(directionToGrapple.x, 0, directionToGrapple.z).normalized;

            // Use much stronger force for grounded grapples
            float actualGroundedForce = groundedGrappleForce * 2f; // Double the force

            // Combine existing horizontal velocity with grapple momentum
            Vector3 launchDirection = Quaternion.AngleAxis(45f, Vector3.Cross(horizontalDirection, Vector3.up)) * horizontalDirection;


            // Add upward boost based on grapple angle
            // float upwardBoost = Mathf.Max(5f, grapplePointRelativeYPos * 2f);

            // Apply the new velocity directly
            Vector3 finalVelocity = launchDirection * actualGroundedForce;
            mvmt.moveData.grounded = false;
            mvmt.moveData.velocity = finalVelocity;

            Debug.Log($"Applied grounded grapple velocity: {finalVelocity} (magnitude: {finalVelocity.magnitude})");
        }
        else
        {
            Debug.Log("Executing AIR grapple");

            // For air grapples, calculate proper trajectory
            Vector3 directionToGrapple = (grapplePoint - playerPos).normalized;
            float distance = Vector3.Distance(playerPos, grapplePoint);

            // Use a simpler, more reliable calculation
            float horizontalForce = 20f; // Base horizontal force
            float verticalForce = 15f; // Base vertical force

            // If grappling upward, increase vertical force
            if (grapplePointRelativeYPos > 0)
            {
                verticalForce += grapplePointRelativeYPos * 2f;
            }

            Vector3 horizontalVel = new Vector3(directionToGrapple.x, 0, directionToGrapple.z) * horizontalForce;
            Vector3 finalVelocity = new Vector3(horizontalVel.x, verticalForce, horizontalVel.z);

            mvmt.moveData.velocity = finalVelocity;
            Debug.Log($"Applied air grapple velocity: {finalVelocity} (magnitude: {finalVelocity.magnitude})");
        }

        Invoke(nameof(StopGrapple), 1f);
    }

    private void StopGrapple()
    {
        mvmt.frozen = false;
        grappling = false;
        grapplingCdTimer = grapplingCd;
        lr.enabled = false;
        lr.positionCount = 0;
    }
}
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

    [Header("Swinging")]
    [SerializeField] float maxSwingDistance;
    private Vector3 swingPoint;
    private SpringJoint jointG, jointS;
    [SerializeField] float jointMaxDistance;
    [SerializeField] float jointMinDistance;
    [SerializeField] float jointSpring;
    [SerializeField] float jointDamper;
    [SerializeField] float jointMassScale;
    private Vector3 currentGrapplePosition;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse2;
    public KeyCode swingKey = KeyCode.Mouse1;

    private bool grappling;
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
        if (Input.GetKeyDown(swingKey)){
            StartSwing();
        }
        if (Input.GetKeyUp(swingKey)){ 
            StopSwing();
        }
    }
    void LateUpdate() {
        if (grappling) {
            lr.SetPosition(0, gunTip.position);
        }
        DrawRope();
    }
    private void StartSwing() {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, grappleable))
        {
            player.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            player.gameObject.GetComponent<Rigidbody>().useGravity = true;
            mvmt.grappling = true;
            mvmt.enabled = false;
            swingPoint = hit.point;
            jointS = player.gameObject.AddComponent<SpringJoint>();
            jointS.autoConfigureConnectedAnchor = false;
            jointS.connectedAnchor=swingPoint;
            jointS.enablePreprocessing = false;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // the distance grapple will try to keep from grapple point. 
            jointS.maxDistance = distanceFromPoint * jointMaxDistance;
            jointS.minDistance = distanceFromPoint * jointMinDistance;

            // customize values as you like
            jointS.spring = jointSpring;
            jointS.damper = jointDamper;
            jointS.massScale = jointMassScale;
            lr.enabled = true;
            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }
    void DrawRope() {
        if (!jointG&&!jointS) return;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
    }
    private void StopSwing()
    {
        Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();
        Vector3 tangentDir = (player.position - swingPoint).normalized;
        Vector3 exitVel = rb.GetPointVelocity(player.position);

        if (jointS != null)
        {
            Destroy(jointS);
            jointS = null;
        }
        Debug.Log("Swing exit velocity magn: " + rb.linearVelocity.magnitude);
        Debug.Log("Swing exit velocity: " + rb.linearVelocity);


        lr.positionCount = 0;
        mvmt.enabled = false;
        //mvmt.moveData.velocity = rb.linearVelocity;
        //mvmt.moveData.origin = player.position;
        //mvmt.enabled = true;
        //rb.linearVelocity += (player.position - swingPoint).normalized * 8f;
        StartCoroutine(ReenableFragsurfAfterSwing(exitVel));
        //StartCoroutine(ReenableKinematic(0.3f));

    }
    /*IEnumerator ReenableKinematic(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Optional: only reenable if grounded or speed is low
        Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();
        if (mvmt.enabled)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }*/
    IEnumerator ReenableFragsurfAfterSwing(Vector3 carryOverVelocity)
    {
        yield return new WaitForFixedUpdate(); // Wait one physics frame
        //yield return new WaitForSeconds(0.05f); // Helps avoid snapping

        Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        mvmt.enabled = true;
        mvmt.grappling = false;

        // Wait a single frame so Fragsurf doesn't glitch
        yield return null;
        Debug.Log("Velocity before: " + carryOverVelocity);
        mvmt.moveData.velocity = carryOverVelocity;
        Debug.Log("Velocity after: " + mvmt.moveData.velocity);

    }
    private void StartGrapple() {
        if (grapplingCdTimer > 0) {
            return;
        }
        grappling = true;
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            mvmt.frozen = true;
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelay);
        }
        else {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelay);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }
    private void ExecuteGrapple()
    {
        mvmt.frozen = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootY;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootY;

        mvmt._controller.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);

    }
    private void StopGrapple()
    {
        mvmt.frozen = false;
        grappling = false;
        grapplingCdTimer = grapplingCd;
        lr.enabled = false;
    }


}

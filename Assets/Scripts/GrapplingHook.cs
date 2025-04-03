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
    private SpringJoint joint;
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
            Debug.Log("swing");
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
            swingPoint = hit.point;
            Debug.Log("hello?");
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor=swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // the distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * jointMaxDistance;
            joint.minDistance = distanceFromPoint * jointMinDistance;

            // customize values as you like
            joint.spring = jointSpring;
            joint.damper = jointDamper;
            joint.massScale = jointMassScale;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }
    void DrawRope() {
        if (!joint) return;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
    }
    private void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }
    // Update is called once per frame
    private void StartGrapple() {
        Debug.Log("hook fired");
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

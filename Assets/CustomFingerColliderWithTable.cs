using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class CustomFingerColliderWithTable : MonoBehaviour
{
    private OVRSkeleton rightHandSkeleton, leftHandSkeleton, bodySkeleton;
    private OVRHand rightHand, leftHand;
    private OVRBone rightIndexTipBone, leftIndexTipBone, leftShoulderBone, rightShoulderBone;
    private Vector3 chickLeftWingInitialPosition;
    private Vector3 chickRightWingInitialPosition;
    [SerializeField]
    private GameObject chickLeftLeg, chickRightLeg;

    private float firstTapFlagLeft, firstTapFlagRight;

    [SerializeField]
    private GameObject chickLeftWing, chickRightWing;

    private Vector3 chickLeftLegInitialPosition;
    private Vector3 chickRightLegInitialPosition;

    [SerializeField]
    private GameObject chicken;
    public float tableSurfaceHeight; // Set this to the Y position of your table surface


    [SerializeField]
    private TMP_Text debugTextViewIndex, debugTextViewMiddle;

    [SerializeField]
    private float dampingFactor = 0.001f;


    [SerializeField]
    private float maxFrequency = 1f;

    // Variables for tap detection
    private Vector3 previousLeftIndexTipPosition;
    private bool isTapGestureDetected = false;

    private float lastLegMovementTime = 0f;
    private float legMovementFrequency = 0f;
    private Vector3 lastChickLeftLegPosition;
    private Vector3 lastChickRightLegPosition;
    private float walkingSpeed = 0f;


    private float lastWingMovementTime = 0f;
    private float wingMovementFrequency = 0f;
    private Vector3 lastChickLeftWingPosition;
    private Vector3 lastChickRightWingPosition;


    void Start()
    {
        // Automatically find the right hand OVRSkeleton
        OVRSkeleton[] skeletons = FindObjectsOfType<OVRSkeleton>();
        OVRHand[] hands = FindObjectsOfType<OVRHand>();
        // Store initial positions
        chickLeftLegInitialPosition = chickLeftLeg.transform.localPosition;
        chickRightLegInitialPosition = chickRightLeg.transform.localPosition;
        chickLeftWingInitialPosition = chickLeftWing.transform.localPosition;
        chickRightWingInitialPosition = chickRightWing.transform.localPosition;


        lastChickLeftWingPosition = chickLeftWing.transform.localPosition;
        lastChickRightWingPosition = chickRightWing.transform.localPosition;

        foreach (var skeleton in skeletons)
        {
            if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
            {
                rightHandSkeleton = skeleton;

            }

            else if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft)
            {
                leftHandSkeleton = skeleton;

            }
            //  else if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.FullBody)
            // {
            //     bodySkeleton = skeleton;

            // }

            // else if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.Body)
            // {
            //     bodySkeleton = skeleton;

            // }
        }

        foreach (var hand in hands)
        {
            if (hand.IsDominantHand)
            {
                rightHand = hand;
            }
            else
            {
                leftHand = hand;
            }
        }

        if (rightHandSkeleton == null)
        {
            debugTextViewIndex.text = "Right hand OVRSkeleton not found.";
            Debug.LogError("Right hand OVRSkeleton not found.");
            return;
        }

        // foreach (var bone in bodySkeleton.Bones)
        // {
        //     if (bone.Id == OVRSkeleton.BoneId.Body_RightShoulder)
        //     {
        //         rightShoulderBone = bone;
        //         //break;
        //     }
        //     if (bone.Id == OVRSkeleton.BoneId.Body_LeftShoulder)
        //     {
        //         leftShoulderBone = bone;
        //         //break;
        //     }

        // }
        // Find the index tip bone
        foreach (var bone in rightHandSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                rightIndexTipBone = bone;
                //break;
            }


        }
        foreach (var bone in leftHandSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                leftIndexTipBone = bone;
                //break;
            }

        }

        foreach (var bone in bodySkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.FullBody_LeftShoulder)
            {
                leftShoulderBone = bone;
                //break;
            }

            if (bone.Id == OVRSkeleton.BoneId.FullBody_RightShoulder)
            {
                rightShoulderBone = bone;
                //break;
            }

        }

        lastChickLeftLegPosition = chickLeftLeg.transform.localPosition;
        lastChickRightLegPosition = chickRightLeg.transform.localPosition;




    }

    void AnchorChickToFootGround()
    {
        // Assuming the ground is at a constant y position (e.g., y=0 or tableSurfaceHeight for a table scenario)
        // Adjust the y position to the ground level or table surface height, ensuring feet are always touching the ground
        // float groundLevel = tableSurfaceHeight; // or a specific y value if the ground is not at y=0
        // Vector3 groundedPosition = new Vector3(chicken.transform.position.x, groundLevel, chicken.transform.position.z);
        // chicken.transform.position = groundedPosition;

        // Ensure the chick is always upright
        // This sets the rotation to be upright regardless of previous physics interactions

    }

    void Update()
    {
        bool leftTapDetected = false;
        bool rightTapDetected = false;
        Rigidbody chickenRigidbody = chicken.GetComponent<Rigidbody>();

        //reset table height precisely
        if (leftIndexTipBone != null && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            tableSurfaceHeight = leftIndexTipBone.Transform.position.y;
        }

        float offsetFactor = 0.02f;
        if (leftIndexTipBone != null)
        {
            float leftHandYPosition = leftIndexTipBone.Transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetLeft = leftHandYPosition - Camera.main.transform.position.y;
            //float yOffsetLeft = leftHandYPosition - leftShoulderBone.Transform.position.y;
            // Apply the offset with a multiplier for more or less exaggeration
            chickLeftWing.transform.localPosition = new Vector3(
                chickLeftWing.transform.localPosition.x,
                chickLeftWing.transform.localPosition.y, // Adjust multiplier as needed
                chickLeftWingInitialPosition.z - yOffsetLeft * 0.1f - offsetFactor);
        }

        // Adjust the right wing based on the right hand's Y position
        if (rightIndexTipBone != null)
        {
            float rightHandYPosition = rightIndexTipBone.Transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetRight = rightHandYPosition - Camera.main.transform.position.y;
            //float yOffsetRight = rightHandYPosition - rightShoulderBone.Transform.position.y;

            // Apply the offset with a multiplier for more or less exaggeration
            chickRightWing.transform.localPosition = new Vector3(
                chickRightWing.transform.localPosition.x,
                chickRightWing.transform.localPosition.y, // Adjust multiplier as needed
                chickRightWingInitialPosition.z + yOffsetRight * 0.1f + offsetFactor);
        }



        // Calculate wing movement frequency
        float timeSinceLastWingMovement = Time.time - lastWingMovementTime;
        Vector3 currentLeftWingPosition = chickLeftWing.transform.localPosition;
        Vector3 currentRightWingPosition = chickRightWing.transform.localPosition;


        if ((currentLeftWingPosition - lastChickLeftWingPosition).magnitude > 0.004f || (currentRightWingPosition - lastChickRightWingPosition).magnitude > 0.004f)
        {
            wingMovementFrequency = 1f / timeSinceLastWingMovement; // Frequency is inverse of time
            lastWingMovementTime = Time.time;
        }
        else
        {
            wingMovementFrequency = 0f;
        }

        // Limit the wing movement frequency to prevent unrealistic values
        // wingMovementFrequency = Mathf.Min(wingMovementFrequency, maxFrequency); // Define maxFrequency as needed

        // wingMovementFrequency=1f;
        // Apply frequency to chicken's Y position with a damping factor
        // float newYPosition = Mathf.Lerp(chicken.transform.position.y, chicken.transform.position.y + (wingMovementFrequency * 1f), Time.deltaTime * dampingFactor); // Define dampingFactor as needed
        // chicken.transform.position = new Vector3(chicken.transform.position.x, newYPosition, chicken.transform.position.z);


        if (wingMovementFrequency > 0)
        {
            // Apply a continuous upward force based on the wing movement frequency
            float flyForce = Mathf.Lerp(0, 20f, wingMovementFrequency * 4); // Scale the force based on frequency
            chickenRigidbody.AddForce(Vector3.up * flyForce, ForceMode.Force);

            // Calculate the difference between left and right hand Y positions
            float handYPositionDifference = rightIndexTipBone.Transform.position.y - leftIndexTipBone.Transform.position.y;

            // Determine the directional force based on the hand Y position difference
            // This will add a force that is slightly offset from the forward direction based on the hand positions
            Vector3 directionalForce = chicken.transform.forward - (chicken.transform.right * handYPositionDifference * 2f); // Adjust the multiplier as needed for desired effect

            // Move the chicken forward while flying, with a slight offset based on hand positions
            float forwardForce = 1f; // Adjust this value as needed for desired forward movement speed
            chickenRigidbody.velocity = new Vector3(directionalForce.x * forwardForce, chickenRigidbody.velocity.y, directionalForce.z * forwardForce);





            lastChickLeftWingPosition = currentLeftWingPosition;
            lastChickRightWingPosition = currentRightWingPosition;

        }

        if (wingMovementFrequency == 0)
        {



            if (rightIndexTipBone.Transform.position.y < tableSurfaceHeight + 0.05 && leftIndexTipBone.Transform.position.y < tableSurfaceHeight + 0.05)
            {


                if (rightIndexTipBone != null)
                {
                    if (rightIndexTipBone.Transform.position.y <= tableSurfaceHeight)
                    {
                        rightTapDetected = true;
                        debugTextViewIndex.text = "Collided right";

                        //Debug.Log("Right index tip is below or at the table surface height!");

                        if (firstTapFlagRight == 1)
                        {
                            Debug.Log("First tap");
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Footsteps/walk");
                            firstTapFlagRight = 0;
                        }



                    }
                    else
                    {
                        debugTextViewIndex.text = "Not collided";
                        firstTapFlagRight = 1;
                        // Optionally reset to initial position if no collision
                        //chickLeftLeg.transform.position = chickLeftLegInitialPosition;
                        // Corrected: Assuming you want to move the leg relative to the chicken's current position and orientation

                        float rightHandYPositionDifference = rightIndexTipBone.Transform.position.y - tableSurfaceHeight;
                        // Apply the difference to the localPosition.y of the right leg
                        chickRightLeg.transform.localPosition = new Vector3(
                        chickRightLegInitialPosition.x - 1f * rightHandYPositionDifference,
                        chickRightLeg.transform.localPosition.y, // Adjust this formula as needed
                        chickRightLeg.transform.localPosition.z);


                    }
                }

                if (leftIndexTipBone != null)
                {
                    if (leftIndexTipBone.Transform.position.y <= tableSurfaceHeight)
                    {
                        leftTapDetected = true;
                        Debug.Log("Left index tip is below or at the table surface height!");
                        debugTextViewMiddle.text = "Collided left";
                        if (firstTapFlagLeft == 1)
                        {

                            Debug.Log("First tap");
                            FMODUnity.RuntimeManager.PlayOneShot("event:/Footsteps/walk");
                            firstTapFlagLeft = 0;
                        }

                    }
                    else
                    {
                        firstTapFlagLeft = 1;
                        debugTextViewMiddle.text = "Not collided";

                        float leftHandYPositionDifference = leftIndexTipBone.Transform.position.y - tableSurfaceHeight;
                        // Apply the difference to the localPosition.y of the left leg
                        chickLeftLeg.transform.localPosition = new Vector3(
                            chickLeftLegInitialPosition.x - 1f * leftHandYPositionDifference,
                            chickLeftLeg.transform.localPosition.y, // Adjust this formula as needed
                            chickLeftLeg.transform.localPosition.z);



                        // Optionally reset to initial position if no collision
                        //chickRightLeg.transform.position = chickRightLegInitialPosition;
                    }
                }

            }
            // Calculate leg movement frequency and update walking speed
            float timeSinceLastMovement = Time.time - lastLegMovementTime;
            Vector3 currentLeftLegPosition = chickLeftLeg.transform.localPosition;
            Vector3 currentRightLegPosition = chickRightLeg.transform.localPosition;
            float deviationThreshold = 60f; // Threshold in degrees for deviation
            Vector3 expectedUpDirection = Vector3.up;
            float angleDeviation = Vector3.Angle(chicken.transform.up, expectedUpDirection);

            if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude > 0.005f && (currentRightLegPosition - lastChickRightLegPosition).magnitude < 0.005f)
            {
                // Rotate chicken to left by 15 degrees
                RotateChicken(Quaternion.Euler(0, -15, 0) * chicken.transform.forward);
            }

            else if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude < 0.005f && (currentRightLegPosition - lastChickRightLegPosition).magnitude > 0.005f)
            {
                // Rotate chicken to right by 15 degrees
                RotateChicken(Quaternion.Euler(0, 15, 0) * chicken.transform.forward);
            }

            // else if(angleDeviation > deviationThreshold){
            //       RotateChicken(Quaternion.Euler(0, 0, 0) * chicken.transform.forward);
            // }




            // Check if either leg has moved significantly since last frame
            if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude > 0.005f || (currentRightLegPosition - lastChickRightLegPosition).magnitude > 0.005f)
            {
                legMovementFrequency = 1f / timeSinceLastMovement; // Frequency is inverse of time
                walkingSpeed = legMovementFrequency * 0.01f; // Adjust multiplier to scale speed appropriately
                lastLegMovementTime = Time.time;
            }
            else
            {
                // If there's no significant movement, set walking speed to 0
                walkingSpeed = 0f;
            }

            lastChickLeftLegPosition = currentLeftLegPosition;
            lastChickRightLegPosition = currentRightLegPosition;

            // Move the chicken forward based on walking speed
            Vector3 moveDirection = chicken.transform.forward * walkingSpeed * Time.deltaTime;
            chicken.transform.position += moveDirection;


        }

        lastChickLeftWingPosition = currentLeftWingPosition;
        lastChickRightWingPosition = currentRightWingPosition;


        // chickRightLegInitialPosition += moveDirection;
        // chickLeftLegInitialPosition += moveDirection;
        // chickLeftWingInitialPosition += moveDirection;
        // chickRightWingInitialPosition += moveDirection;

        float currentYRotation = chicken.transform.eulerAngles.y;
        chicken.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);



    }

    void FixedUpdate()
    {
        ApplyCustomGravity();
    }


    void ApplyCustomGravity()
    {
        // Assuming 'chicken' has a Rigidbody component attached
        Rigidbody chickenRigidbody = chicken.GetComponent<Rigidbody>();
        if (chickenRigidbody != null)
        {
            // Define your custom gravity force vector
            Vector3 customGravityForce = new Vector3(0, -3f, 0); // This is just normal Earth gravity, adjust as needed

            // Apply the custom gravity force
            chickenRigidbody.AddForce(customGravityForce * chickenRigidbody.mass); // Multiply by mass to get a force equivalent to gravity
        }
    }

    void RotateChicken(Vector3 movementDirection)
    {
        // Normalize the movement direction to get only the direction vector
        Vector3 direction = movementDirection.normalized;
        // Ensure rotation is only on the Y axis
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            // Calculate the target rotation based on the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            // Extract the Y component of the target rotation
            float targetYRotation = targetRotation.eulerAngles.y;

            // Get the current rotation of the chicken in Euler angles
            Vector3 currentRotation = chicken.transform.rotation.eulerAngles;
            // Create a new Vector3 for the rotation, combining the target Y rotation with the current X and Z rotations set to 0
            Vector3 newRotation = new Vector3(0, targetYRotation, 0);

            // Smoothly rotate the chicken towards the target direction, preserving the upright orientation
            chicken.transform.rotation = Quaternion.Slerp(chicken.transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * 20);
        }
    }
}
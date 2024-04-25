using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class CustomFingerColliderWithTable : MonoBehaviour
{
    private OVRSkeleton rightHandSkeleton, leftHandSkeleton, bodySkeleton;
    private OVRHand rightHand, leftHand;
    private OVRBone rightIndexTipBone, rightMiddleTipBone, leftShoulderBone, rightShoulderBone, leftIndexTipBone;
    private Vector3 chickLeftWingInitialPosition;
    private Vector3 chickRightWingInitialPosition;
    [SerializeField]
    private GameObject chickLeftLeg, chickRightLeg;

    private float firstTapFlagLeft, firstTapFlagRight;

    private float headRotationFactor = 8.0f;

    private float rotationSpeed = 5f;

    [SerializeField]
    private GameObject chickLeftWing, chickRightWing;

    private Vector3 chickLeftLegInitialPosition;
    private Vector3 chickRightLegInitialPosition;

    [SerializeField]
    private GameObject chicken;
    public float tableSurfaceHeight; // Set this to the Y position of your table surface
    private  float forwardForce = 0.7f;

    [SerializeField]
    private TMP_Text debugTextViewIndex, debugTextViewMiddle;

    [SerializeField]
    private float dampingFactor = 0.001f;

    float wingThreshold = 0.004f;


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


            //turn left
    float legMovementThreshold = 0.003f;
    private Vector3 lastChickLeftWingPosition;
    private Vector3 lastChickRightWingPosition;

    private Rigidbody chickenRigidbody;

    void SpawnChicken()
    {
        if (chicken == null)
        {
            Debug.LogError("Chicken prefab is not assigned.");
            return;
        }

        // Calculate spawn position: 2 meters in front of the user
        Vector3 spawnPosition = Camera.main.transform.position;

        // Instantiate the chicken at the spawn position, with the same rotation as the prefab
        chicken.transform.position = spawnPosition; 
    }
    void Start()
    {
        SpawnChicken();
        // Automatically find the right hand OVRSkeleton
        OVRSkeleton[] skeletons = FindObjectsOfType<OVRSkeleton>();
        OVRHand[] hands = FindObjectsOfType<OVRHand>();
        chickenRigidbody = chicken.GetComponent<Rigidbody>();
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
            //debugTextViewIndex.text = "Right hand OVRSkeleton not found.";
            Debug.LogError("Right hand OVRSkeleton not found.");
            return;
        }

        // Find the index tip bone
        foreach (var bone in rightHandSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                rightIndexTipBone = bone;
                //break;
            }

             if (bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
            {
                rightMiddleTipBone = bone;
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

        lastChickLeftLegPosition = chickLeftLeg.transform.localPosition;
        lastChickRightLegPosition = chickRightLeg.transform.localPosition;
    }

private float MapFrequencyToNormalizedRange(float value, float min, float max)
{
    // Ensure the value is within the bounds of min and max
    value = Mathf.Clamp(value, min, max);
    // Map the value from [min, max] to [0, 1]
    return (value - min) / (max - min);
}
    void Update()
    {


        bool leftTapDetected = false;
        bool rightTapDetected = false;
       

        //reset table height precisely
        // if (leftIndexTipBone != null && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        // {
        //     tableSurfaceHeight = leftIndexTipBone.Transform.position.y;
        // }

         if (leftHand != null)
        {
            tableSurfaceHeight = leftHand.transform.position.y + 0.04f;
        }


        //Link wing rig to hands

        float offsetFactor = 0.02f;
        if (leftIndexTipBone != null)
        {
            float leftYPosition = leftIndexTipBone.Transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetLeft = leftYPosition - Camera.main.transform.position.y;
            // Apply the offset with a multiplier for more or less exaggeration
            chickLeftWing.transform.localPosition = new Vector3(
                chickLeftWing.transform.localPosition.x,
                chickLeftWing.transform.localPosition.y, // Adjust multiplier as needed
                chickLeftWingInitialPosition.z - yOffsetLeft * 0.08f - offsetFactor);
        }
        if (rightIndexTipBone != null)
        {
            float rightYPosition = rightIndexTipBone.Transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetRight = rightYPosition - Camera.main.transform.position.y;
            // Apply the offset with a multiplier for more or less exaggeration
            chickRightWing.transform.localPosition = new Vector3(
                chickRightWing.transform.localPosition.x,
                chickRightWing.transform.localPosition.y, // Adjust multiplier as needed
                chickRightWingInitialPosition.z + yOffsetRight * 0.08f + offsetFactor);
        }

        // Calculate wing movement frequency
        float timeSinceLastWingMovement = Time.time - lastWingMovementTime;
        Vector3 currentLeftWingPosition = chickLeftWing.transform.localPosition;
        Vector3 currentRightWingPosition = chickRightWing.transform.localPosition;
        

        // Calculate leg movement frequency and update walking speed
        float timeSinceLastMovement = Time.time - lastLegMovementTime;
        Vector3 currentLeftLegPosition = chickLeftLeg.transform.localPosition;
        Vector3 currentRightLegPosition = chickRightLeg.transform.localPosition;
       

        //flying mechanics
        if ((currentLeftWingPosition - lastChickLeftWingPosition).magnitude > wingThreshold || (currentRightWingPosition - lastChickRightWingPosition).magnitude > wingThreshold)
        {
            //play FMOD wing sound
            // FMODUnity.RuntimeManager.PlayOneShot("event:/Hackathon/Locomotion/wing");

            debugTextViewMiddle.text = "Wing:"+wingMovementFrequency;

            float normalizedFrequency = MapFrequencyToNormalizedRange(wingMovementFrequency, 0f, 100f); // Assuming 10 is the max frequency you've observed

                        // Create a parameter for FMOD
            FMOD.Studio.EventInstance wingSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/wing");
            wingSoundInstance.setParameterByName("Wing_Pitch", normalizedFrequency);
            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
            wingSoundInstance.set3DAttributes(attributes);
            wingSoundInstance.start();
            wingSoundInstance.release();

            // Example usage within the Update method or wherever you're handling the FMOD event triggering
           

            wingMovementFrequency = 1f / timeSinceLastWingMovement; // Frequency is inverse of time
            lastWingMovementTime = Time.time;

            // Apply a continuous upward force based on the wing movement frequency
            float flyForce = Mathf.Lerp(0, 10f, wingMovementFrequency ); // Scale the force based on frequency
            chickenRigidbody.AddForce(Vector3.up * flyForce, ForceMode.Force);

            // Calculate the difference between left and right hand Y positions
              // Get the roll angle of the player's head. Assuming the camera represents the player's head.
        float headRoll = Camera.main.transform.eulerAngles.z;

        // Normalize the roll value to be between -1 and 1 where 0 is upright, -1 is fully left, and 1 is fully right
        // Assuming that the roll value is given in degrees and can go from -180 to 180
        float normalizedHeadRoll = 0;
        if (headRoll <= 180)
        {
            normalizedHeadRoll = headRoll / 180;
        }
        else
        {
            normalizedHeadRoll = (headRoll - 360) / 180;
        }

        // Use the normalizedHeadRoll to influence the directional force
        Vector3 directionalForce = chicken.transform.forward - (chicken.transform.right * normalizedHeadRoll * headRotationFactor);

        // The rest of your flying mechanics code
        // Adjust this value as needed for desired forward movement speed

        chickenRigidbody.velocity = new Vector3(directionalForce.x * forwardForce, chickenRigidbody.velocity.y, directionalForce.z * forwardForce);
          
          if (chickenRigidbody.velocity != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(chickenRigidbody.velocity.normalized);
        chicken.transform.rotation = Quaternion.Slerp(chicken.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
        }
        //foot step connection with fingers and rig
        else
        {
            if (rightIndexTipBone.Transform.position.y >= tableSurfaceHeight - 0.05f && rightMiddleTipBone.Transform.position.y >= tableSurfaceHeight-0.05f && rightIndexTipBone.Transform.position.y < tableSurfaceHeight + 0.05f && rightMiddleTipBone.Transform.position.y < tableSurfaceHeight + 0.05f)
            {
               
                    if (rightMiddleTipBone != null && rightMiddleTipBone.Transform.position.y <= tableSurfaceHeight)
                    {
                        rightTapDetected = true;
                        debugTextViewIndex.text = "Collided right";
                        //Debug.Log("Right index tip is below or at the table surface height!");
                        if (firstTapFlagRight == 1)
                        {
                            Debug.Log("First tap");
                            float normalizedFrequency = MapFrequencyToNormalizedRange(legMovementFrequency, 0f, 10f); // Assuming 10 is the max frequency you've observe               // Create a parameter for FMOD
                            FMOD.Studio.EventInstance walkSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/walk");
                            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
                            walkSoundInstance.set3DAttributes(attributes);
                            walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
                            walkSoundInstance.start();
                            walkSoundInstance.release();

                            firstTapFlagRight = 0;
                        }
                    }

                    else
                    {
                        debugTextViewIndex.text = "Not collided";
                        firstTapFlagRight = 1;
                        float rightHandYPositionDifference = rightMiddleTipBone.Transform.position.y - tableSurfaceHeight;
                        // Apply the difference to the localPosition.y of the right leg
                        chickRightLeg.transform.localPosition = new Vector3(
                        chickRightLegInitialPosition.x - 1f * rightHandYPositionDifference,
                        chickRightLeg.transform.localPosition.y, // Adjust this formula as needed
                        chickRightLeg.transform.localPosition.z);
                    }

             
                    if (rightIndexTipBone != null && rightIndexTipBone.Transform.position.y <= tableSurfaceHeight)
                    {
                        leftTapDetected = true;
                        debugTextViewIndex.text = "Collided left";
                        Debug.Log("Left index tip is below or at the table surface height!");
                        //debugTextViewMiddle.text = "Collided left";
                        if (firstTapFlagLeft == 1)
                        {

                            Debug.Log("First tap");
                            
                            //debugTextViewIndex.text = "Leg:"+legMovementFrequency;
                            float normalizedFrequency = MapFrequencyToNormalizedRange(legMovementFrequency, 0f, 10f); // Assuming 10 is the max frequency you've observed
                         // Create a parameter for FMOD
                            FMOD.Studio.EventInstance walkSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/walk");
                            walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
                            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
                            walkSoundInstance.set3DAttributes(attributes);
                            walkSoundInstance.start();
                            walkSoundInstance.release();
                            firstTapFlagLeft = 0;
                        }
                    }
                    else
                    {
                        firstTapFlagLeft = 1;
                       debugTextViewMiddle.text = "Not collided";

                        float leftHandYPositionDifference = rightIndexTipBone.Transform.position.y - tableSurfaceHeight;
                        // Apply the difference to the localPosition.y of the left leg
                        chickLeftLeg.transform.localPosition = new Vector3(
                            chickLeftLegInitialPosition.x - 1f * leftHandYPositionDifference,
                            chickLeftLeg.transform.localPosition.y, // Adjust this formula as needed
                            chickLeftLeg.transform.localPosition.z);
                    }
                

            }

            if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude > legMovementThreshold && (currentRightLegPosition - lastChickRightLegPosition).magnitude < legMovementThreshold)
            {
                // Rotate chicken to left by 15 degrees
                RotateChicken(Quaternion.Euler(0, -20, 0) * chicken.transform.forward);
            }
              //turn right
            else if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude < legMovementThreshold && (currentRightLegPosition - lastChickRightLegPosition).magnitude > legMovementThreshold)
            {
                // Rotate chicken to right by 15 degrees
                RotateChicken(Quaternion.Euler(0, 20, 0) * chicken.transform.forward);
            }
            //walk forward
            if ((currentLeftLegPosition - lastChickLeftLegPosition).magnitude > legMovementThreshold || (currentRightLegPosition - lastChickRightLegPosition).magnitude > legMovementThreshold)
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

            

            // Move the chicken forward based on walking speed
            Vector3 moveDirection = chicken.transform.forward * walkingSpeed * Time.deltaTime;
            chicken.transform.position += moveDirection;
        }



        //fix the bird to always face parallel to XZ plane
        float currentYRotation = chicken.transform.eulerAngles.y;
        chicken.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);

        lastChickLeftLegPosition = currentLeftLegPosition;
        lastChickRightLegPosition = currentRightLegPosition;
        lastChickLeftWingPosition = currentLeftWingPosition;
        lastChickRightWingPosition = currentRightWingPosition;
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
            Vector3 customGravityForce = new Vector3(0, -5f, 0); // This is just normal Earth gravity, adjust as needed

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

            // Define a rotation speed (degrees per second)
            float rotationSpeed = 20f; // Adjust this value as needed

            // Smoothly rotate the chicken towards the target direction, preserving the upright orientation
            // Adjust the interpolation speed based on Time.deltaTime and rotationSpeed
            chicken.transform.rotation = Quaternion.Slerp(chicken.transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * rotationSpeed);
        }
    }
}
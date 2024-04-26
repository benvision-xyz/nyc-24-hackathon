using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class CustomFingerColliderWithTable : MonoBehaviour
{
    private OVRSkeleton rightHandSkeleton, leftHandSkeleton, bodySkeleton;

    [SerializeField]
    private GameObject nest;
    private bool isHoldingBird = false;
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
    private float forwardForce = 0.7f;

    private bool isBirdOnNest = false;

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

    // Add these variables to your class to track the previous directions



    private float lastWingMovementTime = 0f;
    private float wingMovementFrequency = 0f;


bool birdOnNestFirst = false;
    bool leftTapDetected = false;
    bool rightTapDetected = false;
    bool isBirdNearNest  = false;

    private Animator chickenAnimator;
    //turn left
    float legMovementThreshold = 0.002f;
    private Vector3 lastChickLeftWingPosition;
    private Vector3 lastChickRightWingPosition;
    FMOD.Studio.EventInstance wingSoundInstance, walkSoundInstance, singSoundInstance, nestLoopSoundInstance;

    private Rigidbody chickenRigidbody;

    void SpawnChicken()
    {
        if (chicken == null)
        {
            Debug.LogError("Chicken prefab is not assigned.");
            return;
        }

        // Calculate spawn position: 2 meters in front of the user
           Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;
    // Set the bird's position to the new position
         chicken.transform.position = newPosition;
    }


    // void OnCollisionEnter(Collision collision)
    // {
    //     // Check if the bird is colliding with both hands
    //     if (collision.gameObject.tag == "Hand")
    //     {
    //         // You might need a more sophisticated way to ensure both hands are holding the bird
    //         nest.transform.position = Camera.main.transform.position + 0.3f*Camera.main.transform.forward;
    //         LeanTween.scale(nest, Vector3.one*0.015f, 0.5f).setEase(LeanTweenType.easeOutBack);
    //         isHoldingBird = true;
    //     }
    // }

    //  void OnCollisionExit(Collision collision)
    // {
    //     // Check if the bird is colliding with both hands
    //     if (collision.gameObject.tag == "Hand")
    //     {
    //         // You might need a more sophisticated way to ensure both hands are holding the bird
    //         LeanTween.scale(nest, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);
    //         isHoldingBird = true;
    //     }
    // }

    // void OnCollisionStay(Collision collision)
    // {
    //     // Check if the bird is on the nest and is relatively stable
    //     if (collision.gameObject.tag == "Nest" && isHoldingBird && chickenRigidbody.velocity.magnitude < 0.1f && !isBirdOnNest)
    //     {
    //         isBirdOnNest = true;
    //         // Scale the nest up to its original size (e.g., Vector3.one) or any desired size
    //         LeanTween.scale(nest, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
    //     }
    // }
    void Start()
    {

         chickenAnimator = chicken.GetComponent<Animator>();


        wingSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/wing");
        walkSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/walk");
        singSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/VO/Sing");
        nestLoopSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/VO/Nest");

        
        LeanTween.scale(nest, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);

          Invoke("SpawnChicken", 3f);
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
        // Assuming you have a way to get the positions of the hands and the chicken
        Vector3 rightHandPosition = rightHand.transform.position;
        Vector3 leftHandPosition = leftHand.transform.position;
        Vector3 chickenPosition = chicken.transform.position;

        // Define a threshold for how close the hands need to be to the chicken to be considered as holding it
        float holdThreshold = 0.2f; // Adjust this value as needed

        // Calculate the distances
        float distanceToRightHand = Vector3.Distance(rightHandPosition, chickenPosition);
        float distanceToLeftHand = Vector3.Distance(leftHandPosition, chickenPosition);
        float distaneBirdNest = Vector3.Distance(chickenPosition, nest.transform.position);
        if (distaneBirdNest < 0.2f && nest.transform.position.y < chickenPosition.y)
        {
            isBirdOnNest = true;
        }
        else{
            isBirdOnNest = false;
        }

 if (distaneBirdNest < 0.5f && nest.transform.position.y < chickenPosition.y)
        {
            isBirdNearNest = true;
        }
        else{
            isBirdNearNest = false;
        }

        if(isBirdOnNest && !birdOnNestFirst){

            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
            singSoundInstance.set3DAttributes(attributes);
            singSoundInstance.start();
            birdOnNestFirst = true;
        }

        // Check if either hand is within the threshold distance to the chicken
        if (distanceToRightHand < holdThreshold || distanceToLeftHand < holdThreshold)
        {
            if (!isHoldingBird)
            {
                // The bird is now being held
                isHoldingBird = true;
                // Show the nest by moving it to the chicken's position and scaling it up
                FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(nest.transform.position);
                nestLoopSoundInstance.set3DAttributes(attributes);
                nestLoopSoundInstance.start();
                nest.transform.position = Camera.main.transform.position + 0.5f * Vector3.forward;
                LeanTween.scale(nest, Vector3.one * 0.015f, 0.5f).setEase(LeanTweenType.easeOutBack);
            }

        }
        else if(!isBirdNearNest)
        {         // The bird is no longer being held
                isHoldingBird = false;
                // Hide the nest when the bird is not held
                LeanTween.scale(nest, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);
                nestLoopSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                birdOnNestFirst = false;
        }


        // The bird is no longer being held, check if it's placed back in the nest









        //reset table height precisely
        // if (leftIndexTipBone != null && leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        // {
        //     tableSurfaceHeight = leftIndexTipBone.Transform.position.y;
        // }

        if (leftHand != null)
        {
            tableSurfaceHeight = leftHand.transform.position.y +  0.03f;
            //
            //tableSurfaceHeight = CalculatePalmCenter().y;
        }

        float distanceBetweenHands = Vector3.Distance(leftHand.transform.position,rightHand.transform.position);   

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
        if ( distanceBetweenHands > 0.5 && (currentLeftWingPosition - lastChickLeftWingPosition).magnitude > wingThreshold || (currentRightWingPosition - lastChickRightWingPosition).magnitude > wingThreshold)
        {
            //   if (chickenAnimator.enabled) chickenAnimator.enabled = false; 

            debugTextViewMiddle.text = "Wing:" + wingMovementFrequency;
            float normalizedFrequency = MapFrequencyToNormalizedRange(wingMovementFrequency, 0f, 100f);

            // Assuming 10 is the max frequency you've observed

            // Create a parameter for FMOD

            wingSoundInstance.setParameterByName("Wing_Pitch", normalizedFrequency);
            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
            wingSoundInstance.set3DAttributes(attributes);
            wingSoundInstance.start();

            // Example usage within the Update method or wherever you're handling the FMOD event triggering
            float legsUpConstant = 0.05f;
                    chickRightLeg.transform.localPosition = new Vector3(
                    chickRightLegInitialPosition.x - legsUpConstant ,
                    chickRightLeg.transform.localPosition.y, // Adjust this formula as needed
                    chickRightLeg.transform.localPosition.z);

                     chickLeftLeg.transform.localPosition = new Vector3(
                    chickLeftLegInitialPosition.x -legsUpConstant,
                    chickLeftLeg.transform.localPosition.y, // Adjust this formula as needed
                    chickLeftLeg.transform.localPosition.z);
            wingMovementFrequency = 1f / timeSinceLastWingMovement; // Frequency is inverse of time
            lastWingMovementTime = Time.time;

            // Apply a continuous upward force based on the wing movement frequency
            float flyForce = Mathf.Lerp(0, 17f, wingMovementFrequency*0.7f); // Scale the force based on frequency
            if (isBirdOnNest || isHoldingBird)
            {

                flyForce = 0f;
            }


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
            if (isBirdOnNest || isHoldingBird)
            {
                directionalForce = Vector3.zero;
            }
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
            //   if (chickenAnimator.enabled) chickenAnimator.enabled = false;
            if (rightIndexTipBone.Transform.position.y >= tableSurfaceHeight - 0.05f && rightMiddleTipBone.Transform.position.y >= tableSurfaceHeight - 0.05f && rightIndexTipBone.Transform.position.y < tableSurfaceHeight + 0.05f && rightMiddleTipBone.Transform.position.y < tableSurfaceHeight + 0.05f)
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

                        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
                        walkSoundInstance.set3DAttributes(attributes);
                        walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
                        walkSoundInstance.start();

                        firstTapFlagRight = 0;
                        rightTapDetected = true;
                    }
                }

                else
                {
                    rightTapDetected = false;
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

                    debugTextViewIndex.text = "Leg: " + legMovementFrequency;
                    Debug.Log("Left index tip is below or at the table surface height!");
                    //debugTextViewMiddle.text = "Collided left";
                    if (firstTapFlagLeft == 1)
                    {

                        Debug.Log("First tap");

                        //debugTextViewIndex.text = "Leg:"+legMovementFrequency;
                        float normalizedFrequency = MapFrequencyToNormalizedRange(legMovementFrequency, 0f, 10f); // Assuming 10 is the max frequency you've observed
                        FMOD.Studio.EventInstance walkSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/walk");
                        walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
                        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(chicken.transform.position);
                        walkSoundInstance.set3DAttributes(attributes);
                        walkSoundInstance.start();
                        firstTapFlagLeft = 0;
                        leftTapDetected = true;
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

            if ((currentRightLegPosition - lastChickRightLegPosition).magnitude < legMovementThreshold && (currentLeftLegPosition - lastChickLeftLegPosition).magnitude > legMovementThreshold)
            {
                // Rotate chicken to left by 15 degrees
                RotateChicken(Quaternion.Euler(0, -15, 0) * chicken.transform.forward);
            }
            //turn right
            else if ((currentRightLegPosition - lastChickRightLegPosition).magnitude > legMovementThreshold && (currentLeftLegPosition - lastChickLeftLegPosition).magnitude < legMovementThreshold)
            {
                // Rotate chicken to right by 15 degrees
                RotateChicken(Quaternion.Euler(0, 15, 0) * chicken.transform.forward);
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

            if (isBirdOnNest || isHoldingBird)
            {
                walkingSpeed = 0f;
            }



            // Move the chicken forward based on walking speed
            Vector3 moveDirection = chicken.transform.forward * walkingSpeed * Time.deltaTime;
            chicken.transform.position += moveDirection;
        }

        if(walkingSpeed > 0f || wingMovementFrequency > 0f){
            if (chickenAnimator.enabled) chickenAnimator.enabled = false;
        }
        else{
             if (!chickenAnimator.enabled) chickenAnimator.enabled = true;
           
        }
        // 

       



        //fix the bird to always face parallel to XZ plane
        float currentYRotation = chicken.transform.eulerAngles.y;
        chicken.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);

        lastChickLeftLegPosition = currentLeftLegPosition;
        lastChickRightLegPosition = currentRightLegPosition;
        lastChickLeftWingPosition = currentLeftWingPosition;
        lastChickRightWingPosition = currentRightWingPosition;

        leftTapDetected = false;
        rightTapDetected = false;
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

    void OnDestroy()
    {
        walkSoundInstance.release();
        wingSoundInstance.release();
        nestLoopSoundInstance.release();

    }


    Vector3 CalculatePalmCenter()
{
     Vector3 thumbBase = Vector3.zero, middleBase = Vector3.zero, wristRoot = Vector3.zero;

    // Assuming you have already found the bones for leftHandSkeleton
    foreach (var bone in leftHandSkeleton.Bones)
    {
        switch (bone.Id)
        {
            case OVRSkeleton.BoneId.Hand_WristRoot:
                wristRoot = bone.Transform.position;
                break;
            case OVRSkeleton.BoneId.Hand_Thumb0:
                thumbBase = bone.Transform.position;
                break;
            case OVRSkeleton.BoneId.Hand_Middle1:
                middleBase = bone.Transform.position;
                break;
        }
    }

    // Calculate the plane
    
    return(wristRoot + middleBase) /2 ;
}


}
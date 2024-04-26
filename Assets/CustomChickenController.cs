using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomChickenController : MonoBehaviour
{
    // Start is called before the first frame update

    FMOD.Studio.EventInstance wingSoundInstance, walkSoundInstance;
    private OVRSkeleton rightHandSkeleton, leftHandSkeleton;
    [SerializeField]
    private GameObject chickLeftLeg, chickRightLeg, chickLeftWing, chickRightWing;
    private float flyingRotationSpeed = 5f;
    private OVRBone rightIndexTipBone, rightMiddleTipBone, leftIndexTipBone, leftWristRootBone;
    private OVRHand rightHand, leftHand;
        private Vector3 lastChickLeftWingPosition;
    private Vector3 lastChickRightWingPosition;

    private float lastWingMovementTime = 0f;
    private float wingMovementFrequency = 0f;
        private  float flyingForwardForce = 0.7f;

        private bool isHoldingBird = false;
        private bool isBirdOnNest = false;
      float wingThreshold = 0.004f;

        private bool previousTapIndex = false;
    private bool previousTapMiddle = false;
    bool indexTapDetected = false;
    bool middleTapDetected = false;
    [SerializeField]
    private TMP_Text debugTextView, debugTextView2;
    [SerializeField]
    private GameObject nest;
    private Vector3 chickLeftLegInitialPosition, chickRightLegInitialPosition, chickLeftWingInitialPosition, chickRightWingInitialPosition;

    private Rigidbody chickenRigidbody;
      private float headRotationFactor = 8.0f;
    //offset values
    private float offsetFactor = 0.02f;

    void InitChicken(){

        chickenRigidbody = GetComponent<Rigidbody>();   

        chickLeftLegInitialPosition = chickLeftLeg.transform.localPosition;
        chickRightLegInitialPosition = chickRightLeg.transform.localPosition;
        chickLeftWingInitialPosition = chickLeftWing.transform.localPosition;
        chickRightWingInitialPosition = chickRightWing.transform.localPosition;
        lastChickLeftWingPosition = chickLeftWing.transform.localPosition;
        lastChickRightWingPosition = chickRightWing.transform.localPosition;
    }

    void SetupFMODSounds(){
        wingSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/wing");
        walkSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Hackathon/Locomotion/walk");
    }
    void Start()
    {
       SetupSkeleton();
       SetupBones();
       InitChicken();
       SetupFMODSounds();
         LeanTween.scale(nest, Vector3.zero, 0f); 
       
    }


    // Update is called once per frame
    void Update()
{

    float distanceIndex = CalculatePalmCenter(leftHandSkeleton, rightIndexTipBone.Transform.position);
    float distanceMiddle = CalculatePalmCenter(leftHandSkeleton, rightMiddleTipBone.Transform.position);
    float distanceToBirdRight = Vector3.Distance(rightHand.transform.position,transform.position);
     float distanceToBirdLeft = Vector3.Distance(leftHand.transform.position,transform.position);
     float holdThreshold = 0.3f; // Adjust this value as needed
    if(distanceToBirdRight > holdThreshold && distanceToBirdLeft > holdThreshold && !isBirdOnNest){
            DetectTapsAndMoveChicken(distanceIndex,distanceMiddle);
            AnimateChickenLegsWithFingers(distanceIndex,distanceMiddle);
            AnimateChickenWingsWithHands();
            DetectFlapsAndFlyChicken(); ;
    }
 
 
    ActivateNestForEndGame(distanceToBirdRight, distanceToBirdLeft, holdThreshold);

    // Optional: Update debug text views
    debugTextView.text = "Index Distance: " + distanceIndex;
    debugTextView2.text = "Middle Distance: " + distanceMiddle;
}
void ActivateNestForEndGame(float distanceToBirdLeft, float distanceToBirdRight, float holdThreshold){
    // Check if either hand is within the threshold distance to the chicken
    if (distanceToBirdLeft < holdThreshold || distanceToBirdRight < holdThreshold)
    {
        if (!isHoldingBird)
        {
            // The bird is now being held
            isHoldingBird = true;
            // Show the nest by moving it to the chicken's position and scaling it up
            nest.transform.position = Camera.main.transform.position + 0.5f * transform.forward;
            LeanTween.scale(nest, Vector3.one * 0.015f, 0.5f).setEase(LeanTweenType.easeOutBack);
        }
    }
    else
    {
        if (isHoldingBird)
        {
            // The bird is no longer being held
            isHoldingBird = false;
            // Hide the nest when the bird is not held
            LeanTween.scale(nest, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);
        }
    }
}
void DetectFlapsAndFlyChicken(){
      // Calculate wing movement frequency
        float timeSinceLastWingMovement = Time.time - lastWingMovementTime;
        Vector3 currentLeftWingPosition = chickLeftWing.transform.localPosition;
        Vector3 currentRightWingPosition = chickRightWing.transform.localPosition;

        //flying mechanics
        if ((currentLeftWingPosition - lastChickLeftWingPosition).magnitude > wingThreshold || (currentRightWingPosition - lastChickRightWingPosition).magnitude > wingThreshold)
        {
            debugTextView.text = "Wing:"+wingMovementFrequency;
             float normalizedFrequency = MapFrequencyToNormalizedRange(wingMovementFrequency, 0f, 100f);

            // Assuming 10 is the max frequency you've observed

                        // Create a parameter for FMOD
           
            wingSoundInstance.setParameterByName("Wing_Pitch", normalizedFrequency);
            FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);
            wingSoundInstance.set3DAttributes(attributes);
            wingSoundInstance.start();

            // Example usage within the Update method or wherever you're handling the FMOD event triggering
           

            wingMovementFrequency = 1f / timeSinceLastWingMovement; // Frequency is inverse of time
            lastWingMovementTime = Time.time;

            // Apply a continuous upward force based on the wing movement frequency
            float flyForce = Mathf.Lerp(0, 15f, wingMovementFrequency ); // Scale the force based on frequency
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
        Vector3 directionalForce = transform.forward - (transform.right * normalizedHeadRoll * headRotationFactor);

        // The rest of your flying mechanics code
        // Adjust this value as needed for desired forward movement speed

        chickenRigidbody.velocity = new Vector3(directionalForce.x * flyingForwardForce, chickenRigidbody.velocity.y, directionalForce.z * flyingForwardForce);
          
          if (chickenRigidbody.velocity != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(chickenRigidbody.velocity.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * flyingRotationSpeed);
    }
        }


        //fix the bird to always face parallel to XZ plane
        float currentYRotation = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);

        lastChickLeftWingPosition = currentLeftWingPosition;
        lastChickRightWingPosition = currentRightWingPosition;
}


void FixedUpdate()
    { 
        ApplyCustomGravity();
    }

 void ApplyCustomGravity()
    {
    
        if (chickenRigidbody != null)
        {
            // Define your custom gravity force vector
            Vector3 customGravityForce = new Vector3(0, -5f, 0); // This is just normal Earth gravity, adjust as needed

            // Apply the custom gravity force
            chickenRigidbody.AddForce(customGravityForce * chickenRigidbody.mass); // Multiply by mass to get a force equivalent to gravity
        }
    }


void AnimateChickenWingsWithHands(){
     float leftYPosition = leftHand.transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetLeft = leftYPosition - Camera.main.transform.position.y;
            // Apply the offset with a multiplier for more or less exaggeration
            chickLeftWing.transform.localPosition = new Vector3(
                chickLeftWing.transform.localPosition.x,
                chickLeftWing.transform.localPosition.y, // Adjust multiplier as needed
                chickLeftWingInitialPosition.z - yOffsetLeft * 0.08f - offsetFactor);

     float rightYPosition = rightHand.transform.position.y;
            // Calculate the offset from the initial position
            float yOffsetRight = rightYPosition - Camera.main.transform.position.y;
            // Apply the offset with a multiplier for more or less exaggeration
            chickRightWing.transform.localPosition = new Vector3(
                chickRightWing.transform.localPosition.x,
                chickRightWing.transform.localPosition.y, // Adjust multiplier as needed
                chickRightWingInitialPosition.z + yOffsetRight * 0.08f + offsetFactor);
}

void AnimateChickenLegsWithFingers(float distanceIndex, float distanceMiddle){
    if(distanceIndex < 0.2){

         // Apply the difference to the localPosition.y of the right leg
            chickRightLeg.transform.localPosition = new Vector3(
            chickRightLegInitialPosition.x - 1f * distanceMiddle,
            chickRightLeg.transform.localPosition.y, // Adjust this formula as needed
            chickRightLeg.transform.localPosition.z);

            chickLeftLeg.transform.localPosition = new Vector3(
            chickLeftLegInitialPosition.x - 1f * distanceIndex,
            chickLeftLeg.transform.localPosition.y, // Adjust this formula as needed
            chickLeftLeg.transform.localPosition.z);

    }
           
}
void DetectTapsAndMoveChicken(float distanceIndex, float distanceMiddle){

    float tapThreshold = 0.025f;
    bool currentTapIndex = distanceIndex <= tapThreshold;
    bool currentTapMiddle = distanceMiddle <= tapThreshold;

    // Index Finger Tap Detection
    if (currentTapIndex && !indexTapDetected)
    {
        Debug.Log("Tap detected between right index finger and left palm");
        indexTapDetected = true;
        float normalizedFrequency = MapFrequencyToNormalizedRange(5f, 0f, 10f); // Assuming 10 is the max frequency you've observe               // Create a parameter for FMOD
        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);
        walkSoundInstance.set3DAttributes(attributes);
        walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
        walkSoundInstance.start();
        if (previousTapMiddle) {
            WalkChickenForward();
        } else {
             RotateChicken(Quaternion.Euler(0, -30, 0) * transform.forward);
        }
    }
    else if (!currentTapIndex && indexTapDetected)
    {
        indexTapDetected = false;
    }

    // Middle Finger Tap Detection
    if (currentTapMiddle && !middleTapDetected)
    {
        Debug.Log("Tap detected between right middle finger and left palm");
        float normalizedFrequency = MapFrequencyToNormalizedRange(5f, 0f, 10f); // Assuming 10 is the max frequency you've observe               // Create a parameter for FMOD
        FMOD.ATTRIBUTES_3D attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform.position);
        walkSoundInstance.set3DAttributes(attributes);
        walkSoundInstance.setParameterByName("Walk_Pitch", normalizedFrequency);
        walkSoundInstance.start();
        middleTapDetected = true;
        if (previousTapIndex) {
            WalkChickenForward();
        } else {
            RotateChicken(Quaternion.Euler(0, 30, 0) * transform.forward);
        }
    }
    else if (!currentTapMiddle && middleTapDetected)
    {
        middleTapDetected = false;
    }

    // Update previous taps at the end of the method
    previousTapIndex = indexTapDetected;
    previousTapMiddle = middleTapDetected;
}

void WalkChickenForward() {
    // Implement the logic to move the chicken forward
    // For example, moving the chicken forward by a fixed amount
    transform.Translate(Vector3.forward * 0.05f);
}

void TurnChicken(float degrees) {
    // Implement the logic to turn the chicken left by a certain degree
    transform.Rotate(Vector3.up, degrees);
    transform.Translate(Vector3.forward * 0.01f);
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
            Vector3 currentRotation = transform.rotation.eulerAngles;
            // Create a new Vector3 for the rotation, combining the target Y rotation with the current X and Z rotations set to 0
            Vector3 newRotation = new Vector3(0, targetYRotation, 0);

            // Define a rotation speed (degrees per second)
            float rotationSpeed = 20f; // Adjust this value as needed

            // Smoothly rotate the chicken towards the target direction, preserving the upright orientation
            // Adjust the interpolation speed based on Time.deltaTime and rotationSpeed
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime * rotationSpeed);
        }


    }


private float MapFrequencyToNormalizedRange(float value, float min, float max)
{
    // Ensure the value is within the bounds of min and max
    value = Mathf.Clamp(value, min, max);
    // Map the value from [min, max] to [0, 1]
    return (value - min) / (max - min);
}













//Setup initial
    void SetupBones(){

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
        if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
        {
            leftWristRootBone = bone;
            break;
        }
        if (bone.Id == OVRSkeleton.BoneId.Hand_WristRoot)
        {
            leftWristRootBone = bone;
            break;
        }
    }

    }

    float DistanceFromPointToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
{
    return Mathf.Abs(Vector3.Dot(planeNormal, point - planePoint));
}

    float CalculatePalmCenter(OVRSkeleton skeleton, Vector3 vector3)
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
    Vector3 v1 = thumbBase - wristRoot;
    Vector3 v2 = middleBase - wristRoot;
    Vector3 planeNormal = Vector3.Cross(v1, v2).normalized;
    float distance = DistanceFromPointToPlane(vector3, wristRoot, planeNormal);
    return distance;
}

    void SetupSkeleton(){
        OVRHand[] hands = FindObjectsOfType<OVRHand>();


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

            OVRSkeleton[] skeletons = FindObjectsOfType<OVRSkeleton>();
                    foreach (var skeleton in skeletons)
                    {
                         if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
                        {
                            rightHandSkeleton = skeleton;

                        }

                         if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft)
                        {
                            leftHandSkeleton = skeleton;

                        }
                    }
    }


    void OnDestroy(){
              walkSoundInstance.release();
              wingSoundInstance.release();
              
        }
}



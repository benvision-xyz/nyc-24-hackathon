using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class CustomFingerColliderWithTable : MonoBehaviour
{
    private OVRSkeleton rightHandSkeleton;
    private OVRBone indexTipBone, middleTipBone;
    public float tableSurfaceHeight = 0.69f; // Set this to the Y position of your table surface


    [SerializeField]
    private TMP_Text debugTextViewIndex, debugTextViewMiddle;
    void Start()
    {
        // Automatically find the right hand OVRSkeleton
        OVRSkeleton[] skeletons = FindObjectsOfType<OVRSkeleton>();
        foreach (var skeleton in skeletons)
        {
            if (skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight)
            {
                rightHandSkeleton = skeleton;
                break;
            }
        }

        if (rightHandSkeleton == null)
        {
            debugTextViewIndex.text = "Right hand OVRSkeleton not found.";
            Debug.LogError("Right hand OVRSkeleton not found.");
            return;
        }

        // Find the index tip bone
        foreach (var bone in rightHandSkeleton.Bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                indexTipBone = bone;
                //break;
            }
             if (bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
            {
                middleTipBone = bone;
               // break;
            }

        }

    }

    void Update()
    {
        if (indexTipBone != null)
        {
            // Check if the Y position of the index tip is less than or equal to the table surface height
            if (indexTipBone.Transform.position.y <= tableSurfaceHeight)
            {
                Debug.Log("Index tip is below or at the table surface height!");
                 debugTextViewIndex.text = "Collided index";
              
        }
          
                else{
                       debugTextViewIndex.text = "Not collided";
                }
    }

     if (middleTipBone != null)
        {
            // Check if the Y position of the index tip is less than or equal to the table surface height
            if (middleTipBone.Transform.position.y <= tableSurfaceHeight)
            {
                Debug.Log("Index tip is below or at the table surface height!");
                 debugTextViewMiddle.text = "Collided middle";
              
        }
          
                else{
                       debugTextViewMiddle.text = "Not collided";
                }
    }

}
}
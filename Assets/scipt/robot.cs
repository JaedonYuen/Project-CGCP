using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//authored by jaedon yuen (matric no: 2402335J)
//this is a modular system that allows for you to make a set of poses that can be used to animate a robot

[Serializable]
public class transformValue 
{
    public GameObject target; 
    public Vector3 rotation;
    public Vector3 position;
    public Vector3 startPosition;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

    public bool applyRotation = true;
    public bool applyPosition = true;
}
[Serializable]
public class pose 
{
    public string name;
    public List<transformValue> transformValues = new List<transformValue>();

    
}
[Serializable]

public class robotAnimation 
{
    public string name;
    public bool loop = false;
    public List<pose> poses = new List<pose>();

    
}

public class robot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public List<robotAnimation> animations = new List<robotAnimation>();
    public string targetAnimation = "";
    
    private pose targetPose;
    void Start()
    {
        // intialize all the poses in the robotAnimation class to have the start position of the robot
        foreach (robotAnimation ra in animations)
        {
            foreach (pose p in ra.poses)
            {
                foreach (transformValue tv in p.transformValues)
                {
                    tv.startPosition = tv.target.transform.localPosition;
                }
            }
        }
    }


    //ball park functions allow for a tolerance to be set for comparisons since slerp isnt exactly accurate
    bool ballParkRotation(Quaternion a, Quaternion b, float tolerance)
    {
        //check if the two vectors are within the tolerance of each other
        return Quaternion.Angle(a, b) < tolerance;
    }

    bool ballParkPostion(Vector3 a, Vector3 b, float tolerance)
    {
        //check if the two vectors are within the tolerance of each other
        return Vector3.Distance(a, b) < tolerance;
    }

    // Update is called once per frame
    void Update()
    {
        // workflow:
        // check if a target animation is set
        // go through all the poses in that animation
        // init the target pose to the first pose of the animation
        // check if the robot has achived the pose for each translation
        // if so, we set bools to tell the rest of the code that we have achieved the pose
        // if we have achieved the pose, we move to the next pose in the animation
        // if there are no more poses, check if the loop bool is set to true
        // if so, we can go and set it back to the beginning
        // else, we just set it to null and stop the animation
        if (targetAnimation != "")
        {
            robotAnimation roboAnim = animations.Find(x => x.name == targetAnimation);
            if (roboAnim != null)
            {
                // Initialize the target pose to the first pose in the animation
                if (targetPose == null)
                {
                    targetPose = roboAnim.poses[0];
                }

                // Aggregate flags for the entire pose
                bool poseAchievedRotation = true;
                bool poseAchievedPosition = true;

                foreach (transformValue tv in targetPose.transformValues)
                {
                    // Check if the robot has achieved the pose
                    bool achievedRotation = !tv.applyRotation;
                    bool achievedPosition = !tv.applyPosition;

                    if (tv.applyRotation)
                    {
                        if (!ballParkRotation(tv.target.transform.localRotation, Quaternion.Euler(tv.rotation), 0.05f))
                        {
                            // If not, lerp to the target pose
                            tv.target.transform.localRotation = Quaternion.Slerp(tv.target.transform.localRotation, Quaternion.Euler(tv.rotation), Time.deltaTime * tv.rotationSpeed);
                            
                        }
                        else
                        {
                            achievedRotation = true;
                        }
                    }

                    if (tv.applyPosition)
                    {
                        if (!ballParkPostion(tv.target.transform.localPosition, tv.position, 0.05f))
                        {
                            // If not, lerp to the target pose
                            tv.target.transform.localPosition = Vector3.Slerp(tv.target.transform.localPosition, tv.position, Time.deltaTime * tv.moveSpeed);
                            
                        }
                        else
                        {
                            achievedPosition = true;
                        }
                    }

                    // Update the aggregate flags
                    poseAchievedRotation &= achievedRotation;
                    poseAchievedPosition &= achievedPosition;
                }

                // Check if the entire pose is achieved
                if (poseAchievedRotation && poseAchievedPosition)
                {
                    Debug.Log("achieved pose: " + targetPose.name);
                    // Move to the next pose
                    int index = roboAnim.poses.IndexOf(targetPose);
                    if (index < roboAnim.poses.Count - 1)
                    {
                        targetPose = roboAnim.poses[index + 1];
                    }
                    else
                    {
                        if (roboAnim.loop)
                        {
                            // If the animation is looping, go back to the first pose
                            targetPose = roboAnim.poses[0];
                        }
                        else
                        {
                            // If the animation is not looping, stop the animation
                            targetPose = null;
                            targetAnimation = ""; // Reset the target animation
                            Debug.Log("Animation completed: " + roboAnim.name);
                        }
                    }

                    
                }
                else
                {
                    Debug.Log("not achieved pose: " + targetPose.name + ", Results: " + "achievedRotation: " + poseAchievedRotation + ", achievedPosition: " + poseAchievedPosition);
                }
            }
            else
            {
                // Reset the robot to the start position if the animation is not found
                foreach (robotAnimation ra in animations)
                {
                    foreach (pose p in ra.poses)
                    {
                        foreach (transformValue tv in p.transformValues)
                        {
                            if (tv.applyRotation)
                            {
                                // Lerp to the start rotation
                                tv.target.transform.localRotation = Quaternion.Slerp(tv.target.transform.localRotation, Quaternion.Euler(Vector3.zero), Time.deltaTime * tv.rotationSpeed);
                            }
                            if (tv.applyPosition)
                            {
                                tv.target.transform.localPosition = Vector3.Slerp(tv.target.transform.localPosition, tv.startPosition, Time.deltaTime * tv.moveSpeed);
                            }
                        }
                    }
                }
            }
        }

    }

    
}

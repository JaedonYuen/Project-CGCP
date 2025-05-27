using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;



[Serializable]
public class TransformValue 
{
    public GameObject target; 
    public Vector3 rotation;
    public Vector3 position;
    public Vector3 startPosition;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public bool useSlerp = true;

    public bool applyRotation = false;
    public bool applyPosition = false;
}
[Serializable]
public class Pose 
{
    public string name;
    public List<TransformValue> TransformValues = new List<TransformValue>();

    
}
[Serializable]

public class RobotAnimation 
{
    public string name;
    public bool loop = false;
    public List<Pose> Poses = new List<Pose>();

    
}

public class robot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public List<RobotAnimation> animations = new List<RobotAnimation>();

    public bool isPlaying = true;
    public string targetAnimation = "";

    public float rotationSpeedMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;
    
    private Pose targetPose;
    private List<GameObject> targetsInUse = new List<GameObject>();

    private bool needToRest = false;
    private string animationAfterResting = "";

    //another odd thing about unity and math in general is that slerp, which is what i use to make smooth animations, isnt accurate to some degree, which makes my code a broken, but luckily i can just make these funtions to sort of give my code tolerance.
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

    void Awake()
    {
        // -- ANIMATONS -- //
        // i felt like storing in the values in a table would make the code a bit more modular, allowing for anything to be animated.
        // make some animations, we do this first so that we can see the used limbs when adding the rest animation
        // fun fact, previously i would have used the unity table system but i felt that it wasnt very "code in the animations" so i just ported them here.
        // walking
        RobotAnimation walkingAnimation = new RobotAnimation(); // create main animation
        walkingAnimation.name = "walk";
        walkingAnimation.loop = true;

        Pose walkPose1 = new Pose(); //pose 1, raise left leg, kick right leg back
        walkPose1.name = "walkP1";

        TransformValue walkP1T1 = new TransformValue(); //raise upper left leg up
        walkP1T1.target = GameObject.Find("UpperLeftLeg");
        walkP1T1.rotation = new Vector3(40, 0, 0);
        walkP1T1.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1T1);

        TransformValue walkP1T2 = new TransformValue(); //droop the lower left leg down
        walkP1T2.target = GameObject.Find("LowerLeftLeg");
        walkP1T2.rotation = new Vector3(-40, 0, 0);
        walkP1T2.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1T2);

        TransformValue walkP1T3 = new TransformValue(); //kick back the right leg
        walkP1T3.target = GameObject.Find("UpperRightLeg");
        walkP1T3.rotation = new Vector3(-40, 0, 0);
        walkP1T3.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1T3);

        TransformValue walkP1T4 = new TransformValue(); 
        walkP1T4.target = GameObject.Find("LowerRightLeg");
        walkP1T4.rotation = new Vector3(0, 0, 0);
        walkP1T4.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1T4);

        walkingAnimation.Poses.Add(walkPose1); //add the pose

        Pose walkPose2 = new Pose(); //pose2, kick back left leg, raise right leg

        TransformValue walkP2T1 = new TransformValue(); //raise upper right leg up
        walkP2T1.target = GameObject.Find("UpperRightLeg");
        walkP2T1.rotation = new Vector3(40, 0, 0);
        walkP2T1.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2T1);

        TransformValue walkP2T2 = new TransformValue(); //droop the lower right leg down
        walkP2T2.target = GameObject.Find("LowerRightLeg");
        walkP2T2.rotation = new Vector3(-40, 0, 0);
        walkP2T2.applyRotation = true;
        walkPose2.TransformValues.Add(walkP1T2);

        TransformValue walkP2T3 = new TransformValue(); //kick back the left leg
        walkP2T3.target = GameObject.Find("UpperLeftLeg");
        walkP2T3.rotation = new Vector3(-40, 0, 0);
        walkP2T3.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2T3);

        TransformValue walkP2T4 = new TransformValue(); 
        walkP2T4.target = GameObject.Find("LowerLeftLeg");
        walkP2T4.rotation = new Vector3(0, 0, 0);
        walkP2T4.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2T4);

        walkingAnimation.Poses.Add(walkPose2); //add the pose

        
        animations.Add(walkingAnimation);
    }

    void Start()
    {
        
        

        //for some odd reason my code doesnt like forcibliy transforming the limbs back so a loophole i found is i make a default rest animation that diffrent limbs can fall back to
        foreach (RobotAnimation robotAnimation in animations)
        {
            foreach (Pose p in robotAnimation.Poses)
            {
                foreach (TransformValue transform in p.TransformValues)
                {
                    transform.startPosition = transform.target.transform.localPosition;
                    //add target to the list of targets in use if it is not already in the list
                    if (!targetsInUse.Contains(transform.target))
                    {
                        targetsInUse.Add(transform.target);
                    }
                }
            }
        }
        //we should initialize to prevent odd things at the start
        Pose restingPose = new Pose();
        restingPose.name = "rest";
        foreach (GameObject target in targetsInUse)
        {
            TransformValue transform = new TransformValue();
            transform.target = target;
            transform.startPosition = target.transform.localPosition;
            transform.rotation = Vector3.zero;
            transform.position = target.transform.localPosition;
            transform.applyRotation = true;
            transform.applyPosition = true;
            transform.rotationSpeed = 50f;
            restingPose.TransformValues.Add(transform);
        }
        RobotAnimation restingAnimation = new RobotAnimation();
        restingAnimation.name = "rest";
        restingAnimation.loop = true;
        restingAnimation.Poses.Add(restingPose);
        animations.Add(restingAnimation);

        





        
    }


    
    // Update is called once per frame
    void Update()
    {
        // da workflow:
        // check if a target animation is set
        // go through all the Poses in that animation
        // init the target Pose to the first Pose of the animation
        // check if the robot has achived the Pose for each translation
        // if so, we set bools to tell the rest of the code that we have achieved the Pose
        // if we have achieved the Pose, we move to the next Pose in the animation
        // if there are no more Poses, check if the loop bool is set to true
        // if so, we can go and set it back to the beginning
        // else, we just set it to null and stop the animation
        //setting it to null will HOPEFULLY make it rest, as we will make sure that the null value gets changed into the rest anim

        //help me this took like 3 days 
        if (isPlaying)
        {
            if (targetAnimation != "")
            {
                RobotAnimation roboAnim = animations.Find(x => x.name == targetAnimation);
                if (roboAnim != null)
                {
                    if (needToRest)
                    {
                        // Reset the robot to the start position if the animation is not found
                        roboAnim = animations.Find(x => x.name == "rest");
                        targetAnimation = "rest"; // rest is teh rest animation
                    }
                    // Initialize the target Pose to the first Pose in the animation
                    if (targetPose == null)
                    {
                        targetPose = roboAnim.Poses[0];
                    }

                    // Aggregate flags for the entire Pose
                    bool PoseAchievedRotation = true;
                    bool PoseAchievedPosition = true;

                    foreach (TransformValue tv in targetPose.TransformValues)
                    {
                        // Check if the robot has achieved the Pose
                        bool achievedRotation = !tv.applyRotation;
                        bool achievedPosition = !tv.applyPosition;

                        // check if the animation changed midway through
                        if (targetAnimation != roboAnim.name)
                        {
                            // Reset the robot to the start position if the animation is not found
                            targetAnimation = "rest"; // Reset the target animation
                            resetPose();
                            return;
                        }

                        if (tv.applyRotation)
                        {
                            if (!ballParkRotation(tv.target.transform.localRotation, Quaternion.Euler(tv.rotation), 0.05f))
                            {
                                // If not, lerp to the target Pose
                                if (tv.useSlerp)
                                {
                                    tv.target.transform.localRotation = Quaternion.Slerp(tv.target.transform.localRotation, Quaternion.Euler(tv.rotation), Time.deltaTime * tv.rotationSpeed * rotationSpeedMultiplier);
                                }
                                else
                                {
                                    tv.target.transform.localRotation = Quaternion.RotateTowards(tv.target.transform.localRotation, Quaternion.Euler(tv.rotation), Time.deltaTime * tv.rotationSpeed * rotationSpeedMultiplier);
                                }

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
                                // If not, lerp to the target Pose
                                if (tv.useSlerp)
                                {
                                    tv.target.transform.localPosition = Vector3.Slerp(tv.target.transform.localPosition, tv.position, Time.deltaTime * tv.moveSpeed * moveSpeedMultiplier);
                                }
                                else
                                {
                                    tv.target.transform.localPosition = Vector3.MoveTowards(tv.target.transform.localPosition, tv.position, Time.deltaTime * tv.moveSpeed * moveSpeedMultiplier);
                                }

                            }
                            else
                            {
                                achievedPosition = true;
                            }
                        }

                        // Update the aggregate flags
                        PoseAchievedRotation &= achievedRotation;
                        PoseAchievedPosition &= achievedPosition;
                    }

                    // Check if the entire Pose is achieved
                    if (PoseAchievedRotation && PoseAchievedPosition)
                    {
                        //Debug.Log("Animation: "+ roboAnim.name + " achieved Pose: " + targetPose.name);
                        if (needToRest)
                        {
                            needToRest = false;
                            targetAnimation = animationAfterResting; // Set the target animation to the one after resting
                            animationAfterResting = ""; // Reset the animation after resting
                        }
                        // Move to the next Pose
                        int index = roboAnim.Poses.IndexOf(targetPose);
                        if (index < roboAnim.Poses.Count - 1)
                        {
                            targetPose = roboAnim.Poses[index + 1];
                        }
                        else
                        {
                            if (roboAnim.loop)
                            {
                                // If the animation is looping, go back to the first Pose
                                targetPose = roboAnim.Poses[0];
                            }
                            else
                            {
                                // If the animation is not looping, stop the animation
                                targetPose = null;
                                targetAnimation = ""; // Reset the target animation
                                //Debug.Log("Animation completed: " + roboAnim.name);
                            }
                        }


                    }
                    else
                    {
                        //Debug.Log( "Animation: "+ roboAnim.name + " not achieved Pose: " + targetPose.name + ", Results: " + "achievedRotation: " + PoseAchievedRotation + ", achievedPosition: " + PoseAchievedPosition);
                    }
                }
                else
                {
                    // Reset the robot to the start position if the animation is not found
                    targetAnimation = "rest"; // Reset the target animation


                }
            }
            else
            {
                // Reset the robot to the start position if no animation is set
                targetAnimation = "rest";
            }
        }

    }

    void resetPose()
    {
        foreach(GameObject target in targetsInUse)
        {
            // Reset the target to its start position
            //target.transform.localPosition = target.transform.localPosition;
            target.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    public void setTargetAnimation(string animationName)
    {

        // Set the target animation to the specified name
        needToRest = true;

        animationAfterResting = animationName;
    }

    
}

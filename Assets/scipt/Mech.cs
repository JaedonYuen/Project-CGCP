using UnityEngine;
using System.Collections.Generic;
using System;


// This script uses tables and custom classes to store varables, and the script will parse the animations and animate it.

// Animations essenially sets targets for diffrent limbs to move to, and the core of ths script will dynamically animate based on these targets i set with the animations, allowing me to dynamically change targets on the fly, allowing me to dynamically add, remove, and change andimations with out issue.

// these classes help store and organize the animation data
[Serializable]
public class TransformValue // transform values help store values as well as how to animate them
{
    public GameObject target; 
    public Vector3 rotation;
    public Vector3 position;
    public Vector3 startPosition;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public bool interpolate = true;

    public bool applyRotation = false;
    public bool applyPosition = false;
}
[Serializable]
public class Pose // poses just store transform values
{
    public string name;
    public List<TransformValue> TransformValues = new List<TransformValue>();

    
}
[Serializable]

public class MechAnimation // robot animations store poses and give it a name, allowing for nice organization, because I im already very disorganized. It also determins if an animation should loop or not.
{
    public string name;
    public bool loop = false;
    public List<Pose> Poses = new List<Pose>();

    
}

public class Mech : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public List<MechAnimation> animations = new List<MechAnimation>();

    public bool isPlaying = true;
    private string targetAnimation = "";

    private float rotationSpeedMultiplier = 1f;
    private float moveSpeedMultiplier = 1f;

    private Pose targetPose;
    private List<GameObject> targetsInUse = new List<GameObject>();

    private bool needToRest = false;
    private string animationAfterResting = "";

    private float poseTolerance = .5f;

    //another odd thing about unity and math in general is that slerp, which is what i use to make smooth animations, isnt accurate to some degree, which makes my code a broken, but luckily i can just make these funtions to sort of give my code tolerance.
    bool ballParkRotation(Quaternion a, Quaternion b)
    {
        //check if the two vectors are within the tolerance of each other
        return Quaternion.Angle(a, b) < poseTolerance;
    }

    bool ballParkPostion(Vector3 a, Vector3 b)
    {
        //check if the two vectors are within the tolerance of each other
        return Vector3.Distance(a, b) < poseTolerance;
    }
    // -- ANIMATONS -- //
    // i felt like storing in the values in a table would make the code a bit more modular, allowing for anything to be animated.
    // make some animations, we do this first so that we can see the used limbs when adding the rest animation
    void Awake()
    {

        // walking // 
        MechAnimation walkingAnimation = new MechAnimation(); // create main animation
        walkingAnimation.name = "walk";
        walkingAnimation.loop = true;

        Pose walkPose1 = new Pose(); // pose 1, raise left leg, kick right leg back
        walkPose1.name = "walkP1";

        TransformValue walkP1ULLeg = new TransformValue(); // raise upper left leg up
        walkP1ULLeg.target = GameObject.Find("UpperLeftLeg");
        walkP1ULLeg.rotation = new Vector3(40, 0, 0);
        walkP1ULLeg.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1ULLeg);

        TransformValue walkP1LLLeg = new TransformValue(); // droop the lower left leg down
        walkP1LLLeg.target = GameObject.Find("LowerLeftLeg");
        walkP1LLLeg.rotation = new Vector3(-40, 0, 0);
        walkP1LLLeg.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1LLLeg);

        TransformValue walkP1URLeg = new TransformValue(); // kick back the right leg
        walkP1URLeg.target = GameObject.Find("UpperRightLeg");
        walkP1URLeg.rotation = new Vector3(-40, 0, 0);
        walkP1URLeg.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1URLeg);

        TransformValue walkP1LRLeg = new TransformValue();
        walkP1LRLeg.target = GameObject.Find("LowerRightLeg");
        walkP1LRLeg.rotation = new Vector3(0, 0, 0);
        walkP1LRLeg.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1LRLeg);


        TransformValue walkP1Head = new TransformValue(); // turn head to look forward
        walkP1Head.target = GameObject.Find("Head");
        walkP1Head.rotation = new Vector3(0, 30, 0);
        walkP1Head.applyRotation = true;

        // next set of values just make the arms a bit less stiff looking 
        TransformValue walkP1RShoulder = new TransformValue();
        walkP1RShoulder.target = GameObject.Find("RightShoulder");
        walkP1RShoulder.rotation = new Vector3(-30, 0, -5);
        walkP1RShoulder.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1RShoulder);

        TransformValue walkP1RBicep = new TransformValue();
        walkP1RBicep.target = GameObject.Find("RightBicep");
        walkP1RBicep.rotation = new Vector3(0, -5, 0);
        walkP1RBicep.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1RBicep);

        TransformValue walkP1RForarm = new TransformValue();
        walkP1RForarm.target = GameObject.Find("RightForearm");
        walkP1RForarm.rotation = new Vector3(5, 0, 0);
        walkP1RForarm.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1RForarm);

        TransformValue walkP1LShoulder = new TransformValue();
        walkP1LShoulder.target = GameObject.Find("LeftShoulder");
        walkP1LShoulder.rotation = new Vector3(30, 0, 5);
        walkP1LShoulder.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1LShoulder);

        TransformValue walkP1LBicep = new TransformValue();
        walkP1LBicep.target = GameObject.Find("LeftBicep");
        walkP1LBicep.rotation = new Vector3(0, 5, 0);
        walkP1LBicep.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1LBicep);

        TransformValue walkP1LForearm = new TransformValue();
        walkP1LForearm.target = GameObject.Find("LeftForearm");
        walkP1LForearm.rotation = new Vector3(5, 0, 0);
        walkP1LForearm.applyRotation = true;
        walkPose1.TransformValues.Add(walkP1LForearm);

        walkingAnimation.Poses.Add(walkPose1); // add the pose

        Pose walkPose2 = new Pose(); // pose 2, raise right leg, kick left leg back
        walkPose2.name = "walkP2";
        TransformValue walkP2URLeg = new TransformValue(); // raise upper right leg up
        walkP2URLeg.target = GameObject.Find("UpperRightLeg");
        walkP2URLeg.rotation = new Vector3(40, 0, 0);
        walkP2URLeg.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2URLeg);

        TransformValue walkP2LRLeg = new TransformValue(); // droop the lower right leg down
        walkP2LRLeg.target = GameObject.Find("LowerRightLeg");
        walkP2LRLeg.rotation = new Vector3(-40, 0, 0);
        walkP2LRLeg.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2LRLeg);

        TransformValue walkP2ULLeg = new TransformValue(); // kick back the left leg
        walkP2ULLeg.target = GameObject.Find("UpperLeftLeg");
        walkP2ULLeg.rotation = new Vector3(-40, 0, 0);
        walkP2ULLeg.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2ULLeg);

        TransformValue walkP2LLLeg = new TransformValue();
        walkP2LLLeg.target = GameObject.Find("LowerLeftLeg");
        walkP2LLLeg.rotation = new Vector3(0, 0, 0);
        walkP2LLLeg.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2LLLeg);

        TransformValue walkP2Head = new TransformValue(); // turn head to look forward
        walkP2Head.target = GameObject.Find("Head");
        walkP2Head.rotation = new Vector3(0, -30, 0);
        walkP2Head.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2Head);

        TransformValue walkP2RShoulder = new TransformValue(); // swing the arms
        walkP2RShoulder.target = GameObject.Find("RightShoulder");
        walkP2RShoulder.rotation = new Vector3(30, 0, 5);
        walkP2RShoulder.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2RShoulder);

        TransformValue walkP2LShoulder = new TransformValue();
        walkP2LShoulder.target = GameObject.Find("LeftShoulder");
        walkP2LShoulder.rotation = new Vector3(-30, 0, 5);
        walkP2LShoulder.applyRotation = true;
        walkPose2.TransformValues.Add(walkP2LShoulder);

        walkingAnimation.Poses.Add(walkPose2); // add the pose

        animations.Add(walkingAnimation); // add the animation

        // Punching //
        MechAnimation punchingAnimation = new MechAnimation(); // create main animation
        punchingAnimation.name = "punch";
        punchingAnimation.loop = false;

        Pose punchPose1 = new Pose(); //pose 1, raise left arm up
        punchPose1.name = "punchP1";

        TransformValue punchP1RShoulder = new TransformValue(); // raise upper right arm up
        punchP1RShoulder.target = GameObject.Find("RightShoulder");
        punchP1RShoulder.rotation = new Vector3(90, 100, 60);
        punchP1RShoulder.applyRotation = true;
        punchPose1.TransformValues.Add(punchP1RShoulder);

        TransformValue punchP1RBicep = new TransformValue(); // raise right bicep up
        punchP1RBicep.target = GameObject.Find("RightBicep");
        punchP1RBicep.rotation = new Vector3(0, -90, 0);
        punchP1RBicep.applyRotation = true;
        punchPose1.TransformValues.Add(punchP1RBicep);

        TransformValue punchP1RForarm = new TransformValue(); // raise right forearm up
        punchP1RForarm.target = GameObject.Find("RightForearm");
        punchP1RForarm.rotation = new Vector3(100, 0, 0);
        punchP1RForarm.applyRotation = true;
        punchPose1.TransformValues.Add(punchP1RForarm);

        TransformValue punchP1LTorso = new TransformValue(); // turn lower torso
        punchP1LTorso.target = GameObject.Find("LowerTorso");
        punchP1LTorso.rotation = new Vector3(0, 35, 0);
        punchP1LTorso.applyRotation = true;
        punchPose1.TransformValues.Add(punchP1LTorso);

        TransformValue punchP1Head = new TransformValue(); // turn head to look forward again
        punchP1Head.target = GameObject.Find("Head");
        punchP1Head.rotation = new Vector3(0, -35, 0);
        punchP1Head.applyRotation = true;
        punchPose1.TransformValues.Add(punchP1Head);

        punchingAnimation.Poses.Add(punchPose1); // add the pose

        float punchSpeed = 50f; // set the speed to a faster speed so it looks like a punch

        Pose punchPose2 = new Pose(); // pose 2, punch right arm forward   
        punchPose2.name = "punchP2";
        TransformValue punchP2RShoulder = new TransformValue(); // punch right shoulder forward
        punchP2RShoulder.target = GameObject.Find("RightShoulder");
        punchP2RShoulder.rotation = new Vector3(90, 100, 60);
        punchP2RShoulder.applyRotation = true;
        punchP2RShoulder.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2RShoulder);

        TransformValue punchP2RBicep = new TransformValue(); // punch right bicep forward
        punchP2RBicep.target = GameObject.Find("RightBicep");
        punchP2RBicep.rotation = new Vector3(0, -90, 0);
        punchP2RBicep.applyRotation = true;
        punchP2RBicep.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2RBicep);

        TransformValue punchP2RForarm = new TransformValue(); // punch right forearm forward
        punchP2RForarm.target = GameObject.Find("RightForearm");
        punchP2RForarm.rotation = new Vector3(0, 0, 0);
        punchP2RForarm.applyRotation = true;
        punchP2RForarm.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2RForarm);

        TransformValue punchP2LTorso = new TransformValue(); // turn lower torso
        punchP2LTorso.target = GameObject.Find("LowerTorso");
        punchP2LTorso.rotation = new Vector3(0, -35, 0);
        punchP2LTorso.applyRotation = true;
        punchP2LTorso.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2LTorso);

        TransformValue punchP2Head = new TransformValue(); // turn head to look forward again
        punchP2Head.target = GameObject.Find("Head");
        punchP2Head.rotation = new Vector3(0, 35, 0);
        punchP2Head.applyRotation = true;
        punchP2Head.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2Head);

        TransformValue punchP2Hips = new TransformValue(); //move the robot forward for that extra punch effect
        punchP2Hips.target = GameObject.Find("Hips");
        punchP2Hips.position = new Vector3(0, 17.25f, -10f); // move the hips forward
        punchP2Hips.applyPosition = true;
        punchP2Hips.moveSpeed = punchSpeed; // set the speed to the same speed as the punch
        punchPose2.TransformValues.Add(punchP2Hips);

        TransformValue punchP2ULLegs = new TransformValue(); // turn the legs for a more dynamic look
        punchP2ULLegs.target = GameObject.Find("UpperLeftLeg");
        punchP2ULLegs.rotation = new Vector3(-12,3,13); 
        punchP2ULLegs.applyRotation = true;
        punchP2ULLegs.rotationSpeed = punchSpeed;
        punchingAnimation.Poses.Add(punchPose2); 

        TransformValue punchP2URLegs = new TransformValue(); 
        punchP2URLegs.target = GameObject.Find("UpperRightLeg");
        punchP2URLegs.rotation = new Vector3(29,5,-17);
        punchP2URLegs.applyRotation = true;
        punchP2URLegs.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2URLegs);

        TransformValue punchP2LLLegs = new TransformValue(); 
        punchP2LLLegs.target = GameObject.Find("LowerLeftLeg");
        punchP2LLLegs.rotation = new Vector3(-36, 0, 0);
        punchP2LLLegs.applyRotation = true;
        punchP2LLLegs.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2LLLegs);

        punchPose2.TransformValues.Add(punchP2LLLegs);
        TransformValue punchP2LRLegs = new TransformValue(); 
        punchP2LRLegs.target = GameObject.Find("LowerRightLeg");
        punchP2LRLegs.rotation = new Vector3(-58, 0, 0);
        punchP2LRLegs.applyRotation = true;
        punchP2LRLegs.rotationSpeed = punchSpeed;
        punchPose2.TransformValues.Add(punchP2LRLegs);

        Pose punchPose3 = new Pose(); // pose 3, retract arm
        punchPose3.name = "punchP3";

        TransformValue punchP3RShoulder = new TransformValue(); // raise upper right arm up
        punchP3RShoulder.target = GameObject.Find("RightShoulder");
        punchP3RShoulder.rotation = new Vector3(90, 100, 60);
        punchP3RShoulder.applyRotation = true;
        punchPose3.TransformValues.Add(punchP3RShoulder);

        TransformValue punchP3RBicep = new TransformValue(); // raise right bicep up
        punchP3RBicep.target = GameObject.Find("RightBicep");
        punchP3RBicep.rotation = new Vector3(0, -90, 0);
        punchP3RBicep.applyRotation = true;
        punchPose3.TransformValues.Add(punchP3RBicep);

        TransformValue punchP3RForarm = new TransformValue(); // raise right forearm up
        punchP3RForarm.target = GameObject.Find("RightForearm");
        punchP3RForarm.rotation = new Vector3(100, 0, 0);
        punchP3RForarm.applyRotation = true;
        punchPose3.TransformValues.Add(punchP3RForarm);

        TransformValue punchP3LTorso = new TransformValue(); // turn lower torso
        punchP3LTorso.target = GameObject.Find("LowerTorso");
        punchP3LTorso.rotation = new Vector3(0, 35, 0);
        punchP3LTorso.applyRotation = true;
        punchPose3.TransformValues.Add(punchP3LTorso);

        TransformValue punchP3Head = new TransformValue(); // turn head to look forward again
        punchP3Head.target = GameObject.Find("Head");
        punchP3Head.rotation = new Vector3(0, -35, 0);
        punchP3Head.applyRotation = true;
        punchPose3.TransformValues.Add(punchP3Head);


        punchingAnimation.Poses.Add(punchPose3); // add the pose

        animations.Add(punchingAnimation); // add the animation


        // -- ANIMATONS END -- //
    }

    void Start()
    {
        // A resting animation that the robot will use to reset itself to a neutral position once its done animating.
        // this will be used to reset the robot to a neutral position, so that it can be used for other animations
        foreach (MechAnimation MechAnimation in animations)
        {
            foreach (Pose p in MechAnimation.Poses)
            {
                foreach (TransformValue transform in p.TransformValues)
                {
                    if (transform.target != null)
                    {
                        transform.startPosition = transform.target.transform.localPosition;
                        // add target to the list of targets in use if it is not already in the list
                        if (!targetsInUse.Contains(transform.target))
                        {
                            targetsInUse.Add(transform.target);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Transform target is null for " + transform.target + " in animation " + MechAnimation.name + " pose " + p.name);
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
            transform.rotationSpeed = 20f;
            transform.moveSpeed = 20f;
            restingPose.TransformValues.Add(transform);
        }
        MechAnimation restingAnimation = new MechAnimation();
        restingAnimation.name = "rest";
        restingAnimation.loop = true;
        restingAnimation.Poses.Add(restingPose);
        animations.Add(restingAnimation);








    }



    // Update is called once per frame
    void Update()
    {
        // this is the core of the animation system.
        // It dynaically animates the robot based on the targets i set, which is the current animation.
        // It will loop throught the poses and uses unity's lerp functions to smoothly animate the robot (if it calls for it, if it doesnt, it will just linearly animate.)
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
        // setting it to null will HOPEFULLY make it rest, as we will make sure that the null value gets changed into the rest anim

        if (isPlaying)
        {
            if (targetAnimation != "")
            {
                MechAnimation roboAnim = animations.Find(x => x.name == targetAnimation);
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

                    // check bools to see if the transform has been achived
                    bool poseAchievedRotation = true;
                    bool poseAchievedPosition = true;


                    foreach (TransformValue transformValue in targetPose.TransformValues)
                    {
                        // Check if the robot has achieved the Pose
                        bool achievedRotation = !transformValue.applyRotation;
                        bool achievedPosition = !transformValue.applyPosition;

                        // check if the animation changed midway through
                        if (targetAnimation != roboAnim.name)
                        {
                            // Reset the robot to the start position if the animation is not found
                            targetAnimation = "rest"; // Reset the target animation
                            return;
                        }


                        if (transformValue.applyRotation)
                        {
                            if (!ballParkRotation(transformValue.target.transform.localRotation, Quaternion.Euler(transformValue.rotation)))
                            {
                                // If not, lerp to the target Pose
                                
                                if (transformValue.interpolate)
                                {
                                    transformValue.target.transform.localRotation = Quaternion.Slerp(transformValue.target.transform.localRotation, Quaternion.Euler(transformValue.rotation), Time.deltaTime * transformValue.rotationSpeed * rotationSpeedMultiplier);
                                }
                                else
                                {
                                    transformValue.target.transform.localRotation = Quaternion.RotateTowards(transformValue.target.transform.localRotation, Quaternion.Euler(transformValue.rotation), Time.deltaTime * transformValue.rotationSpeed * rotationSpeedMultiplier);
                                }

                            }
                            else
                            {
                                achievedRotation = true;
                            }
                        }

                        if (transformValue.applyPosition)
                        {
                            if (!ballParkPostion(transformValue.target.transform.localPosition, transformValue.position))
                            {
                                // If not, lerp to the target Pose
                                if (transformValue.interpolate)
                                {
                                    transformValue.target.transform.localPosition = Vector3.Slerp(transformValue.target.transform.localPosition, transformValue.position, Time.deltaTime * transformValue.moveSpeed * moveSpeedMultiplier);
                                }
                                else
                                {
                                    transformValue.target.transform.localPosition = Vector3.MoveTowards(transformValue.target.transform.localPosition, transformValue.position, Time.deltaTime * transformValue.moveSpeed * moveSpeedMultiplier);
                                }

                            }
                            else
                            {
                                achievedPosition = true;
                            }
                        }

                        // use a bitwise and to update the bools as needed. If i have achived rotation for one limb but another limb isnt then we havent achived full proper pose.
                        poseAchievedRotation &= achievedRotation;
                        poseAchievedPosition &= achievedPosition;
                    }

                    // Check if the entire Pose is achieved
                    if (poseAchievedRotation && poseAchievedPosition)
                    {
                        Debug.Log("Animation: " + roboAnim.name + " achieved Pose: " + targetPose.name);
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
                        //Debug.Log("Animation: " + roboAnim.name + " not achieved Pose: " + targetPose.name + ", Results: " + "achievedRotation: " + poseAchievedRotation + ", achievedPosition: " + poseAchievedPosition);
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


    public void SetTargetAnimation(string animationName)
    {
        // Set the target animation to the specified name
        needToRest = true;
        animationAfterResting = animationName;
    }

    public void SetSpeed(float speed)
    {
        moveSpeedMultiplier = speed;
        rotationSpeedMultiplier = speed;
    }


}

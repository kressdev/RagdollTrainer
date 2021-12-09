
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;

namespace JKress.AITrainer
{
    public class WalkerAgent : Agent
    {
        /// <summary>
        /// Based on walker example.
        /// Changed body parts and joints for Robot Kyle rig.
        /// Added Heuristic function to test joints by user input.
        /// Added ray perception sensor 3d to help navigate around walls.
        /// </summary>
        /// 
        [Header("Training Type")] //If true, agent is penalized for moving away from target
        public bool earlyTraining = false;

        [Header("Target Goal")]
        [SerializeField] Transform targetT; //Target the agent will walk towards during training
        [SerializeField] TargetController targetController;

        [Header("Body Parts")]
        [SerializeField] Transform hips;
        [SerializeField] Transform spine;
        [SerializeField] Transform head;
        [SerializeField] Transform thighL;
        [SerializeField] Transform shinL;
        [SerializeField] Transform footL;
        [SerializeField] Transform thighR;
        [SerializeField] Transform shinR;
        [SerializeField] Transform footR;
        [SerializeField] Transform armL;
        [SerializeField] Transform forearmL;
        [SerializeField] Transform armR;
        [SerializeField] Transform forearmR;

        [Header("Stabilizer")]
        [Range(1000, 4000)] [SerializeField] float m_stabilizerTorque = 4000f;
        float m_minStabilizerTorque = 1000;
        float m_maxStabilizerTorque = 4000; 
        [SerializeField] Stabilizer hipsStabilizer;
        [SerializeField] Stabilizer spineStabilizer;

        [Header("Walk Speed")] //The walking speed to try and achieve
        [Range(0.1f, 4)] [SerializeField] float m_TargetWalkingSpeed = 2; 
        float m_minWalkingSpeed = 0.1f; 
        float m_maxWalkingSpeed = 4; 

        public float MTargetWalkingSpeed // property
        {
            get { return m_TargetWalkingSpeed; }
            set { m_TargetWalkingSpeed = Mathf.Clamp(value, m_minWalkingSpeed, m_maxWalkingSpeed); }
        }

        public float MStabilizerTorque 
        {
            get { return m_stabilizerTorque; }
            set { m_stabilizerTorque = Mathf.Clamp(value, m_minStabilizerTorque, m_maxStabilizerTorque); }
        }

        //Should the agent sample a new goal velocity each episode?
        //If true, walkSpeed will be randomly set between zero and m_maxWalkingSpeed in OnEpisodeBegin()
        //If false, the goal velocity will be walkingSpeed
        public bool randomizeWalkSpeedEachEpisode;

        //This will be used as a stabilized model space reference point for observations
        //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
        OrientationCubeController m_OrientationCube;

        //The indicator graphic gameobject that points towards the target
        JointDriveController m_JdController;


        public override void Initialize()
        {
            m_OrientationCube = GetComponentInChildren<OrientationCubeController>();

            //Setup each body part
            m_JdController = GetComponent<JointDriveController>();

            m_JdController.SetupBodyPart(hips);
            m_JdController.SetupBodyPart(spine);
            m_JdController.SetupBodyPart(head);
            m_JdController.SetupBodyPart(thighL);
            m_JdController.SetupBodyPart(shinL);
            m_JdController.SetupBodyPart(footL);
            m_JdController.SetupBodyPart(thighR);
            m_JdController.SetupBodyPart(shinR);
            m_JdController.SetupBodyPart(footR);
            m_JdController.SetupBodyPart(armL);
            m_JdController.SetupBodyPart(forearmL);
            m_JdController.SetupBodyPart(armR);
            m_JdController.SetupBodyPart(forearmR);

            hipsStabilizer.uprightTorque = m_stabilizerTorque;
            spineStabilizer.uprightTorque = m_stabilizerTorque;
        }

        /// <summary>
        /// Loop over body parts and reset them to initial conditions.
        /// </summary>
        public override void OnEpisodeBegin()
        {
            targetController.MoveTargetToRandomPosition(); //method also fixes overlaps

            //Reset all of the body parts
            foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
            {
                bodyPart.Reset(bodyPart);
            }

            //Random start rotation to help generalize
            hips.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

            UpdateOrientationObjects();

            //Set our goal walking speed
            MTargetWalkingSpeed =
                randomizeWalkSpeedEachEpisode ? Random.Range(m_minWalkingSpeed, m_maxWalkingSpeed) : MTargetWalkingSpeed;
        }

        void FixedUpdate()
        {
            UpdateOrientationObjects();
           
            //Penalty if feet cross over
            var footSpacingReward = Vector3.Dot(footR.position - footL.position, footL.right);
            if (footSpacingReward > 0.1f) footSpacingReward = 0.1f;
            AddReward(footSpacingReward);

            var cubeForward = m_OrientationCube.transform.forward;
            var lookAtTargetReward = Vector3.Dot(head.forward, cubeForward) + 1; 

            var matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());

            if (earlyTraining)
            { //*Important* Forces movement towards target (penalize stationary swinging)
                matchSpeedReward = Vector3.Dot(GetAvgVelocity(), cubeForward); 
                if (matchSpeedReward > 0) matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());
            }

            AddReward(matchSpeedReward + 0.1f * lookAtTargetReward);
        }

        /// <summary>
        /// Add relevant information on each body part to observations.
        /// </summary>
        public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
        {
            //Interaction Objects Contact Check
            sensor.AddObservation(bp.objectContact.touchingGround); // Is this bp touching the ground
            sensor.AddObservation(bp.objectContact.touchingWall); // Is this bp touching the wall

            //Get velocities in the context of our orientation cube's space
            //Note: You can get these velocities in world space as well but it may not train as well.
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.velocity));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

            //Get position relative to hips in the context of our orientation cube's space
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - hips.position));

            //Get local rotations (including hips)
            sensor.AddObservation(bp.rb.transform.localRotation);

            //Skip body parts without a joint drive
            if (bp.rb.transform != hips) sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
        }

        /// <summary>
        /// Loop over body parts to add them to observation.
        /// </summary>
        public override void CollectObservations(VectorSensor sensor)
        {
            var cubeForward = m_OrientationCube.transform.forward;

            //velocity we want to match
            var velGoal = cubeForward * MTargetWalkingSpeed;
            //ragdoll's avg vel
            var avgVel = GetAvgVelocity();

            //current ragdoll velocity. normalized
            sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
            //avg body vel relative to cube
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVel));
            //vel goal relative to cube
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));

            //rotation deltas
            sensor.AddObservation(Quaternion.FromToRotation(hips.forward, cubeForward));
            sensor.AddObservation(Quaternion.FromToRotation(head.forward, cubeForward));

            //Position of target position relative to cube
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(targetT.position));
            
            foreach (var bodyPart in m_JdController.bodyPartsList)
            {
                CollectObservationBodyPart(bodyPart, sensor);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var bpDict = m_JdController.bodyPartsDict;

            var continuousActions = actionBuffers.ContinuousActions;
            var i = -1;

            bpDict[spine].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[thighL].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[thighR].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[shinL].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[shinR].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[footR].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[footL].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[armL].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[armR].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[forearmL].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[forearmR].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[head].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);

            //update joint strength settings
            bpDict[spine].SetJointStrength(continuousActions[++i]);
            bpDict[head].SetJointStrength(continuousActions[++i]);
            bpDict[thighL].SetJointStrength(continuousActions[++i]);
            bpDict[shinL].SetJointStrength(continuousActions[++i]);
            bpDict[footL].SetJointStrength(continuousActions[++i]);
            bpDict[thighR].SetJointStrength(continuousActions[++i]);
            bpDict[shinR].SetJointStrength(continuousActions[++i]);
            bpDict[footR].SetJointStrength(continuousActions[++i]);
            bpDict[armL].SetJointStrength(continuousActions[++i]);
            bpDict[forearmL].SetJointStrength(continuousActions[++i]);
            bpDict[armR].SetJointStrength(continuousActions[++i]);
            bpDict[forearmR].SetJointStrength(continuousActions[++i]);
        }

        public override void Heuristic(in ActionBuffers actionBuffers)
        {
            float x = Input.GetAxis("Vertical");
            float y = Input.GetAxis("Horizontal");
            float force = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;

            var continuousActions = actionBuffers.ContinuousActions;
            var i = -1;

            //SetJointTargetRotation
            continuousActions[++i] = -x; continuousActions[++i] = y; continuousActions[++i] = y;
            continuousActions[++i] = x; continuousActions[++i] = y;
            continuousActions[++i] = x; continuousActions[++i] = y;
            continuousActions[++i] = x;
            continuousActions[++i] = x;
            continuousActions[++i] = x; continuousActions[++i] = y; continuousActions[++i] = y;
            continuousActions[++i] = x; continuousActions[++i] = y; continuousActions[++i] = y;
            continuousActions[++i] = x; continuousActions[++i] = y;
            continuousActions[++i] = x; continuousActions[++i] = y;
            continuousActions[++i] = x;
            continuousActions[++i] = x;
            continuousActions[++i] = x; continuousActions[++i] = y;

            //SetJointStrength 
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
            continuousActions[++i] = force;
        }

        //Update OrientationCube and DirectionIndicator
        void UpdateOrientationObjects()
        {
            m_OrientationCube.UpdateOrientation(hips, targetT);
        }

        //Returns the average velocity of all of the body parts
        //Using the velocity of the hips only has shown to result in more erratic movement from the limbs, so...
        //...using the average helps prevent this erratic movement
        Vector3 GetAvgVelocity()
        {
            Vector3 velSum = Vector3.zero;

            //All Rbs
            int numOfRb = 0;
            foreach (var item in m_JdController.bodyPartsList)
            {
                numOfRb++;
                velSum += item.rb.velocity;
            }

            var avgVel = velSum / numOfRb;
            return avgVel;
        }

        Vector3 GetAvgPosition()
        {
            Vector3 posSum = Vector3.zero;

            //All Rbs
            int numOfRb = 0;
            foreach (var item in m_JdController.bodyPartsList)
            {
                numOfRb++;
                posSum += item.rb.position;
            }

            var avgPos = posSum / numOfRb;
            return avgPos;
        }

        //normalized value of the difference in avg speed vs goal walking speed.
        public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
        {
            //distance between our actual velocity and goal velocity
            var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, MTargetWalkingSpeed);

            //fix nan error
            if (MTargetWalkingSpeed == 0) MTargetWalkingSpeed = 0.01f;

            //return the value on a declining sigmoid shaped curve that decays from 1 to 0
            //This reward will approach 1 if it matches perfectly and approach zero as it deviates
            return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / MTargetWalkingSpeed, 2), 2);
        }
    }
}
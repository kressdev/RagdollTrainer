![Ragdoll Screenshot](/docs/RagdollScreenshot.png)

# Ragdoll Trainer Unity Project

Active ragdoll training with Unity ML-Agents (PyTorch). 

## Ragdoll Agent

Based on [walker example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md)

The Robot Kyle model from the Unity assets store is used for the ragdoll.

![RobotKyleBlend Image](/docs/RobotKyleBlend.png)

### Changes to the Walker Example:

* Default Robot Kyle rig replaced with a new rig created in blender (blend file is included in the assets folder).

* Ragdoll prefabs are instantiated by a spawner script attached to the camera.

* Added Heuristic function to drive the joints by user input (for testing only).

* Added stabilizer to the ragdoll. The stabilizer applies torque to rotate the ragdoll upright.

* Allow ragdoll to fall to the floor without reset. The agent learns to stand up after falling.

* Match speed reward modified to include negative value (i.e. moving away from target).

* Added LookAtTarget average for left and right foot transforms.

* Apply cutoff to LookAtTarget reward (dot product 0.5f or greater is set to 0.5f max)

* Speed and LookAtTarget reward added (instead of multiplied).

### TensorBoard Results

![TensorBoard Image](/docs/RagdollTensor.png)

Results shown are for 25 agents training the same behavior (C# MaxStep is 4000). 
The config file is the same as Walker.yaml example. Speed range is randomized (0.1-4.0).

Graph marker (1) agent learns to stand up and balance. 
Graph marker (2) agent learns walking and reaches the first target. 
Graph marker (3) agent optimizes walking to targets.

The match speed reward is computed for the hips and includes negative direction (penalty). 
The look at target reward is computed for the feet. A cutoff is applied so max reward is +/- 30deg to target.
Further testing is needed to compare this reward with the average body velocity from the Walker example.

## Behavior Controller

TODO: Agent changes behavior depending on observations (see [wall jump example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md))

![Ragdoll Screenshot](/docs/RagdollScreenshot.png)

# AI Trainer Unity Project

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

* Speed and LookAtTarget reward added (instead of multiplied) during early training stage.

### TensorBoard Results

![TensorBoard Image](/docs/RagdollTensor.png)

Results shown are for 25 agents training the same behavior (C# MaxStep is 4000). 
The config file is the same as Walker.yaml example.

Standup/balancing/turning to target requires 2e6 steps. Taking first steps forward to target requires another 1e6 steps.
During these steps the agent is rewarded by adding the foot lookAtTarget to the match speed. 

Full training for walking/running to target requires roughly 1E7 steps (speed range 0.1-4.0). During these steps 
the head lookAtTarget is multiplied with match speed (same reward as the Walker example).

The match speed reward is computed from the hips and includes negative direction (penalty). Further testing is needed 
to compare this reward with the average body velocity from the Walker example.

### Areas for Improvement

The agent will try to walk sideways or backwards if the head is used for lookAtTarget. 
The most effective solution so far is to reward lookAtTarget for the feet instead of head. 
Is there an alternative for early training to ensure forward walking?

With the foot lookAtTarget reward the agent develops a narrow stance where one foot is often blocking/in front of the other. 
The agent eventually learns to widen stance to walk faster but this behavior seems to slow training in the early stage.

The spine tends to oscillate around the vertical axis during walking behavior. This is likely caused by the stabilizer. 
Several solutions to check: increase joint damping, increase rb drag/angular drag, decrease stabilizer torque, or modify torque curve.
This also may be related to agent actions. Check after beta and entropy reach minimum (2e7-3e7 steps).

## Behavior Controller

TODO: Agent changes behavior depending on observations (see [wall jump example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md))

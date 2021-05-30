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

* Added "firstSteps" option to adjust rewards and stabilizer for early training 
  (set true if starting from untrained model, then toggle off later in training)

* Allow ragdoll to fall without reset. The agent learns to stand up and balance after falling.

* Match speed reward modified to include negative value (i.e. moving away from target).

* Added reward for foot spacing to prevent feet from crossing over (if firstSteps true).

### TensorBoard Results

![TensorBoard Image](/docs/tensorboard.png)

Results shown are for 25 agents training the same behavior (C# MaxStep is 5000). 
The config file is the same as Walker.yaml example. Speed range is randomized (0.01-4.0).

Set firstSteps true for 0-5e6 steps, then false for 5e6-3e7 steps (shown by arrow in reward plot).

Setting firstSteps true applies a negative direction (penalty) for the matching speed reward. This prevents the agent
from learning a stationary forward swinging motion. Setting firstSteps true also increases the stabilizer torque.

Setting firstSteps true also rewards foot spacing to keep feet apart. This prevents feet from crossing over and 
tripping up the ragdoll as it learns to walk forward.

Early training spring strength and damper are 140k and 3k respectively. This provides stiffer more responsive joints. 
However these settings cause more jitter. Settings can be scaled back to 80k/5k later in training to reduce jitter.

## Neural State Machine

TODO: Agent changes behavior state depending on observations.

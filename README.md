![Ragdoll Screenshot](/docs/CombinedAgent.png)

# Ragdoll Trainer Unity Project

Active ragdoll training with Unity ML-Agents (PyTorch). 

## Ragdoll Agent

Based on [walker example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md)

The Robot Kyle model from the Unity assets store is used for the ragdoll.

![RobotKyleBlend Image](/docs/RobotKyleBlend.png)

### Features:

* Default Robot Kyle rig replaced with a new rig created in blender. FBX and blend file included.

* Heuristic function inlcuded to drive the joints by user input (for development testing only).

* Added stabilizer to hips and spine. The stabilizer applies torque to help ragdoll balance.

* Added "earlyTraining" bool for initial balance/walking toward target.

* Added WallsAgent prefab for navigating around obstacles (using Ray Perception Sensor 3D).

* Added StairsAgent prefab for navigating small and large steps.

* Added curiosity to yaml to improve walls and stairs training.

### Training Process (where bool in parenthesis refers to "earlyTraining" setting): 

* Walking: WalkerAgent (true) -> WalkerAgent (false)
* Walls: WalkerAgent (true) -> WalkerAgent (false) -> WallsAgent (false)
* Stairs: WalkerAgent (true) -> StairsAgent (true) -> StairsAgent (false)


# Electrical Trigeminal Stimulation

This is a repository for the prototype electrical trigeminal stimulator and accompanying Unity files in the "PAPER TITLE HERE" paper. The device is an intra-nasal (in-nose) functional electrical stimulator, which can render simple electrical pulses to the nose's trigeminal nerve-endings.

## Repository Structure

* `Study-01` contains all the C files to run the paper's first user study. To run the study, you would need a RehaMove. This builds off of the [Rehamove Integration Library](https://github.com/humancomputerintegration/rehamove-integration-lib).
* `Study-02` contains all the Unity files to run the paper's second user study. The code features thorough comments and further explanation below. To actually run the study, you would need a RehaMove 3, Unity 3D (2018.4.17f1), Python, and ANSYS.
* `rehamove-adapter` contains 3D printing files and instructions to make a nose clip adapter for the RehaMove 3.
* `wearable-device` contains the schematics, 3D printing files, bill of materials, and instructions to assemble the electrical trigeminal stimulator.

## Device



## Unity Scene

### Environment

As we may develop this environment for out VR test in the future, I make it in the form of first person. So that we just need to replace the controller part with real VR-glasses tracking data in the next step. For the current version, if you open the project in Unity and look into the scene, you can see a little red player in the center of the game world and the main camera is at the position of its eyes. We'll regard the movement (position and rotation) of this camera as the movement of future subjects' noses. There are some other objects in the environment like stairs and boxes. They are just for checking the correctness of the movement log. You can check it by move the player yourself and try mapping the movement with the log file created after the application quits.

### Game controls

The controls of the game is very simple, just like most of the First Person Shooting games. you can use your mouse to see around (change the rotation) and use "WASD" and the Space on your keyboard to move the player (change the position).

### Movement log

Basically the script that moves the player also records the movement with a log frequency, which is one record for every 10 frames by default. After the whole application quits, these records will be written into the file called *MovementLog.txt* in the following format:

1. The first line states the format of other lines.
2. Each other line represents a movement record.
3. For each line, there are 3 components: a float showing the time stamp, a Vector3 showing the position of the camera and a Vector2 showing the rotation (or the direction) of the camera.
4. Both the time stamp and the position of the camera are quite easy to understand. The reason why the rotation is shown in a Vector2 instead of a Vector3 is that we don't care whether the subjects tilt their head, which results in only two rotation axis, one of which represents turning head and the other one represents looking up and down.
5. Different components in one line is split by comma. So a standard line in the *MovementLog.txt* would be like:

$$
\begin{align}
time,\ (p_1,\ p_2,\ p_3),\ (r_1,\ r_2)
\end{align}
$$

## Licensing & Citing

This documentation describes Open Hardware under the GNU  General Public License v3.0 as well as Open Source Software under the <Final License> License. When using or building upon this device in  an academic publication, please consider citing as follows:

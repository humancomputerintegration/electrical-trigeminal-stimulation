# Electrical Trigeminal Stimulation

This is a repository for the prototype electrical trigeminal stimulator and accompanying Unity files in the "PAPER TITLE HERE" paper. The device is an intra-nasal (in-nose) functional electrical stimulator, which can render simple electrical pulses to the nose's trigeminal nerve-endings.

## Repository Structure

* `Study-01` contains all the C files to run the paper's first user study. To run the study, you would need a RehaMove. This builds off of the [Rehamove Integration Library](https://github.com/humancomputerintegration/rehamove-integration-lib).
* `Study-02` contains all the Unity files to run the paper's second user study. The code features thorough comments and further explanation below. To actually run the study, you would need a RehaMove 3, Unity 3D (2018.4.17f1), Python, and ANSYS.
  * `ansys-Python` includes (1) `AnsysProcessing` files to process the ANSYS results and segment it into bounding volumes, and (2) `RehaMoveServer` to handle messages from Unity to RehaMove via an OSC protocol.
  * `ansys-Simulation` all files to run ANSYS simulations for the study.
  * `unity-DesktopVR` all Unity files to run the study. Note, the results from `AnsysProcessing` were omitted due to the file size, so you will have to copy the BVH results to Unity and direct the PlumeManager accordingly.
* `rehamove-adapter` contains 3D printing file as well as a bill of materials to make a nose clip adapter for the RehaMove 3.
* `wearable-device` contains the schematics, 3D printing files, bill of materials, and instructions to assemble the electrical trigeminal stimulator.

## Device: Electrical Trigeminal Stimulator



## User Study 01: Electrical Lateralization

Explain the code here.

## User Study 02: Real versus Virtual Odor Field

### Environment

As we may develop this environment for out VR test in the future, I make it in the form of first person. So that we just need to replace the controller part with real VR-glasses tracking data in the next step. For the current version, if you open the project in Unity and look into the scene, you can see a little red player in the center of the game world and the main camera is at the position of its eyes. We'll regard the movement (position and rotation) of this camera as the movement of future subjects' noses. There are some other objects in the environment like stairs and boxes. They are just for checking the correctness of the movement log. You can check it by move the player yourself and try mapping the movement with the log file created after the application quits.

### Movement logging

As the participant navigates the odor field (real or virtual), we record their movements ever <time> ms. Once the application quits, these records are written to a comma-separated values file name whose name is the participant's number and the trial number (e.g. `0927_01.csv`).

The values recorded include: the time stamp (ms), the head's positions (x,y,z), and the head's look at direction (x,y,z).

## Licensing & Citing

The Electrical Trigeminal Stimulator is certified open hardware (<number>). The interface hardware is released under the CERN OHL 2. The interface software is released under the GNU GPL V3. The interface documentation  is released under the CC0.

When using or building upon this device in  an academic publication, please consider citing as follows:

Jas Brooks, Jingxuan Wen, Jun Nishida, Romain Nith, Akifumi Takahashi, Shan-Yuan Teng and Pedro Lopes. 2021. “Wearable Stereo-Olfaction Substitution via Electrical Trigeminal Stimulation.” In *Proceedings of the 2021 CHI Conference on Human Factors in Computing Systems* (CHI ’21). Association for Computing Machinery, New York, NY, USA, <pages>. <DOI>
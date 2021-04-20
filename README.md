# Stereo-Smell via Electrical Trigeminal Stimulation

This is a repository for the prototype electrical trigeminal stimulator and accompanying Unity files in the "Stereo-Smell via Electrical Trigeminal Stimulation" paper. The device is an intra-nasal (in-nose) functional electrical stimulator, which can render simple electrical pulses to the nose's trigeminal nerve-endings.

## Repository Structure

* `Study-01` contains all the C files to run the paper's first user study. To run the study, you would need a RehaMove. This builds off of the [Rehamove Integration Library](https://github.com/humancomputerintegration/rehamove-integration-lib).
* `Study-02` contains all the Unity files to run the paper's second user study. The code features thorough comments and further explanation below. To actually run the study, you would need a RehaMove 3, Unity 3D (2018.4.17f1), Python, and ANSYS.
  * `ansys-Python` includes (1) `AnsysProcessing` files to process the ANSYS results and segment it into bounding volumes, and (2) `RehaMoveServer` to handle messages from Unity to RehaMove via an OSC protocol.
  * `ansys-Simulation` all files to run ANSYS simulations for the study.
  * `unity-DesktopVR` all Unity files to run the study. Note, the results from `AnsysProcessing` were omitted due to the file size, so you will have to copy the BVH results to Unity and direct the PlumeManager accordingly.
* `rehamove-adapter` contains 3D printing file to make a nose clip adapter for the RehaMove 3.
* `wearable-device` contains the schematic and 3D printing files to assemble the intranasal stimulator.

## Licensing & Citing

The interface hardware is released under the CERN OHL 2. The interface software is released under the GNU GPL V3. The interface documentation  is released under the CC0.

When using or building upon this device in  an academic publication, please consider citing as follows:

Jas Brooks, Shan-Yuan Teng, Jingxuan Wen, Romain Nith, and Jun Nishida, and Pedro Lopes. 2021. “Stereo-Smell via Electrical Trigeminal Stimulation.” In *Proceedings of the 2021 CHI Conference on Human Factors in Computing Systems* (CHI ’21). Association for Computing Machinery, New York, NY, USA. (Forthcoming.)
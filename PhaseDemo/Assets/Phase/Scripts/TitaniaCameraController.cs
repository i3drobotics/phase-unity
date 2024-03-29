﻿/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRStereoCamera.cs
 * @brief Example using I3DR Stereo Vision Unity API
 * @details Connect to I3DR stereo cameras, view captured images,
 * and display depth as point cloud using depth shader. 
 */

using I3DR.Phase.StereoCamera;

namespace I3DR.PhaseUnity
{
    public class TitaniaCameraController : CameraController
    {
        public TitaniaCameraController() : base(CameraDeviceType.DEVICE_TYPE_TITANIA, CameraInterfaceType.INTERFACE_TYPE_USB) {
            
        }
    }
}
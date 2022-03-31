/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file PlayerCameraController.cs
 * @brief Example using I3DR Stereo Vision Unity API
 * @details Connect to I3DR stereo cameras, view captured images,
 * and display depth as point cloud using depth shader. 
 */

using UnityEngine;

namespace I3DR.PhaseUnity
{
    public class PlayerCameraController : MonoBehaviour
    {
        public float lookSpeed = 3;
        private Vector2 rotation = Vector2.zero;

        void Update()
        {
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            rotation.x = Mathf.Clamp(rotation.x, -15f, 15f);
            Vector3 camRotation = new Vector3(rotation.x, rotation.y, 0) * lookSpeed;
            Camera.main.transform.localRotation = Quaternion.Euler(camRotation);
        }
    }
}
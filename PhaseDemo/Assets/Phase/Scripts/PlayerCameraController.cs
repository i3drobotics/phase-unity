using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace I3DR
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
using UnityEngine;
using UnityEditor;

namespace I3DR.PhaseUnity
{
    [CustomEditor(typeof(CameraController), true)]
    public class CameraControllerEditor : Editor
    {
        private void OnEnable()
        {
            CameraController camctrl = (CameraController)target;
            if (string.IsNullOrWhiteSpace(camctrl.leftImageFilename)){
                camctrl.leftImageFilename = Application.streamingAssetsPath + "/PhaseSamples/left.png";
            }
            if (string.IsNullOrWhiteSpace(camctrl.rightImageFilename))
            {
                camctrl.rightImageFilename = Application.streamingAssetsPath + "/PhaseSamples/right.png";
            }
            if (string.IsNullOrWhiteSpace(camctrl.leftCalibration))
            {
                camctrl.leftCalibration = Application.streamingAssetsPath + "/PhaseSamples/left.yaml";
            }
            if (string.IsNullOrWhiteSpace(camctrl.rightCalibration))
            {
                camctrl.rightCalibration = Application.streamingAssetsPath + "/PhaseSamples/right.yaml";
            }
        }

        // OnInspector GUI
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using I3DR.Phase;

namespace I3DR.PhaseUnityTests
{
    public class StereoCalibrationTests
    {
        [Test]
        public void test_LoadCalibration()
        {
            string camera_name = "stereotheatresim";
            string resource_folder = Application.dataPath + "/../../../../resources";
            string left_ros_yaml = resource_folder + "/test/" + camera_name + "/ros/left.yaml";
            string right_ros_yaml = resource_folder + "/test/" + camera_name + "/ros/right.yaml";
            string left_cv_yaml = resource_folder + "/test/" + camera_name + "/cv/left.yaml";
            string right_cv_yaml = resource_folder + "/test/" + camera_name + "/cv/right.yaml";

            StereoCameraCalibration cal_ros = StereoCameraCalibration.calibrationFromYAML(left_ros_yaml, right_ros_yaml);
            Assert.IsTrue(cal_ros.isValid());

            StereoCameraCalibration cal_cv = StereoCameraCalibration.calibrationFromYAML(left_cv_yaml, right_cv_yaml);
            Assert.IsTrue(cal_cv.isValid());

            Debug.Log("calibration load test success");
        }

        [Test]
        public void test_SaveCalibration()
        {
            string camera_name = "stereotheatresim";
            string out_folder = Application.dataPath + "/../../../../out/unity";
            string resource_folder = Application.dataPath + "/../../../../resources";
            string left_yaml = resource_folder + "/test/" + camera_name + "/ros/left.yaml";
            string right_yaml = resource_folder + "/test/" + camera_name + "/ros/right.yaml";

            StereoCameraCalibration cal = StereoCameraCalibration.calibrationFromYAML(left_yaml, right_yaml);
            Assert.IsTrue(cal.isValid());

            bool saveSuccess = cal.saveToYAML(out_folder + "/left_ros.yaml", out_folder + "/right_ros.yaml", CalibrationFileType.ROS_YAML);
            Assert.IsTrue(saveSuccess);
            saveSuccess &= cal.saveToYAML(out_folder + "/left_cv.yaml", out_folder + "/right_cv.yaml", CalibrationFileType.OPENCV_YAML);
            Assert.IsTrue(saveSuccess);

            Debug.Log("calibration save test success");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StereoCalibrationTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
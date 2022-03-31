using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using I3DR.Phase;

namespace I3DR.PhaseUnityTests
{
    public class StereoCalibrationTests
    {
        [Test]
        public void test_LoadCalibration()
        {
            string test_folder = Application.temporaryCachePath + "/.phasetest";
            string left_ros_yaml = test_folder + "/left_ros.yaml";
            string right_ros_yaml = test_folder + "/right_ros.yaml";
            string left_cv_yaml = test_folder + "/left_cv.yaml";
            string right_cv_yaml = test_folder + "/right_cv.yaml";

            Debug.Log("Generating test data...");

            Directory.CreateDirectory(test_folder);

            string left_ros_yaml_data = "" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: leftCamera\n" +
                "camera_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients:\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n";
            string right_ros_yaml_data = "" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: rightCamera\n" +
                "camera_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients:\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., -3.4782608695652175e+02, 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n";
            string left_cv_yaml_data = "" +
                "%YAML:1.0\n" +
                "---\n" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: leftCamera\n" +
                "camera_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   dt: d\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients: !!opencv-matrix\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   dt: d\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   dt: d\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   dt: d\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n" +
                "rms_error: \"\"\n";
            string right_cv_yaml_data = "" +
                "%YAML:1.0\n" +
                "---\n" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: leftCamera\n" +
                "camera_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   dt: d\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients: !!opencv-matrix\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   dt: d\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   dt: d\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix: !!opencv-matrix\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   dt: d\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., -3.4782608695652175e+02, 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n" +
                "rms_error: \"\"\n";

            File.WriteAllText(left_ros_yaml, left_ros_yaml_data);
            File.WriteAllText(right_ros_yaml, right_ros_yaml_data);
            File.WriteAllText(left_cv_yaml, left_cv_yaml_data);
            File.WriteAllText(right_cv_yaml, right_cv_yaml_data);

            StereoCameraCalibration cal_ros = StereoCameraCalibration.calibrationFromYAML(left_ros_yaml, right_ros_yaml);
            Assert.IsTrue(cal_ros.isValid());

            StereoCameraCalibration cal_cv = StereoCameraCalibration.calibrationFromYAML(left_cv_yaml, right_cv_yaml);
            Assert.IsTrue(cal_cv.isValid());

            Debug.Log("calibration load test success");
        }

        [Test]
        public void test_SaveCalibration()
        {
            string test_folder = Application.temporaryCachePath + "/.phasetest";
            string left_yaml = test_folder + "/left.yaml";
            string right_yaml = test_folder + "/right.yaml";
            string left_ros_yaml = test_folder + "/left_ros.yaml";
            string right_ros_yaml = test_folder + "/right_ros.yaml";
            string left_cv_yaml = test_folder + "/left_cv.yaml";
            string right_cv_yaml = test_folder + "/right_cv.yaml";

            Debug.Log("Generating test data...");

            Directory.CreateDirectory(test_folder);

            string left_yaml_data = "" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: leftCamera\n" +
                "camera_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients:\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n";
            string right_yaml_data = "" +
                "image_width: 2448\n" +
                "image_height: 2048\n" +
                "camera_name: rightCamera\n" +
                "camera_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., 0., 3.4782608695652175e+03, 1024., 0., 0., 1. ]\n" +
                "distortion_model: plumb_bob\n" +
                "distortion_coefficients:\n" +
                "   rows: 1\n" +
                "   cols: 5\n" +
                "   data: [ 0., 0., 0., 0., 0. ]\n" +
                "rectification_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 3\n" +
                "   data: [1., 0., 0., 0., 1., 0., 0., 0., 1.]\n" +
                "projection_matrix:\n" +
                "   rows: 3\n" +
                "   cols: 4\n" +
                "   data: [ 3.4782608695652175e+03, 0., 1224., -3.4782608695652175e+02, 0., 3.4782608695652175e+03, 1024., 0., 0., 0., 1., 0. ]\n";

            File.WriteAllText(left_yaml, left_yaml_data);
            File.WriteAllText(right_yaml, right_yaml_data);

            StereoCameraCalibration cal = StereoCameraCalibration.calibrationFromYAML(left_yaml, right_yaml);
            Assert.IsTrue(cal.isValid());

            bool saveSuccess = cal.saveToYAML(left_ros_yaml, right_ros_yaml, CalibrationFileType.ROS_YAML);
            Assert.IsTrue(saveSuccess);
            saveSuccess &= cal.saveToYAML(left_cv_yaml, right_cv_yaml, CalibrationFileType.OPENCV_YAML);
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
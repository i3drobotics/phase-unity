using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;

namespace I3DR.Phase.UnityTest
{
    public class RGBDVideoWriterTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void RGBDVideoWriterTest_SimplePasses()
        {
            string camera_name = "stereotheatresim";
            string cal_type = "ros";
            //string resource_folder = "D:\\Code\\I3DR\\i3dr-intranet\\phase-dev\\resources";
            string resource_folder = Application.dataPath + "/../../../../resources";
            string left_yaml = resource_folder+"/test/"+ camera_name +"/"+ cal_type +"/left.yaml";
            string right_yaml = resource_folder + "/test/" + camera_name + "/" + cal_type + "/right.yaml";
            string left_image_file = resource_folder + "/test/" + camera_name + "/left.png";
            string right_image_file = resource_folder + "/test/" + camera_name + "/right.png";
            //string out_folder = "D:\\Code\\I3DR\\i3dr-intranet\\phase-dev\\out\\cpp";
            string out_folder = Application.dataPath + "/../../../../out/unity";
            string out_rgb_video = out_folder + "/rgb.mp4";
            string out_depth_video = out_folder + "/depth.avi";

            Directory.CreateDirectory(out_folder);

            int num_of_frames = 1;

            Console.WriteLine("Loading test images...");
            //TODO get image size from file
            int image_width = 2448;
            int image_height = 2048;
            int image_rows = image_height;
            int image_cols = image_width;
            byte[] left_image_cv = Utils.readImage(left_image_file, image_width, image_height);
            byte[] right_image_cv = Utils.readImage(right_image_file, image_width, image_height);

            Assert.IsTrue(left_image_cv.Length != 0);
            Assert.IsTrue(right_image_cv.Length != 0);

            StereoCameraCalibration calibration = StereoCameraCalibration.calibrationFromYAML(left_yaml, right_yaml);

            Assert.IsTrue(calibration.isValid());

            StereoImagePair rect_image_pair = calibration.rectify(left_image_cv, right_image_cv, image_width, image_height);

            int image_channels = 3;
            
            MatrixUInt8 left_image = new MatrixUInt8(
                image_rows, image_cols, image_channels,
                rect_image_pair.left, true);
            MatrixUInt8 right_image = new MatrixUInt8(
                image_rows, image_cols, image_channels,
                rect_image_pair.right, true);

            Console.WriteLine("Processing stereo...");
            StereoParams stereo_params = new StereoParams(
                StereoMatcherType.STEREO_MATCHER_BM,
                11, 0, 25, false
            );
            MatrixFloat disparity = StereoProcess.processStereo(
                stereo_params, left_image, right_image, calibration, false
            );

            Assert.IsTrue(!disparity.isEmpty());

            float[] disparity_cv =  disparity.getData();

            float[] depth = Utils.disparity2Depth(disparity_cv, image_width, image_height, calibration.getQ());

            Assert.IsTrue(depth.Length != 0);

            Console.WriteLine("Setting up video writing...");
            RGBDVideoWriter rgbdVideoWriter = new RGBDVideoWriter(
                out_rgb_video, out_depth_video,
                left_image.getColumns(), left_image.getRows()
            );
            Assert.IsTrue(rgbdVideoWriter.isOpened());
            Console.WriteLine("Writing video...");
            for (int i = 0; i < num_of_frames; i++){
                rgbdVideoWriter.add(rect_image_pair.left, depth);
            }

            Console.WriteLine("Saving video file...");
            rgbdVideoWriter.saveThreaded();
            while(rgbdVideoWriter.isSaveThreadRunning()){}
            Assert.IsTrue(rgbdVideoWriter.getSaveThreadResult());

            rgbdVideoWriter.dispose();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator RGBDVideoWriterTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
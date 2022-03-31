using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using I3DR.Phase;

namespace I3DR.PhaseUnityTests
{
    public class StereoVisionTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void StereoVision_init()
        {
            CameraDeviceInfo device_info = new CameraDeviceInfo(
                "0815-0000", "0815-0001", "virtualpylon",
                CameraDeviceType.DEVICE_TYPE_GENERIC_PYLON,
                CameraInterfaceType.INTERFACE_TYPE_VIRTUAL
            );
            StereoMatcherType matcher_type = StereoMatcherType.STEREO_MATCHER_BM;
            //TODO generate interal calibration from ideal for tests
            StereoVision sv = new StereoVision(device_info, matcher_type, "", "");
            sv.dispose(); //check manual dispose of class works (useful in Unity when used in Editor)
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StereoVisionTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
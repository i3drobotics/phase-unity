using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace I3DR.Phase.UnityTest
{
    public class CameraDeviceInfoTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void CameraDeviceInfoTest_SimplePasses()
        {
            //TODO
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CameraDeviceInfoTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
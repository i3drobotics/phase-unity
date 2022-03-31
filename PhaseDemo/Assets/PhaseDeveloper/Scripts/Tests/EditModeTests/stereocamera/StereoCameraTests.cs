using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace I3DR.PhaseUnityTests
{
    public class StereoCameraTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void StereoCameraTest_SimplePasses()
        {
            //TODO
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator StereoCameraTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
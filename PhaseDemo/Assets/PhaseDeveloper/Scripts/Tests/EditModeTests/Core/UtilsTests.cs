using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace I3DR.Phase.UnityTest
{
    public class UtilsTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void UtilsTest_SimplePasses()
        {
            // Use the Assert class to test conditions
            //TODO
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator UtilsTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
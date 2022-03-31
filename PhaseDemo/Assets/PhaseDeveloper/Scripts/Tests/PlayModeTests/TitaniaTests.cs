using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using I3DR.PhaseUnity;

namespace I3DR.PhaseUnityTests
{
    public class TitaniaControllerTests
    {
        [OneTimeSetUp]
        public void LoadScene()
        {
            SceneManager.LoadScene("TitaniaTestScene");
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TitaniaControllerTest_SimplePasses()
        {
            // Use the Assert class to test conditions
            
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TitaniaControllerTest_WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [UnityTest]
        public IEnumerator MonoBehaviourTest_Works()
        {
            yield return new MonoBehaviourTest<TitaniaCameraControllerMonoBehaviourTest>();
        }

        public class TitaniaCameraControllerMonoBehaviourTest : TitaniaCameraController, IMonoBehaviourTest
        {
            public TitaniaCameraControllerMonoBehaviourTest() : base()
            {
                isVirtual = true;
            }

            private int frameCount;
            public bool IsTestFinished
            {
                get { 
                    if (frameCount > 10){
                        return true;
                    }
                    return false;
                }
            }

            void Update()
            {
                frameCount++;
            }
        }

    }
}
/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRDemoBuild
 * @brief Build pipeline for command line building of project
 */

using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace I3DR
{
    class PhaseAutoTest
    {
        // Playmode causes domain reload that clears callbacks
        // Reregister callbacks on domain reload
        [InitializeOnLoad]
        public class OnDomainReload
        {
            static OnDomainReload()
            {
                var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                testRunnerApi.RegisterCallbacks(new TestCallbacks());
            }
        }

        [MenuItem("Window/General/Test Extensions/Run Play Mode Tests")]
        public static void TestPlayMode()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode
            };
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }

        [MenuItem("Window/General/Test Extensions/Run Edit Mode Tests")]
        public static void TestEditMode()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter()
            {
                testMode = TestMode.EditMode
            };
            testRunnerApi.Execute(new ExecutionSettings(filter));
        }

        private class TestCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {

            }

            public void RunFinished(ITestResultAdaptor result)
            {

            }

            public void TestStarted(ITestAdaptor test)
            {

            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren)
                {
                    Debug.Log(string.Format("Test {0} {1}", result.Test.Name, result.ResultState));
                } else
                {
                    //Debug.Log(string.Format("{0}", result.Test.Name));
                }
            }
        }
    }
}
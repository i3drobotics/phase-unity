/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file I3DRStereoCamera.cs
 * @brief Example using I3DR Stereo Vision Unity API
 * @details Connect to I3DR stereo cameras, view captured images,
 * and display depth as point cloud using depth shader. 
 */

using UnityEngine;
using I3DR.Phase.StereoCamera;
using I3DR.Phase.StereoMatcher;
using I3DR.Phase.Calib;
using I3DR.Phase;
using I3DR.Phase.Types;
using System.Collections;
using System.IO;

namespace I3DR.PhaseUnity
{
    [RequireComponent(typeof(DepthRenderer))]
    abstract public class CameraController : MonoBehaviour
    {
        [SerializeField] public int exposure = 5000;
        [SerializeField] public float downsampleFactor = 1.0f;
        [SerializeField] public GameObject leftImageDisplayPlane;
        [SerializeField] public GameObject rightImageDisplayPlane;
        [SerializeField] public GameObject depthImageDisplayPlane;

        public bool isVirtual = false;
        [ConditionalHide("isVirtual", true)]
        public string leftImageFilename;
        [ConditionalHide("isVirtual", true)]
        public string rightImageFilename;

        public string leftCalibration;
        public string rightCalibration;

        private CameraDeviceType _deviceType;
        private CameraInterfaceType _interfaceType;
        private float _cameraReadRate;

        private Texture2D _leftImageTexture;
        private Texture2D _rightImageTexture;
        private Texture2D _depthImageTexture;
        private Material _leftImageMaterial;
        private Material _rightImageMaterial;
        private Material _depthImageMaterial;

        private AbstractStereoCamera _stereoCam;
        private AbstractStereoMatcher _stereoMatcher;
        private StereoCameraCalibration _stereoCalib;
        private DepthRenderer _depthRenderer;

        private bool _firstFrameReady;
        private bool _newFrameReady;
        private float _readNextActionTime;
        private float _readInterval;
        private bool _readThreadStarted;
        private bool _matchThreadStarted;
        private int _previousExposure;
        private float _previousDownsampleFactor;

        byte[] latest_left_image;
        byte[] latest_right_image;
        byte[] latest_rect_left_image;
        byte[] latest_rect_right_image;
        byte[] latest_left_image_rgba;

        public CameraController(CameraDeviceType deviceType, CameraInterfaceType interfaceType, float readRate = 10.0f){
            _deviceType = deviceType;
            _interfaceType = interfaceType;
            _cameraReadRate = readRate;
        }

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(leftImageFilename))
            {
                leftImageFilename = Application.streamingAssetsPath + "/PhaseSamples/left.png";
            }
            if (string.IsNullOrWhiteSpace(rightImageFilename))
            {
                rightImageFilename = Application.streamingAssetsPath + "/PhaseSamples/right.png";
            }
            if (string.IsNullOrWhiteSpace(leftCalibration))
            {
                leftCalibration = Application.streamingAssetsPath + "/PhaseSamples/left.yaml";
            }
            if (string.IsNullOrWhiteSpace(rightCalibration))
            {
                rightCalibration = Application.streamingAssetsPath + "/PhaseSamples/right.yaml";
            }
        }

        void Start()
        {
            _depthRenderer = GetComponent<DepthRenderer>();
            _readInterval = 1.0f / _cameraReadRate;
            _previousExposure = exposure;
            _previousDownsampleFactor = downsampleFactor;
            _readThreadStarted = false;
            _matchThreadStarted = false;
            _newFrameReady = false;

            bool license_valid = StereoI3DRSGM.isLicenseValid();
            Debug.Log("I3DRSGM license: " + license_valid);

            string camera_name, left_serial, right_serial;

            CameraDeviceInfo deviceInfo;
            if (_deviceType == CameraDeviceType.DEVICE_TYPE_INVALID)
            {
                Debug.LogError("Invalid camera device type");
                return;
            }

            if (isVirtual)
            {
                _interfaceType = CameraInterfaceType.INTERFACE_TYPE_VIRTUAL;
            }

            if (_interfaceType == CameraInterfaceType.INTERFACE_TYPE_VIRTUAL)
            {
                left_serial = "0815-0000";
                right_serial = "0815-0001";
                if (_deviceType == CameraDeviceType.DEVICE_TYPE_TITANIA)
                {
                    camera_name = "titania";
                } else  if (_deviceType == CameraDeviceType.DEVICE_TYPE_PHOBOS)
                {
                    camera_name = "phobos";
                }
                else
                {
                    camera_name = "stereotheatresim";
                }
            } else
            {
                if (_deviceType == CameraDeviceType.DEVICE_TYPE_PHOBOS)
                {
                    // TODO add serial numbers for Phobos
                    throw new System.NotImplementedException();
                }
                else if (_deviceType == CameraDeviceType.DEVICE_TYPE_TITANIA)
                {
                    left_serial = "40091829";
                    right_serial = "40098273";
                    camera_name = "titania";
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }

            deviceInfo = new CameraDeviceInfo(left_serial, right_serial, camera_name, _deviceType, _interfaceType);

            StereoMatcherType matcher_type;
            if (license_valid){
                matcher_type = StereoMatcherType.STEREO_MATCHER_I3DRSGM;
            } else {
                matcher_type = StereoMatcherType.STEREO_MATCHER_BM;
            }

            if (!File.Exists(leftCalibration))
            {
                Debug.LogError("Calibration file does not exist at: " + leftCalibration);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    UnityEngine.Application.Quit();
#endif
                return;
            }
            if (!File.Exists(rightCalibration))
            {
                Debug.LogError("Calibration file does not exist at: " + rightCalibration);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    UnityEngine.Application.Quit();
#endif
                return;
            }

            _stereoCalib = StereoCameraCalibration.calibrationFromYAML(leftCalibration, rightCalibration);
            if (!_stereoCalib.isValid())
            {
                Debug.LogError("Calibration is invalid");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
                return;
            }

            _stereoMatcher = StereoMatcher.createStereoMatcher(matcher_type);

            _stereoCam = StereoCamera.createStereoCamera(deviceInfo);
            if (_interfaceType == CameraInterfaceType.INTERFACE_TYPE_VIRTUAL)
            {
                if (!File.Exists(leftImageFilename))
                {
                    Debug.LogError("Image does not exist at: " + leftImageFilename);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    UnityEngine.Application.Quit();
#endif
                    return;
                }
                if (!File.Exists(rightImageFilename))
                {
                    Debug.LogError("Image does not exist at: " + rightImageFilename);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    UnityEngine.Application.Quit();
#endif
                    return;
                }
                _stereoCam.setTestImagePaths(
                    leftImageFilename, rightImageFilename
                );
            }
            

            Debug.Log("Connecting to camera...");
            bool success = _stereoCam.connect();
            if (success)
            {
                Debug.Log("Connected!");
                int imageHeight = _stereoCam.getHeight();
                int imageWidth = _stereoCam.getWidth();
                float hfov = _stereoCalib.getHFOV();

                _stereoCam.setExposure(exposure);
                _stereoCam.setDownsampleFactor(downsampleFactor);
                _stereoCalib.setDownsampleFactor(downsampleFactor);

                if (leftImageDisplayPlane != null)
                {
                    _leftImageMaterial = leftImageDisplayPlane.GetComponent<Renderer>().material;
                    if (_leftImageMaterial == null)
                    {
                        _leftImageMaterial = new Material(Shader.Find("Standard"));
                    }
                    _leftImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                    _leftImageMaterial.mainTexture = _leftImageTexture;
                }

                if (rightImageDisplayPlane != null)
                {
                    _rightImageMaterial = rightImageDisplayPlane.GetComponent<Renderer>().material;
                    if (_rightImageMaterial == null)
                    {
                        _rightImageMaterial = new Material(Shader.Find("Standard"));
                    }
                    _rightImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                    _rightImageMaterial.mainTexture = _rightImageTexture;
                }

                if (depthImageDisplayPlane != null)
                {
                    _depthImageMaterial = depthImageDisplayPlane.GetComponent<Renderer>().material;
                    if (_depthImageMaterial == null)
                    {
                        _depthImageMaterial = new Material(Shader.Find("Standard"));
                    }
                    _depthImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                    _depthImageMaterial.mainTexture = _depthImageTexture;
                }

                _depthRenderer.Init(imageWidth, imageHeight, hfov);
                _stereoCam.startCapture();
            } else
            {
                Debug.Log("Failed to connect to camera");
            }
            
        }

        IEnumerator processReadResult(CameraReadResult readResult)
        {
            if (readResult.valid)
            {
                Debug.Log("New frame");
                int imageWidth = _stereoCam.getWidth();
                int imageHeight = _stereoCam.getHeight();

                latest_left_image = readResult.left;
                latest_right_image = readResult.right;

                byte[] left_image = Utils.flip(readResult.left, imageWidth, imageHeight, 3, 1);
                byte[] right_image = Utils.flip(readResult.right, imageWidth, imageHeight, 3, 1);

                StereoImagePair rect_pair = _stereoCalib.rectify(readResult.left, readResult.right, imageWidth, imageHeight);

                latest_rect_left_image = rect_pair.left;
                latest_rect_right_image = rect_pair.right;

                /*byte[] rect_left_image = Utils.flip(readResult.left, imageWidth, imageHeight, 3, 1);
                byte[] rect_right_image = Utils.flip(readResult.right, imageWidth, imageHeight, 3, 1);*/

                yield return null;

                byte[] left_image_rgba = Utils.bgr2rgba(left_image, imageWidth, imageHeight);
                byte[] right_image_rgba = Utils.bgr2rgba(right_image, imageWidth, imageHeight);

                /*byte[] rect_left_image_rgba = Utils.bgr2rgba(rect_left_image, imageWidth, imageHeight);
                byte[] rect_right_image_rgba = Utils.bgr2rgba(rect_right_image, imageWidth, imageHeight);*/

                latest_left_image_rgba = left_image_rgba;

                yield return null;

                if (leftImageDisplayPlane != null)
                {
                    if (_leftImageTexture == null || _leftImageTexture.width != imageWidth || _leftImageTexture.height != imageHeight)
                    {
                        _leftImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                        _leftImageMaterial.mainTexture = _leftImageTexture;
                    }
                    _leftImageTexture.LoadRawTextureData(left_image_rgba);
                    _leftImageTexture.Apply(false);
                }
                if (rightImageDisplayPlane != null)
                {
                    if (_rightImageTexture == null || _rightImageTexture.width != imageWidth || _rightImageTexture.height != imageHeight)
                    {
                        _rightImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                        _rightImageMaterial.mainTexture = _rightImageTexture;
                    }
                    _rightImageTexture.LoadRawTextureData(right_image_rgba);
                    _rightImageTexture.Apply(false);
                }
                _newFrameReady = true;
            }
            yield return null;
        }

        IEnumerator processComputeResult(StereoMatcherComputeResult computeResult)
        {
            if (computeResult.valid)
            {
                int imageWidth = _stereoCam.getWidth();
                int imageHeight = _stereoCam.getHeight();

                float[] disparity = Utils.flip(computeResult.disparity, imageWidth, imageHeight, 1, 1);

                yield return null;

                byte[] disp_image = Utils.normaliseDisparity(disparity, imageWidth, imageHeight);

                byte[] disp_image_rgba = Utils.bgr2rgba(disp_image, imageWidth, imageHeight);

                yield return null;

                float[] Q = _stereoCalib.getQ();
                float[] depth = Utils.disparity2Depth(disparity, imageWidth, imageHeight, Q);

                yield return null;

                _depthRenderer.UpdateBuffers(latest_left_image_rgba, depth);

                yield return null;

                
                if (depthImageDisplayPlane != null)
                {
                    if (_depthImageTexture == null || _depthImageTexture.width != imageWidth || _depthImageTexture.height != imageHeight)
                    {
                        _depthImageTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                        _depthImageMaterial.mainTexture = _depthImageTexture;
                    }
                    _depthImageTexture.LoadRawTextureData(disp_image_rgba);
                    _depthImageTexture.Apply(false);
                }
            }
            yield return null;
        }

        void ReadCameraFrameThreaded()
        {
            if (_stereoCam != null)
            {
                if (_stereoCam.isConnected())
                {
                    if (!_readThreadStarted)
                    {
                        _readThreadStarted = true;
                        _stereoCam.startReadThread();
                    }
                    else
                    {
                        if (!_stereoCam.isReadThreadRunning())
                        {
                            CameraReadResult readResult = _stereoCam.getReadThreadResult();
                            StartCoroutine(processReadResult(readResult));
                            _readThreadStarted = false;
                        }
                    }
                }
            }

            if (_newFrameReady)
            {
                _newFrameReady = false;
                if (_stereoMatcher != null)
                {
                    int imageWidth = _stereoCam.getWidth();
                    int imageHeight = _stereoCam.getHeight();
                    if (!_matchThreadStarted)
                    {
                        _matchThreadStarted = true;
                        _stereoMatcher.startComputeThread(
                            latest_rect_left_image, latest_rect_right_image,
                            imageWidth, imageHeight);
                    }
                    else
                    {
                        if (!_stereoMatcher.isComputeThreadRunning())
                        {
                            StereoMatcherComputeResult computeResult = _stereoMatcher.getComputeThreadResult(
                                imageWidth, imageHeight);
                            StartCoroutine(processComputeResult(computeResult));
                            _matchThreadStarted = false;
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (Time.time > _readNextActionTime)
            {
                _readNextActionTime += _readInterval;
                ReadCameraFrameThreaded();
            }

            if (_stereoCam != null)
            {
                if (_stereoCam.isConnected())
                {
                    if (exposure != _previousExposure)
                    {
                        _stereoCam.setExposure(exposure);
                        _previousExposure = exposure;
                    }
                    if (downsampleFactor != _previousDownsampleFactor)
                    {
                        _stereoCam.setDownsampleFactor(downsampleFactor);
                        _stereoCalib.setDownsampleFactor(downsampleFactor);
                        _previousDownsampleFactor = downsampleFactor;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_stereoCam != null)
            {
                if (_stereoCam.isConnected())
                {
                    _stereoCam.disconnect();
                }
                _stereoCam.dispose();
            }
            if (_stereoCalib != null)
            {
                _stereoCalib.dispose();
            }
            if (_stereoMatcher != null)
            {
                _stereoMatcher.dispose();
            }
        }
    }
}
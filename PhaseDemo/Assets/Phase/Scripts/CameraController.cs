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
using I3DR.Phase;
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

        public bool firstFrameReady;

        private CameraDeviceType _deviceType;
        private CameraInterfaceType _interfaceType;
        private float _cameraReadRate;

        private Texture2D _leftImageTexture;
        private Texture2D _rightImageTexture;
        private Texture2D _depthImageTexture;
        private Material _leftImageMaterial;
        private Material _rightImageMaterial;
        private Material _depthImageMaterial;

        private StereoVision _stereoVis;
        private AbstractStereoCamera _stereoCam;
        private StereoCameraCalibration _calibration;
        private DepthRenderer _depthRenderer;

        private float _readNextActionTime;
        private float _readInterval;
        private bool _readThreadStarted;
        private int _previousExposure;
        private float _previousDownsampleFactor;

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
            firstFrameReady = false;

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

            _stereoVis = new StereoVision(deviceInfo, matcher_type, leftCalibration, rightCalibration);
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
                _stereoVis.setTestImagePaths(
                    leftImageFilename, rightImageFilename
                );
            }

            Debug.Log("Connecting to camera...");
            bool success = _stereoVis.connect();
            if (success)
            {
                Debug.Log("Connected!");
                int imageHeight = _stereoVis.getHeight();
                int imageWidth = _stereoVis.getWidth();
                float hfov = _stereoVis.getHFOV();

                _stereoVis.getCamera(out _stereoCam);
                _stereoCam.setExposure(exposure);
                _stereoVis.setDownsampleFactor(downsampleFactor);

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
                _stereoVis.getCalibration(out _calibration);
                _stereoVis.startCapture();
            } else
            {
                Debug.Log("Failed to connect to camera");
            }
            
        }

        IEnumerator processReadResult(StereoVisionReadResult readResult)
        {
            if (readResult.valid)
            {
                Debug.Log("New frame");
                firstFrameReady = true;
                int imageWidth = _stereoVis.getWidth();
                int imageHeight = _stereoVis.getHeight();

                byte[] left_image = Utils.flip(readResult.left_image, imageWidth, imageHeight, 3, 1);
                byte[] right_image = Utils.flip(readResult.right_image, imageWidth, imageHeight, 3, 1);
                float[] disparity = Utils.flip(readResult.disparity, imageWidth, imageHeight, 1, 1);

                yield return null;

                byte[] left_image_rgba = Utils.bgr2rgba(left_image, imageWidth, imageHeight);
                byte[] right_image_rgba = Utils.bgr2rgba(right_image, imageWidth, imageHeight);

                yield return null;

                float[] Q = _calibration.getQ();
                float[] depth = Utils.disparity2Depth(disparity, imageWidth, imageHeight, Q);

                byte[] disp_image = Utils.normaliseDisparity(readResult.disparity, imageWidth, imageHeight);

                byte[] disp_image_rgba = Utils.bgr2rgba(disp_image, imageWidth, imageHeight);

                yield return null;

                _depthRenderer.UpdateBuffers(left_image_rgba, depth);

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
            if (_stereoVis != null)
            {
                if (_stereoVis.isConnected())
                {
                    if (!_readThreadStarted)
                    {
                        _readThreadStarted = true;
                        _stereoVis.startReadThread();
                    }
                    else
                    {
                        if (!_stereoVis.isReadThreadRunning())
                        {
                            StereoVisionReadResult readResult = _stereoVis.getReadThreadResult();
                            StartCoroutine(processReadResult(readResult));
                            _readThreadStarted = false;
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

            if (_stereoVis != null)
            {
                if (_stereoVis.isConnected())
                {
                    if (exposure != _previousExposure)
                    {
                        _stereoCam.setExposure(exposure);
                        _previousExposure = exposure;
                    }
                    if (downsampleFactor != _previousDownsampleFactor)
                    {
                        _stereoVis.setDownsampleFactor(downsampleFactor);
                        _previousDownsampleFactor = downsampleFactor;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_stereoVis != null)
            {
                if (_stereoVis.isConnected())
                {
                    _stereoVis.disconnect();
                }
                _stereoVis.dispose();
            }
        }
    }
}
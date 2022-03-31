/*!
 * @authors Ben Knight (bknight@i3drobotics.com)
 * @date 2021-05-26
 * @copyright Copyright (c) I3D Robotics Ltd, 2021
 * 
 * @file PhaseDepthRenderer.cs
 * @brief Pass data to depth compute shader for displaying point cloud
 */

using UnityEngine;
using UnityEngine.Rendering;

namespace I3DR.PhaseUnity
{
    public class DepthRenderer : MonoBehaviour
    {
        [Range(0.01f, 20.0f)] [SerializeField] private float _ZNear = 1.0f;
        [Range(1.0f, 100.0f)] [SerializeField] private float _ZFar = 10.0f;
        [Range(0.1f, 10.0f)] [SerializeField] private float _particleSize = 1.0f;
        [Range(0.1f, 10.0f)] [SerializeField] private float _scale = 1.0f;
        [SerializeField] private bool _flipX;
        [SerializeField] private bool _flipY;

        /// <summary>
        /// Unity's convention is to specify the vertical field of view in degrees.
        /// </summary>
        private float _verticalFOV;

        private int _imageWidth;
        private int _imageHeight;
        private Material _material;
        private float _aspectRatio;
        private ComputeShader _computeShader;
        private ComputeBuffer _depthCBuffer;
        private ComputeBuffer _colorCBuffer;

        private bool _isInit = false;
        private object _isInitLock;

        /// <summary>
        /// The buffer output to by the compute shader of each point's four vertices in world space. 
        /// </summary>
        private ComputeBuffer _particleCBuffer;

        private int _displacementKernel;
        private Bounds _boundingBox;
        private Matrix4x4 _cameraTransform;

        /// <summary>
        /// Flag used to signal the main thread that there's new data to be uploaded to the GPU.
        /// </summary>
        private bool _dataReady;
        private object _dataReadyLock;

        // The depth in metres from the camera's principal point.
        private float[] _depthBuffer;

        /// <summary>
        /// BGRA colour array.
        /// </summary>
        private byte[] _colorBuffer;
        /// <summary>
        /// BGRA32 packed colour array.
        /// </summary>
        //private int[] _colorBuffer;

        private static readonly int ParticleSizeProp = Shader.PropertyToID("particle_size");
        private static readonly int NearPlaneProp = Shader.PropertyToID("near_plane");
        private static readonly int FarPlaneProp = Shader.PropertyToID("far_plane");
        private static readonly int ImageSizeProp = Shader.PropertyToID("image_size");
        private static readonly int ParticleBufferProp = Shader.PropertyToID("particle_buffer");
        private static readonly int ColorBufferProp = Shader.PropertyToID("color_buffer");
        private static readonly int DepthBufferProp = Shader.PropertyToID("depth_buffer");

        /// <summary>
        /// Whether to flip the image along the X axis.
        /// Must be called only from the main thread.
        /// </summary>
        public bool FlipX
        {
            get => _flipX;
            set
            {
                _flipX = value;
                _computeShader.SetInt("flip_x", _flipX ? 1 : -1);
            }
        }

        /// <summary>
        /// Whether to flip the image along the Y axis.
        /// Must be called only from the main thread.
        /// </summary>
        public bool FlipY
        {
            get => _flipY;
            set
            {
                _flipY = value;
                _computeShader.SetInt("flip_y", _flipY ? 1 : -1);
            }
        }

        /// <summary>
        /// Uniform scaling of the point cloud. Scales near and far clip planes too.
        /// Must be called only from the main thread.
        /// </summary>
        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _computeShader.SetFloat("scale", _scale);
            }
        }

        /// <summary>
        /// Set the near clip plane distance in metres from the camera's principal point. Points behind this plane are culled.
        /// Must be called only from the main thread.
        /// </summary>
        public float ZNear
        {
            get => _ZNear;
            set
            {
                _ZNear = value;
                _material.SetFloat(NearPlaneProp, _ZNear);
            }
        }

        /// <summary>
        /// Set the far clip plane distance in metres from the camera's principal point. Points beyond this plane are culled.
        /// Must be called only from the main thread.
        /// </summary>
        public float ZFar
        {
            get => _ZFar;
            set
            {
                _ZFar = value;
                _material.SetFloat(FarPlaneProp, _ZFar);
            }
        }

        /// <summary>
        /// Sets the particle screen-space size in pixels.
        /// Must be called only from the main thread.
        /// </summary>
        public float ParticleSize
        {
            get => _particleSize;
            set
            {
                _particleSize = value;
                _material.SetFloat(ParticleSizeProp, _particleSize);
            }
        }

        private void Awake()
        {
            _material = new Material(Shader.Find("I3DR/displacement"));
            _computeShader = Resources.Load<ComputeShader>("displacement");
            _displacementKernel = _computeShader.FindKernel("GenerateParticles");
            _dataReadyLock = new object();
            _isInitLock = new object();
        }

        /// <summary>
        /// Initialize fixed parameters, allocates buffers, and sets shader initial values.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="horizontalFOV">The camera's horizontal field of view in radians.</param>
        public void Init(int width, int height, float horizontalFOV)
        {
            _imageWidth = width;
            _imageHeight = height;
            _aspectRatio = (float)_imageWidth / _imageHeight;
            _verticalFOV = (horizontalFOV / _aspectRatio) * Mathf.Rad2Deg;

            UpdateCameraParameters();

            _depthCBuffer = new ComputeBuffer(_imageWidth * _imageHeight, 4);
            _colorCBuffer = new ComputeBuffer(_imageWidth * _imageHeight, 4);
            _particleCBuffer = new ComputeBuffer(_imageWidth * _imageHeight * 4, 7 * 4);

            _depthBuffer = new float[_imageWidth * _imageHeight];
            _colorBuffer = new byte[_imageWidth * _imageHeight * 4];

            _material.SetFloat(ParticleSizeProp, _particleSize);
            _material.SetFloat(NearPlaneProp, _ZNear);
            _material.SetFloat(FarPlaneProp, _ZFar);
            _material.SetVector(ImageSizeProp, new Vector4(_imageWidth, _imageHeight, 0.0f, 0.0f));
            _material.SetBuffer(ParticleBufferProp, _particleCBuffer);
            _material.SetBuffer(ColorBufferProp, _colorCBuffer);
            _material.SetBuffer(DepthBufferProp, _depthCBuffer);

            _computeShader.SetBuffer(_displacementKernel, "depth_buffer", _depthCBuffer);
            _computeShader.SetBuffer(_displacementKernel, "particle_buffer", _particleCBuffer);
            _computeShader.SetMatrix("camera_p", _cameraTransform);
            _computeShader.SetMatrix("camera_p_inv", _cameraTransform.inverse);
            _computeShader.SetMatrix("camera_v_inv", transform.worldToLocalMatrix.inverse);
            _computeShader.SetVector("image_size", new Vector4(_imageWidth, _imageHeight, 0.0f, 0.0f));
            _computeShader.SetInt("flip_x", _flipX ? 1 : -1);
            _computeShader.SetInt("flip_y", _flipY ? 1 : -1);
            _computeShader.SetFloat("scale", _scale);

            lock (_isInitLock)
            {
                _isInit = true;
            }
        }

        public void UpdateHFOV(float horizontalFOV)
        {
            _verticalFOV = (horizontalFOV / _aspectRatio) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Copies the depth and colour buffers to arrays for later uploading to the GPU.
        /// Should be called from a worker thread.
        /// </summary>
        /// <param name="depth">The OpenCV depth buffer.</param>
        /// <param name="rgb">The OpenCV colour buffer.</param>
        public void UpdateBuffers(byte[] rgba, float[] depth)
        {
            // If the main thread hasn't done anything with the latest data, wait here until it has.
            /*while (_dataReady)
            {
                Thread.Sleep(0);
            }*/

            // Copy data to GPU buffer
            //_depthBuffer = depthZ;
            //_colorBuffer = rgba;
            _depthCBuffer.SetData(depth);
            _colorCBuffer.SetData(rgba);
            RebuildCloud();
            //Array.Copy(depthZ, _depthBuffer, depthZ.Length);
            //Array.Copy(rgba, _colorBuffer, rgba.Length);

            // Signal the main thread that there is fresh data.
            lock (_dataReadyLock)
            {
                _dataReady = true;
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || _material == null)
            {
                return;
            }

            _material.SetFloat(NearPlaneProp, _ZNear);
            _material.SetFloat(FarPlaneProp, _ZFar);
            _material.SetFloat(ParticleSizeProp, _particleSize);
            _computeShader.SetFloat("scale", _scale);
            _computeShader.SetInt("flip_x", _flipX ? 1 : -1);
            _computeShader.SetInt("flip_y", _flipY ? 1 : -1);
        }

        /// <summary>
        /// Recalculates the bounding box and the projection matrix.
        /// </summary>
        private void UpdateCameraParameters()
        {
            float tz = _ZFar - _ZNear;
            float ty = Mathf.Tan(_verticalFOV * Mathf.Deg2Rad / 2.0f) * _ZFar * 2.0f;
            float tx = ty * _aspectRatio;

            // The axis-aligned bounding box actually fits the frustum's oriented bounding box, so it tends to over
            // estimate the size of the box. But as this is just used by Unity for culling the whole cloud, it's not
            // a big issue.
            Vector3 centre = transform.TransformPoint(0.0f, 0.0f, (_ZFar - _ZNear) / 2.0f + _ZNear);
            Vector3[] corners = {
                transform.TransformVector(tx, ty, tz),
                transform.TransformVector(tx, ty, -tz),
                transform.TransformVector(tx, -ty, tz),
                transform.TransformVector(tx, -ty, -tz),
                transform.TransformVector(-tx, ty, tz),
                transform.TransformVector(-tx, ty, -tz),
                transform.TransformVector(-tx, -ty, tz),
                transform.TransformVector(-tx, -ty, -tz)
            };

            Vector3 extents = new Vector3();

            for (int i = 0; i < 8; ++i)
            {
                extents.x = Mathf.Max(extents.x, corners[i].x);
                extents.y = Mathf.Max(extents.y, corners[i].y);
                extents.z = Mathf.Max(extents.z, corners[i].z);
            }

            _boundingBox = new Bounds(centre, extents);

            Matrix4x4 proj = Matrix4x4.Perspective(_verticalFOV, _aspectRatio, _ZNear, _ZFar);
            // Used to make the projection matrix work regardless of rendering API.
            _cameraTransform = GL.GetGPUProjectionMatrix(proj, false);
        }

        /// <summary>
        /// Runs the compute shader with the latest camera matrix and buffers.
        /// </summary>
        private void RebuildCloud()
        {
            _computeShader.SetMatrix("camera_p", _cameraTransform);
            _computeShader.SetMatrix("camera_p_inv", _cameraTransform.inverse);
            _computeShader.SetMatrix("camera_v_inv", transform.worldToLocalMatrix.inverse);
            _computeShader.Dispatch(_displacementKernel, (_imageWidth + 7) / 8, (_imageHeight + 7) / 8, 1);
        }

        private void Update()
        {
            bool initReady = false;
            lock (_isInitLock)
            {
                initReady = _isInit;
            }
            if (initReady) 
            {
                //UpdateCameraParameters();

                if (_dataReady)
                {
                    // If there's new data, upload it to the GPU then tell the compute shader to run.
                    //_depthCBuffer.SetData(_depthBuffer);
                    //_colorCBuffer.SetData(_colorBuffer);
                    //RebuildCloud();

                    // Once we've updated the cloud, signal that we're ready for more data.
                    /*lock (_dataReadyLock)
                    {
                        _dataReady = false;
                    }*/
                }

                Graphics.DrawProcedural(_material, _boundingBox, MeshTopology.Quads, _particleCBuffer.count,
                    1, null, null, ShadowCastingMode.Off, false);
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // Draw the axis-aligned bounding box.
            Gizmos.DrawWireCube(_boundingBox.center, _boundingBox.size);

            // Draw the camera's frustum.
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, _verticalFOV, _ZFar, _ZNear, _aspectRatio);
        }

        private void OnDestroy()
        {
            bool wasInit;
            lock (_isInitLock)
            {
                wasInit = _isInit;
                _isInit = false;
            }
            if (wasInit)
            {
                _depthCBuffer.Dispose();
                _particleCBuffer.Dispose();
                _colorCBuffer.Dispose();
            }
        }
    }
}
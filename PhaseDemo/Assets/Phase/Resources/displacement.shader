//!
// @authors Ben Knight (bknight@i3drobotics.com)
// @date 2021-05-26
// @copyright Copyright (c) I3D Robotics Ltd, 2021
// 
// @file displacement.shader
// @brief Shader for depth point cloud
//

Shader "I3DR/displacement" {
    Properties {
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // We have to target at least 4.5 for StructuredBuffers.
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct quad_vert
            {
                float4 position;
                float2 corner_coordinates;
                uint particle_id;
            };

            struct appdata
            {
                uint id : SV_VertexID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR0;
            };

            uniform float particle_size;
            uniform float near_plane;
            uniform float far_plane;
            uniform float2 image_size;

            #ifdef SHADER_API_D3D11
            uniform StructuredBuffer<quad_vert> particle_buffer;
            uniform StructuredBuffer<uint> color_buffer;
            uniform StructuredBuffer<float> depth_buffer;
            #endif

            // Unpack a 32-bit int into colour components.
            fixed4 uint_to_fixed4(uint a)
            {
                fixed4 result;
                //result.r = GammaToLinearSpace(((a >> 0) & 0xFF) / 255.0);
                //result.g = GammaToLinearSpace(((a >> 8) & 0xFF) / 255.0);
                //result.b = GammaToLinearSpace(((a >> 16) & 0xFF) / 255.0);
                result.r = ((a >> 0) & 0xFF) / 255.0;
                result.g = ((a >> 8) & 0xFF) / 255.0;
                result.b = ((a >> 16) & 0xFF) / 255.0;
                result.a = ((a >> 24) & 0xFF) / 255.0;
                return result;
            }

            // Calculate the index into an array given its x and y coordinates.
            uint IndexToID(uint2 index)
            {
                return index.y * image_size.x + index.x;
            }

            // Calculate the x and y coordinates given an array index.
            uint2 IDToIndex(uint id)
            {
                return uint2(id % image_size.x, id / image_size.x);
            }

            v2f vert(const appdata v)
            {
                v2f o;

                quad_vert qv = particle_buffer[v.id];

                const float4 position = qv.position;

                // Get the distance of the particle from the render camera to calculate the screen-space size in pixels.
                const float depth = UnityWorldToViewPos(position).z;
                const float2 size = float2(depth / _ScreenParams.x, depth / _ScreenParams.y) * particle_size;

                // Project the particle to screen-space, expanding out the corners of each particle.
                o.vertex = UnityWorldToClipPos(position) + float4(qv.corner_coordinates * size, 0.0, 0.0);

                o.color = uint_to_fixed4(color_buffer[qv.particle_id]);

                // Near/far clipping.
                if (depth_buffer[qv.particle_id] > far_plane || depth_buffer[qv.particle_id] < near_plane)
                {
                    // We use an alpha of 0 to indicate the particle mustn't be drawn.
                    o.color.a = 0.0;
                }

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Discard this fragment if it's not to be drawn. This isn't super-efficient and we should eventually
                // contrive a way to cull it in the compute shader, way before it gets here.
                if (i.color.a == 0.0)
                {
                    discard;
                }

                return i.color;
            }
            ENDCG
        }
    }
}
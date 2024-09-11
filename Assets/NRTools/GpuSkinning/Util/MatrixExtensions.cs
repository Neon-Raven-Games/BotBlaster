using UnityEngine;

namespace NRTools.GpuSkinning.Util
{
    public static class MatrixExtensions
    {
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward = matrix.GetColumn(2);
            Vector3 upwards = matrix.GetColumn(1);

            // Check if forward or upwards are degenerate (zero length or nearly zero length)
            if (forward.sqrMagnitude < Mathf.Epsilon || upwards.sqrMagnitude < Mathf.Epsilon)
            {
                Debug.LogWarning("Invalid forward or upwards vector detected in matrix. Returning identity rotation.");
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector4 position;
            position = matrix.GetColumn(3);
            position.w = 1;
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }
        public static float[] ToFloatArray(this Matrix4x4 matrix)
        {
            return new float[]
            {
                matrix.m00, matrix.m01, matrix.m02, matrix.m03,
                matrix.m10, matrix.m11, matrix.m12, matrix.m13,
                matrix.m20, matrix.m21, matrix.m22, matrix.m23,
                matrix.m30, matrix.m31, matrix.m32, matrix.m33
            };
        }

        public static Matrix4x4 ToMatrix4x4(this float[] array)
        {
            if (array.Length != 16)
            {
                Debug.LogError("Invalid array length for Matrix4x4");
                return Matrix4x4.identity;
            }
    
            return new Matrix4x4
            {
                m00 = array[0],  m01 = array[1],  m02 = array[2],  m03 = array[3],
                m10 = array[4],  m11 = array[5],  m12 = array[6],  m13 = array[7],
                m20 = array[8],  m21 = array[9],  m22 = array[10], m23 = array[11],
                m30 = array[12], m31 = array[13], m32 = array[14], m33 = array[15]
            };
        }

    }
}
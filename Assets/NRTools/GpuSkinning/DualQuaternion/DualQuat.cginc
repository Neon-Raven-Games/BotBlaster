struct dual_quaternion
{
    float4 rotation_quaternion;
    float4 translation_quaternion;
};

float4 quaternion_invert(float4 q)
{
    q.xyz *= -1;
    return q;
}

float4 quaternion_multiply(float4 q1, float4 q2)
{
    const float w = q1.w * q2.w - dot(q1.xyz, q2.xyz);
    q1.xyz = q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz);
    q1.w = w;
    return q1;
}

dual_quaternion dual_quaternion_multiply(const dual_quaternion dq1, const dual_quaternion dq2)
{
    dual_quaternion result;

    result.translation_quaternion = quaternion_multiply(dq1.rotation_quaternion, dq2.translation_quaternion) +
        quaternion_multiply(dq1.translation_quaternion, dq2.rotation_quaternion);

    result.rotation_quaternion = quaternion_multiply(dq1.rotation_quaternion, dq2.rotation_quaternion);

    const float mag = length(result.rotation_quaternion);
    result.rotation_quaternion /= mag;
    result.translation_quaternion /= mag;

    return result;
}

dual_quaternion dual_quaternion_shortest_path(dual_quaternion dq1, const dual_quaternion dq2)
{
    const bool is_bad_path = dot(dq1.rotation_quaternion, dq2.rotation_quaternion) < 0;
    dq1.rotation_quaternion = is_bad_path ? -dq1.rotation_quaternion : dq1.rotation_quaternion;
    dq1.translation_quaternion = is_bad_path ? -dq1.translation_quaternion : dq1.translation_quaternion;
    return dq1;
}

float4 quaternion_apply_rotation(float4 v, float4 rotQ)
{
    v = quaternion_multiply(rotQ, v);
    return quaternion_multiply(v, quaternion_invert(rotQ));
}

inline float sign_no_zero(const float x)
{
    const float s = sign(x);
    return s != 0.0 ? s : 1.0;
}

dual_quaternion dual_quaternion_from_matrix4_x4(float4x4 m)
{
    dual_quaternion dq;

    // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
    dq.rotation_quaternion.w = sqrt(max(0, 1.0 + m[0][0] + m[1][1] + m[2][2])) / 2.0;
    dq.rotation_quaternion.x = sqrt(max(0, 1.0 + m[0][0] - m[1][1] - m[2][2])) / 2.0;
    dq.rotation_quaternion.y = sqrt(max(0, 1.0 - m[0][0] + m[1][1] - m[2][2])) / 2.0;
    dq.rotation_quaternion.z = sqrt(max(0, 1.0 - m[0][0] - m[1][1] + m[2][2])) / 2.0;
    dq.rotation_quaternion.x *= sign_no_zero(m[2][1] - m[1][2]);
    dq.rotation_quaternion.y *= sign_no_zero(m[0][2] - m[2][0]);
    dq.rotation_quaternion.z *= sign_no_zero(m[1][0] - m[0][1]);

    dq.rotation_quaternion = normalize(dq.rotation_quaternion); // ensure unit quaternion

    dq.translation_quaternion = float4(m[0][3], m[1][3], m[2][3], 0);
    dq.translation_quaternion = quaternion_multiply(dq.translation_quaternion, dq.rotation_quaternion) * 0.5;

    return dq;
}

#include <distance_joint.h>

DllExport Vector2 GetJointAnchorA(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2DistanceJoint*>(joint)->GetAnchorA();
    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointAnchorB(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2DistanceJoint*>(joint)->GetAnchorB();
    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt)
{
    b2Vec2 vv = static_cast<b2DistanceJoint*>(joint)->GetReactionForce(inv_dt);
    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt)
{
    return static_cast<b2DistanceJoint*>(joint)->GetReactionTorque(inv_dt);
}

DllExport float GetJointLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetLength();
}

DllExport float SetJointLength(b2Joint* joint, float length)
{
    return static_cast<b2DistanceJoint*>(joint)->SetLength(length);
}

DllExport float GetJointMinLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetMinLength();
}

DllExport float SetJointMinLength(b2Joint* joint, float minLength)
{
    return static_cast<b2DistanceJoint*>(joint)->SetMinLength(minLength);
}

DllExport float GetJointMaxLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetMaxLength();
}

DllExport float SetJointMaxLength(b2Joint* joint, float maxLength)
{
    return static_cast<b2DistanceJoint*>(joint)->SetMaxLength(maxLength);
}

DllExport float GetJointCurrentLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetCurrentLength();
}

DllExport void SetJointStiffness(b2Joint* joint, float stiffness)
{
    return static_cast<b2DistanceJoint*>(joint)->SetStiffness(stiffness);
}

DllExport float GetJointStiffness(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetStiffness();
}

DllExport void SetJointDamping(b2Joint* joint, float damping)
{
    return static_cast<b2DistanceJoint*>(joint)->SetDamping(damping);
}

DllExport float GetJointDamping(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetDamping();
}

// this is collection of useful static math functions
//--------------------------------------------------------------------------------------------------//


using Unity.Mathematics;
using System.Runtime.CompilerServices;

public static class MathUtils
{
    // These mysterious kotay bits are used to access a bit mask used when calculating the power of 2 (log2(n)).
    static readonly byte[] KotayBits = { 0, 1, 2, 16, 3, 6, 17, 21, 14, 4, 7, 9, 18, 11, 22, 26, 31, 15, 5, 20, 13, 8, 10, 25, 30, 19, 12, 24, 29, 23, 28, 27 };

    // there are 31 slots in this array, where each slot corresponds with a power of 2.
    // the array contains the inverse of each power.
    static readonly float[] InversePowers = {
        1f, 1f/2f, 1f/4f, 1f/8f, 1f/16f, 1f/32f, 1f/64f, 1f/128f, 1f/256f, 1f/512f, 1f/1024f, 1f/2048f, 1f/4096f, 1f/8192f, 1f/16384f, 1f/32768f, 1f/65536f, 1f/131072f, 1f/262144f, 1f/524288f, 1f/1048576f, 
        1f/2097152f, 1f/4194304f, 1f/8388608f, 1f/16777216f, 1f/33554432f, 1f/67108864f, 1f/134217728f, 1f/268435456f, 1f/536870912f, 1f/1073741824f, 1f/2147483648f, 1f/4294967296f
    };

    public const float kEpsilonNormalSqrt = 1e-15F;

    // WARNING - This ONLY works if the numerator is an unsigned int, and the divisor is a power of 2!.
    // WARNING - Also note that the return value is a uint - NOT a float or an int!.
    // See http://sree.kotay.com/2007/04/shift-registers-and-de-bruijn-sequences_10.html
    // It is slightly faster than the float version of this function (one less lookup and shift instead of multiply).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint FastIntegerDivideByPow2(uint num, uint den)
    {
        //0x04ad19df --> 78453215 (special kotay bit mask).
        // the kotay bits give us a super fast way of doing a log2(n) (aka finding the power of 2 for n).
        return num >> KotayBits[den * 78453215U >> 27];
    }

    // WARNING - This ONLY works if the divisor is a power of 2!
    // den --> the largest possible value is 2^31 (2147483648).
    // The return value is a float.
    // It is slightly slower than the integer version of this function (extra lookup and multiply instead of shift).
    // See http://sree.kotay.com/2007/04/shift-registers-and-de-bruijn-sequences_10.html
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastDivideByPow2(float num, uint den)
    {
        return InversePowers[KotayBits[den * 78453215U >> 27]] * num;
    }

    // same as above, except this accepts a float3
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 FastDivideByPow2(float3 num, uint den)
    {
        return new float3(FastDivideByPow2(num.x, den), FastDivideByPow2(num.y, den), FastDivideByPow2(num.z, den));
    }

    // returns the interpolated version
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Interpolate(float x, float x0, float x1, float y0, float y1)
    {
        return (y0 + (y1 - y0) * (x - x0) / (x1 - x0));
    }

    // returns true if val is a power of 2
    // warning! if val is zero, this will incorrectly return true.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOfTwo(uint val)
    {
        return (val & (val - 1)) == 0;
    }

    // returns true if val is a power of 2
    // this version is slightly slower (uses a branch), but it works even if val is zero.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOfTwoSafe(uint val)
    {
        return (val != 0) && ((val & (val - 1)) == 0);
    }

    public static float3 AngularVelocityToTarget(in quaternion fromRotation, in float3 toDirection, float turnSpeed, in float3 up)
    {
        var wanted = quaternion.LookRotation(toDirection, up);
        wanted = math.normalizesafe(wanted);
        return AngularVelocityToTarget(fromRotation, wanted, turnSpeed);
    }

    // returns the anglular velocity required to move from one quaternion to another in order to target it with the given turn-speed
    public static float3 AngularVelocityToTarget(in quaternion fromRotation, in quaternion toRotation, float turnSpeed)
    {
        quaternion delta = math.mul(toRotation, math.inverse(fromRotation));
        delta = math.normalizesafe(delta);
        float3 axis = ToAngleAxis(delta, out float angle);
        // We get an infinite axis in the event that our rotation is already aligned.
        if (float.IsInfinity(axis.x)) { return default; }
        if (angle > 180f) { angle -= 360f; }
        // Here I drop down to 0.9f times the desired movement,
        // since we'd rather undershoot and ease into the correct angle
        // than overshoot and oscillate around it in the event of errors.
        return (math.radians(0.9f) * angle / turnSpeed) * math.normalizesafe(axis);
    }

    // given a quaternion, returns the angle axis in degrees
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAngleAxis(in quaternion q)
    {
        return math.degrees(2.0f * (float)math.acos(q.value.w));
    }

    // returns the axis and fills angle (in degrees)
    public static float3 ToAngleAxis(in quaternion q, out float angle)
    {
        quaternion qSafe = math.normalizesafe(q);
        angle = 2.0f * (float)math.acos(qSafe.value.w);
        angle = math.degrees(angle);
        float den = (float)math.sqrt(1.0 - qSafe.value.w * qSafe.value.w);
        if (den > 0.0001f) {
            return qSafe.value.xyz / den;
        }
        // This occurs when the angle is zero.
        // Not a problem: just set an arbitrary normalized axis.
        return new float3(1, 0, 0);
    }

    // Returns the Euler angles (in radians) between two quaternions
    public static float3 EstimateAnglesBetween(in quaternion from, in quaternion to)
    {
        float3 fromImag = new float3(from.value.x, from.value.y, from.value.z);
        float3 toImag = new float3(to.value.x, to.value.y, to.value.z);

        float3 angle = math.cross(fromImag, toImag);
        angle -= to.value.w * fromImag;
        angle += from.value.w * toImag;
        angle += angle;
        return math.dot(toImag, fromImag) < 0 ? -angle : angle;
    }

    // Returns the signed angle in degrees from 180 to -180 between two float3s.
    // the axis is used to determine the sign
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAngleSigned(in float3 from, in float3 to, in float3 axis)
    {
        float angle = math.acos(math.dot(math.normalize(from), math.normalize(to)));
        float sign = math.sign(math.dot(axis, math.cross(from, to)));
        return math.degrees(angle * sign);
    }

    //// Returns the angle in degrees from 0 to 180 between two float3s.
    //public static float CalculateAngleAndNormalize(float3 from, float3 to)
    //{
    //    // theta = cos-1 [ (a · b) / (|a| |b|) ]
    //    return 180f - math.degrees(math.acos(math.dot(math.normalize(from), math.normalize(to))));
    //}

    // Returns the angle in degrees between from and to.
    // This was take from Vector3.Angle source code
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAngle(float3 from, float3 to)
    {
        // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
        float denominator = (float)math.sqrt(math.lengthsq(from) * math.lengthsq(to));
        if (denominator < kEpsilonNormalSqrt) { return 0f; }
        float dot = math.clamp(math.dot(from, to) / denominator, -1f, 1f);
        return math.degrees(math.acos(dot));
    }

    // Returns the signed angle in degrees between from and to.
    // Always returns the smallest possible angle (-180 to 180)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAngleSigned(float3 from, float3 to)
    {
        float unsignedAngle = CalculateAngle(from, to);
        float sign = math.sign(from.x * to.y - from.y * to.x);
        return unsignedAngle * sign;
    }

    // Sign function that has the old Mathf behaviour of returning 1 if f == 0
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ZeroIsOneSign(float f)
    {
        return f >= 0F ? 1F : -1F;
    }

    //// Returns the angle in degrees from 0 to 180 between two float3s.
    //public static float CalculateAngle(float3 from, float3 to)
    //{
    //    // theta = cos-1 [ (a · b) / (|a| |b|) ]
    //    return 180f - math.degrees(math.acos(math.dot(from, to)));
    //}

    // given two quaterions, returns the angle between them
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateAngle(in quaternion q1, in quaternion q2)
    {
        var dot = math.dot(q1, q2);
        return !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
    }

    // lerps an angle with amout t between a and b
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpAngle(float a, float b, float t)
    {
        float dif = b - a;
        float delta = math.clamp(dif - math.floor(dif / 360) * 360, 0.0f, 360);
        if (delta > 180) { delta -= 360; }
        return a + delta * math.clamp(t,0f, 1f);
    }

    // rotate/slerp the quaternion from one quaterion to another at the specified max degrees
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion RotateTowards(in quaternion from, in quaternion to, float maxDegreesDelta)
    {
        float num = CalculateAngle(from, to);
        return num <= float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
    }

    // scales val, which is between oldMin and oldMax, to a number between newMin and newMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Scale(float val, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (((val - oldMin) / (oldMax - oldMin)) * (newMax - newMin)) + newMin;
    }

    // scales val, which is between oldMin and oldMax, to a vector between newMin and newMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 Scale(in float3 val, in float3 oldMin, in float3 oldMax, in float3 newMin, in float3 newMax)
    {
        return new float3(Scale(val.x, oldMin.x, oldMax.x, newMin.x, newMax.x), Scale(val.y, oldMin.y, oldMax.y, newMin.y, newMax.y), Scale(val.z, oldMin.z, oldMax.z, newMin.z, newMax.z));
    }

    // returns val scaled between 0 and 1
    // val MUST be between oldMin and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToUnitInterval(float val, float oldMin, float oldMax)
    {
        return (val - oldMin) / (oldMax - oldMin);
    }
    // returns val scaled between 0 and 1
    // if val is smaller than oldMin it will be clamped to 0
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToClampedUnitInterval(float val, float oldMin, float oldMax)
    {
        return (math.clamp(val, oldMin, oldMax) - oldMin) / (oldMax - oldMin);
    }

    // returns val scaled between 0 and 1
    // val MUST be between 0 and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToUnitInterval(float val, float oldMax)
    {
        return val / oldMax;
    }
    // returns val scaled between 0 and 1
    // if val is smaller than 0 it will be clamped to 0
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToClampedUnitInterval(float val, float oldMax)
    {
        return math.clamp(val, 0, oldMax) / oldMax;
    }

    // returns val scaled between 0 and 1
    // val MUST be between oldMin and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToUnitInterval(in float3 val, in float3 oldMin, in float3 oldMax)
    {
        return new float3(ScaleToUnitInterval(val.x, oldMin.x, oldMax.x), ScaleToUnitInterval(val.y, oldMin.y, oldMax.y), ScaleToUnitInterval(val.z, oldMin.z, oldMax.z));
    }
    // returns val scaled between 0 and 1
    // if val is smaller than oldMin it will be clamped to 0
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToClampedUnitInterval(in float3 val, in float3 oldMin, in float3 oldMax)
    {
        return new float3(ScaleToClampedUnitInterval(val.x, oldMin.x, oldMax.x), ScaleToClampedUnitInterval(val.y, oldMin.y, oldMax.y), ScaleToClampedUnitInterval(val.z, oldMin.z, oldMax.z));
    }

    // returns val scaled between 0 and 1
    // val MUST be between oldMin and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToUnitInterval(in float3 val, in float3 oldMax)
    {
        return new float3(ScaleToUnitInterval(val.x, oldMax.x), ScaleToUnitInterval(val.y, oldMax.y), ScaleToUnitInterval(val.z, oldMax.z));
    }
    // returns val scaled between 0 and 1
    // if val is smaller than 0 it will be clamped to 0
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToClampedUnitInterval(in float3 val, in float3 oldMax)
    {
        return new float3(ScaleToClampedUnitInterval(val.x, oldMax.x), ScaleToClampedUnitInterval(val.y, oldMax.y), ScaleToClampedUnitInterval(val.z, oldMax.z));
    }

    // returns val scaled between -1 and 1
    // val MUST be between oldMin and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToUnitRange(float val, float oldMin, float oldMax)
    {
        return (((val - oldMin) / (oldMax - oldMin)) * 2) - 1;
    }
    // returns val scaled between -1 and 1
    // if val is smaller than oldMin it will be clamped to -1
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToClampedUnitRange(float val, float oldMin, float oldMax)
    {
        return (((math.clamp(val, oldMin, oldMax) - oldMin) / (oldMax - oldMin)) * 2) - 1;
    }

    // returns val scaled between -1 and 1
    // val MUST be between 0 and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToUnitRange(float val, float oldMax)
    {
        return ((val / oldMax) * 2) - 1;
    }
    // returns val scaled between -1 and 1
    // if val is smaller than 0 it will be clamped to -1
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleToClampedUnitRange(float val, float oldMax)
    {
        return ((math.clamp(val, 0, oldMax) / oldMax) * 2) - 1;
    }

    // returns val scaled between -1 and 1
    // val MUST be between 0 and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToUnitRange(in float3 val, in float3 oldMax)
    {
        return new float3(ScaleToUnitRange(val.x, oldMax.x), ScaleToUnitRange(val.y, oldMax.y), ScaleToUnitRange(val.z, oldMax.z));
    }
    // returns val scaled between -1 and 1
    // if val is smaller than 0 it will be clamped to -1
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToClampedUnitRange(in float3 val, in float3 oldMax)
    {
        return new float3(ScaleToClampedUnitRange(val.x, oldMax.x), ScaleToClampedUnitRange(val.y, oldMax.y), ScaleToClampedUnitRange(val.z, oldMax.z));
    }

    // returns val scaled between -1 and 1
    // val MUST be between oldMin and oldMax
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToUnitRange(in float3 val, in float3 oldMin, in float3 oldMax)
    {
        return new float3(ScaleToUnitRange(val.x, oldMin.x, oldMax.x), ScaleToUnitRange(val.y, oldMin.y, oldMax.y), ScaleToUnitRange(val.z, oldMin.z, oldMax.z));
    }
    // returns val scaled between -1 and 1
    // if val is smaller than oldMin it will be clamped to -1
    // if val is larger than oldMax it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleToClampedUnitRange(in float3 val, in float3 oldMin, in float3 oldMax)
    {
        return new float3(ScaleToClampedUnitRange(val.x, oldMin.x, oldMax.x), ScaleToClampedUnitRange(val.y, oldMin.y, oldMax.y), ScaleToClampedUnitRange(val.z, oldMin.z, oldMax.z));
    }


    // returns val scaled between newMin and newMax
    // val MUST be between -1 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromUnitRange(float val, float newMin, float newMax)
    {
        return (((val + 1) * 0.5f) * (newMax - newMin)) + newMin;
    }
    // returns val scaled between newMin and newMax
    // if val is less than -1, it will be clamped to -1
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromClampedUnitRange(float val, float newMin, float newMax)
    {
        return (((math.clamp(val, -1, 1) + 1) * 0.5f) * (newMax - newMin)) + newMin;
    }

    // returns val scaled between 0 and newMax
    // val MUST be between -1 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromUnitRange(float val, float newMax)
    {
        return ((val + 1) * 0.5f) * newMax;
    }
    // returns val scaled between 0 and newMax
    // if val is less than -1, it will be clamped to -1
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromClampedUnitRange(float val, float newMax)
    {
        return ((math.clamp(val, -1, 1) + 1) * 0.5f) * newMax;
    }

    // returns val scaled between newMin and newMax
    // val MUST be between -1 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromUnitRange(in float3 val, in float3 newMin, in float3 newMax)
    {
        return new float3(ScaleFromUnitRange(val.x, newMin.x, newMax.x), ScaleFromUnitRange(val.y, newMin.y, newMax.y), ScaleFromUnitRange(val.z, newMin.z, newMax.z));
    }
    // returns val scaled between newMin and newMax
    // if val is less than -1, it will be clamped to -1
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromClampedUnitRange(in float3 val, in float3 newMin, in float3 newMax)
    {
        return new float3(ScaleFromClampedUnitRange(val.x, newMin.x, newMax.x), ScaleFromClampedUnitRange(val.y, newMin.y, newMax.y), ScaleFromClampedUnitRange(val.z, newMin.z, newMax.z));
    }

    // returns val scaled between newMin and newMax
    // val MUST be between -1 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromUnitRange(in float3 val, in float3 newMax)
    {
        return new float3(ScaleFromUnitRange(val.x, newMax.x), ScaleFromUnitRange(val.y, newMax.y), ScaleFromUnitRange(val.z, newMax.z));
    }
    // returns val scaled between newMin and newMax
    // if val is less than -1, it will be clamped to -1
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromClampedUnitRange(in float3 val, in float3 newMax)
    {
        return new float3(ScaleFromClampedUnitRange(val.x, newMax.x), ScaleFromClampedUnitRange(val.y, newMax.y), ScaleFromClampedUnitRange(val.z, newMax.z));
    }

    // returns val scaled between newMin and newMax
    // val MUST be between 0 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromUnitInterval(float val, float newMin, float newMax)
    {
        return (val * (newMax - newMin)) + newMin;
    }
    // returns val scaled between newMin and newMax
    // if val is less than 0, it will be clamped to 0
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromClampedUnitInterval(float val, float newMin, float newMax)
    {
        return (math.clamp(val, 0, 1) * (newMax - newMin)) + newMin;
    }

    // returns val scaled between newMin and newMax
    // val MUST be between 0 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromUnitInterval(float val, float newMax)
    {
        return val * newMax;
    }
    // returns val scaled between newMin and newMax
    // if val is less than 0, it will be clamped to 0
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ScaleFromClampedUnitInterval(float val, float newMax)
    {
        return math.clamp(val, 0, 1) * newMax;
    }

    // returns val scaled between newMin and newMax
    // val MUST be between 0 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromUnitInterval(in float3 val, in float3 newMin, in float3 newMax)
    {
        return new float3(ScaleFromUnitInterval(val.x, newMin.x, newMax.x), ScaleFromUnitInterval(val.y, newMin.y, newMax.y), ScaleFromUnitInterval(val.z, newMin.z, newMax.z));
    }
    // returns val scaled between newMin and newMax
    // if val is less than 0, it will be clamped to 0
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromClampedUnitInterval(in float3 val, in float3 newMin, in float3 newMax)
    {
        return new float3(ScaleFromClampedUnitInterval(val.x, newMin.x, newMax.x), ScaleFromClampedUnitInterval(val.y, newMin.y, newMax.y), ScaleFromClampedUnitInterval(val.z, newMin.z, newMax.z));
    }

    // returns val scaled between newMin and newMax
    // val MUST be between 0 and 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromUnitInterval(in float3 val, in float3 newMax)
    {
        return new float3(ScaleFromUnitInterval(val.x, newMax.x), ScaleFromUnitInterval(val.y, newMax.y), ScaleFromUnitInterval(val.z, newMax.z));
    }
    // returns val scaled between newMin and newMax
    // if val is less than 0, it will be clamped to 0
    // if val is greater than 1, it will be clamped to 1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ScaleFromClampedUnitInterval(in float3 val, in float3 newMax)
    {
        return new float3(ScaleFromClampedUnitInterval(val.x, newMax.x), ScaleFromClampedUnitInterval(val.y, newMax.y), ScaleFromClampedUnitInterval(val.z, newMax.z));
    }

    // this is just a really fast way to find the largest difference without looping and reusing as many comparision results as possible
    // from bradgonesurfing on stackoverflow - https://stackoverflow.com/questions/19199473/biggest-and-smallest-of-four-integers-no-arrays-no-functions-fewest-if-stat/19199615
    public static float FastFindLargestDifference(float a, float b, float c, float d)
    {
        float min, max;
        // min = a b c d, max = a b c d
        if (a <= b) {
            // min = a c d, max = b c d
            if (c <= d) {
                min = a <= c ? a : c; // min = a c
                max = b > d ? b : d; // max = b d
            } else {
                min = a <= d ? a : d; // min = a d
                max = b > c ? b : c; // max = b c
            }
        } else {
            // min = b c d, max = a c d
            if (c <= d) {
                min = b < c ? b : c; // min = b c
                max = a > d ? a : d; // max = a d
            } else {
                min = b < d ? b : d; // min = b d
                max = a > c ? a : c; // max = a c
            }
        }
        return max - min;
    }

    // same as Mathf.Approximately, except that it takes an epsilon parameter
    // for example... you can use math.EPSILON for epsilon
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(float a, float b, float epsilon)
    {
        return (a >= (b - epsilon)) && (a <= (b + epsilon));
    }

    // same as Mathf.Approximately
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(float a, float b)
    {
        return (float)math.abs(b - a) < (float)math.max(1E-06f * math.max(math.abs(a), math.abs(b)), math.EPSILON * 8f);
    }

    // returns true if all components of the vectors are close to each other
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(in UnityEngine.Vector2 a, in UnityEngine.Vector2 b, float epsilon)
    {
        return Approximately(a.x, b.x, epsilon) && Approximately(a.y, b.y, epsilon);
    }

    // returns true if all components of the vectors are close to each other
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(in float2 a, in float2 b, float epsilon)
    {
        return Approximately(a.x, b.x, epsilon) && Approximately(a.y, b.y, epsilon);
    }

    // returns true if all components of the vectors are close to each other
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(in UnityEngine.Vector2 a, in UnityEngine.Vector2 b)
    {
        return Approximately(a.x, b.x) && Approximately(a.y, b.y);
    }

    // returns true if all components of the vectors are close to each other
    // if epsilon is < 0 then Mathf.Epsilon will be used, otherwise, the epsilon parameter will be used
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(in float3 a, in float3 b, float epsilonSq)
    {
        //return Approximately(a.x, b.x, epsilon) && Approximately(a.y, b.y, epsilon) && Approximately(a.z, b.z, epsilon);
        return math.distancesq(a, b) < epsilonSq;
    }

    // Utility function used by SolveQuadratic, SolveCubic, and SolveQuartic
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(double d)
    {
        return d > -1e-9 && d < 1e-9;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(float3 d)
    {
        return (d.x > -1e-9 && d.x < 1e-9) && (d.y > -1e-9 && d.y < 1e-9) && (d.z > -1e-9 && d.z < 1e-9);
    }

    // returns the cubic root of the specified value
    public static double GetCubicRoot(double value)
    {
        if (value > 0.0) {
            return System.Math.Pow(value, 1.0 / 3.0);
        } else if (value < 0) {
            return -System.Math.Pow(-value, 1.0 / 3.0);
        } else {
            return 0.0;
        }
    }

    // Solve quadratic equation: c0*x^2 + c1*x + c2. 
    // Returns number of solutions.
    // from forest the woods - https://www.forrestthewoods.com/blog/solving_ballistic_trajectories/
    public static int SolveQuadric(double c0, double c1, double c2, out double s0, out double s1)
    {
        s0 = double.NaN;
        s1 = double.NaN;

        double p, q, D;

        // normal form: x^2 + px + q = 0
        p = c1 / (2 * c0);
        q = c2 / c0;

        D = p * p - q;

        if (IsZero(D)) {
            s0 = -p;
            return 1;
        } else if (D < 0) {
            return 0;
        } else { // if (D > 0)
            double sqrt_D = System.Math.Sqrt(D);

            s0 = sqrt_D - p;
            s1 = -sqrt_D - p;
            return 2;
        }
    }

    // Solve cubic equation: c0*x^3 + c1*x^2 + c2*x + c3. 
    // Returns number of solutions.
    // from forest the woods - https://www.forrestthewoods.com/blog/solving_ballistic_trajectories/
    public static int SolveCubic(double c0, double c1, double c2, double c3, out double s0, out double s1, out double s2)
    {
        s0 = double.NaN;
        s1 = double.NaN;
        s2 = double.NaN;

        int num;
        double sub;
        double A, B, C;
        double sq_A, p, q;
        double cb_p, D;

        // normal form: x^3 + Ax^2 + Bx + C = 0
        A = c1 / c0;
        B = c2 / c0;
        C = c3 / c0;

        //  substitute x = y - A/3 to eliminate quadric term:  x^3 +px + q = 0
        sq_A = A * A;
        p = 1.0 / 3 * (-1.0 / 3 * sq_A + B);
        q = 1.0 / 2 * (2.0 / 27 * A * sq_A - 1.0 / 3 * A * B + C);

        // use Cardano's formula
        cb_p = p * p * p;
        D = q * q + cb_p;

        if (IsZero(D)) {
            if (IsZero(q)) { // one triple solution
                s0 = 0;
                num = 1;
            } else { // one single and one double solution
                double u = GetCubicRoot(-q);
                s0 = 2 * u;
                s1 = -u;
                num = 2;
            }
        } else if (D < 0) { // Casus irreducibilis: three real solutions
            double phi = 1.0 / 3 * System.Math.Acos(-q / System.Math.Sqrt(-cb_p));
            double t = 2 * System.Math.Sqrt(-p);

            s0 = t * System.Math.Cos(phi);
            s1 = -t * System.Math.Cos(phi + System.Math.PI / 3);
            s2 = -t * System.Math.Cos(phi - System.Math.PI / 3);
            num = 3;
        } else { // one real solution
            double sqrt_D = System.Math.Sqrt(D);
            double u = GetCubicRoot(sqrt_D - q);
            double v = -GetCubicRoot(sqrt_D + q);

            s0 = u + v;
            num = 1;
        }

        // resubstitute
        sub = 1.0 / 3 * A;
        if (num > 0) { s0 -= sub; }
        if (num > 1) { s1 -= sub; }
        if (num > 2) { s2 -= sub; }

        return num;
    }

    // Solve quartic function: c0*x^4 + c1*x^3 + c2*x^2 + c3*x + c4. 
    // Returns number of solutions.
    public static int SolveQuartic(double c0, double c1, double c2, double c3, double c4, out double s0, out double s1, out double s2, out double s3)
    {
        s0 = double.NaN;
        s1 = double.NaN;
        s2 = double.NaN;
        s3 = double.NaN;

        double[] coeffs = new double[4];
        double z, u, v, sub;
        double A, B, C, D;
        double sq_A, p, q, r;
        int num;

        // normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0
        A = c1 / c0;
        B = c2 / c0;
        C = c3 / c0;
        D = c4 / c0;

        //  substitute x = y - A/4 to eliminate cubic term: x^4 + px^2 + qx + r = 0
        sq_A = A * A;
        p = -3.0 / 8 * sq_A + B;
        q = 1.0 / 8 * sq_A * A - 1.0 / 2 * A * B + C;
        r = -3.0 / 256 * sq_A * sq_A + 1.0 / 16 * sq_A * B - 1.0 / 4 * A * C + D;

        if (IsZero(r)) {
            // no absolute term: y(y^3 + py + q) = 0

            coeffs[3] = q;
            coeffs[2] = p;
            coeffs[1] = 0;
            coeffs[0] = 1;

            num = SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);
        } else {
            // solve the resolvent cubic ...
            coeffs[3] = 1.0 / 2 * r * p - 1.0 / 8 * q * q;
            coeffs[2] = -r;
            coeffs[1] = -1.0 / 2 * p;
            coeffs[0] = 1;

            SolveCubic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], out s0, out s1, out s2);

            // ... and take the one real solution ...
            z = s0;

            // ... to build two quadric equations
            u = z * z - r;
            v = 2 * z - p;

            if (IsZero(u)) {
                u = 0;
            } else if (u > 0) {
                u = System.Math.Sqrt(u);
            } else {
                return 0;
            }

            if (IsZero(v)) {
                v = 0;
            } else if (v > 0) {
                v = System.Math.Sqrt(v);
            } else {
                return 0;
            }

            coeffs[2] = z - u;
            coeffs[1] = q < 0 ? -v : v;
            coeffs[0] = 1;

            num = SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);

            coeffs[2] = z + u;
            coeffs[1] = q < 0 ? v : -v;
            coeffs[0] = 1;

            if (num == 0) {
                num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s0, out s1);
            } else if (num == 1) {
                num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s1, out s2);
            } else if (num == 2) {
                num += SolveQuadric(coeffs[0], coeffs[1], coeffs[2], out s2, out s3);
            }
        }

        // resubstitute
        sub = 1.0 / 4 * A;

        if (num > 0) { s0 -= sub; }
        if (num > 1) { s1 -= sub; }
        if (num > 2) { s2 -= sub; }
        if (num > 3) { s3 -= sub; }

        return num;
    }
}

namespace UnityEngine.VRInteraction
{
    public static class Interpolations
    {
        public enum eInterpolation
        {
            INTERPOLATE_TYPE_LINEAR,
            INTERPOLATE_TYPE_SIGMOID,
            INTERPOLATE_TYPE_4SIN,
            INTERPOLATE_TYPE_SIN_ACEL,
            INTERPOLATE_TYPE_SIN_DECEL,
            INTERPOLATE_TYPE_SPRING_063,
            INTERPOLATE_TYPE_BOUNCE_IN,
            INTERPOLATE_TYPE_BOUNCE_OUT
        }

        public static Vector2 Interpolate(Vector2 from, Vector2 to, float delta, eInterpolation type)
        {
            Vector2 result;
            result.x = Interpolate(from.x, to.x, delta, type);
            result.y = Interpolate(from.y, to.y, delta, type);
            return result;
        }

        public static Vector3 Interpolate(Vector3 from, Vector3 to, float delta, eInterpolation type)
        {
            Vector3 result;
            result.x = Interpolate(from.x, to.x, delta, type);
            result.y = Interpolate(from.y, to.y, delta, type);
            result.z = Interpolate(from.z, to.z, delta, type);
            return result;
        }

        public static Color32 Interpolate(Color32 from, Color32 to, float delta, eInterpolation type)
        {
            Vector4 from_ = new Vector4((float)from.r, (float)from.g, (float)from.b, (float)from.a);
            Vector4 to_ = new Vector4((float)to.r, (float)to.g, (float)to.b, (float)to.a);
            Vector4 result_ = Interpolate(from_, to_, delta, type);
            Color32 result = new Color32((byte)result_.x, (byte)result_.y, (byte)result_.z, (byte)result_.w);
            return result;
        }

        public static Color Interpolate(Color from, Color to, float delta, eInterpolation type)
        {
            Vector4 result;
            result.x = Interpolate(from.r, to.r, delta, type);
            result.y = Interpolate(from.g, to.g, delta, type);
            result.z = Interpolate(from.b, to.b, delta, type);
            result.w = Interpolate(from.a, to.a, delta, type);
            return result;
        }

        public static Vector4 Interpolate(Vector4 from, Vector4 to, float delta, eInterpolation type)
        {
            Vector4 result;
            result.x = Interpolate(from.x, to.x, delta, type);
            result.y = Interpolate(from.y, to.y, delta, type);
            result.z = Interpolate(from.z, to.z, delta, type);
            result.w = Interpolate(from.w, to.w, delta, type);
            return result;
        }

        public static float Interpolate(float from, float to, float delta, eInterpolation type)
        {
            float result = from;

            switch (type)
            {
                case eInterpolation.INTERPOLATE_TYPE_LINEAR:
                    result = LinearInterpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_SIGMOID:
                    result = SigmoidInterpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_4SIN:
                    result = Sin4Interpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_SIN_ACEL:
                    result = SinAcelerateInterpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_SIN_DECEL:
                    result = SinDecelerateInterpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_SPRING_063:
                    result = Spring063Interpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_BOUNCE_IN:
                    result = BounceInInterpolate(from, to, delta);
                    break;
                case eInterpolation.INTERPOLATE_TYPE_BOUNCE_OUT:
                    result = BounceOutInterpolate(from, to, delta);
                    break;
            }

            return result;
        }

        //========================================================================

        static float LinearInterpolate(float from, float to, float delta)
        {
            if (delta <= 0.0f)
                return from;
            else if (delta >= 1.0f)
                return to;

            return (from * (1.0f - delta)) + (to * delta);
        }

        static float SigmoidInterpolate(float from, float to, float delta)
        {
            delta = Mathf.Clamp((delta - 0.5f) * 2.0f, -1.0f, 1.0f);
            float sigmoid = Mathf.Clamp(1.0f / (1.0f + Mathf.Pow(2.718282f, -15.0f * delta)), 0.0f, 1.0f);
            return (from * (1.0f - sigmoid)) + (to * sigmoid);
        }

        static float Sin4Interpolate(float from, float to, float delta)
        {
            if (delta <= 0.0f)
                return from;
            else if (delta >= 1.0f)
                return to;

            float sin_v = (Mathf.Sin(((delta - 0.5f) * Mathf.PI)) + 1) * 0.5f;
            return (from * (1.0f - sin_v)) + (to * sin_v);
        }

        static float SinDecelerateInterpolate(float from, float to, float delta)
        {
            if (delta <= 0.0f)
                return from;
            else if (delta >= 1.0f)
                return to;

            float sin_v = Mathf.Sin(((delta * 0.5f) * Mathf.PI));
            return (from * (1.0f - sin_v)) + (to * sin_v);
        }

        static float SinAcelerateInterpolate(float from, float to, float delta)
        {
            if (delta <= 0.0f)
                return from;
            else if (delta >= 1.0f)
                return to;

            float sin_v = (Mathf.Sin((((delta - 1.0f) * 0.5f) * Mathf.PI))) + 1.0f;
            return (from * (1.0f - sin_v)) + (to * sin_v);
        }

        static float Spring063Interpolate(float from, float to, float delta)
        {
            if (delta <= 0.0f)
                return from;
            else if (delta >= 1.0f)
                return to;

            float sin_v = (-Mathf.Cos((delta + 0.08f) * 10.0f) * (2.0f / (Mathf.Pow(1.9f, (((delta + 0.08f) * 10.0f) * 0.63f))))) + 1.0f;
            return (from * (1.0f - sin_v)) + (to * sin_v);
        }

        static float BounceInInterpolate(float from, float to, float delta)
        {
            return from + (1 - Mathf.Abs(Mathf.Sin(10 + 5.28f * (delta + 0.5f) * (delta + 0.5f)) * (1f - delta)) * (to - from));
        }

        static float BounceOutInterpolate(float from, float to, float delta)
        {
            return from + (Mathf.Abs(Mathf.Sin(10 + 5.28f * (delta + 0.5f) * (delta + 0.5f)) * (1f - delta)) * (to - from));
        }
    }
}

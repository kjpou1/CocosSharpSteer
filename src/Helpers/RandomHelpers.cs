using System;

namespace CocosSharpSteer.Helpers
{
    public class RandomHelpers
    {
        [ThreadStatic]
        private static Random _rng;

        private static Random rng
        {
            get
            {
                if (_rng == null)
                    _rng = new Random();
                return _rng;
            }
        }

        /// <summary>
        /// Returns a float randomly distributed between 0 and 1
        /// </summary>
        /// <returns></returns>
        public static float Random()
        {
            return (float)rng.NextDouble();
        }

        /// <summary>
        /// Returns a float randomly distributed between lowerBound and upperBound
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        public static float Random(float lowerBound, float upperBound)
        {
            return lowerBound + (Random() * (upperBound - lowerBound));
        }

        public static int RandomInt(int min, int max)
        {
            return (int)Random(min, max);
        }
    }
}

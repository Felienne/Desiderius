using System;
using System.Collections.Generic;
using System.Threading;

namespace Sodes.Base
{
    /// <summary>
    /// Thread safe implementation of the framework random generator
    /// </summary>
    public static class RandomGenerator
    {
        private static readonly Random _global = new Random();

        [ThreadStatic]
        private static Random _local;

        /// <summary>
        /// Singleton pattern for each thread
        /// </summary>
        private static Random instance
        {
            get
            {
                if (_local == null)
                {
                    lock (_global)
                    {
                        _local = new Random(_global.Next());
                    }
                }

                return _local;
            }
            set
            {
                _local = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int Next()
        {
            return instance.Next();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int maxValue)
        {
            return instance.Next(maxValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns>A random integer between <paramref name="minValue"/> and <paramref name="maxValue"/></returns>
        public static int Next(int minValue, int maxValue)
        {
            return instance.Next(minValue, maxValue);
        }

        /// <summary>
        /// If there is no need for a specific number but only a decision that has to be taken in a certain percentage of all cases
        /// </summary>
        /// <param name="p"></param>
        /// <returns>True in p% of all calls</returns>
        public static bool Percentage(int p)
        {
            return instance.Next(100) < p;
        }

        /// <summary>
        /// If there is no need for a specific number but only a decision that has to be taken in a certain percentage of all cases
        /// </summary>
        /// <param name="p"></param>
        /// <returns>True in p% of all calls</returns>
        public static bool Percentage(double p)
        {
            return instance.Next(100) < 100 * p;
        }

        /// <summary>
        /// Returns a random number between 0.0 and 1.0
        /// </summary>
        /// <returns>A double between 0.0 and 1.0</returns>
        public static double Percentage()
        {
            return instance.NextDouble();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ResetSeed()
        {
            ResetSeed(0);    // in debug mode I need a repeatable sequence every new deal
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void ResetSeed(int seed)
        {
            instance = new Random(seed);
        }

        public static void RandomSeed()
        {
            instance = null;
        }
    }
}

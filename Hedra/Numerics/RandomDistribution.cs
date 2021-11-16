using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Numerics
{
    public class RandomDistribution : IRandom
    {
        private readonly object _cacheLock;
        private int _inext;
        private int _inextp;
        private int _seed;
        private readonly int[] _seedArray;
        private readonly Dictionary<int, int[]> _seedCache;

        public RandomDistribution() : this(Environment.TickCount)
        {
        }

        public RandomDistribution(bool UseCache) : this(Environment.TickCount)
        {
            if (UseCache)
            {
                _seedCache = new Dictionary<int, int[]>();
                _cacheLock = new object();
            }
        }

        public RandomDistribution(int Seed)
        {
            _seedArray = new int[56];
            this.Seed = Seed;
        }

        public int Seed
        {
            get => _seed;
            set
            {
                if (_seedCache != null)
                    lock (_cacheLock)
                    {
                        if (_seedCache.TryGetValue(value, out var arr))
                        {
                            for (var i = 0; i < arr.Length; ++i)
                                _seedArray[i] = arr[i];
                        }
                        else
                        {
                            FillSeedArray(value);
                            _seedCache.Add(value, _seedArray.ToArray());
                        }
                    }
                else
                    FillSeedArray(value);

                _inext = 0;
                _inextp = 21;
                _seed = value;
            }
        }

        public virtual int Next(int MinValue, int MaxValue)
        {
            if (MinValue > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(MinValue),
                    $"{MinValue} cannot be bigger than {MaxValue}.");
            var num = MaxValue - (long)MinValue;
            if (num <= int.MaxValue)
                return (int)(Sample() * num) + MinValue;
            return (int)((long)(GetSampleForLargeRange() * num) + MinValue);
        }

        public virtual double NextDouble()
        {
            return Sample();
        }

        private void FillSeedArray(int Value)
        {
            var num1 = 161803398 - (Value == int.MinValue ? int.MaxValue : Math.Abs(Value));
            _seedArray[55] = num1;
            var num2 = 1;
            for (var index1 = 1; index1 < 55; ++index1)
            {
                var index2 = 21 * index1 % 55;
                _seedArray[index2] = num2;
                num2 = num1 - num2;
                if (num2 < 0)
                    num2 += int.MaxValue;
                num1 = _seedArray[index2];
            }

            for (var index1 = 1; index1 < 5; ++index1)
            for (var index2 = 1; index2 < 56; ++index2)
            {
                _seedArray[index2] -= _seedArray[1 + (index2 + 30) % 55];
                if (_seedArray[index2] < 0)
                    _seedArray[index2] += int.MaxValue;
            }
        }

        protected virtual double Sample()
        {
            return InternalSample() * 4.6566128752458E-10;
        }

        private int InternalSample()
        {
            var inext = _inext;
            var inextp = _inextp;
            int index1;
            if ((index1 = inext + 1) >= 56)
                index1 = 1;
            int index2;
            if ((index2 = inextp + 1) >= 56)
                index2 = 1;
            var num = _seedArray[index1] - _seedArray[index2];
            if (num == int.MaxValue)
                --num;
            if (num < 0)
                num += int.MaxValue;
            _seedArray[index1] = num;
            _inext = index1;
            _inextp = index2;
            return num;
        }

        private double GetSampleForLargeRange()
        {
            var num = InternalSample();
            if (InternalSample() % 2 == 0)
                num = -num;
            return (num + 2147483646.0) / 4294967293.0;
        }
    }
}
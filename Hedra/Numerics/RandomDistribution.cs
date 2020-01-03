using System;
using System.Collections.Generic;
using System.Linq;

namespace Hedra.Numerics
{
    public class RandomDistribution : IRandom
    {
        private Dictionary<int, int[]> _seedCache;
        private int[] _seedArray;
        private int _seed;
        private int _inext;
        private int _inextp;

        public RandomDistribution() : this(Environment.TickCount)
        {
        }

        public RandomDistribution(bool UseCache) : this(Environment.TickCount)
        {
            if(UseCache) _seedCache = new Dictionary<int, int[]>();
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
                {
                    FillSeedArray(value);
                }

                this._inext = 0;
                this._inextp = 21;
                _seed = value;
            }
        }

        private void FillSeedArray(int Value)
        {
            int num1 = 161803398 - (Value == int.MinValue ? int.MaxValue : System.Math.Abs(Value));
            this._seedArray[55] = num1;
            int num2 = 1;
            for (int index1 = 1; index1 < 55; ++index1)
            {
                int index2 = 21 * index1 % 55;
                this._seedArray[index2] = num2;
                num2 = num1 - num2;
                if (num2 < 0)
                    num2 += int.MaxValue;
                num1 = this._seedArray[index2];
            }
            for (int index1 = 1; index1 < 5; ++index1)
            {
                for (int index2 = 1; index2 < 56; ++index2)
                {
                    this._seedArray[index2] -= this._seedArray[1 + (index2 + 30) % 55];
                    if (this._seedArray[index2] < 0)
                        this._seedArray[index2] += int.MaxValue;
                }
            }
        }

        protected virtual double Sample()
        {
            return (double) this.InternalSample() * 4.6566128752458E-10;
        }

        private int InternalSample()
        {
            int inext = this._inext;
            int inextp = this._inextp;
            int index1;
            if ((index1 = inext + 1) >= 56)
                index1 = 1;
            int index2;
            if ((index2 = inextp + 1) >= 56)
                index2 = 1;
            int num = this._seedArray[index1] - this._seedArray[index2];
            if (num == int.MaxValue)
                --num;
            if (num < 0)
                num += int.MaxValue;
            this._seedArray[index1] = num;
            this._inext = index1;
            this._inextp = index2;
            return num;
        }

        private double GetSampleForLargeRange()
        {
            int num = this.InternalSample();
            if (this.InternalSample() % 2 == 0)
                num = -num;
            return ((double) num + 2147483646.0) / 4294967293.0;
        }

        public virtual int Next(int MinValue, int MaxValue)
        {
            if (MinValue > MaxValue)
                throw new ArgumentOutOfRangeException(nameof(MinValue),
                    $"{MinValue} cannot be bigger than {MaxValue}.");
            long num = (long) MaxValue - (long) MinValue;
            if (num <= (long) int.MaxValue)
                return (int) (this.Sample() * (double) num) + MinValue;
            return (int) ((long) (this.GetSampleForLargeRange() * (double) num) + (long) MinValue);
        }
        
        public virtual double NextDouble()
        {
            return this.Sample();
        }
    }
}
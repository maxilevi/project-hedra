namespace Hedra.Numerics
{
    public interface IRandom
    {
        double NextDouble();

        int Next(int Min, int Max);
    }
}
namespace Hedra.Engine.ComplexMath
{
    public interface IRandom
    {
        double NextDouble();

        int Next(int Min, int Max);
    }
}
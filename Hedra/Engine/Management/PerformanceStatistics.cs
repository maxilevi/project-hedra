using Hedra.Engine.IO;

namespace Hedra.Engine.Management
{
    public static class PerformanceStatistics
    {
        private static double _averageOptimization;
        private static int _optimizationsCounter;

        public static void RegisterMeshOptimization(int Count, int PreviousCount)
        {
            _averageOptimization = (double)Count / PreviousCount;
            _optimizationsCounter++;
        }

        public static void Write()
        {
            Log.WriteLine($"Average vertex reduction = {(int)(_averageOptimization / _optimizationsCounter * 100)} %");
        }
    }
}
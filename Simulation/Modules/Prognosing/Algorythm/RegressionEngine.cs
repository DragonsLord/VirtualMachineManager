using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using Simulation.Models;
using Simulation.Modules.Prognosing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Prognosing.Algorythm
{
    public class RegressionEngine
    {
        public float[] Run(StatisticalDataStream<float> valuesStream, int stepsAmount = GlobalConstants.PROGNOSE_DEPTH)
        {
            var independentCount = GlobalConstants.INDEPENDENT_VALUES_AMOUNT;
            var Vec = Vector<float>.Build;
            var Y = Vec.DenseOfEnumerable(valuesStream.Take(valuesStream.Count - independentCount));
            var X = Matrix<float>.Build.DenseOfRows(
                Enumerable.Range(1, valuesStream.Count - independentCount)
                .Select(offset => valuesStream.GetPartial(offset, independentCount)));
            try
            {
                // https://numerics.mathdotnet.com/Regression.html
                var K = MultipleRegression.NormalEquations(X, Y);

                var prediction = new float[stepsAmount];

                for (int i = 0; i < stepsAmount; i++)
                {
                    Y = Vec.DenseOfEnumerable(valuesStream.Take(independentCount - i).AddBefore(prediction, 0, i));
                    prediction[i] = K.DotProduct(Y);
                }

                return prediction;
            }
            catch (ArgumentException)
            {
                return new float[stepsAmount];
            }
        }
    }
}

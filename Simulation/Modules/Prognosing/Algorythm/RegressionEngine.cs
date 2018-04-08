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
    // TODO: change model - value and length to avoid a lot of same values
    public class RegressionEngine
    {
        public float[] Run(StatisticalDataStream<float> valuesStream, int stepsAmount = GlobalConstants.PROGNOSE_DEPTH)
        {
            var independentCount = GlobalConstants.INDEPENDENT_VALUES_AMOUNT;
            var Vec = Vector<float>.Build;
            var Y = Vec.DenseOfEnumerable(valuesStream.Take(independentCount));
            var X = Matrix<float>.Build.DenseOfRows(
                Enumerable.Range(1, valuesStream.Count - independentCount)
                .Select(offset => valuesStream.GetPartial(offset, independentCount - 1).And(1)));
            try
            {
                // https://numerics.mathdotnet.com/Regression.html
                var K = MultipleRegression.NormalEquations(X, Y);

                var prediction = new float[stepsAmount];

                prediction[0] = K.DotProduct(Y);
                for (int i = 1; i < stepsAmount; i++)
                {
                    Y = Vec.DenseOfEnumerable(valuesStream.Take(independentCount - i).And(prediction[i - 1]));
                    prediction[i] = K.DotProduct(Y);
                }
                return prediction;
            }
            catch (ArgumentException)
            {
                return Y.Reverse().Take(stepsAmount).ToArray();
            }
        }
    }
}

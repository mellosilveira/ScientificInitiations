using IcVibracoes.Core.DTO;
using IcVibracoes.Core.DTO.NumericalMethodInput.FiniteElements;
using IcVibracoes.Core.DTO.NumericalMethodInput.RigidBody;
using IcVibracoes.Core.Mapper;

namespace IcVibracoes.Core.NumericalIntegrationMethods
{
    /// <summary>
    /// It is responsible to execute the numerical integration method to generate the analysis results.
    /// </summary>
    public abstract class NumericalIntegrationMethod : INumericalIntegrationMethod
    {
        private readonly IMappingResolver _mappingResolver;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="mappingResolver"></param>
        protected NumericalIntegrationMethod(IMappingResolver mappingResolver)
        {
            this._mappingResolver = mappingResolver;
        }

        /// <summary>
        /// Calculates the result for the initial time for a finite element analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public abstract FiniteElementResult CalculateFiniteElementResultForInitialTime(FiniteElementMethodInput input);

        /// <summary>
        /// Calculates and write in a file the results for a finite element analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="previousResult"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public abstract FiniteElementResult CalculateFiniteElementResult(FiniteElementMethodInput input,
            FiniteElementResult previousResult, double time);

        /// <summary>
        /// Calculates and write in a file the results for one degree of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public abstract double[] CalculateOneDegreeOfFreedomResult(OneDegreeOfFreedomInput input, double time, double[] previousResult);

        /// <summary>
        /// Calculates and write in a file the results for two degrees of freedom analysis.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="time"></param>
        /// <param name="previousResult"></param>
        /// <returns></returns>
        public virtual double[] CalculateTwoDegreesOfFreedomResult(TwoDegreesOfFreedomInput input, double time,
            double[] previousResult)
        {
            FiniteElementMethodInput finiteElementMethodInput = this._mappingResolver.BuildFiniteElementMethodInput(input);
            FiniteElementResult previousFiniteElementResult = this._mappingResolver.BuildFiniteElementResult(previousResult, input.Force);

            FiniteElementResult finiteElementResult = this.CalculateFiniteElementResult(finiteElementMethodInput, previousFiniteElementResult, time);

            double[] result = this._mappingResolver.BuildVariableVector(finiteElementResult);

            return result;
        }
    }
}

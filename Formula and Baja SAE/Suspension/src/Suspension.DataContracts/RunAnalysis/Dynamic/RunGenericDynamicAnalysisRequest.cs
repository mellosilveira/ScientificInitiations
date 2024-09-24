﻿using MudRunner.Commons.DataContracts.Models.Enums;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.DataContracts.Models;

namespace MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic
{
    /// <summary>
    /// It represents the generic request content of dynamic analysis operation.
    /// </summary>
    public class RunGenericDynamicAnalysisRequest : OperationRequestBase
    {
        /// <summary>
        /// True, if should consider large displacementes. False, otherwise.
        /// </summary>
        public bool ConsiderLargeDisplacements { get; set; }

        /// <summary>
        /// Unit: s (second).
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Unit: s (second).
        /// </summary>
        public double FinalTime { get; set; }

        /// <summary>
        /// An additional information to be set in the file name that contains the analysis results.
        /// </summary>
        public string AdditionalFileNameInformation { get; set; }

        /// <summary>
        /// The available numerical methods for differential equations.
        /// </summary>
        public DifferentialEquationMethodEnum DifferentialEquationMethodEnum { get; set; }

        /// <summary>
        /// The variables to calculate the base excitation at the system.
        /// </summary>
        public BaseExcitation BaseExcitation { get; set; }
    }
}

using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.Domain.Models;
using MelloSilveiraTools.Domain.NumericalMethods.DifferentialEquation;
using MelloSilveiraTools.ExtensionMethods;
using MelloSilveiraTools.Infrastructure.Logger;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MelloSilveiraTools.MechanicsOfMaterials.Models.NumericalMethods;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.DataContracts.RunAnalysis.Dynamic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MudRunner.Suspension.Core.Operations.RunAnalysis.Dynamic;

/// <summary>
/// Runs the dynamic analysis to suspension system.
/// </summary>
public abstract class RunDynamicAnalysis<TRequest>(
    ILogger logger,
    DifferentialEquationMethodFactory differentialEquationMethodFactory,
    IMappingResolver mappingResolver,
    TimeProvider timeProvider) : OperationBaseWithData<TRequest, RunDynamicAnalysisResponseData>(logger)
    where TRequest : RunGenericDynamicAnalysisRequest, new()
{
    /// <summary>
    /// The number of boundary conditions for dynamic analysis.
    /// Dimensionless.
    /// </summary>
    protected abstract uint NumberOfBoundaryConditions { get; }

    /// <summary>
    /// The path for solution file.
    /// </summary>
    protected abstract string SolutionPath { get; }

    /// <summary>
    /// The type of analysis. It can be: quarter car, half car, one car, and more.
    /// </summary>
    protected abstract string AnalysisType { get; }

    /// <summary>
    /// The date and time the operation is instantiated.
    /// </summary>
    protected DateTimeOffset ExecutionDateTime { get; } = timeProvider.GetUtcNow();

    /// <inheritdoc/>
    public uint NumberOfFilesPerRequest => 2;

    /// <inheritdoc/>
    protected async override Task<OperationResponseBase<RunDynamicAnalysisResponseData>> ProcessOperationAsync(TRequest request)
    {
        var response = OperationResponse.CreateSuccessOk<OperationResponseBase<RunDynamicAnalysisResponseData>>();

        // Step 1 - Build the input for numerical method.
        NumericalMethodInput input = await BuildNumericalMethodInputAsync(request).ConfigureAwait(false);

        // Step 2 - Create the full file name for the following files:
        //      Result file that will contain the results from numerical model.
        //      Deformation file that will contain the real results of each boundary condition.

        // Step 2.i - Create the file that will contain the results from numerical model and its folder if they do not exist.
        // The results written in that file represents the displacement, velocity and acceleration of each boundary condition
        // in relation to the origin of the system.
        if (!TryCreateSolutionFile(request.AdditionalFileNameInformation, out string resultFullFileName))
        {
            return OperationResponse.CreateConflict<OperationResponseBase<RunDynamicAnalysisResponseData>>($"The file '{resultFullFileName}' already exist.");
        }

        // Step 2.ii - Create the file that will contain the real results of each boundary condition and at and its
        // folder if they do not exist.
        // The results written in that file represents the real deformation, deformation velocity and deformation acceleration
        // in relation to the origin of the system.
        // OBS.:
        //   The deformation velocity is the deformation derivative on time and represents how the deformation varies on time.
        //   The deformation acceleration is the second deformation derivative on time and represents how the deformation
        //   velocity varies on time.
        if (!TryCreateSolutionFile($"{request.AdditionalFileNameInformation}_Deformation", out string deformationFullFileName))
        {
            return OperationResponse.CreateConflict<OperationResponseBase<RunDynamicAnalysisResponseData>>($"The file '{deformationFullFileName}' already exist.");
        }

        NumericalMethodResult maximumResult = new(NumberOfBoundaryConditions);
        NumericalMethodResult maximumDeformationResult = new(NumberOfBoundaryConditions);

        try
        {
            IDifferentialEquationMethod differentialEquationMethod = differentialEquationMethodFactory.Get(request.DifferentialEquationMethodType);

            using (StreamWriter resultStreamWriter = new(resultFullFileName))
            using (StreamWriter deformationStreamWriter = new(deformationFullFileName))
            {
                // Step 3 - Write the header in the files.
                resultStreamWriter.WriteLine(CreateResultFileHeader());
                deformationStreamWriter.WriteLine(CreateDeformationResultFileHeader());

                // Step 4 - Calculate the result for initial time.
                double time = Constants.InitialTime;
                NumericalMethodResult previousResult = differentialEquationMethod.CalculateResult(input, time, null);
                maximumResult = previousResult;
                maximumDeformationResult = previousResult;

                time += request.TimeStep;
                while (time <= request.FinalTime)
                {
                    // Step 5 - Calculate the results and write it in the file.
                    NumericalMethodResult result = differentialEquationMethod.CalculateResult(input, time, previousResult);
                    if (!request.ConsiderLargeDisplacements)
                    {
                        resultStreamWriter.WriteLine($"{result}");

                        NumericalMethodResult deformationResult = CalculateDeformationResult(request, result, time);
                        deformationStreamWriter.WriteLine($"{deformationResult}");

                        maximumResult = GetMaximumResult(maximumResult, result);
                        maximumDeformationResult = GetMaximumResult(maximumDeformationResult, deformationResult);
                    }
                    else
                    {
                        NumericalMethodResult largeDisplacementResult = BuildLargeDisplacementResult(result);
                        resultStreamWriter.WriteLine($"{largeDisplacementResult}");

                        NumericalMethodResult deformationResult = CalculateDeformationResult(request, largeDisplacementResult, time);
                        deformationStreamWriter.WriteLine($"{deformationResult}");

                        maximumResult = GetMaximumResult(maximumResult, largeDisplacementResult);
                        maximumDeformationResult = GetMaximumResult(maximumDeformationResult, deformationResult);
                    }

                    // Step 6 - Save the current result in the variable 'previousResult' to be used at next step
                    // and iterate the time.
                    previousResult = result;
                    time += input.TimeStep;

                    // Step 7 - Calculates the equivalent force to be used at next step.
                    input.EquivalentForce = BuildEquivalentForceVector(request, time);
                }
            }
        }
        catch (Exception ex)
        {
            return OperationResponse.CreateUnprocessableEntity<OperationResponseBase<RunDynamicAnalysisResponseData>>($"Error trying to calculate and write the result in file. {ex}.");
        }
        finally
        {
            // Step 8 - At the end of the process, it maps the full name of solution file and the maximum result to response
            // and set the success as true and HTTP Status Code as 201 (Created).
            response.Data.FullFileNames.Add(resultFullFileName);
            response.Data.FullFileNames.Add(deformationFullFileName);
            response.Data.MaximumResult = mappingResolver.MapFrom(maximumResult);
            response.Data.MaximumDeformationResult = mappingResolver.MapFrom(maximumDeformationResult);
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<NumericalMethodInput> BuildNumericalMethodInputAsync(TRequest request)
    {
        // Step i - Create the mass, the stiffness and the damping matrixes, and the equivalent force vector.
        double[,] mass = default;
        double[,] stiffness = default;
        double[,] damping = default;
        double[] equivalentForce = default;

        // Step ii - Asynchronously, build the mass, the stiffness and the damping matrixes, and the equivalent force vector.
        List<Task> tasks =
        [
            Task.Run(() => mass = BuildMassMatrix(request)),
            Task.Run(() => stiffness = BuildStiffnessMatrix(request)),
            Task.Run(() => damping = BuildDampingMatrix(request)),
            Task.Run(() => equivalentForce = BuildEquivalentForceVector(request, Constants.InitialTime)),
        ];

        // Step iii - Wait all tasks to be executed.
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Step iv - Map the matrixes and vector to the numerical method input and return it.
        return new()
        {
            Mass = mass,
            Stiffness = stiffness,
            Damping = damping,
            EquivalentForce = equivalentForce,
            NumberOfBoundaryConditions = NumberOfBoundaryConditions,
            TimeStep = request.TimeStep
        };
    }

    /// <inheritdoc/>
    public abstract double[,] BuildMassMatrix(TRequest request);

    /// <inheritdoc/>
    public abstract double[,] BuildDampingMatrix(TRequest request);

    /// <inheritdoc/>
    public abstract double[,] BuildStiffnessMatrix(TRequest request);

    /// <inheritdoc/>
    public abstract double[] BuildEquivalentForceVector(TRequest request, double time);

    /// <inheritdoc/>
    public bool TryCreateSolutionFile(string additionalFileNameInformation, out string fullFileName)
    {
        // TODO: Criar interface para checar se arquivo existe e criar arquivo
        FileInfo fileInfo = new(Path.Combine(
            SolutionPath,
            CreateSolutionFileName(additionalFileNameInformation)));

        if (fileInfo.Exists)
        {
            fullFileName = fileInfo.FullName;
            return false;
        }

        if (!fileInfo.Directory.Exists)
            fileInfo.Directory.Create();

        fullFileName = fileInfo.FullName;
        return true;
    }

    /// <inheritdoc/>
    public abstract string CreateResultFileHeader();

    /// <inheritdoc/>
    public abstract string CreateDeformationResultFileHeader();

    /// <inheritdoc/>
    public abstract NumericalMethodResult BuildLargeDisplacementResult(NumericalMethodResult result);

    /// <inheritdoc/>
    public abstract NumericalMethodResult CalculateDeformationResult(TRequest request, NumericalMethodResult result, double time);

    /// <summary>
    /// Creates the solution file name.
    /// </summary>
    /// <param name="additionalFileNameInformation"></param>
    /// <returns></returns>
    protected string CreateSolutionFileName(string additionalFileNameInformation)
    {
        StringBuilder fileName = new($"{AnalysisType}_DOF-{NumberOfBoundaryConditions}_");

        if (string.IsNullOrWhiteSpace(additionalFileNameInformation) == false)
            fileName.Append($"{additionalFileNameInformation}_");

        fileName.Append($"{ExecutionDateTime:yyyyMMddHHmmss}.csv");

        return fileName.ToString();
    }

    /// <summary>
    /// Returns the maximum results between two <see cref="NumericalMethodResult"/>.
    /// </summary>
    /// <param name="result1"></param>
    /// <param name="result2"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected NumericalMethodResult GetMaximumResult(NumericalMethodResult result1, NumericalMethodResult result2)
    {
        NumericalMethodResult maximumResult = new(NumberOfBoundaryConditions);

        for (int i = 0; i < NumberOfBoundaryConditions; i++)
        {
            maximumResult.Displacement[i] = GetMaximumAbsolutValue(result1.Displacement[i], result2.Displacement[i]);
            maximumResult.Velocity[i] = GetMaximumAbsolutValue(result1.Velocity[i], result2.Velocity[i]);
            maximumResult.Acceleration[i] = GetMaximumAbsolutValue(result1.Acceleration[i], result2.Acceleration[i]);
            maximumResult.EquivalentForce[i] = GetMaximumAbsolutValue(result1.EquivalentForce[i], result2.EquivalentForce[i]);
        }

        return maximumResult;
    }

    /// <summary>
    /// Gets the maximum absulet value between two values.
    /// </summary>
    /// <param name="value1"></param>
    /// <param name="value2"></param>
    /// <returns></returns>
    protected double GetMaximumAbsolutValue(double value1, double value2)
    {
        if (Math.Abs(value1) > Math.Abs(value2))
            return value1;
        else
            return value2;
    }

    /// <inheritdoc/>
    protected override Task<OperationResponseBase<RunDynamicAnalysisResponseData>> ValidateOperationAsync(TRequest request)
    {
        var response = OperationResponse
            .CreateSuccessOk<OperationResponseBase<RunDynamicAnalysisResponseData>>()
            .AddErrorIfNegative(request.TimeStep, $"The time step: '{request.TimeStep}' must be greather than zero.")
            .AddErrorIfNegative(request.FinalTime, $"The final time: '{request.FinalTime}' must be greather than zero.")
            .AddErrorIf(request.TimeStep >= request.FinalTime, $"The time step: '{request.TimeStep}' must be smaller than final time: '{request.FinalTime}'.")
            .AddErrorIfInvalidEnum<OperationResponseBase<RunDynamicAnalysisResponseData>, DifferentialEquationMethodType>(request.DifferentialEquationMethodType, $"Invalid {nameof(request.DifferentialEquationMethodType)}: '{request.DifferentialEquationMethodType}'.");

        if (request.BaseExcitation != null)
        {
            response
                .AddErrorIfNullOrEmpty(request.BaseExcitation.Constants, $"'{nameof(request.BaseExcitation.Constants)}' cannot be null or empty.")
                .AddErrorIfInvalidEnum(request.BaseExcitation.CurveType, $"Invalid curve type: '{request.BaseExcitation.CurveType}'.");

            if (request.BaseExcitation.CurveType == CurveType.Cosine)
            {
                response
                    .AddErrorIfNegativeOrZero(request.BaseExcitation.ObstacleWidth, $"'{nameof(request.BaseExcitation.ObstacleWidth)}' must be greather than zero.")
                    .AddErrorIfZero(request.BaseExcitation.CarSpeed, $"'{nameof(request.BaseExcitation.CarSpeed)}' cannot be zero.");
            }
        }

        return response.AsTask();
    }
}

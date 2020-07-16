# IC Vibracoes App
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## UNRELEASED
### Changed
- Operations in ArrayOperation and BoundaryCondition converted to extension methods.

## [2.3.1] - 2020-07-10
### Fixed
- Folders and files position.
- Namespace of class BoundaryCondition.

## [2.3.0] - 2020-07-09
### Added
- Class OperationErrorCode, containing the error codes that the application can return.
- Class OperationResponseBaseExtensions responsible to build the HTTP response.
### Removed
- Property InitialTime from class OperationRequestBase.
- Class SingleElementRequestData, containing the data of a single element.
- Class File, responsible to execute file operations.
- AuxiliarOperations folder.
### Changed
- Name of properties on class FasteningType from LinearDisplacement and AngularDisplacement to AlowLinearDisplacement and AlowAngularDisplacement.
- Name of properties on class DynamicVibrationAbsorber from DvaMass, DvaStiffness and DvaNodePosition to Mass, Stiffness and NodePosition.
- Name of class OperationResponseBaseExtensions to HttpResponseBuilder.
- Name of property on class RigidBodyRequest from DampingRatioList to DampingRatios.
- Name of class from MechanicalProperties to MechanicalProperty.

## [2.2.0] - 2020-06-26
### Changed
- Variable folderPath to fileUri on methods CreateSolutionPath and CreateMaxValuesPath.
- Path used to save the solutions. 
- Moved obsoleted requests to folder 'Obsoleted' in Request folder.
- Properties on classes RequestData moved to classes Request.
- Class FiniteElementsMethodInput to be used in all FiniteElementsMethod operations.
- Matrix with piezoelectric boundary conditions to consider the beam fastening.
- Constructor of class FiniteElementsMethodInput receives the Numerical Method choosen for the analysis to set the correct values on properties Beta and Gama.
- All names with FiniteElements to FiniteElement.
- Numerical method instantiated in the process operation, instead of in the constructor, to allow the user to decide which numerical method to use in the analysis without having to create an endpoint per numerical method.
- Method ProcessOperation in CalculateVibration_FiniteElement do not calculate natural frequency.
### Removed
- All classes RequestData.
- Classes CentralDifferenceMethodInput, ImplicitAccelerationMethodInput and NewmarkMethodInput.
### Added
- Property NumericalMethod on OperationRequestBase.
- Enum NumericalMethod.
- Property NumericalMethod in class OperationRequestBase.
- Property FileUri in class OperationResponseBase.

## [2.1.0] - 2020-06-04
### Added
- Generic operation to calculate the vibration of structure.
- File class in AuxiliarOperation folder responsible to execute operations evolving files.
- BoundaryCondition class in AuxiliarOperation folder responsible to apply the boundary conditions in matrixes and vectors.
- A single way to calculate the final time.
- Summaries in Swagger.
- Solutions for analysis of beam and beam with piezoelectric with 4 elements to be used in poster.
- Solutions for analysis of beam with piezoelectric for convergence analysis.
- Calculation of the maximum value of the analysis.
### Fixed
- Time step calculation for Runge Kutta Forth Order method.
- Separated linear displacement, angular displacement and additional values when write in file.
- Folders name and folder organization.
### Removed
- AuxiliarOperation class.

## [2.0.1] - 2020-05-07
### Fixed
- CalculateVibration_RigidBody derived classes constructor.

## [2.0.0] - 2020-05-07
### Changed
- Classes and folder name from CalculateVibration to FiniteElements operations.
### Fixed
- Inheritances, classes and folders name and folder organization.
### Added
- Separation to finite element analysis.
- Rigid body analysis.
- Runge Kutta Forth Order numerical integration method.
- Newmark Beta numerical integration method.
- Ways to calculate the natural frequency of structures.
- Ways to calculate the time step and final time.

## [1.3.0] - 2020-04-16
### Changed
- Piezoelectric geometric properties calculations.
- Piezoelectric solution program in C.
### Removed
- Unnecessary rograms in C.
### Fixed
- Application methods based in the unit tests.
- Name of classes.
- Summaries.

## [1.2.0] - 2020-03-20 
### Added
- Create file based in the type of analysis, number of elements and initial and final angular frequency.
- AnalysisType in OperationRequestBase.
- Finite Element Procedures - Klaus-Jurgen Bathe in bibliography.
### Changed
- Input.Parameter receives each parameter of Request.NewmarkMethodParameter.
### Removed
- AnalysisExplanation in OperationRequestBase.

## [1.1.0] - 2020-03-19
### Added
- Programs in C to solution.
- Research presentation.
- Newmark method to beam with dynamic vibration absorber.
- Sumary in classes.
- Validators.
### Changed
- Folder path of projects.
- Simplified ArrayOperation.
- Piezoelectric operations receives position of piezoelectric.

## [1.0.0] - 2020-02-29
### Added
- First version of the program.
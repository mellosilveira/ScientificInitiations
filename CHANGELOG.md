# IC Vibracoes App
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## UNRELEASED
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
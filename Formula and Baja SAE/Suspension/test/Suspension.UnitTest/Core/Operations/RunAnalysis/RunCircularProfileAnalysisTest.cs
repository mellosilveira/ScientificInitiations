using MelloSilveiraTools.Application.Operations;
using MelloSilveiraTools.Infrastructure.Logger;
using MelloSilveiraTools.MechanicsOfMaterials.ConstitutiveEquations;
using MelloSilveiraTools.MechanicsOfMaterials.GeometricProperties;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MelloSilveiraTools.MechanicsOfMaterials.Models.Profiles;
using Moq;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Operations;
using MudRunner.Suspension.Core.Operations.RunAnalysis;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;
using MudRunner.Suspension.UnitTest.Helper;
using System;

namespace MudRunner.Suspension.UnitTest.Core.Operations.RunAnalysis
{
    public class RunCircularProfileAnalysisTest
    {
        private readonly RunStaticAnalysis<CircularProfile> _operation;
        private readonly RunStaticAnalysisRequest<CircularProfile> _requestStub;

        private readonly Mock<CalculateReactions> _calculateReactionsMock;
        private readonly Mock<IConstitutiveEquationsCalculator> _mechanicsOfMaterialsMock;
        private readonly Mock<IGeometricPropertyCalculator<CircularProfile>> _geometricPropertyMock;
        private readonly Mock<IMappingResolver> _mappingResolverMock;

        public RunCircularProfileAnalysisTest()
        {
            _requestStub = RunAnalysisHelper.CreateCircularProfileRequest();

            _calculateReactionsMock = new Mock<CalculateReactions>();
            _calculateReactionsMock
                .Setup(cr => cr.ProcessAsync(It.IsAny<CalculateReactionsRequest>()))
                .ReturnsAsync(OperationResponse.CreateSuccessOk(new CalculateReactionsResponseData
                {
                    LowerWishboneReaction1 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                    LowerWishboneReaction2 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                    UpperWishboneReaction1 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                    UpperWishboneReaction2 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                    ShockAbsorberReaction = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                    TieRodReaction = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 }
                }));

            _mappingResolverMock = new Mock<IMappingResolver>();
            _mappingResolverMock
                .Setup(mr => mr.MapFrom(_requestStub, It.IsAny<CalculateReactionsResponseData>()));

            _mechanicsOfMaterialsMock = new Mock<IConstitutiveEquationsCalculator>();

            _geometricPropertyMock = new Mock<IGeometricPropertyCalculator<CircularProfile>>();

            _operation = new RunStaticAnalysis<CircularProfile>(
                Mock.Of<ILogger>(),
                _calculateReactionsMock.Object,
                _mechanicsOfMaterialsMock.Object,
                _geometricPropertyMock.Object,
                _mappingResolverMock.Object);
        }
    }
}

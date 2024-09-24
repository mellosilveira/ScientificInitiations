﻿using Moq;
using MudRunner.Commons.Core.ConstitutiveEquations.MechanicsOfMaterials;
using MudRunner.Commons.Core.GeometricProperties.CircularProfile;
using MudRunner.Commons.DataContracts.Models;
using MudRunner.Commons.DataContracts.Operation;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Operations.CalculateReactions;
using MudRunner.Suspension.Core.Operations.RunAnalysis.Static.CircularProfile;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.DataContracts.RunAnalysis.Static;
using MudRunner.Suspension.UnitTest.Helper;
using System;
using DataContract = MudRunner.Commons.DataContracts.Models.Profiles;

namespace MudRunner.Suspension.UnitTest.Core.Operations.RunAnalysis.CircularProfile
{
    public class RunCircularProfileAnalysisTest
    {
        private readonly RunCircularProfileStaticAnalysis _operation;
        private readonly RunStaticAnalysisRequest<DataContract.CircularProfile> _requestStub;

        private readonly Mock<ICalculateReactions> _calculateReactionsMock;
        private readonly Mock<IMechanicsOfMaterials> _mechanicsOfMaterialsMock;
        private readonly Mock<ICircularProfileGeometricProperty> _geometricPropertyMock;
        private readonly Mock<IMappingResolver> _mappingResolverMock;

        public RunCircularProfileAnalysisTest()
        {
            this._requestStub = RunAnalysisHelper.CreateCircularProfileRequest();

            this._calculateReactionsMock = new Mock<ICalculateReactions>();
            this._calculateReactionsMock
                .Setup(cr => cr.ProcessAsync(It.IsAny<CalculateReactionsRequest>()))
                .ReturnsAsync(() =>
                {
                    var response = new OperationResponse<CalculateReactionsResponseData>
                    {
                        Data = new CalculateReactionsResponseData
                        {
                            LowerWishboneReaction1 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                            LowerWishboneReaction2 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                            UpperWishboneReaction1 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                            UpperWishboneReaction2 = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                            ShockAbsorberReaction = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 },
                            TieRodReaction = new Force { AbsolutValue = Math.Sqrt(3), X = 1, Y = 1, Z = 1 }
                        }
                    };
                    response.SetSuccessOk();

                    return response;
                });

            this._mappingResolverMock = new Mock<IMappingResolver>();
            this._mappingResolverMock
                .Setup(mr => mr.MapFrom(this._requestStub, It.IsAny<CalculateReactionsResponseData>()));

            this._mechanicsOfMaterialsMock = new Mock<IMechanicsOfMaterials>();

            this._geometricPropertyMock = new Mock<ICircularProfileGeometricProperty>();

            this._operation = new RunCircularProfileStaticAnalysis(
                this._calculateReactionsMock.Object,
                this._mechanicsOfMaterialsMock.Object,
                this._geometricPropertyMock.Object,
                this._mappingResolverMock.Object);
        }
    }
}

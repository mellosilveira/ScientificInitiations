using FluentAssertions;
using Moq;
using MelloSilveiraTools.MechanicsOfMaterials.Models;
using MelloSilveiraTools.Application.Operations;
using MudRunner.Suspension.Core.Mapper;
using MudRunner.Suspension.Core.Models.SuspensionComponents;
using MudRunner.Suspension.DataContracts.CalculateReactions;
using MudRunner.Suspension.UnitTest.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using MudRunner.Suspension.Core.Operations;
using MelloSilveiraTools.Infrastructure.Logger;

namespace MudRunner.Suspension.UnitTest.Core.Operations
{
    public class CalculateReactionsTest
    {
        private readonly Mock<IMappingResolver> _mappingResolverMock;

        private readonly CalculateReactionsRequest _requestStub;
        private readonly OperationResponseBase<CalculateReactionsResponseData> _expectedResponse;
        private readonly SuspensionSystem _suspensionSystem;
        private readonly double[] _reactions;
        private readonly CalculateReactions _operation;

        public CalculateReactionsTest()
        {
            _requestStub = CalculateReactionsHelper.CreateRequest();

            _expectedResponse = CalculateReactionsHelper.CreateResponse();

            _suspensionSystem = CalculateReactionsHelper.CreateSuspensionSystem();

            _reactions = new double[6] { 706.844136886457, -2318.54871728814, -410.390832183452, 693.435739188224, -1046.35331054682, 568.377481091224 };

            _mappingResolverMock = new Mock<IMappingResolver>();
            _mappingResolverMock
                .Setup(mr => mr.MapFrom(_requestStub))
                .Returns(_suspensionSystem);

            _operation = new CalculateReactions(Mock.Of<ILogger>(), _mappingResolverMock.Object);
        }

        [Theory(DisplayName = "Feature: ValidateAsync | Given: Invalid parameters. | When: Call method. | Should: Return a failure for a bad request.")]
        [InlineData("0,0,0")]
        [InlineData("0.0,0,0")]
        [InlineData("0.0,0.0,0")]
        [InlineData("0.0,0.0,0.0")]
        public async Task ValidateAsync_InvalidForce_Should_ReturnBadRequest(string invalidForce)
        {
            // Arrange
            _requestStub.AppliedForce = invalidForce;

            // Act
            var response = await _operation.ProcessAsync(_requestStub).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.ErrorMessages.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact(DisplayName = "Feature: ValidateAsync | Given: Invalid parameters. | When: Call method. | Should: Return a failure for a bad request.")]
        public async Task ValidateAsync_NullRequest_Should_ReturnBadRequest()
        {
            // Act
            var response = await _operation.ProcessAsync(null).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.ErrorMessages.Should().HaveCountGreaterOrEqualTo(1);
        }

        [Fact(DisplayName = "Feature: BuildDisplacementMatrix | Given: Valid parameters. | When: Call method. | Should: Return a valid displacement matrix.")]
        public void BuildDisplacementMatrix_ValidParameters_Should_Return_ValidMatrix()
        {
            // Arrange 
            Point3D origin = CalculateReactionsHelper.CreateOrigin();

            // Act
            double[,] result = _operation.BuildDisplacementMatrix(_suspensionSystem, origin);

            // Assert
            JToken.DeepEquals(JToken.FromObject(result), CalculateReactionsHelper.CreateDisplacementMatrixAsJToken());
        }

        [Fact(DisplayName = "Feature: BuildEffortsVector | Given: Valid parameters. | When: Call method. | Should: Return valid vector for the efforts.")]
        public void BuildEffortsVector_ValidParameters_Should_Return_ValidVector()
        {
            // Arrange
            Vector3D appliedForce = Vector3D.Create(_requestStub.AppliedForce);
            var effortExpected = new double[] { appliedForce.X, appliedForce.Y, appliedForce.Z, 0, 0, 0 };

            // Act
            double[] result = _operation.BuildEffortsVector(appliedForce);

            // Assert
            result.Should().BeEquivalentTo(effortExpected);
        }

        [Theory(DisplayName = "Feature: MapToResponseData | Given: Valid parameters. | When: ShouldRoundReults is true. | Should: Return valid reactions for components of suspension system.")]
        [InlineData(0)]
        [InlineData(2)]
        public void MapToResponseData_ValidParameters_When_ShouldRound_Is_True_Should_Return_ValidReactions(int decimals)
        {
            // Arrange
            _expectedResponse.Data.LowerWishboneReaction1.AbsolutValue = Math.Round(-_reactions[0], decimals);
            _expectedResponse.Data.LowerWishboneReaction2.AbsolutValue = Math.Round(-_reactions[1], decimals);
            _expectedResponse.Data.UpperWishboneReaction1.AbsolutValue = Math.Round(-_reactions[2], decimals);
            _expectedResponse.Data.UpperWishboneReaction2.AbsolutValue = Math.Round(-_reactions[3], decimals);
            _expectedResponse.Data.ShockAbsorberReaction.AbsolutValue = Math.Round(-_reactions[4], decimals);
            _expectedResponse.Data.TieRodReaction.AbsolutValue = Math.Round(-_reactions[5], decimals);

            // Act
            CalculateReactionsResponseData responseData = _operation.MapToResponseData(_suspensionSystem, _reactions, true, decimals);

            // Assert
            responseData.Should().NotBeNull();
            responseData.LowerWishboneReaction1.AbsolutValue.Should().Be(_expectedResponse.Data.LowerWishboneReaction1.AbsolutValue);
            responseData.LowerWishboneReaction2.AbsolutValue.Should().Be(_expectedResponse.Data.LowerWishboneReaction2.AbsolutValue);
            responseData.UpperWishboneReaction1.AbsolutValue.Should().Be(_expectedResponse.Data.UpperWishboneReaction1.AbsolutValue);
            responseData.UpperWishboneReaction2.AbsolutValue.Should().Be(_expectedResponse.Data.UpperWishboneReaction2.AbsolutValue);
            responseData.ShockAbsorberReaction.AbsolutValue.Should().Be(_expectedResponse.Data.ShockAbsorberReaction.AbsolutValue);
            responseData.TieRodReaction.AbsolutValue.Should().Be(_expectedResponse.Data.TieRodReaction.AbsolutValue);
        }

        [Fact(DisplayName = "Feature: MapToResponseData| Given: Valid parameters. | When: ShouldRoundReults is false. | Should: Return valid reactions for components of suspension system.")]
        public void MapToResponseData_ValidParameters_When_ShouldRound_Is_False_Should_Return_ValidReactions()
        {
            // Act
            CalculateReactionsResponseData responseData = _operation.MapToResponseData(_suspensionSystem, _reactions, false, null);

            // Assert
            responseData.Should().NotBeNull();
            responseData.LowerWishboneReaction1.AbsolutValue.Should().Be(_expectedResponse.Data.LowerWishboneReaction1.AbsolutValue);
            responseData.LowerWishboneReaction2.AbsolutValue.Should().Be(_expectedResponse.Data.LowerWishboneReaction2.AbsolutValue);
            responseData.UpperWishboneReaction1.AbsolutValue.Should().Be(_expectedResponse.Data.UpperWishboneReaction1.AbsolutValue);
            responseData.UpperWishboneReaction2.AbsolutValue.Should().Be(_expectedResponse.Data.UpperWishboneReaction2.AbsolutValue);
            responseData.ShockAbsorberReaction.AbsolutValue.Should().Be(_expectedResponse.Data.ShockAbsorberReaction.AbsolutValue);
            responseData.TieRodReaction.AbsolutValue.Should().Be(_expectedResponse.Data.TieRodReaction.AbsolutValue);
        }

        // This unit test is failing and must be investigated.
        [Fact(DisplayName = "Feature: ProcessAsync | Given: Valid parameters. | When: Call method. | Should: Return expected response.")]
        public async Task ProcessAsync_ValidParameters_Should_ReturnExpectedResponse()
        {
            // Arrange
            double precision = 1e-6;

            // Act
            var response = await _operation.ProcessAsync(_requestStub).ConfigureAwait(false);

            //Assert
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(_expectedResponse.StatusCode);
            response.Success.Should().Be(_expectedResponse.Success);
            response.ErrorMessages.Should().BeEquivalentTo(_expectedResponse.ErrorMessages);
            response.Data.Should().NotBeNull();
            response.Data.LowerWishboneReaction1.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.LowerWishboneReaction1.AbsolutValue, precision);
            response.Data.LowerWishboneReaction2.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.LowerWishboneReaction2.AbsolutValue, precision);
            response.Data.UpperWishboneReaction1.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.UpperWishboneReaction1.AbsolutValue, precision);
            response.Data.UpperWishboneReaction2.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.UpperWishboneReaction2.AbsolutValue, precision);
            response.Data.ShockAbsorberReaction.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.ShockAbsorberReaction.AbsolutValue, precision);
            response.Data.TieRodReaction.AbsolutValue.Should().BeApproximately(_expectedResponse.Data.TieRodReaction.AbsolutValue, precision);
        }
    }
}
using IcVibracoes.Common.Classes;
using IcVibracoes.Common.Profiles;
using IcVibracoes.DataContracts.FiniteElement.Beam;
using System.Collections.Generic;

namespace IcVibracoes.Test.Helper
{
    public static class RequestStub
    {
        public static BeamRequest<CircularProfile> CircularBeam
            => new BeamRequest<CircularProfile>
            {
                AngularFrequencyStep = 1,
                Author = "Darth Vader",
                Fastenings = new List<Fastening>
                {
                    new Fastening
                    {
                        NodePosition = 0,
                        Type = "Pinned"
                    },
                    new Fastening
                    {
                        NodePosition = 2,
                        Type = "Pinned"
                    }
                },
                Forces = new List<Force>
                {
                    new Force
                    {
                        NodePosition = 1,
                        Value = 10
                    }
                },
                ForceType = "Harmonic",
                InitialAngularFrequency = 0,
                FinalAngularFrequency = 10,
                Length = 1,
                Material = "Aluminum",
                NumberOfElements = 2,
                NumericalMethod = "Newmark",
                PeriodCount = 10,
                PeriodDivision = 10,
                Profile = GeometricPropertyModel.CircularBeamProfileWithoutThickness
            };

        public static BeamRequest<RectangularProfile> RectangularBeam
            => new BeamRequest<RectangularProfile>
            {
                AngularFrequencyStep = 1,
                Author = "Darth Vader",
                Fastenings = new List<Fastening>
                    {
                        new Fastening
                        {
                            NodePosition = 0,
                            Type = "Pinned"
                        },
                        new Fastening
                        {
                            NodePosition = 2,
                            Type = "Pinned"
                        }
                    },
                Forces = new List<Force>
                    {
                        new Force
                        {
                            NodePosition = 1,
                            Value = 10
                        }
                    },
                ForceType = "Harmonic",
                InitialAngularFrequency = 0,
                FinalAngularFrequency = 10,
                Length = 1,
                Material = "Aluminum",
                NumberOfElements = 2,
                NumericalMethod = "Newmark",
                PeriodCount = 10,
                PeriodDivision = 10,
                Profile = GeometricPropertyModel.RectangularBeamProfileWithoutThickness
            };
    }
}
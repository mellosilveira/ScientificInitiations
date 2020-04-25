﻿using IcVibracoes.Common.Profiles;
using IcVibracoes.Core.Mapper.BeamProfiles;
using IcVibracoes.Core.Models.BeamCharacteristics;
using System.Threading.Tasks;

namespace IcVibracoes.Core.Mapper.BeamProfiles
{
    /// <summary>
    /// It's responsible to build the profile.
    /// </summary>
    /// <typeparam name="TProfile"></typeparam>
    public abstract class ProfileMapper<TProfile> : IProfileMapper<TProfile>
        where TProfile : Profile
    {
        /// <summary>
        /// Method to build the profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="degreesFreedomMaximum"></param>
        /// <returns></returns>
        public abstract Task<GeometricProperty> Execute(TProfile profile, uint degreesFreedomMaximum);
    }
}
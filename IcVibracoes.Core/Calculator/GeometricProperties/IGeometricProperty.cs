﻿using IcVibracoes.Common.Profiles;
using System.Threading.Tasks;

namespace IcVibracoes.Calculator.GeometricProperties
{
    /// <summary>
    /// It's responsible to calculate any geometric property.
    /// </summary>
    public interface IGeometricProperty<TProfile>
        where TProfile : Profile, new()
    {
        /// <summary>
        /// This method calculates the vector with the beam or piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        Task<double[]> CalculateArea(TProfile profile, uint numberOfElements);

        /// <summary>
        /// This method calculates the vector with the beam moment of inertia.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        Task<double[]> CalculateMomentOfInertia(TProfile profile, uint numberOfElements);

        /// <summary>
        /// This method calculates the vector with the piezoelectric area.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricPerElement"></param>
        /// <returns></returns>
        Task<double[]> CalculatePiezoelectricArea(TProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricPerElement);

        /// <summary>
        /// This method calculates the vector with the piezoelectric moment of inertia.
        /// </summary>
        /// <param name="piezoelectricProfile"></param>
        /// <param name="profile"></param>
        /// <param name="numberOfElements"></param>
        /// <param name="elementsWithPiezoelectric"></param>
        /// <param name="numberOfPiezoelectricsPerElement"></param>
        /// <returns></returns>
        Task<double[]> CalculatePiezoelectricMomentOfInertia(TProfile piezoelectricProfile, TProfile profile, uint numberOfElements, uint[] elementsWithPiezoelectric, uint numberOfPiezoelectricsPerElement);
    }
}

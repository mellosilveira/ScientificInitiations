namespace IcVibracoes.Common.Classes
{
    /// <summary>
    /// The numerical methods availables.
    /// The explanation for each numerical method can be found in the file "Numerical Integration.pdf", in the folder "Bibliography".
    /// </summary>
    public enum NumericalMethod
    {
        CentralDifferenceMethod = 1,
        
        ImplicitLinearAccelerationMethod = 2,
        
        NewmarkBeta = 3,
        
        Newmark = 4,
        
        RungeKuttaForthOrder = 5
    }
}

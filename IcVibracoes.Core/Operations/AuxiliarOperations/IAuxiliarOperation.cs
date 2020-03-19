using IcVibracoes.Common.Classes;

namespace IcVibracoes.Methods.AuxiliarOperations
{
    public interface IAuxiliarOperation
    {
        double[,] ApplyBondaryConditions(double[,] matrix, bool[] bondaryConditions, uint size);

        double[] ApplyBondaryConditions(double[] matrix, bool[] bondaryConditions, uint size);

        uint CalculateDegreesFreedomMaximum(uint numberOfElements);

        void WriteInFile(double time, double[] values);

        void WriteInFile(double angularFrequency);
    }
}
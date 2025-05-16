using System.Linq;

namespace AutenticacaoFunction
{
    public static class CpfValidator
    {
        public static string CleanCpf(string cpf)
        {
            return cpf?.Replace(".", "").Replace("-", "").Trim();
        }

        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = CleanCpf(cpf);

            if (cpf.Length != 11)
                return false;

            if (!cpf.All(char.IsDigit))
                return false;

            return true;
        }
    }
}
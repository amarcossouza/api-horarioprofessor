namespace API_HorarioProfessor.Classes
{
    public static class ProfessorHelper
    {
        /// <summary>
        /// Extrai o nome do professor antes do traço.
        /// </summary>
        /// <param name="professor">String no formato "NOME-SUFIXO" ou apenas "NOME".</param>
        /// <returns>O nome do professor antes do traço ou a string original se não houver traço.</returns>
        public static string ExtrairNomeProfessor(string professor)
        {
            // Verifica se a entrada é nula ou vazia e retorna ela mesma
            if (string.IsNullOrWhiteSpace(professor))
                return professor;

            // Dividir a string no traço
            var partes = professor.Split('-');

            // Retornar a primeira parte (o nome)
            return partes[0].Trim();
        }
    }
}

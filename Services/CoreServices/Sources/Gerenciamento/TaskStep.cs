using API_HorarioProfessor.Services.CoreServices;

namespace API_HorarioProfessor.Services.CoreServices.Sources.Gerenciamento
{
    public class TaskStep
    {
        private readonly AppState _appState;

        public string Name { get; set; }
        public Action Method { get; set; }
        public Func<bool> Condition { get; set; }

        public TaskStep(AppState appState, string name, Action method, Func<bool> condition = null)
        {
            _appState = appState; // Injetando o AppState
            Name = name;
            Method = method;
            Condition = condition ?? (() => true); // Assume true if no condition is provided.
        }

        // Aqui você pode usar _appState dentro dos métodos ou ações conforme necessário
        public void ExecuteStep()
        {
            if (Condition())
            {
                Method();
            }
        }
    }
}

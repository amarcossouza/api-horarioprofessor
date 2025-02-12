using API_HorarioProfessor.Services.CoreServices;

namespace API_HorarioProfessor.Services.CoreServices.Sources.Gerenciamento
{
    public class WorkflowManager
    {
        private readonly AppState _appState;
        private List<TaskStep> tasks;

        public WorkflowManager(AppState appState)
        {
            _appState = appState;
            tasks = new List<TaskStep>();
        }

        public void AddTask(TaskStep task)
        {
            tasks.Add(task);
        }

        public void Execute()
        {
            foreach (var task in tasks)
            {
                if (task.Condition())
                {
                    Console.WriteLine($"Executing: {task.Name}");
                    task.Method();
                }
                else
                {
                    Console.WriteLine($"Skipped: {task.Name} due to condition.");
                }
            }
        }
    }
}

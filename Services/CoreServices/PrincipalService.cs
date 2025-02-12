namespace API_HorarioProfessor.Services.CoreServices
{
    public class PrincipalService
    {
        private readonly AppState _appState;
        private readonly IParametroDiarioOficialService _parametroDiarioOficialService;
       // private readonly IEscolaService _escolaService;
        private readonly IUsuarioAcessoService _usuarioAcessoService;

        public PrincipalService(AppState appState, IParametroDiarioOficialService parametroDiarioOficialService,
                                //IEscolaService escolaService,
                                IUsuarioAcessoService usuarioAcessoService)
        {
            _appState = appState;
            _parametroDiarioOficialService = parametroDiarioOficialService;
            //_escolaService = escolaService;
            _usuarioAcessoService = usuarioAcessoService;
        }

        // Adicione os métodos necessários aqui
        public async Task Executar(int idEscola, string periodo, int ativo, int anoLetivo, int OrdenarMaiorNumeroOpcao1OuOpcao2, int QuantidadeHorarios )
        {
            // Lógica principal aqui
             await _appState.Main( idEscola, periodo, ativo, anoLetivo, OrdenarMaiorNumeroOpcao1OuOpcao2,  QuantidadeHorarios);
        }
    }
}

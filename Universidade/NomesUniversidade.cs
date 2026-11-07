using System.Globalization;
using System.Text;

namespace API_HorarioProfessor.Universidade;

/// <summary>
/// Regras de nomes e horários do módulo universidade, num lugar só.
/// O motor antigo identifica a pessoa pelo texto antes do primeiro hífen de NomeProfessor
/// e usa o formato "SEG3" para dia + aula; aqui ficam essas conversões.
/// </summary>
public static class NomesUniversidade
{
    public static readonly string[] Dias = { "SEG", "TER", "QUA", "QUI", "SEX", "SAB" };
    public static readonly string[] DiasNome = { "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado" };

    private static readonly HashSet<string> Ligacoes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DE", "DA", "DO", "DAS", "DOS", "E", "A", "O", "AO", "AOS", "AS", "OS", "EM", "NA", "NO", "NAS", "NOS",
        "PARA", "P", "-", "/", "À", "COM"
    };

    public static string SemAcentos(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static List<string> Palavras(string? texto) =>
        SemAcentos(texto).ToUpperInvariant()
            .Replace("(FICTICIO)", " ")
            .Split(new[] { ' ', '.', ',', '-', '/', '(', ')', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => new string(p.Where(char.IsLetterOrDigit).ToArray()))
            .Where(p => p.Length > 0)
            .ToList();

    /// <summary>
    /// Nome curto para o cadastro do motor: primeiro e último nome colados (ex.: RENATAWOLF), sem acento,
    /// sem espaço e sem hífen. O motor antigo usa o texto antes do primeiro hífen como a pessoa e a
    /// chave professor_turma aceita só 2 ou 3 partes, então o nome não pode ter hífen nem sublinhado.
    /// </summary>
    public static string NomeCurto(string nome)
    {
        var p = Palavras(nome).Where(x => !Ligacoes.Contains(x) && x != "JR" && x != "JUNIOR" && x != "NETO" && x != "FILHO").ToList();
        if (p.Count == 0) p = Palavras(nome);
        if (p.Count == 0) return "PROFESSOR";
        var curto = p.Count == 1 ? p[0] : p[0] + p[^1];
        return curto.Length > 30 ? curto[..30] : curto;
    }

    /// <summary>Tira espaços, hífens e sublinhados de um nome curto antigo (ex.: "RENATA WOLF" → "RENATAWOLF").</summary>
    public static string NomeCurtoSemSeparador(string nomeCurto) =>
        new string((nomeCurto ?? "").Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '_').ToArray()).ToUpperInvariant();

    /// <summary>Sigla da disciplina gravada como matéria (ex.: "Cálculo Diferencial e Integral III" → CDI3).</summary>
    public static string SiglaDisciplina(string nome) => SiglaDisciplina(nome, 8);

    /// <summary>
    /// Mesma regra, cabendo em <paramref name="tamanhoMaximo"/> caracteres (a coluna Sigla da tabela antiga pode ser curta):
    /// as letras encolhem e o número fica (ex.: com 3 → CD3).
    /// </summary>
    public static string SiglaDisciplina(string nome, int tamanhoMaximo)
    {
        if (tamanhoMaximo < 1) tamanhoMaximo = 1;
        var p = Palavras(nome).Where(x => !Ligacoes.Contains(x)).ToList();
        if (p.Count == 0) return "DISC";
        var numero = "";
        var ultimo = p[^1];
        var romanos = new Dictionary<string, string> { ["I"] = "1", ["II"] = "2", ["III"] = "3", ["IV"] = "4", ["V"] = "5", ["VI"] = "6" };
        if (romanos.TryGetValue(ultimo, out var n)) { numero = n; p.RemoveAt(p.Count - 1); }
        else if (ultimo.All(char.IsDigit)) { numero = ultimo; p.RemoveAt(p.Count - 1); }
        var extra = p.Remove("EXTRA") ? "X" : "";
        p.Remove("OPTATIVA");
        string letras;
        if (p.Count == 0) letras = "DISC";
        else if (p.Count == 1) letras = p[0].Length >= 3 ? p[0][..3] : p[0];
        else letras = string.Concat(p.Take(4).Select(x => x[0]));
        var sufixo = numero + extra;
        if (sufixo.Length >= tamanhoMaximo) sufixo = tamanhoMaximo > 1 ? sufixo[..(tamanhoMaximo - 1)] : "";
        var espaco = tamanhoMaximo - sufixo.Length;
        if (letras.Length > espaco) letras = letras[..espaco];
        return letras + sufixo;
    }

    /// <summary>Sigla da turma gravada em hprof_turmapadraoescola (ex.: "1ª Fase" → 1F, "2ª Fase Extra" → 2FX).</summary>
    public static string SiglaTurma(string nome)
    {
        var limpo = SemAcentos(nome).ToUpperInvariant();
        var digitos = new string(limpo.TakeWhile(char.IsDigit).ToArray());
        var extra = limpo.Contains("EXTRA") ? "X" : "";
        if (digitos.Length > 0) return digitos + "F" + extra;
        var p = Palavras(nome);
        var baseSigla = p.Count == 0 ? "TUR" : (p[0].Length >= 3 ? p[0][..3] : p[0]);
        return baseSigla + extra;
    }

    /// <summary>Gera um valor único acrescentando 2, 3… quando já existe.</summary>
    public static string Unico(string desejado, ICollection<string> usados, int tamanhoMaximo)
    {
        var candidato = desejado.Length > tamanhoMaximo ? desejado[..tamanhoMaximo] : desejado;
        var i = 2;
        while (usados.Contains(candidato))
        {
            var sufixo = i.ToString(CultureInfo.InvariantCulture);
            var baseTexto = desejado.Length + sufixo.Length > tamanhoMaximo ? desejado[..(tamanhoMaximo - sufixo.Length)] : desejado;
            candidato = baseTexto + sufixo;
            i++;
        }
        usados.Add(candidato);
        return candidato;
    }

    /// <summary>"SEG", "Seg", "segunda", "1" → 1 (segunda) … 6 (sábado). Zero quando não reconhece.</summary>
    public static int DiaDoTexto(string? dia)
    {
        var t = SemAcentos(dia).Trim().ToUpperInvariant();
        if (t.Length == 0) return 0;
        if (int.TryParse(t, out var n)) return n is >= 1 and <= 6 ? n : 0;
        for (var i = 0; i < Dias.Length; i++)
        {
            if (t.StartsWith(Dias[i], StringComparison.Ordinal)) return i + 1;
        }
        return 0;
    }

    public static string Dia3(int dia) => dia is >= 1 and <= 6 ? Dias[dia - 1] : "";

    public static bool TentarHora(string? texto, out TimeSpan hora)
    {
        hora = default;
        var t = (texto ?? "").Trim().Replace('h', ':').Replace('H', ':');
        return TimeSpan.TryParseExact(t, new[] { @"hh\:mm", @"h\:mm" }, CultureInfo.InvariantCulture, out hora);
    }

    public static string Hora(TimeSpan hora) => hora.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

    /// <summary>Formato de hprof_gradehorariosconfig.Horarios: "08:20 - 09:10,09:10 - 10:00".</summary>
    public static string FormatarHorariosConfig(IEnumerable<(TimeSpan Inicio, TimeSpan Fim)> horarios) =>
        string.Join(",", horarios.Select(h => $"{Hora(h.Inicio)} - {Hora(h.Fim)}"));

    /// <summary>Lê hprof_gradehorariosconfig.Horarios. Itens inválidos ficam com 00:00.</summary>
    public static List<(TimeSpan Inicio, TimeSpan Fim)> LerHorariosConfig(string? horarios)
    {
        var lista = new List<(TimeSpan, TimeSpan)>();
        foreach (var item in (horarios ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = item.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            TimeSpan ini = default, fim = default;
            if (partes.Length >= 2)
            {
                TentarHora(partes[0], out ini);
                TentarHora(partes[1], out fim);
            }
            lista.Add((ini, fim));
        }
        return lista;
    }

    public static bool Sobrepoe(TimeSpan inicioA, TimeSpan fimA, TimeSpan inicioB, TimeSpan fimB) =>
        inicioA < fimB && inicioB < fimA;
}

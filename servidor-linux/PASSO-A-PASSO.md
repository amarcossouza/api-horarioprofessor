# Deploy da API Horario Professor no droplet

- Pasta no servidor: `/var/www/horarioprofessor-api`
- Servico: `horarioprofessor-api` (porta **5311**, so 127.0.0.1)
- **Nao tem nginx de proposito:** a API nao tem autenticacao, entao fica acessivel
  apenas de dentro do servidor. Quem chama a API e o front (`horarioprofessor`, porta 5310),
  pelo endereco `http://127.0.0.1:5311` configurado no `appsettings.Production.json` do front.
- Pacote gerado por `gera_deploy.bat` em `bin\Release\net8.0\linux-x64\publish\` (caminho padrao do dotnet publish).
- Modelos (service, config, nginx) ficam em `servidor-linux\` no projeto — nao vao no pacote.

O pacote e self-contained (linux-x64): **nao precisa instalar .NET no servidor**.

## Configuracao

A config de producao (porta em `Urls`, URL da API, conexao do banco) esta no `appsettings.Production.json`
do projeto e ja vai dentro do pacote. Para mudar, edite no projeto e gere o pacote de novo.

## Primeira vez

1. **FileZilla:** envie TODO o conteudo de `bin\Release\net8.0\linux-x64\publish\` para `/var/www/horarioprofessor-api/`.

2. **Console (DO):** confirme que a porta esta livre (nao pode aparecer nada):
   ```bash
   ss -ltnp | grep 5311
   ```

3. Permissoes:
   ```bash
   chmod +x /var/www/horarioprofessor-api/API-HorarioProfessor
   chown -R www-data:www-data /var/www/horarioprofessor-api
   ```

4. Servico systemd:
   ```bash
   nano /etc/systemd/system/horarioprofessor-api.service     # cole o conteudo de servidor-linux\horarioprofessor-api.service
   systemctl daemon-reload
   systemctl enable --now horarioprofessor-api
   systemctl status horarioprofessor-api --no-pager
   curl -I http://127.0.0.1:5311/swagger/index.html      # esperado: 200
   ```

## Atualizar (proximas vezes)

1. Rode `gera_deploy.bat`.
2. **FileZilla:** envie o conteudo de `bin\Release\net8.0\linux-x64\publish\` por cima de
   `/var/www/horarioprofessor-api/` (sobrescrever).
3. Console:
   ```bash
   chmod +x /var/www/horarioprofessor-api/API-HorarioProfessor
   chown -R www-data:www-data /var/www/horarioprofessor-api
   systemctl restart horarioprofessor-api
   systemctl status horarioprofessor-api --no-pager
   ```

## Diagnostico

```bash
journalctl -u horarioprofessor-api -n 100 --no-pager
journalctl -u horarioprofessor-api -f
```

Ver o Swagger do seu PC sem expor a API (tunel SSH):
```bash
ssh -L 5311:127.0.0.1:5311 root@134.209.164.87
```
e abra `http://localhost:5311/swagger` no navegador.

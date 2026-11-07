# Carga de demonstração do módulo universidade

Esta pasta **não** faz parte das migrações normais. Ela só deve ser incluída no banco de
demonstração/homologação:

```
flyway -configFiles=flyway.conf -locations=filesystem:./sql,filesystem:./sql-demo info
flyway -configFiles=flyway.conf -locations=filesystem:./sql,filesystem:./sql-demo validate
flyway -configFiles=flyway.conf -locations=filesystem:./sql,filesystem:./sql-demo migrate
flyway -configFiles=flyway.conf -locations=filesystem:./sql,filesystem:./sql-demo info
```

`R__uni_demo_udesc_cesfi.sql` é uma migração **repetível**: não trava as próximas
versões (V22, V23…) e pode rodar de novo sem duplicar nada. Ela adiciona:

- a escola da instituição em `hprof_escolas` (TipoEscola = `Universidade`) e o login
  `udesc.demo` / `udesc2026` em `hprof_usuariosacesso`;
- a instituição, o cliente de integração e três envios **recebidos**:
  1. `UDESC-PESSOAS-2026-2-001` — 30 professores (pessoas, com carga total);
  2. `UDESC-PET-2026-2-001` — Engenharia de Petróleo, do PDF de 28/07/2026;
  3. `UDESC-ADM-2026-2-001` — Administração Pública (**simulação, dados fictícios**).

Depois do `migrate`, entre com `udesc.demo`, abra **Universidade > Envios** e aprove os três
envios nessa ordem (professores primeiro: os cursos são conferidos na hora da aprovação). A aprovação grava cursos, turmas, disciplinas, atribuições e
indisponibilidades nas tabelas atuais, com auditoria de quem aprovou.

A chave de integração de demonstração está no cabeçalho do arquivo R__. Troque-a antes de usar fora
do teste (grave o SHA-256 da nova chave em `hprof_uni_integracaocliente.chave_hash`).

# Domino Ponta de Quina

## Projetos

- `DominoPontaDeQuina.Core`: regras e fluxo do jogo (`Partida` -> `Rodadas` -> `Jogadas`).
- `DominoPontaDeQuina.Domain`: entidades e enums persistentes.
- `DominoPontaDeQuina.Repository`: `DominoDbContext`, mapeamentos Fluent API, interfaces e repositorios EF Core.
- `DominoPontaDeQuina.Services`: camada de servicos que orquestra as regras de uso da aplicacao.
- `DominoPontaDeQuina.App`: aplicacao de console que registra as dependencias e executa o fluxo principal.
- `DominoPontaDeQuina.Migrations`: aplicacao console usada como startup project para migrations.
- `DominoPontaDeQuina.Tests`: testes automatizados do nucleo do jogo, dos repositorios e dos servicos.

## Arquitetura

```
DominoPontaDeQuina.App  ->  Services  ->  Repository  ->  Domain
                              |
                              +-------->  Core (motor do jogo)
```

O console nao conhece o Entity Framework nem o motor do jogo: ele depende apenas das interfaces de servico.
Os servicos aplicam as regras de uso e delegam o acesso a dados as interfaces de repositorio, que concentram
todas as consultas LINQ. O `Core` continua responsavel pelas regras do domino.

## Modelo persistente

`Usuario` representa a conta do aplicativo cliente e pode possuir varios `Jogador`, que sao perfis de jogo.
`Partida` armazena a disputa e sua `PontuacaoAlvo`. `TimePartida` guarda a composicao e a pontuacao de cada
time, e `ParticipacaoPartida` liga um jogador a uma partida registrando assento, time, pontuacao e resultado.
`Rodada` registra cada set da partida com o motivo do encerramento e o jogador vencedor, e `Jogada` guarda a
peca, o lado do tabuleiro, a passagem de vez e os pontos gerados em cada lance.

Os enums de status sao gravados como texto e o modelo possui indices unicos para e-mail do usuario, nome de
exibicao por usuario, participacao por partida, numero da rodada e sequencia da jogada.

## Injecao de dependencia

`DominoPontaDeQuina.App/Program.cs` registra o `DbContext`, os repositorios e os servicos:

```csharp
builder.Services.AddDbContext<DominoDbContext>(options => options.UseSqlite(stringDeConexao));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IJogadorRepository, JogadorRepository>();
builder.Services.AddScoped<IPartidaRepository, PartidaRepository>();
builder.Services.AddScoped<IParticipacaoPartidaRepository, ParticipacaoPartidaRepository>();
builder.Services.AddScoped<IRodadaRepository, RodadaRepository>();
builder.Services.AddScoped<IJogadaRepository, JogadaRepository>();

builder.Services.AddScoped<IHashSenhaService, HashSenhaService>();
builder.Services.AddScoped<IJogo, Jogo>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IJogadorService, JogadorService>();
builder.Services.AddScoped<IPartidaService, PartidaService>();
builder.Services.AddScoped<IEstatisticasService, EstatisticasService>();

builder.Services.AddScoped<AplicacaoConsole>();
```

`AplicacaoConsole` recebe `IUsuarioService`, `IJogadorService`, `IPartidaService` e `IEstatisticasService` por
construtor. Nenhuma classe de aplicacao e instanciada com `new` fora do container: a unica excecao e a fabrica
`DominoDbContextFactory`, exigida pelas ferramentas de linha de comando do Entity Framework Core.

## Pre-requisitos

- .NET 8 SDK
- Ferramenta `dotnet-ef` 8.x (`dotnet tool install --global dotnet-ef --version 8.*`)

## Restaurar e compilar

```bash
dotnet restore
dotnet build
```

## Executar a aplicacao

O console aplica as migrations pendentes, garante a conta e os jogadores da mesa, disputa uma partida completa
e apresenta o placar, o historico e o ranking a partir dos dados persistidos:

```bash
dotnet run --project DominoPontaDeQuina.App

# a pontuacao alvo tambem pode ser informada por argumento
dotnet run --project DominoPontaDeQuina.App -- 20
```

A string de conexao fica em `DominoPontaDeQuina.App/appsettings.json`, na chave `ConnectionStrings:Domino`.

## Migrations

Os comandos devem usar `DominoPontaDeQuina.Migrations` como startup project e `DominoPontaDeQuina.Repository`
como projeto do contexto:

```bash
dotnet ef migrations add NomeDaMigration \
  --project DominoPontaDeQuina.Repository \
  --startup-project DominoPontaDeQuina.Migrations

dotnet ef database update \
  --project DominoPontaDeQuina.Repository \
  --startup-project DominoPontaDeQuina.Migrations
```

Em tempo de design a conexao vem do argumento `--connection=<valor>`, da variavel de ambiente
`DOMINO_CONNECTION_STRING` ou do valor padrao. O banco SQLite local `domino.db` e ignorado pelo Git.

## Testes

```bash
dotnet test DominoPontaDeQuina.Tests/DominoPontaDeQuina.Tests.csproj
```

Os testes do nucleo do jogo rodam em memoria. Os testes de repositorio, de servico e do fluxo principal do
console usam um banco SQLite em memoria e o mesmo container de injecao de dependencia configurado em
`Program.cs`, garantindo que as consultas LINQ sejam realmente executadas pelo Entity Framework Core.

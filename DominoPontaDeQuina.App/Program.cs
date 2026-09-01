using DominoPontaDeQuina.App;
using DominoPontaDeQuina.Core;
using DominoPontaDeQuina.Core.Interfaces;
using DominoPontaDeQuina.Repository.Context;
using DominoPontaDeQuina.Repository.Interfaces;
using DominoPontaDeQuina.Repository.Repositories;
using DominoPontaDeQuina.Services;
using DominoPontaDeQuina.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var stringDeConexao = builder.Configuration.GetConnectionString("Domino")
    ?? DominoDbContext.ConnectionStringPadrao;

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

using var host = builder.Build();
using var escopo = host.Services.CreateScope();

var contexto = escopo.ServiceProvider.GetRequiredService<DominoDbContext>();
await contexto.Database.MigrateAsync();

var pontuacaoAlvo = builder.Configuration.GetValue("Partida:PontuacaoAlvo", Jogo.PontuacaoAlvoPadrao);

if (args.Length > 0 && int.TryParse(args[0], out var pontuacaoAlvoInformada))
    pontuacaoAlvo = pontuacaoAlvoInformada;

var aplicacao = escopo.ServiceProvider.GetRequiredService<AplicacaoConsole>();

return await aplicacao.ExecutarAsync(pontuacaoAlvo);

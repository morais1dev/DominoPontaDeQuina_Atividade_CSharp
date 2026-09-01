using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DominoPontaDeQuina.Repository.Migrations
{
    /// <inheritdoc />
    public partial class EvoluirModeloDePartida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesPartida_PartidaId",
                table: "ParticipacoesPartida");

            migrationBuilder.DropIndex(
                name: "IX_Jogadores_UsuarioId",
                table: "Jogadores");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Partidas",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "PontuacaoAlvo",
                table: "Partidas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TimePartidaId",
                table: "ParticipacoesPartida",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Jogadores",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Rodadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartidaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TipoFinalizacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    JogadorVencedorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PontuacaoVencedor = table.Column<int>(type: "INTEGER", nullable: false),
                    IniciadaEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinalizadaEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rodadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rodadas_Jogadores_JogadorVencedorId",
                        column: x => x.JogadorVencedorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rodadas_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimesPartida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartidaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Pontuacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Vencedor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimesPartida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimesPartida_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jogadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RodadaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JogadorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Sequencia = table.Column<int>(type: "INTEGER", nullable: false),
                    PecaValorA = table.Column<int>(type: "INTEGER", nullable: true),
                    PecaValorB = table.Column<int>(type: "INTEGER", nullable: true),
                    Lado = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PassouVez = table.Column<bool>(type: "INTEGER", nullable: false),
                    PontosGerados = table.Column<int>(type: "INTEGER", nullable: false),
                    RegistradaEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadas_Jogadores_JogadorId",
                        column: x => x.JogadorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jogadas_Rodadas_RodadaId",
                        column: x => x.RodadaId,
                        principalTable: "Rodadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_IniciadoEm",
                table: "Partidas",
                column: "IniciadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_Status",
                table: "Partidas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesPartida_PartidaId_JogadorId",
                table: "ParticipacoesPartida",
                columns: new[] { "PartidaId", "JogadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesPartida_TimePartidaId",
                table: "ParticipacoesPartida",
                column: "TimePartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_UsuarioId_NomeExibicao",
                table: "Jogadores",
                columns: new[] { "UsuarioId", "NomeExibicao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jogadas_JogadorId",
                table: "Jogadas",
                column: "JogadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadas_RodadaId_Sequencia",
                table: "Jogadas",
                columns: new[] { "RodadaId", "Sequencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rodadas_JogadorVencedorId",
                table: "Rodadas",
                column: "JogadorVencedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Rodadas_PartidaId_Numero",
                table: "Rodadas",
                columns: new[] { "PartidaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimesPartida_PartidaId_Nome",
                table: "TimesPartida",
                columns: new[] { "PartidaId", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipacoesPartida_TimesPartida_TimePartidaId",
                table: "ParticipacoesPartida",
                column: "TimePartidaId",
                principalTable: "TimesPartida",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipacoesPartida_TimesPartida_TimePartidaId",
                table: "ParticipacoesPartida");

            migrationBuilder.DropTable(
                name: "Jogadas");

            migrationBuilder.DropTable(
                name: "TimesPartida");

            migrationBuilder.DropTable(
                name: "Rodadas");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Partidas_IniciadoEm",
                table: "Partidas");

            migrationBuilder.DropIndex(
                name: "IX_Partidas_Status",
                table: "Partidas");

            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesPartida_PartidaId_JogadorId",
                table: "ParticipacoesPartida");

            migrationBuilder.DropIndex(
                name: "IX_ParticipacoesPartida_TimePartidaId",
                table: "ParticipacoesPartida");

            migrationBuilder.DropIndex(
                name: "IX_Jogadores_UsuarioId_NomeExibicao",
                table: "Jogadores");

            migrationBuilder.DropColumn(
                name: "PontuacaoAlvo",
                table: "Partidas");

            migrationBuilder.DropColumn(
                name: "TimePartidaId",
                table: "ParticipacoesPartida");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Jogadores");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Partidas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipacoesPartida_PartidaId",
                table: "ParticipacoesPartida",
                column: "PartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_UsuarioId",
                table: "Jogadores",
                column: "UsuarioId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CapitalPos.Cpe.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emisores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ruc = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NombreComercial = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ubigeo = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Departamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Provincia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Distrito = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    AmbienteDefault = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProveedorEnvio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UrlOverrideBeta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UrlOverrideProduccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emisores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesEmitidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ruc = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    TipoComprobante = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Serie = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Correlativo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    NombreXml = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    NombreZip = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    NombreCdr = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    RazonSocialEmisor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoDocumentoCliente = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    NumeroDocumentoCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RazonSocialCliente = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    SistemaOrigen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaEmision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaProceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EtapasJson = table.Column<string>(type: "text", nullable: true),
                    ErroresJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesEmitidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesEmitidos_Emisores_EmisorId",
                        column: x => x.EmisorId,
                        principalTable: "Emisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmisorCertificados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PfxCiphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    PasswordCiphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    HuellaThumbprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VigenteDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VigenteHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmisorCertificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmisorCertificados_Emisores_EmisorId",
                        column: x => x.EmisorId,
                        principalTable: "Emisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmisorCredencialesSol",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmisorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ambiente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioSolCiphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    ClaveSolCiphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmisorCredencialesSol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmisorCredencialesSol_Emisores_EmisorId",
                        column: x => x.EmisorId,
                        principalTable: "Emisores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CdrRegistros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComprobanteEmitidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoSunat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MensajeSunat = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ObservacionesJson = table.Column<string>(type: "text", nullable: true),
                    NombreArchivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    RecibidoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CdrRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CdrRegistros_ComprobantesEmitidos_ComprobanteEmitidoId",
                        column: x => x.ComprobanteEmitidoId,
                        principalTable: "ComprobantesEmitidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CdrRegistros_ComprobanteEmitidoId",
                table: "CdrRegistros",
                column: "ComprobanteEmitidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesEmitidos_EmisorId",
                table: "ComprobantesEmitidos",
                column: "EmisorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesEmitidos_Ruc_TipoComprobante_Serie_Correlativo",
                table: "ComprobantesEmitidos",
                columns: new[] { "Ruc", "TipoComprobante", "Serie", "Correlativo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmisorCertificados_EmisorId_Activo",
                table: "EmisorCertificados",
                columns: new[] { "EmisorId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmisorCredencialesSol_EmisorId_Ambiente_Activo",
                table: "EmisorCredencialesSol",
                columns: new[] { "EmisorId", "Ambiente", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Emisores_Ruc",
                table: "Emisores",
                column: "Ruc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CdrRegistros");

            migrationBuilder.DropTable(
                name: "EmisorCertificados");

            migrationBuilder.DropTable(
                name: "EmisorCredencialesSol");

            migrationBuilder.DropTable(
                name: "ComprobantesEmitidos");

            migrationBuilder.DropTable(
                name: "Emisores");
        }
    }
}

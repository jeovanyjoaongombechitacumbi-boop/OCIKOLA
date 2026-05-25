using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;

namespace PlataformaOCIKOLA
{
    public partial class CompletarPerfil : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verificar se o utilizador está autenticado
            if (Session["TempUserId"] == null && Session["IdUsuario"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CarregarGeneros();
                CarregarMunicipios();
                PreencherDadosTemporarios();
            }
        }

        private void PreencherDadosTemporarios()
        {
            if (Session["TempUserName"] != null)
            {
                txtNomeCompleto.Value = Session["TempUserName"].ToString();
            }
        }

        private void CarregarGeneros()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_genero FROM Genero", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlGenero.Items.Add(new System.Web.UI.WebControls.ListItem(
                                    reader["Nome_genero"].ToString(),
                                    reader["id"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarGeneros: {ex.Message}");
            }
        }

        private void CarregarMunicipios()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT DISTINCT m.id, m.Nome_municipio
                        FROM Municipio m
                        ORDER BY m.Nome_municipio
                    ", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlMunicipio.Items.Add(new System.Web.UI.WebControls.ListItem(
                                    reader["Nome_municipio"].ToString(),
                                    reader["id"].ToString()
                                ));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarMunicipios: {ex.Message}");
            }
        }

        protected void btnConcluir_Click(object sender, EventArgs e)
        {
            try
            {
                string nome = txtNomeCompleto.Value.Trim();
                string contacto = txtContacto.Value.Trim();
                string generoId = ddlGenero.Value;
                string dataNascimento = txtDataNascimento.Value;
                string municipioId = ddlMunicipio.Value;
                string bairro = txtBairro.Value.Trim();

                if (string.IsNullOrEmpty(nome))
                {
                    MostrarErro("Nome é obrigatório");
                    return;
                }

                int usuarioId = 0;
                if (Session["TempUserId"] != null)
                    int.TryParse(Session["TempUserId"].ToString(), out usuarioId);
                else if (Session["IdUsuario"] != null)
                    int.TryParse(Session["IdUsuario"].ToString(), out usuarioId);

                if (usuarioId <= 0)
                {
                    MostrarErro("Sessão inválida");
                    return;
                }

                // Atualizar perfil do utilizador
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    string sql = @"UPDATE [User] SET 
                                    Nome_user = @Nome,
                                    Contacto = @Contacto,
                                    Genero_id = @Genero,
                                    DataNascimento = @DataNascimento,
                                    Municipio_id = @Municipio,
                                    Bairro = @Bairro,
                                    PerfilCompleto = 1
                                WHERE id = @Id";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", usuarioId);
                        cmd.Parameters.AddWithValue("@Nome", nome);
                        cmd.Parameters.AddWithValue("@Contacto", string.IsNullOrEmpty(contacto) ? DBNull.Value : (object)contacto);
                        cmd.Parameters.AddWithValue("@Genero", string.IsNullOrEmpty(generoId) ? DBNull.Value : (object)int.Parse(generoId));
                        cmd.Parameters.AddWithValue("@DataNascimento", string.IsNullOrEmpty(dataNascimento) ? DBNull.Value : (object)DateTime.Parse(dataNascimento));
                        cmd.Parameters.AddWithValue("@Municipio", string.IsNullOrEmpty(municipioId) ? DBNull.Value : (object)int.Parse(municipioId));
                        cmd.Parameters.AddWithValue("@Bairro", string.IsNullOrEmpty(bairro) ? DBNull.Value : (object)bairro);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Atualizar sessão
                Session["NomeUsuario"] = nome;
                Session["UsuarioLogado"] = Session["TempUserEmail"];
                Session["IdUsuario"] = usuarioId;

                // Limpar dados temporários
                Session["TempUserId"] = null;
                Session["TempUserEmail"] = null;
                Session["TempUserName"] = null;
                Session["TempUserFoto"] = null;

                Response.Redirect("VerEscolas.aspx");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnConcluir_Click: {ex.Message}");
                MostrarErro("Erro ao completar perfil");
            }
        }

        private void MostrarErro(string mensagem)
        {
            ltMensagem.Text = $"<div class='message-box message-error'>❌ {mensagem}</div>";
            ltMensagem.Visible = true;
        }

        private void MostrarSucesso(string mensagem)
        {
            ltMensagem.Text = $"<div class='message-box message-success'>✅ {mensagem}</div>";
            ltMensagem.Visible = true;
        }
    }
}

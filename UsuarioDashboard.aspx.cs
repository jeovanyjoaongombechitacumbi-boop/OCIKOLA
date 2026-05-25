using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace PlataformaOCIKOLA
{
    public partial class UsuarioDashboard : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verificar autenticação
            if (Session["IdUsuario"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CarregarDadosUsuario();
                CarregarInscricoes();
            }
        }

        private void CarregarDadosUsuario()
        {
            try
            {
                int usuarioId = int.Parse(Session["IdUsuario"].ToString());

                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT Nome_user, Email, Contacto, FotoPerfil FROM [User] WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", usuarioId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nome = reader["Nome_user"].ToString();
                                string email = reader["Email"].ToString();
                                string contacto = reader["Contacto"]?.ToString() ?? "Não informado";

                                // Atualizar UI
                                Session["NomeUsuario"] = nome;
                                Session["UsuarioLogado"] = email;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarDadosUsuario: {ex.Message}");
            }
        }

        private void CarregarInscricoes()
        {
            try
            {
                int usuarioId = int.Parse(Session["IdUsuario"].ToString());

                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            i.id,
                            i.Data_inscricao,
                            i.Status,
                            e.Nome_escola,
                            c.Nome_curso
                        FROM Inscricao i
                        INNER JOIN Escola e ON i.Escola_id = e.id
                        INNER JOIN Curso c ON i.Curso_id = c.id
                        WHERE i.User_id = @usuarioId
                        ORDER BY i.Data_inscricao DESC
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                // Carregar inscrições no repeater se existir
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarInscricoes: {ex.Message}");
            }
        }

        protected void btnSair_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            HttpCookie cookie = new HttpCookie("LembrarUsuario");
            cookie.Value = "";
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);

            Response.Redirect("Index.aspx");
        }
    }
}

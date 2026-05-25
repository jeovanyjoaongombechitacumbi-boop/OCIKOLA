using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace PlataformaOCIKOLA
{
    public partial class DetalhesEscola : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Verificar autenticação
            if (Session["UsuarioLogado"] != null)
            {
                divLoggedOut.Visible = false;
                divLoggedIn.Visible = true;
                AtualizarNavbarUser();
            }
            else
            {
                divLoggedOut.Visible = true;
                divLoggedIn.Visible = false;
            }

            if (!IsPostBack)
            {
                CarregarDetalhesEscola();
            }
        }

        private void AtualizarNavbarUser()
        {
            if (Session["NomeUsuario"] != null)
            {
                string nome = Session["NomeUsuario"].ToString();
                string primeiraLetra = nome.Length > 0 ? nome[0].ToString().ToUpper() : "U";
                spanAvatar.InnerText = primeiraLetra;
                spanUserName.InnerText = nome;
            }
        }

        private void CarregarDetalhesEscola()
        {
            try
            {
                string escolaId = Request.QueryString["id"];

                if (string.IsNullOrEmpty(escolaId) || !int.TryParse(escolaId, out int id))
                {
                    Response.Redirect("VerEscolas.aspx");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    // Carregar dados da escola
                    string sql = @"
                        SELECT 
                            e.id,
                            e.Nome_escola,
                            e.Descricao,
                            e.Imagem,
                            e.Inicio_inscricoes,
                            e.Fim_inscricoes,
                            e.Data_fundacao,
                            e.Email_escola,
                            e.Website,
                            e.Latitude,
                            e.Longitude,
                            m.Nome_municipio,
                            ti.Nome_tipo_instituicao,
                            te.Nome_tipo_ensino
                        FROM Escola e
                        LEFT JOIN Municipio m ON e.Municipio_id = m.id
                        LEFT JOIN Tipo_instituicao ti ON e.Tipo_instituicao_id = ti.id
                        LEFT JOIN Tipo_ensino te ON e.Tipo_ensino_id = te.id
                        WHERE e.id = @id
                    ";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Atualizar dados da escola
                                titleEscola.InnerText = reader["Nome_escola"].ToString();
                                descEscola.InnerText = reader["Descricao"]?.ToString() ?? "Sem descrição";

                                // Imagem
                                if (reader["Imagem"] != DBNull.Value && !string.IsNullOrEmpty(reader["Imagem"].ToString()))
                                    imgEscola.ImageUrl = ResolveUrl("~/Uploads/Escolas/" + reader["Imagem"]);
                                else
                                    imgEscola.ImageUrl = ResolveUrl("~/Imagens/IM1.jpg");

                                // Localização
                                string municipio = reader["Nome_municipio"]?.ToString() ?? "Não informada";
                                locEscola.InnerText = municipio;

                                // Período de inscrições
                                if (reader["Inicio_inscricoes"] != DBNull.Value && reader["Fim_inscricoes"] != DBNull.Value)
                                {
                                    DateTime inicio = Convert.ToDateTime(reader["Inicio_inscricoes"]);
                                    DateTime fim = Convert.ToDateTime(reader["Fim_inscricoes"]);
                                    periodoEscola.InnerText = $"De {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}";
                                }
                                else
                                    periodoEscola.InnerText = "Não informado";

                                // Email
                                emailEscola.InnerText = reader["Email_escola"]?.ToString() ?? "Não informado";

                                // Website
                                if (reader["Website"] != DBNull.Value && !string.IsNullOrEmpty(reader["Website"].ToString()))
                                    webEscola.InnerText = $"<a href='{reader["Website"]}' target='_blank'>{reader["Website"]}</a>";
                                else
                                    webEscola.InnerText = "Não informado";

                                // Carregar cursos
                                CarregarCursos(id);
                            }
                            else
                            {
                                Response.Redirect("VerEscolas.aspx");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarDetalhesEscola: {ex.Message}");
                Response.Redirect("VerEscolas.aspx");
            }
        }

        private void CarregarCursos(int escolaId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_curso, Descricao, Vagas, Propina FROM Curso WHERE Escola_id = @escolaId", conn))
                    {
                        cmd.Parameters.AddWithValue("@escolaId", escolaId);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                RepeaterCursos.DataSource = dt;
                                RepeaterCursos.DataBind();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarCursos: {ex.Message}");
            }
        }

        public string TruncateDescription(object descricao, int length)
        {
            if (descricao == null || descricao == DBNull.Value)
                return "Sem descrição";

            string text = descricao.ToString();
            if (text.Length <= length)
                return text;

            return text.Substring(0, length) + "...";
        }

        protected void btnInscrever_Click(object sender, EventArgs e)
        {
            if (Session["IdUsuario"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // Redirecionar para página de inscrição
            string escolaId = Request.QueryString["id"];
            Response.Redirect($"Inscricao.aspx?escolaId={escolaId}");
        }

        protected void Button1_Click(object sender, EventArgs e) { Response.Redirect("Login.aspx"); }
        protected void Button2_Click(object sender, EventArgs e) { Response.Redirect("Cadastro.aspx"); }
        protected void btnNomeUtilizador_Click(object sender, EventArgs e) { Response.Redirect("UsuarioDashboard.aspx"); }
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

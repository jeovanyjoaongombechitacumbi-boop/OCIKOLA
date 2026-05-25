using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PlataformaOCIKOLA
{
    public partial class VerEscolas : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Verificar se o utilizador está autenticado
                if (Session["UsuarioLogado"] != null)
                {
                    divLoggedOut.Visible = false;
                    divLoggedIn.Visible = true;
                    AtualizarNavbarUser();
                }
                else
                {
                    // Tentar restaurar pela cookie
                    TentarRestaurarSessaoCookie();
                }

                CarregarFiltros();
                CarregarEscolas();
            }
        }

        private void TentarRestaurarSessaoCookie()
        {
            HttpCookie cookie = Request.Cookies["LembrarUsuario"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    string email = cookie.Value.Trim().ToLower();

                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_user, Email FROM [User] WHERE LOWER(Email) = @email", conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Session["IdUsuario"] = reader["id"].ToString();
                                Session["NomeUsuario"] = reader["Nome_user"].ToString();
                                Session["UsuarioLogado"] = reader["Email"].ToString();
                                Session.Timeout = 30;

                                divLoggedOut.Visible = false;
                                divLoggedIn.Visible = true;
                                AtualizarNavbarUser();
                            }
                        }
                    }
                }
            }
            catch { }
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

        private void CarregarFiltros()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    // Carregar Tipos de Ensino
                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_tipo_ensino FROM Tipo_ensino ORDER BY Nome_tipo_ensino", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlTipoEnsino.Items.Add(new ListItem(reader["Nome_tipo_ensino"].ToString(), reader["id"].ToString()));
                            }
                        }
                    }

                    // Carregar Municípios
                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_municipio FROM Municipio ORDER BY Nome_municipio", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ddlMunicipio.Items.Add(new ListItem(reader["Nome_municipio"].ToString(), reader["id"].ToString()));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarFiltros: {ex.Message}");
            }
        }

        private void CarregarEscolas()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            e.id,
                            e.Nome_escola,
                            e.Descricao,
                            e.Imagem,
                            e.Inicio_inscricoes,
                            e.Fim_inscricoes,
                            m.Nome_municipio AS NomeMunicipio,
                            te.Nome_tipo_ensino AS NomeTipoEnsino
                        FROM Escola e
                        LEFT JOIN Municipio m ON e.Municipio_id = m.id
                        LEFT JOIN Tipo_ensino te ON e.Tipo_ensino_id = te.id
                        WHERE e.Ativo = 1 OR e.Ativo IS NULL
                        ORDER BY e.Nome_escola
                    ";

                    // Aplicar filtros se existirem
                    if (!string.IsNullOrEmpty(ddlTipoEnsino.SelectedValue))
                    {
                        sql = sql.Replace("WHERE e.Ativo = 1", $"WHERE e.Tipo_ensino_id = {ddlTipoEnsino.SelectedValue} AND (e.Ativo = 1");
                        sql = sql.Replace("ORDER BY", "OR e.Ativo IS NULL) ORDER BY");
                    }

                    if (!string.IsNullOrEmpty(ddlMunicipio.SelectedValue) && ddlMunicipio.SelectedValue != "0")
                    {
                        if (sql.Contains("WHERE e.Tipo_ensino_id"))
                            sql = sql.Replace("OR e.Ativo IS NULL) ORDER BY", $" AND e.Municipio_id = {ddlMunicipio.SelectedValue} OR e.Ativo IS NULL) ORDER BY");
                        else
                            sql = sql.Replace("WHERE e.Ativo = 1", $"WHERE e.Municipio_id = {ddlMunicipio.SelectedValue} AND (e.Ativo = 1");
                    }

                    if (!string.IsNullOrEmpty(TextBox1.Text))
                    {
                        string busca = TextBox1.Text.Replace("'", "''");
                        if (sql.Contains("WHERE e.Tipo_ensino_id") || sql.Contains("WHERE e.Municipio_id"))
                            sql = sql.Replace("ORDER BY", $"AND e.Nome_escola LIKE '%{busca}%' ORDER BY");
                        else
                            sql = sql.Replace("WHERE e.Ativo = 1", $"WHERE e.Nome_escola LIKE '%{busca}%' AND (e.Ativo = 1");
                    }

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                Repeater1.DataSource = dt;
                                Repeater1.DataBind();
                                PanelSemEscolas.Visible = false;
                            }
                            else
                            {
                                Repeater1.DataSource = null;
                                Repeater1.DataBind();
                                PanelSemEscolas.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CarregarEscolas: {ex.Message}");
                lblErroEscolas.Text = "Erro ao carregar escolas";
                lblErroEscolas.Visible = true;
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

        public string GetBadgeInscricoes(object dataInicio, object dataFim)
        {
            try
            {
                DateTime hoje = DateTime.Now;

                if (dataInicio != null && dataInicio != DBNull.Value && dataFim != null && dataFim != DBNull.Value)
                {
                    DateTime inicio = Convert.ToDateTime(dataInicio);
                    DateTime fim = Convert.ToDateTime(dataFim);

                    if (hoje >= inicio && hoje <= fim)
                        return "<span class='badge-inscricoes-abertas'>✓ Inscrições Abertas</span>";
                    else if (hoje < inicio)
                        return "<span class='badge-inscricoes-breve'>⏰ Inscrições em Breve</span>";
                    else
                        return "<span class='badge-inscricoes-encerradas'>✗ Inscrições Encerradas</span>";
                }

                return "<span class='badge-inscricoes-breve'>⏰ Data não informada</span>";
            }
            catch
            {
                return "<span class='badge-inscricoes-breve'>⏰ Status desconhecido</span>";
            }
        }

        protected void Repeater1_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "VerDetalhes")
            {
                string escolaId = e.CommandArgument.ToString();
                Response.Redirect($"DetalhesEscola.aspx?id={escolaId}");
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CarregarEscolas();
        }

        protected void btnLimpar_Click(object sender, EventArgs e)
        {
            ddlTipoEnsino.SelectedValue = "";
            ddlCurso.SelectedValue = "";
            ddlMunicipio.SelectedValue = "0";
            ddlBairro.SelectedValue = "";
            ddlPeriodo.SelectedValue = "";
            TextBox1.Text = "";
            CarregarEscolas();
        }

        protected void ddlMunicipio_SelectedIndexChanged(object sender, EventArgs e)
        {
            CarregarEscolas();
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

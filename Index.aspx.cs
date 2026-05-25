using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace PlataformaOCIKOLA
{
    public partial class Index : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Panel divLoggedOut;
        protected global::System.Web.UI.WebControls.Panel divLoggedIn;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl spanAvatar;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl spanUserName;
        protected global::System.Web.UI.WebControls.LinkButton btnSair;
        protected global::System.Web.UI.WebControls.Button Button1;
        protected global::System.Web.UI.WebControls.Button Button2;
        protected global::System.Web.UI.WebControls.LinkButton btnNomeUtilizador;

        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Session["NomeUsuario"] as string))
                    TentarRestaurarSessaoCookie();
            }
            AtualizarNavbar();
        }

        private void TentarRestaurarSessaoCookie()
        {
            HttpCookie cookie = Request.Cookies["LembrarUsuario"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(CS()))
                {
                    con.Open();
                    string email = cookie.Value.Trim().ToLower();

                    using (SqlCommand cmd = new SqlCommand("SELECT id, Nome_user, Email, PerfilCompleto FROM [User] WHERE LOWER(Email) = @email", con))
                    {
                        cmd.Parameters.Add("@email", System.Data.SqlDbType.NVarChar, 255).Value = email;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Session["IdUsuario"] = reader["id"].ToString();
                                Session["NomeUsuario"] = reader["Nome_user"].ToString();
                                Session["UsuarioLogado"] = reader["Email"].ToString();
                                Session.Timeout = 30;

                                // Renovar cookie
                                cookie.Expires = DateTime.Now.AddDays(30);
                                cookie.HttpOnly = true;
                                cookie.Secure = true;
                                Response.Cookies.Add(cookie);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TentarRestaurarSessaoCookie: {ex.Message}");
            }
        }

        private void AtualizarNavbar()
        {
            string nome = Session["NomeUsuario"] as string;
            if (!string.IsNullOrEmpty(nome))
            {
                if (divLoggedOut != null) divLoggedOut.Visible = false;
                if (divLoggedIn != null) divLoggedIn.Visible = true;
                string primeiraLetra = nome.Trim().Length > 0 ? nome.Trim()[0].ToString().ToUpper() : "?";
                if (spanAvatar != null) spanAvatar.InnerText = primeiraLetra;
                if (spanUserName != null) spanUserName.InnerText = nome;
            }
            else
            {
                if (divLoggedOut != null) divLoggedOut.Visible = true;
                if (divLoggedIn != null) divLoggedIn.Visible = false;
            }
        }

        protected void Button1_Click(object sender, EventArgs e) { Response.Redirect("Login.aspx"); }
        protected void Button2_Click(object sender, EventArgs e) { Response.Redirect("Cadastro.aspx"); }

        protected void btnNomeUtilizador_Click(object sender, EventArgs e)
        {
            Response.Redirect("UsuarioDashboard.aspx");
        }

        protected void btnSair_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            HttpCookie cookie = new HttpCookie("LembrarUsuario");
            cookie.Value = "";
            cookie.Expires = DateTime.Now.AddDays(-1);
            cookie.HttpOnly = true;
            cookie.Secure = true;
            Response.Cookies.Add(cookie);

            Response.Redirect("Index.aspx");
        }
    }
}

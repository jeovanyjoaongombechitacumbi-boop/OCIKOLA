using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;

namespace PlataformaOCIKOLA
{
    public partial class Login : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Redirecionar se já está logado
            if (Session["UsuarioLogado"] != null)
            {
                Response.Redirect("VerEscolas.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string code = Request.QueryString["code"];
                string error = Request.QueryString["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    string errorDesc = Request.QueryString["error_description"] ?? "Autenticação recusada";
                    MostrarErro(System.Web.HttpUtility.HtmlEncode(errorDesc));
                }
                else if (!string.IsNullOrEmpty(code))
                {
                    ProcessGoogleLogin(code);
                }
            }
        }

        private void ProcessGoogleLogin(string code)
        {
            try
            {
                GoogleUserInfo userInfo = GetGoogleUserInfo(code);

                if (userInfo != null && !string.IsNullOrEmpty(userInfo.email))
                {
                    Usuario usuario = CadastrarOuObterUsuario(userInfo);

                    if (usuario != null)
                    {
                        if (usuario.PerfilCompleto)
                        {
                            IniciarSessao(usuario);
                            Response.Redirect("VerEscolas.aspx");
                        }
                        else
                        {
                            Session["TempUserId"] = usuario.Id;
                            Session["TempUserEmail"] = usuario.Email;
                            Session["TempUserName"] = usuario.Nome;
                            Session["TempUserFoto"] = usuario.FotoPerfil ?? "";
                            Response.Redirect("CompletarPerfil.aspx");
                        }
                    }
                    else
                    {
                        MostrarErro("Erro ao processar cadastro. Tente novamente.");
                    }
                }
                else
                {
                    MostrarErro("Email não disponível no Google. Tente com outra conta.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProcessGoogleLogin: {ex.Message}");
                MostrarErro("Erro ao processar login. Tente novamente mais tarde.");
            }
        }

        private void IniciarSessao(Usuario usuario)
        {
            Session["IdUsuario"] = usuario.Id;
            Session["UsuarioLogado"] = usuario.Email;
            Session["NomeUsuario"] = usuario.Nome;
            Session["FotoUsuario"] = usuario.FotoPerfil ?? "";
            Session.Timeout = 30;

            // Cookie para "lembrar" utilizador
            HttpCookie cookie = new HttpCookie("LembrarUsuario")
            {
                Value = usuario.Email,
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(30)
            };
            Response.Cookies.Add(cookie);
        }

        private GoogleUserInfo GetGoogleUserInfo(string code)
        {
            try
            {
                string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
                string clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
                string redirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    System.Diagnostics.Debug.WriteLine("Google configuration missing");
                    return null;
                }

                // Validar código
                if (string.IsNullOrWhiteSpace(code) || code.Length > 500)
                    return null;

                // Trocar código por token
                TokenResponse tokenResponse = ExchangeCodeForToken(code, clientId, clientSecret, redirectUri);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
                    return null;

                // Obter informações do utilizador
                return GetUserInfoFromGoogle(tokenResponse.access_token);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetGoogleUserInfo: {ex.Message}");
                return null;
            }
        }

        private TokenResponse ExchangeCodeForToken(string code, string clientId, string clientSecret, string redirectUri)
        {
            try
            {
                string tokenUrl = "https://oauth2.googleapis.com/token";
                string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                    HttpUtility.UrlEncode(code),
                    HttpUtility.UrlEncode(clientId),
                    HttpUtility.UrlEncode(clientSecret),
                    HttpUtility.UrlEncode(redirectUri));

                byte[] data = System.Text.Encoding.UTF8.GetBytes(postData);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(tokenUrl);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                request.Timeout = 10000;

                using (var stream = request.GetRequestStream())
                    stream.Write(data, 0, data.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return null;

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        var serializer = new JavaScriptSerializer();
                        return serializer.Deserialize<TokenResponse>(responseText);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExchangeCodeForToken: {ex.Message}");
                return null;
            }
        }

        private GoogleUserInfo GetUserInfoFromGoogle(string accessToken)
        {
            try
            {
                string userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                HttpWebRequest userRequest = (HttpWebRequest)WebRequest.Create(userInfoUrl);
                userRequest.Headers.Add("Authorization", "Bearer " + accessToken);
                userRequest.Timeout = 10000;

                using (var userResponse = (HttpWebResponse)userRequest.GetResponse())
                {
                    if (userResponse.StatusCode != HttpStatusCode.OK)
                        return null;

                    using (var userReader = new StreamReader(userResponse.GetResponseStream()))
                    {
                        string userInfoJson = userReader.ReadToEnd();
                        var serializer = new JavaScriptSerializer();
                        return serializer.Deserialize<GoogleUserInfo>(userInfoJson);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserInfoFromGoogle: {ex.Message}");
                return null;
            }
        }

        private Usuario CadastrarOuObterUsuario(GoogleUserInfo googleUser)
        {
            if (googleUser == null || string.IsNullOrEmpty(googleUser.email))
                return null;

            Usuario usuario = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();

                    string email = googleUser.email.Trim().ToLower();

                    // Procurar utilizador existente
                    string selectSql = "SELECT id, Nome_user, Email, FotoPerfil, PerfilCompleto FROM [User] WHERE LOWER(Email) = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                usuario = new Usuario
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Email = reader["Email"].ToString(),
                                    Nome = reader["Nome_user"].ToString(),
                                    FotoPerfil = reader["FotoPerfil"]?.ToString(),
                                    PerfilCompleto = Convert.ToBoolean(reader["PerfilCompleto"])
                                };
                            }
                        }
                    }

                    // Se não existe, criar novo utilizador
                    if (usuario == null)
                    {
                        string nome = !string.IsNullOrEmpty(googleUser.name)
                            ? googleUser.name.Trim()
                            : googleUser.email.Split('@')[0];

                        string insertSql = @"INSERT INTO [User] (GoogleId, Email, Nome_user, FotoPerfil, DataCadastro, UltimoAcesso, TipoLogin, PerfilCompleto, Ativo)
                                            VALUES (@GoogleId, @Email, @Nome, @Foto, @DataCadastro, @UltimoAcesso, 'Google', 0, 1);
                                            SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@GoogleId", googleUser.id ?? "");
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Nome", nome);
                            cmd.Parameters.AddWithValue("@Foto", googleUser.picture ?? "");
                            cmd.Parameters.AddWithValue("@DataCadastro", DateTime.Now);
                            cmd.Parameters.AddWithValue("@UltimoAcesso", DateTime.Now);

                            int newId = Convert.ToInt32(cmd.ExecuteScalar());

                            usuario = new Usuario
                            {
                                Id = newId,
                                Email = email,
                                Nome = nome,
                                FotoPerfil = googleUser.picture,
                                PerfilCompleto = false
                            };
                        }
                    }
                    else
                    {
                        // Atualizar último acesso
                        string updateSql = "UPDATE [User] SET UltimoAcesso = @UltimoAcesso WHERE id = @Id";
                        using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@UltimoAcesso", DateTime.Now);
                            cmd.Parameters.AddWithValue("@Id", usuario.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CadastrarOuObterUsuario: {ex.Message}");
                return null;
            }

            return usuario;
        }

        public string GetGoogleAuthUrl()
        {
            string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            string redirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
                return "";

            return string.Format("https://accounts.google.com/o/oauth2/v2/auth?client_id={0}&redirect_uri={1}&response_type=code&scope=email%20profile&access_type=online&prompt=select_account",
                HttpUtility.UrlEncode(clientId),
                HttpUtility.UrlEncode(redirectUri));
        }

        private void MostrarErro(string mensagem)
        {
            ltMessage.Text = $"<div class='message-box message-error'>❌ {mensagem}</div>";
            ltMessage.Visible = true;
        }

        private void MostrarSucesso(string mensagem)
        {
            ltMessage.Text = $"<div class='message-box message-success'>✅ {mensagem}</div>";
            ltMessage.Visible = true;
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class GoogleUserInfo
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Nome { get; set; }
        public string FotoPerfil { get; set; }
        public bool PerfilCompleto { get; set; }
    }
}

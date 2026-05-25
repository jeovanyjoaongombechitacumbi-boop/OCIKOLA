using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace PlataformaOCIKOLA
{
    public partial class Cadastro : System.Web.UI.Page
    {
        private string CS() => ConfigurationManager.ConnectionStrings["OcikolaDBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UsuarioLogado"] != null)
            {
                Response.Redirect("VerEscolas.aspx");
                return;
            }
        }

        protected void btnCadastrar_Click(object sender, EventArgs e)
        {
            try
            {
                string nome = txtNome.Value.Trim();
                string email = txtEmail.Value.Trim().ToLower();
                string senha = txtSenha.Value;
                string confirmaSenha = txtConfirmaSenha.Value;

                // Validações
                if (string.IsNullOrEmpty(nome) || nome.Length < 3)
                {
                    MostrarErro("Nome deve ter pelo menos 3 caracteres");
                    return;
                }

                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                {
                    MostrarErro("Email inválido");
                    return;
                }

                if (string.IsNullOrEmpty(senha) || senha.Length < 6)
                {
                    MostrarErro("Senha deve ter pelo menos 6 caracteres");
                    return;
                }

                if (senha != confirmaSenha)
                {
                    MostrarErro("Senhas não coincidem");
                    return;
                }

                // Verificar se email já existe
                if (EmailExiste(email))
                {
                    MostrarErro("Este email já está registado");
                    return;
                }

                // Gerar código 2FA
                string codigo = GerarCodigo2FA();
                string codigoHash = BCrypt.Net.BCrypt.HashPassword(codigo);

                // Guardar código no banco de dados
                DateTime dataExpiracao = DateTime.Now.AddMinutes(15);
                GuardarCodigo2FA(codigoHash, email, codigo, dataExpiracao);

                // Enviar email com código
                if (EnviarEmailCodigo(email, nome, codigo))
                {
                    // Guardar dados temporários na sessão
                    Session["TempNome"] = nome;
                    Session["TempEmail"] = email;
                    Session["TempSenha"] = SHA256Hash(senha);
                    Session["TempDataCadastro"] = DateTime.Now;

                    MostrarSucesso("Código enviado para seu email! Verifique a sua caixa de entrada.");
                    pnlCadastroForm.Visible = false;
                    pnlVerificacaoCodigo.Visible = true;
                }
                else
                {
                    MostrarErro("Erro ao enviar email. Tente novamente.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnCadastrar_Click: {ex.Message}");
                MostrarErro("Erro ao processar cadastro");
            }
        }

        protected void btnVerificarCodigo_Click(object sender, EventArgs e)
        {
            try
            {
                string codigoInserido = txtCodigoVerificacao.Value.Trim();

                if (string.IsNullOrEmpty(codigoInserido) || codigoInserido.Length != 6)
                {
                    MostrarErro("Código inválido");
                    return;
                }

                string email = Session["TempEmail"]?.ToString();

                if (string.IsNullOrEmpty(email))
                {
                    MostrarErro("Sessão expirada. Tente novamente.");
                    pnlCadastroForm.Visible = true;
                    pnlVerificacaoCodigo.Visible = false;
                    return;
                }

                // Verificar código
                if (VerificarCodigo2FA(email, codigoInserido))
                {
                    // Criar utilizador
                    string nome = Session["TempNome"]?.ToString();
                    string senhaHash = Session["TempSenha"]?.ToString();

                    int usuarioId = CriarUsuario(nome, email, senhaHash);

                    if (usuarioId > 0)
                    {
                        // Iniciar sessão
                        IniciarSessao(usuarioId, nome, email);

                        // Redirecionar para completar perfil
                        Session["TempUserId"] = usuarioId;
                        Session["TempUserName"] = nome;
                        Session["TempUserEmail"] = email;
                        Response.Redirect("CompletarPerfil.aspx");
                    }
                    else
                    {
                        MostrarErro("Erro ao criar utilizador");
                    }
                }
                else
                {
                    MostrarErro("Código inválido ou expirado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"btnVerificarCodigo_Click: {ex.Message}");
                MostrarErro("Erro ao verificar código");
            }
        }

        private bool EmailExiste(string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE LOWER(Email) = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private string GerarCodigo2FA()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private void GuardarCodigo2FA(string codigoHash, string email, string codigo, DateTime dataExpiracao)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Codigo2FA (CodigoHash, Email, Codigo, Data_expiracao, Usado) 
                                                              VALUES (@CodigoHash, @Email, @Codigo, @DataExpiracao, 0)", conn))
                    {
                        cmd.Parameters.AddWithValue("@CodigoHash", codigoHash);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Codigo", codigo);
                        cmd.Parameters.AddWithValue("@DataExpiracao", dataExpiracao);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GuardarCodigo2FA: {ex.Message}");
            }
        }

        private bool VerificarCodigo2FA(string email, string codigoInserido)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"SELECT Codigo, Data_expiracao, Usado FROM Codigo2FA 
                                                              WHERE Email = @Email AND Usado = 0 
                                                              ORDER BY id DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string codigoArmazenado = reader["Codigo"].ToString();
                                DateTime dataExpiracao = Convert.ToDateTime(reader["Data_expiracao"]);

                                // Verificar se expirou
                                if (DateTime.Now > dataExpiracao)
                                    return false;

                                // Verificar se o código corresponde
                                if (codigoArmazenado == codigoInserido)
                                {
                                    // Marcar como usado
                                    conn.Close();
                                    conn.Open();
                                    using (SqlCommand updateCmd = new SqlCommand("UPDATE Codigo2FA SET Usado = 1 WHERE Email = @Email AND Codigo = @Codigo", conn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@Email", email);
                                        updateCmd.Parameters.AddWithValue("@Codigo", codigoInserido);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VerificarCodigo2FA: {ex.Message}");
            }

            return false;
        }

        private bool EnviarEmailCodigo(string email, string nome, string codigo)
        {
            try
            {
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;
                string remetente = ConfigurationManager.AppSettings["EmailRemetente"];
                string senhaApp = ConfigurationManager.AppSettings["SenhaAppEmail"];

                using (SmtpClient client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(remetente, senhaApp);

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(remetente, "OCIKOLA");
                        mail.To.Add(email);
                        mail.Subject = "Código de Verificação OCIKOLA";
                        mail.Body = $@"
Olá {nome},

Bem-vindo(a) à OCIKOLA!

Seu código de verificação é: <strong>{codigo}</strong>

Este código expira em 15 minutos.

Se não solicitou este código, por favor ignore esta mensagem.

Atenciosamente,
Equipa OCIKOLA
";
                        mail.IsBodyHtml = true;

                        client.Send(mail);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnviarEmailCodigo: {ex.Message}");
                return false;
            }
        }

        private int CriarUsuario(string nome, string email, string senhaHash)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(CS()))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"INSERT INTO [User] (Email, Nome_user, Senha, DataCadastro, UltimoAcesso, TipoLogin, PerfilCompleto, Ativo)
                                                              VALUES (@Email, @Nome, @Senha, @DataCadastro, @UltimoAcesso, 'Email', 0, 1);
                                                              SELECT SCOPE_IDENTITY();", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Nome", nome);
                        cmd.Parameters.AddWithValue("@Senha", senhaHash);
                        cmd.Parameters.AddWithValue("@DataCadastro", DateTime.Now);
                        cmd.Parameters.AddWithValue("@UltimoAcesso", DateTime.Now);

                        int usuarioId = Convert.ToInt32(cmd.ExecuteScalar());
                        return usuarioId;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CriarUsuario: {ex.Message}");
                return -1;
            }
        }

        private void IniciarSessao(int usuarioId, string nome, string email)
        {
            Session["IdUsuario"] = usuarioId;
            Session["UsuarioLogado"] = email;
            Session["NomeUsuario"] = nome;
            Session.Timeout = 30;
        }

        private string SHA256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashedBytes);
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

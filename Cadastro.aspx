<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cadastro.aspx.cs" Inherits="PlataformaOCIKOLA.Cadastro" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Cadastro - OCIKOLA</title>
    <link href="bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="icons/bootsrtrap-icons/bootstrap-icons.css" rel="stylesheet" />
    <link href="StyleSheet1.css" rel="stylesheet" />
    <style>
        :root {
            --verde: #0f5f3a;
            --verde-esc: #0a4a2e;
            --azul-esc: #1a2b4c;
            --cinza-f: #f4f6f9;
            --cinza-bd: #dee2e6;
            --texto: #212529;
            --texto-mut: #6c757d;
            --raio: 12px;
            --sombra: 0 4px 24px rgba(0,0,0,.08);
        }
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
            background: var(--cinza-f);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 2rem 1rem;
        }
        .auth-card {
            background: #fff;
            border-radius: var(--raio);
            box-shadow: var(--sombra);
            padding: 2.5rem 2.25rem;
            width: 100%;
            max-width: 480px;
        }
        .logo-wrap {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: .5rem;
            margin-bottom: .5rem;
        }
        .logo-wrap .bi-book { font-size: 2rem; color: var(--verde); }
        .logo-texto { font-size: 1.75rem; font-weight: 800; color: var(--azul-esc); letter-spacing: .04em; }
        .subtitulo { color: var(--texto-mut); font-size: .9rem; text-align: center; margin-bottom: 1.75rem; }
        .form-group { margin-bottom: 1rem; }
        .form-group label { font-weight: 600; color: var(--texto); font-size: .9rem; margin-bottom: .5rem; display: block; }
        .form-group input {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid var(--cinza-bd);
            border-radius: 8px;
            font-size: .95rem;
            transition: all 0.3s;
        }
        .form-group input:focus {
            outline: none;
            border-color: var(--verde);
            box-shadow: 0 0 0 3px rgba(15, 95, 58, 0.1);
        }
        .btn-submit {
            width: 100%;
            background: var(--verde);
            color: white;
            border: none;
            padding: 12px;
            border-radius: 8px;
            font-weight: 600;
            font-size: .95rem;
            cursor: pointer;
            transition: all 0.3s;
            margin-top: 1rem;
        }
        .btn-submit:hover {
            background: var(--verde-esc);
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(15, 95, 58, 0.3);
        }
        .message-box {
            padding: 12px;
            border-radius: 8px;
            margin-bottom: 20px;
            text-align: center;
        }
        .message-error {
            background: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        .message-success {
            background: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .link-login { font-size: .875rem; color: var(--texto-mut); text-align: center; margin-top: 1.25rem; }
        .link-login a { color: var(--verde); font-weight: 600; text-decoration: none; }
        .divider { display: flex; align-items: center; gap: 1rem; margin: 1.5rem 0; }
        .divider::before, .divider::after { content: ''; flex: 1; height: 1px; background: var(--cinza-bd); }
        .divider-text { font-size: .85rem; color: var(--texto-mut); }
        .google-btn {
            width: 100%;
            background: white;
            border: 2px solid #ddd;
            border-radius: 8px;
            padding: 10px;
            font-size: .95rem;
            font-weight: 600;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
            transition: all 0.3s;
            margin-bottom: 1rem;
        }
        .google-btn:hover {
            border-color: #4285f4;
            background: #f8f9fa;
        }
        .voltar { text-align: center; margin-top: 1.5rem; }
        .voltar a { font-size: .85rem; color: var(--texto-mut); text-decoration: none; display: inline-flex; align-items: center; gap: .35rem; }
        .voltar a:hover { color: var(--verde); }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="auth-card">
            <div class="logo-wrap">
                <i class="bi bi-book"></i>
                <span class="logo-texto">OCIKOLA</span>
            </div>
            <p class="subtitulo">Crie sua conta para acessar a plataforma</p>

            <asp:Literal ID="ltMensagem" runat="server" Visible="false"></asp:Literal>

            <!-- Painel de Cadastro -->
            <div id="pnlCadastroForm" runat="server" visible="true">
                <!-- Login com Google -->
                <button type="button" class="google-btn" onclick="redirectToGoogleSignup()">
                    <svg width="18" height="18" viewBox="0 0 24 24">
                        <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                        <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                        <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                        <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
                    </svg>
                    Cadastrar com Google
                </button>

                <div class="divider">
                    <span class="divider-text">OU</span>
                </div>

                <!-- Formulário de Cadastro por Email -->
                <div class="form-group">
                    <label for="txtNome">Nome Completo</label>
                    <input type="text" id="txtNome" runat="server" placeholder="João Silva" required />
                </div>

                <div class="form-group">
                    <label for="txtEmail">Email</label>
                    <input type="email" id="txtEmail" runat="server" placeholder="seu.email@exemplo.com" required />
                </div>

                <div class="form-group">
                    <label for="txtSenha">Senha</label>
                    <input type="password" id="txtSenha" runat="server" placeholder="Mínimo 6 caracteres" required />
                </div>

                <div class="form-group">
                    <label for="txtConfirmaSenha">Confirmar Senha</label>
                    <input type="password" id="txtConfirmaSenha" runat="server" placeholder="Confirme a senha" required />
                </div>

                <asp:Button ID="btnCadastrar" runat="server" Text="Criar Conta" CssClass="btn-submit" OnClick="btnCadastrar_Click" />

                <p class="link-login">
                    Já tem conta? <a href="Login.aspx">Faça login aqui</a>
                </p>
            </div>

            <!-- Painel de Verificação de Código -->
            <div id="pnlVerificacaoCodigo" runat="server" visible="false">
                <p style="color: var(--texto-mut); font-size: .9rem; margin-bottom: 1.5rem;">
                    Enviamos um código de 6 dígitos para seu email. Digite-o abaixo para confirmar o cadastro.
                </p>

                <div class="form-group">
                    <label for="txtCodigoVerificacao">Código de Verificação</label>
                    <input type="text" id="txtCodigoVerificacao" runat="server" placeholder="000000" 
                           maxlength="6" style="text-align: center; font-size: 1.3rem; letter-spacing: 5px;" required />
                </div>

                <p style="color: var(--texto-mut); font-size: .85rem; text-align: center; margin-bottom: 1rem;">
                    Código expira em <strong>15 minutos</strong>
                </p>

                <asp:Button ID="btnVerificarCodigo" runat="server" Text="Verificar e Continuar" 
                            CssClass="btn-submit" OnClick="btnVerificarCodigo_Click" />

                <button type="button" class="btn-submit" style="background: #6c757d; margin-top: 0.5rem;" 
                        onclick="document.getElementById('pnlCadastroForm').style.display='block'; document.getElementById('pnlVerificacaoCodigo').style.display='none'; return false;">
                    Voltar
                </button>
            </div>
        </div>

        <div class="voltar">
            <a href="Index.aspx"><i class="bi bi-arrow-left"></i> Voltar para a página inicial</a>
        </div>
    </form>

    <script>
        function redirectToGoogleSignup() {
            var clientId = '<%= System.Configuration.ConfigurationManager.AppSettings["GoogleClientId"] %>';
            var redirectUri = '<%= System.Configuration.ConfigurationManager.AppSettings["GoogleRedirectUri"] %>';

            if (!clientId || clientId === '') {
                alert('Erro de configuração. Contacte o administrador.');
                return false;
            }

            var url = 'https://accounts.google.com/o/oauth2/v2/auth?' +
                'client_id=' + encodeURIComponent(clientId) +
                '&redirect_uri=' + encodeURIComponent(redirectUri) +
                '&response_type=code' +
                '&scope=email%20profile' +
                '&access_type=online' +
                '&prompt=consent';

            window.location.href = url;
            return false;
        }
    </script>
</body>
</html>

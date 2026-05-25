<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DetalhesEscola.aspx.cs" Inherits="PlataformaOCIKOLA.DetalhesEscola" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Detalhes da Escola - OCIKOLA</title>
    <link href="bootstrap/css/bootstrap.min.css" rel="stylesheet" />
    <link href="icons/bootsrtrap-icons/bootstrap-icons.css" rel="stylesheet" />
    <link href="StyleSheet1.css" rel="stylesheet" />
    <style>
        :root { --verde:#0f5f3a; --verde-esc:#0a4a2e; }
        body { background:#f5f6f8; font-family:'DM Sans',sans-serif; }
        .hero-image { width:100%; height:400px; object-fit:cover; border-radius:15px; }
        .info-section { background:#fff; padding:30px; border-radius:15px; margin-bottom:25px; box-shadow:0 2px 8px rgba(0,0,0,.08); }
        .info-title { color:var(--verde); font-weight:700; font-size:1.4rem; margin-bottom:20px; }
        .info-item { display:flex; align-items:flex-start; gap:15px; margin-bottom:15px; }
        .info-icon { width:40px; height:40px; border-radius:50%; background:#e6f4ed; display:flex; align-items:center; justify-content:center; color:var(--verde); flex-shrink:0; }
        .info-content h6 { font-weight:600; margin-bottom:5px; }
        .info-content p { color:#6b7280; margin:0; font-size:.95rem; }
        .btn-inscrever { background:var(--verde); color:#fff; border:none; padding:12px 30px; border-radius:8px; font-weight:600; cursor:pointer; transition:all .3s; }
        .btn-inscrever:hover { background:var(--verde-esc); transform:translateY(-2px); box-shadow:0 4px 12px rgba(15,95,58,.3); }
        .cursos-grid { display:grid; grid-template-columns:repeat(auto-fit, minmax(280px, 1fr)); gap:20px; }
        .curso-card { background:#fff; padding:20px; border-radius:12px; box-shadow:0 2px 8px rgba(0,0,0,.08); border-left:4px solid var(--verde); }
        .curso-card h6 { color:var(--verde); font-weight:700; margin-bottom:10px; }
        .nav-user-pill { display:flex; align-items:center; gap:8px; background:#e6f4ed; border-radius:50px; padding:5px 14px 5px 5px; font-size:.85rem; font-weight:600; color:var(--verde-esc); text-decoration:none; transition:background .2s; }
        .nav-user-pill:hover { background:#c8e6d8; }
        .nav-user-avatar { width:30px; height:30px; border-radius:50%; background:var(--verde); color:#fff; font-size:.75rem; font-weight:700; display:inline-flex; align-items:center; justify-content:center; flex-shrink:0; }
    </style>
</head>
<body>
<form id="form1" runat="server">

    <!-- NAVBAR -->
    <nav class="navbar navbar-expand-lg navbar-light bg-white fixed-top shadow-sm">
        <div class="container">
            <a class="navbar-brand" href="index.aspx">
                <div class="logo-container">
                    <i class="bi bi-book me-2 logo-icon"></i>
                    <span class="logo-text">OCIKOLA</span>
                </div>
            </a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#menu">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="menu">
                <ul class="navbar-nav mx-auto">
                    <li class="nav-item"><a href="index.aspx" class="nav-link">Início</a></li>
                    <li class="nav-item"><a href="VerEscolas.aspx" class="nav-link active fw-bold">Escolas</a></li>
                </ul>

                <div class="d-flex align-items-center gap-2" id="divLoggedOut" runat="server" visible="true">
                    <asp:Button ID="Button1" CssClass="btn btn-success btn-sm me-1" runat="server" Text="Entrar" OnClick="Button1_Click" />
                    <asp:Button ID="Button2" CssClass="btn btn-outline-success btn-sm" runat="server" Text="Cadastrar" OnClick="Button2_Click" />
                </div>

                <div class="d-flex align-items-center gap-2" id="divLoggedIn" runat="server" visible="false">
                    <asp:LinkButton ID="btnNomeUtilizador" runat="server" CssClass="nav-user-pill" OnClick="btnNomeUtilizador_Click">
                        <span class="nav-user-avatar" id="spanAvatar" runat="server">U</span>
                        <span id="spanUserName" runat="server">Utilizador</span>
                        <i class="bi bi-chevron-right small"></i>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnSair" runat="server" style="background:transparent; border:none; color:#6c757d; font-size:.85rem; cursor:pointer;" OnClick="btnSair_Click" ToolTip="Terminar sessão">
                        <i class="bi bi-box-arrow-right"></i> Sair
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </nav>

    <!-- CONTEÚDO PRINCIPAL -->
    <div style="margin-top:80px; padding-bottom:50px;">
        <div class="container mt-5">
            <!-- Imagem Hero -->
            <asp:Image ID="imgEscola" runat="server" CssClass="hero-image" AlternateText="Imagem da Escola" />

            <!-- Informações Básicas -->
            <div class="row mt-5">
                <div class="col-lg-8">
                    <div class="info-section">
                        <h2 id="titleEscola" runat="server" class="info-title">Nome da Escola</h2>
                        <p id="descEscola" runat="server" style="color:#6b7280; line-height:1.6;"></p>

                        <hr />

                        <div class="info-item">
                            <div class="info-icon"><i class="bi bi-geo-alt"></i></div>
                            <div class="info-content">
                                <h6>Localização</h6>
                                <p id="locEscola" runat="server">Não informada</p>
                            </div>
                        </div>

                        <div class="info-item">
                            <div class="info-icon"><i class="bi bi-calendar2"></i></div>
                            <div class="info-content">
                                <h6>Período de Inscrições</h6>
                                <p id="periodoEscola" runat="server">Não informado</p>
                            </div>
                        </div>

                        <div class="info-item">
                            <div class="info-icon"><i class="bi bi-envelope"></i></div>
                            <div class="info-content">
                                <h6>Email</h6>
                                <p id="emailEscola" runat="server">Não informado</p>
                            </div>
                        </div>

                        <div class="info-item">
                            <div class="info-icon"><i class="bi bi-globe"></i></div>
                            <div class="info-content">
                                <h6>Website</h6>
                                <p id="webEscola" runat="server">Não informado</p>
                            </div>
                        </div>
                    </div>

                    <!-- Cursos -->
                    <div class="info-section">
                        <h4 class="info-title"><i class="bi bi-book me-2"></i>Cursos Disponíveis</h4>
                        <div class="cursos-grid">
                            <asp:Repeater ID="RepeaterCursos" runat="server">
                                <ItemTemplate>
                                    <div class="curso-card">
                                        <h6><%# Eval("Nome_curso") %></h6>
                                        <p class="text-muted small"><%# TruncateDescription(Eval("Descricao"), 80) %></p>
                                        <small class="text-muted"><strong>Vagas:</strong> <%# Eval("Vagas") %></small><br />
                                        <small class="text-muted"><strong>Propina:</strong> AOA <%# Eval("Propina") %></small>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>

                <!-- Sidebar CTA -->
                <div class="col-lg-4">
                    <div class="info-section position-sticky" style="top:100px;">
                        <h5 class="fw-bold mb-3">Inscrever-se</h5>
                        <p class="text-muted small mb-3">Clique no botão abaixo para se inscrever nesta escola.</p>
                        <asp:Button ID="btnInscrever" runat="server" Text="Inscrever-se Agora" CssClass="btn-inscrever w-100" OnClick="btnInscrever_Click" />
                        <p class="text-muted small mt-3 text-center">Precisas de ajuda? <a href="#">Contacta-nos</a></p>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- FOOTER -->
    <footer class="bg-dark text-white py-5 mt-5">
        <div class="container">
            <div class="row">
                <div class="col-lg-4 mb-4">
                    <div class="logo-container mb-3">
                        <i class="bi bi-book logo-icon"></i>
                        <span class="logo-white">OCIKOLA</span>
                    </div>
                    <p class="text-white-50">Plataforma digital de localização e interação escolar.</p>
                </div>
            </div>
            <hr class="my-4 border-secondary" />
            <div class="row">
                <div class="col-md-6">
                    <p class="mb-0 text-white-50">&copy; 2026 Ocikola. Todos os direitos reservados.</p>
                </div>
            </div>
        </div>
    </footer>

</form>
<script src="bootstrap/js/bootstrap.bundle.min.js"></script>
</body>
</html>

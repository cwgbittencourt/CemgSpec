param(
    [switch]$InstallLocally
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $PSScriptRoot "dist\SpecAudit-win-x64"
$dockerPublishDir = Join-Path $PSScriptRoot "dist\SpecAudit-docker-payload"
$payloadZip = Join-Path $PSScriptRoot "dist\payload.zip"
$dockerPayloadZip = Join-Path $PSScriptRoot "dist\docker-payload.zip"
$setupExe = Join-Path $PSScriptRoot "dist\SpecAudit-Setup.exe"
$dockerSetupExe = Join-Path $PSScriptRoot "dist\SpecAudit-Docker-Setup.exe"
$icoPath = Join-Path $repoRoot "SpecAudit\wwwroot\specaudit.ico"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "1/6 Publicando SpecAudit Windows (win-x64, Self-Contained, Release com icone oficial)..." -ForegroundColor Cyan
dotnet publish (Join-Path $repoRoot "SpecAudit\SpecAudit.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o $publishDir `
    --nologo

Copy-Item $icoPath (Join-Path $publishDir "specaudit.ico") -Force

$docsOut = Join-Path $publishDir "docs"
New-Item -ItemType Directory -Force -Path $docsOut | Out-Null
foreach ($doc in @("GUIA_DE_USO_TELAS.md", "apresentacao-specaudit.html", "apresentacao-specaudit.pdf")) {
    $srcDoc = Join-Path $repoRoot "docs\$doc"
    if (Test-Path $srcDoc) { Copy-Item $srcDoc $docsOut -Force }
}

Write-Host "2/6 Compilando SpecAudit-Launcher.exe (sem janela de prompt) e SpecAudit-Uninstall.exe..." -ForegroundColor Cyan

# --- SpecAudit-Launcher.exe ---
$launcherCs = Join-Path $PSScriptRoot "dist\LauncherProgram.cs"
$launcherCode = @'
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("SpecAudit Launcher")]
[assembly: AssemblyDescription("Inicializador em segundo plano do SpecAudit (sem janela de prompt)")]
[assembly: AssemblyCompany("Carlos Wagner G. Bittencourt")]
[assembly: AssemblyProduct("SpecAudit")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace SpecAuditLauncher
{
    public sealed class TrayContext : ApplicationContext
    {
        private readonly NotifyIcon trayIcon;
        private Process serverProcess;
        private readonly string url = "http://127.0.0.1:5187";

        public TrayContext()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exePath = Path.Combine(baseDir, "SpecAudit.exe");
            var icoPath = Path.Combine(baseDir, "specaudit.ico");

            Icon appIcon = SystemIcons.Application;
            try
            {
                if (File.Exists(icoPath)) appIcon = new Icon(icoPath);
                else appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            var menu = new ContextMenuStrip();
            var itemOpen = new ToolStripMenuItem("Abrir SpecAudit no navegador (127.0.0.1:5187)");
            itemOpen.Font = new Font(itemOpen.Font, FontStyle.Bold);
            itemOpen.Click += (s, e) => OpenBrowser();

            var itemStop = new ToolStripMenuItem("Encerrar SpecAudit");
            itemStop.Click += (s, e) => ExitServer();

            menu.Items.Add(itemOpen);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemStop);

            trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Text = "SpecAudit — Rodando em segundo plano (127.0.0.1:5187)",
                ContextMenuStrip = menu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => OpenBrowser();

            StartHiddenServer(exePath, baseDir);
        }

        private void StartHiddenServer(string exePath, string baseDir)
        {
            try
            {
                serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = baseDir,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
                    EnableRaisingEvents = true
                };
                serverProcess.Exited += (s, e) =>
                {
                    trayIcon.Visible = false;
                    Application.Exit();
                };
                serverProcess.Start();

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (Program.IsPortListening(5187)) break;
                        Thread.Sleep(200);
                    }
                    OpenBrowser();
                });

                trayIcon.ShowBalloonTip(
                    3000,
                    "SpecAudit ativo em segundo plano",
                    "O SpecAudit foi aberto no navegador sem janela de prompt. Clique neste ícone perto do relógio para reabrir ou encerrar.",
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível iniciar o SpecAudit: " + ex.Message, "SpecAudit", MessageBoxButtons.OK, MessageBoxIcon.Error);
                trayIcon.Visible = false;
                Environment.Exit(1);
            }
        }

        private void OpenBrowser()
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch { }
        }

        private void ExitServer()
        {
            try
            {
                trayIcon.Visible = false;
                if (serverProcess != null && !serverProcess.HasExited)
                {
                    serverProcess.Kill();
                    serverProcess.WaitForExit(2000);
                }
            }
            catch { }
            Application.Exit();
        }
    }

    public static class Program
    {
        public static bool IsPortListening(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var ar = client.BeginConnect("127.0.0.1", port, null, null);
                    var ok = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
                    if (!ok) return false;
                    client.EndConnect(ar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool noBrowser = args != null && Array.Exists(args, a => string.Equals(a, "--no-browser", StringComparison.OrdinalIgnoreCase));

            if (IsPortListening(5187))
            {
                if (!noBrowser)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = "http://127.0.0.1:5187", UseShellExecute = true });
                    }
                    catch { }
                }
                return;
            }

            Application.Run(new TrayContext());
        }
    }
}
'@
[System.IO.File]::WriteAllText($launcherCs, $launcherCode, (New-Object System.Text.UTF8Encoding($true)))
$launcherExe = Join-Path $publishDir "SpecAudit-Launcher.exe"
& $csc /nologo /target:winexe /optimize+ `
    "/out:$launcherExe" `
    "/win32icon:$icoPath" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    $launcherCs
Remove-Item $launcherCs -Force -ErrorAction SilentlyContinue

# --- SpecAudit-Uninstall.exe ---
$uninstallCs = Join-Path $PSScriptRoot "dist\UninstallProgram.cs"
$uninstallCode = @'
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Desinstalador do SpecAudit")]
[assembly: AssemblyDescription("Desinstalador oficial do SpecAudit")]
[assembly: AssemblyCompany("Carlos Wagner G. Bittencourt")]
[assembly: AssemblyProduct("SpecAudit")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace SpecAuditUninstall
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool quiet = args != null && Array.Exists(args, a => string.Equals(a, "--quiet", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "/S", StringComparison.OrdinalIgnoreCase));

            if (!quiet)
            {
                var confirm = MessageBox.Show(
                    "Deseja desinstalar o SpecAudit do seu computador?\n\n(Seus projetos auditados e relatórios salvos não serão afetados.)",
                    "Desinstalar SpecAudit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
            }

            string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "SpecAudit");

            foreach (var procName in new[] { "SpecAudit-Launcher", "SpecAudit" })
            {
                foreach (var p in Process.GetProcessesByName(procName))
                {
                    try { p.Kill(); p.WaitForExit(3000); } catch { }
                }
            }

            try
            {
                var desktopLnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit.lnk");
                if (File.Exists(desktopLnk)) File.Delete(desktopLnk);
            }
            catch { }

            try
            {
                var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "SpecAudit");
                if (Directory.Exists(startMenuDir)) Directory.Delete(startMenuDir, true);
            }
            catch { }

            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\SpecAudit", false);
            }
            catch { }

            if (!quiet)
            {
                MessageBox.Show(
                    "O SpecAudit foi desinstalado com sucesso.",
                    "Desinstalar SpecAudit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            try
            {
                if (Directory.Exists(installDir))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c timeout /t 2 /nobreak >nul & rmdir /s /q \"" + installDir + "\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = Path.GetTempPath()
                    });
                }
            }
            catch { }
        }
    }
}
'@
[System.IO.File]::WriteAllText($uninstallCs, $uninstallCode, (New-Object System.Text.UTF8Encoding($true)))
$uninstallExe = Join-Path $publishDir "SpecAudit-Uninstall.exe"
& $csc /nologo /target:winexe /optimize+ `
    "/out:$uninstallExe" `
    "/win32icon:$icoPath" `
    /r:System.dll /r:System.Windows.Forms.dll `
    $uninstallCs
Remove-Item $uninstallCs -Force -ErrorAction SilentlyContinue

Write-Host "3/6 Empacotando payload Windows e compilando SpecAudit-Setup.exe..." -ForegroundColor Cyan
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $payloadZip -CompressionLevel Optimal

$setupCs = Join-Path $PSScriptRoot "dist\SetupProgram.cs"
$csCode = @'
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("Instalador do SpecAudit")]
[assembly: AssemblyDescription("Instalador oficial do SpecAudit - Auditoria Documental SpecKit")]
[assembly: AssemblyCompany("Carlos Wagner G. Bittencourt")]
[assembly: AssemblyProduct("SpecAudit")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Carlos Wagner G. Bittencourt")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace SpecAuditSetup
{
    public sealed class SetupForm : Form
    {
        private readonly CheckBox chkDesktop;
        private readonly CheckBox chkLaunch;
        private readonly ProgressBar progress;
        private readonly Label lblStatus;
        private readonly Button btnInstall;
        private readonly Button btnUninstall;
        private readonly Button btnClose;
        private readonly string targetDir;

        public SetupForm()
        {
            targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "SpecAudit");
            Text = "Instalador do SpecAudit v1.0 (Windows Desktop)";
            Size = new Size(560, 390);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(17, 21, 31);
            ForeColor = Color.FromArgb(243, 246, 251);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.FromArgb(23, 31, 46) };
            header.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(20, 16, 58, 58);
                using (var br = new LinearGradientBrush(r, Color.FromArgb(30, 64, 175), Color.FromArgb(124, 58, 237), 45f))
                using (var pen = new Pen(Color.FromArgb(147, 197, 253), 2f))
                {
                    e.Graphics.FillEllipse(br, r);
                    e.Graphics.DrawEllipse(pen, r);
                }
                using (var f = new Font("Segoe UI", 19f, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString("SA", f, Brushes.White, r, sf);
                }
                var badge = new Rectangle(58, 54, 22, 22);
                using (var bBr = new SolidBrush(Color.FromArgb(16, 185, 129)))
                using (var bPen = new Pen(Color.FromArgb(17, 21, 31), 2.5f))
                using (var bf = new Font("Segoe UI Symbol", 10f, FontStyle.Bold))
                {
                    e.Graphics.FillEllipse(bBr, badge);
                    e.Graphics.DrawEllipse(bPen, badge);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString("✓", bf, Brushes.White, badge, sf);
                }
            };

            var lblTitle = new Label
            {
                Text = "SpecAudit — Auditoria Documental SpecKit",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(92, 20),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Autor: Carlos Wagner G. Bittencourt · Versão 1.0 (Sem janela de prompt)",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(154, 167, 189),
                Location = new Point(94, 49),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            Controls.Add(header);

            var lblPathCaption = new Label
            {
                Text = "Pasta de instalação (não requer privilégios de Administrador):",
                ForeColor = Color.FromArgb(154, 167, 189),
                Location = new Point(24, 110),
                AutoSize = true
            };
            var txtPath = new TextBox
            {
                Text = targetDir,
                ReadOnly = true,
                BackColor = Color.FromArgb(23, 31, 46),
                ForeColor = Color.FromArgb(216, 222, 235),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(24, 134),
                Width = 496
            };

            chkDesktop = new CheckBox
            {
                Text = "Criar atalho com ícone SA na Área de Trabalho e no Menu Iniciar",
                Checked = true,
                Location = new Point(24, 174),
                AutoSize = true
            };
            chkLaunch = new CheckBox
            {
                Text = "Iniciar o SpecAudit em segundo plano (ícone SA perto do relógio) e abrir navegador",
                Checked = true,
                Location = new Point(24, 202),
                AutoSize = true
            };

            lblStatus = new Label
            {
                Text = "Pronto para instalar ou desinstalar.",
                ForeColor = Color.FromArgb(147, 197, 253),
                Location = new Point(24, 238),
                AutoSize = true
            };
            progress = new ProgressBar
            {
                Location = new Point(24, 262),
                Width = 496,
                Height = 18,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            btnUninstall = new Button
            {
                Text = "Desinstalar versão atual",
                BackColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 302),
                Size = new Size(165, 36),
                Cursor = Cursors.Hand
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += async (s, e) => await RunUninstallAsync();

            btnInstall = new Button
            {
                Text = "Instalar agora",
                BackColor = Color.FromArgb(79, 140, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(268, 302),
                Size = new Size(134, 36),
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += async (s, e) => await RunInstallAsync();

            btnClose = new Button
            {
                Text = "Cancelar",
                BackColor = Color.FromArgb(37, 46, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(412, 302),
                Size = new Size(108, 36),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblPathCaption, txtPath, chkDesktop, chkLaunch, lblStatus, progress, btnUninstall, btnInstall, btnClose });
        }

        private async Task RunUninstallAsync()
        {
            var confirm = MessageBox.Show(
                "Deseja desinstalar o SpecAudit desta máquina?\n\n(Seus projetos auditados e relatórios salvos não serão afetados.)",
                "Desinstalar SpecAudit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnInstall.Enabled = false;
            btnUninstall.Enabled = false;
            lblStatus.Text = "Removendo SpecAudit...";
            progress.Value = 40;

            await Task.Run(() =>
            {
                foreach (var procName in new[] { "SpecAudit-Launcher", "SpecAudit" })
                {
                    foreach (var p in Process.GetProcessesByName(procName))
                    {
                        try { p.Kill(); p.WaitForExit(3000); } catch { }
                    }
                }
                try
                {
                    var desktopLnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit.lnk");
                    if (File.Exists(desktopLnk)) File.Delete(desktopLnk);
                }
                catch { }
                try
                {
                    var startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "SpecAudit");
                    if (Directory.Exists(startMenuDir)) Directory.Delete(startMenuDir, true);
                }
                catch { }
                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\SpecAudit", false);
                }
                catch { }
                try
                {
                    if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
                }
                catch { }
            });

            progress.Value = 100;
            lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
            lblStatus.Text = "SpecAudit desinstalado com sucesso desta máquina!";
            btnClose.Text = "Fechar";
            btnInstall.Enabled = true;
        }

        private async Task RunInstallAsync()
        {
            btnInstall.Enabled = false;
            btnUninstall.Enabled = false;
            chkDesktop.Enabled = false;
            chkLaunch.Enabled = false;
            try
            {
                lblStatus.Text = "Encerrando instâncias anteriores do SpecAudit...";
                progress.Value = 15;
                await Task.Run(() =>
                {
                    foreach (var procName in new[] { "SpecAudit-Launcher", "SpecAudit" })
                    {
                        foreach (var p in Process.GetProcessesByName(procName))
                        {
                            try { p.Kill(); p.WaitForExit(3000); } catch { }
                        }
                    }
                });

                lblStatus.Text = "Extraindo arquivos do SpecAudit...";
                progress.Value = 40;
                await Task.Run(() =>
                {
                    if (Directory.Exists(targetDir))
                    {
                        foreach (var f in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
                        {
                            try { File.SetAttributes(f, FileAttributes.Normal); } catch { }
                        }
                    }
                    Directory.CreateDirectory(targetDir);
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
                    {
                        if (stream == null) throw new InvalidOperationException("Pacote interno payload.zip não encontrado.");
                        var tempZip = Path.Combine(Path.GetTempPath(), "specaudit-payload-" + Guid.NewGuid().ToString("N") + ".zip");
                        try
                        {
                            using (var fs = File.Create(tempZip)) stream.CopyTo(fs);
                            using (var archive = ZipFile.OpenRead(tempZip))
                            {
                                foreach (var entry in archive.Entries)
                                {
                                    var fullPath = Path.Combine(targetDir, entry.FullName);
                                    var dir = Path.GetDirectoryName(fullPath);
                                    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                                    if (!string.IsNullOrEmpty(entry.Name))
                                        entry.ExtractToFile(fullPath, overwrite: true);
                                }
                            }
                        }
                        finally
                        {
                            if (File.Exists(tempZip)) File.Delete(tempZip);
                        }
                    }
                });

                progress.Value = 80;
                var launcherPath = Path.Combine(targetDir, "SpecAudit-Launcher.exe");
                var uninstallPath = Path.Combine(targetDir, "SpecAudit-Uninstall.exe");
                var icoFile = Path.Combine(targetDir, "specaudit.ico");

                if (chkDesktop.Checked)
                {
                    lblStatus.Text = "Criando atalhos com ícone oficial (sem janela de prompt)...";
                    CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit.lnk"), launcherPath, targetDir, icoFile, "SpecAudit - Auditoria Documental SpecKit");
                    var startDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "SpecAudit");
                    Directory.CreateDirectory(startDir);
                    CreateShortcut(Path.Combine(startDir, "SpecAudit.lnk"), launcherPath, targetDir, icoFile, "SpecAudit - Auditoria Documental SpecKit");
                    CreateShortcut(Path.Combine(startDir, "Desinstalar SpecAudit.lnk"), uninstallPath, targetDir, icoFile, "Desinstalar o SpecAudit");
                }

                RegisterUninstaller(targetDir, launcherPath, uninstallPath);
                progress.Value = 100;
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                lblStatus.Text = "Instalação concluída com sucesso!";
                btnClose.Text = "Concluir";
                btnUninstall.Enabled = true;

                if (chkLaunch.Checked && File.Exists(launcherPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = launcherPath,
                        WorkingDirectory = targetDir,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "Erro: " + ex.Message;
                btnInstall.Enabled = true;
                btnUninstall.Enabled = true;
            }
        }

        private static void CreateShortcut(string lnkPath, string targetExe, string workDir, string icoFile, string description)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic sc = shell.CreateShortcut(lnkPath);
            sc.TargetPath = targetExe;
            sc.Arguments = "";
            sc.WorkingDirectory = workDir;
            sc.Description = description;
            sc.IconLocation = File.Exists(icoFile) ? icoFile : (targetExe + ",0");
            sc.Save();
        }

        private static void RegisterUninstaller(string installDir, string displayIconPath, string uninstallExePath)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\SpecAudit"))
                {
                    if (key == null) return;
                    key.SetValue("DisplayName", "SpecAudit — Auditoria Documental SpecKit");
                    key.SetValue("DisplayVersion", "1.0.0");
                    key.SetValue("Publisher", "Carlos Wagner G. Bittencourt");
                    key.SetValue("InstallLocation", installDir);
                    key.SetValue("DisplayIcon", displayIconPath);
                    key.SetValue("UninstallString", "\"" + uninstallExePath + "\"");
                    key.SetValue("QuietUninstallString", "\"" + uninstallExePath + "\" --quiet");
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }
}
'@
[System.IO.File]::WriteAllText($setupCs, $csCode, (New-Object System.Text.UTF8Encoding($true)))

& $csc /nologo /target:winexe /optimize+ `
    "/out:$setupExe" `
    "/win32icon:$icoPath" `
    "/resource:$payloadZip,payload.zip" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /r:Microsoft.CSharp.dll `
    $setupCs

Remove-Item $setupCs, $payloadZip -Force -ErrorAction SilentlyContinue
Unblock-File -Path $setupExe -ErrorAction SilentlyContinue

Write-Host "4/6 Publicando payload portatil Linux/Docker (sem codigo-fonte)..." -ForegroundColor Cyan
if (Test-Path $dockerPublishDir) { Remove-Item $dockerPublishDir -Recurse -Force }
$dockerAppDir = Join-Path $dockerPublishDir "app"
New-Item -ItemType Directory -Force -Path $dockerAppDir | Out-Null

dotnet publish (Join-Path $repoRoot "SpecAudit\SpecAudit.csproj") `
    -c Release `
    --self-contained false `
    -o $dockerAppDir `
    --nologo

Copy-Item $icoPath (Join-Path $dockerPublishDir "specaudit.ico") -Force

$runtimeDockerfile = @'
FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y --no-install-recommends python3 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY app/ .
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://0.0.0.0:5187
EXPOSE 5187
ENTRYPOINT ["dotnet", "SpecAudit.dll"]
'@
[System.IO.File]::WriteAllText((Join-Path $dockerPublishDir "Dockerfile"), $runtimeDockerfile, (New-Object System.Text.UTF8Encoding($false)))

if (Test-Path $dockerPayloadZip) { Remove-Item $dockerPayloadZip -Force }
Compress-Archive -Path (Join-Path $dockerPublishDir "*") -DestinationPath $dockerPayloadZip -CompressionLevel Optimal

Write-Host "5/6 Compilando instalador unico para Docker (SpecAudit-Docker-Setup.exe)..." -ForegroundColor Cyan
$dockerSetupCs = Join-Path $PSScriptRoot "dist\DockerSetupProgram.cs"
$dockerCsCode = @'
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: AssemblyTitle("Instalador Docker do SpecAudit")]
[assembly: AssemblyDescription("Instalador único do SpecAudit para Docker Desktop")]
[assembly: AssemblyCompany("Carlos Wagner G. Bittencourt")]
[assembly: AssemblyProduct("SpecAudit Docker")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Carlos Wagner G. Bittencourt")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace SpecAuditDockerSetup
{
    public sealed class DockerSetupForm : Form
    {
        private readonly TextBox txtProjetos;
        private readonly TextBox txtRelatorios;
        private readonly CheckBox chkShortcut;
        private readonly CheckBox chkOpenBrowser;
        private readonly ProgressBar progress;
        private readonly Label lblStatus;
        private readonly Button btnInstall;
        private readonly Button btnRemove;
        private readonly Button btnClose;
        private readonly string workDir;

        public DockerSetupForm()
        {
            workDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "SpecAudit-Docker");
            Text = "Instalador Docker do SpecAudit v1.0";
            Size = new Size(600, 455);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(17, 21, 31);
            ForeColor = Color.FromArgb(243, 246, 251);
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            var header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Color.FromArgb(23, 31, 46) };
            header.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(20, 16, 58, 58);
                using (var br = new LinearGradientBrush(r, Color.FromArgb(30, 64, 175), Color.FromArgb(124, 58, 237), 45f))
                using (var pen = new Pen(Color.FromArgb(147, 197, 253), 2f))
                {
                    e.Graphics.FillEllipse(br, r);
                    e.Graphics.DrawEllipse(pen, r);
                }
                using (var f = new Font("Segoe UI", 19f, FontStyle.Bold))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString("SA", f, Brushes.White, r, sf);
                }
                var badge = new Rectangle(58, 54, 22, 22);
                using (var bBr = new SolidBrush(Color.FromArgb(16, 185, 129)))
                using (var bPen = new Pen(Color.FromArgb(17, 21, 31), 2.5f))
                using (var bf = new Font("Segoe UI Symbol", 10f, FontStyle.Bold))
                {
                    e.Graphics.FillEllipse(bBr, badge);
                    e.Graphics.DrawEllipse(bPen, badge);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString("✓", bf, Brushes.White, badge, sf);
                }
            };

            var lblTitle = new Label
            {
                Text = "SpecAudit — Instalador Único para Docker",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(92, 20),
                AutoSize = true
            };
            var lblSub = new Label
            {
                Text = "Autor: Carlos Wagner G. Bittencourt · Container Linux em 2º plano (Porta 5187)",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(154, 167, 189),
                Location = new Point(94, 49),
                AutoSize = true
            };
            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            Controls.Add(header);

            string defaultProjects = Directory.Exists(@"D:\Codex_Workspace\Projetos")
                ? @"D:\Codex_Workspace\Projetos"
                : @"C:\Projetos";
            string defaultReports = Directory.Exists(@"D:\Codex_Workspace")
                ? @"D:\Codex_Workspace\Relatorios-SpecAudit"
                : @"C:\Projetos\Relatorios-SpecAudit";

            var lblProj = new Label
            {
                Text = "Pasta de Projetos no Windows (mapeada como /projetos em SOMENTE LEITURA):",
                ForeColor = Color.FromArgb(154, 167, 189),
                Location = new Point(24, 108),
                AutoSize = true
            };
            txtProjetos = new TextBox
            {
                Text = defaultProjects,
                BackColor = Color.FromArgb(23, 31, 46),
                ForeColor = Color.FromArgb(216, 222, 235),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(24, 130),
                Width = 436
            };
            var btnBrowseProj = new Button
            {
                Text = "Procurar...",
                BackColor = Color.FromArgb(37, 46, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(468, 128),
                Size = new Size(92, 27)
            };
            btnBrowseProj.FlatAppearance.BorderSize = 0;
            btnBrowseProj.Click += (s, e) => BrowseFolder(txtProjetos);

            var lblRep = new Label
            {
                Text = "Pasta de Relatórios no Windows (mapeada como /relatorios para salvar relatórios):",
                ForeColor = Color.FromArgb(154, 167, 189),
                Location = new Point(24, 168),
                AutoSize = true
            };
            txtRelatorios = new TextBox
            {
                Text = defaultReports,
                BackColor = Color.FromArgb(23, 31, 46),
                ForeColor = Color.FromArgb(216, 222, 235),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(24, 190),
                Width = 436
            };
            var btnBrowseRep = new Button
            {
                Text = "Procurar...",
                BackColor = Color.FromArgb(37, 46, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(468, 188),
                Size = new Size(92, 27)
            };
            btnBrowseRep.FlatAppearance.BorderSize = 0;
            btnBrowseRep.Click += (s, e) => BrowseFolder(txtRelatorios);

            chkShortcut = new CheckBox
            {
                Text = "Criar atalho SpecAudit (Docker) com ícone SA na Área de Trabalho",
                Checked = true,
                Location = new Point(24, 230),
                AutoSize = true
            };
            chkOpenBrowser = new CheckBox
            {
                Text = "Abrir http://127.0.0.1:5187 no navegador ao concluir",
                Checked = true,
                Location = new Point(24, 256),
                AutoSize = true
            };

            lblStatus = new Label
            {
                Text = "Pronto para instalar o container SpecAudit no Docker.",
                ForeColor = Color.FromArgb(147, 197, 253),
                Location = new Point(24, 294),
                AutoSize = true
            };
            progress = new ProgressBar
            {
                Location = new Point(24, 318),
                Width = 536,
                Height = 18,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            btnRemove = new Button
            {
                Text = "Desinstalar do Docker",
                BackColor = Color.FromArgb(127, 29, 29),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(24, 358),
                Size = new Size(165, 36),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += async (s, e) => await RunRemoveDockerAsync();

            btnInstall = new Button
            {
                Text = "Instalar no Docker",
                BackColor = Color.FromArgb(79, 140, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(296, 358),
                Size = new Size(154, 36),
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += async (s, e) => await RunInstallDockerAsync();

            btnClose = new Button
            {
                Text = "Cancelar",
                BackColor = Color.FromArgb(37, 46, 64),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(460, 358),
                Size = new Size(100, 36),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[]
            {
                lblProj, txtProjetos, btnBrowseProj,
                lblRep, txtRelatorios, btnBrowseRep,
                chkShortcut, chkOpenBrowser,
                lblStatus, progress,
                btnRemove, btnInstall, btnClose
            });
        }

        private static void BrowseFolder(TextBox targetBox)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = Directory.Exists(targetBox.Text) ? targetBox.Text : "";
                if (dlg.ShowDialog() == DialogResult.OK)
                    targetBox.Text = dlg.SelectedPath;
            }
        }

        private async Task RunRemoveDockerAsync()
        {
            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
            lblStatus.Text = "Parando e removendo container specaudit do Docker...";
            progress.Value = 40;

            await Task.Run(() =>
            {
                RunHiddenProcess("docker", "rm -f specaudit");
                RunHiddenProcess("docker", "rmi specaudit-docker-specaudit specaudit-specaudit");
                try
                {
                    var desktopLnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit (Docker).lnk");
                    if (File.Exists(desktopLnk)) File.Delete(desktopLnk);
                }
                catch { }
            });

            progress.Value = 100;
            lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
            lblStatus.Text = "Container SpecAudit removido do Docker com sucesso!";
            btnClose.Text = "Fechar";
            btnInstall.Enabled = true;
            btnRemove.Enabled = true;
        }

        private async Task RunInstallDockerAsync()
        {
            string projDir = txtProjetos.Text.Trim();
            string repDir = txtRelatorios.Text.Trim();

            btnInstall.Enabled = false;
            btnRemove.Enabled = false;
            try
            {
                Directory.CreateDirectory(projDir);
                Directory.CreateDirectory(repDir);

                lblStatus.Text = "Verificando se o Docker Desktop está em execução...";
                progress.Value = 15;
                int dockerCheck = await Task.Run(() => RunHiddenProcess("docker", "info"));
                if (dockerCheck != 0)
                {
                    throw new InvalidOperationException("O Docker Desktop não está em execução ou não foi encontrado nesta máquina.");
                }

                lblStatus.Text = "Extraindo pacote pré-compilado do SpecAudit (sem código-fonte)...";
                progress.Value = 35;
                await Task.Run(() =>
                {
                    if (Directory.Exists(workDir)) Directory.Delete(workDir, true);
                    Directory.CreateDirectory(workDir);

                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("docker-payload.zip"))
                    {
                        if (stream == null) throw new InvalidOperationException("Pacote interno docker-payload.zip não encontrado.");
                        var tempZip = Path.Combine(Path.GetTempPath(), "specaudit-docker-" + Guid.NewGuid().ToString("N") + ".zip");
                        try
                        {
                            using (var fs = File.Create(tempZip)) stream.CopyTo(fs);
                            ZipFile.ExtractToDirectory(tempZip, workDir);
                        }
                        finally
                        {
                            if (File.Exists(tempZip)) File.Delete(tempZip);
                        }
                    }

                    string projNormalized = projDir.Replace("\\", "/");
                    string repNormalized = repDir.Replace("\\", "/");
                    string composeYaml =
"services:\n" +
"  specaudit:\n" +
"    build:\n" +
"      context: .\n" +
"      dockerfile: Dockerfile\n" +
"    container_name: specaudit\n" +
"    ports:\n" +
"      - \"127.0.0.1:5187:5187\"\n" +
"    environment:\n" +
"      - DOTNET_RUNNING_IN_CONTAINER=true\n" +
"      - ASPNETCORE_URLS=http://0.0.0.0:5187\n" +
"    volumes:\n" +
"      - \"" + projNormalized + ":/projetos:ro\"\n" +
"      - \"" + repNormalized + ":/relatorios:rw\"\n" +
"      - specaudit_appdata:/root/.local/share/SpecAudit\n" +
"    restart: unless-stopped\n\n" +
"volumes:\n" +
"  specaudit_appdata:\n";
                    File.WriteAllText(Path.Combine(workDir, "docker-compose.yml"), composeYaml, new UTF8Encoding(false));
                });

                lblStatus.Text = "Construindo imagem e iniciando container SpecAudit no Docker...";
                progress.Value = 70;
                int upCode = await Task.Run(() => RunHiddenProcess("docker", "compose up -d --build", workDir));
                if (upCode != 0)
                {
                    throw new InvalidOperationException("Falha ao subir o container com 'docker compose up -d --build'.");
                }

                if (chkShortcut.Checked)
                {
                    var icoFile = Path.Combine(workDir, "specaudit.ico");
                    var lnkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit (Docker).lnk");
                    CreateUrlShortcut(lnkPath, "http://127.0.0.1:5187", icoFile);
                }

                progress.Value = 100;
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                lblStatus.Text = "Container SpecAudit ativo no Docker em http://127.0.0.1:5187 !";
                btnClose.Text = "Concluir";
                btnRemove.Enabled = true;

                if (chkOpenBrowser.Checked)
                {
                    Process.Start(new ProcessStartInfo { FileName = "http://127.0.0.1:5187", UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblStatus.Text = "Erro: " + ex.Message;
                btnInstall.Enabled = true;
                btnRemove.Enabled = true;
            }
        }

        private static int RunHiddenProcess(string fileName, string arguments, string workingDir = null)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                if (!string.IsNullOrEmpty(workingDir)) psi.WorkingDirectory = workingDir;
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode;
                }
            }
            catch
            {
                return -1;
            }
        }

        private static void CreateUrlShortcut(string lnkPath, string url, string icoFile)
        {
            try
            {
                var shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic sc = shell.CreateShortcut(lnkPath);
                sc.TargetPath = "explorer.exe";
                sc.Arguments = url;
                sc.Description = "SpecAudit (Container Docker em http://127.0.0.1:5187)";
                if (File.Exists(icoFile)) sc.IconLocation = icoFile;
                sc.Save();
            }
            catch { }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DockerSetupForm());
        }
    }
}
'@
[System.IO.File]::WriteAllText($dockerSetupCs, $dockerCsCode, (New-Object System.Text.UTF8Encoding($true)))

& $csc /nologo /target:winexe /optimize+ `
    "/out:$dockerSetupExe" `
    "/win32icon:$icoPath" `
    "/resource:$dockerPayloadZip,docker-payload.zip" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /r:Microsoft.CSharp.dll `
    $dockerSetupCs

Remove-Item $dockerSetupCs, $dockerPayloadZip -Force -ErrorAction SilentlyContinue
Remove-Item $dockerPublishDir -Recurse -Force -ErrorAction SilentlyContinue
Unblock-File -Path $dockerSetupExe -ErrorAction SilentlyContinue

Write-Host "6/6 Atualizando instalacao local existente em %LOCALAPPDATA%\Programs\SpecAudit..." -ForegroundColor Cyan
$installedDir = Join-Path $env:LOCALAPPDATA "Programs\SpecAudit"
if ((Test-Path $installedDir) -or $InstallLocally) {
    Stop-Process -Name "SpecAudit-Launcher", "SpecAudit" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 600

    New-Item -ItemType Directory -Force -Path $installedDir | Out-Null
    Copy-Item (Join-Path $publishDir "*") $installedDir -Recurse -Force

    $regPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\SpecAudit"
    if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null }
    $instLauncher = Join-Path $installedDir "SpecAudit-Launcher.exe"
    $instUninstall = Join-Path $installedDir "SpecAudit-Uninstall.exe"
    $instIco = Join-Path $installedDir "specaudit.ico"
    Set-ItemProperty -Path $regPath -Name "DisplayName" -Value "SpecAudit — Auditoria Documental SpecKit"
    Set-ItemProperty -Path $regPath -Name "DisplayVersion" -Value "1.0.0"
    Set-ItemProperty -Path $regPath -Name "Publisher" -Value "Carlos Wagner G. Bittencourt"
    Set-ItemProperty -Path $regPath -Name "InstallLocation" -Value $installedDir
    Set-ItemProperty -Path $regPath -Name "DisplayIcon" -Value $instLauncher
    Set-ItemProperty -Path $regPath -Name "UninstallString" -Value "`"$instUninstall`""
    Set-ItemProperty -Path $regPath -Name "QuietUninstallString" -Value "`"$instUninstall`" --quiet"
    Set-ItemProperty -Path $regPath -Name "NoModify" -Value 1 -Type DWord
    Set-ItemProperty -Path $regPath -Name "NoRepair" -Value 1 -Type DWord

    $wsh = New-Object -ComObject WScript.Shell
    $desktopLnk = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "SpecAudit.lnk"
    if (Test-Path $desktopLnk) {
        $sc = $wsh.CreateShortcut($desktopLnk)
        $sc.TargetPath = $instLauncher
        $sc.Arguments = ""
        $sc.WorkingDirectory = $installedDir
        $sc.Description = "SpecAudit - Auditoria Documental SpecKit"
        $sc.IconLocation = $instIco
        $sc.Save()
    }
    $startDir = Join-Path ([Environment]::GetFolderPath("Programs")) "SpecAudit"
    if (Test-Path $startDir) {
        $sc2 = $wsh.CreateShortcut((Join-Path $startDir "SpecAudit.lnk"))
        $sc2.TargetPath = $instLauncher
        $sc2.Arguments = ""
        $sc2.WorkingDirectory = $installedDir
        $sc2.Description = "SpecAudit - Auditoria Documental SpecKit"
        $sc2.IconLocation = $instIco
        $sc2.Save()

        $sc3 = $wsh.CreateShortcut((Join-Path $startDir "Desinstalar SpecAudit.lnk"))
        $sc3.TargetPath = $instUninstall
        $sc3.Arguments = ""
        $sc3.WorkingDirectory = $installedDir
        $sc3.Description = "Desinstalar o SpecAudit"
        $sc3.IconLocation = $instIco
        $sc3.Save()
    }

    Start-Process -FilePath $instLauncher -ArgumentList "--no-browser" -WorkingDirectory $installedDir
    Write-Host "Instalacao local em $installedDir atualizada com o novo icone interno SA e reiniciada em segundo plano!" -ForegroundColor Green
}

Get-Item $setupExe, $dockerSetupExe | Select-Object Name, Length, FullName

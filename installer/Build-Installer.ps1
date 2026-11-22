param(
    [switch]$InstallLocally
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $PSScriptRoot "dist\SpecAudit-win-x64"
$payloadZip = Join-Path $PSScriptRoot "dist\payload.zip"
$setupExe = Join-Path $PSScriptRoot "dist\SpecAudit-Setup.exe"
$icoPath = Join-Path $repoRoot "SpecAudit\wwwroot\specaudit.ico"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "1/5 Publicando SpecAudit (win-x64, Self-Contained, Release com icone oficial)..." -ForegroundColor Cyan
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

Write-Host "2/5 Compilando SpecAudit-Launcher.exe (execucao invisivel em segundo plano + icone na bandeja do relogio) e SpecAudit-Uninstall.exe..." -ForegroundColor Cyan

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
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (IsPortListening(5187))
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = "http://127.0.0.1:5187", UseShellExecute = true });
                }
                catch { }
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

Write-Host "3/5 Empacotando payload comprimido..." -ForegroundColor Cyan
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $payloadZip -CompressionLevel Optimal

Write-Host "4/5 Compilando instalador grafico Windows (SpecAudit-Setup.exe) com logo, bandeja invisivel e desinstalador registrado..." -ForegroundColor Cyan
$setupCs = Join-Path $PSScriptRoot "dist\SetupProgram.cs"
$csCode = @'
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private readonly Button btnClose;
        private readonly string targetDir;

        public SetupForm()
        {
            targetDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "SpecAudit");
            Text = "Instalador do SpecAudit v1.0";
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
                Text = "Pronto para instalar.",
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

            Controls.AddRange(new Control[] { lblPathCaption, txtPath, chkDesktop, chkLaunch, lblStatus, progress, btnInstall, btnClose });
        }

        private async Task RunInstallAsync()
        {
            btnInstall.Enabled = false;
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

Write-Host "5/5 Limpando Zone.Identifier e atualizando instalacao local existente..." -ForegroundColor Cyan
Unblock-File -Path $setupExe -ErrorAction SilentlyContinue

$installedDir = Join-Path $env:LOCALAPPDATA "Programs\SpecAudit"
if ((Test-Path $installedDir) -or $InstallLocally) {
    New-Item -ItemType Directory -Force -Path $installedDir | Out-Null
    Copy-Item $launcherExe (Join-Path $installedDir "SpecAudit-Launcher.exe") -Force
    Copy-Item $uninstallExe (Join-Path $installedDir "SpecAudit-Uninstall.exe") -Force
    Copy-Item $icoPath (Join-Path $installedDir "specaudit.ico") -Force

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
    Write-Host "Instalacao local em $installedDir atualizada com SpecAudit-Launcher.exe e botao Desinstalar habilitado no Windows!" -ForegroundColor Green
}

Get-Item $setupExe | Select-Object Name, Length, FullName
Write-Host "Instalador grafico pronto em: $setupExe" -ForegroundColor Green

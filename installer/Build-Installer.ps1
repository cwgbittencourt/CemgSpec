param(
    [switch]$InstallLocally
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $PSScriptRoot "dist\SpecAudit-win-x64"
$payloadZip = Join-Path $PSScriptRoot "dist\payload.zip"
$setupExe = Join-Path $PSScriptRoot "dist\SpecAudit-Setup.exe"
$icoPath = Join-Path $repoRoot "SpecAudit\wwwroot\specaudit.ico"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "1/4 Publicando SpecAudit (win-x64, Self-Contained, Release com icone oficial)..." -ForegroundColor Cyan
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

Write-Host "2/4 Empacotando payload comprimido..." -ForegroundColor Cyan
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $payloadZip -CompressionLevel Optimal

Write-Host "3/4 Compilando instalador grafico Windows (SpecAudit-Setup.exe) com logo e icone..." -ForegroundColor Cyan
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
                Text = "Autor: Carlos Wagner G. Bittencourt · Versão 1.0 (win-x64)",
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
                Text = "Criar atalho com ícone do SpecAudit na Área de Trabalho e no Menu Iniciar",
                Checked = true,
                Location = new Point(24, 174),
                AutoSize = true
            };
            chkLaunch = new CheckBox
            {
                Text = "Iniciar o SpecAudit no navegador ao concluir a instalação",
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
                    foreach (var p in Process.GetProcessesByName("SpecAudit"))
                    {
                        try { p.Kill(); p.WaitForExit(3000); } catch { }
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
                var exePath = Path.Combine(targetDir, "SpecAudit.exe");
                var icoFile = Path.Combine(targetDir, "specaudit.ico");

                if (chkDesktop.Checked)
                {
                    lblStatus.Text = "Criando atalhos com ícone oficial...";
                    CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SpecAudit.lnk"), exePath, targetDir, icoFile);
                    var startDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "SpecAudit");
                    Directory.CreateDirectory(startDir);
                    CreateShortcut(Path.Combine(startDir, "SpecAudit.lnk"), exePath, targetDir, icoFile);
                }

                RegisterUninstaller(targetDir, exePath);
                progress.Value = 100;
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                lblStatus.Text = "Instalação concluída com sucesso!";
                btnClose.Text = "Concluir";

                if (chkLaunch.Checked && File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = "--open-browser",
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

        private static void CreateShortcut(string lnkPath, string targetExe, string workDir, string icoFile)
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic sc = shell.CreateShortcut(lnkPath);
            sc.TargetPath = targetExe;
            sc.Arguments = "--open-browser";
            sc.WorkingDirectory = workDir;
            sc.Description = "SpecAudit - Auditoria Documental SpecKit";
            sc.IconLocation = File.Exists(icoFile) ? icoFile : (targetExe + ",0");
            sc.Save();
        }

        private static void RegisterUninstaller(string installDir, string exePath)
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
                    key.SetValue("DisplayIcon", exePath);
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

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
& $csc /nologo /target:winexe /optimize+ `
    "/out:$setupExe" `
    "/win32icon:$icoPath" `
    "/resource:$payloadZip,payload.zip" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.IO.Compression.dll /r:System.IO.Compression.FileSystem.dll /r:Microsoft.CSharp.dll `
    $setupCs

Remove-Item $setupCs, $payloadZip -Force -ErrorAction SilentlyContinue

Write-Host "4/4 Limpando Zone.Identifier (Mark-of-the-Web) para evitar falso alerta local..." -ForegroundColor Cyan
Unblock-File -Path $setupExe -ErrorAction SilentlyContinue

Get-Item $setupExe | Select-Object Name, Length, FullName
Write-Host "Instalador grafico pronto em: $setupExe" -ForegroundColor Green

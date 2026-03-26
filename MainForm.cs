using System.Drawing;
using System.Windows.Forms;
using CoworkingAgendamento.Data;

namespace CoworkingAgendamento.Forms;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Sistema de Agendamentos - Coworking";
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 700);
        BackColor = Color.FromArgb(240, 244, 248);

        BuildMenu();
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip
        {
            BackColor = Color.FromArgb(30, 80, 160),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            Dock = DockStyle.Top
        };

        var mnuSalas = new ToolStripMenuItem("Salas") { ForeColor = Color.White };
        mnuSalas.DropDownItems.Add(
            "Gerenciar Salas",
            null,
            (_, _) => AbrirFilho<SalaForm>());

        var mnuAgendamentos = new ToolStripMenuItem("Agendamentos") { ForeColor = Color.White };
        mnuAgendamentos.DropDownItems.Add(
            "Gerenciar Agendamentos",
            null,
            (_, _) => AbrirFilho<AgendamentoForm>());

        menu.Items.Add(mnuSalas);
        menu.Items.Add(mnuAgendamentos);

        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    private void AbrirFilho<TForm>() where TForm : Form, new()
    {
        foreach (Form child in MdiChildren)
        {
            if (child is TForm)
            {
                child.BringToFront();
                child.Focus();
                return;
            }
        }

        var form = new TForm
        {
            MdiParent = this
        };

        form.Show();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

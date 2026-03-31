using System.Drawing;
using System.Windows.Forms;

namespace CoworkingAgendamento.Forms;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Sistema de Agendamentos - Coworking";
        IsMdiContainer = true;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 700);
        BackColor = UiTheme.PageBackground;
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildMenu();
        BuildHeader();
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip
        {
            BackColor = UiTheme.BrandDark,
            ForeColor = Color.White,
            Font = UiTheme.Font(10f, FontStyle.Bold),
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

    private void BuildHeader()
    {
        var painel = UiTheme.CreateCardPanel(96);
        painel.Padding = new Padding(24, 18, 24, 18);

        var lblTitulo = new Label
        {
            Text = "Painel de agendamentos",
            AutoSize = true,
            Font = UiTheme.Font(18f, FontStyle.Bold),
            ForeColor = UiTheme.BrandDark,
            Location = new Point(24, 18)
        };

        var lblDescricao = new Label
        {
            Text = "Abra as telas de salas e agendamentos pelo menu superior para gerenciar o coworking.",
            AutoSize = true,
            Font = UiTheme.Font(10f),
            ForeColor = UiTheme.TextMuted,
            Location = new Point(24, 52)
        };

        painel.Controls.Add(lblTitulo);
        painel.Controls.Add(lblDescricao);
        Controls.Add(painel);
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

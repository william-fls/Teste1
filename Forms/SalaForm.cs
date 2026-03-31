using System.Drawing;
using System.Windows.Forms;
using CoworkingAgendamento.Models;
using CoworkingAgendamento.Repositories;

namespace CoworkingAgendamento.Forms;

public class SalaForm : Form
{
    public static event Action? SalasAtualizadas;

    private readonly SalaRepository _repo = new();

    private DataGridView _grid = null!;
    private TextBox _txtNome = null!;
    private Button _btnNovo = null!;
    private Button _btnSalvar = null!;
    private Button _btnExcluir = null!;
    private Label _lblStatus = null!;

    private int _idSelecionado;
    private bool _carregando;

    public SalaForm()
    {
        Text = "Cadastro de Salas";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 520);
        MinimumSize = new Size(760, 520);
        BackColor = UiTheme.PageBackground;
        AutoScaleMode = AutoScaleMode.Dpi;

        BuildUi();
        CarregarDadosIniciais();
    }

    private void BuildUi()
    {
        var painelTopo = UiTheme.CreateCardPanel(154);
        painelTopo.Padding = new Padding(18);

        var lblTitulo = UiTheme.CreateTitleLabel("Gerencie as salas de reuniao", new Point(18, 18));
        var lblNome = UiTheme.CreateFieldLabel("Nome da sala", new Point(18, 60));
        _txtNome = UiTheme.CreateTextBox(new Point(18, 84), 320, 100);

        _btnNovo = UiTheme.CreateButton("Novo", UiTheme.Neutral, new Point(360, 82));
        _btnSalvar = UiTheme.CreateButton("Salvar", UiTheme.Success, new Point(478, 82));
        _btnExcluir = UiTheme.CreateButton("Excluir", UiTheme.Danger, new Point(596, 82));

        _btnNovo.Click += (_, _) => LimparFormulario();
        _btnSalvar.Click += BtnSalvar_Click;
        _btnExcluir.Click += BtnExcluir_Click;

        _lblStatus = UiTheme.CreateStatusLabel("Cadastre quantas salas precisar.", new Point(18, 122));

        painelTopo.Controls.Add(lblTitulo);
        painelTopo.Controls.Add(lblNome);
        painelTopo.Controls.Add(_txtNome);
        painelTopo.Controls.Add(_btnNovo);
        painelTopo.Controls.Add(_btnSalvar);
        painelTopo.Controls.Add(_btnExcluir);
        painelTopo.Controls.Add(_lblStatus);

        _grid = UiTheme.CreateGrid();

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Sala.Id),
            DataPropertyName = nameof(Sala.Id),
            HeaderText = "ID",
            FillWeight = 20
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Sala.Nome),
            DataPropertyName = nameof(Sala.Nome),
            HeaderText = "Nome",
            FillWeight = 80
        });

        _grid.SelectionChanged += Grid_SelectionChanged;

        Controls.Add(UiTheme.CreateSpacer(12));
        Controls.Add(_grid);
        Controls.Add(painelTopo);
    }

    private void CarregarDadosIniciais()
    {
        try
        {
            CarregarGrid();
            LimparFormulario();
        }
        catch (Exception ex)
        {
            UiFeedback.ShowLoadError("as salas", ex);
        }
    }

    private void CarregarGrid()
    {
        _carregando = true;
        try
        {
            _grid.DataSource = null;
            _grid.DataSource = _repo.ListarTodas();
            DesfixarSelecaoGrid();
        }
        finally
        {
            _carregando = false;
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_carregando)
            return;

        if (_grid.CurrentRow?.DataBoundItem is not Sala sala)
            return;

        _idSelecionado = sala.Id;
        _txtNome.Text = sala.Nome;
        _lblStatus.Text = $"Editando a sala ID {_idSelecionado}.";
        AtualizarStatus();
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        UiFeedback.Execute(() =>
        {
            var sala = new Sala { Id = _idSelecionado, Nome = _txtNome.Text.Trim() };
            if (_idSelecionado == 0)
                _repo.Inserir(sala);
            else
                _repo.Atualizar(sala);
            CarregarGrid();
            LimparFormulario();
            NotificarSalasAtualizadas();
        });
    }

    private void BtnExcluir_Click(object? sender, EventArgs e)
    {
        if (_idSelecionado == 0)
        {
            UiFeedback.ShowInfo("Selecione uma sala para excluir.", "Exclusao de sala");
            return;
        }

        if (!UiFeedback.Confirm("Deseja excluir a sala selecionada?", "Exclusao de sala"))
            return;

        UiFeedback.Execute(() =>
        {
            _repo.Excluir(_idSelecionado);
            CarregarGrid();
            LimparFormulario();
            NotificarSalasAtualizadas();
        });
    }

    private void LimparFormulario()
    {
        _carregando = true;
        try
        {
            _idSelecionado = 0;
            _txtNome.Clear();
            DesfixarSelecaoGrid();
            _txtNome.Focus();
            AtualizarStatus();
        }
        finally
        {
            _carregando = false;
        }
    }

    private void DesfixarSelecaoGrid()
    {
        _grid.ClearSelection();
        _grid.CurrentCell = null;
    }

    private void AtualizarStatus()
    {
        _btnExcluir.Enabled = _idSelecionado != 0;
        _lblStatus.Text = _idSelecionado != 0
            ? $"Editando a sala ID {_idSelecionado}."
            : $"{_grid.Rows.Count} sala(s) cadastrada(s).";
    }

    private static void NotificarSalasAtualizadas() =>
        SalasAtualizadas?.Invoke();
}

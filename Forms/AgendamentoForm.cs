using System.Drawing;
using System.Windows.Forms;
using CoworkingAgendamento.Models;
using CoworkingAgendamento.Repositories;

namespace CoworkingAgendamento.Forms;

public class AgendamentoForm : Form
{
    private readonly AgendamentoRepository _repo = new();
    private readonly SalaRepository _salaRepo = new();

    private DataGridView _grid = null!;
    private ComboBox _cmbSala = null!;
    private DateTimePicker _dtpInicio = null!;
    private DateTimePicker _dtpFim = null!;
    private Button _btnNovo = null!;
    private Button _btnSalvar = null!;
    private Button _btnExcluir = null!;
    private Label _lblStatus = null!;

    private int _idSelecionado;
    private bool _carregando;

    public AgendamentoForm()
    {
        Text = "Cadastro de Agendamentos";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(940, 580);
        MinimumSize = new Size(940, 580);
        BackColor = UiTheme.PageBackground;
        AutoScaleMode = AutoScaleMode.Dpi;

        SalaForm.SalasAtualizadas += SincronizarComSalas;
        FormClosed += (_, _) => SalaForm.SalasAtualizadas -= SincronizarComSalas;

        BuildUi();
        CarregarDadosIniciais();
    }

    private void BuildUi()
    {
        var painelTopo = UiTheme.CreateCardPanel(190);
        painelTopo.Padding = new Padding(18);

        var lblTitulo = UiTheme.CreateTitleLabel("Gerencie os agendamentos das salas", new Point(18, 18));

        AddLabel(painelTopo, "Sala", new Point(18, 64));
        _cmbSala = UiTheme.CreateComboBox(new Point(18, 88), 250);

        AddLabel(painelTopo, "Data/Hora inicio", new Point(290, 64));
        _dtpInicio = UiTheme.CreateDateTimePicker(new Point(290, 88), 200, DateTime.Now);

        AddLabel(painelTopo, "Data/Hora fim", new Point(512, 64));
        _dtpFim = UiTheme.CreateDateTimePicker(new Point(512, 88), 200, DateTime.Now.AddHours(1));

        _btnNovo = UiTheme.CreateButton("Novo", UiTheme.Neutral, new Point(18, 132));
        _btnSalvar = UiTheme.CreateButton("Salvar", UiTheme.Success, new Point(136, 132));
        _btnExcluir = UiTheme.CreateButton("Excluir", UiTheme.Danger, new Point(254, 132));

        _btnNovo.Click += (_, _) => LimparFormulario();
        _btnSalvar.Click += BtnSalvar_Click;
        _btnExcluir.Click += BtnExcluir_Click;

        _lblStatus = UiTheme.CreateStatusLabel("Cadastre uma sala para iniciar os agendamentos.", new Point(390, 142));

        painelTopo.Controls.Add(lblTitulo);
        painelTopo.Controls.Add(_cmbSala);
        painelTopo.Controls.Add(_dtpInicio);
        painelTopo.Controls.Add(_dtpFim);
        painelTopo.Controls.Add(_btnNovo);
        painelTopo.Controls.Add(_btnSalvar);
        painelTopo.Controls.Add(_btnExcluir);
        painelTopo.Controls.Add(_lblStatus);

        _grid = UiTheme.CreateGrid();

        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Agendamento.Id),
            DataPropertyName = nameof(Agendamento.Id),
            Visible = false
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Agendamento.SalaId),
            DataPropertyName = nameof(Agendamento.SalaId),
            Visible = false
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Agendamento.SalaNome),
            DataPropertyName = nameof(Agendamento.SalaNome),
            HeaderText = "Sala",
            FillWeight = 25
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Agendamento.Inicio),
            DataPropertyName = nameof(Agendamento.Inicio),
            HeaderText = "Inicio",
            FillWeight = 20,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
        });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(Agendamento.Fim),
            DataPropertyName = nameof(Agendamento.Fim),
            HeaderText = "Fim",
            FillWeight = 20,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" }
        });

        _grid.SelectionChanged += Grid_SelectionChanged;

        Controls.Add(UiTheme.CreateSpacer(12));
        Controls.Add(_grid);
        Controls.Add(painelTopo);
    }

    private static void AddLabel(Control parent, string text, Point location)
    {
        parent.Controls.Add(UiTheme.CreateFieldLabel(text, location));
    }

    private void CarregarDadosIniciais()
    {
        try
        {
            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        }
        catch (Exception ex)
        {
            UiFeedback.ShowLoadError("os agendamentos", ex);
        }
    }

    private void CarregarSalas()
    {
        var salaSelecionadaAnterior = _cmbSala.SelectedValue;
        var salas = _salaRepo.ListarTodas();
        _cmbSala.DataSource = null;
        _cmbSala.DataSource = salas;
        _cmbSala.DisplayMember = nameof(Sala.Nome);
        _cmbSala.ValueMember = nameof(Sala.Id);

        if (_idSelecionado != 0 && salaSelecionadaAnterior is not null)
            _cmbSala.SelectedValue = salaSelecionadaAnterior;
        else
            _cmbSala.SelectedIndex = -1;
    }

    private void CarregarGrid()
    {
        _carregando = true;
        try
        {
            _grid.DataSource = null;
            _grid.DataSource = _repo.ListarTodos();
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

        if (_grid.CurrentRow?.DataBoundItem is not Agendamento agendamento)
            return;

        _idSelecionado = agendamento.Id;
        _cmbSala.SelectedValue = agendamento.SalaId;
        _dtpInicio.Value = agendamento.Inicio;
        _dtpFim.Value = agendamento.Fim;
        AtualizarStatus();
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        UiFeedback.Execute(() =>
        {
            var agendamento = new Agendamento
            {
                Id = _idSelecionado,
                SalaId = ObterSalaIdSelecionada(),
                Inicio = _dtpInicio.Value,
                Fim = _dtpFim.Value
            };

            if (_idSelecionado == 0)
                _repo.Inserir(agendamento);
            else
                _repo.Atualizar(agendamento);

            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        });
    }

    private void BtnExcluir_Click(object? sender, EventArgs e)
    {
        if (_idSelecionado == 0)
        {
            UiFeedback.ShowInfo(
                "Selecione um agendamento para excluir.",
                "Exclusao de agendamento");
            return;
        }

        if (_repo.PossuiPeriodoFuturo(_idSelecionado))
        {
            UiFeedback.ShowInfo(
                "Nao e possivel excluir o agendamento selecionado, pois ele ainda possui data/hora futura.",
                "Exclusao de agendamento");
            return;
        }

        if (!UiFeedback.Confirm(
                "Deseja excluir o agendamento selecionado?",
                "Exclusao de agendamento"))
            return;

        UiFeedback.Execute(() =>
        {
            _repo.Excluir(_idSelecionado);
            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        });
    }

    private void LimparFormulario()
    {
        _carregando = true;
        try
        {
            _idSelecionado = 0;
            _cmbSala.SelectedIndex = -1;
            _dtpInicio.Value = DateTime.Now;
            _dtpFim.Value = DateTime.Now.AddHours(1);
            DesfixarSelecaoGrid();
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

        if (IsHandleCreated)
        {
            BeginInvoke(() =>
            {
                _grid.ClearSelection();
                _grid.CurrentCell = null;
            });
        }
    }

    private void AtualizarStatus()
    {
        var totalSalas = _cmbSala.Items.Count;
        var totalAgendamentos = _grid.Rows.Count;

        _btnSalvar.Enabled = totalSalas > 0;
        _btnExcluir.Enabled = _idSelecionado != 0;

        _lblStatus.Text = totalSalas == 0
            ? "Cadastre uma sala para comecar a agendar."
            : _idSelecionado != 0
                ? $"Editando o agendamento ID {_idSelecionado}."
                : $"{totalAgendamentos} agendamento(s) cadastrado(s) em {totalSalas} sala(s).";
    }

    private int? ObterSalaIdSelecionada()
    {
        if (_cmbSala.SelectedValue is int salaId)
            return salaId;

        if (_cmbSala.SelectedItem is Sala sala)
            return sala.Id;

        return null;
    }

    private void SincronizarComSalas()
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(SincronizarComSalas);
            return;
        }

        try
        {
            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        }
        catch (Exception ex)
        {
            UiFeedback.ShowError(ex);
        }
    }
}

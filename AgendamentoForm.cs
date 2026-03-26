using System.Drawing;
using System.Windows.Forms;
using CoworkingAgendamento.Models;
using CoworkingAgendamento.Repositories;
using Npgsql;

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

    public AgendamentoForm()
    {
        Text = "Cadastro de Agendamentos";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(940, 580);
        MinimumSize = new Size(940, 580);
        BackColor = Color.FromArgb(245, 248, 252);

        BuildUi();
        CarregarDadosIniciais();
    }

    private void BuildUi()
    {
        var painelTopo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 185,
            Padding = new Padding(18),
            BackColor = Color.White
        };

        var lblTitulo = new Label
        {
            Text = "Gerencie os agendamentos das salas",
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 80, 160),
            AutoSize = true,
            Location = new Point(18, 18)
        };

        AddLabel(painelTopo, "Sala", new Point(18, 64));
        _cmbSala = new ComboBox
        {
            Location = new Point(18, 88),
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular)
        };

        AddLabel(painelTopo, "Data/Hora inicio", new Point(290, 64));
        _dtpInicio = new DateTimePicker
        {
            Location = new Point(290, 88),
            Width = 200,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            ShowUpDown = true,
            Value = DateTime.Now
        };

        AddLabel(painelTopo, "Data/Hora fim", new Point(512, 64));
        _dtpFim = new DateTimePicker
        {
            Location = new Point(512, 88),
            Width = 200,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            ShowUpDown = true,
            Value = DateTime.Now.AddHours(1)
        };

        _btnNovo = CreateButton("Novo", Color.FromArgb(100, 100, 110), new Point(18, 132));
        _btnSalvar = CreateButton("Salvar", Color.FromArgb(30, 130, 76), new Point(134, 132));
        _btnExcluir = CreateButton("Excluir", Color.FromArgb(192, 57, 43), new Point(250, 132));

        _btnNovo.Click += (_, _) => LimparFormulario();
        _btnSalvar.Click += BtnSalvar_Click;
        _btnExcluir.Click += BtnExcluir_Click;

        _lblStatus = new Label
        {
            Text = "Cadastre uma sala para iniciar os agendamentos.",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = Color.FromArgb(90, 98, 110),
            AutoSize = true,
            Location = new Point(380, 140)
        };

        painelTopo.Controls.Add(lblTitulo);
        painelTopo.Controls.Add(_cmbSala);
        painelTopo.Controls.Add(_dtpInicio);
        painelTopo.Controls.Add(_dtpFim);
        painelTopo.Controls.Add(_btnNovo);
        painelTopo.Controls.Add(_btnSalvar);
        painelTopo.Controls.Add(_btnExcluir);
        painelTopo.Controls.Add(_lblStatus);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
        };

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

        Controls.Add(_grid);
        Controls.Add(painelTopo);
    }

    private static void AddLabel(Control parent, string text, Point location)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            AutoSize = true,
            Location = location
        });
    }

    private static Button CreateButton(string text, Color backColor, Point location) =>
        new()
        {
            Text = text,
            Location = location,
            Size = new Size(100, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };

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
            MostrarErroDeCarga("agendamentos", ex);
        }
    }

    private void CarregarSalas()
    {
        var salas = _salaRepo.ListarTodas();
        _cmbSala.DataSource = null;
        _cmbSala.DataSource = salas;
        _cmbSala.DisplayMember = nameof(Sala.Nome);
        _cmbSala.ValueMember = nameof(Sala.Id);
    }

    private void CarregarGrid()
    {
        _grid.DataSource = null;
        _grid.DataSource = _repo.ListarTodos();
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Agendamento agendamento)
        {
            return;
        }

        _idSelecionado = agendamento.Id;
        _cmbSala.SelectedValue = agendamento.SalaId;
        _dtpInicio.Value = agendamento.Inicio;
        _dtpFim.Value = agendamento.Fim;
        _lblStatus.Text = $"Editando o agendamento ID {_idSelecionado}.";
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        if (_cmbSala.SelectedItem is not Sala salaSelecionada)
        {
            MessageBox.Show(
                "Cadastre ao menos uma sala antes de criar agendamentos.",
                "Agendamento",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
        {
            var agendamento = new Agendamento
            {
                Id = _idSelecionado,
                SalaId = salaSelecionada.Id,
                Inicio = _dtpInicio.Value,
                Fim = _dtpFim.Value
            };

            if (_idSelecionado == 0)
            {
                _repo.Inserir(agendamento);
            }
            else
            {
                _repo.Atualizar(agendamento);
            }

            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        }
        catch (PostgresException ex)
        {
            MessageBox.Show(
                ex.MessageText,
                "Erro no banco de dados",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnExcluir_Click(object? sender, EventArgs e)
    {
        if (_idSelecionado == 0)
        {
            MessageBox.Show(
                "Selecione um agendamento para excluir.",
                "Exclusao de agendamento",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var confirmacao = MessageBox.Show(
            "Deseja excluir o agendamento selecionado?",
            "Exclusao de agendamento",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacao != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _repo.Excluir(_idSelecionado);
            CarregarSalas();
            CarregarGrid();
            LimparFormulario();
            AtualizarStatus();
        }
        catch (PostgresException ex)
        {
            MessageBox.Show(
                ex.MessageText,
                "Erro no banco de dados",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LimparFormulario()
    {
        _idSelecionado = 0;

        if (_cmbSala.Items.Count > 0)
        {
            _cmbSala.SelectedIndex = 0;
        }

        _dtpInicio.Value = DateTime.Now;
        _dtpFim.Value = DateTime.Now.AddHours(1);
        _grid.ClearSelection();
        AtualizarStatus();
    }

    private void AtualizarStatus()
    {
        var totalSalas = _cmbSala.Items.Count;
        var totalAgendamentos = _grid.Rows.Count;

        _btnSalvar.Enabled = totalSalas > 0;
        _btnExcluir.Enabled = totalAgendamentos > 0;

        _lblStatus.Text = totalSalas == 0
            ? "Cadastre uma sala para comecar a agendar."
            : $"{totalAgendamentos} agendamento(s) cadastrado(s) em {totalSalas} sala(s).";
    }

    private static void MostrarErroDeCarga(string contexto, Exception ex)
    {
        var mensagem = ex is PostgresException postgresException
            ? postgresException.MessageText
            : ex.Message;

        MessageBox.Show(
            $"Nao foi possivel carregar os {contexto}.\n\n{mensagem}",
            "Inicializacao da tela",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

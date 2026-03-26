using System.Drawing;
using System.Windows.Forms;
using CoworkingAgendamento.Models;
using CoworkingAgendamento.Repositories;
using Npgsql;

namespace CoworkingAgendamento.Forms;

public class SalaForm : Form
{
    private readonly SalaRepository _repo = new();

    private DataGridView _grid = null!;
    private TextBox _txtNome = null!;
    private Button _btnNovo = null!;
    private Button _btnSalvar = null!;
    private Button _btnExcluir = null!;
    private Label _lblStatus = null!;

    private int _idSelecionado;

    public SalaForm()
    {
        Text = "Cadastro de Salas";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(760, 520);
        MinimumSize = new Size(760, 520);
        BackColor = Color.FromArgb(245, 248, 252);

        BuildUi();
        CarregarDadosIniciais();
    }

    private void BuildUi()
    {
        var painelTopo = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(18),
            BackColor = Color.White
        };

        var lblTitulo = new Label
        {
            Text = "Gerencie as salas de reuniao",
            Font = new Font("Segoe UI", 15f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 80, 160),
            AutoSize = true,
            Location = new Point(18, 18)
        };

        var lblNome = new Label
        {
            Text = "Nome da sala",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            AutoSize = true,
            Location = new Point(18, 60)
        };

        _txtNome = new TextBox
        {
            Location = new Point(18, 84),
            Width = 320,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Regular),
            MaxLength = 100
        };

        _btnNovo = CreateButton("Novo", Color.FromArgb(100, 100, 110), new Point(360, 82));
        _btnSalvar = CreateButton("Salvar", Color.FromArgb(30, 130, 76), new Point(476, 82));
        _btnExcluir = CreateButton("Excluir", Color.FromArgb(192, 57, 43), new Point(592, 82));

        _btnNovo.Click += (_, _) => LimparFormulario();
        _btnSalvar.Click += BtnSalvar_Click;
        _btnExcluir.Click += BtnExcluir_Click;

        _lblStatus = new Label
        {
            Text = "Cadastre quantas salas precisar.",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = Color.FromArgb(90, 98, 110),
            AutoSize = true,
            Location = new Point(18, 118)
        };

        painelTopo.Controls.Add(lblTitulo);
        painelTopo.Controls.Add(lblNome);
        painelTopo.Controls.Add(_txtNome);
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

        Controls.Add(_grid);
        Controls.Add(painelTopo);
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
            CarregarGrid();
            LimparFormulario();
        }
        catch (Exception ex)
        {
            MostrarErroDeCarga("salas", ex);
        }
    }

    private void CarregarGrid()
    {
        _grid.DataSource = null;
        _grid.DataSource = _repo.ListarTodas();
        _lblStatus.Text = $"{_grid.Rows.Count} sala(s) cadastrada(s).";
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.CurrentRow?.DataBoundItem is not Sala sala)
        {
            return;
        }

        _idSelecionado = sala.Id;
        _txtNome.Text = sala.Nome;
        _lblStatus.Text = $"Editando a sala ID {_idSelecionado}.";
    }

    private void BtnSalvar_Click(object? sender, EventArgs e)
    {
        try
        {
            var sala = new Sala
            {
                Id = _idSelecionado,
                Nome = _txtNome.Text
            };

            if (_idSelecionado == 0)
            {
                _repo.Inserir(sala);
            }
            else
            {
                _repo.Atualizar(sala);
            }

            CarregarGrid();
            LimparFormulario();
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
                "Selecione uma sala para excluir.",
                "Exclusao de sala",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var confirmacao = MessageBox.Show(
            "Deseja excluir a sala selecionada?",
            "Exclusao de sala",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmacao != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _repo.Excluir(_idSelecionado);
            CarregarGrid();
            LimparFormulario();
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
        _txtNome.Clear();
        _grid.ClearSelection();
        _txtNome.Focus();
        _lblStatus.Text = $"{_grid.Rows.Count} sala(s) cadastrada(s).";
    }

    private static void MostrarErroDeCarga(string contexto, Exception ex)
    {
        var mensagem = ex is PostgresException postgresException
            ? postgresException.MessageText
            : ex.Message;

        MessageBox.Show(
            $"Nao foi possivel carregar as {contexto}.\n\n{mensagem}",
            "Inicializacao da tela",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

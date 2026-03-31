using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace CoworkingAgendamento.Forms;

internal static class UiTheme
{
    public static readonly Color PageBackground = Color.FromArgb(242, 246, 251);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 253);
    public static readonly Color Border = Color.FromArgb(220, 228, 238);
    public static readonly Color TextPrimary = Color.FromArgb(31, 41, 55);
    public static readonly Color TextMuted = Color.FromArgb(107, 114, 128);
    public static readonly Color BrandDark = Color.FromArgb(15, 61, 110);
    public static readonly Color Success = Color.FromArgb(34, 139, 94);
    public static readonly Color Neutral = Color.FromArgb(99, 110, 125);
    public static readonly Color Danger = Color.FromArgb(192, 57, 43);

    public static Font Font(float size, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", size, style);

    public static Panel CreateCardPanel(int height) =>
        new()
        {
            Dock = DockStyle.Top,
            Height = height,
            Padding = new Padding(20, 18, 20, 18),
            BackColor = Surface
        };

    public static Panel CreateSpacer(int height) =>
        new()
        {
            Dock = DockStyle.Top,
            Height = height,
            BackColor = PageBackground
        };

    public static Label CreateTitleLabel(string text, Point location) =>
        new()
        {
            Text = text,
            Font = Font(16f, FontStyle.Bold),
            ForeColor = BrandDark,
            AutoSize = true,
            Location = location
        };

    public static Label CreateFieldLabel(string text, Point location) =>
        new()
        {
            Text = text,
            Font = Font(9.5f),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = location
        };

    public static Label CreateStatusLabel(string text, Point location) =>
        new()
        {
            Text = text,
            Font = Font(9f),
            ForeColor = TextMuted,
            AutoSize = true,
            Location = location
        };

    public static TextBox CreateTextBox(Point location, int width, int maxLength) =>
        new()
        {
            Location = location,
            Width = width,
            Font = Font(10f),
            MaxLength = maxLength,
            BorderStyle = BorderStyle.FixedSingle
        };

    public static ComboBox CreateComboBox(Point location, int width) =>
        new()
        {
            Location = location,
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = Font(10f),
            IntegralHeight = false
        };

    public static DateTimePicker CreateDateTimePicker(Point location, int width, DateTime value) =>
        new()
        {
            Location = location,
            Width = width,
            Font = Font(10f),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            ShowUpDown = true,
            Value = value
        };

    public static Button CreateButton(string text, Color backColor, Point location) =>
        new()
        {
            Text = text,
            Location = location,
            Size = new Size(108, 40),
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = Font(9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };

    public static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Surface,
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = Font(9.5f),
            GridColor = Border,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 42,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            RowTemplate = { Height = 34 }
        };

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = BrandDark,
            ForeColor = Color.White,
            Font = Font(9.5f, FontStyle.Bold),
            SelectionBackColor = BrandDark,
            SelectionForeColor = Color.White,
            Padding = new Padding(8, 8, 8, 8),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            WrapMode = DataGridViewTriState.False
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = TextPrimary,
            SelectionBackColor = Color.FromArgb(224, 238, 252),
            SelectionForeColor = TextPrimary,
            Padding = new Padding(6, 4, 6, 4),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            WrapMode = DataGridViewTriState.False
        };

        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = SurfaceAlt,
            ForeColor = TextPrimary,
            SelectionBackColor = Color.FromArgb(224, 238, 252),
            SelectionForeColor = TextPrimary,
            Padding = new Padding(6, 4, 6, 4),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            WrapMode = DataGridViewTriState.False
        };

        return grid;
    }
}

internal static class UiFeedback
{
    public static void Execute(Action action)
    {
        try
        {
            action();
        }
        catch (PostgresException ex)
        {
            ShowDatabaseError(ex);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    public static void ShowDatabaseError(PostgresException ex) =>
        MessageBox.Show(
            ex.MessageText,
            "Erro no banco de dados",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

    public static void ShowInfo(string message, string title) =>
        MessageBox.Show(
            message,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

    public static void ShowError(Exception ex) =>
        MessageBox.Show(
            ex.Message,
            "Erro",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

    public static bool Confirm(string message, string title) =>
        MessageBox.Show(
            message,
            title,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes;

    public static void ShowLoadError(string contexto, Exception ex)
    {
        var mensagem = ex is PostgresException postgresException
            ? postgresException.MessageText
            : ex.Message;

        MessageBox.Show(
            $"Nao foi possivel carregar {contexto}.\n\n{mensagem}",
            "Inicializacao da tela",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

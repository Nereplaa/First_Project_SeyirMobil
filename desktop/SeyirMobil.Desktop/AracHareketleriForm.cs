using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

public partial class AracHareketleriForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();

    public AracHareketleriForm()
    {
        InitializeComponent();
        SetupGridColumns();
    }

    private void SetupGridColumns()
    {
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracId",
            DataPropertyName = "AracId",
            HeaderText = "Araç ID",
            Width = 80
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracPlaka",
            DataPropertyName = "AracPlaka",
            HeaderText = "Araç Plaka",
            Width = 140
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "VeriTarihi",
            DataPropertyName = "VeriTarihi",
            HeaderText = "Veri Tarihi",
            Width = 130,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Hiz",
            DataPropertyName = "Hiz",
            HeaderText = "Hız",
            Width = 90
        });
        dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "KmSayaci",
            DataPropertyName = "KmSayaci",
            HeaderText = "Km Sayacı",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
    }

    private async void AracHareketleriForm_Load(object? sender, EventArgs e)
    {
        await RefreshGridAsync();
    }

    private async void btnYenile_Click(object? sender, EventArgs e)
    {
        await RefreshGridAsync();
    }

    private void btnHareketRaporu_Click(object? sender, EventArgs e)
    {
        using var raporForm = new AracHareketRaporuForm();
        raporForm.ShowDialog(this);
    }

    private async Task RefreshGridAsync()
    {
        lblStatus.Text = "Yükleniyor...";
        try
        {
            var hareketler = await _apiClient.GetTumHareketlerAsync();
            dgvHareketler.DataSource = hareketler;
            lblStatus.Text = $"{hareketler.Count} hareket kaydı yüklendi.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Veri yüklenemedi.";
            MessageBox.Show(
                $"Araç hareketleri alınamadı. Backend API çalışıyor mu?\n\nHata: {ex.Message}",
                "Bağlantı Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}

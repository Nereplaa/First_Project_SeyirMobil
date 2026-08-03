using SeyirMobil.Desktop.Services;
using SeyirMobil.Desktop.Validation;

namespace SeyirMobil.Desktop;

public partial class VehiclesForm : Form
{
    private readonly VehicleApiClient _apiClient = new();

    public VehiclesForm()
    {
        InitializeComponent();
        SetupGridColumns();
    }

    private void SetupGridColumns()
    {
        dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "AracId",
            DataPropertyName = "AracId",
            HeaderText = "Araç ID",
            Width = 70
        });
        dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Plaka",
            DataPropertyName = "Plaka",
            HeaderText = "Plaka",
            Width = 120
        });
        dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TotalKm",
            DataPropertyName = "TotalKm",
            HeaderText = "Toplam KM",
            Width = 150,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" }
        });
        dgvVehicles.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "KayitTrh",
            DataPropertyName = "KayitTrh",
            HeaderText = "Kayıt Tarihi",
            Width = 160,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
        });
    }

    private async void VehiclesForm_Load(object? sender, EventArgs e)
    {
        await RefreshGridAsync();
    }

    private async void btnYenile_Click(object? sender, EventArgs e)
    {
        await RefreshGridAsync();
    }

    private async Task RefreshGridAsync()
    {
        lblStatus.Text = "Yükleniyor...";
        try
        {
            var vehicles = await _apiClient.GetVehiclesAsync();
            dgvVehicles.DataSource = vehicles;
            lblStatus.Text = $"{vehicles.Count} kayıt yüklendi.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Veri yüklenemedi.";
            MessageBox.Show(
                $"Araç listesi alınamadı. Backend API çalışıyor mu?\n\nHata: {ex.Message}",
                "Bağlantı Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void btnEkle_Click(object? sender, EventArgs e)
    {
        var plaka = PlakaValidator.Normalize(txtPlaka.Text);
        if (string.IsNullOrWhiteSpace(plaka))
        {
            MessageBox.Show("Plaka boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!PlakaValidator.IsValid(plaka))
        {
            MessageBox.Show(
                "Geçersiz plaka formatı.\n\nBeklenen: il kodu (01-81) + 1-3 harf + rakam.\nÖrnek: 34ABC123, 06A1234, 41SM001",
                "Uyarı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtTotalKm.Text.Trim(), out var totalKm) || totalKm < 0)
        {
            MessageBox.Show("Toplam KM geçerli bir sayı olmalı (negatif olamaz).", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnEkle.Enabled = false;
        lblStatus.Text = "Ekleniyor...";
        try
        {
            await _apiClient.CreateVehicleAsync(plaka, totalKm);
            txtPlaka.Clear();
            txtTotalKm.Clear();
            txtPlaka.Focus();
            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Ekleme başarısız.";
            MessageBox.Show(
                $"Araç eklenemedi.\n\nHata: {ex.Message}",
                "Hata",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnEkle.Enabled = true;
        }
    }

    private void dgvVehicles_SelectionChanged(object? sender, EventArgs e)
    {
        btnSil.Enabled = dgvVehicles.CurrentRow is not null;
    }

    private async void btnSil_Click(object? sender, EventArgs e)
    {
        if (dgvVehicles.CurrentRow?.DataBoundItem is not Models.VehicleDto selected)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"\"{selected.Plaka}\" plakalı aracı silmek istediğine emin misin?",
            "Silme Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        btnSil.Enabled = false;
        lblStatus.Text = "Siliniyor...";
        try
        {
            await _apiClient.DeleteVehicleAsync(selected.AracId);
            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Silme başarısız.";
            MessageBox.Show(
                $"Araç silinemedi.\n\nHata: {ex.Message}",
                "Hata",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            btnSil.Enabled = dgvVehicles.CurrentRow is not null;
        }
    }
}

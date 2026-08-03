using SeyirMobil.Desktop.Models;
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
        await PlakalarYukleAsync();
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

    // ---------- Silme ----------

    private void dgvHareketler_SelectionChanged(object? sender, EventArgs e)
    {
        btnSil.Enabled = dgvHareketler.CurrentRow is not null;
    }

    private async void btnSil_Click(object? sender, EventArgs e)
    {
        if (dgvHareketler.CurrentRow?.DataBoundItem is not AracHareketDto secili)
        {
            return;
        }

        var onay = MessageBox.Show(
            $"\"{secili.AracPlaka}\" - {secili.VeriTarihi:dd.MM.yyyy} tarihli kaydı silmek istediğine emin misin?",
            "Silme Onayı",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (onay != DialogResult.Yes)
        {
            return;
        }

        btnSil.Enabled = false;
        lblStatus.Text = "Siliniyor...";
        try
        {
            await _apiClient.DeleteHareketAsync(secili.Id);
            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Silme başarısız.";
            MessageBox.Show($"Kayıt silinemedi.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSil.Enabled = dgvHareketler.CurrentRow is not null;
        }
    }

    // ---------- Ekleme sihirbazı (4 adım: Plaka -> Tarih -> Hız -> Km Sayacı) ----------

    private async Task PlakalarYukleAsync()
    {
        try
        {
            var plakalar = await _apiClient.GetPlakalarAsync();
            cmbPlaka.DataSource = plakalar;
            cmbPlaka.DisplayMember = nameof(AracPlakaLookupDto.AracPlaka);
            cmbPlaka.ValueMember = nameof(AracPlakaLookupDto.AracId);
            cmbPlaka.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Plaka listesi alınamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void cmbPlaka_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbPlaka.SelectedIndex < 0)
        {
            groupTarih.Visible = false;
            groupHiz.Visible = false;
            groupKm.Visible = false;
            btnEkle.Enabled = false;
            return;
        }

        // Adim 1 (plaka) tamamlandi -> Adim 2 (tarih) gorunur olur, varsayilan bugun.
        groupTarih.Visible = true;
        var bugun = DateTime.Today;
        if (dtpTarih.Value.Date == bugun)
        {
            // Deger zaten bugun oldugu icin ValueChanged tetiklenmeyecek, sinirlari elle hesapla.
            await GuncelleSinirlarVeAdimlariAsync();
        }
        else
        {
            dtpTarih.Value = bugun; // bu satir dtpTarih_ValueChanged'i tetikleyip sinirlari hesaplatir
        }
    }

    private async void dtpTarih_ValueChanged(object? sender, EventArgs e)
    {
        await GuncelleSinirlarVeAdimlariAsync();
    }

    private void nudHiz_ValueChanged(object? sender, EventArgs e)
    {
        // Hiz, NumericUpDown'un kendi Minimum/Maximum'u ile zaten "dogrulanmis" sayilir.
    }

    // Secilen plaka + tarihe gore en yakin onceki/sonraki okumayi backend'den ceker, km sayaci
    // icin gecerli araligi (Adim 4) hesaplayip Adim 3 (hiz) + Adim 4'u (km) gosterir.
    private async Task GuncelleSinirlarVeAdimlariAsync()
    {
        if (cmbPlaka.SelectedItem is not AracPlakaLookupDto secilenArac)
        {
            return;
        }

        var tarih = DateOnly.FromDateTime(dtpTarih.Value);
        lblStatus.Text = "Sınırlar hesaplanıyor...";
        groupHiz.Visible = false;
        groupKm.Visible = false;
        btnEkle.Enabled = false;

        try
        {
            var sinirlar = await _apiClient.GetSinirlarAsync(secilenArac.AracPlaka, tarih);

            if (sinirlar.AyniTarihVarMi)
            {
                lblStatus.Text = "Bu plaka için bu tarihte zaten bir kayıt var. Farklı bir tarih seçin.";
                return;
            }

            // NumericUpDown'un Minimum/Maximum siralama hatasi vermemesi icin once genis bir
            // araliga sifirlanip SONRA gercek (dar) araliga cekiliyor.
            nudKm.Minimum = 0m;
            nudKm.Maximum = 99999999.99m;

            var min = sinirlar.OncekiKm.HasValue ? sinirlar.OncekiKm.Value + 0.01m : 0m;
            var max = sinirlar.SonrakiKm.HasValue ? sinirlar.SonrakiKm.Value - 0.01m : 99999999.99m;

            if (min > max)
            {
                lblStatus.Text = "Bu tarih için geçerli bir km aralığı yok (önceki/sonraki okumalar birbirine çok yakın).";
                return;
            }

            nudKm.Minimum = min;
            nudKm.Maximum = max;
            nudKm.Value = min;

            var oncekiMetin = sinirlar.OncekiTarih.HasValue
                ? $"Önceki: {sinirlar.OncekiTarih:dd.MM.yyyy} → {sinirlar.OncekiKm:N2} km"
                : "Önceki: yok (bu, ilk kayıt olacak)";
            var sonrakiMetin = sinirlar.SonrakiTarih.HasValue
                ? $"Sonraki: {sinirlar.SonrakiTarih:dd.MM.yyyy} → {sinirlar.SonrakiKm:N2} km"
                : "Sonraki: yok (bu, son kayıt olacak)";
            lblSinirBilgisi.Text = $"{oncekiMetin}\n{sonrakiMetin}\nGirilecek km bu ikisinin arasında olmalı.";

            groupHiz.Visible = true;
            groupKm.Visible = true;
            btnEkle.Enabled = true;
            lblStatus.Text = "Hız ve km sayacını girip Ekle'ye basabilirsin.";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Sınırlar hesaplanamadı.";
            MessageBox.Show($"Sınırlar hesaplanamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnEkle_Click(object? sender, EventArgs e)
    {
        if (cmbPlaka.SelectedItem is not AracPlakaLookupDto secilenArac)
        {
            return;
        }

        var request = new CreateAracHareketRequestDto(
            secilenArac.AracId,
            secilenArac.AracPlaka,
            DateOnly.FromDateTime(dtpTarih.Value),
            (int)nudHiz.Value,
            nudKm.Value);

        btnEkle.Enabled = false;
        lblStatus.Text = "Ekleniyor...";
        try
        {
            await _apiClient.CreateHareketAsync(request);
            lblStatus.Text = "Kayıt eklendi.";

            // Sihirbazi basa sar.
            cmbPlaka.SelectedIndex = -1;
            groupTarih.Visible = false;
            groupHiz.Visible = false;
            groupKm.Visible = false;

            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Ekleme başarısız.";
            MessageBox.Show($"Kayıt eklenemedi.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnEkle.Enabled = true;
        }
    }
}

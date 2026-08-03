using SeyirMobil.Desktop.Models;
using SeyirMobil.Desktop.Services;

namespace SeyirMobil.Desktop;

public partial class AracHareketleriForm : Form
{
    private readonly AracHareketApiClient _apiClient = new();

    // Ayni anda birden fazla sinir hesaplama istegi cakisirsa (ör. plaka hizli degistirilirse),
    // sadece EN SON baslatilan istegin sonucu UI'a uygulanir - eskisi "surum" uyusmadigi icin
    // sessizce yok sayilir.
    private int _sinirSorgusuSurumu;

    // Filtre seridi, API'ye tekrar gitmeden bu listenin uzerinde bellekte calisiyor.
    private List<AracHareketDto> _tumHareketler = [];

    public AracHareketleriForm()
    {
        InitializeComponent();
        SetupGridColumns();
    }

    private void SetupGridColumns()
    {
        // Basliklarin her zaman gorunur ve okunakli olmasi icin acikca ayarlaniyor.
        dgvHareketler.ColumnHeadersVisible = true;
        dgvHareketler.EnableHeadersVisualStyles = false;
        dgvHareketler.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
        dgvHareketler.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        dgvHareketler.ColumnHeadersDefaultCellStyle.Font = new Font(dgvHareketler.Font, FontStyle.Bold);
        dgvHareketler.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

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
        FiltrePlakaListesiniDoldur();
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
            _tumHareketler = await _apiClient.GetTumHareketlerAsync();
            dgvHareketler.DataSource = _tumHareketler;
            lblStatus.Text = $"{_tumHareketler.Count} hareket kaydı yüklendi.";
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
        finally
        {
            // flowUst'un yuksekligi adim adim degistigi icin (gruplar acilip kapaniyor),
            // dgvHareketler'in (Dock=Fill) hemen dogru sekilde yeniden hizalanmasini garantiler -
            // aksi halde basliklar/ust satirlar bir onceki yerlesimin altinda kalabiliyordu.
            flowUst.PerformLayout();
            PerformLayout();
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

    // ---------- Ekleme sihirbazı (4 adım: Plaka -> Tarih (Onayla) -> Hız -> Km Sayacı) ----------

    private async Task PlakalarYukleAsync()
    {
        try
        {
            var plakalar = await _apiClient.GetPlakalarAsync();

            // DataSource atarken WinForms ilk ogeyi otomatik secip SelectedIndexChanged'i hemen
            // tetikliyor - bu istenmeyen bir sihirbaz baslangicina yol aciyordu (bkz. 2026-08-03
            // kullanici bulgusu). Baglama sirasinda event'i gecici olarak ayirip, listeyi
            // SECIMSIZ (SelectedIndex=-1) birakip SONRA event'i tekrar bagliyoruz.
            cmbPlaka.SelectedIndexChanged -= cmbPlaka_SelectedIndexChanged;
            cmbPlaka.DataSource = plakalar;
            cmbPlaka.DisplayMember = nameof(AracPlakaLookupDto.AracPlaka);
            cmbPlaka.ValueMember = nameof(AracPlakaLookupDto.AracId);
            cmbPlaka.SelectedIndex = -1;
            cmbPlaka.SelectedIndexChanged += cmbPlaka_SelectedIndexChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Plaka listesi alınamadı.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cmbPlaka_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Adim 1 degisince, henuz tamamlanmamis TUM sonraki adimlar sifirlanir - onceki plaka
        // icin hesaplanmis sinirlar/degerler yanlislikla yeni plakaya tasinmasin diye.
        SonrakiAdimlariSifirla();

        if (cmbPlaka.SelectedIndex < 0)
        {
            groupTarih.Visible = false;
            return;
        }

        groupTarih.Visible = true;
        dtpTarih.Value = DateTime.Today;
        flowUst.PerformLayout();
    }

    private void dtpTarih_ValueChanged(object? sender, EventArgs e)
    {
        // Tarih degistigi (veya ilk kez ayarlandigi) an, henuz "onaylanmadigi" icin sonraki
        // adimlar (hiz/km) gizli kalir - kullanici "Tarihi Onayla"ya basmadan gorunmezler.
        SonrakiAdimlariSifirla();
    }

    private void SonrakiAdimlariSifirla()
    {
        groupHiz.Visible = false;
        groupKm.Visible = false;
        btnEkle.Enabled = false;
        lblSinirBilgisi.Text = "";
        flowUst.PerformLayout();
        PerformLayout();
    }

    private void nudHiz_ValueChanged(object? sender, EventArgs e)
    {
        // Hiz, NumericUpDown'un kendi Minimum/Maximum'u ile zaten "dogrulanmis" sayilir.
    }

    private async void btnTarihOnayla_Click(object? sender, EventArgs e)
    {
        await GuncelleSinirlarVeAdimlariAsync();
    }

    // Secilen plaka + "onaylanmis" tarihe gore en yakin onceki/sonraki okumayi backend'den
    // ceker, km sayaci icin gecerli araligi hesaplayip Adim 3 (hiz) + Adim 4'u (km) gosterir.
    private async Task GuncelleSinirlarVeAdimlariAsync()
    {
        if (cmbPlaka.SelectedItem is not AracPlakaLookupDto secilenArac)
        {
            return;
        }

        var buSurum = ++_sinirSorgusuSurumu;
        var tarih = DateOnly.FromDateTime(dtpTarih.Value);
        lblStatus.Text = "Sınırlar hesaplanıyor...";

        try
        {
            var sinirlar = await _apiClient.GetSinirlarAsync(secilenArac.AracPlaka, tarih);

            // Bu bekleme surerken kullanici plakayi/tarihi degistirmis olabilir - o zaman bu
            // sonuc artik ESKI (surum uyusmuyor), UI'a hic dokunmadan sessizce cikilir.
            if (buSurum != _sinirSorgusuSurumu)
            {
                return;
            }

            if (sinirlar.AyniTarihVarMi)
            {
                lblStatus.Text = "Bu plaka için bu tarihte zaten bir kayıt var. Farklı bir tarih seçin.";
                return;
            }

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
            flowUst.PerformLayout();
            PerformLayout();
            lblStatus.Text = "Hız ve km sayacını girip Ekle'ye basabilirsin.";
        }
        catch (Exception ex)
        {
            if (buSurum != _sinirSorgusuSurumu)
            {
                return;
            }
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

            await RefreshGridAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Ekleme başarısız.";
            MessageBox.Show($"Kayıt eklenemedi.\n\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnEkle.Enabled = true;
        }
    }

    // ---------- Filtre şeridi (yerelde, bellekte - API'ye tekrar gitmez) ----------

    private const string FiltreTumu = "Tümü";

    private void FiltrePlakaListesiniDoldur()
    {
        var plakalar = _tumHareketler
            .Select(h => h.AracPlaka)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
        plakalar.Insert(0, FiltreTumu);

        cmbFiltrePlaka.DataSource = plakalar;
        cmbFiltrePlaka.SelectedIndex = 0;
    }

    private void chkFiltreTarih_CheckedChanged(object? sender, EventArgs e)
    {
        dtpFiltreTarih.Enabled = chkFiltreTarih.Checked;
    }

    private void btnFiltreUygula_Click(object? sender, EventArgs e)
    {
        IEnumerable<AracHareketDto> sonuc = _tumHareketler;

        if (cmbFiltrePlaka.SelectedItem is string plaka && plaka != FiltreTumu)
        {
            sonuc = sonuc.Where(h => h.AracPlaka == plaka);
        }

        if (chkFiltreTarih.Checked)
        {
            var tarih = DateOnly.FromDateTime(dtpFiltreTarih.Value);
            sonuc = sonuc.Where(h => h.VeriTarihi == tarih);
        }

        if (!string.IsNullOrWhiteSpace(txtFiltreHiz.Text))
        {
            if (!int.TryParse(txtFiltreHiz.Text.Trim(), out var hiz))
            {
                MessageBox.Show("Hız filtresi geçerli bir tam sayı olmalı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            sonuc = sonuc.Where(h => h.Hiz == hiz);
        }

        if (!string.IsNullOrWhiteSpace(txtFiltreKm.Text))
        {
            if (!decimal.TryParse(txtFiltreKm.Text.Trim(), out var km))
            {
                MessageBox.Show("Km sayacı filtresi geçerli bir sayı olmalı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            sonuc = sonuc.Where(h => h.KmSayaci == km);
        }

        var liste = sonuc.ToList();
        dgvHareketler.DataSource = liste;
        lblStatus.Text = $"{liste.Count} / {_tumHareketler.Count} kayıt gösteriliyor (filtreli).";
    }

    private void btnFiltreTemizle_Click(object? sender, EventArgs e)
    {
        cmbFiltrePlaka.SelectedIndex = 0;
        chkFiltreTarih.Checked = false;
        txtFiltreHiz.Clear();
        txtFiltreKm.Clear();

        dgvHareketler.DataSource = _tumHareketler;
        lblStatus.Text = $"{_tumHareketler.Count} hareket kaydı yüklendi.";
    }
}

import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  DxFileUploaderModule,
  DxDataGridModule,
  DxButtonModule,
} from 'devextreme-angular';
import { AracHareketApi } from '../../services/arac-hareket-api';
import { Bildirim } from '../../services/bildirim';
import { ImportDosyaKoprusu } from '../../services/import-dosya-koprusu';
import { ImportSatiriSonucDto, ImportOnaylaSatiriDto } from '../../models/arac-hareket.models';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';
import { dosyaIndir } from '../../utils/dosya-indir';

// Grid'in KENDI icinde tuttugu ve duzenledigi satir tipi - backend'den gelen
// ImportSatiriSonucDto'ya, sadece client'ta anlam ifade eden `cakismaAksiyonu` alanini ekler.
type GridSatiri = ImportSatiriSonucDto & { cakismaAksiyonu: string };

const CAKISMA_AKSIYON_SECENEKLERI = [
  { deger: 'UzerineYaz', metin: 'Üzerine Yaz' },
  { deger: 'Atla', metin: 'Atla' },
];

@Component({
  selector: 'app-import',
  imports: [DxFileUploaderModule, DxDataGridModule, DxButtonModule],
  templateUrl: './import.html',
  styleUrl: './import.css',
})
export class Import implements OnInit {
  private readonly api = inject(AracHareketApi);
  private readonly bildirim = inject(Bildirim);
  private readonly dosyaKoprusu = inject(ImportDosyaKoprusu);

  satirlar = signal<GridSatiri[]>([]);
  yukleniyor = signal(false);
  onaylaniyor = signal(false);
  dosyaHatasi = signal<string | null>(null);

  readonly columns = [
    { dataField: 'satirNo', caption: '#', width: 45, allowEditing: false },
    { dataField: 'aracPlaka', caption: 'Plaka', width: 130 },
    {
      dataField: 'veriTarihi',
      caption: 'Tarih',
      width: 110,
      customizeText: (c: { value?: string }) => (c.value ? this.formatTarih(c.value) : 'Geçersiz'),
    },
    { dataField: 'hiz', caption: 'Hız', width: 80, dataType: 'number' as const },
    { dataField: 'kmSayaci', caption: 'Km Sayacı', width: 120, dataType: 'number' as const, format: '#,##0.00' },
    {
      caption: 'Durum',
      width: 170,
      allowEditing: false,
      calculateCellValue: (r: GridSatiri) => this.durumMetni(r),
    },
    {
      dataField: 'cakismaAksiyonu',
      caption: 'Çakışma Aksiyonu',
      width: 150,
      lookup: { dataSource: CAKISMA_AKSIYON_SECENEKLERI, valueExpr: 'deger', displayExpr: 'metin' },
    },
    {
      caption: 'Hata',
      minWidth: 220,
      allowEditing: false,
      calculateCellValue: (r: GridSatiri) => r.hatalar.join(' · '),
    },
  ];

  toplamSatir = computed(() => this.satirlar().length);
  hepsiGecerliMi = computed(
    () => this.satirlar().length > 0 && this.satirlar().every((s) => this.satirGecerliMi(s))
  );

  ngOnInit(): void {
    // Liste sayfasindaki "Excel'den Veri Aktar" kisayolundan gelindiyse, dosya zaten
    // secilmis oluyor - kullanicinin burada AYRICA "Excel Dosyası Seç" demesine gerek yok.
    const bekleyen = this.dosyaKoprusu.bekleyenDosya;
    if (bekleyen) {
      this.dosyaKoprusu.bekleyenDosya = null;
      this.dosyaYukle(bekleyen);
    }
  }

  private satirGecerliMi(s: GridSatiri): boolean {
    if (s.hatalar.length > 0) {
      return false;
    }
    if (s.cakismaVarMi && !s.cakismaAksiyonu) {
      return false;
    }
    return true;
  }

  durumMetni(s: GridSatiri): string {
    if (s.hatalar.length > 0) {
      return 'Hata';
    }
    if (s.cakismaVarMi && !s.cakismaAksiyonu) {
      return 'Çakışma — karar bekliyor';
    }
    if (s.cakismaVarMi) {
      return s.cakismaAksiyonu === 'UzerineYaz' ? 'Üzerine yazılacak' : 'Atlanacak';
    }
    if (s.yeniAracMi) {
      return 'Yeni araç';
    }
    return 'Hazır';
  }

  // Grid satırlarını duruma göre renklendirir - kırmızımsı: hata, sarımsı: çakışma karar
  // bekliyor, yeşilimsi: içe aktarılmaya hazır. Önceden bu ayrım SADECE metinle (Durum
  // sütunu) yapılıyordu, göz taraması için yetersizdi (gerçek kullanıcı geri bildirimi).
  onRowPrepared(e: { rowType: string; data?: GridSatiri; rowElement: HTMLElement }): void {
    if (e.rowType !== 'data' || !e.data) {
      return;
    }
    if (e.data.hatalar.length > 0) {
      e.rowElement.style.backgroundColor = '#fef2f2';
    } else if (e.data.cakismaVarMi && !e.data.cakismaAksiyonu) {
      e.rowElement.style.backgroundColor = '#fefce8';
    } else {
      e.rowElement.style.backgroundColor = '#f0fdf4';
    }
  }

  dosyaSecildi(event: { value?: File[] }): void {
    const dosya = event.value?.[0];
    if (!dosya) {
      return;
    }
    this.dosyaYukle(dosya);
  }

  private dosyaYukle(dosya: File): void {
    this.dosyaHatasi.set(null);
    this.satirlar.set([]);
    this.yukleniyor.set(true);
    this.api.importOnizle(dosya).subscribe({
      next: (yanit) => {
        this.yukleniyor.set(false);
        if (yanit.dosyaHatasi) {
          this.dosyaHatasi.set(yanit.dosyaHatasi);
          return;
        }
        this.satirlar.set(yanit.satirlar.map((s) => ({ ...s, cakismaAksiyonu: '' })));
      },
      error: (err) => {
        this.yukleniyor.set(false);
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`Dosya okunamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  sablonIndir(): void {
    this.api.importSablonIndir().subscribe({
      next: (blob) => dosyaIndir(blob, 'import-sablon.xlsx'),
      error: (err) => {
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`Şablon indirilemedi.\n\nHata: ${err.message}`);
      },
    });
  }

  // Grid'deki hucre duzenlemeleri DevExtreme tarafindan dataSource dizisi UZERINDE dogrudan
  // yapiliyor (referans degismiyor) - Angular signal'in bunu fark edip bagli computed'lari
  // (hepsiGecerliMi vb.) yeniden hesaplamasi icin referansi ELLE tazeliyoruz.
  gridKaydedildi(): void {
    this.satirlar.update((arr) => [...arr]);
  }

  yenidenDogrula(): void {
    const hamSatirlar = this.satirlar().map((s) => ({
      satirNo: s.satirNo,
      aracPlaka: s.aracPlaka,
      veriTarihi: s.veriTarihi ?? '',
      hiz: s.hiz,
      kmSayaci: s.kmSayaci,
    }));
    this.yukleniyor.set(true);
    this.api.importYenidenDogrula(hamSatirlar).subscribe({
      next: (yanit) => {
        const eskiAksiyonlar = new Map(this.satirlar().map((s) => [s.satirNo, s.cakismaAksiyonu]));
        this.satirlar.set(
          yanit.satirlar.map((s) => ({ ...s, cakismaAksiyonu: eskiAksiyonlar.get(s.satirNo) ?? '' }))
        );
        this.yukleniyor.set(false);
        this.bildirim.bilgi('Yeniden doğrulandı.');
      },
      error: (err) => {
        this.yukleniyor.set(false);
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`Yeniden doğrulanamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  async iceAktar(): Promise<void> {
    if (!this.hepsiGecerliMi()) {
      this.bildirim.bilgi(
        'Tüm satırlar geçerli olmadan içe aktarılamaz. Hatalı veya karar bekleyen satırları düzeltip "Yeniden Doğrula"ya basın.'
      );
      return;
    }
    const onay = await this.bildirim.onayla(`${this.toplamSatir()} satır içe aktarılacak. Devam edilsin mi?`);
    if (!onay) {
      return;
    }

    const gonderilecek: ImportOnaylaSatiriDto[] = this.satirlar().map((s) => ({
      satirNo: s.satirNo,
      aracPlaka: s.aracPlaka,
      veriTarihi: s.veriTarihi!,
      hiz: s.hiz!,
      kmSayaci: s.kmSayaci!,
      cakismaAksiyonu: s.cakismaAksiyonu,
    }));

    this.onaylaniyor.set(true);
    this.api.importOnayla(gonderilecek).subscribe({
      next: (sonuc) => {
        this.onaylaniyor.set(false);
        this.bildirim.bilgi(
          `İçe aktarma tamamlandı: ${sonuc.eklenenSayisi} eklendi, ${sonuc.guncellenenSayisi} güncellendi, ${sonuc.atlananSayisi} atlandı.`
        );
        this.satirlar.set([]);
      },
      error: (err) => {
        this.onaylaniyor.set(false);
        if (oturumHatasiMi(err)) {
          return;
        }
        this.bildirim.hata(`İçe aktarılamadı.\n\nHata: ${err.error?.message ?? err.message}`);
      },
    });
  }

  formatTarih(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  }
}

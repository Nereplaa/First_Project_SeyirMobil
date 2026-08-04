import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AracHareketApi } from '../../services/arac-hareket-api';
import {
  AracHareketDto,
  AracPlakaLookupDto,
  AracHareketSinirlarDto,
} from '../../models/arac-hareket.models';
import { dosyaIndir } from '../../utils/dosya-indir';

const FILTRE_TUMU = 'Tümü';

function bugunIso(): string {
  const d = new Date();
  const yerelGun = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
  return yerelGun.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-liste',
  imports: [FormsModule],
  templateUrl: './liste.html',
  styleUrl: './liste.css',
})
export class Liste implements OnInit {
  private readonly api = inject(AracHareketApi);

  // ---------- Ana liste ----------
  tumHareketler = signal<AracHareketDto[]>([]);
  gosterilenHareketler = signal<AracHareketDto[]>([]);
  seciliHareket = signal<AracHareketDto | null>(null);
  statusText = signal('');
  disaAktariliyor = signal(false);

  // ---------- Sayfalama (filtrelenmiş/gösterilen liste üzerinde, yerelde) ----------
  readonly sayfaBoyutuSecenekleri = [10, 25, 50, 100];
  sayfaBoyutu = signal(25);
  suankiSayfa = signal(1);
  toplamSayfa = computed(() => Math.max(1, Math.ceil(this.gosterilenHareketler().length / this.sayfaBoyutu())));
  sayfalanmisHareketler = computed(() => {
    const baslangic = (this.suankiSayfa() - 1) * this.sayfaBoyutu();
    return this.gosterilenHareketler().slice(baslangic, baslangic + this.sayfaBoyutu());
  });

  // ---------- Filtre şeridi ----------
  readonly filtreTumu = FILTRE_TUMU;
  filtrePlakaListesi = signal<string[]>([FILTRE_TUMU]);
  filtrePlaka = FILTRE_TUMU;
  filtreTarihAktif = false;
  filtreTarih = bugunIso();
  filtreHiz = '';
  filtreKm = '';
  filtreHatasi = signal('');

  // ---------- Ekleme sihirbazı ----------
  plakalar = signal<AracPlakaLookupDto[]>([]);
  wizardAracId: number | null = null;
  wizardTarih = bugunIso();
  tarihOnaylandi = signal(false);
  sinirlarHesaplaniyor = signal(false);
  sinirBilgisi = signal('');
  wizardHiz = 0;
  wizardKm = 0;
  kmMin = signal(0);
  kmMax = signal(99999999.99);
  ekleEtkin = signal(false);
  ekleniyor = signal(false);

  private sinirSorgusuSurumu = 0;

  ngOnInit(): void {
    this.refreshGrid();
    this.plakalarYukle();
  }

  // ---------- Ana liste ----------

  refreshGrid(): void {
    this.statusText.set('Yükleniyor...');
    this.api.getTumHareketler().subscribe({
      next: (hareketler) => {
        this.tumHareketler.set(hareketler);
        this.gosterilenHareketler.set(hareketler);
        this.suankiSayfa.set(1);
        this.filtrePlakaListesiniDoldur(hareketler);
        this.statusText.set(`${hareketler.length} hareket kaydı yüklendi.`);
      },
      error: (err) => {
        this.statusText.set('Veri yüklenemedi.');
        alert(`Araç hareketleri alınamadı. Backend API çalışıyor mu?\n\nHata: ${err.message}`);
      },
    });
  }

  satirSec(h: AracHareketDto): void {
    this.seciliHareket.set(h);
  }

  sil(): void {
    const secili = this.seciliHareket();
    if (!secili) {
      return;
    }
    const onay = confirm(
      `"${secili.aracPlaka}" - ${this.formatTarih(secili.veriTarihi)} tarihli kaydı silmek istediğine emin misin?`
    );
    if (!onay) {
      return;
    }

    this.statusText.set('Siliniyor...');
    this.api.deleteHareket(secili.id).subscribe({
      next: () => {
        this.seciliHareket.set(null);
        this.refreshGrid();
      },
      error: (err) => {
        this.statusText.set('Silme başarısız.');
        alert(`Kayıt silinemedi.\n\nHata: ${err.message}`);
      },
    });
  }

  // ---------- Ekleme sihirbazı ----------

  private plakalarYukle(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => alert(`Plaka listesi alınamadı.\n\nHata: ${err.message}`),
    });
  }

  onPlakaSecildi(): void {
    this.sonrakiAdimlariSifirla();
    if (this.wizardAracId != null) {
      this.wizardTarih = bugunIso();
    }
  }

  onTarihDegisti(): void {
    this.sonrakiAdimlariSifirla();
  }

  private sonrakiAdimlariSifirla(): void {
    this.tarihOnaylandi.set(false);
    this.ekleEtkin.set(false);
    this.sinirBilgisi.set('');
  }

  get seciliAracPlaka(): string | null {
    const arac = this.plakalar().find((p) => p.aracId === this.wizardAracId);
    return arac ? arac.aracPlaka : null;
  }

  tarihiOnayla(): void {
    this.guncelleSinirlarVeAdimlari();
  }

  private guncelleSinirlarVeAdimlari(): void {
    const plaka = this.seciliAracPlaka;
    if (!plaka) {
      return;
    }

    const buSurum = ++this.sinirSorgusuSurumu;
    this.sinirlarHesaplaniyor.set(true);
    this.statusText.set('Sınırlar hesaplanıyor...');

    this.api.getSinirlar(plaka, this.wizardTarih).subscribe({
      next: (sinirlar: AracHareketSinirlarDto) => {
        if (buSurum !== this.sinirSorgusuSurumu) {
          return;
        }
        this.sinirlarHesaplaniyor.set(false);

        if (sinirlar.ayniTarihVarMi) {
          this.statusText.set('Bu plaka için bu tarihte zaten bir kayıt var. Farklı bir tarih seçin.');
          return;
        }

        const min = sinirlar.oncekiKm != null ? sinirlar.oncekiKm + 0.01 : 0;
        const max = sinirlar.sonrakiKm != null ? sinirlar.sonrakiKm - 0.01 : 99999999.99;

        if (min > max) {
          this.statusText.set(
            'Bu tarih için geçerli bir km aralığı yok (önceki/sonraki okumalar birbirine çok yakın).'
          );
          return;
        }

        this.kmMin.set(min);
        this.kmMax.set(max);
        this.wizardKm = Math.round(min * 100) / 100;
        this.wizardHiz = 0;

        const oncekiMetin = sinirlar.oncekiTarih
          ? `Önceki: ${this.formatTarih(sinirlar.oncekiTarih)} → ${sinirlar.oncekiKm?.toFixed(2)} km`
          : 'Önceki: yok (bu, ilk kayıt olacak)';
        const sonrakiMetin = sinirlar.sonrakiTarih
          ? `Sonraki: ${this.formatTarih(sinirlar.sonrakiTarih)} → ${sinirlar.sonrakiKm?.toFixed(2)} km`
          : 'Sonraki: yok (bu, son kayıt olacak)';
        this.sinirBilgisi.set(`${oncekiMetin}\n${sonrakiMetin}\nGirilecek km bu ikisinin arasında olmalı.`);

        this.tarihOnaylandi.set(true);
        this.ekleEtkin.set(true);
        this.statusText.set('Hız ve km sayacını girip Ekle\'ye basabilirsin.');
      },
      error: (err) => {
        if (buSurum !== this.sinirSorgusuSurumu) {
          return;
        }
        this.sinirlarHesaplaniyor.set(false);
        this.statusText.set('Sınırlar hesaplanamadı.');
        alert(`Sınırlar hesaplanamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  ekle(): void {
    const arac = this.plakalar().find((p) => p.aracId === this.wizardAracId);
    if (!arac) {
      return;
    }
    if (this.wizardKm < this.kmMin() || this.wizardKm > this.kmMax()) {
      alert(`Km sayacı ${this.kmMin().toFixed(2)} ile ${this.kmMax().toFixed(2)} arasında olmalı.`);
      return;
    }

    this.ekleniyor.set(true);
    this.statusText.set('Ekleniyor...');
    this.api
      .createHareket({
        aracId: arac.aracId,
        aracPlaka: arac.aracPlaka,
        veriTarihi: this.wizardTarih,
        hiz: this.wizardHiz,
        kmSayaci: this.wizardKm,
      })
      .subscribe({
        next: () => {
          this.statusText.set('Kayıt eklendi.');
          this.wizardAracId = null;
          this.sonrakiAdimlariSifirla();
          this.ekleniyor.set(false);
          this.refreshGrid();
        },
        error: (err) => {
          this.statusText.set('Ekleme başarısız.');
          alert(`Kayıt eklenemedi.\n\nHata: ${err.message}`);
          this.ekleniyor.set(false);
        },
      });
  }

  // ---------- Filtre şeridi (yerelde, bellekte - API'ye tekrar gitmez) ----------

  private filtrePlakaListesiniDoldur(hareketler: AracHareketDto[]): void {
    const plakalar = [...new Set(hareketler.map((h) => h.aracPlaka))].sort();
    this.filtrePlakaListesi.set([FILTRE_TUMU, ...plakalar]);
  }

  filtreleUygula(): void {
    this.filtreHatasi.set('');
    let sonuc = this.tumHareketler();

    if (this.filtrePlaka !== FILTRE_TUMU) {
      sonuc = sonuc.filter((h) => h.aracPlaka === this.filtrePlaka);
    }

    if (this.filtreTarihAktif) {
      sonuc = sonuc.filter((h) => h.veriTarihi === this.filtreTarih);
    }

    if (this.filtreHiz.trim() !== '') {
      const hiz = Number(this.filtreHiz.trim());
      if (!Number.isInteger(hiz)) {
        this.filtreHatasi.set('Hız filtresi geçerli bir tam sayı olmalı.');
        return;
      }
      sonuc = sonuc.filter((h) => h.hiz === hiz);
    }

    if (this.filtreKm.trim() !== '') {
      const km = Number(this.filtreKm.trim());
      if (Number.isNaN(km)) {
        this.filtreHatasi.set('Km sayacı filtresi geçerli bir sayı olmalı.');
        return;
      }
      sonuc = sonuc.filter((h) => h.kmSayaci === km);
    }

    this.gosterilenHareketler.set(sonuc);
    this.suankiSayfa.set(1);
    this.statusText.set(`${sonuc.length} / ${this.tumHareketler().length} kayıt gösteriliyor (filtreli).`);
  }

  filtreleTemizle(): void {
    this.filtrePlaka = FILTRE_TUMU;
    this.filtreTarihAktif = false;
    this.filtreHiz = '';
    this.filtreKm = '';
    this.filtreHatasi.set('');
    this.gosterilenHareketler.set(this.tumHareketler());
    this.suankiSayfa.set(1);
    this.statusText.set(`${this.tumHareketler().length} hareket kaydı yüklendi.`);
  }

  // ---------- Sayfalama ----------

  sayfaBoyutuDegisti(): void {
    this.suankiSayfa.set(1);
  }

  oncekiSayfa(): void {
    if (this.suankiSayfa() > 1) {
      this.suankiSayfa.update((s) => s - 1);
    }
  }

  sonrakiSayfa(): void {
    if (this.suankiSayfa() < this.toplamSayfa()) {
      this.suankiSayfa.update((s) => s + 1);
    }
  }

  // ---------- Excel'e Aktar ----------

  excelAktar(): void {
    const satirlar = this.gosterilenHareketler();
    if (satirlar.length === 0) {
      alert('Aktarılacak kayıt yok.');
      return;
    }
    this.disaAktariliyor.set(true);
    this.api.exportHareketler(satirlar).subscribe({
      next: (blob) => {
        dosyaIndir(blob, 'arac-hareketleri.xlsx');
        this.disaAktariliyor.set(false);
      },
      error: (err) => {
        this.disaAktariliyor.set(false);
        alert(`Excel'e aktarılamadı.\n\nHata: ${err.message}`);
      },
    });
  }

  formatTarih(iso: string): string {
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  }
}

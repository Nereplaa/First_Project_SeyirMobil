import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AracHareketApi } from '../../services/arac-hareket-api';
import {
  AracPlakaLookupDto,
  AracRaporSonucuDto,
  AracHareketDetayRaporSatiriDto,
} from '../../models/arac-hareket.models';
import { dosyaIndir } from '../../utils/dosya-indir';

function bugunIso(): string {
  const d = new Date();
  const yerelGun = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
  return yerelGun.toISOString().slice(0, 10);
}

function yarinIso(iso: string): string {
  const d = new Date(iso + 'T00:00:00');
  d.setDate(d.getDate() + 1);
  return d.toISOString().slice(0, 10);
}

@Component({
  selector: 'app-rapor',
  imports: [FormsModule],
  templateUrl: './rapor.html',
  styleUrl: './rapor.css',
})
export class Rapor implements OnInit {
  private readonly api = inject(AracHareketApi);

  plakalar = signal<AracPlakaLookupDto[]>([]);
  seciliPlakalar = signal<string[]>([]);

  // ---------- Plaka lookup (arama kutusu + açılır öneri listesi) ----------
  plakaArama = signal('');
  lookupAcik = signal(false);

  lookupSonuclari = computed(() => {
    const secili = new Set(this.seciliPlakalar());
    const arama = this.plakaArama().trim().toLocaleUpperCase('tr-TR');
    return this.plakalar()
      .filter((p) => !secili.has(p.aracPlaka))
      .filter((p) => arama === '' || p.aracPlaka.toLocaleUpperCase('tr-TR').includes(arama));
  });

  baslangic = bugunIso();
  bitis = yarinIso(bugunIso());
  detayliRapor = false;

  ozetSonuclar = signal<AracRaporSonucuDto[]>([]);
  detaySonuclar = signal<AracHareketDetayRaporSatiriDto[]>([]);
  raporUretildi = signal(false);
  yukleniyor = signal(false);
  statusText = signal('');

  // ---------- Excel'e Aktar ----------
  exportModu: 'ayri' | 'tumu' = 'ayri';
  disaAktariliyor = signal(false);

  raporOlusturEtkin = computed(() => this.seciliPlakalar().length > 0 && this.bitis > this.baslangic);

  ngOnInit(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => alert(`Araç listesi alınamadı.\n\nHata: ${err.message}`),
    });
  }

  onBaslangicDegisti(): void {
    const minBitis = yarinIso(this.baslangic);
    if (this.bitis < minBitis) {
      this.bitis = minBitis;
    }
  }

  lookupAc(): void {
    this.lookupAcik.set(true);
  }

  lookupKapat(): void {
    // Bir liste öğesine tıklanırken de blur tetiklenir - tıklamanın (mousedown) önce
    // işlenebilmesi için kapatmayı bir sonraki event döngüsüne erteliyoruz.
    setTimeout(() => this.lookupAcik.set(false), 150);
  }

  plakaSec(plaka: string): void {
    if (!this.seciliPlakalar().includes(plaka)) {
      this.seciliPlakalar.set([...this.seciliPlakalar(), plaka]);
    }
    this.plakaArama.set('');
  }

  plakaKaldir(plaka: string): void {
    this.seciliPlakalar.set(this.seciliPlakalar().filter((p) => p !== plaka));
  }

  raporOlustur(): void {
    const plakalar = this.seciliPlakalar();
    this.yukleniyor.set(true);
    this.statusText.set('Rapor oluşturuluyor...');
    this.raporUretildi.set(false);

    if (this.detayliRapor) {
      this.api
        .getDetayRaporu({ plakalar, baslangic: this.baslangic, bitis: this.bitis })
        .subscribe({
          next: (satirlar) => {
            this.detaySonuclar.set(satirlar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(`${satirlar.length} okuma için detaylı rapor oluşturuldu.`);
          },
          error: (err) => this.raporHatasi(err),
        });
    } else {
      this.api
        .getRaporToplu({ plakalar, baslangic: this.baslangic, bitis: this.bitis })
        .subscribe({
          next: (sonuclar) => {
            this.ozetSonuclar.set(sonuclar);
            this.raporUretildi.set(true);
            this.yukleniyor.set(false);
            this.statusText.set(`${sonuclar.length} araç için rapor oluşturuldu.`);
          },
          error: (err) => this.raporHatasi(err),
        });
    }
  }

  private raporHatasi(err: any): void {
    this.yukleniyor.set(false);
    this.statusText.set('Rapor oluşturulamadı.');
    alert(`Rapor oluşturulamadı.\n\nHata: ${err.message}`);
  }

  excelAktar(): void {
    if (!this.raporOlusturEtkin()) {
      return;
    }
    this.disaAktariliyor.set(true);
    this.statusText.set('Excel oluşturuluyor...');
    this.api
      .exportRapor({
        plakalar: this.seciliPlakalar(),
        baslangic: this.baslangic,
        bitis: this.bitis,
        detayliMi: this.detayliRapor,
        ayriPlakaBazliMi: this.exportModu === 'ayri',
      })
      .subscribe({
        next: (blob) => {
          const modAdi = this.detayliRapor ? 'detayli' : 'ozet';
          dosyaIndir(blob, `rapor-${modAdi}.xlsx`);
          this.disaAktariliyor.set(false);
          this.statusText.set('Excel dosyası indirildi.');
        },
        error: (err) => {
          this.disaAktariliyor.set(false);
          this.statusText.set('Excel\'e aktarılamadı.');
          alert(`Excel'e aktarılamadı.\n\nHata: ${err.message}`);
        },
      });
  }

  formatTarih(iso: string | null): string {
    if (!iso) {
      return '-';
    }
    const [y, m, d] = iso.split('-');
    return `${d}.${m}.${y}`;
  }

  formatKm(km: number | null): string {
    return km == null ? '-' : km.toFixed(2);
  }
}

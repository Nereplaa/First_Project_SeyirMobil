import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  DxDateRangeBoxModule,
  DxTagBoxModule,
  DxCheckBoxModule,
  DxSelectBoxModule,
  DxButtonModule,
} from 'devextreme-angular';
import { AracHareketApi } from '../../services/arac-hareket-api';
import {
  AracPlakaLookupDto,
  AracRaporSonucuDto,
  AracHareketDetayRaporSatiriDto,
} from '../../models/arac-hareket.models';
import { dosyaIndir } from '../../utils/dosya-indir';
import { oturumHatasiMi } from '../../utils/hata-yardimcisi';

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

// DevExtreme DateRangeBox Date nesneleriyle calisiyor, backend/rapor mantigi ise ISO tarih
// string'i ("yyyy-MM-dd") bekliyor - bu iki yardimci fonksiyon aradaki donusumu yapiyor.
// getFullYear/getMonth/getDate KULLANILIYOR (toISOString DEGIL) - toISOString once UTC'ye
// cevirir, yerel saat diliminde gece yarisina yakin secimlerde bir gun kayabilirdi.
function tarihToIso(d: Date): string {
  const y = d.getFullYear();
  const ay = String(d.getMonth() + 1).padStart(2, '0');
  const gun = String(d.getDate()).padStart(2, '0');
  return `${y}-${ay}-${gun}`;
}

function isoToTarih(iso: string): Date {
  return new Date(iso + 'T00:00:00');
}

@Component({
  selector: 'app-rapor',
  imports: [DxDateRangeBoxModule, DxTagBoxModule, DxCheckBoxModule, DxSelectBoxModule, DxButtonModule],
  templateUrl: './rapor.html',
  styleUrl: './rapor.css',
})
export class Rapor implements OnInit {
  private readonly api = inject(AracHareketApi);

  plakalar = signal<AracPlakaLookupDto[]>([]);
  // Plaka secimi artik dx-tag-box'in KENDI arama+cip mekanizmasi ile yapiliyor - onceki elle
  // yazilmis arama kutusu/acilir liste/cip listesi (plakaArama, lookupAcik, lookupSonuclari,
  // lookupAc/Kapat, plakaSec/Kaldir) tamamen kaldirildi, DevExtreme zaten ayni ozelligi
  // (aranabilir coklu-secim + kaldirilabilir cipler) hazir sunuyor.
  seciliPlakalar = signal<string[]>([]);

  seciliPlakalarDegisti(event: { value?: string[] | null }): void {
    this.seciliPlakalar.set(event.value ?? []);
  }

  baslangic = bugunIso();
  bitis = yarinIso(bugunIso());
  detayliRapor = false;

  // dx-date-range-box'a baglanan Date alanlari - asil "kaynak" hala baslangic/bitis (ISO
  // string) alanlari, rapor mantigi/export/backend cagrilari hepsi bu string'leri kullaniyor.
  // BILINCLI OLARAK getter/setter DEGIL, sabit alan: bir getter her change-detection turunda
  // "new Date(...)" ile YENI bir nesne dondurseydi, DevExtreme bunu "deger degisti" sanip
  // widget'i surekli yeniden baslatiyordu (gercek bug, ekranda ust uste yigilan onlarca takvim
  // olarak ortaya cikti) - sabit alan + degisiklikleri SADECE kullanici etkilesiminde (event
  // handler'larda) guncelleme, referans kararliligini koruyor.
  baslangicDate: Date = isoToTarih(this.baslangic);
  bitisDate: Date = isoToTarih(this.bitis);

  baslangicDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.baslangicDate = tarih;
    this.baslangic = tarihToIso(tarih);
    this.onBaslangicDegisti();
  }

  bitisDateDegisti(deger: string | number | Date | null): void {
    if (deger == null) {
      return;
    }
    const tarih = new Date(deger);
    this.bitisDate = tarih;
    this.bitis = tarihToIso(tarih);
  }

  ozetSonuclar = signal<AracRaporSonucuDto[]>([]);
  detaySonuclar = signal<AracHareketDetayRaporSatiriDto[]>([]);
  raporUretildi = signal(false);
  yukleniyor = signal(false);
  statusText = signal('');

  // ---------- Excel'e Aktar ----------
  readonly exportModuSecenekleri = [
    { deger: 'ayri' as const, metin: 'Her plaka için ayrı bölüm' },
    { deger: 'tumu' as const, metin: 'Tüm plakalar tek tabloda' },
  ];
  exportModu: 'ayri' | 'tumu' = 'ayri';
  disaAktariliyor = signal(false);

  raporOlusturEtkin = computed(() => this.seciliPlakalar().length > 0 && this.bitis > this.baslangic);

  ngOnInit(): void {
    this.api.getPlakalar().subscribe({
      next: (plakalar) => this.plakalar.set(plakalar),
      error: (err) => {
        if (!oturumHatasiMi(err)) {
          alert(`Araç listesi alınamadı.\n\nHata: ${err.message}`);
        }
      },
    });
  }

  onBaslangicDegisti(): void {
    const minBitis = yarinIso(this.baslangic);
    if (this.bitis < minBitis) {
      this.bitis = minBitis;
      this.bitisDate = isoToTarih(this.bitis);
    }
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
    if (oturumHatasiMi(err)) {
      return;
    }
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
          if (oturumHatasiMi(err)) {
            return;
          }
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

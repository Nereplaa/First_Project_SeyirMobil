export interface AracHareketDto {
  id: number;
  aracId: number;
  aracPlaka: string;
  veriTarihi: string; // yyyy-MM-dd
  hiz: number;
  kmSayaci: number;
}

export interface AracPlakaLookupDto {
  aracId: number;
  aracPlaka: string;
}

export interface AracHareketSinirlarDto {
  ayniTarihVarMi: boolean;
  oncekiTarih: string | null;
  oncekiKm: number | null;
  sonrakiTarih: string | null;
  sonrakiKm: number | null;
}

export interface CreateAracHareketRequestDto {
  aracId: number;
  aracPlaka: string;
  veriTarihi: string;
  hiz: number;
  kmSayaci: number;
}

export interface RaporTopluRequestDto {
  plakalar: string[];
  baslangic: string;
  bitis: string;
}

export interface AracRaporSonucuDto {
  aracPlaka: string;
  bulunduMu: boolean;
  baslangicTarihi: string | null;
  baslangicKm: number | null;
  bitisTarihi: string | null;
  bitisKm: number | null;
  yapilanKm: number | null;
}

export interface AracHareketDetayRaporSatiriDto {
  aracPlaka: string;
  veriTarihi: string;
  kmSayaci: number;
  artis: number | null;
}


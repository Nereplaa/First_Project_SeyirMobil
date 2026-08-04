import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AracHareketDto,
  AracPlakaLookupDto,
  AracHareketSinirlarDto,
  CreateAracHareketRequestDto,
  RaporTopluRequestDto,
  AracRaporSonucuDto,
  AracHareketDetayRaporSatiriDto,
  RaporExportRequestDto,
} from '../models/arac-hareket.models';

const API_BASE = 'http://localhost:5080/api/arac-hareketleri';

@Service()
export class AracHareketApi {
  private readonly http = inject(HttpClient);

  getTumHareketler(): Observable<AracHareketDto[]> {
    return this.http.get<AracHareketDto[]>(API_BASE);
  }

  getPlakalar(): Observable<AracPlakaLookupDto[]> {
    return this.http.get<AracPlakaLookupDto[]>(`${API_BASE}/plakalar`);
  }

  getSinirlar(plaka: string, tarih: string): Observable<AracHareketSinirlarDto> {
    const params = { plaka, tarih };
    return this.http.get<AracHareketSinirlarDto>(`${API_BASE}/sinirlar`, { params });
  }

  createHareket(request: CreateAracHareketRequestDto): Observable<AracHareketDto> {
    return this.http.post<AracHareketDto>(API_BASE, request);
  }

  deleteHareket(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/${id}`);
  }

  getRaporToplu(request: RaporTopluRequestDto): Observable<AracRaporSonucuDto[]> {
    return this.http.post<AracRaporSonucuDto[]>(`${API_BASE}/rapor-toplu`, request);
  }

  getDetayRaporu(request: RaporTopluRequestDto): Observable<AracHareketDetayRaporSatiriDto[]> {
    return this.http.post<AracHareketDetayRaporSatiriDto[]>(`${API_BASE}/rapor-detay`, request);
  }

  exportHareketler(hareketler: AracHareketDto[]): Observable<Blob> {
    return this.http.post(`${API_BASE}/export`, hareketler, { responseType: 'blob' });
  }

  exportRapor(request: RaporExportRequestDto): Observable<Blob> {
    return this.http.post(`${API_BASE}/rapor-export`, request, { responseType: 'blob' });
  }
}

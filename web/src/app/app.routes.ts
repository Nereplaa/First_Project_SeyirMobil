import { Routes } from '@angular/router';
import { Liste } from './pages/liste/liste';
import { Rapor } from './pages/rapor/rapor';

export const routes: Routes = [
  { path: '', component: Liste },
  { path: 'rapor', component: Rapor },
  { path: '**', redirectTo: '' },
];

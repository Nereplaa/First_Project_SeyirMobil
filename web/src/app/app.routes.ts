import { Routes } from '@angular/router';
import { Liste } from './pages/liste/liste';
import { Rapor } from './pages/rapor/rapor';
import { Login } from './pages/login/login';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', component: Liste, canActivate: [authGuard] },
  { path: 'rapor', component: Rapor, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];

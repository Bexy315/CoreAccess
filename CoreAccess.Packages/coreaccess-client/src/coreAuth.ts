import type { CoreAccessConfig } from './types';
import {
    LocalStorageTokenStorage,
    type TokenStorage
} from './core/tokenStorage';
import httpClient from './core/httpClient';

type AuthChangeCallback = (isAuthenticated: boolean) => void;
type VoidCallback = () => void;

class CoreAuth {
    private isAuthenticated = false;
    private user: any = null;
    private config?: CoreAccessConfig;

    private tokenStorage: TokenStorage = new LocalStorageTokenStorage();

    private loginListeners = new Set<VoidCallback>();
    private logoutListeners = new Set<VoidCallback>();
    private authChangeListeners = new Set<AuthChangeCallback>();

    configure(config: CoreAccessConfig) {
        this.config = config;
        httpClient.defaults.baseURL = config.baseUrl;

        this.restoreFromStorage();
    }

    restoreFromStorage() {
        const access = this.tokenStorage.getAccessToken();

        if (access) {
            this.isAuthenticated = true;
            this.user = localStorage.getItem('coreaccess_userId') ?? '';

            this.notifyLogin();
        } else {
            this.isAuthenticated = false;
            this.user = null;

            this.notifyLogout();
        }
    }

    onLogin(callback: VoidCallback) {
        this.loginListeners.add(callback);
    }

    onLogout(callback: VoidCallback) {
        this.logoutListeners.add(callback);
    }

    onAuthChange(callback: AuthChangeCallback) {
        this.authChangeListeners.add(callback);
    }

    private notifyLogin() {
        this.loginListeners.forEach(cb => cb());
        this.authChangeListeners.forEach(cb => cb(true));
    }

    private notifyLogout() {
        this.logoutListeners.forEach(cb => cb());
        this.authChangeListeners.forEach(cb => cb(false));
    }

    async login(credentials: { username: string; password: string }) {
        try {
            const formData = new URLSearchParams();
            formData.append('grant_type', 'password');
            formData.append('username', credentials.username);
            formData.append('password', credentials.password);
            formData.append('scope', 'openid offline_access');

            const response = await httpClient.post('/connect/token', formData, {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            });

            const data = response.data;

            if (!data?.access_token || !data?.refresh_token) {
                throw new Error('Invalid response: tokens missing from server');
            }

            this.tokenStorage.setTokens(data.access_token, data.refresh_token);

            // Wenn du sub oder andere Claims im Token parst
            const payload = this.decodeJwt(data.access_token);
            this.user = payload.sub || '';
            localStorage.setItem('coreaccess_userId', this.user);

            this.isAuthenticated = true;
            this.notifyLogin();
        } catch (error: any) {
            this.tokenStorage.clearTokens();
            this.user = null;
            localStorage.removeItem('coreaccess_userId');
            this.isAuthenticated = false;

            this.notifyLogout();

            const msg = error.response?.data?.error_description || error.message || 'Login failed';
            throw new Error(msg);
        }
    }

    logout() {
        this.tokenStorage.clearTokens();

        this.isAuthenticated = false;
        this.user = null;

        this.notifyLogout();
    }

    isLoggedIn(): boolean {
        return this.isAuthenticated;
    }

    getCurrentUser(): any | null {
        return this.user;
    }

    getAccessToken(): string | null {
        return this.tokenStorage.getAccessToken();
    }

    async refreshAccessToken(): Promise<string | null> {
        try {
            const refreshToken = this.tokenStorage.getRefreshToken();
            if (!refreshToken) throw new Error('No refresh token found');

            const formData = new URLSearchParams();
            formData.append('grant_type', 'refresh_token');
            formData.append('refresh_token', refreshToken);

            const response = await httpClient.post('/connect/token', formData, {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            });

            const data = response.data;
            if (!data?.access_token || !data?.refresh_token) {
                throw new Error('Invalid response from token endpoint');
            }

            this.tokenStorage.setTokens(data.access_token, data.refresh_token);

            // Optional: sub erneut auslesen
            const payload = this.decodeJwt(data.access_token);
            this.user = payload.sub || '';
            localStorage.setItem('coreaccess_userId', this.user);

            return data.access_token;
        } catch (error) {
            this.tokenStorage.clearTokens();
            this.isAuthenticated = false;
            this.user = null;
            this.notifyLogout();
            throw error;
        }
    }

    private decodeJwt(token: string): any {
        try {
            const payload = token.split('.')[1];
            const decoded = atob(payload);
            return JSON.parse(decoded);
        } catch {
            return {};
        }
    }
}

export const coreAuth = new CoreAuth();

import type { CoreAccessConfig } from './types';
import {
    LocalStorageTokenStorage,
    type TokenStorage
} from './core/tokenStorage';

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

        this.restoreFromStorage();

        console.debug('[CoreAccess] Configured', config);
    }

    restoreFromStorage() {
        const access = this.tokenStorage.getAccessToken();

        if (access) {
            this.isAuthenticated = true;

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
        // TODO: Ersetze durch HTTP-Aufruf
        const fakeAccessToken = 'mock_access_token';
        const fakeRefreshToken = 'mock_refresh_token';
        const fakeUser = { username: credentials.username };

        this.tokenStorage.setTokens(fakeAccessToken, fakeRefreshToken);

        this.isAuthenticated = true;
        this.user = fakeUser;

        this.notifyLogin();
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
}

export const coreAuth = new CoreAuth();

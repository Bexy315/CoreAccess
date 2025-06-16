export interface TokenStorage {
    getAccessToken(): string | null;
    getRefreshToken(): string | null;
    setTokens(accessToken: string, refreshToken: string): void;
    clearTokens(): void;
}

export class LocalStorageTokenStorage implements TokenStorage {
    private accessKey = 'coreaccess_accessToken';
    private refreshKey = 'coreaccess_refreshToken';

    getAccessToken(): string | null {
        return localStorage.getItem(this.accessKey);
    }

    getRefreshToken(): string | null {
        return localStorage.getItem(this.refreshKey);
    }

    setTokens(accessToken: string, refreshToken: string): void {
        localStorage.setItem(this.accessKey, accessToken);
        localStorage.setItem(this.refreshKey, refreshToken);
    }

    clearTokens(): void {
        localStorage.removeItem(this.accessKey);
        localStorage.removeItem(this.refreshKey);
    }
}

let tokenStorage: TokenStorage = new LocalStorageTokenStorage(); // default

export function getTokenStorage(): TokenStorage {
    return tokenStorage;
}
export interface CoreAccessConfig {
    baseUrl: string;
    storage?: 'localStorage' | 'sessionStorage';
    onLogout?: () => void;
}
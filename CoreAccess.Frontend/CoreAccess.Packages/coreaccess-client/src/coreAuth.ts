import type { CoreAccessConfig } from './types';

interface CoreAuthState {
    isAuthenticated: boolean;
    user: any | null;  //TODO: Define a proper user type
    config?: CoreAccessConfig;
}

class CoreAuth {
    private state: CoreAuthState = {
        isAuthenticated: false,
        user: null
    };

    configure(config: CoreAccessConfig) {
        this.state.config = config;
        console.debug('[CoreAccess] Configured', config);
    }

    async login(credentials: { username: string; password: string }) {
        // TODO: Login-Logik mit HTTP
        console.debug('[CoreAccess] login() called', credentials);
        this.state.isAuthenticated = true;
        this.state.user = { username: credentials.username };
    }

    logout() {
        console.debug('[CoreAccess] logout() called');
        // TODO: Logout-Logik mit HTTP
        this.state.isAuthenticated = false;
        this.state.user = null;

        this.state.config?.onLogout?.();
    }

    isAuthenticated(): boolean {
        return this.state.isAuthenticated;
    }

    getCurrentUser(): any | null {
        return this.state.user;
    }
}

export const coreAuth = new CoreAuth();

import axios from "axios";
import {ref} from "vue";

interface TokenSet {
    access_token: string;
    refresh_token?: string;
    expires_at: number;
}

const config = {
    authBaseUrl: import.meta.env.VITE_AUTH_BASE_URL,
    clientId: import.meta.env.VITE_AUTH_CLIENT_ID,
    redirectUri: import.meta.env.VITE_AUTH_REDIRECT_URI,
    postLogoutRedirectUri: import.meta.env.VITE_AUTH_POST_LOGOUT_REDIRECT_URI,
    scopes: import.meta.env.VITE_AUTH_SCOPES,
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
};

export function getTokens(): TokenSet | null {
    const raw = localStorage.getItem("tokens");
    if (!raw) return null;
    return JSON.parse(raw);
}

function saveTokens(tokens: any) {
    const expiresIn = tokens.expires_in || 3600;
    const expiresAt = Date.now() + expiresIn * 1000;
    localStorage.setItem(
        "tokens",
        JSON.stringify({
            ...tokens,
            expires_at: expiresAt,
        })
    );
}

export function login(state: string = "") {
    const url = new URL(config.authBaseUrl + "/connect/authorize");
    url.searchParams.set("client_id", config.clientId);
    url.searchParams.set("redirect_uri", config.redirectUri);
    url.searchParams.set("response_type", "code");
    url.searchParams.set("scope", config.scopes);
    url.searchParams.set("state", state);
    window.location.href = url.toString();
}

export async function handleCallback(code: string) {
    const res = await axios.post(
        config.authBaseUrl + "/connect/token",
        new URLSearchParams({
            grant_type: "authorization_code",
            code,
            redirect_uri: config.redirectUri,
            client_id: config.clientId,
        }),
        { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
    );
    saveTokens(res.data);
}

export async function getAccessToken(): Promise<string | null> {
    let tokens = getTokens();
    if (!tokens) return null;

    if (Date.now() > tokens.expires_at) {
        // refresh
        if (!tokens.refresh_token) return null;
        const res = await axios.post(
            config.authBaseUrl + "/connect/token",
            new URLSearchParams({
                grant_type: "refresh_token",
                refresh_token: tokens.refresh_token,
                client_id: config.clientId,
            }),
            { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
        );
        saveTokens(res.data);
        tokens = getTokens();
    }
    return tokens?.access_token || null;
}

export async function refreshToken(): Promise<string | null> {
    const tokens = getTokens();
    if (!tokens?.refresh_token) return null;

        const res = await axios.post(
            config.authBaseUrl + "/connect/token",
            new URLSearchParams({
                grant_type: "refresh_token",
                refresh_token: tokens.refresh_token,
                client_id: config.clientId,
            }),
            { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
        );

        saveTokens(res.data);
        const newTokens = getTokens();
        return newTokens?.access_token || null;

}


export async function getUserInfo() {
    const token = await getAccessToken();
    if (!token) throw new Error("Not authenticated");
    const res = await axios.get(config.authBaseUrl + "/connect/userinfo", {
        headers: { Authorization: `Bearer ${token}` },
    });
    return res.data;
}

export function isAuthenticated(): boolean {
    const token = getTokens()
    isAuthenticatedRef.value = !!token
    return !!token;
}

export const isAuthenticatedRef = ref(false)

export function logout() {

    localStorage.removeItem("tokens");

    const url = new URL(config.authBaseUrl + "/connect/endsession");

    url.searchParams.set("client_id", config.clientId);
    url.searchParams.set("post_logout_redirect_uri", config.postLogoutRedirectUri);

    window.location.href = url.toString();
}

import axios, { AxiosInstance } from 'axios';
import { LoginRequest, LoginResponse, RegisterRequest } from './models';

export class CoreAccessClient {
    private axios: AxiosInstance;

    constructor(baseUrl: string) {
        this.axios = axios.create({
            baseURL: baseUrl,
            headers: { 'Content-Type': 'application/json' }
        });
    }

    async login(payload: LoginRequest): Promise<LoginResponse> {
        const response = await this.axios.post<LoginResponse>('/auth/login', payload);
        return response.data;
    }

    async register(payload: RegisterRequest): Promise<void> {
        await this.axios.post('/auth/register', payload);
    }

    async logout(): Promise<void> {
        await this.axios.post('/auth/logout');
    }

    // z. B. getCurrentUser, refreshToken etc. kannst du leicht ergänzen
}

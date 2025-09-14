import axios from 'axios';
import qs from 'qs';
import {getTokens, logout, refreshToken} from "./auth.ts";

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
    timeout: 5000,
    headers: {
        'Content-Type': 'application/json',
    },
    paramsSerializer: (params) => {
        return qs.stringify(params, { arrayFormat: 'repeat' })
    }
});

apiClient.interceptors.request.use(
    (config) => {
        const token = getTokens()?.access_token;

        if (token && token !== '') {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

apiClient.interceptors.response.use(
    (response: any): any => response,
    async (error: any): Promise<any> => {
        const originalRequest = error.config as any & { _retry?: boolean }

        if (
            error.response?.status === 401 &&
            !originalRequest._retry
        ) {
            originalRequest._retry = true
            try {
                const refreshResponse = await refreshToken();

                if (originalRequest.headers)
                    originalRequest.headers.Authorization = `Bearer ${refreshResponse}`;

                return apiClient(originalRequest)
            } catch (refreshError) {
                logout();
                return Promise.reject(refreshError)
            }
        }

        return Promise.reject(error)
    }
)

export default apiClient;

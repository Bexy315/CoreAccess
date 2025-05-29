import axios from 'axios'
import { getTokenStorage } from './tokenStorage'

const httpClient: any = axios.create({
    baseURL: '',
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
})

// === Request Interceptor: Access Token hinzufügen ===
httpClient.interceptors.request.use(
    (config: any): any => {
        const token = getTokenStorage().getAccessToken()
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error: any): Promise<any> => {
        return Promise.reject(error)
    }
)

// === Response Interceptor: Refresh bei 401 ===
httpClient.interceptors.response.use(
    (response: any): any => response,
    async (error: any): Promise<any> => {
        const originalRequest = error.config as any & { _retry?: boolean }

        if (
            error.response?.status === 401 &&
            !originalRequest._retry &&
            getTokenStorage().getRefreshToken()
        ) {
            originalRequest._retry = true
            try {
                const refreshResponse = await axios.post(
                    '/auth/refresh-token',
                    {
                        refreshToken: getTokenStorage().getRefreshToken(),
                    },
                    {
                        baseURL: httpClient.defaults.baseURL,
                    }
                )

                const newAccessToken = (refreshResponse.data as any).accessToken
                const newRefreshToken = (refreshResponse.data as any).refreshToken

                getTokenStorage().setTokens(newAccessToken, newRefreshToken)

                if (originalRequest.headers)
                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`

                return httpClient(originalRequest)
            } catch (refreshError) {
                getTokenStorage().clearTokens()
                return Promise.reject(refreshError)
            }
        }

        return Promise.reject(error)
    }
)

export default httpClient
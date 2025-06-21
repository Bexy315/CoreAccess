import apiClient from "./apiClient";


export function getAppConfig() {
    return apiClient.get('/admin/config');
}
import apiClient from "./apiClient.ts";

export interface HealthStatus {
    status: string;
    uptime: number;
    checks: Record<string, string>;
}

export async function getHealthStatus(): Promise<HealthStatus> {
    const response = await apiClient.get<HealthStatus>('/system/health');
    return response.data;
}

export async function getDashboardMetrics(): Promise<any> {
   // TODO: Implement user metrics backend
    const response = await apiClient.get('/metrics/dashboard');
    return response.data;
}
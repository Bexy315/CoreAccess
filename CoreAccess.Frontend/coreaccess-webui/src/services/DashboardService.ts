import apiClient from "./apiClient.ts";

export interface HealthStatus {
    status: string;
    uptime: string;
    checks: Record<string, string>;
}

export async function getHealthStatus(): Promise<HealthStatus> {
    const response = await apiClient.get<HealthStatus>('/system/health');
    return response.data;
}
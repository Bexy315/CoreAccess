import apiClient from "./apiClient.ts";

export async function getApplications(opts: { page: number; pageSize: number; search?: string; } ): Promise<any> {
    const response = await apiClient.get<string>('/applications', { params: opts });
    return response.data;
}

export async function getApplication(id: string): Promise<any> {
    const response = await apiClient.get<string>('/applications/' + id);
    return response.data;
}
import apiClient from "./apiClient.ts";
import type {ApplicationDetailDto, ApplicationDto, ApplicationUpdateRequest} from "../model/ApplicationModel.ts";
import type {PagedResult} from "../model/CommonModel.ts";

export async function getApplications(opts: { page: number; pageSize: number; search?: string; } ): Promise<PagedResult<ApplicationDto>> {
    const response = await apiClient.get<PagedResult<ApplicationDto>>('/applications', { params: opts });
    return response.data;
}

export async function getApplication(id: string): Promise<ApplicationDetailDto> {
    const response = await apiClient.get<ApplicationDetailDto>('/applications/' + id);
    return response.data;
}

export async function updateApplication(id: string, request: ApplicationUpdateRequest): Promise<ApplicationDetailDto> {
    const response = await apiClient.put<ApplicationDetailDto>('/applications/' + id, request);
    return response.data;
}
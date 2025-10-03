import type {PermissionDetailDto, PermissionDto, PermissionSearchOptions} from "../model/CorePermissionModel.ts";
import type {PagedResult} from "../model/CommonModel.ts";
import apiClient from "./apiClient.ts";

export async function getPermissions(options: PermissionSearchOptions): Promise<PagedResult<PermissionDto>> {
    const response = await apiClient.get<PagedResult<PermissionDto>>('/permissions', { params: options });
    return response.data;
}

export async function fetchPermission(id: string, includeRoles: boolean = false): Promise<PermissionDetailDto> {
    const response = await apiClient.get<PermissionDetailDto>(`/permissions/${id}`, { params: { includeRoles } });
    return response.data;
}

export async function updatePermission(id: string, name: string, description: string): Promise<PermissionDetailDto> {
    const response = await apiClient.put<PermissionDetailDto>(`/permissions/${id}`, { name, description });
    return response.data;
}

export async function createPermission(name: string, description: string): Promise<PermissionDto> {
    const response = await apiClient.post<PermissionDto>('/permissions', { name, description });
    return response.data;
}

export async function deletePermission(id: string): Promise<void> {
    await apiClient.delete(`/permissions/${id}`);
}
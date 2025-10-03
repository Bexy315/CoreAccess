import apiClient from './apiClient';
import type {PagedResult} from '../model/CommonModel.ts';

import type {
    RoleDetailDto,
    RoleDto
} from '../model/CoreRoleModel.ts';

/**
 * Ruft alle Rollen ab (Admin-Only).
 * @returns Eine Liste aller Rollen.
 */
export async function getRoles(opts: { page: number; pageSize: number; search?: string; } ): Promise<PagedResult<RoleDto>> {
    const response = await apiClient.get<PagedResult<RoleDto>>('/role', { params: opts });
    return response.data;
}
export async function fetchRole(roleId: string, includeUsers: boolean = false, includePermissions: boolean = false): Promise<RoleDetailDto> {
    const response = await apiClient.get<RoleDetailDto>(`/role/${roleId}`, { params: { includeUsers, includePermissions } });
    return response.data;
}
export async function assignPermissionToRole(roleId: string, permissionId: string): Promise<any> {
    return await apiClient.post(`/role/${roleId}/permissions/${permissionId}`);
}

export async function removePermissionFromRole(roleId: string, permissionId: string): Promise<any> {
    return await apiClient.delete(`/role/${roleId}/permissions/${permissionId}`);
}

export async function deleteRole(roleId: string): Promise<any> {
    return await apiClient.delete(`/role/${roleId}`);
}
export async function createRole(roleName: string, description: string): Promise<RoleDetailDto> {
    const response = await apiClient.post<RoleDetailDto>('/role', { name: roleName, description: description });
    return response.data;
}
export async function updateRole(roleId: string, roleName: string): Promise<RoleDetailDto> {
    const response = await apiClient.put<RoleDetailDto>(`/role/${roleId}`, { name: roleName });
    return response.data;
}
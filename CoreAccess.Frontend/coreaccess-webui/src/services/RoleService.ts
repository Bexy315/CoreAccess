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
export async function fetchRoles(): Promise<PagedResult<RoleDto>> {
    const response = await apiClient.get<PagedResult<RoleDto>>('/role');
    return response.data;
}
export async function fetchRole(roleId: string): Promise<RoleDetailDto> {
    const response = await apiClient.get<RoleDetailDto>(`/role/${roleId}`);
    return response.data;
}
export async function deleteRole(roleId: string): Promise<any> {
    return await apiClient.delete(`/role/${roleId}`);
}
export async function createRole(roleName: string): Promise<RoleDetailDto> {
    const response = await apiClient.post<RoleDetailDto>('/role', { name: roleName });
    return response.data;
}
export async function updateRole(roleId: string, roleName: string): Promise<RoleDetailDto> {
    const response = await apiClient.put<RoleDetailDto>(`/role/${roleId}`, { name: roleName });
    return response.data;
}
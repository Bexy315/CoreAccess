import apiClient from './apiClient';
import type {PagedResult} from '../model/CommonModel.ts';
import type {
    CoreUserCreateRequest, UserDetailDto,
    UserDto,
    CoreUserSearchOptions,
    CoreUserUpdateRequest
} from '../model/CoreUserModel.ts';

/**
 * Ruft Benutzer anhand von Suchoptionen ab (Admin-Only).
 * @param options Suchkriterien wie Paging, Filter etc.
 * @returns Ein paginierter Satz an Benutzern.
 */
export async function fetchUsers(options: CoreUserSearchOptions): Promise<PagedResult<UserDto>> {
    const params: Record<string, any> = { ...options }

    if (options.status && options.status.length > 0) {
        params.status = options.status
    }

    const response = await apiClient.get<PagedResult<UserDetailDto>>('/user', { params })
    return response.data;
}

export async function fetchUser(id: string): Promise<UserDetailDto> {
    const response = await apiClient.get<UserDetailDto>('/user/' + id);

    return response.data;
}

export async function createUser(request: CoreUserCreateRequest): Promise<UserDetailDto> {
    const response = await apiClient.post<UserDetailDto>('/user', request);
    return response.data;
}

export async function updateUser(userId: string, request: CoreUserUpdateRequest): Promise<UserDetailDto>{
    const response = await apiClient.put<UserDetailDto>(`/user/${userId}`, request);
    return response.data;
}

export async function assignRoleToUser(userId: string, roleId: string): Promise<UserDetailDto> {
    const response = await apiClient.post<UserDetailDto>(`/user/${userId}/role/${roleId}`);
    return response.data;
}

export async function removeRoleFromUser(userId: string, roleId: string): Promise<UserDetailDto> {
    const response = await apiClient.delete<UserDetailDto>(`/user/${userId}/role/${roleId}`);
    return response.data;
}

export async function deleteUser(userId: string): Promise<any> {
    return await apiClient.delete(`/user/${userId}`);
}
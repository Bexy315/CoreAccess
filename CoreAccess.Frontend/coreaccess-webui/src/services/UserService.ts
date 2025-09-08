import apiClient from './apiClient';
import type {PagedResult} from '../model/CommonModel.ts';
import type {
    CoreUserCreateRequest,
    CoreUserDto,
    CoreUserSearchOptions,
    CoreUserUpdateRequest
} from '../model/CoreUserModel.ts';

/**
 * Ruft Benutzer anhand von Suchoptionen ab (Admin-Only).
 * @param options Suchkriterien wie Paging, Filter etc.
 * @returns Ein paginierter Satz an Benutzern.
 */
export async function fetchUsers(options: CoreUserSearchOptions): Promise<PagedResult<CoreUserDto>> {
    const response = await apiClient.get<PagedResult<CoreUserDto>>('/user', {
        params: options,
    });

    return response.data;
}

export async function createUser(request: CoreUserCreateRequest): Promise<CoreUserDto> {
    const response = await apiClient.post<CoreUserDto>('/user', request);
    return response.data;
}

export async function updateUser(userId: string, request: CoreUserUpdateRequest): Promise<CoreUserDto>{
    const response = await apiClient.put<CoreUserDto>(`/user/${userId}`, request);
    return response.data;
}

export async function deleteUser(userId: string): Promise<any> {
    return await apiClient.delete(`/user/${userId}`);
}
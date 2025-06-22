import apiClient from './apiClient';
import type { PagedResult } from '../model/CommonModel.ts';
import type { CoreUserDto, CoreUserSearchOptions } from '../model/CoreUserModel.ts';

/**
 * Ruft Benutzer anhand von Suchoptionen ab (Admin-Only).
 * @param options Suchkriterien wie Paging, Filter etc.
 * @returns Ein paginierter Satz an Benutzern.
 */
export async function fetchUsers(options: CoreUserSearchOptions): Promise<PagedResult<CoreUserDto>> {
    const response = await apiClient.get<PagedResult<CoreUserDto>>('/admin/user', {
        params: options,
    });

    return response.data;
}
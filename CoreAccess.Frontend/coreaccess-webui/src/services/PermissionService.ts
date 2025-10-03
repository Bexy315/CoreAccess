import type {PermissionDto, PermissionSearchOptions} from "../model/CorePermissionModel.ts";
import type {PagedResult} from "../model/CommonModel.ts";
import apiClient from "./apiClient.ts";

export async function getPermissions(options: PermissionSearchOptions): Promise<PagedResult<PermissionDto>> {
    const response = await apiClient.get<PagedResult<PermissionDto>>('/permissions', { params: options });
    return response.data;
}
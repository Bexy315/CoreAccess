import type {RoleDto} from "./CoreRoleModel.ts";

export interface PermissionDto {
    id: string;
    name: string;
    description?: string;
}

export interface PermissionDetailDto extends PermissionDto {
    createdAt: string;
    updatedAt: string;
    isSystem: boolean;
    roles: RoleDto[];
}

export interface PermissionSearchOptions {
    page: number;
    pageSize: number;
    search?: string;
}
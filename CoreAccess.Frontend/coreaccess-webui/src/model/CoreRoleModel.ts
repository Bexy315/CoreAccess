import type {PermissionDto} from './CorePermissionModel';
import type {UserDto} from "./CoreUserModel.ts";

export interface RoleDto {
    id: string;
    name: string;
    description?: string;
    isSystem: boolean;
}

export interface RoleDetailDto extends RoleDto {
    createdAt: string;
    updatedAt: string;
    permissions: PermissionDto[];
    users: UserDto[];
}

export interface UpdateRoleRequest {
    Name?: string;
    Description?: string;
}

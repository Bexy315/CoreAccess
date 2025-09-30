import type {PermissionDto} from './CorePermissionModel';
import type {UserDto} from "./CoreUserModel.ts";

export interface RoleDto {
    id: string;
    name: string;
    description?: string;
    permissions: PermissionDto[];
    users: UserDto[];
}

export interface RoleDetailDto extends RoleDto {
    createdAt: string;
    updatedAt: string;
    isSystem: boolean;
}

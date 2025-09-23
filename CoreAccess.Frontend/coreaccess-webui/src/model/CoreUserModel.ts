import type {RoleDto} from "./CoreRoleModel.ts";

export const CoreUserStatus = {
    Active: 0,
    Inactive: 1,
    Suspended: 2,
    Deleted: 3,
} as const;

export type CoreUserStatus = typeof CoreUserStatus[keyof typeof CoreUserStatus];

export interface CoreUserSearchOptions {
    search?: string;
    id?: string;
    username?: string;
    name?: string;
    email?: string;
    phone?: string;
    address?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    status?: string[];
    page?: number;
    pageSize?: number;
}

export interface UserDto {
    id: string;
    username: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    status: CoreUserStatus;
}

export interface UserDetailDto extends UserDto {
    phone?: string;
    address?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    profilePicture?: Uint8Array;
    profilePictureContentType?: string;
    createdAt: string;
    updatedAt: string;
    roles: RoleDto[];
}


export interface CoreUserCreateRequest {
    username: string;
    password: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    address?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    status: CoreUserStatus;
}

export interface CoreUserUpdateRequest {
    username?: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    address?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    status?: CoreUserStatus;

    // Diese beiden Felder sind laut TODO bald unnötig – trotzdem für jetzt enthalten:
    updatedAt?: string; // im Format "yyyy-MM-dd HH:mm:ss"
    refreshTokens?: any[] | null; // Typ `RefreshToken` ggf. definieren, aber laut TODO bald weg
}

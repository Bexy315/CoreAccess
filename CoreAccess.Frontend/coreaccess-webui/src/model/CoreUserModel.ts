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
    status?: CoreUserStatus;
    page?: number;
    pageSize?: number;
}

export interface CoreUserDto {
    id: string;
    username: string;
    email?: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    address?: string;
    city?: string;
    state?: string;
    zip?: string;
    country?: string;
    profilePicture?: string; // Base64 encoded string
    profilePictureContentType?: string;
    status: CoreUserStatus;
    createdAt: string; // ISO-8601
    updatedAt: string;
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

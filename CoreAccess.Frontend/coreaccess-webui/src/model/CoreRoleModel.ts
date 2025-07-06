import type { CorePermission } from './CorePermissionModel';

export interface CoreRole {
    id: string;
    name: string;
    description?: string;
    createdAt: string;
    updatedAt: string;
    isSystem: boolean;
    permissions: CorePermission[];
}
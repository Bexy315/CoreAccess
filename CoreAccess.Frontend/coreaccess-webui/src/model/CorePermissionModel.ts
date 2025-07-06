export interface CorePermission {
    id: string; // Guid in C# is mapped to string in TypeScript
    name: string;
    description?: string;
    createdAt: string; // ISO-8601 or formatted string
    updatedAt: string;
    isSystem: boolean;
}
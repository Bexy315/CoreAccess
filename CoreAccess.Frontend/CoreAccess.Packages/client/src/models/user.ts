export interface Role {
    id: string;
    name: string;
    permissions: string[];
}

export interface User {
    id: string;
    username: string;
    email: string;
    roles: Role[];
}

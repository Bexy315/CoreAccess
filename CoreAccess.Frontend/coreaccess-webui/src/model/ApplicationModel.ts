export interface ApplicationSearchOptions {
    search: string;
    page?: number;        // Default in C# war 1
    pageSize?: number;    // Default in C# war 10
}

export interface ApplicationUpdateRequest {
    clientId?: string;
    displayName?: string;
    redirectUris?: string[];
    postLogoutRedirectUris?: string[];
    clientSecret?: string;
    permissions?: string[];
    requirements?: string[];
}

export interface ApplicationDto {
    id: string;
    clientId: string;
    displayName: string;
    clientType: string;
}

export interface ApplicationDetailDto extends ApplicationDto {
    redirectUris?: string[];
    postLogoutRedirectUris?: string[];
    applicationType: string;
    consentType: string;
    clientSecret: string;
    permissions?: string[];
    requirements?: string;
}

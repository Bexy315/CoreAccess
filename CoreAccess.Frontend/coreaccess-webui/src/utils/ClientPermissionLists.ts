export const Endpoints: string[] = [
    "ept:authorization",
    "ept:device_authorization",
    "ept:end_session",
    "ept:introspection",
    "ept:pushed_authorization",
    "ept:revocation",
    "ept:token",
];

export const GrantTypes: string[] = [
    "gt:authorization_code",
    "gt:client_credentials",
    "gt:urn:ietf:params:oauth:grant-type:device_code",
    "gt:implicit",
    "gt:password",
    "gt:refresh_token",
    "gt:urn:ietf:params:oauth:grant-type:token-exchange",
];

export const Prefixes: string[] = [
    "aud:",
    "ept:",
    "gt:",
    "rst:",
    "rsrc:",
    "scp:",
];

export const ResponseTypes: string[] = [
    "rst:code",
    "rst:code id_token",
    "rst:code id_token token",
    "rst:code token",
    "rst:id_token",
    "rst:id_token token",
    "rst:none",
    "rst:token",
];

export const Scopes: string[] = [
    "scp:address",
    "scp:email",
    "scp:phone",
    "scp:profile",
    "scp:roles",
];

export const AllOpenIdValues: string[] = [
    // Endpoints
    "ept:authorization",
    "ept:device_authorization",
    "ept:end_session",
    "ept:introspection",
    "ept:pushed_authorization",
    "ept:revocation",
    "ept:token",

    // GrantTypes
    "gt:authorization_code",
    "gt:client_credentials",
    "gt:urn:ietf:params:oauth:grant-type:device_code",
    "gt:implicit",
    "gt:password",
    "gt:refresh_token",
    "gt:urn:ietf:params:oauth:grant-type:token-exchange",

    // ResponseTypes
    "rst:code",
    "rst:code id_token",
    "rst:code id_token token",
    "rst:code token",
    "rst:id_token",
    "rst:id_token token",
    "rst:none",
    "rst:token",

    // Scopes
    "scp:address",
    "scp:email",
    "scp:phone",
    "scp:profile",
    "scp:roles",
];

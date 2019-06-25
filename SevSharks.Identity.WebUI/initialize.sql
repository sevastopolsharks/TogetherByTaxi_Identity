
create table "PersistedGrants"
(
  "Key"          varchar(200)   not null
    constraint "PK_PersistedGrants"
    primary key,
  "Type"         varchar(50)    not null,
  "SubjectId"    varchar(200),
  "ClientId"     varchar(200)   not null,
  "CreationTime" timestamp      not null,
  "Expiration"   timestamp,
  "Data"         varchar(50000) not null
);

create index "IX_PersistedGrants_SubjectId_ClientId_Type"
  on "PersistedGrants" ("SubjectId", "ClientId", "Type");

create table "ApiResources"
(
  "Id"          serial       not null
    constraint "PK_ApiResources"
    primary key,
  "Enabled"     boolean      not null,
  "Name"        varchar(200) not null,
  "DisplayName" varchar(200),
  "Description" varchar(1000)
);

create unique index "IX_ApiResources_Name"
  on "ApiResources" ("Name");

create table "Clients"
(
  "Id"                                serial       not null
    constraint "PK_Clients"
    primary key,
  "Enabled"                           boolean      not null,
  "ClientId"                          varchar(200) not null,
  "ProtocolType"                      varchar(200) not null,
  "RequireClientSecret"               boolean      not null,
  "ClientName"                        varchar(200),
  "Description"                       varchar(1000),
  "ClientUri"                         varchar(2000),
  "LogoUri"                           varchar(2000),
  "RequireConsent"                    boolean      not null,
  "AllowRememberConsent"              boolean      not null,
  "AlwaysIncludeUserClaimsInIdToken"  boolean      not null,
  "RequirePkce"                       boolean      not null,
  "AllowPlainTextPkce"                boolean      not null,
  "AllowAccessTokensViaBrowser"       boolean      not null,
  "FrontChannelLogoutUri"             varchar(2000),
  "FrontChannelLogoutSessionRequired" boolean      not null,
  "BackChannelLogoutUri"              varchar(2000),
  "BackChannelLogoutSessionRequired"  boolean      not null,
  "AllowOfflineAccess"                boolean      not null,
  "IdentityTokenLifetime"             integer      not null,
  "AccessTokenLifetime"               integer      not null,
  "AuthorizationCodeLifetime"         integer      not null,
  "ConsentLifetime"                   integer,
  "AbsoluteRefreshTokenLifetime"      integer      not null,
  "SlidingRefreshTokenLifetime"       integer      not null,
  "RefreshTokenUsage"                 integer      not null,
  "UpdateAccessTokenClaimsOnRefresh"  boolean      not null,
  "RefreshTokenExpiration"            integer      not null,
  "AccessTokenType"                   integer      not null,
  "EnableLocalLogin"                  boolean      not null,
  "IncludeJwtId"                      boolean      not null,
  "AlwaysSendClientClaims"            boolean      not null,
  "ClientClaimsPrefix"                varchar(200),
  "PairWiseSubjectSalt"               varchar(200)
);

create unique index "IX_Clients_ClientId"
  on "Clients" ("ClientId");

create table "IdentityResources"
(
  "Id"                      serial       not null
    constraint "PK_IdentityResources"
    primary key,
  "Enabled"                 boolean      not null,
  "Name"                    varchar(200) not null,
  "DisplayName"             varchar(200),
  "Description"             varchar(1000),
  "Required"                boolean      not null,
  "Emphasize"               boolean      not null,
  "ShowInDiscoveryDocument" boolean      not null
);

create unique index "IX_IdentityResources_Name"
  on "IdentityResources" ("Name");

create table "ApiClaims"
(
  "Id"            serial       not null
    constraint "PK_ApiClaims"
    primary key,
  "Type"          varchar(200) not null,
  "ApiResourceId" integer      not null
    constraint "FK_ApiClaims_ApiResources_ApiResourceId"
    references "ApiResources"
    on delete cascade
);

create index "IX_ApiClaims_ApiResourceId"
  on "ApiClaims" ("ApiResourceId");

create table "ApiScopes"
(
  "Id"                      serial       not null
    constraint "PK_ApiScopes"
    primary key,
  "Name"                    varchar(200) not null,
  "DisplayName"             varchar(200),
  "Description"             varchar(1000),
  "Required"                boolean      not null,
  "Emphasize"               boolean      not null,
  "ShowInDiscoveryDocument" boolean      not null,
  "ApiResourceId"           integer      not null
    constraint "FK_ApiScopes_ApiResources_ApiResourceId"
    references "ApiResources"
    on delete cascade
);

create index "IX_ApiScopes_ApiResourceId"
  on "ApiScopes" ("ApiResourceId");

create unique index "IX_ApiScopes_Name"
  on "ApiScopes" ("Name");

create table "ApiSecrets"
(
  "Id"            serial  not null
    constraint "PK_ApiSecrets"
    primary key,
  "Description"   varchar(1000),
  "Value"         varchar(2000),
  "Expiration"    timestamp,
  "Type"          varchar(250),
  "ApiResourceId" integer not null
    constraint "FK_ApiSecrets_ApiResources_ApiResourceId"
    references "ApiResources"
    on delete cascade
);

create index "IX_ApiSecrets_ApiResourceId"
  on "ApiSecrets" ("ApiResourceId");

create table "ClientClaims"
(
  "Id"       serial       not null
    constraint "PK_ClientClaims"
    primary key,
  "Type"     varchar(250) not null,
  "Value"    varchar(250) not null,
  "ClientId" integer      not null
    constraint "FK_ClientClaims_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientClaims_ClientId"
  on "ClientClaims" ("ClientId");

create table "ClientCorsOrigins"
(
  "Id"       serial       not null
    constraint "PK_ClientCorsOrigins"
    primary key,
  "Origin"   varchar(150) not null,
  "ClientId" integer      not null
    constraint "FK_ClientCorsOrigins_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientCorsOrigins_ClientId"
  on "ClientCorsOrigins" ("ClientId");

create table "ClientGrantTypes"
(
  "Id"        serial       not null
    constraint "PK_ClientGrantTypes"
    primary key,
  "GrantType" varchar(250) not null,
  "ClientId"  integer      not null
    constraint "FK_ClientGrantTypes_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientGrantTypes_ClientId"
  on "ClientGrantTypes" ("ClientId");

create table "ClientIdPRestrictions"
(
  "Id"       serial       not null
    constraint "PK_ClientIdPRestrictions"
    primary key,
  "Provider" varchar(200) not null,
  "ClientId" integer      not null
    constraint "FK_ClientIdPRestrictions_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientIdPRestrictions_ClientId"
  on "ClientIdPRestrictions" ("ClientId");

create table "ClientPostLogoutRedirectUris"
(
  "Id"                    serial        not null
    constraint "PK_ClientPostLogoutRedirectUris"
    primary key,
  "PostLogoutRedirectUri" varchar(2000) not null,
  "ClientId"              integer       not null
    constraint "FK_ClientPostLogoutRedirectUris_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientPostLogoutRedirectUris_ClientId"
  on "ClientPostLogoutRedirectUris" ("ClientId");

create table "ClientProperties"
(
  "Id"       serial        not null
    constraint "PK_ClientProperties"
    primary key,
  "Key"      varchar(250)  not null,
  "Value"    varchar(2000) not null,
  "ClientId" integer       not null
    constraint "FK_ClientProperties_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientProperties_ClientId"
  on "ClientProperties" ("ClientId");

create table "ClientRedirectUris"
(
  "Id"          serial        not null
    constraint "PK_ClientRedirectUris"
    primary key,
  "RedirectUri" varchar(2000) not null,
  "ClientId"    integer       not null
    constraint "FK_ClientRedirectUris_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientRedirectUris_ClientId"
  on "ClientRedirectUris" ("ClientId");

create table "ClientScopes"
(
  "Id"       serial       not null
    constraint "PK_ClientScopes"
    primary key,
  "Scope"    varchar(200) not null,
  "ClientId" integer      not null
    constraint "FK_ClientScopes_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientScopes_ClientId"
  on "ClientScopes" ("ClientId");

create table "ClientSecrets"
(
  "Id"          serial        not null
    constraint "PK_ClientSecrets"
    primary key,
  "Description" varchar(2000),
  "Value"       varchar(2000) not null,
  "Expiration"  timestamp,
  "Type"        varchar(250),
  "ClientId"    integer       not null
    constraint "FK_ClientSecrets_Clients_ClientId"
    references "Clients"
    on delete cascade
);

create index "IX_ClientSecrets_ClientId"
  on "ClientSecrets" ("ClientId");

create table "IdentityClaims"
(
  "Id"                 serial       not null
    constraint "PK_IdentityClaims"
    primary key,
  "Type"               varchar(200) not null,
  "IdentityResourceId" integer      not null
    constraint "FK_IdentityClaims_IdentityResources_IdentityResourceId"
    references "IdentityResources"
    on delete cascade
);

create index "IX_IdentityClaims_IdentityResourceId"
  on "IdentityClaims" ("IdentityResourceId");

create table "ApiScopeClaims"
(
  "Id"         serial       not null
    constraint "PK_ApiScopeClaims"
    primary key,
  "Type"       varchar(200) not null,
  "ApiScopeId" integer      not null
    constraint "FK_ApiScopeClaims_ApiScopes_ApiScopeId"
    references "ApiScopes"
    on delete cascade
);

create index "IX_ApiScopeClaims_ApiScopeId"
  on "ApiScopeClaims" ("ApiScopeId");

create table "Roles"
(
  "Id"               text not null
    constraint "PK_Roles"
    primary key,
  "Name"             varchar(256),
  "NormalizedName"   varchar(256),
  "ConcurrencyStamp" text
);

create unique index "RoleNameIndex"
  on "Roles" ("NormalizedName");

create table "Users"
(
  "Id"                   text    not null
    constraint "PK_Users"
    primary key,
  "UserName"             varchar(256),
  "NormalizedUserName"   varchar(256),
  "Email"                varchar(256),
  "NormalizedEmail"      varchar(256),
  "EmailConfirmed"       boolean not null,
  "PasswordHash"         text,
  "SecurityStamp"        text,
  "ConcurrencyStamp"     text,
  "PhoneNumber"          text,
  "PhoneNumberConfirmed" boolean not null,
  "TwoFactorEnabled"     boolean not null,
  "LockoutEnd"           timestamp with time zone,
  "LockoutEnabled"       boolean not null,
  "AccessFailedCount"    integer not null
);

create index "EmailIndex"
  on "Users" ("NormalizedEmail");

create unique index "UserNameIndex"
  on "Users" ("NormalizedUserName");

create table "RoleClaims"
(
  "Id"         serial not null
    constraint "PK_RoleClaims"
    primary key,
  "RoleId"     text   not null
    constraint "FK_RoleClaims_Roles_RoleId"
    references "Roles"
    on delete cascade,
  "ClaimType"  text,
  "ClaimValue" text
);

create index "IX_RoleClaims_RoleId"
  on "RoleClaims" ("RoleId");

create table "UserClaims"
(
  "Id"         serial not null
    constraint "PK_UserClaims"
    primary key,
  "UserId"     text   not null
    constraint "FK_UserClaims_Users_UserId"
    references "Users"
    on delete cascade,
  "ClaimType"  text,
  "ClaimValue" text
);

create index "IX_UserClaims_UserId"
  on "UserClaims" ("UserId");

create table "UserLogins"
(
  "LoginProvider"       text not null,
  "ProviderKey"         text not null,
  "ProviderDisplayName" text,
  "UserId"              text not null
    constraint "FK_UserLogins_Users_UserId"
    references "Users"
    on delete cascade,
  constraint "PK_UserLogins"
  primary key ("LoginProvider", "ProviderKey")
);

create index "IX_UserLogins_UserId"
  on "UserLogins" ("UserId");

create table "UserRoles"
(
  "UserId" text not null
    constraint "FK_UserRoles_Users_UserId"
    references "Users"
    on delete cascade,
  "RoleId" text not null
    constraint "FK_UserRoles_Roles_RoleId"
    references "Roles"
    on delete cascade,
  constraint "PK_UserRoles"
  primary key ("UserId", "RoleId")
);

create index "IX_UserRoles_RoleId"
  on "UserRoles" ("RoleId");

create table "UserTokens"
(
  "UserId"        text not null
    constraint "FK_UserTokens_Users_UserId"
    references "Users"
    on delete cascade,
  "LoginProvider" text not null,
  "Name"          text not null,
  "Value"         text,
  constraint "PK_UserTokens"
  primary key ("UserId", "LoginProvider", "Name")
);

INSERT INTO public."ApiResources" ("Id", "Enabled", "Name", "DisplayName", "Description") VALUES (1, true, 'it2_gateway_api', 'Gateway API', null);
INSERT INTO public."ApiScopes" ("Id", "Name", "DisplayName", "Description", "Required", "Emphasize", "ShowInDiscoveryDocument", "ApiResourceId") VALUES (1, 'it2_gateway_api', 'Gateway API', null, false, false, true, 1);
INSERT INTO public."Clients" ("Id", "Enabled", "ClientId", "ProtocolType", "RequireClientSecret", "ClientName", "Description", "ClientUri", "LogoUri", "RequireConsent", "AllowRememberConsent", "AlwaysIncludeUserClaimsInIdToken", "RequirePkce", "AllowPlainTextPkce", "AllowAccessTokensViaBrowser", "FrontChannelLogoutUri", "FrontChannelLogoutSessionRequired", "BackChannelLogoutUri", "BackChannelLogoutSessionRequired", "AllowOfflineAccess", "IdentityTokenLifetime", "AccessTokenLifetime", "AuthorizationCodeLifetime", "ConsentLifetime", "AbsoluteRefreshTokenLifetime", "SlidingRefreshTokenLifetime", "RefreshTokenUsage", "UpdateAccessTokenClaimsOnRefresh", "RefreshTokenExpiration", "AccessTokenType", "EnableLocalLogin", "IncludeJwtId", "AlwaysSendClientClaims", "ClientClaimsPrefix", "PairWiseSubjectSalt") VALUES (1, true, 'it2_web_spa', 'oidc', true, 'IT2 Web SPA Client', null, null, null, true, true, false, false, false, true, null, true, null, true, false, 300, 3600, 300, null, 2592000, 1296000, 1, false, 1, 0, true, false, false, 'client_', null);
INSERT INTO public."ClientCorsOrigins" ("Id", "Origin", "ClientId") VALUES (1, 'http://localhost:4200', 1);
INSERT INTO public."ClientGrantTypes" ("Id", "GrantType", "ClientId") VALUES (1, 'implicit', 1);
INSERT INTO public."ClientPostLogoutRedirectUris" ("Id", "PostLogoutRedirectUri", "ClientId") VALUES (1, 'http://localhost:4200/index.html', 1);
INSERT INTO public."ClientRedirectUris" ("Id", "RedirectUri", "ClientId") VALUES (1, 'http://localhost:4200/callback.html', 1);
INSERT INTO public."ClientScopes" ("Id", "Scope", "ClientId") VALUES (1, 'openid', 1);
INSERT INTO public."ClientScopes" ("Id", "Scope", "ClientId") VALUES (2, 'profile', 1);
INSERT INTO public."ClientScopes" ("Id", "Scope", "ClientId") VALUES (3, 'it2_gateway_api', 1);
INSERT INTO public."IdentityResources" ("Id", "Enabled", "Name", "DisplayName", "Description", "Required", "Emphasize", "ShowInDiscoveryDocument") VALUES (1, true, 'openid', 'Your user identifier', null, true, false, true);
INSERT INTO public."IdentityResources" ("Id", "Enabled", "Name", "DisplayName", "Description", "Required", "Emphasize", "ShowInDiscoveryDocument") VALUES (2, true, 'profile', 'User profile', 'Your user profile information (first name, last name, etc.)', false, true, true);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (1, 'sub', 1);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (2, 'name', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (3, 'family_name', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (4, 'given_name', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (5, 'middle_name', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (6, 'nickname', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (7, 'preferred_username', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (8, 'profile', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (9, 'picture', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (10, 'website', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (11, 'gender', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (12, 'birthdate', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (13, 'zoneinfo', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (14, 'locale', 2);
INSERT INTO public."IdentityClaims" ("Id", "Type", "IdentityResourceId") VALUES (15, 'updated_at', 2);
INSERT INTO public."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('545e009a-fbe9-4afa-b5e6-a8d926e66e99', 'seller1', 'SELLER1', 'seller1@email.com', 'SELLER1@EMAIL.COM', true, 'AQAAAAEAACcQAAAAEB6wcDaZXakw34ERLpR0IcIRPanZXTx7qqCMoAAtOTijBpIouvL0CvTRzVOKd9l0PA==', 'HIMBF3KBRZ6HKVNDRKVUPXRDGUTWWSSF', '1ff69dec-e2ac-4765-bc5b-2862ba7f189e', null, false, false, null, true, 0);
INSERT INTO public."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('841bd90f-50f1-4046-b6da-6cbcd84cab66', 'seller2', 'SELLER2', 'seller2@email.com', 'SELLER2@EMAIL.COM', true, 'AQAAAAEAACcQAAAAECkOussqHXqzPtek3UXgTC++RYYpbujXe9p4XzbGm2HP6NPzmGECaM3Skzjam4nOuA==', 'GM62RJPYYZE7G2ZUM46GL2JM756EPF3J', '8e990b99-fe81-4c97-9396-610bcf79757d', null, false, false, null, true, 0);
INSERT INTO public."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('c4dc0ab8-fc6e-408b-a8af-4730e96339d7', 'buyer1', 'BUYER1', 'buyer1@email.com', 'BUYER1@EMAIL.COM', true, 'AQAAAAEAACcQAAAAELX/gT8GRNH3QLx9hhnh2ONtMqPKb0bjZpNliyx1seArPu79i4BtVRIDCoj4Qaa/SA==', '4ZV3NNCZ6KSVTYEK5IDXZ2OYWV2LR5WD', '75887617-90f0-4698-ae2a-10bc979f297e', null, false, false, null, true, 0);
INSERT INTO public."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") VALUES ('7d577162-07c7-42bc-b7b0-2052dd77a7ab', 'buyer2', 'BUYER2', 'buyer2@email.com', 'BUYER2@EMAIL.COM', true, 'AQAAAAEAACcQAAAAECpj1jxJEoZYKW7dEnSBMwzX0VaP5Rnhs95Mf6LTFyFpjkW3QF3w+D+I37daB6MYXw==', 'FNX3PH6OMF3LCRN7DWRTYB7DHXVCZRSW', 'af1907f5-32cd-47d6-8342-ab211872a8fa', null, false, false, null, true, 0);
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (1, '545e009a-fbe9-4afa-b5e6-a8d926e66e99', 'name', 'Seller1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (2, '545e009a-fbe9-4afa-b5e6-a8d926e66e99', 'given_name', 'Seller1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (3, '545e009a-fbe9-4afa-b5e6-a8d926e66e99', 'family_name', 'Seller1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (4, '841bd90f-50f1-4046-b6da-6cbcd84cab66', 'name', 'Seller2');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (5, '841bd90f-50f1-4046-b6da-6cbcd84cab66', 'given_name', 'Seller2');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (6, '841bd90f-50f1-4046-b6da-6cbcd84cab66', 'family_name', 'Seller2');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (7, 'c4dc0ab8-fc6e-408b-a8af-4730e96339d7', 'name', 'Buyer1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (8, 'c4dc0ab8-fc6e-408b-a8af-4730e96339d7', 'given_name', 'Buyer1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (9, 'c4dc0ab8-fc6e-408b-a8af-4730e96339d7', 'family_name', 'Buyer1');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (10, '7d577162-07c7-42bc-b7b0-2052dd77a7ab', 'name', 'Buyer2');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (11, '7d577162-07c7-42bc-b7b0-2052dd77a7ab', 'given_name', 'Buyer2');
INSERT INTO public."UserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") VALUES (12, '7d577162-07c7-42bc-b7b0-2052dd77a7ab', 'family_name', 'Buyer2');

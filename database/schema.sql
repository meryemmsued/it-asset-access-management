BEGIN;

-- Extensions
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- DROP TABLES (reverse dependency order, for repeatable dev runs)
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS login_attempts CASCADE;
DROP TABLE IF EXISTS security_events CASCADE;
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS asset_accesses CASCADE;
DROP TABLE IF EXISTS access_request_approvals CASCADE;
DROP TABLE IF EXISTS access_requests CASCADE;
DROP TABLE IF EXISTS asset_status_histories CASCADE;
DROP TABLE IF EXISTS asset_assignments CASCADE;
DROP TABLE IF EXISTS digital_asset_details CASCADE;
DROP TABLE IF EXISTS physical_asset_details CASCADE;
DROP TABLE IF EXISTS assets CASCADE;
DROP TABLE IF EXISTS asset_categories CASCADE;
DROP TABLE IF EXISTS role_permissions CASCADE;
DROP TABLE IF EXISTS user_roles CASCADE;
DROP TABLE IF EXISTS permissions CASCADE;
DROP TABLE IF EXISTS roles CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS teams CASCADE;
DROP TABLE IF EXISTS departments CASCADE;

 
-- SECTION: ORGANIZATION (departments, teams, users)
-- Note: departments <-> teams <-> users has a circular relationship
-- (teams.team_lead_user_id -> users.id, users.department_id/team_id ->
-- departments/teams). We break the cycle by creating teams.team_lead_user_id
-- as a plain nullable column first and attaching its FK constraint via
-- ALTER TABLE after the users table exists.

CREATE TABLE departments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(150) NOT NULL,
    description     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_departments_name UNIQUE (name)
);

CREATE TABLE teams (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    department_id       UUID NOT NULL,
    team_lead_user_id   UUID NULL, -- FK attached later, once users exists
    name                VARCHAR(150) NOT NULL,
    description         TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_teams_department
        FOREIGN KEY (department_id) REFERENCES departments (id)
        ON DELETE RESTRICT, -- keep teams from silently losing their department
    CONSTRAINT uq_teams_department_name UNIQUE (department_id, name)
);

CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    department_id   UUID NULL,
    team_id         UUID NULL,
    manager_id      UUID NULL,
    first_name      VARCHAR(100) NOT NULL,
    last_name       VARCHAR(100) NOT NULL,
    email           VARCHAR(255) NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    job_title       VARCHAR(150),
    phone_number    VARCHAR(30),
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    last_login_at   TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ  NULL,
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT fk_users_department
        FOREIGN KEY (department_id) REFERENCES departments (id)
        ON DELETE SET NULL, -- a user survives its department being removed
    CONSTRAINT fk_users_team
        FOREIGN KEY (team_id) REFERENCES teams (id)
        ON DELETE SET NULL,
    CONSTRAINT fk_users_manager
        FOREIGN KEY (manager_id) REFERENCES users (id)
        ON DELETE SET NULL, -- self-referencing org hierarchy
    CONSTRAINT chk_users_not_own_manager CHECK (manager_id IS DISTINCT FROM id)
);

-- Close the circular relationship: teams.team_lead_user_id -> users.id
ALTER TABLE teams
    ADD CONSTRAINT fk_teams_team_lead
        FOREIGN KEY (team_lead_user_id) REFERENCES users (id)
        ON DELETE SET NULL; -- losing the lead user shouldn't delete the team

CREATE INDEX ix_teams_department_id ON teams (department_id);
CREATE INDEX ix_teams_team_lead_user_id ON teams (team_lead_user_id);
CREATE INDEX ix_users_department_id ON users (department_id);
CREATE INDEX ix_users_team_id ON users (team_id);
CREATE INDEX ix_users_manager_id ON users (manager_id);
CREATE INDEX ix_users_email ON users (email);
CREATE INDEX ix_users_is_active ON users (is_active);

-- SECTION: AUTHORIZATION (roles, permissions, user_roles, role_permissions)

CREATE TABLE roles (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    description     TEXT,
    is_system_role  BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_roles_name UNIQUE (name)
);

CREATE TABLE permissions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(150) NOT NULL,
    description     TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_permissions_name UNIQUE (name)
);

-- user_roles and role_permissions are true dependent join records:
-- CASCADE is appropriate here because the association has no meaning
-- once either side of the relationship is gone.
CREATE TABLE user_roles (
    user_id                 UUID NOT NULL,
    role_id                 UUID NOT NULL,
    assigned_by_user_id     UUID NULL,
    assigned_at             TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role
        FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_assigned_by
        FOREIGN KEY (assigned_by_user_id) REFERENCES users (id) ON DELETE SET NULL
);

CREATE TABLE role_permissions (
    role_id         UUID NOT NULL,
    permission_id   UUID NOT NULL,
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, permission_id),
    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (role_id) REFERENCES roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (permission_id) REFERENCES permissions (id) ON DELETE CASCADE
);

CREATE INDEX ix_user_roles_role_id ON user_roles (role_id);
CREATE INDEX ix_user_roles_assigned_by_user_id ON user_roles (assigned_by_user_id);
CREATE INDEX ix_role_permissions_permission_id ON role_permissions (permission_id);

-- SECTION: ASSETS (categories, assets, physical/digital details,
-- assignments, status history)

CREATE TABLE asset_categories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(150) NOT NULL,
    description     TEXT,
    asset_type      VARCHAR(20) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_asset_categories_name UNIQUE (name),
    CONSTRAINT chk_asset_categories_asset_type
        CHECK (asset_type IN ('PHYSICAL', 'DIGITAL'))
);

CREATE TABLE assets (
    id                          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_category_id           UUID NOT NULL,
    asset_code                  VARCHAR(50) NOT NULL,
    name                        VARCHAR(200) NOT NULL,
    description                 TEXT,
    status                      VARCHAR(30) NOT NULL DEFAULT 'AVAILABLE',
    purchase_date               DATE,
    purchase_price              NUMERIC(14, 2),
    warranty_expiration_date    DATE,
    created_by_user_id          UUID NOT NULL,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at                  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_assets_asset_code UNIQUE (asset_code),
    CONSTRAINT fk_assets_category
        FOREIGN KEY (asset_category_id) REFERENCES asset_categories (id)
        ON DELETE RESTRICT, -- a category in use cannot vanish from under assets
    CONSTRAINT fk_assets_created_by
        FOREIGN KEY (created_by_user_id) REFERENCES users (id)
        ON DELETE RESTRICT, -- preserve provenance of the asset record
    CONSTRAINT chk_assets_status
        CHECK (status IN ('AVAILABLE', 'ASSIGNED', 'IN_REPAIR', 'RETIRED', 'LOST', 'DECOMMISSIONED')),
    CONSTRAINT chk_assets_purchase_price CHECK (purchase_price IS NULL OR purchase_price >= 0),
    CONSTRAINT chk_assets_warranty_after_purchase
        CHECK (warranty_expiration_date IS NULL OR purchase_date IS NULL OR warranty_expiration_date >= purchase_date)
);

CREATE INDEX ix_assets_category_id ON assets (asset_category_id);
CREATE INDEX ix_assets_created_by_user_id ON assets (created_by_user_id);
CREATE INDEX ix_assets_asset_code ON assets (asset_code);
CREATE INDEX ix_assets_status ON assets (status);

-- 1:1 detail tables. CASCADE is appropriate: these rows have no meaning
-- without their parent asset row and are true dependent records.
CREATE TABLE physical_asset_details (
    asset_id        UUID PRIMARY KEY,
    serial_number   VARCHAR(150),
    manufacturer    VARCHAR(150),
    model           VARCHAR(150),
    location        VARCHAR(200),
    condition       VARCHAR(30),
    CONSTRAINT fk_physical_asset_details_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE CASCADE,
    CONSTRAINT uq_physical_asset_details_serial_number UNIQUE (serial_number),
    CONSTRAINT chk_physical_asset_details_condition
        CHECK (condition IS NULL OR condition IN ('NEW', 'GOOD', 'FAIR', 'POOR', 'DAMAGED'))
);

CREATE INDEX ix_physical_asset_details_location ON physical_asset_details (location);

CREATE TABLE digital_asset_details (
    asset_id                    UUID PRIMARY KEY,
    license_key                 VARCHAR(500),
    version                     VARCHAR(50),
    license_type                VARCHAR(30),
    license_start_date          DATE,
    license_expiration_date     DATE,
    maximum_users                INTEGER,
    CONSTRAINT fk_digital_asset_details_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE CASCADE,
    CONSTRAINT chk_digital_asset_details_license_type
        CHECK (license_type IS NULL OR license_type IN ('PERPETUAL', 'SUBSCRIPTION', 'TRIAL', 'OPEN_SOURCE')),
    CONSTRAINT chk_digital_asset_details_max_users CHECK (maximum_users IS NULL OR maximum_users > 0),
    CONSTRAINT chk_digital_asset_details_license_dates
        CHECK (license_expiration_date IS NULL OR license_start_date IS NULL OR license_expiration_date >= license_start_date)
);

-- Assignment and status-history tables record real operational/audit
-- history, so the asset FK uses RESTRICT: an asset with assignment or
-- status history cannot be physically deleted.
CREATE TABLE asset_assignments (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id                UUID NOT NULL,
    assigned_to_user_id     UUID NOT NULL,
    assigned_by_user_id     UUID NOT NULL,
    assigned_at             TIMESTAMPTZ NOT NULL DEFAULT now(),
    returned_at             TIMESTAMPTZ,
    notes                   TEXT,
    status                  VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    CONSTRAINT fk_asset_assignments_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_asset_assignments_assigned_to
        FOREIGN KEY (assigned_to_user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_asset_assignments_assigned_by
        FOREIGN KEY (assigned_by_user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT chk_asset_assignments_status
        CHECK (status IN ('ACTIVE', 'RETURNED', 'LOST', 'DAMAGED')),
    CONSTRAINT chk_asset_assignments_return_after_assign
        CHECK (returned_at IS NULL OR returned_at >= assigned_at)
);

CREATE INDEX ix_asset_assignments_asset_id ON asset_assignments (asset_id);
CREATE INDEX ix_asset_assignments_assigned_to_user_id ON asset_assignments (assigned_to_user_id);
CREATE INDEX ix_asset_assignments_assigned_by_user_id ON asset_assignments (assigned_by_user_id);
CREATE INDEX ix_asset_assignments_status ON asset_assignments (status);
-- Fast lookup of the currently-active assignment(s) for an asset
CREATE INDEX ix_asset_assignments_active ON asset_assignments (asset_id) WHERE status = 'ACTIVE';

CREATE TABLE asset_status_histories (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id            UUID NOT NULL,
    old_status          VARCHAR(30),
    new_status          VARCHAR(30) NOT NULL,
    changed_by_user_id  UUID NOT NULL,
    change_reason       TEXT,
    changed_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_asset_status_histories_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_asset_status_histories_changed_by
        FOREIGN KEY (changed_by_user_id) REFERENCES users (id) ON DELETE RESTRICT
);

CREATE INDEX ix_asset_status_histories_asset_id ON asset_status_histories (asset_id);
CREATE INDEX ix_asset_status_histories_changed_by_user_id ON asset_status_histories (changed_by_user_id);
CREATE INDEX ix_asset_status_histories_changed_at ON asset_status_histories (changed_at);

-- SECTION: ACCESS MANAGEMENT (access_requests, approvals, asset_accesses)

CREATE TABLE access_requests (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    requester_user_id       UUID NOT NULL,
    asset_id                UUID NOT NULL,
    requested_access_type   VARCHAR(50) NOT NULL,
    reason                  TEXT,
    status                  VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    requested_at            TIMESTAMPTZ NOT NULL DEFAULT now(),
    resolved_at             TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_access_requests_requester
        FOREIGN KEY (requester_user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_access_requests_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
    CONSTRAINT chk_access_requests_status
        CHECK (status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')),
    CONSTRAINT chk_access_requests_resolved_after_requested
        CHECK (resolved_at IS NULL OR resolved_at >= requested_at)
);

CREATE INDEX ix_access_requests_requester_user_id ON access_requests (requester_user_id);
CREATE INDEX ix_access_requests_asset_id ON access_requests (asset_id);
CREATE INDEX ix_access_requests_status ON access_requests (status);

-- Approval steps are true dependent records of a request: CASCADE is fine.
CREATE TABLE access_request_approvals (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    access_request_id   UUID NOT NULL,
    approver_user_id     UUID NOT NULL,
    approval_order      INTEGER NOT NULL,
    decision            VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    decision_note       TEXT,
    decided_at          TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_access_request_approvals_request
        FOREIGN KEY (access_request_id) REFERENCES access_requests (id) ON DELETE CASCADE,
    CONSTRAINT fk_access_request_approvals_approver
        FOREIGN KEY (approver_user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT chk_access_request_approvals_decision
        CHECK (decision IN ('PENDING', 'APPROVED', 'REJECTED')),
    CONSTRAINT chk_access_request_approvals_order CHECK (approval_order > 0),
    -- Prevent duplicate approval steps for the same request
    CONSTRAINT uq_access_request_approvals_request_order UNIQUE (access_request_id, approval_order)
);

CREATE INDEX ix_access_request_approvals_request_id ON access_request_approvals (access_request_id);
CREATE INDEX ix_access_request_approvals_approver_user_id ON access_request_approvals (approver_user_id);

CREATE TABLE asset_accesses (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id            UUID NOT NULL,
    user_id             UUID NOT NULL,
    access_request_id   UUID NULL,
    access_type         VARCHAR(50) NOT NULL,
    granted_by_user_id  UUID NOT NULL,
    granted_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at          TIMESTAMPTZ,
    revoked_at          TIMESTAMPTZ,
    revocation_reason   TEXT,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_asset_accesses_asset
        FOREIGN KEY (asset_id) REFERENCES assets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_asset_accesses_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT fk_asset_accesses_request
        FOREIGN KEY (access_request_id) REFERENCES access_requests (id) ON DELETE SET NULL,
    CONSTRAINT fk_asset_accesses_granted_by
        FOREIGN KEY (granted_by_user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT chk_asset_accesses_expires_after_granted
        CHECK (expires_at IS NULL OR expires_at >= granted_at),
    CONSTRAINT chk_asset_accesses_revoked_after_granted
        CHECK (revoked_at IS NULL OR revoked_at >= granted_at)
);

CREATE INDEX ix_asset_accesses_asset_id ON asset_accesses (asset_id);
CREATE INDEX ix_asset_accesses_user_id ON asset_accesses (user_id);
CREATE INDEX ix_asset_accesses_access_request_id ON asset_accesses (access_request_id);
CREATE INDEX ix_asset_accesses_granted_by_user_id ON asset_accesses (granted_by_user_id);
-- Fast lookup of currently-active grants
CREATE INDEX ix_asset_accesses_active ON asset_accesses (asset_id, user_id) WHERE is_active = TRUE;

-- SECTION: SECURITY AND AUDITING
-- Audit/security history must survive user deletion, so user_id uses
-- SET NULL rather than CASCADE or RESTRICT.

CREATE TABLE audit_logs (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id      UUID NULL,
    action       VARCHAR(100) NOT NULL,
    entity_type  VARCHAR(100) NOT NULL,
    entity_id    UUID NULL,
    old_values   JSONB,
    new_values   JSONB,
    ip_address   VARCHAR(45),
    user_agent   TEXT,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_audit_logs_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX ix_audit_logs_user_id ON audit_logs (user_id);
CREATE INDEX ix_audit_logs_entity_type_entity_id ON audit_logs (entity_type, entity_id);
CREATE INDEX ix_audit_logs_created_at ON audit_logs (created_at);

CREATE TABLE security_events (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id              UUID NULL,
    event_type           VARCHAR(100) NOT NULL,
    severity             VARCHAR(20) NOT NULL,
    description          TEXT,
    ip_address           VARCHAR(45),
    metadata             JSONB,
    is_resolved          BOOLEAN NOT NULL DEFAULT FALSE,
    resolved_by_user_id  UUID NULL,
    resolved_at          TIMESTAMPTZ,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_security_events_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL,
    CONSTRAINT fk_security_events_resolved_by
        FOREIGN KEY (resolved_by_user_id) REFERENCES users (id) ON DELETE SET NULL,
    CONSTRAINT chk_security_events_severity
        CHECK (severity IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')),
    CONSTRAINT chk_security_events_resolution_consistency
        CHECK (
            (is_resolved = FALSE AND resolved_at IS NULL)
            OR (is_resolved = TRUE AND resolved_at IS NOT NULL)
        )
);

CREATE INDEX ix_security_events_user_id ON security_events (user_id);
CREATE INDEX ix_security_events_severity ON security_events (severity);
CREATE INDEX ix_security_events_is_resolved ON security_events (is_resolved);
CREATE INDEX ix_security_events_created_at ON security_events (created_at);

CREATE TABLE login_attempts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NULL,
    email           VARCHAR(255) NOT NULL,
    ip_address      VARCHAR(45),
    user_agent      TEXT,
    is_successful   BOOLEAN NOT NULL,
    failure_reason  VARCHAR(200),
    attempted_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_login_attempts_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE SET NULL
);

CREATE INDEX ix_login_attempts_user_id ON login_attempts (user_id);
CREATE INDEX ix_login_attempts_email ON login_attempts (email);
CREATE INDEX ix_login_attempts_ip_address ON login_attempts (ip_address);
CREATE INDEX ix_login_attempts_attempted_at ON login_attempts (attempted_at);
CREATE INDEX ix_login_attempts_email_attempted_at ON login_attempts (email, attempted_at);

-- refresh_tokens: never store raw tokens, only their hash.
-- Self-referencing replaced_by_token_id supports rotation chains.
CREATE TABLE refresh_tokens (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 UUID NOT NULL,
    token_hash              VARCHAR(255) NOT NULL,
    expires_at              TIMESTAMPTZ NOT NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    revoked_at              TIMESTAMPTZ,
    revoked_by_ip           VARCHAR(45),
    created_by_ip           VARCHAR(45),
    replaced_by_token_id    UUID NULL,
    CONSTRAINT uq_refresh_tokens_token_hash UNIQUE (token_hash),
    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT fk_refresh_tokens_replaced_by
        FOREIGN KEY (replaced_by_token_id) REFERENCES refresh_tokens (id) ON DELETE SET NULL,
    CONSTRAINT chk_refresh_tokens_expires_after_created
        CHECK (expires_at > created_at)
);

CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens (user_id);
CREATE INDEX ix_refresh_tokens_token_hash ON refresh_tokens (token_hash);
CREATE INDEX ix_refresh_tokens_expires_at ON refresh_tokens (expires_at);
CREATE INDEX ix_refresh_tokens_replaced_by_token_id ON refresh_tokens (replaced_by_token_id);
-- Fast lookup of a user's currently-valid (non-revoked, unexpired) tokens
CREATE INDEX ix_refresh_tokens_active ON refresh_tokens (user_id, expires_at) WHERE revoked_at IS NULL;

-- SECTION: NOTIFICATIONS

CREATE TABLE notifications (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 UUID NOT NULL,
    title                   VARCHAR(200) NOT NULL,
    message                 TEXT NOT NULL,
    notification_type       VARCHAR(50) NOT NULL,
    related_entity_type     VARCHAR(100),
    related_entity_id       UUID NULL,
    is_read                 BOOLEAN NOT NULL DEFAULT FALSE,
    read_at                 TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT fk_notifications_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT chk_notifications_read_consistency
        CHECK (
            (is_read = FALSE AND read_at IS NULL)
            OR (is_read = TRUE AND read_at IS NOT NULL)
        )
);

CREATE INDEX ix_notifications_user_id ON notifications (user_id);
CREATE INDEX ix_notifications_is_read ON notifications (is_read);
CREATE INDEX ix_notifications_created_at ON notifications (created_at);
CREATE INDEX ix_notifications_user_is_read_created_at ON notifications (user_id, is_read, created_at);

COMMIT;

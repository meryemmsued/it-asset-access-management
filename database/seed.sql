BEGIN;

-- SECTION: ROLES
INSERT INTO roles (id, name, description, is_system_role)
VALUES
    (gen_random_uuid(), 'Admin',            'Full administrative access to the system.', TRUE),
    (gen_random_uuid(), 'IT Specialist',    'Manages assets, categories and technical fulfillment of access.', TRUE),
    (gen_random_uuid(), 'Team Lead',        'Oversees team members and approves team-level access requests.', TRUE),
    (gen_random_uuid(), 'Security Officer', 'Monitors security events and audits access/asset activity.', TRUE),
    (gen_random_uuid(), 'Employee',         'Standard user who can request assets and access.', TRUE),
    (gen_random_uuid(), 'Auditor',          'Read-only access to audit logs and historical records.', TRUE)
ON CONFLICT (name) DO NOTHING;

-- SECTION: PERMISSIONS

INSERT INTO permissions (id, name, description)
VALUES
    -- Users
    (gen_random_uuid(), 'users.view',              'View user profiles.'),
    (gen_random_uuid(), 'users.create',             'Create new users.'),
    (gen_random_uuid(), 'users.update',             'Update user profiles.'),
    (gen_random_uuid(), 'users.deactivate',         'Deactivate or reactivate users.'),

    -- Roles
    (gen_random_uuid(), 'roles.view',               'View roles and their permissions.'),
    (gen_random_uuid(), 'roles.manage',             'Create, update or delete roles and permission assignments.'),

    -- Departments
    (gen_random_uuid(), 'departments.view',         'View departments.'),
    (gen_random_uuid(), 'departments.manage',       'Create, update or delete departments.'),

    -- Teams
    (gen_random_uuid(), 'teams.view',               'View teams.'),
    (gen_random_uuid(), 'teams.manage',             'Create, update or delete teams.'),

    -- Assets
    (gen_random_uuid(), 'assets.view',              'View assets and their details.'),
    (gen_random_uuid(), 'assets.create',            'Register new assets.'),
    (gen_random_uuid(), 'assets.update',            'Update asset details.'),
    (gen_random_uuid(), 'assets.retire',            'Retire or decommission assets.'),
    (gen_random_uuid(), 'assets.categories.manage', 'Manage asset categories.'),

    -- Assignments
    (gen_random_uuid(), 'assignments.view',         'View asset assignment history.'),
    (gen_random_uuid(), 'assignments.create',       'Assign assets to users.'),
    (gen_random_uuid(), 'assignments.return',       'Process asset returns.'),

    -- Access requests
    (gen_random_uuid(), 'access_requests.view',        'View access requests.'),
    (gen_random_uuid(), 'access_requests.create',      'Submit access requests.'),
    (gen_random_uuid(), 'access_requests.approve',     'Approve or reject access requests.'),
    (gen_random_uuid(), 'access_requests.grant',       'Grant or revoke asset access.'),

    -- Security events
    (gen_random_uuid(), 'security_events.view',     'View security events.'),
    (gen_random_uuid(), 'security_events.manage',   'Resolve or update security events.'),

    -- Audit logs
    (gen_random_uuid(), 'audit_logs.view',          'View audit log entries.'),

    -- Reports
    (gen_random_uuid(), 'reports.view',             'View system reports and dashboards.'),
    (gen_random_uuid(), 'reports.export',           'Export system reports.')
ON CONFLICT (name) DO NOTHING;

-- SECTION: ROLE-PERMISSION ASSIGNMENTS

-- Admin: every permission in the system
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Admin'
ON CONFLICT DO NOTHING;

-- IT Specialist: assets, assignments, categories, access fulfillment, reports
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'assets.view', 'assets.create', 'assets.update', 'assets.retire', 'assets.categories.manage',
    'assignments.view', 'assignments.create', 'assignments.return',
    'access_requests.view', 'access_requests.grant',
    'departments.view', 'teams.view', 'users.view',
    'reports.view'
)
WHERE r.name = 'IT Specialist'
ON CONFLICT DO NOTHING;

-- Team Lead: manage own team, approve access requests, view assets
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'users.view',
    'teams.view', 'teams.manage',
    'departments.view',
    'assets.view',
    'assignments.view',
    'access_requests.view', 'access_requests.create', 'access_requests.approve',
    'reports.view'
)
WHERE r.name = 'Team Lead'
ON CONFLICT DO NOTHING;

-- Security Officer: security events, audit logs, access oversight
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'security_events.view', 'security_events.manage',
    'audit_logs.view',
    'access_requests.view', 'access_requests.grant',
    'assets.view',
    'users.view',
    'reports.view', 'reports.export'
)
WHERE r.name = 'Security Officer'
ON CONFLICT DO NOTHING;

-- Employee: self-service basics - view assets, request access/assets
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'assets.view',
    'access_requests.view', 'access_requests.create',
    'assignments.view'
)
WHERE r.name = 'Employee'
ON CONFLICT DO NOTHING;

-- Auditor: read-only across audit-relevant areas
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'audit_logs.view',
    'security_events.view',
    'assets.view',
    'assignments.view',
    'access_requests.view',
    'users.view', 'departments.view', 'teams.view',
    'reports.view', 'reports.export'
)
WHERE r.name = 'Auditor'
ON CONFLICT DO NOTHING;

COMMIT;

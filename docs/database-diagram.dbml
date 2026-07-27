// IT Asset & Access Management System
// Database: PostgreSQL

// ==========================================
// ORGANIZATION AND USER MANAGEMENT
// ==========================================

Table departments {
  id integer [primary key, increment]
  name varchar(100) [not null, unique]
  description text
  created_at timestamp [not null]
  updated_at timestamp
}

Table teams {
  id integer [primary key, increment]
  department_id integer [not null]
  team_lead_user_id integer
  name varchar(100) [not null]
  description text
  created_at timestamp [not null]
  updated_at timestamp

  Indexes {
    department_id
    team_lead_user_id
    (department_id, name) [unique]
  }
}

Table users {
  id integer [primary key, increment]
  department_id integer
  team_id integer
  manager_id integer

  first_name varchar(100) [not null]
  last_name varchar(100) [not null]
  email varchar(255) [not null, unique]
  password_hash text [not null]

  job_title varchar(150)
  phone_number varchar(30)

  is_active boolean [not null, default: true]
  last_login_at timestamp
  created_at timestamp [not null]
  updated_at timestamp

  Indexes {
    department_id
    team_id
    manager_id
    email [unique]
  }
}

Table roles {
  id integer [primary key, increment]
  name varchar(100) [not null, unique]
  description text
  is_system_role boolean [not null, default: false]
  created_at timestamp [not null]
}

Table user_roles {
  user_id integer [not null]
  role_id integer [not null]
  assigned_by_user_id integer
  assigned_at timestamp [not null]

  Indexes {
    (user_id, role_id) [pk]
    role_id
  }
}

Table permissions {
  id integer [primary key, increment]
  name varchar(150) [not null, unique]
  description text
  created_at timestamp [not null]
}

Table role_permissions {
  role_id integer [not null]
  permission_id integer [not null]
  assigned_at timestamp [not null]

  Indexes {
    (role_id, permission_id) [pk]
    permission_id
  }
}

// ==========================================
// ASSET MANAGEMENT
// ==========================================

Table asset_categories {
  id integer [primary key, increment]
  name varchar(100) [not null, unique]

  // Physical or Digital
  asset_type varchar(20) [not null]

  description text
  created_at timestamp [not null]
  updated_at timestamp
}

Table assets {
  id integer [primary key, increment]
  category_id integer [not null]
  owner_department_id integer
  created_by_user_id integer [not null]

  name varchar(200) [not null]
  description text

  // Physical or Digital
  asset_type varchar(20) [not null]

  // Available, Assigned, InMaintenance, Lost, Retired, Expired
  status varchar(30) [not null]

  is_active boolean [not null, default: true]
  created_at timestamp [not null]
  updated_at timestamp

  Indexes {
    category_id
    owner_department_id
    created_by_user_id
    asset_type
    status
  }
}

Table physical_asset_details {
  asset_id integer [primary key]

  serial_number varchar(150) [unique]
  inventory_number varchar(150) [unique]

  brand varchar(100)
  model varchar(100)

  purchase_date date
  purchase_price decimal(12,2)
  warranty_end_date date

  location varchar(200)
  mac_address varchar(50)
  ip_address varchar(50)
}

Table digital_asset_details {
  asset_id integer [primary key]

  resource_url text
  host varchar(255)
  port integer

  // Development, Test, Staging, Production
  environment varchar(30)

  secret_reference text
  expiration_date date

  is_sensitive boolean [not null, default: false]
}

Table asset_assignments {
  id integer [primary key, increment]
  asset_id integer [not null]
  user_id integer [not null]
  assigned_by_user_id integer [not null]

  assigned_at timestamp [not null]
  expected_return_at timestamp
  returned_at timestamp

  // Active, Returned, Lost, Damaged
  status varchar(30) [not null]

  notes text

  Indexes {
    asset_id
    user_id
    assigned_by_user_id
    status
  }
}

Table asset_status_histories {
  id integer [primary key, increment]
  asset_id integer [not null]
  changed_by_user_id integer [not null]

  old_status varchar(30)
  new_status varchar(30) [not null]

  description text
  changed_at timestamp [not null]

  Indexes {
    asset_id
    changed_by_user_id
    changed_at
  }
}

// ==========================================
// ACCESS REQUEST MANAGEMENT
// ==========================================

Table access_requests {
  id integer [primary key, increment]
  requester_user_id integer [not null]
  asset_id integer [not null]

  requested_permission varchar(100) [not null]
  reason text [not null]

  requested_start_date timestamp
  requested_end_date timestamp

  // Pending, Approved, Rejected, Cancelled
  status varchar(30) [not null]

  created_at timestamp [not null]
  completed_at timestamp
  cancelled_at timestamp

  Indexes {
    requester_user_id
    asset_id
    status
    created_at
  }
}

Table access_request_approvals {
  id integer [primary key, increment]
  access_request_id integer [not null]
  approver_user_id integer [not null]

  approval_order integer [not null]

  // Pending, Approved, Rejected
  decision varchar(30) [not null]

  comment text
  decided_at timestamp
  created_at timestamp [not null]

  Indexes {
    access_request_id
    approver_user_id
    decision
    (access_request_id, approval_order) [unique]
  }
}

Table asset_accesses {
  id integer [primary key, increment]
  asset_id integer [not null]
  user_id integer [not null]
  access_request_id integer
  granted_by_user_id integer [not null]

  permission_level varchar(100) [not null]

  granted_at timestamp [not null]
  expires_at timestamp
  revoked_at timestamp
  revoked_by_user_id integer

  // Active, Expired, Revoked
  status varchar(30) [not null]

  Indexes {
    asset_id
    user_id
    access_request_id
    status
    expires_at
  }
}

// ==========================================
// SECURITY AND AUDIT
// ==========================================

Table audit_logs {
  id bigint [primary key, increment]
  user_id integer

  action varchar(150) [not null]
  entity_name varchar(150) [not null]
  entity_id varchar(100)

  old_values jsonb
  new_values jsonb

  ip_address varchar(50)
  user_agent text

  created_at timestamp [not null]

  Indexes {
    user_id
    entity_name
    entity_id
    action
    created_at
  }
}

Table security_events {
  id bigint [primary key, increment]
  user_id integer
  resolved_by_user_id integer

  // FailedLogin, UnauthorizedAccess, SuspiciousActivity
  event_type varchar(100) [not null]

  description text [not null]

  // Low, Medium, High, Critical
  severity varchar(20) [not null]

  ip_address varchar(50)
  is_resolved boolean [not null, default: false]

  created_at timestamp [not null]
  resolved_at timestamp

  Indexes {
    user_id
    event_type
    severity
    is_resolved
    created_at
  }
}

Table login_attempts {
  id bigint [primary key, increment]
  user_id integer

  email varchar(255) [not null]
  ip_address varchar(50)

  is_successful boolean [not null]
  failure_reason text

  attempted_at timestamp [not null]

  Indexes {
    user_id
    email
    ip_address
    is_successful
    attempted_at
  }
}

Table refresh_tokens {
  id bigint [primary key, increment]
  user_id integer [not null]

  token_hash text [not null, unique]

  expires_at timestamp [not null]
  created_at timestamp [not null]
  revoked_at timestamp

  is_revoked boolean [not null, default: false]

  Indexes {
    user_id
    expires_at
    is_revoked
  }
}

// ==========================================
// NOTIFICATIONS
// ==========================================

Table notifications {
  id bigint [primary key, increment]
  user_id integer [not null]

  title varchar(200) [not null]
  message text [not null]
  notification_type varchar(50)

  is_read boolean [not null, default: false]
  read_at timestamp
  created_at timestamp [not null]

  Indexes {
    user_id
    is_read
    created_at
  }
}

// ==========================================
// RELATIONSHIPS
// ==========================================

// Department and team relationships

Ref: teams.department_id > departments.id

Ref: teams.team_lead_user_id > users.id

Ref: users.department_id > departments.id

Ref: users.team_id > teams.id

// A user's manager is another user.

Ref: users.manager_id > users.id

// User, role and permission relationships

Ref: user_roles.user_id > users.id

Ref: user_roles.role_id > roles.id

Ref: user_roles.assigned_by_user_id > users.id

Ref: role_permissions.role_id > roles.id

Ref: role_permissions.permission_id > permissions.id

// Asset relationships

Ref: assets.category_id > asset_categories.id

Ref: assets.owner_department_id > departments.id

Ref: assets.created_by_user_id > users.id

Ref: physical_asset_details.asset_id - assets.id

Ref: digital_asset_details.asset_id - assets.id

Ref: asset_assignments.asset_id > assets.id

Ref: asset_assignments.user_id > users.id

Ref: asset_assignments.assigned_by_user_id > users.id

Ref: asset_status_histories.asset_id > assets.id

Ref: asset_status_histories.changed_by_user_id > users.id

// Access request relationships

Ref: access_requests.requester_user_id > users.id

Ref: access_requests.asset_id > assets.id

Ref: access_request_approvals.access_request_id > access_requests.id

Ref: access_request_approvals.approver_user_id > users.id

Ref: asset_accesses.asset_id > assets.id

Ref: asset_accesses.user_id > users.id

Ref: asset_accesses.access_request_id > access_requests.id

Ref: asset_accesses.granted_by_user_id > users.id

Ref: asset_accesses.revoked_by_user_id > users.id

// Security relationships

Ref: audit_logs.user_id > users.id

Ref: security_events.user_id > users.id

Ref: security_events.resolved_by_user_id > users.id

Ref: login_attempts.user_id > users.id

Ref: refresh_tokens.user_id > users.id

Ref: notifications.user_id > users.id
create table if not exists users (
    id integer generated always as identity primary key,
    telegram_chat_id bigint null unique,
    telegram_username varchar(100) not null unique,
    full_name varchar(200) not null,
    role varchar(50) not null,
    created_at timestamp not null default current_timestamp,
    constraint users_role_check check (role in ('Employee', 'Lead', 'Administrator'))
);

create table if not exists daily_reports (
    id integer generated always as identity primary key,
    employee_id integer not null references users(id) on delete cascade,
    report_date date not null,
    is_sent boolean not null default false,
    created_at timestamp not null default current_timestamp,
    constraint daily_reports_employee_date_unique unique (employee_id, report_date)
);

create table if not exists report_completed_tasks (
    id integer generated always as identity primary key,
    report_id integer not null references daily_reports(id) on delete cascade,
    task_text text not null
);

create table if not exists report_blocks (
    id integer generated always as identity primary key,
    report_id integer not null references daily_reports(id) on delete cascade,
    block_text text not null
);

create table if not exists employee_tasks (
    id integer generated always as identity primary key,
    employee_id integer not null references users(id) on delete cascade,
    lead_id integer not null references users(id) on delete cascade,
    title varchar(500) not null,
    deadline date not null,
    status varchar(50) not null default 'Open',
    closing_comment text null,
    created_at timestamp not null default current_timestamp,
    closed_at timestamp null,
    constraint employee_tasks_status_check check (status in ('Open', 'InProgress', 'Closed'))
);

create table if not exists bot_settings (
    id integer generated always as identity primary key,
    setting_key varchar(100) not null unique,
    setting_value varchar(500) not null
);

create index if not exists idx_daily_reports_employee_id on daily_reports(employee_id);
create index if not exists idx_daily_reports_report_date on daily_reports(report_date);
create index if not exists idx_employee_tasks_employee_id on employee_tasks(employee_id);
create index if not exists idx_employee_tasks_status on employee_tasks(status);
create index if not exists idx_employee_tasks_deadline on employee_tasks(deadline);

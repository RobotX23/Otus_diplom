insert into users (telegram_chat_id, telegram_username, full_name, role)
values
    (100001, 'ivan_ivanov', 'Иван Иванов', 'Employee'),
    (100002, 'petr_petrov', 'Петр Петров', 'Employee'),
    (200001, 'anna_lead', 'Анна Lead', 'Lead'),
    (300001, 'admin', 'Администратор', 'Administrator')
on conflict (telegram_chat_id) do nothing;

insert into daily_reports (employee_id, report_date, is_sent)
select id, current_date, true
from users
where full_name = 'Иван Иванов'
on conflict (employee_id, report_date) do nothing;

insert into report_completed_tasks (report_id, task_text)
select daily_reports.id, 'Сделал задачу по авторизации'
from daily_reports
join users on users.id = daily_reports.employee_id
where users.full_name = 'Иван Иванов'
  and daily_reports.report_date = current_date
  and not exists (
      select 1
      from report_completed_tasks
      where report_completed_tasks.report_id = daily_reports.id
        and report_completed_tasks.task_text = 'Сделал задачу по авторизации'
  );

insert into report_blocks (report_id, block_text)
select daily_reports.id, 'Проблем нет'
from daily_reports
join users on users.id = daily_reports.employee_id
where users.full_name = 'Иван Иванов'
  and daily_reports.report_date = current_date
  and not exists (
      select 1
      from report_blocks
      where report_blocks.report_id = daily_reports.id
        and report_blocks.block_text = 'Проблем нет'
  );

insert into employee_tasks (employee_id, lead_id, title, deadline, status)
select employee.id, lead_user.id, 'Подготовить отчет по проекту', current_date + interval '3 day', 'Open'
from users employee
cross join users lead_user
where employee.full_name = 'Иван Иванов'
  and lead_user.full_name = 'Анна Lead'
  and not exists (
      select 1
      from employee_tasks
      where employee_tasks.employee_id = employee.id
        and employee_tasks.title = 'Подготовить отчет по проекту'
  );

insert into employee_tasks (employee_id, lead_id, title, deadline, status)
select employee.id, lead_user.id, 'Проверить форму отправки отчета', current_date + interval '1 day', 'InProgress'
from users employee
cross join users lead_user
where employee.full_name = 'Петр Петров'
  and lead_user.full_name = 'Анна Lead'
  and not exists (
      select 1
      from employee_tasks
      where employee_tasks.employee_id = employee.id
        and employee_tasks.title = 'Проверить форму отправки отчета'
  );

insert into bot_settings (setting_key, setting_value)
values
    ('daily_report_reminder_time', '18:00'),
    ('task_deadline_reminder_hours', '24')
on conflict (setting_key) do nothing;

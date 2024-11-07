namespace PFMS_MI04.Services
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Hosting;
    using PFMS_MI04.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReminderSchedulerService : BackgroundService
    {
        private readonly EmailService _emailService;
        private readonly IHubContext<ReminderHub> _hubContext;

        public ReminderSchedulerService(EmailService emailService, IHubContext<ReminderHub> hubContext)
        {
            _emailService = emailService;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndSendRemindersAsync();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);  // Delay between checks
            }
        }

        private async Task CheckAndSendRemindersAsync()
        {
            // GMT+8 time zone
            TimeZoneInfo gmtPlus8 = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Convert UTC time to GMT+8
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, gmtPlus8);

            List<ReminderItem> allReminders = ReminderService.GetAllReminders();

            var dueReminders = allReminders.Where(r => r.DueDate.ToDateTime(r.DueTime) <= now).ToList();

            foreach (var reminder in dueReminders)
            {
                // Send email reminder
                bool emailSent = await _emailService.SendEmailAsync(
                    reminder.Email,
                    reminder.Title,
                    reminder.EmailContent,
                    reminder.UserName
                );

                if (emailSent)
                {
                    // Notify all clients about the reminder sent
                    await _hubContext.Clients.All.SendAsync("ReceiveReminderUpdate", "A reminder has been sent.");
                }

                // Delete reminder if email sent and log the event
                bool reminderDeletedAndLogged = await ReminderService.DeleteReminder(reminder, emailSent);

                if (reminderDeletedAndLogged)
                {
                    SecurityService.Log("Reminder Scheduler Service", "Automated Reminder Sent: Reminder Recipient:[" + reminder.Email + " ; " + reminder.Title + "]", "CHANGE", "Reminder Scheduler Service");
                }
                else
                {
                    SecurityService.Log("Reminder Scheduler Service", "Automated Reminder Failed: Reminder Recipient:[" + reminder.Email + " ; " + reminder.Title + "]", "ERROR", "Reminder Scheduler Service");
                }
            }
        }
    }
}

using BankingManagement.Data;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BankingManagement.Services
{
    public interface IEmailService
    {
        Task<EmailLog> SendTransactionEmailAsync(
            string recipientEmail,
            string recipientName,
            string transactionType,
            long accountNumber,
            int accountId,
            decimal amount,
            decimal balanceAfter,
            string? description,
            long? relatedAccountNumber = null
        );

        Task<List<EmailLog>> GetAllLogsAsync();
        Task<EmailLog?> GetLogByIdAsync(int id);
    }

    public class EmailService : IEmailService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            AppDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<EmailService> logger)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<EmailLog> SendTransactionEmailAsync(
            string recipientEmail,
            string recipientName,
            string transactionType,
            long accountNumber,
            int accountId,
            decimal amount,
            decimal balanceAfter,
            string? description,
            long? relatedAccountNumber = null)
        {
            var apiKey = _configuration["Resend:ApiKey"] ?? "re_HJPEXZJ5_6GvsWYzNSfYbL4vGQCRbfqfN";
            var fromEmail = _configuration["Resend:FromEmail"] ?? "Banking Management <onboarding@resend.dev>";

            var isDeposit = transactionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase);
            var isWithdraw = transactionType.Equals("Withdraw", StringComparison.OrdinalIgnoreCase);
            var isTransferDebit = transactionType.Equals("Transfer_Debit", StringComparison.OrdinalIgnoreCase) || 
                                  (transactionType.Equals("Transfer", StringComparison.OrdinalIgnoreCase) && relatedAccountNumber != null);
            var isTransferCredit = transactionType.Equals("Transfer_Credit", StringComparison.OrdinalIgnoreCase);

            string subject;
            string headline;
            string typeDisplay;
            string accentColor;

            if (isDeposit)
            {
                subject = $"Deposit Confirmation: Rs. {amount:N2} credited to A/C {accountNumber}";
                headline = "Deposit Successful";
                typeDisplay = "Cash Deposit";
                accentColor = "#059669"; // Emerald
            }
            else if (isWithdraw)
            {
                subject = $"Withdrawal Alert: Rs. {amount:N2} debited from A/C {accountNumber}";
                headline = "Withdrawal Processed";
                typeDisplay = "Cash Withdrawal";
                accentColor = "#d97706"; // Amber
            }
            else if (isTransferCredit)
            {
                subject = $"Credit Alert: Rs. {amount:N2} received in A/C {accountNumber}";
                headline = "Funds Received";
                typeDisplay = $"Transfer from A/C {relatedAccountNumber}";
                accentColor = "#059669";
            }
            else
            {
                subject = $"Transfer Alert: Rs. {amount:N2} transferred from A/C {accountNumber}";
                headline = "Transfer Sent";
                typeDisplay = relatedAccountNumber.HasValue ? $"Transfer to A/C {relatedAccountNumber.Value}" : "Fund Transfer";
                accentColor = "#2563eb"; // Royal Blue
            }

            var friendlyName = string.IsNullOrWhiteSpace(recipientName) ? "Valued Customer" : recipientName;
            var timeStamp = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");

            var plainText = new StringBuilder();
            plainText.AppendLine($"Hello {friendlyName},");
            plainText.AppendLine();
            plainText.AppendLine($"Your transaction of ₹{amount:N2} ({typeDisplay}) for account {accountNumber} has been successfully processed.");
            plainText.AppendLine($"Current Balance: ₹{balanceAfter:N2}");
            plainText.AppendLine($"Transaction Date: {timeStamp}");
            if (!string.IsNullOrWhiteSpace(description))
            {
                plainText.AppendLine($"Reference/Note: {description}");
            }
            plainText.AppendLine();
            plainText.AppendLine("Thank you for banking with us.");
            plainText.AppendLine("Banking Management System");

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append($@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f1f5f9; margin: 0; padding: 24px; color: #1e293b; }}
    .email-container {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.06); border: 1px solid #e2e8f0; }}
    .header {{ background: linear-gradient(135deg, #1e40af 0%, #2563eb 60%, #3b82f6 100%); padding: 30px; text-align: center; color: white; }}
    .header h1 {{ margin: 0; font-size: 22px; font-weight: 700; letter-spacing: -0.5px; }}
    .header p {{ margin: 6px 0 0; font-size: 13px; color: #dbeafe; }}
    .content {{ padding: 32px 30px; }}
    .greeting {{ font-size: 16px; font-weight: 600; color: #0f172a; margin-bottom: 12px; }}
    .status-badge {{ display: inline-block; padding: 6px 14px; border-radius: 9999px; background: {accentColor}18; color: {accentColor}; font-weight: 700; font-size: 13px; margin-bottom: 20px; border: 1px solid {accentColor}40; }}
    .card {{ background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; margin-bottom: 24px; }}
    .amount-row {{ text-align: center; padding: 14px 0 20px; border-bottom: 1px dashed #cbd5e1; margin-bottom: 16px; }}
    .amount-label {{ font-size: 12px; text-transform: uppercase; color: #64748b; font-weight: 600; letter-spacing: 0.5px; }}
    .amount-val {{ font-size: 32px; font-weight: 800; color: {accentColor}; margin: 4px 0 0; }}
    .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; font-size: 13.5px; }}
    .detail-label {{ color: #64748b; }}
    .detail-val {{ font-weight: 600; color: #0f172a; text-align: right; }}
    .security-note {{ font-size: 12px; color: #64748b; background: #fffbeb; border-left: 3px solid #f59e0b; padding: 10px 14px; border-radius: 4px; margin-bottom: 20px; }}
    .footer {{ background: #f8fafc; border-top: 1px solid #e2e8f0; padding: 20px; text-align: center; font-size: 11.5px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class='email-container'>
    <div class='header'>
      <h1>Banking Management</h1>
      <p>Official Transaction Notification</p>
    </div>
    <div class='content'>
      <div class='status-badge'>✔ {headline}</div>
      <div class='greeting'>Dear {friendlyName},</div>
      <p style='font-size: 14px; color: #475569; line-height: 1.5; margin-top: 0;'>
        Your account transaction has been processed successfully. Below are the details of the transaction:
      </p>
      
      <div class='card'>
        <div class='amount-row'>
          <div class='amount-label'>Transaction Amount</div>
          <div class='amount-val'>₹{amount:N2}</div>
        </div>
        <div class='detail-row'>
          <span class='detail-label'>Account Number</span>
          <span class='detail-val'>A/C {accountNumber}</span>
        </div>
        <div class='detail-row'>
          <span class='detail-label'>Transaction Type</span>
          <span class='detail-val'>{typeDisplay}</span>
        </div>
        <div class='detail-row'>
          <span class='detail-label'>Available Balance</span>
          <span class='detail-val' style='color: #059669;'>₹{balanceAfter:N2}</span>
        </div>
        <div class='detail-row'>
          <span class='detail-label'>Date & Time</span>
          <span class='detail-val'>{timeStamp}</span>
        </div>");

            if (!string.IsNullOrWhiteSpace(description))
            {
                htmlBuilder.Append($@"
        <div class='detail-row'>
          <span class='detail-label'>Description</span>
          <span class='detail-val'>{description}</span>
        </div>");
            }

            htmlBuilder.Append(@"
      </div>

      <div class='security-note'>
        <strong>Security Notice:</strong> If you did not recognize or authorize this transaction, please report it immediately to your bank branch or customer support.
      </div>
    </div>
    <div class='footer'>
      &copy; " + DateTime.UtcNow.Year + @" Banking Management System. All rights reserved.<br>
      This is an automated system notification. Please do not reply directly to this email.
    </div>
  </div>
</body>
</html>");

            var emailLog = new EmailLog
            {
                RecipientEmail = recipientEmail,
                RecipientName = friendlyName,
                Subject = subject,
                Message = plainText.ToString(),
                TransactionType = transactionType,
                AccountId = accountId,
                AccountNumber = accountNumber,
                Amount = amount,
                SentAt = DateTime.UtcNow,
                Status = "Pending"
            };

            // Call Resend API
            try
            {
                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    emailLog.Status = "Failed";
                    emailLog.ErrorMessage = "Recipient email address is missing or empty.";
                }
                else
                {
                    var client = _httpClientFactory.CreateClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    var payload = new
                    {
                        from = fromEmail,
                        to = new[] { recipientEmail },
                        subject = subject,
                        html = htmlBuilder.ToString(),
                        text = plainText.ToString()
                    };

                    var jsonOptions = new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };

                    request.Content = new StringContent(
                        JsonSerializer.Serialize(payload, jsonOptions),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        emailLog.Status = "Sent";
                        try
                        {
                            using var doc = JsonDocument.Parse(responseBody);
                            if (doc.RootElement.TryGetProperty("id", out var idProp))
                            {
                                emailLog.ResendEmailId = idProp.GetString();
                            }
                        }
                        catch
                        {
                            emailLog.ResendEmailId = responseBody;
                        }
                    }
                    else
                    {
                        emailLog.Status = "Failed";
                        try
                        {
                            using var doc = JsonDocument.Parse(responseBody);
                            if (doc.RootElement.TryGetProperty("message", out var msgProp))
                            {
                                emailLog.ErrorMessage = msgProp.GetString();
                            }
                            else
                            {
                                emailLog.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}";
                            }
                        }
                        catch
                        {
                            emailLog.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Resend API to {Email}", recipientEmail);
                emailLog.Status = "Failed";
                emailLog.ErrorMessage = ex.Message;
            }

            // Save to DB EmailLog table
            try
            {
                _context.EmailLogs.Add(emailLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save EmailLog to database");
            }

            return emailLog;
        }

        public async Task<List<EmailLog>> GetAllLogsAsync()
        {
            return await _context.EmailLogs
                .AsNoTracking()
                .OrderByDescending(l => l.SentAt)
                .ToListAsync();
        }

        public async Task<EmailLog?> GetLogByIdAsync(int id)
        {
            return await _context.EmailLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}

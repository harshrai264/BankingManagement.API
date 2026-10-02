using BankingManagement.Data;
using BankingManagement.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BankingManagement.Services
{
    public interface IAssistantService
    {
        Task<ChatResponseDto> ProcessQueryAsync(string query);
    }

    public class AssistantService : IAssistantService
    {
        private readonly AppDbContext _context;

        public AssistantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatResponseDto> ProcessQueryAsync(string rawQuery)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
            {
                return new ChatResponseDto
                {
                    Reply = "Hello! I am your **Banking AI Assistant**. You can ask me questions about customer accounts, balances, or transaction statistics. For example: *'Total transactions in last month'* or *'Customers with balance less than 1000'*.",
                    Category = "Greeting",
                    Suggestions = new List<string>
                    {
                        "Total transactions in last month",
                        "Customers with balance less than 1000",
                        "Total customers with active accounts",
                        "Total bank balance"
                    }
                };
            }

            // 1. Normalize Query & Auto-correct common typos (e.g. custerm -> customer, trans -> transaction)
            string normalized = NormalizeQuery(rawQuery);

            // 2. Check for Balance Comparison Queries (e.g. "customer with less than 1000 balance", "balance > 50000", etc.)
            var balanceFilterResult = await TryHandleBalanceFilterAsync(normalized, rawQuery);
            if (balanceFilterResult != null)
            {
                return balanceFilterResult;
            }

            // 3. Transactions in Last Month
            if (ContainsAny(normalized, "last month", "previous month", "past month") &&
                ContainsAny(normalized, "transaction", "trans", "activity", "count", "volume", "summary", "report", "how many", "all"))
            {
                return await GetLastMonthTransactionsAsync();
            }

            // 4. Transactions This Month
            if (ContainsAny(normalized, "this month", "current month") &&
                ContainsAny(normalized, "transaction", "trans", "activity", "count", "volume"))
            {
                return await GetThisMonthTransactionsAsync();
            }

            // 5. Transactions Today / Recent Activity
            if (ContainsAny(normalized, "today", "yesterday", "recent transaction", "latest transaction", "last transaction", "recent activity"))
            {
                return await GetRecentTransactionsAsync();
            }

            // 6. Deposit Breakdown
            if (ContainsAny(normalized, "total deposit", "how much deposit", "sum of deposit", "all deposit", "deposit count", "deposit volume"))
            {
                return await GetTransactionTypeSummaryAsync("Deposit");
            }

            // 7. Withdrawal Breakdown
            if (ContainsAny(normalized, "total withdraw", "how much withdraw", "sum of withdraw", "all withdraw", "withdraw count", "withdraw volume"))
            {
                return await GetTransactionTypeSummaryAsync("Withdraw");
            }

            // 8. Transfer Breakdown
            if (ContainsAny(normalized, "total transfer", "how much transfer", "sum of transfer", "all transfer", "transfer count", "transfer volume"))
            {
                return await GetTransactionTypeSummaryAsync("Transfer");
            }

            // 9. Customer with Most Transactions / Most Active Customer
            if (ContainsAny(normalized, "most transaction", "highest transaction", "maximum transaction", "top transaction", "most active customer", "who has most transaction", "which customer has most transaction", "highest number of transaction", "max transaction") ||
                (ContainsAny(normalized, "most", "highest", "maximum", "top", "greatest") && ContainsAny(normalized, "customer", "who", "which", "person", "account") && ContainsAny(normalized, "transaction", "activity", "trans")))
            {
                return await GetCustomerWithMostTransactionsAsync();
            }

            // 10. Total Customers with Active Accounts
            if ((ContainsAny(normalized, "active account", "active customer", "customers with active") ||
                 (ContainsAny(normalized, "active") && ContainsAny(normalized, "customer", "account"))) &&
                ContainsAny(normalized, "total", "customer", "count", "how many", "list", "all", "number"))
            {
                return await GetCustomersWithActiveAccountsAsync();
            }

            // 10. Total Inactive Accounts
            if (ContainsAny(normalized, "inactive", "suspended", "dormant", "closed") && ContainsAny(normalized, "account", "customer"))
            {
                return await GetInactiveAccountsAsync();
            }

            // 11. Total Bank Liquidity / Total Balance / Total Funds
            if (ContainsAny(normalized, "total balance", "bank balance", "total money", "liquidity", "vault balance", "total fund", "sum of balance", "how much money"))
            {
                return await GetTotalBankBalanceAsync();
            }

            // 12. Top / Highest Balance Accounts
            if (ContainsAny(normalized, "top account", "highest balance", "richest", "most money", "max balance", "maximum balance", "top customer", "largest account"))
            {
                return await GetTopBalanceAccountsAsync();
            }

            // 13. Direct Account Number Lookup (e.g. "account 100234" or "acct 100234" or "100234")
            var accountNum = ExtractAccountNumber(normalized);
            if (accountNum.HasValue)
            {
                var accountResult = await SearchAccountByNumberAsync(accountNum.Value);
                if (accountResult != null)
                {
                    return accountResult;
                }
            }

            // 14. Specific Customer Search (e.g., "customer Kishan", "details of John", "find Rahul", or direct name)
            var customerName = ExtractCustomerName(normalized, rawQuery);
            if (!string.IsNullOrEmpty(customerName))
            {
                var customerResult = await SearchCustomerByNameAsync(customerName);
                if (customerResult != null)
                {
                    return customerResult;
                }
            }

            // 15. All-time Total Transactions
            if (ContainsAny(normalized, "total transaction", "all transaction", "how many transaction", "transaction count", "number of transaction"))
            {
                return await GetAllTransactionsSummaryAsync();
            }

            // 16. Total Customers in system
            if (ContainsAny(normalized, "total customer", "how many customer", "customer count", "number of customer", "all customer"))
            {
                return await GetTotalCustomersCountAsync();
            }

            // 17. Total Accounts in system
            if (ContainsAny(normalized, "total account", "how many account", "account count", "number of account", "all account"))
            {
                return await GetTotalAccountsCountAsync();
            }

            // 18. Greetings / Help
            if (ContainsAny(normalized, "hi", "hello", "hey", "help", "who are you", "what can you do", "features", "options"))
            {
                return new ChatResponseDto
                {
                    Reply = "👋 **Hello! I am your AI Banking Assistant.**\n\nI can analyze live bank records and answer your questions directly:\n\n" +
                            "• 📊 **Transaction Analytics**: *'Total transactions in last month'*, *'Recent transactions'*, *'Total deposits'*\n" +
                            "• 👥 **Customer & Balance Filters**: *'Customers with balance less than 1000'*, *'Active accounts'*, *'Balance greater than 50000'*\n" +
                            "• 🔍 **Account & Name Lookup**: *'Customer Kishan'* or *'Account 100234'*\n" +
                            "• 💰 **Bank Liquidity**: *'Total bank balance'*, *'Top accounts by balance'*\n\n" +
                            "What would you like to check?",
                    Category = "Help",
                    Suggestions = new List<string>
                    {
                        "Total transactions in last month",
                        "Customers with balance less than 1000",
                        "Total customers with active accounts",
                        "Total bank balance"
                    }
                };
            }

            // 19. Smart Fallback with Live System Overview
            return await GetSmartFallbackResponseAsync(rawQuery);
        }

        #region Typo Normalization
        private static string NormalizeQuery(string raw)
        {
            var q = " " + raw.Trim().ToLowerInvariant() + " ";

            // Common typos for 'customer'
            q = Regex.Replace(q, @"\b(custerm|custmer|customr|custmr|costomer|coustomer|customar|custmers|custerms|customers)\b", "customer");

            // Common typos for 'account'
            q = Regex.Replace(q, @"\b(accout|accouts|acount|acounts|acct|accts|accs|acc|accounts)\b", "account");

            // Common typos for 'transaction'
            q = Regex.Replace(q, @"\b(trans|transction|tranaction|transection|transations|trnx|tranx|transactions)\b", "transaction");

            // Common typos for 'balance'
            q = Regex.Replace(q, @"\b(bal|balanc|blance|balnce|blnc|balances)\b", "balance");

            // Common typos for 'total'
            q = Regex.Replace(q, @"\b(toal|totl|totle|tota)\b", "total");

            // Common typos for 'active'
            q = Regex.Replace(q, @"\b(actv|atcive|activ)\b", "active");

            // Common typos for 'inactive'
            q = Regex.Replace(q, @"\b(inactv|inactve|inactiv|suspnd|suspnded)\b", "inactive");

            // Comparison grammar typos
            q = Regex.Replace(q, @"\bless\s+then\b", "less than");
            q = Regex.Replace(q, @"\bmore\s+then\b", "more than");
            q = Regex.Replace(q, @"\bgreater\s+then\b", "greater than");
            q = Regex.Replace(q, @"\bhigher\s+then\b", "higher than");
            q = Regex.Replace(q, @"\blower\s+then\b", "lower than");
            q = Regex.Replace(q, @"\bsmaller\s+then\b", "smaller than");

            // Transaction types typos
            q = Regex.Replace(q, @"\b(deposite|depost|deposits)\b", "deposit");
            q = Regex.Replace(q, @"\b(withraw|withdrw|withdrawl|withdrawals)\b", "withdraw");
            q = Regex.Replace(q, @"\b(transfr|transfered|transfers)\b", "transfer");

            return q.Trim();
        }
        #endregion

        #region Balance / Amount Filter Logic
        private async Task<ChatResponseDto?> TryHandleBalanceFilterAsync(string normalized, string raw)
        {
            // Case 1: Zero / Nil / Empty Balance
            if (Regex.IsMatch(normalized, @"\b(zero\s+balance|no\s+balance|nil\s+balance|0\s+balance|empty\s+account)\b"))
            {
                return await QueryAccountsByBalanceConditionAsync(0m, BalanceOperator.LessThanOrEqual, "zero or negative balance");
            }

            // Case 2: Low Balance / Minimum Balance
            if (Regex.IsMatch(normalized, @"\b(low\s+balance|minimum\s+balance)\b"))
            {
                return await QueryAccountsByBalanceConditionAsync(1000m, BalanceOperator.LessThanOrEqual, "low balance (₹1,000 or below)");
            }

            // Case 3: "Between X and Y"
            var betweenMatch = Regex.Match(normalized, @"\b(?:between)\s*(?:rs\.?|inr|₹)?\s*([0-9]+(?:\.[0-9]+)?(?:k|lakh|lac|m)?)\s*(?:and|to|-)\s*(?:rs\.?|inr|₹)?\s*([0-9]+(?:\.[0-9]+)?(?:k|lakh|lac|m)?)\b");
            if (betweenMatch.Success)
            {
                if (TryParseAmount(betweenMatch.Groups[1].Value, out decimal minVal) &&
                    TryParseAmount(betweenMatch.Groups[2].Value, out decimal maxVal))
                {
                    if (minVal > maxVal)
                    {
                        var temp = minVal;
                        minVal = maxVal;
                        maxVal = temp;
                    }
                    return await QueryAccountsBetweenBalancesAsync(minVal, maxVal);
                }
            }

            // Case 4: "Less than / Under / Below / Smaller than / Lower than / < / At most / Up to"
            var lessMatch = Regex.Match(normalized, @"\b(?:less\s+than|under|below|smaller\s+than|lower\s+than|at\s+most|up\s+to|<)\s*(?:rs\.?|inr|₹)?\s*([0-9]+(?:\.[0-9]+)?(?:k|lakh|lac|m|cr)?)\b");
            if (lessMatch.Success)
            {
                if (TryParseAmount(lessMatch.Groups[1].Value, out decimal threshold))
                {
                    return await QueryAccountsByBalanceConditionAsync(threshold, BalanceOperator.LessThan, $"less than ₹{threshold:N2}");
                }
            }

            // Case 5: "Greater than / More than / Above / Over / Higher than / > / At least / Exceeding"
            var greaterMatch = Regex.Match(normalized, @"\b(?:greater\s+than|more\s+than|above|over|higher\s+than|at\s+least|exceeding|>)\s*(?:rs\.?|inr|₹)?\s*([0-9]+(?:\.[0-9]+)?(?:k|lakh|lac|m|cr)?)\b");
            if (greaterMatch.Success)
            {
                if (TryParseAmount(greaterMatch.Groups[1].Value, out decimal threshold))
                {
                    return await QueryAccountsByBalanceConditionAsync(threshold, BalanceOperator.GreaterThan, $"greater than ₹{threshold:N2}");
                }
            }

            // Case 6: "Equal to / Equals / Exactly / ="
            var equalMatch = Regex.Match(normalized, @"\b(?:equal\s+to|equals|exactly|=)\s*(?:rs\.?|inr|₹)?\s*([0-9]+(?:\.[0-9]+)?(?:k|lakh|lac|m)?)\b");
            if (equalMatch.Success)
            {
                if (TryParseAmount(equalMatch.Groups[1].Value, out decimal target))
                {
                    return await QueryAccountsByBalanceConditionAsync(target, BalanceOperator.Equal, $"equal to ₹{target:N2}");
                }
            }

            return null;
        }

        private enum BalanceOperator
        {
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            Equal
        }

        private async Task<ChatResponseDto> QueryAccountsByBalanceConditionAsync(decimal threshold, BalanceOperator op, string conditionDescription)
        {
            var query = _context.Accounts.Include(a => a.Customer).AsQueryable();

            switch (op)
            {
                case BalanceOperator.LessThan:
                    query = query.Where(a => a.Balance < threshold).OrderBy(a => a.Balance);
                    break;
                case BalanceOperator.LessThanOrEqual:
                    query = query.Where(a => a.Balance <= threshold).OrderBy(a => a.Balance);
                    break;
                case BalanceOperator.GreaterThan:
                    query = query.Where(a => a.Balance > threshold).OrderByDescending(a => a.Balance);
                    break;
                case BalanceOperator.GreaterThanOrEqual:
                    query = query.Where(a => a.Balance >= threshold).OrderByDescending(a => a.Balance);
                    break;
                case BalanceOperator.Equal:
                    query = query.Where(a => a.Balance == threshold).OrderBy(a => a.AccountId);
                    break;
            }

            var accounts = await query.ToListAsync();

            if (!accounts.Any())
            {
                // Provide useful context: What are the min and max balances in the system?
                var minBalAccount = await _context.Accounts.Include(a => a.Customer).OrderBy(a => a.Balance).FirstOrDefaultAsync();
                var maxBalAccount = await _context.Accounts.Include(a => a.Customer).OrderByDescending(a => a.Balance).FirstOrDefaultAsync();

                string contextNote = "";
                if (minBalAccount != null && maxBalAccount != null)
                {
                    contextNote = $"\n\n💡 *System Reference:* Lowest balance is **₹{minBalAccount.Balance:N2}** ({minBalAccount.Customer?.Name ?? "Customer"}), and highest balance is **₹{maxBalAccount.Balance:N2}** ({maxBalAccount.Customer?.Name ?? "Customer"}).";
                }

                return new ChatResponseDto
                {
                    Reply = $"🔍 **No Accounts Found**\n\nThere are currently **0** accounts with balance **{conditionDescription}**.{contextNote}",
                    Category = "Balance Filter",
                    Suggestions = new List<string>
                    {
                        "Highest balance accounts",
                        "Total customers with active accounts",
                        "Total bank balance"
                    }
                };
            }

            decimal totalMatchingBalance = accounts.Sum(a => a.Balance);
            decimal avgBalance = totalMatchingBalance / accounts.Count;

            string reply = $"👥 **Customers with Balance {conditionDescription}:**\n\n" +
                           $"Found **{accounts.Count}** matching account{(accounts.Count > 1 ? "s" : "")}:\n\n";

            int displayCount = Math.Min(accounts.Count, 10);
            for (int i = 0; i < displayCount; i++)
            {
                var acc = accounts[i];
                var customerName = acc.Customer?.Name ?? "Unknown";
                var phone = !string.IsNullOrEmpty(acc.Customer?.Phone) ? $" | 📞 {acc.Customer.Phone}" : "";
                var statusIcon = acc.Status == "Active" ? "🟢" : "⚪";

                reply += $"• **{customerName}** — Acct #{acc.AccountNumber}: **₹{acc.Balance:N2}** ({statusIcon} {acc.Status}{phone})\n";
            }

            if (accounts.Count > displayCount)
            {
                reply += $"\n*(Showing top {displayCount} of {accounts.Count} matching accounts)*\n";
            }

            reply += $"\n📊 **Summary Metrics:**\n" +
                     $"• **Total Accounts**: **{accounts.Count}**\n" +
                     $"• **Combined Balance**: **₹{totalMatchingBalance:N2}**\n" +
                     $"• **Average Balance**: **₹{avgBalance:N2}**";

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Balance Filter",
                StructuredData = new { Count = accounts.Count, TotalBalance = totalMatchingBalance, AverageBalance = avgBalance },
                Suggestions = new List<string>
                {
                    op == BalanceOperator.LessThan ? $"Customers with balance greater than {threshold}" : $"Customers with balance less than {threshold}",
                    "Highest balance accounts",
                    "Total customers with active accounts",
                    "Total bank balance"
                }
            };
        }

        private async Task<ChatResponseDto> QueryAccountsBetweenBalancesAsync(decimal min, decimal max)
        {
            var accounts = await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Balance >= min && a.Balance <= max)
                .OrderBy(a => a.Balance)
                .ToListAsync();

            if (!accounts.Any())
            {
                return new ChatResponseDto
                {
                    Reply = $"🔍 **No Accounts Found**\n\nThere are no accounts with balance between **₹{min:N2}** and **₹{max:N2}**.",
                    Category = "Balance Filter",
                    Suggestions = new List<string> { "Highest balance accounts", "Total bank balance" }
                };
            }

            decimal totalBal = accounts.Sum(a => a.Balance);

            string reply = $"👥 **Customers with Balance between ₹{min:N2} and ₹{max:N2}:**\n\n" +
                           $"Found **{accounts.Count}** matching account{(accounts.Count > 1 ? "s" : "")}:\n\n";

            int displayCount = Math.Min(accounts.Count, 10);
            for (int i = 0; i < displayCount; i++)
            {
                var acc = accounts[i];
                var name = acc.Customer?.Name ?? "Unknown";
                reply += $"• **{name}** — Acct #{acc.AccountNumber}: **₹{acc.Balance:N2}** ({acc.Status})\n";
            }

            if (accounts.Count > displayCount)
            {
                reply += $"\n*(Showing first {displayCount} of {accounts.Count} accounts)*\n";
            }

            reply += $"\n• **Total Volume**: **₹{totalBal:N2}**";

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Balance Filter",
                Suggestions = new List<string> { "Total bank balance", "Highest balance accounts" }
            };
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim().Replace(",", "").Replace("₹", "").Replace("rs.", "").Replace("rs", "").Trim();

            decimal multiplier = 1;
            if (text.EndsWith("k", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1000;
                text = text[..^1].Trim();
            }
            else if (text.EndsWith("lakh", StringComparison.OrdinalIgnoreCase) || text.EndsWith("lac", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 100000;
                text = text.Replace("lakh", "", StringComparison.OrdinalIgnoreCase).Replace("lac", "", StringComparison.OrdinalIgnoreCase).Trim();
            }
            else if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 1000000;
                text = text[..^1].Trim();
            }
            else if (text.EndsWith("cr", StringComparison.OrdinalIgnoreCase) || text.EndsWith("crore", StringComparison.OrdinalIgnoreCase))
            {
                multiplier = 10000000;
                text = text.Replace("crore", "", StringComparison.OrdinalIgnoreCase).Replace("cr", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
            {
                amount = val * multiplier;
                return true;
            }

            return false;
        }
        #endregion

        #region Standard Analytical Queries
        private async Task<ChatResponseDto> GetLastMonthTransactionsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfLastMonth = startOfCurrentMonth.AddMonths(-1);
            var endOfLastMonth = startOfCurrentMonth;

            var lastMonthTx = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfLastMonth && t.TransactionDate < endOfLastMonth)
                .ToListAsync();

            int totalCount = lastMonthTx.Count;
            decimal totalVolume = lastMonthTx.Sum(t => t.Amount);

            var deposits = lastMonthTx.Where(t => t.TransactionType == "Deposit").ToList();
            var withdrawals = lastMonthTx.Where(t => t.TransactionType == "Withdraw").ToList();
            var transfers = lastMonthTx.Where(t => t.TransactionType == "Transfer").ToList();

            string monthName = startOfLastMonth.ToString("MMMM yyyy");

            string reply = $"📊 **Transaction Report for {monthName}:**\n\n" +
                           $"• **Total Transactions**: **{totalCount}**\n" +
                           $"• **Total Volume Moved**: **₹{totalVolume:N2}**\n\n" +
                           $"**Breakdown by Type:**\n" +
                           $"• 📥 **Deposits**: **{deposits.Count}** (₹{deposits.Sum(d => d.Amount):N2})\n" +
                           $"• 📤 **Withdrawals**: **{withdrawals.Count}** (₹{withdrawals.Sum(w => w.Amount):N2})\n" +
                           $"• 🔄 **Transfers**: **{transfers.Count}** (₹{transfers.Sum(t => t.Amount):N2})\n";

            if (totalCount == 0)
            {
                reply += $"\n*(Note: No transactions were logged in {monthName}. You can check this month's transactions instead.)*";
            }

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Transactions",
                StructuredData = new { Month = monthName, Total = totalCount, Volume = totalVolume },
                Suggestions = new List<string>
                {
                    "Transactions this month",
                    "Total customers with active accounts",
                    "Total bank balance"
                }
            };
        }

        private async Task<ChatResponseDto> GetThisMonthTransactionsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var thisMonthTx = await _context.Transactions
                .Where(t => t.TransactionDate >= startOfCurrentMonth)
                .ToListAsync();

            int count = thisMonthTx.Count;
            decimal volume = thisMonthTx.Sum(t => t.Amount);
            string monthName = now.ToString("MMMM yyyy");

            return new ChatResponseDto
            {
                Reply = $"📅 **Transaction Report for Current Month ({monthName}):**\n\n" +
                        $"• **Total Transactions so far**: **{count}**\n" +
                        $"• **Total Volume Moved**: **₹{volume:N2}**\n" +
                        $"• 📥 **Deposits**: **{thisMonthTx.Count(t => t.TransactionType == "Deposit")}** (₹{thisMonthTx.Where(t => t.TransactionType == "Deposit").Sum(t => t.Amount):N2})\n" +
                        $"• 📤 **Withdrawals**: **{thisMonthTx.Count(t => t.TransactionType == "Withdraw")}** (₹{thisMonthTx.Where(t => t.TransactionType == "Withdraw").Sum(t => t.Amount):N2})\n" +
                        $"• 🔄 **Transfers**: **{thisMonthTx.Count(t => t.TransactionType == "Transfer")}** (₹{thisMonthTx.Where(t => t.TransactionType == "Transfer").Sum(t => t.Amount):N2})",
                Category = "Transactions",
                Suggestions = new List<string>
                {
                    "Total transactions in last month",
                    "Recent transactions",
                    "Customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetRecentTransactionsAsync()
        {
            var recent = await _context.Transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(5)
                .ToListAsync();

            if (!recent.Any())
            {
                return new ChatResponseDto
                {
                    Reply = "There are no transactions recorded in the system yet.",
                    Category = "Transactions",
                    Suggestions = new List<string> { "Total customers", "Total bank balance" }
                };
            }

            string reply = "⏱️ **5 Most Recent Transactions:**\n\n";
            foreach (var t in recent)
            {
                var icon = t.TransactionType == "Deposit" ? "📥 Deposit" : (t.TransactionType == "Withdraw" ? "📤 Withdraw" : "🔄 Transfer");
                reply += $"• **{icon}**: **₹{t.Amount:N2}** on Account #{t.AccountId} ({t.TransactionDate:dd MMM yyyy, hh:mm tt}) - *Post Balance: ₹{t.BalanceAfterTransaction:N2}*\n";
            }

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Transactions",
                Suggestions = new List<string>
                {
                    "Total transactions in last month",
                    "Total bank balance",
                    "Customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetTransactionTypeSummaryAsync(string type)
        {
            var txs = await _context.Transactions
                .Where(t => t.TransactionType == type)
                .ToListAsync();

            int count = txs.Count;
            decimal sum = txs.Sum(t => t.Amount);

            return new ChatResponseDto
            {
                Reply = $"💰 **{type} Operations Overview:**\n\n" +
                        $"• **Total {type} Operations**: **{count}**\n" +
                        $"• **Total {type} Volume**: **₹{sum:N2}**\n" +
                        $"• **Average {type} Amount**: **₹{(count > 0 ? sum / count : 0):N2}**",
                Category = "Transactions",
                Suggestions = new List<string>
                {
                    "Total transactions in last month",
                    "Which customer has most transactions",
                    "Total bank balance",
                    "Customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetCustomerWithMostTransactionsAsync()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Account)
                    .ThenInclude(a => a.Customer)
                .ToListAsync();

            if (!transactions.Any())
            {
                return new ChatResponseDto
                {
                    Reply = "There are no transactions recorded in the bank database yet.",
                    Category = "Analytics",
                    Suggestions = new List<string>
                    {
                        "Total customers",
                        "Total bank balance",
                        "Customers with active accounts"
                    }
                };
            }

            var customerGroups = transactions
                .Where(t => t.Account?.Customer != null)
                .GroupBy(t => t.Account.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Customer = g.First().Account.Customer!,
                    Account = g.First().Account,
                    Count = g.Count(),
                    TotalVolume = g.Sum(t => t.Amount),
                    Deposits = g.Count(t => t.TransactionType == "Deposit"),
                    Withdrawals = g.Count(t => t.TransactionType == "Withdraw"),
                    Transfers = g.Count(t => t.TransactionType == "Transfer")
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!customerGroups.Any())
            {
                return new ChatResponseDto
                {
                    Reply = "No customer accounts could be mapped to the recorded transactions.",
                    Category = "Analytics"
                };
            }

            var top = customerGroups.First();

            string reply = $"🥇 **Customer with the Most Transactions:**\n\n" +
                           $"The customer with the highest transaction activity is **{top.Customer.Name}** with **{top.Count} total transactions**!\n\n" +
                           $"**Profile & Activity Breakdown:**\n" +
                           $"• **Customer**: **{top.Customer.Name}**\n" +
                           $"• **Account Number**: #{top.Account.AccountNumber}\n" +
                           $"• **Account Status**: {top.Account.Status}\n" +
                           $"• **Current Balance**: **₹{top.Account.Balance:N2}**\n" +
                           $"• **Total Transactions**: **{top.Count}**\n" +
                           $"• **Total Money Moved**: **₹{top.TotalVolume:N2}**\n" +
                           $"• **Activity Split**: 📥 {top.Deposits} Deposits | 📤 {top.Withdrawals} Withdrawals | 🔄 {top.Transfers} Transfers\n";

            if (customerGroups.Count > 1)
            {
                reply += $"\n🏆 **Top Active Customers Leaderboard:**\n";
                int rank = 1;
                foreach (var c in customerGroups.Take(5))
                {
                    var medal = rank == 1 ? "🥇" : (rank == 2 ? "🥈" : (rank == 3 ? "🥉" : $"{rank}."));
                    reply += $"{medal} **{c.Customer.Name}** — **{c.Count} transactions** (Acct #{c.Account.AccountNumber} | Balance: ₹{c.Account.Balance:N2})\n";
                    rank++;
                }
            }

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Analytics",
                StructuredData = new
                {
                    CustomerName = top.Customer.Name,
                    TransactionCount = top.Count,
                    TotalVolume = top.TotalVolume,
                    Balance = top.Account.Balance
                },
                Suggestions = new List<string>
                {
                    $"Details of {top.Customer.Name}",
                    "Highest balance accounts",
                    "Total transactions in last month",
                    "Total bank balance"
                }
            };
        }

        private async Task<ChatResponseDto> GetCustomersWithActiveAccountsAsync()
        {
            var activeAccounts = await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Status == "Active")
                .ToListAsync();

            var distinctCustomerIds = activeAccounts.Select(a => a.CustomerId).Distinct().ToList();
            int totalActiveCustomers = distinctCustomerIds.Count;
            int totalActiveAccounts = activeAccounts.Count;
            decimal totalActiveBalance = activeAccounts.Sum(a => a.Balance);

            var sampleCustomers = activeAccounts
                .Where(a => a.Customer != null)
                .Select(a => $"{a.Customer!.Name} (Acct #{a.AccountNumber} - ₹{a.Balance:N2})")
                .Take(5)
                .ToList();

            string sampleList = sampleCustomers.Any()
                ? "\n**Sample Active Accounts:**\n• " + string.Join("\n• ", sampleCustomers)
                : "";

            return new ChatResponseDto
            {
                Reply = $"👥 **Active Customers & Accounts Summary:**\n\n" +
                        $"• **Total Customers with Active Accounts**: **{totalActiveCustomers}**\n" +
                        $"• **Total Active Accounts**: **{totalActiveAccounts}**\n" +
                        $"• **Cumulative Active Balance**: **₹{totalActiveBalance:N2}**" +
                        sampleList,
                Category = "Customers",
                StructuredData = new { ActiveCustomers = totalActiveCustomers, ActiveAccounts = totalActiveAccounts },
                Suggestions = new List<string>
                {
                    "Inactive accounts",
                    "Customers with balance less than 1000",
                    "Total transactions in last month",
                    "Highest balance accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetInactiveAccountsAsync()
        {
            var inactiveAccounts = await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Status != "Active")
                .ToListAsync();

            int count = inactiveAccounts.Count;

            string details = count > 0
                ? "\n**Inactive Account List:**\n• " + string.Join("\n• ", inactiveAccounts.Select(a => $"{a.Customer?.Name ?? "Unknown"} - #{a.AccountNumber} (Balance: ₹{a.Balance:N2})"))
                : "\n*Great news! There are currently no inactive or suspended accounts.*";

            return new ChatResponseDto
            {
                Reply = $"⚠️ **Inactive / Suspended Accounts Report:**\n\n" +
                        $"• **Total Inactive Accounts**: **{count}**" +
                        details,
                Category = "Accounts",
                Suggestions = new List<string>
                {
                    "Total customers with active accounts",
                    "Total bank balance",
                    "Total transactions in last month"
                }
            };
        }

        private async Task<ChatResponseDto> GetTotalBankBalanceAsync()
        {
            var accounts = await _context.Accounts.ToListAsync();
            decimal totalBalance = accounts.Sum(a => a.Balance);
            int totalAccounts = accounts.Count;
            int activeAccounts = accounts.Count(a => a.Status == "Active");

            return new ChatResponseDto
            {
                Reply = $"💰 **Bank Liquidity & Balance Overview:**\n\n" +
                        $"• **Total Vault Balance**: **₹{totalBalance:N2}**\n" +
                        $"• **Total Accounts**: **{totalAccounts}** ({activeAccounts} active)\n" +
                        $"• **Average Account Balance**: **₹{(totalAccounts > 0 ? totalBalance / totalAccounts : 0):N2}**",
                Category = "Balances",
                Suggestions = new List<string>
                {
                    "Highest balance accounts",
                    "Customers with balance less than 1000",
                    "Total transactions in last month",
                    "Customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetTopBalanceAccountsAsync()
        {
            var topAccounts = await _context.Accounts
                .Include(a => a.Customer)
                .OrderByDescending(a => a.Balance)
                .Take(5)
                .ToListAsync();

            if (!topAccounts.Any())
            {
                return new ChatResponseDto
                {
                    Reply = "No account records found in the database.",
                    Category = "Balances"
                };
            }

            string reply = "🏆 **Top 5 Accounts by Balance:**\n\n";
            int rank = 1;
            foreach (var acc in topAccounts)
            {
                reply += $"{rank}. **{acc.Customer?.Name ?? "Customer"}** — Acct #{acc.AccountNumber}: **₹{acc.Balance:N2}** ({acc.Status})\n";
                rank++;
            }

            return new ChatResponseDto
            {
                Reply = reply,
                Category = "Balances",
                Suggestions = new List<string>
                {
                    "Total bank balance",
                    "Customers with balance less than 1000",
                    "Total customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto?> SearchAccountByNumberAsync(long accountNumber)
        {
            var account = await _context.Accounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber || a.AccountId == accountNumber);

            if (account == null) return null;

            return new ChatResponseDto
            {
                Reply = $"🏦 **Account #{account.AccountNumber} Details:**\n\n" +
                        $"• **Account Holder**: **{account.Customer?.Name ?? "N/A"}**\n" +
                        $"• **Current Balance**: **₹{account.Balance:N2}**\n" +
                        $"• **Status**: **{account.Status}**\n" +
                        $"• **Customer Email**: {account.Customer?.Email ?? "N/A"}\n" +
                        $"• **Customer Phone**: {account.Customer?.Phone ?? "N/A"}",
                Category = "Account Lookup",
                Suggestions = new List<string>
                {
                    "Total bank balance",
                    "Recent transactions",
                    "Customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto?> SearchCustomerByNameAsync(string name)
        {
            var customer = await _context.Customers
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, $"%{name}%"));

            if (customer == null)
            {
                return null;
            }

            string accInfo = customer.Account != null
                ? $"• **Account Number**: **{customer.Account.AccountNumber}**\n" +
                  $"• **Account Status**: **{customer.Account.Status}**\n" +
                  $"• **Current Balance**: **₹{customer.Account.Balance:N2}**"
                : "• *No bank account opened yet for this customer.*";

            return new ChatResponseDto
            {
                Reply = $"👤 **Customer Profile Found:**\n\n" +
                        $"• **Name**: **{customer.Name}**\n" +
                        $"• **Email**: {customer.Email}\n" +
                        $"• **Phone**: {customer.Phone}\n" +
                        $"• **Address**: {customer.Address}\n\n" +
                        $"**Banking Details:**\n" +
                        accInfo,
                Category = "Customer Lookup",
                Suggestions = new List<string>
                {
                    "Customers with balance less than 1000",
                    "Customers with active accounts",
                    "Total bank balance"
                }
            };
        }

        private async Task<ChatResponseDto> GetAllTransactionsSummaryAsync()
        {
            var totalCount = await _context.Transactions.CountAsync();
            var totalVolume = await _context.Transactions.SumAsync(t => (decimal?)t.Amount) ?? 0m;

            return new ChatResponseDto
            {
                Reply = $"📈 **All-Time Transaction Summary:**\n\n" +
                        $"• **Total Transactions Logged**: **{totalCount}**\n" +
                        $"• **Total Cumulative Volume**: **₹{totalVolume:N2}**",
                Category = "Transactions",
                Suggestions = new List<string>
                {
                    "Total transactions in last month",
                    "Recent transactions",
                    "Total customers with active accounts"
                }
            };
        }

        private async Task<ChatResponseDto> GetTotalCustomersCountAsync()
        {
            var total = await _context.Customers.CountAsync();
            var withAccounts = await _context.Accounts.Where(a => a.Status == "Active").Select(a => a.CustomerId).Distinct().CountAsync();

            return new ChatResponseDto
            {
                Reply = $"👥 **Customer Count:**\n\n" +
                        $"• **Total Registered Customers**: **{total}**\n" +
                        $"• **Customers with Active Accounts**: **{withAccounts}**",
                Category = "Customers",
                Suggestions = new List<string>
                {
                    "Customers with active accounts",
                    "Customers with balance less than 1000",
                    "Total bank balance"
                }
            };
        }

        private async Task<ChatResponseDto> GetTotalAccountsCountAsync()
        {
            var total = await _context.Accounts.CountAsync();
            var active = await _context.Accounts.CountAsync(a => a.Status == "Active");
            var inactive = total - active;

            return new ChatResponseDto
            {
                Reply = $"🏦 **Account Count:**\n\n" +
                        $"• **Total Accounts**: **{total}**\n" +
                        $"• **Active Accounts**: **{active}**\n" +
                        $"• **Inactive Accounts**: **{inactive}**",
                Category = "Accounts",
                Suggestions = new List<string>
                {
                    "Customers with active accounts",
                    "Total bank balance",
                    "Total transactions in last month"
                }
            };
        }

        private async Task<ChatResponseDto> GetSmartFallbackResponseAsync(string query)
        {
            var customerCount = await _context.Customers.CountAsync();
            var activeAccountCount = await _context.Accounts.CountAsync(a => a.Status == "Active");
            var totalTx = await _context.Transactions.CountAsync();
            var totalBalance = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0m;

            return new ChatResponseDto
            {
                Reply = $"I understood your inquiry, but could not find matching records for *\"{query}\"*.\n\n" +
                        $"**Live Bank Status:**\n" +
                        $"• **Customers**: {customerCount} registered ({activeAccountCount} active)\n" +
                        $"• **Total Vault Balance**: ₹{totalBalance:N2}\n" +
                        $"• **Total Transactions**: {totalTx} logged\n\n" +
                        $"💡 *Try asking one of the suggestions below:*",
                Category = "Overview",
                Suggestions = new List<string>
                {
                    "Customers with balance less than 1000",
                    "Total transactions in last month",
                    "Total customers with active accounts",
                    "Highest balance accounts"
                }
            };
        }
        #endregion

        #region Helper Methods
        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private static long? ExtractAccountNumber(string query)
        {
            var match = Regex.Match(query, @"\b(?:account|acct|#)\s*([0-9]{4,12})\b", RegexOptions.IgnoreCase);
            if (match.Success && long.TryParse(match.Groups[1].Value, out long num))
            {
                return num;
            }
            return null;
        }

        private static string? ExtractCustomerName(string normalized, string rawQuery)
        {
            // Do NOT extract names if query contains comparison terms, operators, or numbers
            if (ContainsAny(normalized, "less than", "greater than", "more than", "under", "below", "above", "between", "zero balance", "low balance", "active", "inactive"))
            {
                return null;
            }

            var patterns = new[]
            {
                @"customer\s+([a-zA-Z\s]{2,30})",
                @"details\s+of\s+([a-zA-Z\s]{2,30})",
                @"find\s+([a-zA-Z\s]{2,30})",
                @"who\s+is\s+([a-zA-Z\s]{2,30})",
                @"balance\s+of\s+([a-zA-Z\s]{2,30})"
            };

            var blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "with", "active", "accounts", "account", "all", "total", "last", "month", "today",
                "yesterday", "balance", "transactions", "transaction", "customers", "customer",
                "money", "count", "list", "show", "get", "bank"
            };

            foreach (var p in patterns)
            {
                var match = Regex.Match(rawQuery, p, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    var name = match.Groups[1].Value.Trim();
                    var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0 && !blacklist.Contains(words[0]) && name.Length >= 2)
                    {
                        return name;
                    }
                }
            }

            // If the raw query itself looks like just a single person's name (e.g., "Kishan" or "John Doe")
            var trimmedRaw = rawQuery.Trim();
            if (Regex.IsMatch(trimmedRaw, @"^[a-zA-Z]{3,20}(?:\s+[a-zA-Z]{3,20})?$"))
            {
                if (!blacklist.Contains(trimmedRaw))
                {
                    return trimmedRaw;
                }
            }

            return null;
        }
        #endregion
    }
}

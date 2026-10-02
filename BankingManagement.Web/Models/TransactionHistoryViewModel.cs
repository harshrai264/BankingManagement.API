namespace BankingManagement.Web.Models
{
    public class TransactionHistoryViewModel
    {
        public int? SelectedAccountId { get; set; }
        public AccountSelectionModel? SelectedAccount { get; set; }
        public List<TransactionHistoryModel> Transactions { get; set; } = new List<TransactionHistoryModel>();
        public List<AccountSelectionModel> Accounts { get; set; } = new List<AccountSelectionModel>();
    }
}

namespace MTS.DAL.Dtos
{
	public class WalletDto
	{
		public Guid UserId { get; set; }
		public decimal Balance { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public List<WalletTransactionDto> Transactions { get; set; }
	}
}

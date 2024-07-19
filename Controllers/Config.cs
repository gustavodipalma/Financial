using DBConnection;
using DBFinancial.Tables;

namespace Financial.Controllers
{
    public class Config
    {
        public void CreateTransaction(UsersDBConnection _db, int payerId, int payeeId, double value)
        {
            try
            {
                var transfer = new TransferHistory
                {
                    PayerId = payerId,
                    PayeeId = payeeId,
                    Value = (float)value,
                    ExecutionDate = DateTime.Now,
                    Status = "Completed"
                };

                _db.transferHistory.Add(transfer);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error = " + ex);
            }
        }
    }
}

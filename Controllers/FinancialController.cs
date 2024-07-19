using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using DBConnection;
using DBFinancial.Tables;
using Financial.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Financial.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class FinancialController : ControllerBase
    {
        private UsersDBConnection _db;

        public FinancialController(UsersDBConnection db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet]
        [Route("[Controller]/GetUser/{id}")]
        [ProducesResponseType<UserAccount>(200)]
        public IActionResult GetUser(int id)
        {
            try
            {
                var users = _db.userAccount.Where(f => f.Id == id);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        [Route("[Controller]/CheckBalance/{userId}")]
        public IActionResult CheckBalance(int userId)
        {
            try
            {
                var amounts = _db.account.Where(f => f.UserAccount.Id == userId).Select(s => s.Amount).FirstOrDefault();
                return Ok(new { Amount = amounts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [Route("[Controller]/CreateTransfer/")]
        public IActionResult CreateTransfer([FromBody] TransferHistoryViewModel transfer)
        {
            try
            {
                Config config = new Config();
                int payerId = transfer.payerId;
                int payeeId = transfer.payeeId;
                double value = transfer.value;

                string payerType = _db.userAccount.Where(f => f.Id == payerId).Select(s => s.PersonType).FirstOrDefault();
                if (payerType == "J")
                {
                    throw new Exception("Pessoa juridica nao pode enviar transferencia");
                }

                var payerAmount = _db.account.Where(f => f.UserAccount.Id == payerId).Select(s => s.Amount).FirstOrDefault();
                if(payerAmount <= 0)
                {
                    throw new Exception("Voce nao tem saldo suficiente");
                }
                var payer = _db.account.Where(f => f.UserAccount.Id == payerId).FirstOrDefault();
                var payee = _db.account.Where(f => f.UserAccount.Id == payeeId).FirstOrDefault();
                if (payer != null)
                {
                    payer.Amount = payer.Amount - value;
                    _db.Entry(payer).State = EntityState.Modified;
                }
                if (payee != null)
                {
                    payee.Amount = payee.Amount + value;
                    _db.Entry(payee).State = EntityState.Modified;
                }
                _db.SaveChanges();
                config.CreateTransaction(_db, payerId, payeeId, value);

                return Ok(new { Status = "Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}

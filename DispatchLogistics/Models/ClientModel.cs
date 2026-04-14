using System;

namespace DispatchLogistics.Models
{
    /// <summary>
    /// Модель клиента
    /// </summary>
    public class ClientModel
    {
        public int ClientId { get; set; }
        public string ClientType { get; set; }       // "Юр. лицо" / "Физ. лицо"
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }
        public string Notes { get; set; }
    }
}

using RestAPI.Models.Data;

namespace RestAPI.Models.Services
{
    public class AuthService
    {
        private readonly FirstDbContext db;
        public AuthService()
        {
            db = new FirstDbContext();
        }
        public Client Login(string login,string password)
        {
            Client client=db.Clients.FirstOrDefault(x => x.Login == login&&x.Passport==password)!;
            if (client != null) return client;
            return null!;
        }
        public bool Register(Client regClient)
        {
            Client client=db.Clients.FirstOrDefault(p=>p.Login==regClient.Login)!;
            if(client!=null)
            {
                db.Clients.Add(regClient);
                return true;
            }
            return false;
        }
    }
}

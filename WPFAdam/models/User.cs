using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WPFAdam.models
{
    public class User
    {
        private static string saveFile = AppDomain.CurrentDomain.BaseDirectory + "/users.txt";
        public string VNaam { get; set; }
        public string Naam { get; set; }
        public string RijksRegister { get; set; }
        public List<Tijdslot> TijdSloten { get; set; }
       

        public User(string vnaam, string naam, string rr)
        {
            this.VNaam = vnaam;
            this.Naam = naam;
            this.RijksRegister = rr;
            this.TijdSloten = new List<Tijdslot>();
        }

        public override string ToString()
        {
            return this.VNaam + " " + this.Naam;
        }

        public static List<User> LoadUsers()
        {
            List<User> users = new List<User>();
            if (File.Exists(saveFile))
            {
                using (StreamReader reader = new StreamReader(saveFile))
                {
                    users = JsonConvert.DeserializeObject<List<User>>(reader.ReadLine());
                }
            }
         

            return users;
        }

        public async static void SaveUsers(List<User> users)
        {
            using (StreamWriter wrtr = new StreamWriter(saveFile))
            {
                await wrtr.WriteLineAsync(JsonConvert.SerializeObject(users));
            }
        }
    }
}

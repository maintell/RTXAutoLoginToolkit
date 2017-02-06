using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RtxToolkit
{
    [Serializable]
    public class ConfigEntity
    {

        public string RTXPath { get; set; }

        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }

        public void Save()
        {
            FileStream fs = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "config.dat", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this);
            fs.Close();
        }

        public void Load()
        {
            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "config.dat"))
            {
                var fs = new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "config.dat", FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                ConfigEntity ce = bf.Deserialize(fs) as ConfigEntity;
                fs.Close();
                ServerAddress = ce.ServerAddress;
                ServerPort = ce.ServerPort;
                Account = ce.Account;
                Password = ce.Password;
                RTXPath = ce.RTXPath;
            }
            else
            {
                ServerAddress = "10.0.0.5";
                ServerPort = 8000;
                Account = "";
                Password = "";
                RTXPath = "";
            }
        }
    }
}

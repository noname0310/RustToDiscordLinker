using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DiscordLinker
{
    class ConfigFileManager
    {
        public JObject ConfigJson;
        string ConfigFileName = "DiscordLinkerConfig.json";

        public bool CheckandCreateConfig()
        {
            Console.WriteLine("세팅파일을 불러오는중...");
            Console.WriteLine("");
            FileInfo file = new FileInfo(ConfigFileName);
            if (file.Exists == false)
            {
                Console.WriteLine("세팅파일이 없습니다");
                Console.WriteLine("세팅파일이 새로 생성됍니다");
                Console.WriteLine("");

                StreamWriter swcreate = new StreamWriter(new FileStream(ConfigFileName, FileMode.Create));

                JObject json2 = new JObject();
                json2.Add("apikey", "");
                json2.Add("DataFileLocate", "");
                json2.Add("DisplayRoom", "");
                json2.Add("CommandPrefix", ".");

                swcreate.WriteLine(json2.ToString());
                swcreate.Close();
            }

            StreamReader sr = new StreamReader(new FileStream(ConfigFileName, FileMode.Open));
            string JsonData = sr.ReadToEnd();
            sr.Close();

            ConfigJson = JObject.Parse(JsonData);

            bool returnbool = false;

            if (ConfigJson["apikey"].ToString() == "")
            {
                Console.WriteLine("api키가 없습니다!");
                returnbool = true;
            }

            if (ConfigJson["DataFileLocate"].ToString() == "")
            {
                Console.WriteLine("DataFileLocate값이 없습니다!");
                returnbool = true;
            }
            else
            {
                try
                {
                    FileInfo datafile = new FileInfo(ConfigJson["DataFileLocate"].ToString());
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("DataFileLocate값이 잘못되었습니다!");
                    returnbool = true;
                }
            }


            if (ConfigJson["DisplayRoom"].ToString() == "")
            {
                Console.WriteLine("DisplayRoom값이 없습니다!");
                returnbool = true;
            }

            if (ConfigJson["CommandPrefix"].ToString() == "")
            {
                Console.WriteLine("CommandPrefix값이 없습니다!");
                returnbool = true;
            }

            if (returnbool == true)
            {
                Console.ReadLine();
                return false;
            }
            return true;
        }
    }
}

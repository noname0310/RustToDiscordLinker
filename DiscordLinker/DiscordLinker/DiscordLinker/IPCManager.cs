using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using Newtonsoft.Json.Linq;

namespace DiscordLinker
{
    class IPCManager
    {
        public string check_printDataFile(string filelink)
        {
            FileInfo file = new FileInfo(filelink);
            if (file.Exists == false)
            {
                Console.WriteLine("ERROR : data파일이 없습니다");
                return null;
            }

            StreamReader sr;
            try
            {
                sr = new StreamReader(filelink);
            }
            catch (IOException)
            {
                return null;
            }
            JObject DataFileJson = JObject.Parse(sr.ReadToEnd());
            sr.Close();

            List<ServerMsgData> MsgList = (DataFileJson["ServerMsgData"].ToObject<List<ServerMsgData>>());
            if (MsgList == null)
                return null;

            string msg = "";

            foreach (var item in MsgList)
            {
                ServerMsgData msgdata = item;
                string msgline = "";

                if (msgdata.popupmsg == true)
                {
                    msgline = msgdata.msg;
                    if (msgdata.formatted == true)
                        msgline = "\n" + "```" + msgline + "```";
                }
                else
                {
                    if (msgdata.formatted == true)
                        msgline = "\n" + "```" + msgdata.username + ": " + msgdata.msg + "```";
                    else
                        msgline = "**" + msgdata.username + "**" + ": " + msgdata.msg;
                }
                
                msg = msg + "\n" + msgline;
            }
            msg = msg.Substring(1);

            DataFileJson["ServerMsgData"] = null;
            SaveData(filelink, DataFileJson);

            return msg;
        }
        
        public void AddMsgOnJsonQueue(string filelink, string username, string msg)
        {
            FileInfo file = new FileInfo(filelink);
            if (file.Exists == false)
            {
                Console.WriteLine("ERROR : data파일이 없습니다");
                return;
            }

            JObject DataFileJson = ReadData(filelink);
            if (DataFileJson == null)
                return;
            if (DataFileJson["DiscordMsgData"] == null)
                DataFileJson["DiscordMsgData"] = null;

            DiscordMsgData msgData;
            string commandPrefix = Program.fileManager.ConfigJson["CommandPrefix"].ToString();

            if(msg.Length >= commandPrefix.Length)
            {
                if (msg.Substring(0, commandPrefix.Length) == commandPrefix)
                {
                    string cmdstr = msg.Substring(commandPrefix.Length);
                    msgData = new DiscordMsgData(username, cmdstr, true, SplitArguments(cmdstr));
                }
                else
                    msgData = new DiscordMsgData(username, StripHTML(msg), false, null);
            }
            else
                msgData = new DiscordMsgData(username, StripHTML(msg), false, null);

            List<DiscordMsgData> msgDatas = (DataFileJson["DiscordMsgData"].ToObject<List<DiscordMsgData>>());
            if (msgDatas == null)
                msgDatas = new List<DiscordMsgData>();

            msgDatas.Add(msgData);
            DataFileJson["DiscordMsgData"] = JToken.FromObject(msgDatas);
            
            SaveData(filelink, DataFileJson);
        }

        JObject ReadData(string File)
        {
            StreamReader sr;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    sr = new StreamReader(File);
                    JObject jObject = JObject.Parse(sr.ReadToEnd());
                    sr.Close();
                    return jObject;
                }
                catch { }
            }
            return null;
        }

        void SaveData(string File, JObject jObject)
        {
            StreamWriter sw;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    sw = new StreamWriter(File);//
                    sw.WriteLine(jObject.ToString());
                    sw.Close();
                    break;
                }
                catch { }
            }
            
        }
        
        struct DiscordMsgData
        {
            public string username;
            public string msg;
            public bool iscmd;
            public string[] cmdargs;
            public DiscordMsgData(string username, string msg, bool iscmd, string[] cmdargs)
            {
                this.username = username;
                this.msg = msg;
                this.iscmd = iscmd;
                this.cmdargs = cmdargs;
            }
        }//디스코드 메세지 스트럭트

        struct ServerMsgData
        {
            public string username;
            public string msg;
            public bool popupmsg;
            public bool formatted;
            public ServerMsgData(string username, string msg, bool popupmsg, bool formatted)
            {
                this.username = username;
                this.msg = msg;
                this.popupmsg = popupmsg;
                this.formatted = formatted;
            }
        }//서버 메세지 스트럭트

        string[] SplitArguments(string args)
        {
            for (int ii = 0; ii < args.Length; ii++)
            {
                if (args.Substring(ii, 1) != " ")
                {
                    args = args.Substring(ii);
                    break;
                }
            }
            char[] parmChars = args.ToCharArray();
            bool inSingleQuote = false;
            bool inDoubleQuote = false;
            bool escaped = false;
            bool lastSplitted = false;
            bool justSplitted = false;
            bool lastQuoted = false;
            bool justQuoted = false;

            int i, j;

            for (i = 0, j = 0; i < parmChars.Length; i++, j++)
            {
                parmChars[j] = parmChars[i];

                if (!escaped)
                {
                    if (parmChars[i] == '^')
                    {
                        escaped = true;
                        j--;
                    }
                    else if (parmChars[i] == '"' && !inSingleQuote)
                    {
                        inDoubleQuote = !inDoubleQuote;
                        parmChars[j] = '\n';
                        justSplitted = true;
                        justQuoted = true;
                    }
                    else if (parmChars[i] == '\'' && !inDoubleQuote)
                    {
                        inSingleQuote = !inSingleQuote;
                        parmChars[j] = '\n';
                        justSplitted = true;
                        justQuoted = true;
                    }
                    else if (!inSingleQuote && !inDoubleQuote && parmChars[i] == ' ')
                    {
                        parmChars[j] = '\n';
                        justSplitted = true;
                    }

                    if (justSplitted && lastSplitted && (!lastQuoted || !justQuoted))
                        j--;

                    lastSplitted = justSplitted;
                    justSplitted = false;

                    lastQuoted = justQuoted;
                    justQuoted = false;
                }
                else
                {
                    escaped = false;
                }
            }

            if (lastQuoted)
                j--;

            return (new string(parmChars, 0, j)).Split(new[] { '\n' });
        }

        string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
    }
}

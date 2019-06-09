using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using Newtonsoft.Json.Linq;

namespace Oxide.Plugins
{
    [Info("DiscordLinker", "noname", "0.0.1")]
    [Description("Link the server and discord to eachother")]
    class DiscordLinker : CovalencePlugin
    {
        [PluginReference] Plugin HangulInput, BetterChatMute;
        DynamicConfigFile dataFile;

        #region Hooks

        private void OnServerInitialized()
        {
            AddMsgOnJsonQueue(null, "@everyone```서버가 온라인 입니다\n서버측 디스코드 링커가 켜졌습니다```", true, false);
        }

        private void Loaded()
        {
            timer.Every(0.5f, timer_Tick);
        }//new timer event

        private object OnUserChat(IPlayer player, string message)
        {
            if (BetterChatMute != null)
            {
                bool? muted = BetterChatMute?.Call<bool>("API_IsMuted", player);
                if (muted.HasValue && muted.Value)
                {
                    return null;
                }
            }
            if (HangulInput != null) message = (string)HangulInput?.Call("GetConvertedString", player.Id, message);
            AddMsgOnJsonQueue(player.Name, message, false, false);
            return null;
        }

        #endregion

        #region IPCCommunication

        #region Get

        void timer_Tick()
        {
            JObject jsonobj;
            try
            {
                dataFile = Interface.Oxide.DataFileSystem.GetDatafile("DiscordLinker");

                if (dataFile["DiscordMsgData"] == null)
                    return;
                
                jsonobj = Interface.Oxide.DataFileSystem.ReadObject<JObject>("DiscordLinker");
            }
            catch
            {
                return;
            }

            List<DiscordMsgData> MsgList = (jsonobj["DiscordMsgData"].ToObject<List<DiscordMsgData>>());

            foreach (var item in MsgList)
            {
                DiscordMsgData msgdata = item;
                if (msgdata.iscmd == true)
                {
                    analyzeCmd(msgdata);
                    continue;
                }
                Broadcast("<color=#FF9436>[디스코드]</color> <color=#FFDC7E>" + msgdata.username + "</color>: " + msgdata.msg);
                Puts(msgdata.username + ": " + msgdata.msg);
            }

            jsonobj["DiscordMsgData"] = null;
            SaveData(jsonobj);
        }//Receive

        #endregion

        #region Send

        void AddMsgOnJsonQueue(string username, string msg, bool popupmsg, bool formatted)
        {
            timer.Once(0.1f, () =>
            {
                JObject DataFileJson = ReadData();
                if (DataFileJson == null)
                    return;

                if (DataFileJson["ServerMsgData"] == null)
                    DataFileJson["ServerMsgData"] = null;

                ServerMsgData msgData = new ServerMsgData(username, msg, popupmsg, formatted);
                List<ServerMsgData> msgDatas = (DataFileJson["ServerMsgData"].ToObject<List<ServerMsgData>>());
                if (msgDatas == null)
                    msgDatas = new List<ServerMsgData>();

                msgDatas.Add(msgData);
                DataFileJson["ServerMsgData"] = JToken.FromObject(msgDatas);
                SaveData(DataFileJson);
            });
        }//AddInQueue /api

        #endregion

        #endregion

        #region DataIO

        private JObject ReadData()
        {
            JObject jsonobj;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    jsonobj = Interface.Oxide.DataFileSystem.ReadObject<JObject>("DiscordLinker");
                    return jsonobj;
                }
                catch { }
            }
            return null;
        }//ReadData

        private void SaveData(JObject jObject)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Interface.Oxide.DataFileSystem.WriteObject("DiscordLinker", jObject);
                    break;
                }
                catch { }
            }
        }//WriteData

        #endregion

        #region DataStruct

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
        }//DiscordMsgStruct

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
        }//ServerMsgStruct

        #endregion

        #region helper

        private void Broadcast(string msg)
        {
            foreach (var player in players.Connected)
                player.Message(msg);
        }//Mag To all

        #endregion

        #region DiscordCommandHandle

        private void analyzeCmd(DiscordMsgData msgdata)
        {
            string[] lowercmdargs = new string[msgdata.cmdargs.Length];

            for (int i = 0; i < msgdata.cmdargs.Length; i++)
            {
                lowercmdargs[i] = msgdata.cmdargs[i].ToLower();
            }

            if (msgdata.cmdargs[0] == "")
            {
                AddMsgOnJsonQueue(null,
                    "\',\'는 디스코드 링커 명령어 키워드입니다" +
                    "\n이 뒤에 구문을 붙임으로서 서버측에서 명령어를 수행하며 응답합니다" +
                    "\n" +
                    "\n명령어 리스트를 보려면 ,help를 입력하세요"
                    , true, true);

                return;
            }

            object result = Interface.CallHook("OnDiscordCommand", msgdata.username, msgdata.msg, lowercmdargs);

            if (result == null)
            {
                AddMsgOnJsonQueue(null, "" +
                        "\"" + msgdata.cmdargs[0] + "\"는(은) 존재하지 않는 명령어 입니다" +
                        "", true, true);
            }
        }//CommandHandle

        #endregion
    }
}
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins.HangulInputExtensions;

namespace Oxide.Plugins
{
	[Info("HangulInput", "ToriaTo", "1.2.1")]
	[Description("Hangul Input Manager for Koreans")]

	class HangulInput : RustPlugin
	{
		private static PlayerDataStorage playerDataStorage;

		class PlayerDataStorage
		{
			public Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();

			public PlayerDataStorage()
			{
			}
		}

		class PlayerData
		{
			public bool isHangul;

			public PlayerData(bool isHangul)
			{
				this.isHangul = isHangul;
			}
		}

		protected override void LoadDefaultConfig()
		{
			Config.Clear();
			
			Config["명령어"] = "/";
			Config["명령어, 한글 입력을 기본으로 설정할까요?"] = true;

			Config["메세지, 한글 활성화"] = "한글 입력이 <color=#0F0>활성화</color> 됐습니다.\n영어는 <color=orange>##ENG##</color> 형태로 입력할 수 있습니다.";
			Config["메세지, 한글 비활성화"] = "한글 입력이 <color=#F00>비활성화</color> 됐습니다.";
			Config["메세지, 접속 안내"] = "한글 입력은 // 명령어로 끄고 켤 수 있습니다.";
			Config["메세지, 이전 명령어"] = "이 명령어는 더 이상 지원하지 않습니다.\n // 명령어를 사용해 한글 입력을 켜주세요.";

			Config.Save();
		}

		void Loaded()
		{
			playerDataStorage = Interface.Oxide.DataFileSystem.ReadObject<PlayerDataStorage>("HangulInput");

			cmd.AddChatCommand((string)Config["명령어"], this, "CommandToggle");

			//if (!string.IsNullOrEmpty((string)Config["메세지, 이전 명령어"]))
				//cmd.AddChatCommand("h", this, "CommandObsolete");
		}

		// 플레이어 데이터 불러오기
		PlayerData GetPlayerData(BasePlayer player)
		{
			var id = player.UserIDString;

			if (!playerDataStorage.players.ContainsKey(id))
				playerDataStorage.players.Add(id, new PlayerData((bool)Config["명령어, 한글 입력을 기본으로 설정할까요?"]));

			return playerDataStorage.players[id];
		}

		// 명령어, 한글 입력 토글
		void CommandToggle(BasePlayer player)
		{
			var playerId = player.UserIDString;
			var config = GetPlayerData(player);

			config.isHangul = !config.isHangul;
			SaveData();

			if (config.isHangul) SendReply(player, (string)Config["메세지, 한글 활성화"]);
			else SendReply(player, (string)Config["메세지, 한글 비활성화"]);
		}

		// 명령어, 이전 플러그인 안내
		void CommandObsolete(BasePlayer player)
		{
			SendReply(player, (string)Config["메세지, 이전 명령어"]);
		}

		// 플레이어 접속 알림
		void OnPlayerInit(BasePlayer player)
		{
			if (!string.IsNullOrEmpty((string)Config["메세지, 접속 안내"]))
				SendReply(player, (string)Config["메세지, 접속 안내"]);
		}

		// 플레이어 설정에 따라 문자열 변환
		string GetConvertedString(string playerId, string message)
		{
			var player = rust.FindPlayerByIdString(playerId);

			// 플레이어가 없다면 NULL 리턴
			if (player == null)
			{
				PrintWarning($"GetConvertedString: {playerId} 플레이어가 존재하지 않습니다.");
				return null;
			}

			var config = GetPlayerData(player);

			if (config.isHangul)
				message = EngHanConverter.Eng2Kor(message);

			return message;
		}
        string GetConvertedStringCs(string message)
		{
		    message = EngHanConverter.Eng2Kor(message);

			return message;
		}

		void SaveData() => Interface.Oxide.DataFileSystem.WriteObject("HangulInput", playerDataStorage);
		string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);
		void Puts(object i) => Console.WriteLine(i);
	}
}

namespace Oxide.Plugins.HangulInputExtensions
{
	internal static class EngHanConverter
	{
		private static readonly char[] IniC = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
		private static readonly string[] IniS = { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };

		private static readonly char[] VolC = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
		private static readonly string[] VolS = { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };

		private static readonly char[] UndC = { '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
		private static readonly string[] UndS = { "", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };

		private static readonly string UpperAllowed = "QWERTOP";

		private static readonly string[] Table =
		{
			"ㄱ", "r", "ㄲ", "R",  "ㄳ", "rt",
			"ㄴ", "s", "ㄵ", "sw", "ㄶ", "sg",
			"ㄷ", "e", "ㄸ", "E",
			"ㄹ", "f", "ㄺ", "fr", "ㄻ", "fa", "ㄼ", "fq", "ㄽ", "ft", "ㄾ", "fx", "ㄿ", "fv", "ㅀ", "fg",
			"ㅁ", "a",
			"ㅂ", "q", "ㅃ", "Q",  "ㅄ", "qt",
			"ㅅ", "t", "ㅆ", "T",
			"ㅇ", "d",
			"ㅈ", "w",
			"ㅉ", "W",
			"ㅊ", "c",
			"ㅋ", "z",
			"ㅌ", "x",
			"ㅍ", "v",
			"ㅎ", "g",
			"ㅏ", "k",
			"ㅐ", "o", "ㅒ", "O",
			"ㅑ", "i",
			"ㅓ", "j",
			"ㅔ", "p", "ㅖ", "P",
			"ㅕ", "u",
			"ㅗ", "h", "ㅘ", "hk", "ㅙ", "ho", "ㅚ", "hl",
			"ㅛ", "y",
			"ㅜ", "n", "ㅝ", "nj", "ㅞ", "np", "ㅟ", "nl",
			"ㅠ", "b",
			"ㅣ", "l",
			"ㅡ", "m", "ㅢ", "ml",
		};

		private static char GetKor(string src, int index, int type, out int len, bool onlyOne = false)
		{
			len = 0;
			if (index >= src.Length) return '\0';

			int i = -1;

			if (type != 0 && !onlyOne && index + 1 < src.Length)
			{
				i = Array.IndexOf<string>(Table, new string(new char[] { src[index], src[index + 1] }));
				len = 2;
			}

			if (i == -1)
			{
				i = Array.IndexOf<string>(Table, src[index].ToString());
				len = 1;
			}

			var c = i >= 0 ? Table[i - 1][0] : '\0';

			if (type == 0) return Array.IndexOf<char>(IniC, c) >= 0 ? c : '\0';
			if (type == 1) return Array.IndexOf<char>(VolC, c) >= 0 ? c : '\0';
			if (type == 2) return Array.IndexOf<char>(UndC, c) >= 0 ? c : '\0';

			len = 0;
			return '\0';
		}
		private static bool Split(char src, out int ini, out int vow, out int und)
		{
			// 원래 초중종 나눔
			int charCode = Convert.ToInt32(src) - 44032;
			int i;

			if ((charCode < 0) || (charCode > 11171))
			{
				ini = vow = und = -1;

				if ((i = Array.IndexOf<char>(IniC, src)) != -1)
					ini = i;
				else if ((i = Array.IndexOf<char>(VolC, src)) != -1)
					vow = i;
				else if (src != '\0' && (i = Array.IndexOf<char>(UndC, src)) != -1)
					und = i;
			}
			else
			{
				ini = charCode / 588;
				vow = (charCode % 588) / 28;
				und = (charCode % 588) % 28;
			}

			return ini != -1 || vow != -1 || und != -1;
		}
		private static char Combine(char ini, char vow, char und = '\0')
		{
			// 조합
			int i = 44032 + Array.IndexOf<char>(IniC, ini) * 588;
			if (vow != '\0') i += Array.IndexOf<char>(VolC, vow) * 28;
			if (und != '\0') i += Array.IndexOf<char>(UndC, und);

			return Convert.ToChar(i);
		}
		
		public static string Eng2Kor(string eng)
		{
			if (eng == null) throw new ArgumentNullException();
			if (eng.Length == 0) throw new ArgumentException();

			char ini, vow, und, current;
			int len, len2, idx = 0;

			bool isHangul = true;

			var result = new StringBuilder(eng.Length);
			var tester = new StringBuilder(eng);

			// 대소문자 검사
			while (idx < eng.Length)
			{
				current = eng[idx];

				if (current.Equals('#') && idx + 1 < eng.Length && eng[idx + 1].Equals('#'))
				{
					isHangul = !isHangul;
					idx += 2;
					continue;
				}
				
				if (isHangul)
				{
					if (!UpperAllowed.Contains(current.ToString()))
					{
						tester.Remove(idx, 1);
						tester.Insert(idx, current.ToString().ToLower());
					}
				}

				idx++;
			}

			eng = tester.ToString();
			idx = 0;

			isHangul = true;

			while (idx < eng.Length)
			{
				current = eng[idx];
				
				// Toggle
				if (current.Equals('#') && idx + 1 < eng.Length && eng[idx + 1].Equals('#'))
				{
					isHangul = !isHangul;
					idx += 2;
					continue;
				}

				// Toggle, English
				if (!isHangul)
				{
					result.Append(current);
					idx++;
					continue;
				}

				ini = vow = und = '\0';

				////////////////////////////////////////////////// 초성
				ini = GetKor(eng, idx, 0, out len);

				// 초성이 아니면
				if (ini == '\0')
				{
					// 자음이 아니면 모음이냐?
					vow = GetKor(eng, idx, 1, out len);

					// 모음도 아니네 :3
					if (vow == '\0')
					{
						result.Append(current);
						idx++;
						continue;
					}

					// 모음 맞네!!!
					result.Append(vow);
					idx += len;
					continue;
				}

				// 모음 다음에 모음이면... 조합 모음?
				if (GetKor(eng, idx + 1, 0, out len2) != '\0')
				{
					// 근데 자자모 순서대로면 조합 모음이 아니라 단순한 모음이니까
					// ㄱㄱㅏ -> ㄱ가
					if (GetKor(eng, idx + 2, 1, out len2) != '\0')
					{
						result.Append(ini);
						idx += len;
						continue;
					}

					// 조합 모음이 맞는지 확인
					und = GetKor(eng, idx, 2, out len2);

					if (len2 == 2)
					{
						// 조합 모음이 맞네
						result.Append(und);
						idx += len2;
						continue;
					}

					// 시무룩. 조합모음이 아니였다.
					// 집어넣고 다음 기회를 노리자
					else
					{
						result.Append(ini);
						idx += len;
						continue;
					}
				}

				// 초성 길이만큼 이동. 어처피 한글자임
				idx += 1;

				////////////////////////////////////////////////// 중성
				vow = GetKor(eng, idx, 1, out len);

				// 중성이 아니면 초성만 넣고 스킵
				if (vow == '\0')
				{
					result.Append(ini);
					continue;
				}

				// 중성 길이만큼 이동
				idx += len;

				////////////////////////////////////////////////// 종성
				und = GetKor(eng, idx, 2, out len);
				// 종성이 아니면 조합해서 넣고 다음으로.
				if (und == '\0')
				{
					result.Append(Combine(ini, vow));
					continue;
				}

				// 자음 뒤에 모음이 나오는 경우 대비
				// 예) 각시 ㄱㅏㄱㅅㅣ => 갃X
				if (len == 2)
				{
					// 자모자자자 순서면 이게 조합 모음이 맞음.
					if (GetKor(eng, idx + 2, 0, out len2) != '\0')
					{
						result.Append(Combine(ini, vow, und));
						idx += len;
						continue;
					}

					// 어이쿠 조합 모음이 아니라 그냥 모음이였네요.
					und = GetKor(eng, idx, 2, out len, true);

					result.Append(Combine(ini, vow, und));
					idx += len;
					continue;
				}
				// 가시 = ㄱㅏㅅㅣ ->갓X
				else
				{
					// 다음에 모음이 나오니까 이건 종성이 아님.
					if (GetKor(eng, idx + 1, 1, out len2) != '\0')
					{
						result.Append(Combine(ini, vow));
						continue;
					}

					// 이게 종성이 맞음.
					result.Append(Combine(ini, vow, und));
					idx += len;
					continue;
				}
			}

			return result.ToString();
		}
	}
}
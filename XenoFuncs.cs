using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moonreborn
{
    internal class XenoFuncs
    {
        public bool console = false;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ClientInfo
        {
            public string version;
            public string name;
            public int id;
            public int state;
        }

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Initialize(bool useConsole);

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Attach();

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetClients();

        [DllImport("bin\\Xeno.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void Execute(byte[] scriptSource, int[] PIDs, int numUsers);

        public List<ClientInfo> ActiveClients { get; private set; } = new List<ClientInfo>();

        public static List<ClientInfo> GetClientsList()
        {
            List<ClientInfo> list = new List<ClientInfo>();
            IntPtr clients = GetClients();
            bool flag = clients == IntPtr.Zero;
            List<ClientInfo> result;
            if (flag)
            {
                result = list;
            }
            else
            {
                string text = Marshal.PtrToStringAnsi(clients);
                bool flag2 = string.IsNullOrEmpty(text);
                if (flag2)
                {
                    result = list;
                }
                else
                {
                    try
                    {
                        MatchCollection matchCollection = Regex.Matches(text, "\\[\\s*(\\d+),\\s*\"(.*?)\",\\s*\"(.*?)\",\\s*(\\d+)\\s*\\]");
                        foreach (object obj in matchCollection)
                        {
                            Match match = (Match)obj;
                            list.Add(new ClientInfo
                            {
                                id = int.Parse(match.Groups[1].Value),
                                name = match.Groups[2].Value,
                                version = match.Groups[3].Value,
                                state = int.Parse(match.Groups[4].Value)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    result = list;
                }
            }
            return result;
        }

        public static void ExecuteScript(string scriptSource)
        {
            List<ClientInfo> clientsList = GetClientsList();
            int[] array = (from c in clientsList select c.id).ToArray<int>();
            Execute(Encoding.UTF8.GetBytes(scriptSource), array, array.Length);
        }

        public static void Inject()
        {
            Initialize(false);
            Attach();
            Thread.Sleep(3000);
            SendNotification("Moon Reborn", "Injected with Xeno");
        }

        public static void SendNotification(string title, string text)
        {
            string s = string.Concat(new string[]
            {
        "\tgame:GetService(\"StarterGui\"):SetCore(\"SendNotification\", {\r\n\t\tTitle = \"",
        title,
        "\",\r\n\t\tText = \"",
        text,
        "\"\r\n\t})"
            });
            List<ClientInfo> clientsList = GetClientsList();
            int[] array = (from c in clientsList select c.id).ToArray<int>();
            Execute(Encoding.UTF8.GetBytes(s), array, array.Length);
        }

        public static bool IsInjected()
        {
            bool result;
            try
            {
                List<ClientInfo> clientsList = GetClientsList();
                result = ((clientsList.Count > 0) ? true : false);
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}

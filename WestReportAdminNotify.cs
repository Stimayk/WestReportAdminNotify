using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using Newtonsoft.Json;
using WestReportSystemApiReborn;

namespace WestReportAdminNotify
{
    public class WestReportAdminNotify : BasePlugin
    {
        public override string ModuleName => "WestReportAdminNotify";
        public override string ModuleVersion => "v1.0";
        public override string ModuleAuthor => "E!N";
        public override string ModuleDescription => "Module that adds notification of report to admins immediately in the game in chat/hud";

        private IWestReportSystemApi? WRS_API;
        private AdminNotifyConfig? _config;

        private bool messageToHudEnabled = false;

        public string? flag;
        private bool chat;
        private bool hud;

        public override void OnAllPluginsLoaded(bool hotReload)
        {
            string configDirectory = GetConfigDirectory();
            EnsureConfigDirectory(configDirectory);
            string configPath = Path.Combine(configDirectory, "AdminNotifyConfig.json");
            _config = AdminNotifyConfig.Load(configPath);

            InitializeAdminNotify();

            WRS_API = IWestReportSystemApi.Capability.Get();

            if (WRS_API == null)
            {
                Console.WriteLine($"{ModuleName} | Error: WestReportSystem API is not available.");
                return;
            }
            else
            {
                WRS_API.OnReportSend += GetReport;
                Console.WriteLine($"{ModuleName} | Successfully subscribed to report send events.");
            }
        }

        private static string GetConfigDirectory()
        {
            return Path.Combine(Server.GameDirectory, "csgo/addons/counterstrikesharp/configs/plugins/WestReportSystem/Modules");
        }

        private void EnsureConfigDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"{ModuleName} | Created configuration directory at: {directoryPath}");
            }
        }

        private void InitializeAdminNotify()
        {
            if (_config == null)
            {
                Console.WriteLine($"{ModuleName} | Error: Configuration is not loaded.");
                return;
            }

            flag = _config.AdminNotifyAdmFlag;
            chat = _config.AdminNotifyChat;
            hud = _config.AdminNotifyHUD;

            Console.WriteLine($"{ModuleName} | Initialized: flag = {flag}, chat = {chat}, hud = {hud}");
        }

        private void GetReport(CCSPlayerController? sender, CCSPlayerController? violator, string? reason)
        {
            if (flag != null)
            {
                if (hud && _config != null)
                {
                    float duration = _config.AdminNotifyDurationHUD;
                    RegisterListener<Listeners.OnTick>(() =>
                    {
                        if (messageToHudEnabled)
                        {
                            Utilities.GetPlayers().Where(player => AdminManager.PlayerHasPermissions(player, flag)).ToList().ForEach(admin =>
                            {
                                OnTick(admin, sender, violator, reason);
                            });
                        }
                    });
                    ToggleMessageToHud(duration, sender, violator, reason);
                }
                if (chat)
                {
                    Utilities.GetPlayers().Where(player => AdminManager.PlayerHasPermissions(player, flag)).ToList().ForEach(admin =>
                    {
                        if (violator != null && sender != null && reason != null)
                        {
                            admin.PrintToChat($"{WRS_API?.GetTranslatedText("wran.ChatMessage", WRS_API.GetTranslatedText("wrs.Prefix"), violator.PlayerName, sender.PlayerName, reason)}");
                        }
                    });
                }
            }
        }

        private void ToggleMessageToHud(float duration, CCSPlayerController? sender, CCSPlayerController? violator, string? reason)
        {
            messageToHudEnabled = true;
            AddTimer(duration, () => { messageToHudEnabled = false; sender = null; violator = null; reason = null; });
        }

        private void OnTick(CCSPlayerController admin, CCSPlayerController? sender, CCSPlayerController? violator, string? reason)
        {
            if (violator != null && sender != null && reason != null)
            {
                admin.PrintToCenterHtml($"{WRS_API?.GetTranslatedText("wran.HudMessage", violator.PlayerName, sender.PlayerName, reason)}");
            }
            return;
        }

        public class AdminNotifyConfig
        {
            public string? AdminNotifyAdmFlag { get; set; } = "@css/ban";
            public bool AdminNotifyChat { get; set; } = true;
            public bool AdminNotifyHUD { get; set; } = true;
            public float AdminNotifyDurationHUD { get; set; } = 5.0f;

            public static AdminNotifyConfig Load(string configPath)
            {
                if (!File.Exists(configPath))
                {
                    AdminNotifyConfig defaultConfig = new();
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Newtonsoft.Json.Formatting.Indented));
                    return defaultConfig;
                }

                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AdminNotifyConfig>(json) ?? new AdminNotifyConfig();
            }
        }
    }
}

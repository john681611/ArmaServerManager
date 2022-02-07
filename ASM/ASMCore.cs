/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/


using System.Collections.Generic;

namespace ASM.Lib
{
    public sealed class ASMCore
    {
        public const string VERSION = "0.0.4";
        public const string REPO_URL = "https://github.com/john681611/ArmaServerLauncher";
        public ASMConfig Config { get; private set; }
        public List<string> logStream {get; set;} = new List<string>();
        public const string BUILD_VERSION = "~BUILD_VERSION~";

        public ASMCore()
        {
            Config = ASMConfig.Load();
            Config.SetServerSide();
        }

        public void RunServer(List<string> modIds, string activeServerId) => BatRunner.RunServer(modIds, activeServerId, Config, logStream);
        public void RunSteamModsUpdate(List<string> modIds, string activeServerId) => BatRunner.RunSteamModsUpdate(modIds, activeServerId, Config, logStream);
        public void RunSteamModInstall(string modId, string folderName, string activeServerId) => BatRunner.RunSteamModInstall(modId, folderName, activeServerId, Config, logStream);
        public void RunSteamServerUpdate(string activeServerId) =>BatRunner.RunSteamServerUpdate(activeServerId, Config, logStream);  
    }
}

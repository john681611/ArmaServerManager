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


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ASM.Lib
{
    public sealed class ASMCore
    {
        public const string VERSION = "0.0.1";
        public const string REPO_URL = "https://github.com/john681611/ArmaServerLauncher";
        public ASMConfig Config { get; private set; }

        public const string BUILD_VERSION = "~BUILD_VERSION~";
        public ASMCore()
        {
            Config = ASMConfig.Load();
            Config.FindMods();
            Config.FindMissions();
        }

        public void RunServer(List<string> modIds)
        {   
            BatRunner.RunServer(modIds, Config);
        }

        public void RunSteamModsUpdate(List<string> modIds)
        {
            BatRunner.RunSteamModsUpdate(modIds, Config);
        }

        public void RunSteamServerUpdate()
        {
            BatRunner.RunSteamServerUpdate(Config);
        }
    }
}

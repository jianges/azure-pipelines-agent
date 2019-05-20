#if OS_WINDOWS
using Microsoft.Win32;
using Microsoft.VisualStudio.Services.Agent.Util;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
    [ServiceLocator(Default = typeof(WindowsRegistryManager))]
    public interface IWindowsRegistryManager : IAgentService
    {
        string GetValue(RegistryHive hive, string subKeyName, string name);
        void SetValue(RegistryHive hive, string subKeyName, string name, string value);
        void DeleteValue(RegistryHive hive, string subKeyName, string name);
        bool SubKeyExists(RegistryHive hive, string subKeyName);
    }

    public class WindowsRegistryManager : AgentService, IWindowsRegistryManager
    {
        public void DeleteValue(RegistryHive hive, string subKeyName, string name)
        {            
            using(RegistryKey key = OpenRegistryKey(hive, subKeyName, true))
            {
                if (key != null)
                {
                    key.DeleteValue(name, false);
                }
            }
        }

        public string GetValue(RegistryHive hive, string subKeyName, string name)
        {            
            using(RegistryKey key = OpenRegistryKey(hive, subKeyName, false))
            {
                if(key == null)
                {
                    return null;
                }

                var value = key.GetValue(name, null);
                return value != null ? value.ToString() : null;
            }
        }

        public void SetValue(RegistryHive hive, string subKeyName, string name, string value)
        {
            using(RegistryKey key = OpenRegistryKey(hive, subKeyName, true))
            {
                if(key == null)
                {
                    Trace.Warning($"Couldn't get the subkey '{subKeyName}. Proceeding to create subkey.");
                    key = CreateRegistryKey(hive, subKeyName, writable: true);

                    return;
                }

                key.SetValue(name, value);
            }
        }

        public bool SubKeyExists(RegistryHive hive, string subKeyName)
        {
            using(RegistryKey key = OpenRegistryKey(hive, subKeyName, false))
            {
                return key != null;
            }
        }

        private RegistryKey OpenRegistryKey(RegistryHive hive, string subKeyName, bool writable = true)
        {
            RegistryKey key = null;
            switch (hive)
            {
                case RegistryHive.CurrentUser :
                    key = Registry.CurrentUser.OpenSubKey(subKeyName, writable);                    
                    break;
                case RegistryHive.Users :
                    key = Registry.Users.OpenSubKey(subKeyName, writable);
                    break;
                case RegistryHive.LocalMachine:
                    key = Registry.LocalMachine.OpenSubKey(subKeyName, writable);                    
                    break;
            }
            return key;
        }

        private RegistryKey CreateRegistryKey(RegistryHive hive, string subKeyName, bool writable = true)
        {
            RegistryKey key = null;
            switch (hive)
            {
                case RegistryHive.CurrentUser :
                    key = Registry.CurrentUser.CreateSubKey(subKeyName, writable);                    
                    break;
                case RegistryHive.Users :
                    key = Registry.Users.CreateSubKey(subKeyName, writable);
                    break;
                case RegistryHive.LocalMachine:
                    key = Registry.LocalMachine.CreateSubKey(subKeyName, writable);                    
                    break;
            }
            return key;
        }
    }
}
#endif
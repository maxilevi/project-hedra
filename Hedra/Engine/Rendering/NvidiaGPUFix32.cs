// <copyright file="SOP.cs">
//
// Copyright (c) 2012, Daniel Cornel. Published on drivenbynostalgia.com.
// All rights reserved.
//
// </copyright>
// <license>
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright
//      notice, this list of conditions and the following disclaimer in the
//      documentation and/or other materials provided with the distribution.
//    * Neither the name of the copyright holder nor the
//      names of its contributors may be used to endorse or promote products
//      derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// </license>

namespace Hedra.Engine.Rendering
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This static class is used to set or remove an Nvidia profile with Optimus
    /// setting such that the discrete GPU is used for the specified application.
    /// </summary>
    public static class NvidiaGPUFix32
    {
        // Return values
        public const int RESULT_NO_CHANGE = 0;
        public const int RESULT_CHANGE = 1;
        public const int RESULT_ERROR = -1;

        // Descriptor for an application bound to an application profile
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private unsafe struct Application
        {
            public uint version;
            public uint isPredefined;
            public fixed ushort appName[2048];
            public fixed ushort userFriendlyName[2048];
            public fixed ushort launcher[2048];
            public fixed ushort fileInFolder[2048];
        };

        // Application profile descriptor
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private unsafe struct Profile
        {
            public uint version;
            public fixed ushort profileName[2048];
            public uint* gpuSupport;
            public uint isPredefined;
            public uint numOfApps;
            public uint numOfSettings;
        };

        // Setting descriptor
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        private struct Setting
        {
            [FieldOffset(0)]
            public uint version;
            [FieldOffset(4), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
            public string settingName;
            [FieldOffset(4100)]
            public uint settingID;
            [FieldOffset(4104)]
            public uint settingType;
            [FieldOffset(4108)]
            public uint settingLocation;
            [FieldOffset(4112)]
            public uint isCurrentPredefined;
            [FieldOffset(4116)]
            public uint isPredefinedValid;

            // Convert the C unions with explicit field offsets
            [FieldOffset(4120)]
            public uint u32PredefinedValue;

            [FieldOffset(8220)]
            public uint u32CurrentValue;
        };

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        // Definitions for required NvAPI functions
        [DllImport("nvapi.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr QueryInterface(uint offset);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CreateSessionDelegate(out IntPtr session);
        private static CreateSessionDelegate CreateSession;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CreateApplicationDelegate(IntPtr session, IntPtr profile, ref Application application);
        private static CreateApplicationDelegate CreateApplication;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CreateProfileDelegate(IntPtr session, ref Profile profileInfo, out IntPtr profile);
        private static CreateProfileDelegate CreateProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DeleteProfileDelegate(IntPtr session, IntPtr profile);
        private static DeleteProfileDelegate DeleteProfile;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DestroySessionDelegate(IntPtr session);
        private static DestroySessionDelegate DestroySession;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate int EnumApplicationsDelegate(IntPtr session, IntPtr profile, uint startIndex, ref uint appCount, Application* allApplications);
        private static EnumApplicationsDelegate EnumApplications;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FindProfileByNameDelegate(IntPtr session, [MarshalAs(UnmanagedType.BStr)] string profileName, out IntPtr profile);
        private static FindProfileByNameDelegate FindProfileByName;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetProfileInfoDelegate(IntPtr session, IntPtr profile, ref Profile profileInfo);
        private static GetProfileInfoDelegate GetProfileInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitializeDelegate();
        private static InitializeDelegate Initialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int LoadSettingsDelegate(IntPtr session);
        private static LoadSettingsDelegate LoadSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SaveSettingsDelegate(IntPtr session);
        private static SaveSettingsDelegate SaveSettings;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetSettingDelegate(IntPtr session, IntPtr profile, ref Setting setting);
        private static SetSettingDelegate SetSetting;

        private static bool CheckForError(int status)
        {
            if (status != 0)
            {
                Log.WriteLine("NvAPI error in SetOptimusProfile: " + status);

                return true;
            }
            else
            {
                return false;
            }
        }

        private static unsafe bool UnicodeStringCompare(ushort* unicodeString, ushort[] referenceString)
        {
            for (int i = 0; i < 2048; i++)
            {
                if (unicodeString[i] != referenceString[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static ushort[] GetUnicodeString(string sourceString)
        {
            ushort[] destinationString = new ushort[2048];

            for (int i = 0; i < 2048; i++)
            {
                if (i < sourceString.Length)
                {
                    destinationString[i] = Convert.ToUInt16(sourceString[i]);
                }
                else
                {
                    destinationString[i] = 0;
                }
            }

            return destinationString;
        }

        private static bool GetProcs()
        {
            // Check if this is a 32 bit application
            if (IntPtr.Size != 4)
            {
                Log.WriteLine("Only 32 bit applications are supported.");
                return false;
            }

            // Check if the nvapi.dll is available
            if (LoadLibrary("nvapi.dll") == IntPtr.Zero)
            {
                Log.WriteLine("The nvapi.dll could not be found.");

                return false;
            }

            try
            {
                CreateApplication = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x4347A9DE), typeof(CreateApplicationDelegate)) as CreateApplicationDelegate;
                CreateProfile = Marshal.GetDelegateForFunctionPointer(QueryInterface(0xCC176068), typeof(CreateProfileDelegate)) as CreateProfileDelegate;
                CreateSession = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x0694D52E), typeof(CreateSessionDelegate)) as CreateSessionDelegate;
                DeleteProfile = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x17093206), typeof(DeleteProfileDelegate)) as DeleteProfileDelegate;
                DestroySession = Marshal.GetDelegateForFunctionPointer(QueryInterface(0xDAD9CFF8), typeof(DestroySessionDelegate)) as DestroySessionDelegate;
                EnumApplications = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x7FA2173A), typeof(EnumApplicationsDelegate)) as EnumApplicationsDelegate;
                FindProfileByName = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x7E4A9A0B), typeof(FindProfileByNameDelegate)) as FindProfileByNameDelegate;
                GetProfileInfo = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x61CD6FD6), typeof(GetProfileInfoDelegate)) as GetProfileInfoDelegate;
                Initialize = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x0150E828), typeof(InitializeDelegate)) as InitializeDelegate;
                LoadSettings = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x375DBD6B), typeof(LoadSettingsDelegate)) as LoadSettingsDelegate;
                SaveSettings = Marshal.GetDelegateForFunctionPointer(QueryInterface(0xFCBC7E14), typeof(SaveSettingsDelegate)) as SaveSettingsDelegate;
                SetSetting = Marshal.GetDelegateForFunctionPointer(QueryInterface(0x577DD202), typeof(SetSettingDelegate)) as SetSettingDelegate;
            }
            catch (Exception)
            {
                Log.WriteLine("The procs of nvapi.dll could not be retrieved.");
                return false;
            }

            return true;
        }

        private static unsafe bool ContainsApplication(IntPtr session, IntPtr profile, Profile profileDescriptor, ushort[] unicodeApplicationName, out Application application)
        {
            application = new Application();

            if (profileDescriptor.numOfApps == 0)
            {
                return false;
            }

            // Iterate over all applications bound to the profile
            Application[] allApplications = new Application[profileDescriptor.numOfApps];
            uint numAppsRead = profileDescriptor.numOfApps;

            fixed (Application* allApplicationsPointer = allApplications)
            {
                allApplicationsPointer[0].version = 147464; // Calculated from the size of the descriptor

                if (CheckForError(EnumApplications(session, profile, 0, ref numAppsRead, allApplicationsPointer)))
                {
                    return false;
                }

                for (uint i = 0; i < numAppsRead; i++)
                {
                    if (UnicodeStringCompare(allApplicationsPointer[i].appName, unicodeApplicationName))
                    {
                        application = allApplicationsPointer[i];

                        return true;
                    }
                }
            }

            return false;
        }

        // Call this from your application to check if an application profile with
        // the name provided exists.
        public static bool SOP_CheckProfile(string profileName)
        {
            bool result = false;
            IntPtr session;
            IntPtr profile;

            // Initialize NvAPI
            if ((!GetProcs()) || (CheckForError(Initialize())))
            {
                return false;
            }

            // Create the session handle to access driver settings
            if (CheckForError(CreateSession(out session)))
            {
                return false;
            }

            // Load all the system settings into the session
            if (CheckForError(LoadSettings(session)))
            {
                return false;
            }

            // Convert the profile name to a unicode string array
            ushort[] unicodeProfileName = GetUnicodeString(profileName);

            // Check if the application profile with the specified name exists
            result = (FindProfileByName(session, profileName, out profile) == 0);

            DestroySession(session);

            return result;
        }

        // Call this from your application to delete the application profile with
        // the name provided. Note that only one application profile per name exists
        // for all applications bound to it.
        // After the profile has been erased, the application will use the default GPU
        // the next time it is started, usually the integrated GPU.
        public static int SOP_RemoveProfile(string profileName)
        {
            int result = NvidiaGPUFix32.RESULT_NO_CHANGE;
            int status = 0;
            IntPtr session;
            IntPtr profile;

            // Initialize NvAPI
            if ((!GetProcs()) || (CheckForError(Initialize())))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Create the session handle to access driver settings
            if (CheckForError(CreateSession(out session)))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Load all the system settings into the session
            if (CheckForError(LoadSettings(session)))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Convert the profile name to a unicode string array
            ushort[] unicodeProfileName = GetUnicodeString(profileName);

            // Check if the application profile with the specified name already exists
            status = FindProfileByName(session, profileName, out profile);

            if (status == 0)
            {
                // The application profile with the specified name exists and can be deleted
                if ((CheckForError(DeleteProfile(session, profile))) || CheckForError(SaveSettings(session)))
                {
                    return NvidiaGPUFix32.RESULT_ERROR;
                }
                else
                {
                    result = NvidiaGPUFix32.RESULT_CHANGE;
                }
            }
            else if (status == -163 /* Profile not found */)
            {
                // The application profile does not exist and does not have to be deleted
                result = NvidiaGPUFix32.RESULT_NO_CHANGE;
            }
            else
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            status = DestroySession(session);

            return result;
        }

        // Call this from your application to create a generic application profile with
        // the name provided and add the application name to it. The application profile is
        // set to start all bound applications with the discrete (NVIDIA) GPU. If the profile
        // already exists because it is shared among several applications, the provided
        // application name is bound to the existing profile.
        // The changes take effect the next time the application is started.
        public static int SOP_SetProfile(string profileName, string applicationName)
        {
            int result = NvidiaGPUFix32.RESULT_NO_CHANGE;
            int status = 0;
            IntPtr session;
            IntPtr profile;

            // Initialize NvAPI
            if ((!GetProcs()) || (CheckForError(Initialize())))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Create the session handle to access driver settings
            if (CheckForError(CreateSession(out session)))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Load all the system settings into the session
            if (CheckForError(LoadSettings(session)))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Convert the profile name to a unicode string array
            ushort[] unicodeProfileName = GetUnicodeString(profileName);

            // Convert the application name to a unicode string array
            ushort[] unicodeApplicationName = GetUnicodeString(applicationName);

            // Check if the application profile with the specified name already exists
            status = FindProfileByName(session, profileName, out profile);

            if (status == -163 /* Profile not found */)
            {
                // The application profile does not yet exist and has to be created
                Profile newProfileDescriptor = new Profile();
                newProfileDescriptor.version = 69652; // Calculated from the size of the descriptor
                newProfileDescriptor.isPredefined = 0;

                unsafe
                {
                    for (int i = 0; i < 2048; i++)
                    {
                        newProfileDescriptor.profileName[i] = unicodeProfileName[i];
                    }

                    fixed (uint* gpuSupport = new uint[32])
                    {
                        newProfileDescriptor.gpuSupport = gpuSupport;
                        newProfileDescriptor.gpuSupport[0] = 1;
                    }
                }

                // Create the application profile
                if (CheckForError(CreateProfile(session, ref newProfileDescriptor, out profile)))
                {
                    return NvidiaGPUFix32.RESULT_ERROR;
                }

                // Create the application settings. This is where the discrete GPU for Optimus is set.
                Setting optimusSetting = new Setting();
                optimusSetting.version = 77856; // Calculated from the size of the descriptor
                optimusSetting.settingID = 0x10F9DC81; // Shim rendering mode ID
                optimusSetting.u32CurrentValue = 0x00000001 | 0x00000010; // Enable | Auto select

                if (CheckForError(SetSetting(session, profile, ref optimusSetting)))
                {
                    return NvidiaGPUFix32.RESULT_ERROR;
                }
            }
            else if (CheckForError(status))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Retrieve the profile information of the application profile
            Profile profileDescriptorManaged = new Profile();
            profileDescriptorManaged.version = 69652; // Calculated from the size of the descriptor

            if (CheckForError(GetProfileInfo(session, profile, ref profileDescriptorManaged)))
            {
                return NvidiaGPUFix32.RESULT_ERROR;
            }

            // Application descriptor
            Application applicationDescriptor = new Application();

            if (!ContainsApplication(session, profile, profileDescriptorManaged, GetUnicodeString(applicationName.ToLower()), out applicationDescriptor))
            {
                applicationDescriptor.version = 147464; // Calculated from the size of the descriptor
                applicationDescriptor.isPredefined = 0;

                unsafe
                {
                    for (int i = 0; i < 2048; i++)
                    {
                        applicationDescriptor.appName[i] = unicodeApplicationName[i];
                    }

                    // Add the current application to the new profile
                    if ((CheckForError(CreateApplication(session, profile, ref applicationDescriptor))) || (CheckForError(SaveSettings(session))))
                    {
                        return NvidiaGPUFix32.RESULT_ERROR;
                    }
                    else
                    {
                        result = NvidiaGPUFix32.RESULT_CHANGE;
                    }
                }
            }

            status = DestroySession(session);

            return result;
        }
    }
}
using System;
using System.IO;

using Microsoft.Win32;

namespace FuckAuido
{
    /// <summary>
    /// 시스템 설정 및 초기화를 담당하는 헬퍼 클래스
    /// </summary>
    public static class SetupHelper
    {
        private const string AppName = "FuckAuido";
        private const string RegistryRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// 애플리케이션 리소스 초기화
        /// </summary>
        public static void InitializeResources()
        {
            try
            {
                // Resources 폴더 생성
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (!Directory.Exists(resourcesPath))
                {
                    Directory.CreateDirectory(resourcesPath);
                }

                // 볼륨 아이콘 생성
                string iconPath = Path.Combine(resourcesPath, "volume-icon.ico");
                if (!File.Exists(iconPath))
                {
                    IconHelper.SaveVolumeIconToFile(iconPath, 70);
                }
            }
            catch
            {
                // 리소스 초기화 실패 시 무시 (런타임에 동적 생성됨)
            }
        }

        /// <summary>
        /// 시작 프로그램 등록 여부 확인
        /// </summary>
        public static bool IsStartupRegistered()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(AppName);
                        return value != null;
                    }
                }
            }
            catch
            {
                // 레지스트리 접근 실패
            }

            return false;
        }

        /// <summary>
        /// 시작 프로그램 등록/해제
        /// </summary>
        /// <param name="register">true: 등록, false: 해제</param>
        public static void RegisterStartup(bool register)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    if (key != null)
                    {
                        if (register)
                        {
                            // 현재 실행 파일 경로
                            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                            if (!string.IsNullOrEmpty(exePath))
                            {
                                key.SetValue(AppName, $"\"{exePath}\"");
                            }
                        }
                        else
                        {
                            // 등록 해제
                            if (key.GetValue(AppName) != null)
                            {
                                key.DeleteValue(AppName);
                            }
                        }
                    }
                }
            }
            catch
            {
                // 레지스트리 접근 실패
            }
        }
    }
}

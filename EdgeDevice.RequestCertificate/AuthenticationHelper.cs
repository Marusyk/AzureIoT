﻿using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EdgeDevice.RequestCertificate
{
    internal class AuthenticationHelper
    {
        private readonly string _clientId;
        private readonly string _tenantId;
        private readonly string[] _scopes;

        public AuthenticationHelper(string clientId, string tenantId, string functionBaseUrl)
        {
            _clientId = clientId;
            _tenantId = tenantId;
            _scopes = new[] { $"{functionBaseUrl}/user_impersonation" };
        }

        public async Task<AuthenticationResult> AcquireTokenAsync()
        {
            var app = PublicClientApplicationBuilder.Create(_clientId).WithTenantId(_tenantId).WithDefaultRedirectUri().Build();

            AuthenticationResult authenticationResult = await app.AcquireTokenWithDeviceCode(_scopes, deviceCodeResult =>
            {
                Console.WriteLine(deviceCodeResult.Message);
                return deviceCodeResult.VerificationUrl == null ? Task.CompletedTask : OpenBrowserAsync(deviceCodeResult.VerificationUrl);
            }).ExecuteAsync();

            return authenticationResult;
        }

        private static Task OpenBrowserAsync(string url)
        {
            var processStartInfo = new ProcessStartInfo { FileName = url, UseShellExecute = true };
            Process.Start(processStartInfo);
            return Task.CompletedTask;
        }
    }
}

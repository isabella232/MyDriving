﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading.Tasks;
using Foundation;
using MyDriving.Utils;
using Microsoft.WindowsAzure.MobileServices;
using MyDriving.Utils.Interfaces;

namespace MyDriving.iOS.Helpers
{
    public class Authentication : IAuthentication
    {
        public async Task<MobileServiceUser> LoginAsync(IMobileServiceClient client,
            MobileServiceAuthenticationProvider provider)
        {
            MobileServiceUser user = null;

            new NSObject().InvokeOnMainThread(async () =>
            {
                try
                {
                    var window = UIKit.UIApplication.SharedApplication.KeyWindow;
                    var root = window.RootViewController;
                    if (root != null)
                    {
                        var current = root;
                        while (current.PresentedViewController != null)
                        {
                            current = current.PresentedViewController;
                        }

                        Settings.Current.LoginAttempts++;

                        user = await client.LoginAsync(current, provider);

                        Settings.Current.AuthToken = user?.MobileServiceAuthenticationToken ?? string.Empty;
                        Settings.Current.AzureMobileUserId = user?.UserId ?? string.Empty;
                    }
                }
                catch (Exception e)
                {
                    if (!e.Message.Contains("cancelled"))
                    {
                        e.Data["method"] = "LoginAsync";
                        Logger.Instance.Report(e);
                    }
                }
            });

            return user;
        }

        public void ClearCookies()
        {
            var store = NSHttpCookieStorage.SharedStorage;
            var cookies = store.Cookies;

            foreach (var c in cookies)
            {
                store.DeleteCookie(c);
            }
        }
    }
}
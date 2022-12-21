﻿using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Natsurainko.FluentCore.Module.Authenticator;

public class YggdrasilAuthenticator : IAuthenticator
{
    public string YggdrasilServerUrl { get; private set; }

    public string Email { get; private set; }

    public string Password { get; private set; }

    public string AccessToken { get; private set; }

    public string ClientToken { get; private set; } = Guid.NewGuid().ToString("N");

    public AuthenticatorMethod Method { get; private set; }

    public YggdrasilAuthenticator(string yggdrasilServerUrl = "https://authserver.mojang.com", AuthenticatorMethod method = AuthenticatorMethod.Login)
    {
        YggdrasilServerUrl = yggdrasilServerUrl;
        Method = method;
    }

    public YggdrasilAuthenticator(AuthenticatorMethod method, string accessToken = default, string clientToken = default, string email = default, string password = default, string yggdrasilServerUrl = "https://authserver.mojang.com")
    {
        Email = email;
        Password = password;
        AccessToken = accessToken;
        ClientToken = clientToken;

        YggdrasilServerUrl = yggdrasilServerUrl;
        Method = method;
    }

    public Account Authenticate()
        => AuthenticateAsync().GetAwaiter().GetResult();

    public async Task<Account> AuthenticateAsync()
    {
        string url = YggdrasilServerUrl;
        string content = string.Empty;

        switch (Method)
        {
            case AuthenticatorMethod.Login:
                url += "/authenticate";
                content = new LoginRequestModel
                {
                    ClientToken = ClientToken,
                    UserName = Email,
                    Password = Password
                }.ToJson();
                break;
            case AuthenticatorMethod.Refresh:
                url += "/refresh";
                content = new
                {
                    clientToken = ClientToken,
                    accessToken = AccessToken,
                    requestUser = true
                }.ToJson();
                break;
            default:
                break;
        }

        using var res = await HttpWrapper.HttpPostAsync(url, content);
        string result = await res.Content.ReadAsStringAsync();

        res.EnsureSuccessStatusCode();

        var model = JsonConvert.DeserializeObject<YggdrasilResponseModel>(result);
        return new YggdrasilAccount()
        {
            AccessToken = model.AccessToken,
            ClientToken = model.ClientToken,
            Name = model.SelectedProfile.Name,
            Uuid = Guid.Parse(model.SelectedProfile.Id),
            YggdrasilServerUrl = YggdrasilServerUrl
        };
    }

    public async Task<bool> ValidateAsync(string accessToken)
    {
        string content = JsonConvert.SerializeObject(
            new YggdrasilRequestModel
            {
                ClientToken = ClientToken,
                AccessToken = accessToken
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/validate", content);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> SignoutAsync()
    {
        string content = JsonConvert.SerializeObject(
            new
            {
                username = Email,
                password = Password
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/signout", content);

        return res.IsSuccessStatusCode;
    }

    public async Task<bool> InvalidateAsync(string accessToken)
    {
        string content = JsonConvert.SerializeObject(
            new YggdrasilRequestModel
            {
                ClientToken = ClientToken,
                AccessToken = accessToken
            }
        );

        using var res = await HttpWrapper.HttpPostAsync($"{YggdrasilServerUrl}/invalidate", content);

        return res.IsSuccessStatusCode;
    }
}

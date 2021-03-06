﻿using RestSharp;
using RestSharp.Deserializers;
using System.Collections.Generic;
using System.Reflection;
using MediaCleaner.DataModels.Emby;
using System.Web.Http;

namespace MediaCleaner.APIClients
{
    class EmbyClient
    {
        RestClient client;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        JsonDeserializer deserialCount = new JsonDeserializer();

        public EmbyClient()
        {
            client = new RestClient(Config.EmbyAddress);
        }

        private void checkBaseUrl()
        {
            if(client.BaseUrl.ToString() != Config.EmbyAddress)
                client = new RestClient(Config.EmbyAddress);
        }

        public bool checkConnection()
        {
            checkBaseUrl();
            var request = new RestRequest("System/Info", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MediaBrowser-Token", Config.embyAccessToken);
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;
            else
            {
                throw response.ErrorException;
            }
        }

        public List<UserItem> getUserItems()
        {
            checkBaseUrl();
            var request = new RestRequest(string.Format("Users/{0}/Items?recursive=true&IncludeItemTypes=Episode&Fields=MediaSources,DateCreated", Config.embyUserid), Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("X-MediaBrowser-Token", Config.embyAccessToken);
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.Trace(response.Content);
                return deserialCount.Deserialize<UserItems>(response).Items;
            }
            else
            {
                throw response.ErrorException;
            }
        }

        public string getAccessToken(string username_, string password_ = "")
        {
            checkBaseUrl();
            var request = new RestRequest("Users/AuthenticateByName", Method.POST);
            request.AddHeader("Authorization", string.Format("MediaBrowser Client=\"MediaCleaner\", Device=\"Media Cleaner\", DeviceId=\"1\", Version=\"{0}\"", Assembly.GetExecutingAssembly().GetName().Version.ToString()));
            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { Username = username_, password = APIHelper.SHA1Hash(password_), passwordMd5 = APIHelper.MD5Hash(password_) });
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.Trace(response.Content);
                return deserialCount.Deserialize<AuthenticateByName>(response).AccessToken;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                logger.Trace(response.Content);
                throw new HttpResponseException(System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                throw response.ErrorException;
            }
        }

        public List<PublicUser> getPublicUsers()
        {
            checkBaseUrl();
            var request = new RestRequest("Users/Public", Method.GET);
            request.RequestFormat = DataFormat.Json;
            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                logger.Trace(response.Content);
                return deserialCount.Deserialize<List<PublicUser>>(response);
            }
            else
            {
                throw response.ErrorException;
            }
        }
    }
}

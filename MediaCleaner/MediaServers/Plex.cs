﻿using MediaCleaner.APIClients;
using MediaCleaner.DataModels.Plex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MediaCleaner.MediaServers
{
    class Plex : IMediaServer
    {
        PlexClient plexAPI;
        List<Episode> UserItemList;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public int _timestamp { get; set; }
        private int _lastTimestamp;

        public Plex ()
        {
            plexAPI = new PlexClient();
        }
        public bool checkConnection()
        {
            return plexAPI.checkConnection();
        }

        public bool checkSettings()
        {
            logger.Debug("[Mediaserver Plex] username: \"{0}\"; plexUuid: \"{1}\"; plexAccessToken: \"{2}\"; PlexAddress: \"{3}\";", Config.plexUsername, Config.plexUuid, Config.plexAccessToken, Config.PlexAddress);

            if (Config.plexUsername == "" || Config.plexUuid == "" || Config.plexAccessToken == "" || Config.PlexAddress == "")
                return false;

            return true;
        }

        public DataModels.Episode getItem(string episodePath)
        {
            if (UserItemList is null || _lastTimestamp != _timestamp)
                UserItemList = plexAPI.getUserItems();

            var PlexItem = UserItemList.FirstOrDefault(item1 => item1.Media.Any(sourcelist => sourcelist.Part.Any(source => source.file == episodePath)));

            if (PlexItem == null)
                return null;

            var UserItem = new DataModels.Episode();

            var played = false;
            if (PlexItem.viewCount > 0)
                played = true;

            UserItem.SeriesName = PlexItem.grandparentTitle;
            try { UserItem.SeasonNumber = Int32.Parse(Regex.Replace(PlexItem.parentTitle, "[^0-9]+", string.Empty)); } catch { UserItem.SeasonNumber = 0; }
            UserItem.EpisodeNumber = PlexItem.index;
            UserItem.EpisodeTitle = PlexItem.title;
            UserItem.FilePath = episodePath;
            UserItem.IsFavorite = false;
            UserItem.Played = played;
            UserItem.dateAdded = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(PlexItem.addedAt);

            _lastTimestamp = _timestamp;

            return UserItem;
        }

    }
}

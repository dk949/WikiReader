﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ID;
using static WikiBrowser.Logging;

namespace WikiBrowser.Requests {
    internal static class Helpers {
        public const string BaseUri = "https://terraria.fandom.com/api.php";

        public enum RequestType {
            Search = 0,
            GetItem = 1
        }

        private static readonly Dictionary<string, string>[] ParamDicts = {
            new Dictionary<string, string> {
                {"action", "query"},
                {"format", "json"},
                {"maxlag", ""},
                {"prop", "extracts"},
                {"continue", ""},
                {"rawcontinue", "1"},
                {"explaintext", "1"},
                {"exlimit", "1"}
            },

            new Dictionary<string, string> {
                {"action", "opensearch"},
                {"format", "json"},
                {"limit", "1"}
            }
        };

        public static Result ResultUnavailable =>
            new Result("This page is not available.",
                "This page is only generated for items and tiles." +
                "\n \nIf you searched for an item and are still seeing this page," +
                " the item could not be found with the current method. Sorry.");

        public static string GetExtract(string json) {
            return JsonConvert.DeserializeObject<JObject>(json)?.SelectToken("$..extract")?.ToString();
        }

        public static string GetTitle(string json) {
            return JsonConvert.DeserializeObject<JObject>(json)?.SelectToken("$..title")?.ToString();
        }

        public static string GetTrueItemName(string json) {
            return JsonConvert.DeserializeObject<JArray>(json)?[1].First?.ToString();
        }

        public static string FormatItemName(string item) {
            return item.Replace(' ', '_');
        }

        public static string FormUri(string item, string baseUri, RequestType type) {
            Log(item ?? "item is null in FormUri", LogType.Info);
            Log(baseUri ?? "baseUri is null in FormUri", LogType.Info);
            var sb = new StringBuilder(baseUri);
            sb.Append('?');

            foreach (var param in ParamDicts[type == RequestType.GetItem ? 0 : 1]) {
                sb.AppendFormat("{0}={1}&", param.Key, param.Value);
            }

            sb.AppendFormat(type == RequestType.GetItem ? "titles={0}" : "search={0}", FormatItemName(item));

            return sb.ToString();
        }


        public static async Task<string> GetStringFromHttp(string uri, HttpClient httpClient) {
            Log("Starting the http task", LogType.Info);
            try {
                Log(uri, LogType.Info);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                return await httpClient.GetStringAsync(uri);
            } catch (HttpRequestException e) {
                Log("http error: " + e.StackTrace, LogType.Error);
            } catch (Exception e) {
                Log("unknown error: " + e.Message, LogType.Error);
            }

            return null;
        }


        public static string TileFromId(int id) {
            foreach (var field in typeof(TileID).GetFields()) {
                try {
                    // For some reason it was failing when i was checking types
                    // Also can't use as or is, because ushort is a primitive
                    if ((ushort) field.GetValue(null) == (ushort) id) {
                        return field.Name;
                    }
                } catch (InvalidCastException) {
                    // Ignored
                }
            }

            return "Tile not found";
        }

        public static Item ItemFromName(string name) {
            var type = IdFromItem(name);
            if (type == -1) return null;
            var id = Item.NewItem(new Rectangle(), type);
            return Main.item[id];
        }

        private static int IdFromItem(string name) {
            // This is not guaranteed to work, not all item ID's match their names. But this is the best I can do right now
            // TODO: make this better?
            foreach (var field in typeof(ItemID).GetFields()) {
                if (!string.Equals(field.Name, name.Replace(" ", string.Empty),
                    StringComparison.CurrentCultureIgnoreCase)) {
                    continue;
                }

                try {
                    return (short) field.GetValue(null);
                } catch (InvalidCastException) { }
            }

            return -1;
        }
    }
}
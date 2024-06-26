﻿using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FallGuysStats {
    internal class FallalyticsReporter {
        private static readonly string ReportAPIEndpoint = "https://fallalytics.com/api/report";
        public static readonly string RegisterPbAPIEndpoint = "https://fallalytics.com/api/best-time";
        public static readonly string WeeklyCrownAPIEndpoint = "https://fallalytics.com/api/weekly-crown";
        private static readonly HttpClient HttpClient = new HttpClient();

        public async Task<bool> WeeklyCrown(RoundInfo stat, bool isAnonymous) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, WeeklyCrownAPIEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("FALLALYTICS_KEY"));
            request.Content = new StringContent(this.RoundInfoToWeeklyCrownJsonString(stat, isAnonymous), Encoding.UTF8, "application/json");
            
            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    return true;
                }

                return false;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<bool> RegisterPb(RoundInfo stat, double record, DateTime finish, bool isAnonymous) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RegisterPbAPIEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("FALLALYTICS_KEY"));
            request.Content = new StringContent(this.RoundInfoToRegisterPbJsonString(stat, record, finish, isAnonymous), Encoding.UTF8, "application/json");

            try {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode) {
                    return true;
                }
                return false;
            } catch (HttpRequestException) {
                return false;
            }
        }
        
        public async Task Report(RoundInfo stat, string apiKey) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, ReportAPIEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(this.RoundInfoToReportJsonString(stat), Encoding.UTF8, "application/json");
            try {
                await HttpClient.SendAsync(request);
            } catch (HttpRequestException e) {
                Console.WriteLine($@"Error in FallalyticsReporter. Should not be a problem as it only affects the reporting. Error: {e.Message}");
            }
        }
        
        private string RoundInfoToReportJsonString(RoundInfo stat) {
            var payload = new {
                round = stat.Name,
                index = stat.Round,
                show = stat.ShowNameId,
                isfinal = stat.IsFinal,
                session = stat.SessionId
            };
            return JsonSerializer.Serialize(payload);
        }
        
        private string RoundInfoToRegisterPbJsonString(RoundInfo stat, double record, DateTime finish, bool isAnonymous) {
            var payload = new {
                country = Stats.HostCountryCode,
                onlineServiceType = $"{stat.OnlineServiceType}",
                onlineServiceId = stat.OnlineServiceId,
                onlineServiceNickname = stat.OnlineServiceNickname,
                isAnonymous,
                finish = $"{finish:o}",
                record,
                round = stat.Name,
                show = stat.ShowNameId,
                session = stat.SessionId
            };
            return JsonSerializer.Serialize(payload);
        }
        
        private string RoundInfoToWeeklyCrownJsonString(RoundInfo stat, bool isAnonymous) {
            var payload = new {
                country = Stats.HostCountryCode,
                onlineServiceType = $"{stat.OnlineServiceType}",
                onlineServiceId = stat.OnlineServiceId,
                onlineServiceNickname = stat.OnlineServiceNickname,
                isAnonymous,
                round = stat.Name,
                show = stat.ShowNameId,
                session = stat.SessionId,
                end = $"{stat.End:o}"
            };
            return JsonSerializer.Serialize(payload);
        }
    }
}

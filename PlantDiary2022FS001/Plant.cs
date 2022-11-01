﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using PlantPlacesPlants;
//
//    var plant = Plant.FromJson(jsonString);

namespace PlantPlacesPlants
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Plant
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("genus")]
        public string Genus { get; set; }

        [JsonProperty("species")]
        public string Species { get; set; }

        [JsonProperty("cultivar")]
        public string Cultivar { get; set; }

        [JsonProperty("common")]
        public string Common { get; set; }

        [JsonProperty("wetSoil")]
        public bool WetSoil { get; set; }

        [JsonProperty("drySoil")]
        public bool DrySoil { get; set; }

        [JsonProperty("rainGarden")]
        public bool RainGarden { get; set; }

        [JsonProperty("fullSun")]
        public long FullSun { get; set; }

        [JsonProperty("partShade")]
        public long PartShade { get; set; }

        [JsonProperty("deepShade")]
        public long DeepShade { get; set; }
    }

    public partial class Plant
    {
        public static List<Plant> FromJson(string json) => JsonConvert.DeserializeObject<List<Plant>>(json, PlantPlacesPlants.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Plant> self) => JsonConvert.SerializeObject(self, PlantPlacesPlants.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SevSharks.Identity.WebUI.esiaconnection
{
    /// <summary>
    /// EsiaInfo
    /// </summary>
    public sealed class EsiaInfo
    {
        /// <summary>
        /// Время начала действия.
        /// </summary>
        [JsonProperty("nbf")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? BeginDate { get; set; }

        /// <summary>
        /// Время прекращения действия.
        /// </summary>
        [JsonProperty("exp")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Время выдачи.
        /// </summary>
        [JsonProperty("iat")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Внутренний идентификатор сессии ЕСИА.
        /// </summary>
        [JsonProperty("urn:esia:sid")]
        public string Sid { get; set; }

        /// <summary>
        /// Идентификатор субъекта (oid).
        /// </summary>
        [JsonProperty("urn:esia:sbj_id")]
        public string SbjId { get; set; }
    }
}

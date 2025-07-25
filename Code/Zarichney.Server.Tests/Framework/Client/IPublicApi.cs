// <auto-generated>
//     This code was generated by Refitter.
// </auto-generated>


using Refit;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Zarichney.Client.Contracts;

#nullable enable annotations

namespace Zarichney.Client
{
    [System.CodeDom.Compiler.GeneratedCode("Refitter", "1.5.5.0")]
    public partial interface IPublicApi
    {
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// </list>
        /// </returns>
        [Get("/api/health")]
        Task<IApiResponse> Health(CancellationToken cancellationToken = default);

        /// <summary>Returns the status of services based on their configuration availability.</summary>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// <item>
        /// <term>500</term>
        /// <description>Internal Server Error</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/status")]
        Task<IApiResponse<ICollection<ServiceStatusInfo>>> StatusAll(CancellationToken cancellationToken = default);

        /// <summary>Returns the configuration item status.</summary>
        /// <returns>
        /// A <see cref="Task"/> representing the <see cref="IApiResponse"/> instance containing the result:
        /// <list type="table">
        /// <listheader>
        /// <term>Status</term>
        /// <description>Description</description>
        /// </listheader>
        /// <item>
        /// <term>200</term>
        /// <description>OK</description>
        /// </item>
        /// <item>
        /// <term>500</term>
        /// <description>Internal Server Error</description>
        /// </item>
        /// </list>
        /// </returns>
        [Headers("Accept: text/plain, application/json, text/json")]
        [Get("/api/config")]
        Task<IApiResponse<ICollection<ConfigurationItemStatus>>> Config(CancellationToken cancellationToken = default);
    }

}
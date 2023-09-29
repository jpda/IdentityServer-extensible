// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


#nullable enable


using Duende.IdentityServer.Models;
using System.Threading.Tasks;

namespace Duende.IdentityServer.Stores;

public interface IPushedAuthorizationRequestStore
{
    /// <summary>
    /// Stores the pushed authorization request.
    /// </summary>
    /// <param name="pushedAuthorizationRequest">The request.</param>
    /// <returns></returns>
    /// TODO - When this fails, do we need to send that failure to the caller?
    Task StoreAsync(PushedAuthorizationRequest pushedAuthorizationRequest);

    Task ConsumeAsync(string referenceValue);

    /// <summary>
    /// Gets the pushed authorization request.
    /// </summary>
    /// <param name="referenceValue">The reference value.</param>
    /// <returns></returns>
    Task<PushedAuthorizationRequest?> GetAsync(string referenceValue);
}

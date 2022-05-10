// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace gh_sync;

public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAdo, Ado>();
        services.AddSingleton<IGitHub, GitHub>();
        services.AddSingleton<ISynchronizer, Synchronizer>();
    }
}
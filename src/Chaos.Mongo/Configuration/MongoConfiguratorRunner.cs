// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Configuration;

/// <inheritdoc/>
public class MongoConfiguratorRunner : IMongoConfiguratorRunner
{
    private readonly IEnumerable<IMongoConfigurator> _configurators;
    private readonly IMongoHelper _mongoHelper;

    public MongoConfiguratorRunner(IMongoHelper mongoHelper,
                                   IEnumerable<IMongoConfigurator> configurators)
    {
        ArgumentNullException.ThrowIfNull(mongoHelper);
        ArgumentNullException.ThrowIfNull(configurators);
        _mongoHelper = mongoHelper;
        _configurators = configurators;
    }

    /// <inheritdoc/>
    public async Task RunConfiguratorsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var configurator in _configurators)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await configurator.ConfigureAsync(_mongoHelper, cancellationToken).ConfigureAwait(false);
        }
    }
}

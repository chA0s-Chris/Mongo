// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Migrations;

/// <summary>
/// Executes pending MongoDB migrations in order.
/// </summary>
/// <remarks>
/// This runner is typically invoked automatically during application startup via <see cref="MongoHostedService"/>
/// when <see cref="MongoOptions.ApplyMigrationsOnStartup"/> is enabled.
/// </remarks>
public interface IMongoMigrationRunner
{
    /// <summary>
    /// Executes all pending migrations that have not yet been applied.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to abort migration execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    ///     <para>
    ///     This method will:
    ///     <list type="number">
    ///         <item>
    ///             <description>Attempt to acquire a distributed lock (skips if lock is held by another process)</description>
    ///         </item>
    ///         <item>
    ///             <description>Query the migration history collection to find applied migrations</description>
    ///         </item>
    ///         <item>
    ///             <description>Execute pending migrations in order</description>
    ///         </item>
    ///         <item>
    ///             <description>Record timing and metadata for each successful migration</description>
    ///         </item>
    ///     </list>
    ///     </para>
    ///     <para>
    ///     If a migration fails, execution stops immediately and the exception is propagated.
    ///     Previously applied migrations in the same run will remain applied.
    ///     </para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown when cancelled via <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a migration lock expires during execution.</exception>
    Task RunMigrationsAsync(CancellationToken cancellationToken = default);
}

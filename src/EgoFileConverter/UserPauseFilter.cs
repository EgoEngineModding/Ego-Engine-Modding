using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using ConsoleAppFramework;

using Microsoft.Extensions.Logging;

namespace EgoFileConverter;

internal class UserPauseFilter(ConsoleAppFilter next, ILogger<UserPauseFilter> logger) : ConsoleAppFilter(next)
{
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            await Next.InvokeAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, null);
        }
        finally
        {
            Console.WriteLine();
            logger.LogInformation("Elapsed time: {ElapsedTime}", Stopwatch.GetElapsedTime(startTimestamp));
            logger.LogInformation("Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}

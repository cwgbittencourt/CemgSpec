using CemgSpec.Infrastructure.Execution;
namespace CemgSpec.Windows.Tests;
public sealed class ExecutionContractTests
{
    [Fact]
    public void ChannelRejectsForeignRunTokenAndGenerationAndRevokesOnDispose()
    {
        var run=Guid.NewGuid();using var channel=new ExecutionChannel(run,7);
        Assert.False(channel.Authorize(Guid.NewGuid(),channel.Token,7));
        Assert.False(channel.Authorize(run,new string('0',64),7));
        Assert.False(channel.Authorize(run,channel.Token,8));
        Assert.True(channel.Authorize(run,channel.Token,7));
        channel.Dispose();Assert.False(channel.Authorize(run,channel.Token,7));
    }
}

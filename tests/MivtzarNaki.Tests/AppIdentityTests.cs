using System.Reflection;
using MivtzarNaki.Core;
using MivtzarNaki.Windows;
using Xunit;

namespace MivtzarNaki.Tests;

public sealed class AppIdentityTests
{
    [Fact]
    public void UpdateVersionAndDisplayMatchBuiltProductMetadata()
    {
        var product = typeof(UpdateSession).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0];
        Assert.Equal(product, AppIdentity.DisplayVersion);
        Assert.Equal(Version.Parse(product), UpdateSession.AppVersion);
        Assert.Equal(3, AppIdentity.DisplayVersion.Split('.').Length);
    }
}

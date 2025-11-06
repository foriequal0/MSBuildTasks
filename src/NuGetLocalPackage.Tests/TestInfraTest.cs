using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Shouldly;
using Xunit;

namespace NuGetLocalPackage.Tests;

public sealed class TestInfraTest
{
     [Fact]
     public void ShouldInvokeTargetAndCaptureMessage()
     {
         using var tempdir = new DisposableTempDir();
         var xml = tempdir.CreateFile("test.proj", """
<Project>
  <Target Name="Hello">
    <Message Text="World" />
  </Target>
</Project>
""");
         var project = new Project(xml);

         var logger = new MockLogger();
         using (var bm = new IsolatedBuildManager())
         {
             bm.Build(project, "Hello", logger);
         }

         logger.OfType<BuildMessageEventArgs>()
             .ShouldContain(x => x.SenderName == "Message" && x.Message == "World");
     }

     [Fact]
     public void ShouldCaptureError()
     {
         using var tempdir = new DisposableTempDir();
         var xml = tempdir.CreateFile("test.proj", """
<Project>
  <Target Name="Hello">
    <Error Text="NoHello" />
  </Target>
</Project>
""");
         var project = new Project(xml);

         var logger = new MockLogger();
         using (var bm = new IsolatedBuildManager())
         {
             bm.Build(project, "Hello", logger);
         }

         logger.OfType<BuildErrorEventArgs>()
             .ShouldContain(x => x.SenderName == "Error" && x.Message == "NoHello");
    }
}

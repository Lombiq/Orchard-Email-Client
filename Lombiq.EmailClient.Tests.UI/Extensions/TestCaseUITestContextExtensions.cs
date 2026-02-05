using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System.Threading.Tasks;

namespace Lombiq.EmailClient.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    public static async Task TestImapEmailFetchingAsync(this UITestContext context)
    {
        await context.InitSampleEmailsAsync();
        await context.SignInDirectlyAsync();
        await context.ExecuteEmailClientSampleRecipeDirectlyAsync();
        await context.SetImapPortOnAdminAsync();
        await context.GoToImapTestAsync();

        context.GetAll(By.ClassName("email")).Count.ShouldBe(2);
        context.Exists(By.XPath("//div[contains(text(), 'developer@localhost.com')]"));
        context.Exists(By.XPath("//div[contains(text(), '2025-02-24T12:35:19')]"));
        context.Exists(By.XPath("//div[contains(text(), 'Very important sample')]"));
        context.Exists(By.XPath("//code[contains(text(), 'This email is sent for testing purposes.')]"));
    }
}

using PurpleExplorer.Helpers;
using AzureMessage = Microsoft.Azure.ServiceBus.Message;

namespace PurpleExplorerTest.Helpers;

public abstract class ExtensionsTest
{
    public class CloneMessageTest
    {
        [Fact]
        public void Copies_all_relevant_properties()
        {
            var message = new AzureMessage
            {
                Body = System.Text.Encoding.Unicode.GetBytes("my body"),
                Label = "my label",
                To = "my to",
                SessionId = "my session id",
                ContentType = "my content type",
            };

            //  Act.
            var res = message.CloneMessage();

            //  Assert.
            Assert.Equal(System.Text.Encoding.Unicode.GetBytes("my body"), message.Body);
            Assert.Equal("my label", message.Label);
            Assert.Equal("my to", message.To);
            Assert.Equal("my session id", message.SessionId);
            Assert.Equal("my content type", message.ContentType);
        }
    }
}
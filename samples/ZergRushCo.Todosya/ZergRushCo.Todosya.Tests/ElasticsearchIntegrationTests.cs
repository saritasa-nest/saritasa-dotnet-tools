using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Common.Repositories;

namespace ZergRushCo.Todosya.Tests
{
    [TestFixture]
    public class ElasticsearchIntegrationTests
    {
        private ElasticsearchRepository repository;

        private Guid messageId;

        [OneTimeSetUp]
        public void Init()
        {
            repository = new ElasticsearchRepository("http://localhost:9200");
            messageId = Guid.NewGuid();
        }

        [Test, Order(1) ]
        public async Task Test_add_new_message()
        {
            // delete index with all messages
            // DELETE /saritasa/

            var message = new Message
            {
                ContentType = "ZergRushCo.Todosya.Domain.Tasks.Queries.ProjectsQueries.GetByUser",
                Id = messageId,
                Type = 2,
                Content = new Dictionary<string, int>
                {
                    { "userId", 1 },
                    { "page", 1 },
                    { "pageSize", 10 }
                },
                CreatedAt = DateTime.Now,
                ExecutionDuration = 232720,
                Status = Message.ProcessingStatus.Completed
            };

            //await repository.SaveMessageAsync(message);
        }

        [Test, Order(2)]
        public async Task Test_get_messages()
        {
            // search all massages
            // POST /saritasa/messages/_search/
            // { "query": { "match_all": { } } }

            // Elasticsearch needs about 1 sec to process message.
            await Task.Delay(TimeSpan.FromSeconds(2));

            var messageQuery = MessageQuery.Create()
                .WithId(messageId)
                .WithContentType("ZergRushCo.Todosya.Domain.Tasks.Queries.ProjectsQueries.GetByUser")
                .WithStatus(Message.ProcessingStatus.Completed)
                .WithType(2)
                .WithExecutionDurationAbove(232720)
                .WithExecutionDurationBelow(232720);

            /*var messages = await repository.GetAsync(messageQuery);

            Assert.NotNull(messages);
            Assert.AreEqual(1, messages.Count());
            Assert.AreEqual(messageId, messages.First().Id);*/
        }
    }
}

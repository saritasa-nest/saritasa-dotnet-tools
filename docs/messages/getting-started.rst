Getting Started
===============

In this guide we will apply Saritasa Tools Messages project to `MusicStore <https://github.com/aspnet/MusicStore>`_ application. This is a sample ASP.NET Core project that demonstrates framework functionality.

Project Setup
-------------

Checkout sample project to any directory on your computer.

    ::

        git checkout https://github.com/aspnet/MusicStore.git


Brief setup is  required. You can open ``MusicStore.sln`` and change connection string in ``config.json``.

After that you can run project and if everything works fine you should be able to register. Create new account and try to add music to your card! We will add "messages" functionality for "add to card" feature. Once it works we are ready to start.

Add to Card Command
-------------------

1. Install ``Saritasa.Tools.Messages`` packages:

    .. code-block:: c#

        PM> Install-Package Saritasa.Tools.Messages.Abstractions
        PM> Install-Package Saritasa.Tools.Messages

2. When user adds something to card the ``AddToCart`` action is called of ``ShoppingCartController``. Since we modify something we should use command pipeline. Command data and command handler are separate concepts and we can start with command class first. Add new class to ``MusicStore.Controllers`` namespace:

    .. code-block:: c#

        using System;
        using Saritasa.Tools.Messages.Abstractions.Commands;

        namespace MusicStore.Commands
        {
            public class AddToCartCommand
            {
                public int AlbumId { get; set; }

                public string UserId { get; set; }

                [CommandOut]
                public string CartId { get; set; }
            }
        }

As you see to add album to cart we should know ``UserId`` who does adding, ``AlbumId`` and user's ``CartId``. ``CartId`` will be generated in handler and is marked as out parameter.

3. Then we need handler class. We can copy-paste code that we already have working in controller. Our new class can look like this:

    .. code-block:: c#

        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Http;
        using Microsoft.EntityFrameworkCore;
        using Saritasa.Tools.Messages.Abstractions.Commands;
        using MusicStore.Models;

        namespace MusicStore.Commands
        {
            [CommandHandlers]
            public class CommandHandlers
            {
                public async Task HandleAddToCartCommand(AddToCartCommand command, MusicStoreContext dbContext,
                    IHttpContextAccessor httpContextAccessor)
                {
                    // Retrieve the album from the database.
                    var addedAlbum = await dbContext.Albums
                        .SingleAsync(album => album.AlbumId == command.AlbumId);

                    // Add it to the shopping cart.
                    var cart = ShoppingCart.GetCart(dbContext, httpContextAccessor.HttpContext);
                    await cart.AddToCart(addedAlbum);

                    // Save generated cart id.
                    command.CartId = httpContextAccessor.HttpContext.Session.GetString("Session");

                    await dbContext.SaveChangesAsync();
                }
            }
        }

In our case we inject ``dbContext`` and ``httpContextAccessor`` to our handler.

4. Update our ``ShoppingCartController`` controller code then. To be able to use messages pipelines we should inject ``IMessagePipelineService`` class:

    .. code-block:: c#

        private readonly IMessagePipelineService _pipelineService;

        public ShoppingCartController(MusicStoreContext dbContext, ILogger<ShoppingCartController> logger,
            IMessagePipelineService pipelineService)
        {
            DbContext = dbContext;
            _logger = logger;
            _pipelineService = pipelineService;
        }

And call command within ``AddToCart`` action:

    .. code-block:: c#

        public async Task<IActionResult> AddToCart(int id, CancellationToken requestAborted)
        {
            var ctx = await _pipelineService.HandleCommandAsync(new AddToCartCommand
            {
                AlbumId = id,
                UserId = HttpContext.User.Identity.Name
            }, cancellationToken: requestAborted);

            return RedirectToAction("Index");
        }

5. The final step is to register our messages pipeline. To do that open ``Startup`` class and add following lines into ``ConfigureServices`` method:

    .. code-block:: c#

        // Pipelines registration.
        var pipelinesContainer = new DefaultMessagePipelineContainer();
        pipelinesContainer.AddCommandPipeline()
            .UseDefaultMiddlewares(Assembly.GetExecutingAssembly());
        services.AddSingleton<IMessagePipelineContainer>(pipelinesContainer);
        services.AddScoped<IMessagePipelineService, DefaultMessagePipelineService>();

In code above we register our pipelines as singleton for our application. Pipeline service has scoped life cycle so that we use services resolver in it. Now our code is ready to go and should work fine!

Query Pipeline
--------------

1. Let's go ahead and add query pipeline. And we start with new pipeline registration where we have all our command pipeline registration:

    .. code-block:: c#

        pipelinesContainer.AddQueryPipeline()
            .UseDefaultMiddlewares();

2. Take a look at ``Index`` method in ``ShoppingCartController`` controller. In fact there are two queries: ``cart.GetCartItems()`` and ``cart.GetTotal()``. We can keep it as is and just wrap with query pipeline:

    .. code-block:: c#

        var viewModel = new ShoppingCartViewModel
        {
            CartItems = await _pipelineService.Query(cart).With(q => q.GetCartItems()),
            CartTotal = await _pipelineService.Query(cart).With(q => q.GetTotal())
        };

We pass ``cart`` to query pipeline because we already have it resolved and we tell service that this instance should be used to run methods on. As you can see we do not even need to refactor our queries!

Log Our Messages
----------------

1. And to get all benefits let's make logging of all messages. So that every query/command can be reviewed later. To do that we need to use repository middleware, and for simplicity we are going to store all messages to our separate database table. To do this you need to add following lines in startup class:

    .. code-block:: c#

        var adoNetRepository = new AdoNetMessageRepository(SqlClientFactory.Instance,
            Configuration[StoreConfig.ConnectionStringKey.Replace("__", ":")]);
        var messagesRepository = new RepositoryMiddleware(adoNetRepository);

2. After that add ``messagesRepository`` to every pipeline:

    .. code-block:: c#

        pipelinesContainer.AddCommandPipeline()
            .UseDefaultMiddlewares(Assembly.GetExecutingAssembly())
            .AddMiddleware(messagesRepository);
        pipelinesContainer.AddQueryPipeline()
            .UseDefaultMiddlewares()
            .AddMiddleware(messagesRepository);

Now run the app and add albums to your card. In database you should find ``SaritasaMessages`` table with detailed actions in your application.
